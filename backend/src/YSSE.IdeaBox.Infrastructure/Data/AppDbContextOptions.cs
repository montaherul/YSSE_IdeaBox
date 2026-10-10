using Microsoft.EntityFrameworkCore;

namespace YSSE.IdeaBox.Infrastructure.Data;

/// <summary>
/// Centralised EF Core option configuration (PostgreSQL + snake_case naming).
/// Kept in one place so the runtime DI registration and the design-time factory
/// always build equivalent options.
/// </summary>
public static class AppDbContextOptions
{
    public const string DefaultConnectionString =
        "Host=localhost;Port=5432;Database=ysse_ideabox;Username=postgres;Password=admin";

    public static DbContextOptionsBuilder Configure(DbContextOptionsBuilder builder, string? connectionString = null)
    {
        builder
            .UseNpgsql(string.IsNullOrWhiteSpace(connectionString) ? DefaultConnectionString : connectionString)
            .UseSnakeCaseNamingConvention();

        return builder;
    }
}
