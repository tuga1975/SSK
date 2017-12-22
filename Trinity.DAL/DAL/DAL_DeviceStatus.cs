using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;

namespace Trinity.DAL.DAL
{
    class DAL_DeviceStatus
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        public bool Insert(Trinity.DAL.DBContext.ApplicationDevice_Status model)
        {
            return false;
        }
    }
}
