using SSK.DbContext;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace SSK.Client.Repository
{
    public class Repository<T> : IRepository<T>
        where T : class
    {
        private readonly IDbSet<T> dbset;
        private SSKEntities dataContext;
        public Repository(IDatabaseFactory databaseFactory)
        {
            DatabaseFactory = databaseFactory;
            dbset = DataContext.Set<T>();
        }

        public Repository(SSKEntities dataContext)
        {
            this.dataContext = dataContext;
            dbset = this.dataContext.Set<T>();
        }

        protected IDatabaseFactory DatabaseFactory
        {
            get;
            private set;
        }

        protected SSKEntities DataContext
        {
            get { return dataContext ?? (dataContext = DatabaseFactory.GetDbContext()); }
        }

        /// <summary>
        /// Gets lấy về tổng số
        /// </summary>
        public virtual int Count
        {
            get { return dbset.Count(); }
        }

        public virtual IQueryable<T> GetAll()
        {
            return dbset.AsQueryable();
        }

        public virtual IQueryable<T> GetAll(Expression<Func<T, string>> orderByProperty, bool isAscendingOrder)
        {
            var resetSet = dbset.AsQueryable();
            resetSet = isAscendingOrder ? resetSet.OrderBy(orderByProperty) : resetSet.OrderByDescending(orderByProperty);
            return resetSet;
        }

        public virtual T GetById(decimal id)
        {
            return dbset.Find(id);
        }

        public virtual T GetById(string id)
        {
            return dbset.Find(id);
        }

        public T Get(Expression<Func<T, bool>> predicate)
        {
            return dbset.Where(predicate).FirstOrDefault<T>();
        }

        public virtual IQueryable<T> GetMany(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, int count = 0, string includeProperties = "")
        {
            var query = filter != null ? dbset.Where(filter) : dbset;
            if (!String.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (count > 0)
            {
                query = query.Take(count);
            }

            return query;
        }

        public virtual IQueryable<T> GetMany(Expression<Func<T, bool>> predicate)
        {
            return dbset.Where(predicate);
        }

        public virtual IQueryable<T> GetPageMany(Expression<Func<T, bool>> filter, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy, int pageNum = 0, int pageSize = 20)
        {
            if (pageSize <= 0)
            {
                pageSize = 20;
            }

            var query = filter != null ? dbset.Where(filter) : dbset;
            int excludedRows = (pageNum - 1) * (pageSize - 1);
            if (excludedRows <= 0)
            {
                excludedRows = 0;
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return query.Skip(excludedRows).Take(pageSize);
        }

        public virtual T Add(T entity)
        {
            return dbset.Add(entity);
        }

        public virtual void AddRange(IList<T> entities)
        {
            foreach (T e in entities)
            {
                dbset.Add(e);
            }
        }

        public virtual void Update(T entity)
        {
            dbset.Attach(entity);
            dataContext.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Delete(T entity)
        {
            dbset.Remove(entity);
        }

        public virtual void DontUpdateProperty<TEntity>(string propertyName)
        {
            var objectContext = ((IObjectContextAdapter)dataContext).ObjectContext;

            foreach (var entry in objectContext.ObjectStateManager.GetObjectStateEntries(EntityState.Modified).Where(entity => entity.Entity.GetType() == typeof(TEntity)))
            {
                entry.RejectPropertyChanges(propertyName);
            }
        }

        public virtual void Delete(Expression<Func<T, bool>> predicate)
        {
            IQueryable<T> objects = dbset.Where<T>(predicate);
            foreach (T obj in objects)
            {
                dbset.Remove(obj);
            }
        }
    }
}