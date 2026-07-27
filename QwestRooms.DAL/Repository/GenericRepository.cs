using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace QwestRooms.DAL.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        private readonly DbContext context;

        public GenericRepository(DbContext _context)
        {
            context = _context;
        }

        public void Add(TEntity entity)
        {
            context.Set<TEntity>().Add(entity);
            context.SaveChanges();
        }

        public void Delete(TEntity entity)
        {
            context.Set<TEntity>().Remove(entity);
            context.SaveChanges();
        }

        public void Edit(TEntity entity)
        {
            context.Entry(entity).State = EntityState.Modified;
            context.SaveChanges();
        }

        public TEntity Find(int id)
        {
            return context.Set<TEntity>().Find(id);
        }

        public IQueryable<TEntity> GetAll()
        {
            return context.Set<TEntity>();
        }

        public IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> predicate)
        {
            return context.Set<TEntity>().Where(predicate);
        }
    }
}
