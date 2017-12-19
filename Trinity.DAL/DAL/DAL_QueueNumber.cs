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
            var generateQNo = GenerateQueueNumber(_localUnitOfWork.DataContext.Users.Find(UserId).NRIC);
            _localUnitOfWork.GetRepository<QueueNumber>().Add(new QueueNumber()
            {
                Appointment_ID = AppointmentID,
                ID = Guid.NewGuid(),
                CreatedTime = DateTime.Today,
                Status = StatusConstant.Wait,
                QueuedNumber=generateQNo
            });
            _localUnitOfWork.Save();
        }


        public string GenerateQueueNumber(string baseOnNRIC)
        {
            string queueNumber = "";
            if (!string.IsNullOrEmpty(baseOnNRIC) && baseOnNRIC.Length > 6)
            {
                queueNumber += baseOnNRIC.Substring(0, 1) + baseOnNRIC.Substring(baseOnNRIC.Length - 5, 5).PadLeft(8, '*');
            }
            else if (!string.IsNullOrEmpty(baseOnNRIC) && baseOnNRIC.Length <= 6)
            {
                queueNumber += baseOnNRIC.Substring(0, 1) + baseOnNRIC.PadLeft(8, '*');
            }
            else
            {
                queueNumber += null;
            }

            return queueNumber;

        }
        public List<QueueNumber> GetAllQueueNumberByDate(DateTime date)
        {
            date = date.Date;
            return _localUnitOfWork.DataContext.QueueNumbers.Include(d => d.Appointment).Where(d => DbFunctions.TruncateTime(d.CreatedTime).Value == date && (d.Status == StatusConstant.Wait || d.Status == StatusConstant.Working)).OrderBy(d => d.CreatedTime).ToList();
        }

        public bool CheckQueueExistToday(string userId)
        {
            if (CountQueueByStatus(userId, StatusConstant.Wait) > 0 || CountQueueByStatus(userId, StatusConstant.Working) > 0)
            {
                return true;
            }
            if (CountQueueByStatus(userId, StatusConstant.Miss) > 0)
            {
                return false;
            }
            return false;


        }

        public int CountQueueByStatus(string userId, string status)
        {
            var today = DateTime.Now.Date;
            return _localUnitOfWork.DataContext.QueueNumbers.Include(u=>u.Appointment).Count(d => DbFunctions.TruncateTime(d.CreatedTime).Value == today && d.Appointment.UserId == userId && d.Status == status);
        }
    }
}
