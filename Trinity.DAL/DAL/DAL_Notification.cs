using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.BE;
using Trinity.DAL.Repository;

namespace Trinity.DAL
{
    public class DAL_Notification
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        public List<Notification> GetMyNotifications(string myUserId, bool isLocal)
        {
            IQueryable<DBContext.Notification> queryNotifications = null;
            if (isLocal)
            {
                queryNotifications = _localUnitOfWork.DataContext.Notifications.Where(n => n.ToUserId == myUserId);
            }
            else
            {
                queryNotifications = _centralizedUnitOfWork.DataContext.Notifications.Where(n => n.ToUserId == myUserId);
            }
            int count = queryNotifications.Count();
            if (count > 0)
            {
                List<Notification> notifications = new List<Notification>();
                foreach (var item in queryNotifications)
                {
                    Notification notification = new Notification()
                    {
                        Content = item.Content,
                        Date = item.Date,
                        FromUserId = item.FromUserId,
                        ID = item.ID,
                        IsRead = item.IsRead,
                        Subject = item.Subject,
                        ToUserId = item.ToUserId
                    };
                    notifications.Add(notification);
                }

                return notifications;
            }
            return null;
        }

        public void InsertNotification(string subject, string content, string fromUserId, string toUserId, bool isFromSupervisee, bool isLocal)
        {
            Trinity.DAL.DBContext.Notification notifcation = new DBContext.Notification()
            {
                Content = content,
                Date = DateTime.Now,
                FromUserId = fromUserId,
                IsFromSupervisee = isFromSupervisee,
                IsRead = false,
                Subject = subject,
                ToUserId = toUserId
            };
            IRepository<Trinity.DAL.DBContext.Notification> notificationRepo = null;
            if (isLocal)
            {
                notificationRepo = _localUnitOfWork.GetRepository<Trinity.DAL.DBContext.Notification>();
                notificationRepo.Add(notifcation);
                _localUnitOfWork.Save();
            }
            else
            {
                notificationRepo = _centralizedUnitOfWork.GetRepository<Trinity.DAL.DBContext.Notification>();
                notificationRepo.Add(notifcation);
                _centralizedUnitOfWork.Save();
            }
        }
    }
}
