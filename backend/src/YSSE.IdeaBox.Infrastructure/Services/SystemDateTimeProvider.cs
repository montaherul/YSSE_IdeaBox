using YSSE.IdeaBox.Application.Abstractions;

namespace YSSE.IdeaBox.Infrastructure.Services;

/// <summary>System UTC clock abstraction.</summary>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
