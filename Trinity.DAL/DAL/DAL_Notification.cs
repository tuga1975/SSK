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
                queryNotifications = _localUnitOfWork.DataContext.Notifications.Where(n => n.FromUserId == myUserId);
            } else
            {
                queryNotifications = _centralizedUnitOfWork.DataContext.Notifications.Where(n => n.FromUserId == myUserId);
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
    }
}
