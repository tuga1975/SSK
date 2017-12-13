using System;
using Trinity.DAL.DBContext;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Trinity.DAL.Repository
{
    public class DatabaseFactory : IDatabaseFactory
    {
        private readonly SSKEntities localDataContext;
        private readonly TrinityCentralizedDBEntities centralizedDataContext;
        public DatabaseFactory()
        {
            localDataContext = new SSKEntities();
            centralizedDataContext = new TrinityCentralizedDBEntities();
        }

        public SSKEntities GetLocalDbContext()
        {
            return localDataContext;
        }

        public TrinityCentralizedDBEntities GetCentralizedDbContext()
        {
            return centralizedDataContext;
        }
    }
}