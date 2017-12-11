using System;

namespace SSK.Client.Repository
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> GetRepository<T>()
            where T : class;
        int Save();
    }
}