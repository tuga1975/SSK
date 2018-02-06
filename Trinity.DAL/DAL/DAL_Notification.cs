using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.BE;
using Trinity.DAL.Repository;
using Trinity.Common;

namespace Trinity.DAL
{
    public class DAL_Notification
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();


        public void SendToDutyOfficer(string UserId,string DutyOfficerID,string Subject,string Content)
        {
            if (EnumAppConfig.IsLocal)
            {
                
            }
        }


        public string GetNotificationContentById(Guid id, bool isLocal)
        {
            if (isLocal)
            {
                return _localUnitOfWork.DataContext.Notifications.Find(id).Content;
            }
            else
            {
                return _centralizedUnitOfWork.DataContext.Notifications.Find(id).Content;
            }
        }

        public int CountGetMyNotifications(string myUserId, bool isLocal)
        {
            if (isLocal)
            {
                return _localUnitOfWork.DataContext.Notifications.Count(n => n.ToUserId == myUserId && n.IsRead.HasValue && !n.IsRead.Value);
            }
            else
            {
                return _centralizedUnitOfWork.DataContext.Notifications.Count(n => n.ToUserId == myUserId && n.IsRead.HasValue && !n.IsRead.Value);
            }
        }

        public Response<List<Notification>> GetMyNotifications(string userId)
        {
            IQueryable<DBContext.Notification> queryNotifications = null;
            //if (EnumAppConfig.IsLocal)
            //{
            //    queryNotifications = _localUnitOfWork.DataContext.Notifications.Where(n => n.ToUserId == userId);
            //}
            //else
            {
                queryNotifications = _centralizedUnitOfWork.DataContext.Notifications.Where(n => n.ToUserId == userId);
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
                        Date = item.Datetime,
                        FromUserId = item.FromUserId,
                        ID = new Guid(item.NotificationID),
                        IsRead = item.IsRead.HasValue?item.IsRead.Value:false,
                        Subject = item.Subject,
                        ToUserId = item.ToUserId
                    };
                    notifications.Add(notification);
                }

                return new Response<List<Notification>>((int)EnumResponseStatuses.Success,EnumResponseMessage.Success, notifications);
            }
            return new Response<List<Notification>>((int)EnumResponseStatuses.ErrorSystem, EnumResponseMessage.ErrorSystem, null);
        }

        public List<Notification> GetNotificationsSentToDutyOfficer(bool isLocal)
        {
            IQueryable<Trinity.BE.Notification> queryNotifications = null;
            if (isLocal)
            {
                //queryNotifications = _localUnitOfWork.DataContext.Notifications.Where(n => n.ToUserId == null);
                queryNotifications = (from n in _localUnitOfWork.DataContext.Notifications
                    join u in _localUnitOfWork.DataContext.Membership_Users on n.FromUserId equals u.UserId
                    select new Trinity.BE.Notification()
                    {
                        FromUserName = u.Name,
                        Subject = n.Subject,
                        Content = n.Content,
                        Date = n.Datetime,
                        Type = n.Type,
                        Source = n.Source
                    });
            }
            else
            {
                //queryNotifications = _centralizedUnitOfWork.DataContext.Notifications.Where(n => n.ToUserId == null);
                queryNotifications = (from n in _centralizedUnitOfWork.DataContext.Notifications
                                      join u in _centralizedUnitOfWork.DataContext.Membership_Users on n.FromUserId equals u.UserId
                                      select new Trinity.BE.Notification()
                                      {
                                          FromUserName = u.Name,
                                          Subject = n.Subject,
                                          Content = n.Content,
                                          Date = n.Datetime,
                                          Type = n.Type,
                                          Source = n.Source
                                      });
            }
            if (queryNotifications.Count() > 0)
            {
                return queryNotifications.ToList<Notification>();
            }
            return null;
        }

        public List<Notification> GetNotificationsSentToDutyOfficer(bool isLocal, List<string> modules)
        {
            IQueryable<Trinity.BE.Notification> queryNotifications = null;
            if (isLocal)
            {
                //queryNotifications = _localUnitOfWork.DataContext.Notifications.Where(n => n.ToUserId == null);
                queryNotifications = (from n in _localUnitOfWork.DataContext.Notifications
                                      join u in _localUnitOfWork.DataContext.Membership_Users on n.FromUserId equals u.UserId
                                      select new Trinity.BE.Notification()
                                      {
                                          FromUserName = u.Name,
                                          Subject = n.Subject,
                                          Content = n.Content,
                                          Date =
                                          n.Datetime,
                                          Type = n.Type,
                                          Source = n.Source
                                      }).Where(x => modules.Contains(x.Source));
            }
            else
            {
                //queryNotifications = _centralizedUnitOfWork.DataContext.Notifications.Where(n => n.ToUserId == null);
                queryNotifications = (from n in _centralizedUnitOfWork.DataContext.Notifications
                                      join u in _centralizedUnitOfWork.DataContext.Membership_Users on n.FromUserId equals u.UserId
                                      select new Trinity.BE.Notification()
                                      {
                                          FromUserName = u.Name,
                                          Subject = n.Subject,
                                          Content = n.Content,
                                          Date = n.Datetime,
                                          Type = n.Type,
                                          Source = n.Source
                                      }).Where(x => modules.Contains(x.Source));
            }
            if (queryNotifications.Count() > 0)
            {
                return queryNotifications.ToList<Notification>();
            }
            return null;
        }


        public void InsertNotification(string subject, string content, string fromUserId, string toUserId, bool isFromSupervisee, bool isLocal)
        {
            Trinity.DAL.DBContext.Notification notifcation = new DBContext.Notification()
            {
                Content = content,
                Datetime = DateTime.Now,
                FromUserId = fromUserId,
                IsFromSupervisee = isFromSupervisee,
                IsRead = false,
                Subject = subject,
                ToUserId = toUserId,
                NotificationID = Guid.NewGuid().ToString().Trim()
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

        /// <summary>
        /// Add notification
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="content"></param>
        /// <param name="fromUserId"></param>
        /// <param name="toUserId"></param>
        /// <param name="isFromSupervisee"></param>
        /// <param name="isLocal"></param>
        /// <param name="notifyType">NotificationType : {Error : 'E', Notification : 'N', Caution : 'C'}</param>
        /// <param name="source">EnumStations : {SSA, SSK, UHP, ASP, HSA, ASP}</param>
        public void InsertNotification(string subject, string content, string fromUserId,
            string toUserId, bool isFromSupervisee, bool isLocal, string notifyType,
            string source)
        {
            Trinity.DAL.DBContext.Notification notifcation = new DBContext.Notification()
            {
                Content = content,
                Datetime = DateTime.Now,
                FromUserId = fromUserId,
                IsFromSupervisee = isFromSupervisee,
                IsRead = false,
                Subject = subject,
                ToUserId = toUserId,
                Type = notifyType,
                Source = source,
                NotificationID = Guid.NewGuid().ToString().Trim()
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

        public Trinity.DAL.DBContext.Notification GetNotification(Guid notificationId, bool isLocal)
        {
            if (isLocal)
            {
                return _localUnitOfWork.DataContext.Notifications.Find(notificationId);
            }
            else
            {
                return _centralizedUnitOfWork.DataContext.Notifications.Find(notificationId);
            }

        }
        public void ChangeReadStatus(string notificationId)
        {

            var localRepo = _localUnitOfWork.GetRepository<DBContext.Notification>();
            var centralRepo = _centralizedUnitOfWork.GetRepository<DBContext.Notification>();

            var dbLocalNotification = GetNotification(Guid.Parse(notificationId), true);
            if (dbLocalNotification != null)
            {
                dbLocalNotification.IsRead = true;
                localRepo.Update(dbLocalNotification);
                _localUnitOfWork.Save();
            }


            var dbCentralNotification = GetNotification(Guid.Parse(notificationId), false);
            if (dbCentralNotification != null)
            {
                dbCentralNotification.IsRead = true;
                centralRepo.Update(dbCentralNotification);
                _centralizedUnitOfWork.Save();
            }

        }
    }
}
