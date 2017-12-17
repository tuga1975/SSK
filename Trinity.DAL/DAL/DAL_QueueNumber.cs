using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;


namespace Trinity.DAL
{
    public class DAL_QueueNumber
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        public void InsertQueueNumber(Guid AppointmentID, string UserId)
        {
            _localUnitOfWork.GetRepository<QueueNumber>().Add(new QueueNumber() {
                AppointmentID = AppointmentID,
                ID = Guid.NewGuid(),
                Date = DateTime.Today,
                Status = (int)StatusEnums.Wait,
                UserId = UserId
            });
            _localUnitOfWork.Save();
        }


        public List<QueueNumber> GetAllQueueNumberByDate(DateTime date)
        {
            date = date.Date;
            return _localUnitOfWork.DataContext.QueueNumbers.Include(d=>d.User).Where(d => DbFunctions.TruncateTime(d.Date).Value == date && (d.Status == (int)StatusEnums.Wait || d.Status == (int)StatusEnums.Working)).OrderBy(d=>d.Date).ToList();
        }
    }
}
