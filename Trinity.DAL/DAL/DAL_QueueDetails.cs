using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;
using Trinity.Identity;

namespace Trinity.DAL
{
    public class DAL_QueueDetails
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();


        #region 2018
        public void RemoveQueueFromSSK(string UserID)
        {
            if (EnumAppConfig.IsLocal)
            {
                var queueDetail = _localUnitOfWork.DataContext.Queues.Where(item => DbFunctions.TruncateTime(item.CreatedTime).Value == DateTime.Now.Date && ((item.Appointment_ID.HasValue && item.Appointment.UserId == UserID) || (!item.Appointment_ID.HasValue && item.Created_By == UserID))).SelectMany(d => d.QueueDetails).FirstOrDefault(d => d.Station == EnumStation.ARK);

                // Need to check why queue detail is null
                if (queueDetail != null)
                {
                    queueDetail.Status = EnumQueueStatuses.Finished;
                    _localUnitOfWork.GetRepository<DAL.DBContext.QueueDetail>().Update(queueDetail);
                    _localUnitOfWork.Save();
                }
            }
            else
            {
                var queueDetail = _centralizedUnitOfWork.DataContext.Queues.Include("Appointment").Include("QueueDetails").Where(item => DbFunctions.TruncateTime(item.CreatedTime).Value == DateTime.Now.Date && ((item.Appointment_ID.HasValue && item.Appointment.UserId == UserID) || (!item.Appointment_ID.HasValue && item.Created_By == UserID))).SelectMany(d => d.QueueDetails).FirstOrDefault(d => d.Station == EnumStation.ARK);
                if (queueDetail != null)
                {
                    queueDetail.Status = EnumQueueStatuses.Finished;
                    _centralizedUnitOfWork.GetRepository<DAL.DBContext.QueueDetail>().Update(queueDetail);
                    _centralizedUnitOfWork.Save();
                }
            }
        }
        public void UpdateStatusQueueDetail(Guid QueueID, string Station, string Status)
        {
            var queueDetail = _localUnitOfWork.DataContext.QueueDetails.FirstOrDefault(d => d.Queue_ID == QueueID && d.Station == Station);
            queueDetail.Status = Status;
            _localUnitOfWork.GetRepository<DAL.DBContext.QueueDetail>().Update(queueDetail);
            _localUnitOfWork.Save();
        }
        #endregion




    }
}
