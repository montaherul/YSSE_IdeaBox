using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using YSSE.IdeaBox.Application.Abstractions;
using YSSE.IdeaBox.Infrastructure.Data;

namespace YSSE.IdeaBox.Infrastructure.Repositories;

/// <summary>
/// Single generic repository implementation. Standard CRUD uses EF Core via the
/// scoped context. Reporting SQL lives in the Dapper-backed query classes.
/// </summary>
public sealed class GenericRepository<TEntity> : IGenericRepository<TEntity>
    where TEntity : class
{
    private readonly AppDbContext _context;

    public GenericRepository(AppDbContext context) => _context = context;

    private DbSet<TEntity> Set => _context.Set<TEntity>();

    public IQueryable<TEntity> Query() => Set.AsQueryable();

    public async Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default)
        => await Set.FindAsync(new object[] { id }, ct);

    public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default)
        => await Set.AnyAsync(predicate, ct);

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default)
        => predicate is null ? await Set.CountAsync(ct) : await Set.CountAsync(predicate, ct);

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default)
    {
        await Set.AddAsync(entity, ct);
        return entity;
    }

    public void Update(TEntity entity) => Set.Update(entity);

    public void Remove(TEntity entity) => Set.Remove(entity);

    public void RemoveRange(IEnumerable<TEntity> entities) => Set.RemoveRange(entities);
}