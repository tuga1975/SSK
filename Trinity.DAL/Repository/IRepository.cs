using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Trinity.DAL.Repository
{
    public interface IRepository<T>
        where T : class
    {
        int Count { get; }

        IQueryable<T> GetAll();
        IQueryable<T> GetAll(Expression<Func<T, string>> orderByProperty, bool isAscendingOrder);
        T GetById(decimal id);
        T GetById(string id);
        T Get(Expression<Func<T, bool>> predicate);
        IQueryable<T> GetMany(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, int count = 0, string includeProperties = "");
        IQueryable<T> GetMany(Expression<Func<T, bool>> predicate);
        IQueryable<T> GetPageMany(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, int pageNum = 0, int pageSize = 20);
        T Add(T entity);

        void AddRange(IList<T> entities);
        void Update(T entity);
        void Delete(T entity);
        void DontUpdateProperty<TEntity>(string propertyName);
        void Delete(Expression<Func<T, bool>> predicate);
    }
}