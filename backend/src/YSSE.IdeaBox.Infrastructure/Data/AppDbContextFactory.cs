using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace YSSE.IdeaBox.Infrastructure.Data;

/// <summary>
/// Design-time factory used by the `dotnet ef` tooling. Reads the connection string
/// from the <c>YSSE_DB_CONNECTION</c> environment variable and falls back to the
/// local development database. Never used at application runtime.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("YSSE_DB_CONNECTION");

        var options = new DbContextOptionsBuilder<AppDbContext>();
        AppDbContextOptions.Configure(options, connectionString);

        return new AppDbContext(options.Options);
    }
}
