using System;

namespace Trinity.DAL.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> GetRepository<T>()
            where T : class;
        int Save();
    }
}