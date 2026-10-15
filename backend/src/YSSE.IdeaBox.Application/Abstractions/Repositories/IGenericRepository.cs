using System.Linq.Expressions;

namespace YSSE.IdeaBox.Application.Abstractions;

/// <summary>
/// Generic data-access contract implemented by the Infrastructure layer.
/// Standard CRUD flows through <see cref="Query"/> / Add / Update / Remove.
/// Complex/reporting reads live in dedicated Infrastructure query classes exposed
/// through Application read-model abstractions (never raw SQL on this repository).
/// </summary>
public interface IGenericRepository<TEntity>
    where TEntity : class
{
    /// <summary>Queryable access to the entity set (EF Core).</summary>
    IQueryable<TEntity> Query();

    Task<TEntity?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken ct = default);

    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken ct = default);

    Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default);

    void Update(TEntity entity);

    void Remove(TEntity entity);

    void RemoveRange(IEnumerable<TEntity> entities);
}