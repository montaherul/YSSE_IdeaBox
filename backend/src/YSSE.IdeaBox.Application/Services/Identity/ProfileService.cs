using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Application.Services.Common;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Services.Identity;

public sealed class ProfileService : IProfileService
{
    private static readonly string[] ProfileFields =
    {
        nameof(CreatorProfile.Headline),
        nameof(CreatorProfile.ShortBio),
        nameof(CreatorProfile.District),
        nameof(CreatorProfile.Languages),
        nameof(CreatorProfile.SkillsOffered),
        nameof(CreatorProfile.HelpNeeded),
        nameof(CreatorProfile.AreasOfInterest),
        nameof(CreatorProfile.Links),
        nameof(CreatorProfile.Availability),
        nameof(CreatorProfile.PhotoUrl)
    };

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;
    private readonly IAuditWriter _audit;
    private readonly INotificationDispatcher _notifications;
    private readonly IFileStorage _files;

    public ProfileService(
        IUnitOfWork db,
        ICurrentUserService current,
        IDateTimeProvider clock,
        IAuditWriter audit,
        INotificationDispatcher notifications,
        IFileStorage files)
    {
        _unitOfWork = db;
        _current = current;
        _clock = clock;
        _audit = audit;
        _notifications = notifications;
        _files = files;
    }

    public async Task<MemberProfileDto?> GetMemberProfileAsync(int userId, CancellationToken ct = default)
    {
        var profile = await _unitOfWork.Repository<MemberProfile>().Query().FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (profile is null)
        {
            return null;
        }

        return MapMember(profile);
    }

    public async Task<Result> UpdateMemberProfileAsync(MemberProfileUpdateRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var profile = await _unitOfWork.Repository<MemberProfile>().Query().FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (profile is null)
        {
            profile = new MemberProfile { UserId = userId, CreatedAt = _clock.UtcNow };
            await _unitOfWork.Repository<MemberProfile>().AddAsync(profile, ct);
        }

        profile.DisplayName = request.DisplayName.Trim();
        profile.Biography = request.Biography;
        profile.Skills = request.Skills;
        profile.Interests = request.Interests;
        profile.PreferredLanguage = string.IsNullOrWhiteSpace(request.PreferredLanguage) ? "en" : request.PreferredLanguage;
        profile.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok();
    }

    public async Task<Result<string>> UploadAvatarAsync(Stream content, string fileName, string contentType, long length, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();

        if (length <= 0)
        {
            return Result<string>.Fail("The selected image is empty.");
        }

        if (length > UploadLimits.MaxAvatarBytes)
        {
            return Result<string>.Fail($"Profile pictures must be smaller than {UploadLimits.MaxAvatarBytes / (1024 * 1024)} MB.");
        }

        // Buffer the (bounded) upload so we can validate the WebP signature before persisting.
        using var buffer = new MemoryStream(length > 0 && length <= int.MaxValue ? (int)length : 0);
        await content.CopyToAsync(buffer, ct);
        var bytes = buffer.ToArray();

        if (!IsWebP(bytes))
        {
            return Result<string>.Fail("Only WebP images are accepted.");
        }

        buffer.Position = 0;
        var storagePath = await _files.SaveAsync(buffer, "avatar.webp", "image/webp", ct);

        var profile = await _unitOfWork.Repository<MemberProfile>().Query().FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (profile is null)
        {
            profile = new MemberProfile { UserId = userId, CreatedAt = _clock.UtcNow };
            await _unitOfWork.Repository<MemberProfile>().AddAsync(profile, ct);
        }

        var previous = MediaPaths.StoragePathFromUrl(profile.ImageUrl);
        profile.ImageUrl = MediaPaths.Url(storagePath);
        profile.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        if (previous is not null)
        {
            await _files.DeleteAsync(previous, ct);
        }

        return Result<string>.Ok(profile.ImageUrl);
    }

    public async Task<Result> RemoveAvatarAsync(CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var profile = await _unitOfWork.Repository<MemberProfile>().Query().FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (profile is null || string.IsNullOrWhiteSpace(profile.ImageUrl))
        {
            return Result.Ok();
        }

        var previous = MediaPaths.StoragePathFromUrl(profile.ImageUrl);
        profile.ImageUrl = null;
        profile.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        if (previous is not null)
        {
            await _files.DeleteAsync(previous, ct);
        }

        return Result.Ok();
    }

    private static bool IsWebP(byte[] bytes) =>
        bytes.Length >= 12
        && bytes[0] == (byte)'R' && bytes[1] == (byte)'I' && bytes[2] == (byte)'F' && bytes[3] == (byte)'F'
        && bytes[8] == (byte)'W' && bytes[9] == (byte)'E' && bytes[10] == (byte)'B' && bytes[11] == (byte)'P';

    public async Task<CreatorProfileDto?> GetMyCreatorProfileAsync(CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var profile = await _unitOfWork.Repository<CreatorProfile>().Query()
            .Include(p => p.FieldVisibilities)
            .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted, ct);
        if (profile is null)
        {
            return null;
        }

        return await MapCreatorAsync(profile, includeHidden: true, ct);
    }

    public async Task<CreatorProfileDto?> GetPublicCreatorProfileAsync(int userId, CancellationToken ct = default)
    {
        var profile = await _unitOfWork.Repository<CreatorProfile>().Query()
            .Include(p => p.FieldVisibilities)
            .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted, ct);
        if (profile is null)
        {
            return null;
        }

        return await MapCreatorAsync(profile, includeHidden: false, ct);
    }

    public async Task<Result<int>> UpsertCreatorProfileAsync(CreatorProfileUpdateRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var alias = request.Alias.Trim();

        var aliasTaken = await _unitOfWork.Repository<CreatorProfile>().Query().AnyAsync(
            p => p.Alias.ToLower() == alias.ToLower() && p.UserId != userId && !p.IsDeleted, ct);
        if (aliasTaken)
        {
            throw new ConflictException("That alias is already in use.");
        }

        var profile = await _unitOfWork.Repository<CreatorProfile>().Query()
            .Include(p => p.FieldVisibilities)
            .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted, ct);

        if (profile is null)
        {
            profile = new CreatorProfile { UserId = userId, CreatedAt = _clock.UtcNow };
            await _unitOfWork.Repository<CreatorProfile>().AddAsync(profile, ct);
        }

        profile.Alias = alias;
        profile.Headline = request.Headline;
        profile.ShortBio = request.ShortBio;
        profile.District = request.District;
        profile.Languages = request.Languages;
        profile.SkillsOffered = request.SkillsOffered;
        profile.HelpNeeded = request.HelpNeeded;
        profile.AreasOfInterest = request.AreasOfInterest;
        profile.Links = request.Links;
        profile.Availability = request.Availability;
        profile.PhotoUrl = request.PhotoUrl;
        profile.UpdatedAt = _clock.UtcNow;

        var visibilities = request.FieldVisibilities ?? new List<ProfileVisibilityDto>();
        var provided = visibilities.ToDictionary(v => v.FieldName, v => v.Visibility);

        _unitOfWork.Repository<ProfileFieldVisibility>().RemoveRange(profile.FieldVisibilities);
        profile.FieldVisibilities.Clear();

        foreach (var field in ProfileFields)
        {
            var visibility = provided.TryGetValue(field, out var v) ? v : ProfileVisibility.Private;
            await _unitOfWork.Repository<ProfileFieldVisibility>().AddAsync(new ProfileFieldVisibility
            {
                CreatorProfileId = profile.Id,
                FieldName = field,
                Visibility = visibility,
                CreatedAt = _clock.UtcNow
            }, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result<int>.Ok(profile.Id);
    }

    public async Task<Result> DeleteCreatorProfileAsync(CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var profile = await _unitOfWork.Repository<CreatorProfile>().Query().FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted, ct);
        if (profile is null)
        {
            return Result.Fail("You do not have a creator profile.");
        }

        profile.IsDeleted = true;
        profile.DeletedAt = _clock.UtcNow;
        profile.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.RecordAsync("CreatorProfileDeleted", nameof(CreatorProfile), profile.Id.ToString(), null, null, ct);
        return Result.Ok();
    }

    public async Task<EntrepreneurProfileDto?> GetMyEntrepreneurProfileAsync(CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var profile = await _unitOfWork.Repository<EntrepreneurProfile>().Query().FirstOrDefaultAsync(p => p.UserId == userId, ct);
        return profile is null ? null : MapEntrepreneur(profile);
    }

    public async Task<Result<int>> RequestOpportunitySeekerAsync(EntrepreneurProfileUpdateRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var profile = await _unitOfWork.Repository<EntrepreneurProfile>().Query().FirstOrDefaultAsync(p => p.UserId == userId, ct);
        if (profile is null)
        {
            profile = new EntrepreneurProfile { UserId = userId, CreatedAt = _clock.UtcNow };
            await _unitOfWork.Repository<EntrepreneurProfile>().AddAsync(profile, ct);
        }

        profile.Kind = request.Kind;
        profile.Organization = request.Organization.Trim();
        profile.Industry = request.Industry.Trim();
        profile.Description = request.Description;
        profile.Website = request.Website;
        profile.CoarseLocation = request.CoarseLocation;
        profile.InterestAreas = request.InterestAreas;
        profile.Expertise = request.Expertise;
        profile.PreferredCollaborationTypes = request.PreferredCollaborationTypes;
        profile.IsApproved = false;
        profile.IsVerified = false;
        profile.UpdatedAt = _clock.UtcNow;

        await _unitOfWork.SaveChangesAsync(ct);
        return Result<int>.Ok(profile.Id);
    }

    public async Task<PagedResult<EntrepreneurProfileDto>> GetPendingSeekersAsync(int page, int pageSize, CancellationToken ct = default)
    {
        if (!_current.IsAdmin)
        {
            throw new ForbiddenException();
        }

        var (p, s) = Pagination.Normalize(page, pageSize);
        var query = _unitOfWork.Repository<EntrepreneurProfile>().Query().Where(e => !e.IsApproved);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(e => e.CreatedAt)
            .Skip((p - 1) * s)
            .Take(s)
            .Select(e => new EntrepreneurProfileDto
            {
                Id = e.Id,
                Kind = e.Kind,
                Organization = e.Organization,
                Industry = e.Industry,
                Description = e.Description,
                Website = e.Website,
                CoarseLocation = e.CoarseLocation,
                InterestAreas = e.InterestAreas,
                Expertise = e.Expertise,
                PreferredCollaborationTypes = e.PreferredCollaborationTypes,
                IsVerified = e.IsVerified,
                IsApproved = e.IsApproved
            })
            .ToListAsync(ct);

        return PagedResult<EntrepreneurProfileDto>.Create(items, p, s, total);
    }

    public async Task<Result> ApproveSeekerAsync(int userId, bool approve, CancellationToken ct = default)
    {
        if (!_current.IsAdmin)
        {
            throw new ForbiddenException();
        }

        var profile = await _unitOfWork.Repository<EntrepreneurProfile>().Query().FirstOrDefaultAsync(e => e.UserId == userId, ct);
        if (profile is null)
        {
            throw new NotFoundException("No entrepreneur profile exists for that user.");
        }

        profile.IsApproved = approve;
        profile.UpdatedAt = _clock.UtcNow;

        if (approve)
        {
            profile.IsVerified = true;
            profile.ApprovedAt = _clock.UtcNow;

            var roleName = profile.Kind switch
            {
                OpportunitySeekerKind.Investor => SystemRoles.Investor,
                OpportunitySeekerKind.Mentor => SystemRoles.Mentor,
                _ => SystemRoles.Entrepreneur
            };

            var alreadyAssigned = await _unitOfWork.Repository<UserRole>().Query().AnyAsync(
                ur => ur.UserId == userId && ur.Role.Name == roleName, ct);
            if (!alreadyAssigned)
            {
                var role = await _unitOfWork.Repository<Role>().Query().FirstOrDefaultAsync(r => r.Name == roleName, ct);
                if (role is not null)
                {
                    await _unitOfWork.Repository<UserRole>().AddAsync(new UserRole
                    {
                        UserId = userId,
                        RoleId = role.Id,
                        GrantedByUserId = _current.UserId,
                        GrantedAt = _clock.UtcNow
                    }, ct);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync(
            approve ? "OpportunitySeekerApproved" : "OpportunitySeekerRejected",
            nameof(EntrepreneurProfile),
            profile.Id.ToString(),
            null,
            new { profile.UserId, profile.Kind },
            ct);

        await _notifications.NotifyAsync(
            userId,
            NotificationType.AccountNotice,
            approve ? "Seeker application approved" : "Seeker application not approved",
            approve
                ? "Your opportunity seeker profile was approved. You can now send inquiries."
                : "Your opportunity seeker profile was not approved. Please review and resubmit.",
            null,
            essential: true,
            ct);

        return Result.Ok();
    }

    private async Task<CreatorProfileDto> MapCreatorAsync(CreatorProfile profile, bool includeHidden, CancellationToken ct)
    {
        var userId = profile.UserId;
        var publishedCount = includeHidden
            ? await _unitOfWork.Repository<Idea>().Query().CountAsync(i => i.CreatorUserId == userId && !i.IsDeleted && i.Attribution == IdeaAttribution.Public && i.Status == IdeaStatus.Published, ct)
            : await _unitOfWork.Repository<Idea>().Query().CountAsync(i => i.CreatorUserId == userId && !i.IsDeleted && i.Attribution == IdeaAttribution.Public && i.Status == IdeaStatus.Published, ct);

        var visibilityByField = profile.FieldVisibilities.ToDictionary(v => v.FieldName, v => v.Visibility);

        if (!includeHidden)
        {
            var hidden = new HashSet<string>(ProfileFields.Where(f => !visibilityByField.TryGetValue(f, out var v) || v != ProfileVisibility.Public));
            return new CreatorProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                Alias = profile.Alias,
                Headline = hidden.Contains(nameof(profile.Headline)) ? null : profile.Headline,
                ShortBio = hidden.Contains(nameof(profile.ShortBio)) ? null : profile.ShortBio,
                District = hidden.Contains(nameof(profile.District)) ? null : profile.District,
                Languages = hidden.Contains(nameof(profile.Languages)) ? null : profile.Languages,
                SkillsOffered = hidden.Contains(nameof(profile.SkillsOffered)) ? null : profile.SkillsOffered,
                HelpNeeded = hidden.Contains(nameof(profile.HelpNeeded)) ? null : profile.HelpNeeded,
                AreasOfInterest = hidden.Contains(nameof(profile.AreasOfInterest)) ? null : profile.AreasOfInterest,
                Links = hidden.Contains(nameof(profile.Links)) ? null : profile.Links,
                Availability = hidden.Contains(nameof(profile.Availability)) ? null : profile.Availability,
                PhotoUrl = hidden.Contains(nameof(profile.PhotoUrl)) ? null : profile.PhotoUrl,
                PublishedIdeaCount = publishedCount,
                FieldVisibilities = profile.FieldVisibilities.Select(v => new ProfileVisibilityDto(v.FieldName, v.Visibility)).ToList()
            };
        }

        return new CreatorProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            Alias = profile.Alias,
            Headline = profile.Headline,
            ShortBio = profile.ShortBio,
            District = profile.District,
            Languages = profile.Languages,
            SkillsOffered = profile.SkillsOffered,
            HelpNeeded = profile.HelpNeeded,
            AreasOfInterest = profile.AreasOfInterest,
            Links = profile.Links,
            Availability = profile.Availability,
            PhotoUrl = profile.PhotoUrl,
            PublishedIdeaCount = publishedCount,
            FieldVisibilities = profile.FieldVisibilities.Select(v => new ProfileVisibilityDto(v.FieldName, v.Visibility)).ToList()
        };
    }

    private static MemberProfileDto MapMember(MemberProfile profile) => new()
    {
        DisplayName = profile.DisplayName,
        Biography = profile.Biography,
        Skills = profile.Skills,
        Interests = profile.Interests,
        ImageUrl = profile.ImageUrl,
        PreferredLanguage = profile.PreferredLanguage
    };

    private static EntrepreneurProfileDto MapEntrepreneur(EntrepreneurProfile profile) => new()
    {
        Id = profile.Id,
        Kind = profile.Kind,
        Organization = profile.Organization,
        Industry = profile.Industry,
        Description = profile.Description,
        Website = profile.Website,
        CoarseLocation = profile.CoarseLocation,
        InterestAreas = profile.InterestAreas,
        Expertise = profile.Expertise,
        PreferredCollaborationTypes = profile.PreferredCollaborationTypes,
        IsVerified = profile.IsVerified,
        IsApproved = profile.IsApproved
    };
}


