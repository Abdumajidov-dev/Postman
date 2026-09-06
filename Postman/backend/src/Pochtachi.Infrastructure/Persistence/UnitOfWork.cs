using System.Collections.Concurrent;
using Pochtachi.Domain.Common;

namespace Pochtachi.Infrastructure.Persistence;

public class UnitOfWork(PochtachiDbContext context) : IUnitOfWork
{
    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    public IRepository<T> Repository<T>() where T : BaseEntity =>
        (IRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new Repository<T>(context));

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
}
