using Microsoft.EntityFrameworkCore.Storage;

namespace YSSE.IdeaBox.Application.Abstractions;

/// <summary>
/// Unit-of-work contract: exposes generic repositories for the same scoped context,
/// coordinates saves and owns transaction boundaries.
/// </summary>
public interface IUnitOfWork
{
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>Begins a transaction for operations that must succeed or fail together.</summary>
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);

    Task CommitAsync(IDbContextTransaction transaction, CancellationToken ct = default);

    Task RollbackAsync(IDbContextTransaction transaction, CancellationToken ct = default);
}