using Microsoft.EntityFrameworkCore.Storage;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Infrastructure.Data;
using YSSE.IdeaBox.Infrastructure.Repositories;

namespace YSSE.IdeaBox.Infrastructure;

/// <summary>
/// Default unit of work over the scoped EF Core context. Exposes fresh generic
/// repository instances sharing the same context instance.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context) => _context = context;

    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
        => new GenericRepository<TEntity>(_context);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _context.SaveChangesAsync(ct);

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default)
        => _context.Database.BeginTransactionAsync(ct);

    public Task CommitAsync(IDbContextTransaction transaction, CancellationToken ct = default)
        => transaction.CommitAsync(ct);

    public Task RollbackAsync(IDbContextTransaction transaction, CancellationToken ct = default)
        => transaction.RollbackAsync(ct);
}