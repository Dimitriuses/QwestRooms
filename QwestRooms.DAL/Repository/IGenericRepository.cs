using System;
using System.Linq;
using System.Linq.Expressions;

namespace QwestRooms.DAL.Repositories
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        void Add(TEntity entity);
        void Edit(TEntity entity);
        void Delete(TEntity entity);

        /// <summary>
        /// Returns a composable query. Callers add their own filtering, ordering and paging,
        /// which the provider translates into a single SQL statement.
        /// <para>
        /// This used to return <see cref="System.Collections.Generic.IEnumerable{T}"/> over
        /// <c>AsEnumerable()</c>, which forced the entire table into memory before any of that
        /// could be applied -- the room list read all 1000 rooms to display 27 of them.
        /// </para>
        /// </summary>
        IQueryable<TEntity> GetAll();

        IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> predicate);

        TEntity Find(int id);
    }
}
