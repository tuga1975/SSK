using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;
using Trinity.Identity;

namespace Trinity.DAL
{
    public class DAL_User
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        public List<Trinity.BE.User> GetAllSupervisees(bool isLocal)
        {
            if (isLocal)
            {
                var user = (from mu in _localUnitOfWork.DataContext.Membership_Users
                            join mur in _localUnitOfWork.DataContext.Membership_UserRoles on mu.UserId equals mur.UserId
                            join mr in _localUnitOfWork.DataContext.Membership_Roles on mur.RoleId equals mr.Id
                            where mr.Name == "Supervisee"
                            select new Trinity.BE.User() { UserId = mu.UserId, Status = mu.Status, SmartCardId = mu.SmartCardId, RightThumbFingerprint = mu.RightThumbFingerprint, LeftThumbFingerprint = mu.LeftThumbFingerprint, Name = mu.Name, NRIC = mu.NRIC, Role = mr.Name, IsFirstAttempt = mu.IsFirstAttempt });
                return user.ToList();
            }
            else
            {
                var user = (from mu in _centralizedUnitOfWork.DataContext.Membership_Users
                            join mur in _centralizedUnitOfWork.DataContext.Membership_UserRoles on mu.UserId equals mur.UserId
                            join mr in _centralizedUnitOfWork.DataContext.Membership_Roles on mur.RoleId equals mr.Id
                            where mr.Name == "Supervisee"
                            select new Trinity.BE.User() { UserId = mu.UserId, Status = mu.Status, SmartCardId = mu.SmartCardId, RightThumbFingerprint = mu.RightThumbFingerprint, LeftThumbFingerprint = mu.LeftThumbFingerprint, Name = mu.Name, NRIC = mu.NRIC, Role = mr.Name, IsFirstAttempt = mu.IsFirstAttempt });
                return user.ToList();
            }
        }

        public Trinity.BE.User GetUserBySmartCardId(string smartCardId, bool isLocal)
        {
            if (isLocal)
            {
                var user = (from mu in _localUnitOfWork.DataContext.Membership_Users
                            join mur in _localUnitOfWork.DataContext.Membership_UserRoles on mu.UserId equals mur.UserId
                            join mr in _localUnitOfWork.DataContext.Membership_Roles on mur.RoleId equals mr.Id
                            where mu.SmartCardId == smartCardId
                            select new Trinity.BE.User() { UserId = mu.UserId, Status = mu.Status, SmartCardId = mu.SmartCardId, RightThumbFingerprint = mu.RightThumbFingerprint, LeftThumbFingerprint = mu.LeftThumbFingerprint, Name = mu.Name, NRIC = mu.NRIC, Role = mr.Name, IsFirstAttempt = mu.IsFirstAttempt, User_Photo1=mu.User_Profiles.User_Photo1, User_Photo2 = mu.User_Profiles.User_Photo2 });
                return user.FirstOrDefault();
            }
            else
            {
                var user = (from mu in _centralizedUnitOfWork.DataContext.Membership_Users
                            join mur in _centralizedUnitOfWork.DataContext.Membership_UserRoles on mu.UserId equals mur.UserId
                            join mr in _centralizedUnitOfWork.DataContext.Membership_Roles on mur.RoleId equals mr.Id
                            where mu.SmartCardId == smartCardId
                            select new Trinity.BE.User() { UserId = mu.UserId, Status = mu.Status, SmartCardId = mu.SmartCardId, RightThumbFingerprint = mu.RightThumbFingerprint, LeftThumbFingerprint = mu.LeftThumbFingerprint, Name = mu.Name, NRIC = mu.NRIC, Role = mr.Name, IsFirstAttempt = mu.IsFirstAttempt, AccessFailedCount = mu.AccessFailedCount, User_Photo1 = mu.User_Profiles.User_Photo1, User_Photo2 = mu.User_Profiles.User_Photo2 });
                return user.FirstOrDefault();
            }
        }

        public Trinity.BE.User GetUserByUserId(string userId, bool isLocal)
        {
            if (isLocal)
            {
                var user = (from mu in _localUnitOfWork.DataContext.Membership_Users
                            join mur in _localUnitOfWork.DataContext.Membership_UserRoles on mu.UserId equals mur.UserId
                            join mr in _localUnitOfWork.DataContext.Membership_Roles on mur.RoleId equals mr.Id
                            where mu.UserId == userId
                            select new Trinity.BE.User() { UserId = mu.UserId, Status = mu.Status, SmartCardId = mu.SmartCardId, RightThumbFingerprint = mu.RightThumbFingerprint, LeftThumbFingerprint = mu.LeftThumbFingerprint, Name = mu.Name, NRIC = mu.NRIC, Role = mr.Name, IsFirstAttempt = mu.IsFirstAttempt, AccessFailedCount = mu.AccessFailedCount, User_Photo1 = mu.User_Profiles.User_Photo1, User_Photo2 = mu.User_Profiles.User_Photo2 });
                return user.FirstOrDefault();
            }
            else
            {
                var user = (from mu in _centralizedUnitOfWork.DataContext.Membership_Users
                            join mur in _centralizedUnitOfWork.DataContext.Membership_UserRoles on mu.UserId equals mur.UserId
                            join mr in _centralizedUnitOfWork.DataContext.Membership_Roles on mur.RoleId equals mr.Id
                            where mu.UserId == userId
                            select new Trinity.BE.User() { UserId = mu.UserId, Status = mu.Status, SmartCardId = mu.SmartCardId, RightThumbFingerprint = mu.RightThumbFingerprint, LeftThumbFingerprint = mu.LeftThumbFingerprint, Name = mu.Name, NRIC = mu.NRIC, Role = mr.Name, IsFirstAttempt = mu.IsFirstAttempt });
                return user.FirstOrDefault();
            }
        }

        public Trinity.BE.User GetSuperviseeByNRIC(string nric, bool isLocal)
        {
            if (isLocal)
            {
                var user = (from mu in _localUnitOfWork.DataContext.Membership_Users
                            join mur in _localUnitOfWork.DataContext.Membership_UserRoles on mu.UserId equals mur.UserId
                            join mr in _localUnitOfWork.DataContext.Membership_Roles on mur.RoleId equals mr.Id
                            where mu.NRIC == nric
                            select new Trinity.BE.User() { UserId = mu.UserId, Status = mu.Status, SmartCardId = mu.SmartCardId, RightThumbFingerprint = mu.RightThumbFingerprint, LeftThumbFingerprint = mu.LeftThumbFingerprint, Name = mu.Name, NRIC = mu.NRIC, Role = mr.Name, IsFirstAttempt = mu.IsFirstAttempt });
                return user.FirstOrDefault();
            }
            else
            {
                var user = (from mu in _centralizedUnitOfWork.DataContext.Membership_Users
                            join mur in _centralizedUnitOfWork.DataContext.Membership_UserRoles on mu.UserId equals mur.UserId
                            join mr in _centralizedUnitOfWork.DataContext.Membership_Roles on mur.RoleId equals mr.Id
                            where mu.NRIC == nric
                            select new Trinity.BE.User() { UserId = mu.UserId, Status = mu.Status, SmartCardId = mu.SmartCardId, RightThumbFingerprint = mu.RightThumbFingerprint, LeftThumbFingerprint = mu.LeftThumbFingerprint, Name = mu.Name, NRIC = mu.NRIC, Role = mr.Name, IsFirstAttempt = mu.IsFirstAttempt });
                return user.FirstOrDefault();
            }
        }
        public bool UpdateUser(BE.User model, string userId, bool isLocal)
        {
            try
            {
                if (isLocal)
                {
                    UpdateLocal(model, userId);
                    UpdateCentral(model, userId);
                    return true;
                }
                else
                {
                    UpdateCentral(model, userId);
                    return true;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private Membership_Users UpdateCentral(BE.User model, string userId)
        {
            Membership_Users dbUser;
            var centralUserRepo = _centralizedUnitOfWork.GetRepository<Membership_Users>();
            dbUser = centralUserRepo.GetById(userId);
            SetInfo(dbUser, model);
            centralUserRepo.Update(dbUser);
            _centralizedUnitOfWork.Save();
            return dbUser;
        }

        private Membership_Users UpdateLocal(BE.User model, string userId)
        {
            Membership_Users dbUser;
            var localUserRepo = _localUnitOfWork.GetRepository<Membership_Users>();
            dbUser = localUserRepo.GetById(userId);
            SetInfo(dbUser, model);
            localUserRepo.Update(dbUser);
            _localUnitOfWork.Save();
            return dbUser;
        }

        protected void SetInfo(Membership_Users dbUser, BE.User model)
        {
            if (dbUser != null)
            {
                dbUser.Name = model.Name;
                dbUser.NRIC = model.NRIC;
                dbUser.SmartCardId = model.SmartCardId;
                //dbUser.Role = model.Role;
                dbUser.UserId = model.UserId;
                //if (model.RightThumbFingerprint != null)
                //{
                //    byte[] data = new byte[model.RightThumbFingerprint.Length];
                //    for (int i = 0; i < data.Length; i++)
                //    {
                //        data[i] = model.RightThumbFingerprint[i];
                //    }
                //    dbUser.RightThumbFingerprint = data;
                //}
                //if (model.LeftThumbFingerprint != null)
                //{
                //    byte[] data = new byte[model.LeftThumbFingerprint.Length];
                //    for (int i = 0; i < data.Length; i++)
                //    {
                //        data[i] = model.LeftThumbFingerprint[i];
                //    }
                //    dbUser.LeftThumbFingerprint = data;
                //}
                dbUser.RightThumbFingerprint = model.RightThumbFingerprint;
                dbUser.LeftThumbFingerprint = model.LeftThumbFingerprint;
                dbUser.Status = model.Status;
                dbUser.IsFirstAttempt = model.IsFirstAttempt;
            }
        }

        //public bool CreateUser(BE.User user, bool isLocal)
        //{
        //    try
        //    {
        //        Membership_Users dbUser = null;
        //        if (isLocal)
        //        {
        //            var localUserRepo = _localUnitOfWork.GetRepository<Membership_Users>();
        //            dbUser = new Membership_Users();
        //            SetInfo(dbUser, user);
        //            localUserRepo.Add(dbUser);
        //            _localUnitOfWork.Save();
        //        }
        //        else
        //        {
        //            var centralUserRepo = _centralizedUnitOfWork.GetRepository<Membership_Users>();
        //            dbUser = new Membership_Users();
        //            SetInfo(dbUser, user);
        //            centralUserRepo.Add(dbUser);
        //            _centralizedUnitOfWork.Save();
        //        }

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        return false;
        //    }
        //}

        public void ChangeUserStatus(string userId, string status)
        {
            var localUserRepo = _localUnitOfWork.GetRepository<Membership_Users>();
            var centralUserRepo = _centralizedUnitOfWork.GetRepository<Membership_Users>();
            UpdateStatus(userId, status, localUserRepo);
            UpdateStatus(userId, status, centralUserRepo);
            _localUnitOfWork.Save();
            _centralizedUnitOfWork.Save();
        }

        private void UpdateStatus(string userId, string status, IRepository<Membership_Users> userRepo)
        {
            try
            {
                var dbUser = userRepo.GetById(userId);
                if (dbUser != null)
                {
                    dbUser.Status = status;
                    userRepo.Update(dbUser);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void UpdateFingerprint(string userId,byte[] left, byte[] right)
        {
            Membership_Users user = this._localUnitOfWork.DataContext.Membership_Users.FirstOrDefault(d=>d.UserId== userId);
            if (left.Length > 0)
                user.LeftThumbFingerprint = left;
            if (right.Length > 0)
                user.RightThumbFingerprint = right;
            if (left.Length > 0 || right.Length > 0)
            {
                this._localUnitOfWork.GetRepository<Membership_Users>().Update(user);
                this._localUnitOfWork.Save();
            }
        }

        public void ChangeAccessFailedCount(string userId, int count)
        {
            var localUserRepo = _localUnitOfWork.GetRepository<Membership_Users>();
            var centralUserRepo = _centralizedUnitOfWork.GetRepository<Membership_Users>();
            UpdateAccessFailedCount(userId, count, localUserRepo);
            UpdateAccessFailedCount(userId, count, centralUserRepo);
            _localUnitOfWork.Save();
            _centralizedUnitOfWork.Save();
        }

        private void UpdateAccessFailedCount(string userId, int count, IRepository<Membership_Users> userRepo)
        {
            try
            {
                var dbUser = userRepo.GetById(userId);
                if (dbUser != null)
                {
                    dbUser.AccessFailedCount = count;
                    userRepo.Update(dbUser);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
