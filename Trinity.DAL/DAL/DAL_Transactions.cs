using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.BE;
using Trinity.DAL.Repository;
using Trinity.Common;
using System.Data.Entity;

namespace Trinity.DAL
{
    public class DAL_Transactions
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        #region refactor 2018
        
        public Guid Insert(string NRIC, string source,string type, string content,DateTime datetime,string transaction_code)
        {
            Guid IDInsert = Guid.NewGuid();
            _localUnitOfWork.GetRepository<DBContext.Transaction>().Add(new DBContext.Transaction()
            {
                ID = IDInsert,
                datetime = datetime,
                Content = content,
                Source = source,
                NRIC = NRIC,
                TransactionCode = transaction_code
            });
            if (_localUnitOfWork.Save() > 0)
                return IDInsert;
            else
                return Guid.Empty;
        }
        #endregion
    }
}
