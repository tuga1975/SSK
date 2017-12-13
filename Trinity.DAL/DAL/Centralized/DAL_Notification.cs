using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.BE;
using Trinity.DAL.Repository;

namespace Trinity.DAL.Centralized
{
    public class DAL_Notification
    {
        Centralized_UnitOfWork _unitOfWork = new Centralized_UnitOfWork();

        public List<Notification> GetMyNotifications(string myUserId)
        {
            IQueryable<DBContext.Notification> queryNotifications = _unitOfWork.DataContext.Notifications.Where(n => n.FromUserId == myUserId);
            int count = queryNotifications.Count();
            if (count > 0)
            {
                List<Notification> notifications = new List<Notification>();
                foreach (var item in queryNotifications)
                {
                    Notification notification = new Notification()
                    {
                        Content = item.Content,
                        Date = item.Date.Value,
                        FromUserId = item.FromUserId,
                        ID = item.ID,
                        IsRead = item.IsRead.Value,
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
