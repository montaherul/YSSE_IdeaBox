using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;
using YSSE.IdeaBox.Domain.Entities;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Services.Identity;

public sealed class UserRoleService : IUserRoleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _current;
    private readonly IDateTimeProvider _clock;
    private readonly IAuditWriter _audit;

    public UserRoleService(
        IUnitOfWork unitOfWork,
        ICurrentUserService current,
        IDateTimeProvider clock,
        IAuditWriter audit)
    {
        _unitOfWork = unitOfWork;
        _current = current;
        _clock = clock;
        _audit = audit;
    }

    public async Task<Result> AssignRoleAsync(int userId, AssignRoleRequest request, CancellationToken ct = default)
    {
        var roleName = request.Role?.Trim() ?? string.Empty;
        if (roleName.Length == 0)
        {
            return Result.Fail("Unknown role.");
        }

        var roleEntity = await _unitOfWork.Repository<Role>().Query()
            .FirstOrDefaultAsync(r => r.Name.ToLower() == roleName.ToLower(), ct);
        if (roleEntity is null)
        {
            return Result.Fail("Role is not registered on the platform.");
        }

        var user = await _unitOfWork.Repository<User>().Query().FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted, ct);
        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        var exists = await _unitOfWork.Repository<UserRole>().Query().AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleEntity.Id, ct);
        if (exists)
        {
            return Result.Fail("The user already holds this role.");
        }

        await _unitOfWork.Repository<UserRole>().AddAsync(new UserRole
        {
            UserId = userId,
            RoleId = roleEntity.Id,
            GrantedByUserId = _current.UserId,
            GrantedAt = _clock.UtcNow
        }, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("RoleAssigned", nameof(UserRole), $"user:{userId}",
            request.Justification, new { role = roleEntity.Name }, ct);
        return Result.Ok();
    }

    public async Task<Result> RevokeRoleAsync(int userId, string role, CancellationToken ct = default)
    {
        var roleName = role?.Trim() ?? string.Empty;
        if (roleName.Length == 0)
        {
            return Result.Fail("Unknown role.");
        }

        if (string.Equals(roleName, SystemRoles.Member, StringComparison.OrdinalIgnoreCase))
        {
            return Result.Fail("The base Member role cannot be revoked.");
        }

        if (string.Equals(roleName, SystemRoles.Administrator, StringComparison.OrdinalIgnoreCase) && userId == _current.UserId)
        {
            return Result.Fail("You cannot revoke your own Administrator role.");
        }

        var userRole = await _unitOfWork.Repository<UserRole>().Query()
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Role.Name.ToLower() == roleName.ToLower(), ct);
        if (userRole is null)
        {
            return Result.Fail("The user does not hold this role.");
        }

        if (string.Equals(roleName, SystemRoles.Administrator, StringComparison.OrdinalIgnoreCase))
        {
            var otherAdmins = await _unitOfWork.Repository<UserRole>().Query().AnyAsync(
                x => x.UserId != userId && x.Role.Name == SystemRoles.Administrator &&
                     !x.User.IsDeleted && !x.User.IsSuspended, ct);
            if (!otherAdmins)
            {
                return Result.Fail("Cannot revoke the last active Administrator role.");
            }
        }

        _unitOfWork.Repository<UserRole>().Remove(userRole);
        await _unitOfWork.SaveChangesAsync(ct);

        await _audit.RecordAsync("RoleRevoked", nameof(UserRole), $"user:{userId}",
            null, new { role = userRole.Role.Name }, ct);
        return Result.Ok();
    }
}
