using System.ComponentModel.DataAnnotations;
using YSSE.IdeaBox.Domain.Enums;

namespace YSSE.IdeaBox.Application.Dtos;

public record RegisterRequest
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; init; } = string.Empty;

    [MaxLength(32)]
    public string? PhoneNumber { get; init; }

    [Required, MinLength(8), MaxLength(128)]
    public string Password { get; init; } = string.Empty;

    [Required, MaxLength(120)]
    public string DisplayName { get; init; } = string.Empty;

    public bool AcceptTerms { get; init; }
}

public record LoginRequest
{
    [Required]
    public string EmailOrPhone { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

public record RefreshRequest
{
    [Required]
    public string RefreshToken { get; init; } = string.Empty;
}

public record AuthResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public UserDto User { get; init; } = new();

    /// <summary>
    /// Development-only convenience: the OTP just issued to the account so the demo can
    /// complete verification without an email/SMS gateway. The API layer strips this in
    /// non-Development environments before returning the response.
    /// </summary>
    public string? VerificationCode { get; init; }
}

public record UserDto
{
    public int Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public bool IsEmailVerified { get; init; }
    public bool IsPhoneVerified { get; init; }
    public bool IsMinor { get; init; }
    public bool HasCreatorProfile { get; init; }
    public bool HasEntrepreneurProfile { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = Array.Empty<string>();
}

public record VerifyRequest
{
    [Required]
    public string Code { get; init; } = string.Empty;
}

public record ForgotPasswordRequest
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; init; } = string.Empty;
}

public record ResetPasswordRequest
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(4), MaxLength(10)]
    public string Code { get; init; } = string.Empty;

    [Required, MinLength(8), MaxLength(128)]
    public string NewPassword { get; init; } = string.Empty;
}

public record ForgotPasswordResponse
{
    /// <summary>
    /// Development-only convenience: the reset code just issued so the demo can complete
    /// the flow without an email gateway. The API layer strips this outside Development.
    /// </summary>
    public string? ResetCode { get; init; }
}
