using System.Linq.Expressions;

namespace Pochtachi.Domain.Common;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);

    /// <summary>Include zanjiri kerak bo'lgan holatlar uchun — chaqiruvchi navigation property'larni yuklaydi.</summary>
    IQueryable<T> Query();
}
