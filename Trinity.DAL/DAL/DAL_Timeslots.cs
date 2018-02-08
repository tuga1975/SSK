using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;

namespace Trinity.DAL
{
    public class DAL_Timeslots
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        public List<Timeslot> GetTimeSlots(DateTime date)
        {
            try
            {
                // get data from local db
                if (EnumAppConfig.IsLocal)
                {
                    List<Timeslot> timeslots = _localUnitOfWork.DataContext.Timeslots.Where(item => item.Date == date).ToList();

                    // if local have no data, get data from centralizedapi, then update local
                    if (timeslots == null && !EnumAppConfig.ByPassCentralizedDB)
                    {

                    }

                    return timeslots;
                }
                else // request from centralized api
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
