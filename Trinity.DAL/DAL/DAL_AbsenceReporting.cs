using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;


namespace Trinity.DAL
{
    public class DAL_AbsenceReporting
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        public List<AbsenceReporting> GetByUserID(string UserId)
        {
            return _localUnitOfWork.DataContext.AbsenceReportings.Where(d => d.UserId == UserId).ToList();
        }
        public int CountAbsendReporing(string UserId)
        {
            return _localUnitOfWork.DataContext.AbsenceReportings.Count(d=> d.UserId == UserId && d.Status==(int)StatusEnums.Create);
        }
    }
}
