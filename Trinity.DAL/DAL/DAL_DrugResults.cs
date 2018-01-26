using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;


namespace Trinity.DAL
{
    public class DAL_DrugResults
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        public DBContext.DrugResult GetByMarkingNumber(string MarkingNumber)
        {
            return _centralizedUnitOfWork.DataContext.DrugResults.FirstOrDefault(d => d.markingnumber == MarkingNumber);
        }
    }
}
