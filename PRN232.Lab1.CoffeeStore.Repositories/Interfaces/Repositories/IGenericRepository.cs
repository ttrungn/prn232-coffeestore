using System.Linq.Expressions;

namespace PRN232.Lab1.CoffeeStore.Repositories.Interfaces;

public interface IGenericRepository<T, in TId>
    where T : notnull
    where TId : notnull
{
    Task<T?> GetByIdAsync(
        TId id,
        CancellationToken cancellationToken = default
    );

    Task<IEnumerable<T>> GetPagedAsync(
        int skip,
        int take,
        IEnumerable<Expression<Func<T, bool>>>? filters = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        bool asNoTracking = true,
        CancellationToken cancellationToken = default
    );

    Task<int> GetTotalPagesAsync(
        int pageSize,
        IEnumerable<Expression<Func<T, bool>>>? filters = null,
        CancellationToken cancellationToken = default
    );

    IQueryable<T> Query(bool asNoTracking = true);

    Task AddAsync(
        T entity,
        CancellationToken cancellationToken = default
    );

    Task AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default
    );

    Task UpdateAsync(
        T entity,
        CancellationToken cancellationToken = default
    );

    Task RemoveAsync(
        T entity,
        CancellationToken cancellationToken = default
    );

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default
    );
}