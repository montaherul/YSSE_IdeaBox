using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Application.Services.Common;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Services.Identity;

public sealed class AuthService : IAuthService
{
    private static readonly TimeSpan VerificationLifetime = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan PasswordResetLifetime = TimeSpan.FromMinutes(15);
    private const string PasswordResetPurpose = "password-reset";

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _current;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IDateTimeProvider _clock;
    private readonly IVerificationCodeStore _verificationCodes;
    private readonly IHashingService _hashing;
    private readonly IAuditWriter _audit;
    private readonly IEmailSender _email;
    private readonly ISmsSender _sms;
    private readonly IUserPermissionService _permissions;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork db,
        ICurrentUserService current,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IDateTimeProvider clock,
        IVerificationCodeStore verificationCodes,
        IHashingService hashing,
        IAuditWriter audit,
        IEmailSender email,
        ISmsSender sms,
        IUserPermissionService permissions,
        ILogger<AuthService> logger)
    {
        _unitOfWork = db;
        _current = current;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _clock = clock;
        _verificationCodes = verificationCodes;
        _hashing = hashing;
        _audit = audit;
        _email = email;
        _sms = sms;
        _permissions = permissions;
        _logger = logger;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        var phone = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();

        if (email is null && phone is null)
        {
            return Result<AuthResponse>.Fail("An email address or phone number is required.");
        }

        if (!request.AcceptTerms)
        {
            return Result<AuthResponse>.Fail("You must accept the terms to register.");
        }

        if (request.Password.Length < 8)
        {
            return Result<AuthResponse>.Fail("Password must be at least 8 characters.");
        }

        if (email is not null &&
            await _unitOfWork.Repository<User>().Query().AnyAsync(u => !u.IsDeleted && u.Email != null && u.Email.ToLower() == email.ToLower(), ct))
        {
            throw new ConflictException("An account with this email already exists.");
        }

        if (phone is not null &&
            await _unitOfWork.Repository<User>().Query().AnyAsync(u => !u.IsDeleted && u.PhoneNumber == phone, ct))
        {
            throw new ConflictException("An account with this phone number already exists.");
        }

        var user = new User
        {
            Email = email,
            PhoneNumber = phone,
            PasswordHash = _passwordHasher.Hash(request.Password),
            DisplayName = request.DisplayName.Trim(),
            CreatedAt = _clock.UtcNow
        };
        await _unitOfWork.Repository<User>().AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        user.MemberProfile = new MemberProfile
        {
            UserId = user.Id,
            DisplayName = user.DisplayName,
            PreferredLanguage = "en",
            CreatedAt = _clock.UtcNow
        };
        await _unitOfWork.Repository<MemberProfile>().AddAsync(user.MemberProfile, ct);

        var memberRole = await _unitOfWork.Repository<Role>().Query().FirstOrDefaultAsync(r => r.Name == SystemRoles.Member, ct);
        if (memberRole is not null)
        {
            await _unitOfWork.Repository<UserRole>().AddAsync(new UserRole
            {
                UserId = user.Id,
                RoleId = memberRole.Id,
                GrantedAt = _clock.UtcNow
            }, ct);
        }

        await _unitOfWork.SaveChangesAsync(ct);

        var code = GenerateCode();
        var purpose = email is not null ? "email" : "phone";
        _verificationCodes.Store(user.Id, purpose, code, VerificationLifetime);
        await DispatchVerificationAsync(user, purpose, code, ct);

        var response = await IssueTokensAsync(user, ct);
        return Result<AuthResponse>.Ok(response with { VerificationCode = code });
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var identifier = request.EmailOrPhone.Trim();
        var user = await _unitOfWork.Repository<User>().Query().FirstOrDefaultAsync(
            u => !u.IsDeleted &&
                 ((u.Email != null && u.Email.ToLower() == identifier.ToLower()) || u.PhoneNumber == identifier),
            ct);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return Result<AuthResponse>.Fail("Invalid credentials.");
        }

        if (user.IsSuspended)
        {
            throw new ForbiddenException("Your account is suspended. " + (user.SuspensionReason ?? string.Empty));
        }

        user.LastLoginAt = _clock.UtcNow;
        user.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);

        var response = await IssueTokensAsync(user, ct);
        return Result<AuthResponse>.Ok(response);
    }

    public async Task<Result<AuthResponse>> RefreshAsync(RefreshRequest request, CancellationToken ct = default)
    {
        var userId = _tokenService.ValidateRefreshToken(request.RefreshToken);
        if (userId is null)
        {
            return Result<AuthResponse>.Fail("Invalid refresh token.");
        }

        var hash = _hashing.Sha256(request.RefreshToken);
        var stored = await _unitOfWork.Repository<RefreshToken>().Query().FirstOrDefaultAsync(t => t.TokenHash == hash && t.UserId == userId, ct);
        if (stored is null || stored.IsRevoked || stored.ExpiresAt <= _clock.UtcNow)
        {
            return Result<AuthResponse>.Fail("Refresh token is no longer valid.");
        }

        var user = await _unitOfWork.Repository<User>().Query().FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, ct);
        if (user is null)
        {
            return Result<AuthResponse>.Fail("Account not found.");
        }

        if (user.IsSuspended)
        {
            throw new ForbiddenException("Your account is suspended.");
        }

        stored.IsRevoked = true;
        stored.RevokedAt = _clock.UtcNow;
        stored.UpdatedAt = _clock.UtcNow;

        var response = await IssueTokensAsync(user, ct);
        stored.ReplacedByTokenHash = _hashing.Sha256(response.RefreshToken);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result<AuthResponse>.Ok(response);
    }

    public async Task<Result> LogoutAsync(RefreshRequest request, CancellationToken ct = default)
    {
        var hash = _hashing.Sha256(request.RefreshToken);
        var stored = await _unitOfWork.Repository<RefreshToken>().Query().FirstOrDefaultAsync(t => t.TokenHash == hash && !t.IsRevoked, ct);
        if (stored is not null)
        {
            stored.IsRevoked = true;
            stored.RevokedAt = _clock.UtcNow;
            stored.UpdatedAt = _clock.UtcNow;
            await _unitOfWork.SaveChangesAsync(ct);
        }

        return Result.Ok();
    }

    public async Task<UserDto?> GetCurrentUserAsync(CancellationToken ct = default)
    {
        if (!_current.IsAuthenticated || _current.UserId is null)
        {
            return null;
        }

        var user = await _unitOfWork.Repository<User>().Query().FirstOrDefaultAsync(u => u.Id == _current.UserId && !u.IsDeleted, ct);
        if (user is null)
        {
            return null;
        }

        return await BuildUserDtoAsync(user, ct);
    }

    public async Task<Result> RequestVerificationAsync(CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var user = await _unitOfWork.Repository<User>().Query().FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, ct);
        if (user is null)
        {
            throw new NotFoundException("Account not found.");
        }

        var purpose = user.Email is not null ? "email" : "phone";
        var code = GenerateCode();
        _verificationCodes.Store(userId, purpose, code, VerificationLifetime);
        await DispatchVerificationAsync(user, purpose, code, ct);
        return Result.Ok($"A verification code was sent via {purpose}.");
    }

    public async Task<Result> VerifyAsync(VerifyRequest request, CancellationToken ct = default)
    {
        var userId = _current.RequireUserId();
        var user = await _unitOfWork.Repository<User>().Query().FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, ct);
        if (user is null)
        {
            throw new NotFoundException("Account not found.");
        }

        var verified = false;
        if (user.Email is not null && _verificationCodes.TryConsume(userId, "email", request.Code))
        {
            user.IsEmailVerified = true;
            verified = true;
        }
        else if (user.PhoneNumber is not null && _verificationCodes.TryConsume(userId, "phone", request.Code))
        {
            user.IsPhoneVerified = true;
            verified = true;
        }

        if (!verified)
        {
            return Result.Fail("The verification code is invalid or has expired.");
        }

        user.UpdatedAt = _clock.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok("Account verified.");
    }

    public async Task<Result<ForgotPasswordResponse>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim();

        var user = await _unitOfWork.Repository<User>().Query()
            .FirstOrDefaultAsync(u => !u.IsDeleted && u.Email != null && u.Email.ToLower() == email.ToLower(), ct);

        // Do not reveal whether an account exists (avoids user enumeration).
        if (user is null)
        {
            return Result<ForgotPasswordResponse>.Ok(new ForgotPasswordResponse());
        }

        var code = GenerateCode();
        _verificationCodes.Store(user.Id, PasswordResetPurpose, code, PasswordResetLifetime);
        await DispatchPasswordResetAsync(user, code, ct);
        return Result<ForgotPasswordResponse>.Ok(new ForgotPasswordResponse { ResetCode = code });
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        if (request.NewPassword.Length < 8)
        {
            return Result.Fail("Password must be at least 8 characters.");
        }

        var email = request.Email.Trim();
        var user = await _unitOfWork.Repository<User>().Query()
            .FirstOrDefaultAsync(u => !u.IsDeleted && u.Email != null && u.Email.ToLower() == email.ToLower(), ct);

        if (user is null || !_verificationCodes.TryConsume(user.Id, PasswordResetPurpose, request.Code))
        {
            return Result.Fail("The reset code is invalid or has expired.");
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.UpdatedAt = _clock.UtcNow;

        // Invalidate every existing session after a password change.
        var tokens = await _unitOfWork.Repository<RefreshToken>().Query()
            .Where(t => t.UserId == user.Id && !t.IsRevoked)
            .ToListAsync(ct);
        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = _clock.UtcNow;
            token.UpdatedAt = _clock.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Ok("Your password has been reset. Please sign in with your new password.");
    }

    private async Task<AuthResponse> IssueTokensAsync(User user, CancellationToken ct)
    {
        var roles = await GetRoleNamesAsync(user.Id, ct);
        var tokens = _tokenService.CreateTokens(new UserTokenInfo(user.Id, user.DisplayName, user.Email, roles));

        await _unitOfWork.Repository<RefreshToken>().AddAsync(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = _hashing.Sha256(tokens.RefreshToken),
            ExpiresAt = tokens.RefreshExpiresAt,
            CreatedAt = _clock.UtcNow
        }, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        var dto = await BuildUserDtoAsync(user, ct);
        return new AuthResponse
        {
            AccessToken = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            ExpiresAt = tokens.ExpiresAt,
            User = dto
        };
    }

    private async Task<UserDto> BuildUserDtoAsync(User user, CancellationToken ct)
    {
        var roles = await GetRoleNamesAsync(user.Id, ct);
        var hasCreator = await _unitOfWork.Repository<CreatorProfile>().Query().AnyAsync(p => p.UserId == user.Id && !p.IsDeleted, ct);
        var hasEntrepreneur = await _unitOfWork.Repository<EntrepreneurProfile>().Query().AnyAsync(p => p.UserId == user.Id, ct);
        var permissionCodes = await _permissions.GetPermissionsAsync(user.Id, ct);

        var memberProfile = await _unitOfWork.Repository<MemberProfile>().Query().FirstOrDefaultAsync(p => p.UserId == user.Id, ct);

        return new UserDto
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            ImageUrl = memberProfile?.ImageUrl,
            IsEmailVerified = user.IsEmailVerified,
            IsPhoneVerified = user.IsPhoneVerified,
            HasCreatorProfile = hasCreator,
            HasEntrepreneurProfile = hasEntrepreneur,
            Roles = roles.ToList(),
            Permissions = permissionCodes.OrderBy(c => c, StringComparer.OrdinalIgnoreCase).ToList()
        };
    }

    private async Task<List<string>> GetRoleNamesAsync(int userId, CancellationToken ct) =>
        await _unitOfWork.Repository<UserRole>().Query()
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role.Name)
            .Distinct()
            .ToListAsync(ct);

    private static string GenerateCode() => Random.Shared.Next(100000, 999999).ToString();

    private async Task DispatchVerificationAsync(User user, string purpose, string code, CancellationToken ct)
    {
        try
        {
            if (purpose == "email" && !string.IsNullOrWhiteSpace(user.Email))
            {
                await _email.SendAsync(new EmailMessage(
                    user.Email!,
                    "Your YSSE Idea Box verification code",
                    $"<p>Your verification code is <strong>{code}</strong>. It expires in 15 minutes.</p>",
                    $"Your verification code is {code}. It expires in 15 minutes."), ct);
            }
            else if (purpose == "phone" && !string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                await _sms.SendAsync(new SmsMessage(
                    user.PhoneNumber!,
                    $"Your YSSE Idea Box verification code is {code}."), ct);
            }
        }
        catch (Exception ex)
        {
            // Delivery must never fail the account operation; the user can request a new code.
            _logger.LogWarning(ex, "Failed to deliver {Purpose} verification code for user {UserId}.", purpose, user.Id);
        }
    }

    private async Task DispatchPasswordResetAsync(User user, string code, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(user.Email))
        {
            return;
        }

        try
        {
            await _email.SendAsync(new EmailMessage(
                user.Email!,
                "Reset your YSSE Idea Box password",
                $"<p>Your password reset code is <strong>{code}</strong>. It expires in 15 minutes.</p>",
                $"Your password reset code is {code}. It expires in 15 minutes."), ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to deliver password reset code for user {UserId}.", user.Id);
        }
    }
}



