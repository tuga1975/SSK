using System;
using Trinity.DAL.DBContext;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trinity.DAL.Repository
{
    public class DatabaseFactory : IDatabaseFactory
    {
        private readonly SSKEntities dataContext;
        public DatabaseFactory()
        {
            dataContext = new SSKEntities();
        }

        public SSKEntities GetDbContext()
        {
            return dataContext;
        }
    }
}