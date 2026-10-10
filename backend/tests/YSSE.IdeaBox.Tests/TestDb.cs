using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Infrastructure;
using YSSE.IdeaBox.Infrastructure.Data;

namespace YSSE.IdeaBox.Tests;

/// <summary>
/// In-memory EF provider harness wired through the real GenericRepository/UnitOfWork
/// so service tests exercise the same persistence path production uses.
/// </summary>
internal static class TestDb
{
    public static (AppDbContext Context, IUnitOfWork UnitOfWork) Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"ideabox-tests-{Guid.NewGuid():N}")
            .Options;
        var context = new AppDbContext(options);
        return (context, new UnitOfWork(context, new ThrowingSqlQueryExecutor()));
    }
}