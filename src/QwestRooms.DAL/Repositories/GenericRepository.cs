using Microsoft.EntityFrameworkCore;

namespace QwestRooms.DAL.Repositories;

/// <summary>
/// The Entity Framework Core implementation of <see cref="IGenericRepository{TEntity}"/>.
/// </summary>
/// <remarks>
/// <see cref="Query"/> is deliberately <c>AsNoTracking</c>: everything this application does with
/// a room is read it, project it and render it, so paying for change-tracking snapshots of 27
/// entity graphs per page buys nothing.
/// </remarks>
public sealed class GenericRepository<TEntity>(RoomsContext context) : IGenericRepository<TEntity>
    where TEntity : class
{
    private readonly RoomsContext _context = context;

    public IQueryable<TEntity> Query() => _context.Set<TEntity>().AsNoTracking();

    public Task<int> CountAsync(IQueryable<TEntity> query, CancellationToken cancellationToken = default) =>
        query.CountAsync(cancellationToken);

    public Task<List<TResult>> ToListAsync<TResult>(
        IQueryable<TResult> query,
        CancellationToken cancellationToken = default) =>
        query.ToListAsync(cancellationToken);
}
