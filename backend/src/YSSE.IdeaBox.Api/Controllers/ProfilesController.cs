using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using YSSE.IdeaBox.Api.Common;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Api.Controllers;

[Route("api/v1/profiles")]
[Authorize]
public sealed class ProfilesController : ApiControllerBase
{
    private readonly IProfileService _profiles;
    private readonly ICurrentUserService _current;

    public ProfilesController(IProfileService profiles, ICurrentUserService current)
    {
        _profiles = profiles;
        _current = current;
    }

    [HttpGet("me")]
    [Authorize(Policy = Permissions.ProfilesRead)]
    public async Task<IActionResult> GetMember(CancellationToken ct)
    {
        if (_current.UserId is not int userId)
        {
            return Unauthorized(ApiResults.Error("Authentication is required.", "UNAUTHORIZED", TraceId));
        }

        var profile = await _profiles.GetMemberProfileAsync(userId, ct);
        return profile is null ? NotFoundError("Profile not found.") : OkData(profile);
    }

    [HttpPut("me")]
    [Authorize(Policy = Permissions.ProfilesUpdate)]
    public async Task<IActionResult> UpdateMember([FromBody] MemberProfileUpdateRequest request, CancellationToken ct)
        => OkResult(await _profiles.UpdateMemberProfileAsync(request, ct));

    [HttpPost("me/avatar")]
    [Authorize(Policy = Permissions.ProfilesUpdate)]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> UploadAvatar([FromForm] IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            return Failed("Select an image to upload.");
        }

        await using var stream = file.OpenReadStream();
        return OkResult(await _profiles.UploadAvatarAsync(stream, file.FileName, file.ContentType, file.Length, ct));
    }

    [HttpDelete("me/avatar")]
    [Authorize(Policy = Permissions.ProfilesUpdate)]
    public async Task<IActionResult> RemoveAvatar(CancellationToken ct)
        => OkResult(await _profiles.RemoveAvatarAsync(ct));

    [HttpGet("creator/me")]
    [Authorize(Policy = Permissions.ProfilesRead)]
    public async Task<IActionResult> GetMyCreator(CancellationToken ct)
    {
        var profile = await _profiles.GetMyCreatorProfileAsync(ct);
        return profile is null ? NotFoundError("Creator profile not found.") : OkData(profile);
    }

    [HttpPut("creator/me")]
    [Authorize(Policy = Permissions.ProfilesUpdate)]
    public async Task<IActionResult> UpsertCreator([FromBody] CreatorProfileUpdateRequest request, CancellationToken ct)
        => OkResult(await _profiles.UpsertCreatorProfileAsync(request, ct));

    [HttpDelete("creator/me")]
    [Authorize(Policy = Permissions.ProfilesUpdate)]
    public async Task<IActionResult> DeleteCreator(CancellationToken ct)
        => OkResult(await _profiles.DeleteCreatorProfileAsync(ct));

    [HttpGet("creator/{userId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCreator(int userId, CancellationToken ct)
    {
        var profile = await _profiles.GetPublicCreatorProfileAsync(userId, ct);
        return profile is null ? NotFoundError("Creator profile not found.") : OkData(profile);
    }

    [HttpGet("entrepreneur/me")]
    [Authorize(Policy = Permissions.ProfilesRead)]
    public async Task<IActionResult> GetMyEntrepreneur(CancellationToken ct)
    {
        var profile = await _profiles.GetMyEntrepreneurProfileAsync(ct);
        return profile is null ? NotFoundError("Opportunity seeker profile not found.") : OkData(profile);
    }

    [HttpPut("entrepreneur/me")]
    [Authorize(Policy = Permissions.ProfilesUpdate)]
    public async Task<IActionResult> RequestOpportunitySeeker([FromBody] EntrepreneurProfileUpdateRequest request, CancellationToken ct)
        => OkResult(await _profiles.RequestOpportunitySeekerAsync(request, ct));

    [HttpGet("entrepreneur/pending")]
    [Authorize(Policy = Permissions.ProfilesRead)]
    public async Task<IActionResult> GetPending([FromQuery] int page = 1, [FromQuery] int pageSize = Pagination.DefaultPageSize, CancellationToken ct = default)
        => Paged(await _profiles.GetPendingSeekersAsync(page, pageSize, ct));

    [HttpPost("entrepreneur/{userId:int}/approve")]
    [Authorize(Policy = Permissions.ProfilesUpdate)]
    public async Task<IActionResult> Approve(int userId, [FromQuery] bool approve = true, CancellationToken ct = default)
        => OkResult(await _profiles.ApproveSeekerAsync(userId, approve, ct));
}