using Trinity.DAL.DBContext;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Trinity.DAL.Repository
{
    public class Local_UnitOfWork : IUnitOfWork
    {
        private readonly IDatabaseFactory databaseFactory;
        private SSKEntities dataContext;

        private bool disposed;

        public Local_UnitOfWork(IDatabaseFactory databaseFactory)
        {
            // _databaseFactory = databaseFactory;
            this.databaseFactory = new DatabaseFactory();
            dataContext = this.databaseFactory.GetLocalDbContext();
        }

        public Local_UnitOfWork()
        {
            databaseFactory = new DatabaseFactory();
            dataContext = databaseFactory.GetLocalDbContext();
        }

        public SSKEntities DataContext
        {
            get { return dataContext ?? (dataContext = databaseFactory.GetLocalDbContext()); }
        }

        public IRepository<T> GetRepository<T>()
            where T : class
        {
            return new LocalRepository<T>(dataContext);
        }

        public int Save()
        {
            if (dataContext.GetValidationErrors().Any())
            {
                throw new Trinity.Common.ExceptionArgs(dataContext.GetValidationErrors().ToList()[0].ValidationErrors.ToList()[0].ErrorMessage);
            }
            return DataContext.SaveChanges();
        }

        public Task<int> SaveAsync()
        {
            if (dataContext.GetValidationErrors().Any())
            {
                throw new Trinity.Common.ExceptionArgs(dataContext.GetValidationErrors().ToList()[0].ValidationErrors.ToList()[0].ErrorMessage);
            }
            return DataContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    dataContext.Dispose();
                    disposed = true;
                }
            }

            disposed = false;
        }
    }

    public class Centralized_UnitOfWork : IUnitOfWork
    {
        private readonly IDatabaseFactory databaseFactory;
        private TrinityCentralizedDBEntities dataContext;

        private bool disposed;

        public Centralized_UnitOfWork(IDatabaseFactory databaseFactory)
        {
            // _databaseFactory = databaseFactory;
            this.databaseFactory = new DatabaseFactory();
            dataContext = this.databaseFactory.GetCentralizedDbContext();
        }

        public Centralized_UnitOfWork()
        {
            databaseFactory = new DatabaseFactory();
            dataContext = databaseFactory.GetCentralizedDbContext();
        }

        public TrinityCentralizedDBEntities DataContext
        {
            get { return dataContext ?? (dataContext = databaseFactory.GetCentralizedDbContext()); }
        }

        public IRepository<T> GetRepository<T>()
            where T : class
        {
            return new CentralizedRepository<T>(dataContext);
        }

        public int Save()
        {
            if (dataContext.GetValidationErrors().Any())
            {
                throw new Trinity.Common.ExceptionArgs(dataContext.GetValidationErrors().ToList()[0].ValidationErrors.ToList()[0].ErrorMessage);
            }
            return DataContext.SaveChanges();
        }

        public Task<int> SaveAsync()
        {
            if (dataContext.GetValidationErrors().Any())
            {
                throw new Trinity.Common.ExceptionArgs(dataContext.GetValidationErrors().ToList()[0].ValidationErrors.ToList()[0].ErrorMessage);
            }
            return DataContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    dataContext.Dispose();
                    disposed = true;
                }
            }

            disposed = false;
        }
    }
}