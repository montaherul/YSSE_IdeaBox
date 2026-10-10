using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;
using YSSE.IdeaBox.Api.Common;
using YSSE.IdeaBox.Application.Abstractions.Services;
using YSSE.IdeaBox.Application.Common;
using YSSE.IdeaBox.Application.Dtos;

namespace YSSE.IdeaBox.Api.Controllers;

[Route("api/v1/auth")]
public sealed class AuthController : ApiControllerBase
{
    private readonly IAuthService _auth;
    private readonly IWebHostEnvironment _environment;

    public AuthController(IAuthService auth, IWebHostEnvironment environment)
    {
        _auth = auth;
        _environment = environment;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var result = await _auth.RegisterAsync(request, ct);

        // The OTP is surfaced only in Development (no email/SMS gateway in the demo).
        if (result.Success && result.Data is not null && !_environment.IsDevelopment())
        {
            result = Result<AuthResponse>.Ok(result.Data with { VerificationCode = null });
        }

        return OkResult(result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
        => OkResult(await _auth.LoginAsync(request, ct));

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
        => OkResult(await _auth.RefreshAsync(request, ct));

    [HttpPost("logout")]
    [AllowAnonymous]
    public async Task<IActionResult> Logout(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] RefreshRequest? request,
        CancellationToken ct)
        => OkResult(await _auth.LogoutAsync(request ?? new RefreshRequest(), ct));

    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var user = await _auth.GetCurrentUserAsync(ct);
        return user is null ? Unauthorized(ApiResults.Error("Authentication is required.", "UNAUTHORIZED", TraceId)) : OkData(user);
    }

    [HttpPost("verify/request")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> RequestVerification(CancellationToken ct)
        => OkResult(await _auth.RequestVerificationAsync(ct));

    [HttpPost("verify")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Verify([FromBody] VerifyRequest request, CancellationToken ct)
        => OkResult(await _auth.VerifyAsync(request, ct));

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
    {
        var result = await _auth.ForgotPasswordAsync(request, ct);

        // The reset code is surfaced only in Development (no email gateway in the demo).
        if (result.Success && result.Data is not null && !_environment.IsDevelopment())
        {
            result = Result<ForgotPasswordResponse>.Ok(result.Data with { ResetCode = null });
        }

        return OkResult(result);
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
        => OkResult(await _auth.ResetPasswordAsync(request, ct));
}
