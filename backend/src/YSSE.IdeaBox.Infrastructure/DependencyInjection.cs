using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Infrastructure.Data;
using YSSE.IdeaBox.Infrastructure.Repositories;
using YSSE.IdeaBox.Infrastructure.Services;

namespace YSSE.IdeaBox.Infrastructure;

/// <summary>Registers the infrastructure layer (EF Core context + concrete services).</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");

        services.AddDbContext<AppDbContext>(options => AppDbContextOptions.Configure(options, connectionString));

        // Same scoped context instance exposed through the application abstraction.
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        // Repository pattern: services depend on the unit of work, which exposes
        // generic repositories bound to the same scoped context instance.
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IGuestIdentityService, GuestIdentityService>();
        services.AddScoped<IHashingService, HashingService>();
        services.AddScoped<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<ISqlQueryExecutor, DapperSqlQueryExecutor>();
        services.AddScoped<IFileStorage, LocalFileStorage>();
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
        services.AddScoped<IAuditWriter, AuditWriter>();

        // Verification codes are short-lived and process-wide.
        services.AddSingleton<IVerificationCodeStore, InMemoryVerificationCodeStore>();

        return services;
    }
}
