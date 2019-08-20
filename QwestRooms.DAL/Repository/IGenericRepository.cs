using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace DataAccessLayer.Repositories
{
    public interface IGenericRepository<TEntity> where TEntity: class
    {
        void Add(TEntity entity);
        void Edit(TEntity entity);
        void Delete(TEntity entity);
        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> GetAll(Expression<Func<TEntity,bool>> predicate);
        TEntity Find(int Id);
    }
}
