using SSK.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SSK.Client.Repository
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