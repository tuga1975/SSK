using SSK.DbContext;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SSK.Client.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IDatabaseFactory databaseFactory;
        private SSKEntities dataContext;
        private bool disposed;

        public UnitOfWork(IDatabaseFactory databaseFactory)
        {
            // _databaseFactory = databaseFactory;
            this.databaseFactory = new DatabaseFactory();
            dataContext = this.databaseFactory.GetDbContext();
        }

        public UnitOfWork()
        {
            databaseFactory = new DatabaseFactory();
            dataContext = databaseFactory.GetDbContext();
        }

        public SSKEntities DataContext
        {
            get { return dataContext ?? (dataContext = databaseFactory.GetDbContext()); }
        }

        public IRepository<T> GetRepository<T>()
            where T : class
        {
            return new Repository<T>(dataContext);
        }

        public int Save()
        {
            if (dataContext.GetValidationErrors().Any())
            {
                throw new Exception(dataContext.GetValidationErrors().ToList()[0].ValidationErrors.ToList()[0].ErrorMessage);
            }
            return DataContext.SaveChanges();
        }

        public Task<int> SaveAsync()
        {
            if (dataContext.GetValidationErrors().Any())
            {
                throw new Exception(dataContext.GetValidationErrors().ToList()[0].ValidationErrors.ToList()[0].ErrorMessage);
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