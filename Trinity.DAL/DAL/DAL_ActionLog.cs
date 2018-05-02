using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;
using Trinity.Common;

namespace Trinity.DAL
{
    public class DAL_ActionLog
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        public void Insert(string ActionName, string PerformedBy, string Note, string Station)
        {
            _localUnitOfWork.GetRepository<DBContext.ActionLog>().Add(new ActionLog()
            {
                ActionName = ActionName,
                Action_ID = Guid.NewGuid(),
                Note = Note,
                PerformedBy = PerformedBy,
                PerformedDate = DateTime.Now,
                Station = Station
            });
            _localUnitOfWork.Save();
        }
    }
}
