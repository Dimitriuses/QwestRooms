namespace QwestRooms.DAL.Repositories;

/// <summary>
/// Read access to one entity set, split deliberately into <em>composing</em> a query and
/// <em>executing</em> it.
/// </summary>
/// <remarks>
/// <para>
/// The return type of <see cref="Query"/> is the whole point of this interface. It used to be
/// <see cref="System.Collections.Generic.IEnumerable{T}"/> over <c>DbSet.AsEnumerable()</c>, which
/// executed <c>SELECT * FROM Rooms</c> the moment it was called: every filter, sort and page the
/// caller then applied ran in memory over the finished list, and every navigation property the
/// caller touched became another round trip. Handing back <see cref="IQueryable{T}"/> instead lets
/// the caller's <c>Where</c>/<c>OrderBy</c>/<c>Skip</c>/<c>Take</c>/<c>Select</c> become part of
/// the SQL that is eventually sent.
/// </para>
/// <para>
/// The execution methods live here rather than in the business layer so that the business layer
/// needs no reference to Entity Framework -- <c>ToListAsync</c> is an EF Core extension method, and
/// calling it from a service would tie that service to the provider it is trying to stay ignorant
/// of.
/// </para>
/// </remarks>
public interface IGenericRepository<TEntity>
    where TEntity : class
{
    /// <summary>Starts a composable query. Nothing is sent to the database until it is executed.</summary>
    IQueryable<TEntity> Query();

    /// <summary>Counts the rows a composed query matches, in the database.</summary>
    Task<int> CountAsync(IQueryable<TEntity> query, CancellationToken cancellationToken = default);

    /// <summary>Executes a composed query -- usually one ending in a projection -- and materialises it.</summary>
    Task<List<TResult>> ToListAsync<TResult>(IQueryable<TResult> query, CancellationToken cancellationToken = default);
}
