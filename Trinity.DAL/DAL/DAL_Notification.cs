﻿using System;
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

        #region refactor 2018
        public List<Notification> GetAllNotifications(string userId)
        {
            // local request
            if (EnumAppConfig.IsLocal)
            {
                // select from localdb
                List<Notification> notifications = _localUnitOfWork.DataContext.Notifications
                    .Where(d => (!string.IsNullOrEmpty(d.FromUserId) && d.FromUserId == userId) || (!string.IsNullOrEmpty(d.ToUserId) && d.ToUserId == userId))
                    .Select(item => new Notification()
                    {
                        Content = item.Content,
                        Datetime = item.Datetime,
                        FromUserId = item.FromUserId,
                        NotificationID = item.NotificationID,
                        IsRead = item.IsRead.HasValue ? item.IsRead.Value : false,
                        Subject = item.Subject,
                        ToUserId = item.ToUserId
                    }).ToList();

                // if null, get data from centralized (check bypass)
                if (notifications == null && notifications.Count == 0 && !EnumAppConfig.ByPassCentralizedDB)
                {

                }

                // if centralized had data, update local

                return notifications;
            }
            else // centralized api request
            {
                return _centralizedUnitOfWork.DataContext.Notifications.Where(d => (!string.IsNullOrEmpty(d.FromUserId) && d.FromUserId == userId) || (!string.IsNullOrEmpty(d.ToUserId) && d.ToUserId == userId)).Select(item => new Notification()
                {
                    Content = item.Content,
                    Datetime = item.Datetime,
                    FromUserId = item.FromUserId,
                    NotificationID = item.NotificationID,
                    IsRead = item.IsRead.HasValue ? item.IsRead.Value : false,
                    Subject = item.Subject,
                    ToUserId = item.ToUserId
                }).ToList();
            }
        }

        public List<Notification> GetAllNotifications(string userId, List<string> modules)
        {
            // local request
            if (EnumAppConfig.IsLocal)
            {
                // select from localdb
                List<Notification> notifications = _localUnitOfWork.DataContext.Notifications
                    .Where(d => (!string.IsNullOrEmpty(d.FromUserId) && d.FromUserId == userId) || (!string.IsNullOrEmpty(d.ToUserId) && d.ToUserId == userId))
                    .Select(item => new Notification()
                    {
                        Content = item.Content,
                        Datetime = item.Datetime,
                        FromUserId = item.FromUserId,
                        NotificationID = item.NotificationID,
                        IsRead = item.IsRead.HasValue ? item.IsRead.Value : false,
                        Subject = item.Subject,
                        ToUserId = item.ToUserId,
                        Type = item.Type,
                        Source = item.Source
                    }).Where(x => modules.Contains(x.Source)).ToList();

                // if null, get data from centralized (check bypass)
                if (notifications == null && notifications.Count == 0 && !EnumAppConfig.ByPassCentralizedDB)
                {

                }

                // if centralized had data, update local

                return notifications;
            }
            else // centralized api request
            {
                return _centralizedUnitOfWork.DataContext.Notifications.Where(d => (!string.IsNullOrEmpty(d.FromUserId) && d.FromUserId == userId) || (!string.IsNullOrEmpty(d.ToUserId) && d.ToUserId == userId)).Select(item => new Notification()
                {
                    Content = item.Content,
                    Datetime = item.Datetime,
                    FromUserId = item.FromUserId,
                    NotificationID = item.NotificationID,
                    IsRead = item.IsRead.HasValue ? item.IsRead.Value : false,
                    Subject = item.Subject,
                    ToUserId = item.ToUserId,
                     Type = item.Type,
                    Source = item.Source
                }).Where(x => modules.Contains(x.Source)).ToList();
            }
        }

        public string GetNotification(string id)
        {
            // local request
            if (EnumAppConfig.IsLocal)
            {
                // select from localdb
                return _localUnitOfWork.DataContext.Notifications.Find(id).Content;

                // no need sync data because GetAllNotifications() already did it
            }

            return string.Empty;
        }
        #endregion

        /// <summary>
        /// App call ko cần truyền Station
        /// </summary>
        /// <param name="MessageID"></param>
        /// <param name="UserId"></param>
        /// <param name="DutyOfficerID"></param>
        /// <param name="Subject"></param>
        /// <param name="Content"></param>
        /// <param name="notificationType"></param>
        /// <param name="Station"></param>
        public int SendToDutyOfficer(string MessageID,string UserId, string DutyOfficerID, string Subject, string Content, string notificationType, string Station)
        {
            if (EnumAppConfig.IsLocal)
            {
                _localUnitOfWork.GetRepository<DBContext.Notification>().Add(new DBContext.Notification()
                {
                    Content = Content,
                    Datetime = DateTime.Now,
                    FromUserId = UserId,
                    IsFromSupervisee = true,
                    NotificationID = MessageID,
                    Source = Station,
                    Subject = Subject,
                    ToUserId = DutyOfficerID,
                    Type = notificationType.ToString()
                });
                return _localUnitOfWork.Save();
            }
            else
            {
                _centralizedUnitOfWork.GetRepository<DBContext.Notification>().Add(new DBContext.Notification()
                {
                    Content = Content,
                    Datetime = DateTime.Now,
                    FromUserId = UserId,
                    IsFromSupervisee = true,
                    NotificationID = MessageID,
                    Source = Station,
                    Subject = Subject,
                    ToUserId = DutyOfficerID,
                    Type = notificationType.ToString()
                });
                return _centralizedUnitOfWork.Save();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="DutyOfficerID"></param>
        /// <param name="Subject"></param>
        /// <param name="Content"></param>
        /// <param name="notificationType"></param>
        /// <param name="Station"></param
        public List<BE.Notification> SendAllDutyOfficer(string UserId, string Subject, string Content, string notificationType, string Station)
        {
            
            if (EnumAppConfig.IsLocal)
            {
                return null;
            }
            else
            {
                List<string> DOId = _centralizedUnitOfWork.DataContext.Membership_UserRoles.Include("Membership_Roles").Where(d => d.Membership_Roles.Name == EnumUserRoles.DutyOfficer).Select(d => d.UserId).ToList();
                List<DBContext.Notification> arrayInsert = DOId.Select(d => new DBContext.Notification()
                {
                    Content = Content,
                    Datetime = DateTime.Now,
                    FromUserId = UserId,
                    IsFromSupervisee = true,
                    NotificationID = Guid.NewGuid().ToString().Trim(),
                    Source = Station,
                    Subject = Subject,
                    ToUserId = d,
                    Type = notificationType
                }).ToList();
                _centralizedUnitOfWork.GetRepository<DBContext.Notification>().AddRange(arrayInsert);
                _centralizedUnitOfWork.Save();

                return arrayInsert.Select(item => item.Map<BE.Notification>()).ToList();
            }
        }
        public void SendAllDutyOfficer(List<BE.Notification> arrayInsert)
        {

            if (EnumAppConfig.IsLocal)
            {
                _localUnitOfWork.GetRepository<DBContext.Notification>().AddRange(arrayInsert.Select(item => item.Map<DBContext.Notification>()).ToList());
                _localUnitOfWork.Save();
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

        public List<Notification> GetMyNotifications(string userId)
        {
            if (EnumAppConfig.IsLocal)
            {
                bool statusCentralized;
                List<Notification> arrayNoti = CallCentralized.Get<List<Notification>>("Notification", "GetMyNotifications",out statusCentralized, "userId="+ userId);
                if(arrayNoti==null || !statusCentralized)
                {
                    arrayNoti = _localUnitOfWork.DataContext.Notifications.Where(d => (!string.IsNullOrEmpty(d.FromUserId) && d.FromUserId == userId) || (!string.IsNullOrEmpty(d.ToUserId) && d.ToUserId == userId)).Select(item => new Notification()
                    {
                        Content = item.Content,
                        Datetime = item.Datetime,
                        FromUserId = item.FromUserId,
                        NotificationID = item.NotificationID,
                        IsRead = item.IsRead.HasValue ? item.IsRead.Value : false,
                        Subject = item.Subject,
                        ToUserId = item.ToUserId
                    }).ToList();
                }
                return arrayNoti;
            }
            else
            {
                return _centralizedUnitOfWork.DataContext.Notifications.Where(d => (!string.IsNullOrEmpty(d.FromUserId) && d.FromUserId == userId) || (!string.IsNullOrEmpty(d.ToUserId) && d.ToUserId == userId)).Select(item => new Notification()
                {
                    Content = item.Content,
                    Datetime = item.Datetime,
                    FromUserId = item.FromUserId,
                    NotificationID = item.NotificationID,
                    IsRead = item.IsRead.HasValue ? item.IsRead.Value : false,
                    Subject = item.Subject,
                    ToUserId = item.ToUserId
                }).ToList();
            }
        }

        public List<Notification> GetNotificationsByUserId(string userId)
        {
            try
            {
                IQueryable<DBContext.Notification> queryNotifications = null;
                if (EnumAppConfig.IsLocal)
                {
                    var data = _localUnitOfWork.DataContext.Notifications.Where(n => n.ToUserId == userId);
                    if (data != null)
                    {
                        queryNotifications = data;
                        int count = queryNotifications.Count();
                        if (count > 0)
                        {
                            List<Notification> notifications = GetListNotifications(queryNotifications);

                            return notifications;
                        }
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<List<Notification>>(EnumAPIParam.Notification, "GetByUserId", out centralizeStatus, "userId=" + userId);
                        if (centralData != null)
                        {
                            return centralData;
                        }

                    }
                }
                else
                {
                    queryNotifications = _centralizedUnitOfWork.DataContext.Notifications.Where(n => n.ToUserId == userId);
                    int count = queryNotifications.Count();
                    if (count > 0)
                    {
                        List<Notification> notifications = GetListNotifications(queryNotifications);

                        return notifications;
                    }

                }
                return null;


            }
            catch (Exception)
            {

                return null;
            }

        }

        private static List<Notification> GetListNotifications(IQueryable<DBContext.Notification> queryNotifications)
        {
            List<Notification> notifications = new List<Notification>();
            foreach (var item in queryNotifications)
            {
                Notification notification = new Notification()
                {
                    Content = item.Content,
                    Datetime = item.Datetime,
                    FromUserId = item.FromUserId,
                    NotificationID = item.NotificationID,
                    IsRead = item.IsRead.HasValue ? item.IsRead.Value : false,
                    Subject = item.Subject,
                    ToUserId = item.ToUserId
                };
                notifications.Add(notification);
            }

            return notifications;
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
                                          Datetime = n.Datetime,
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
                                          Datetime = n.Datetime,
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
                                          Datetime =
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
                                          Datetime = n.Datetime,
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

        public void InsertNotification(string NotificationID, string subject, string content, string fromUserId,
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
                NotificationID = NotificationID
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
