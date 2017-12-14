﻿using System;
using System.Linq;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;

namespace Trinity.DAL
{
    public class DAL_User
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        public Trinity.BE.User GetUserBySmartCardId(string smartCardId, bool isLocal)
        {
            User dbUser = null;
            if (isLocal)
            {
                dbUser = _localUnitOfWork.DataContext.Users.FirstOrDefault(u => u.SmartCardId == smartCardId);
            }
            else
            {
                dbUser = _centralizedUnitOfWork.DataContext.Users.FirstOrDefault(u => u.SmartCardId == smartCardId);
            }
            if (dbUser != null)
            {
                Trinity.BE.User user = new BE.User()
                {
                    EnrolledDate = dbUser.EnrolledDate,
                    LastLoginTime = dbUser.LastLoginTime,
                    Name = dbUser.Name,
                    NRIC = dbUser.NRIC,
                    SmartCardId = dbUser.SmartCardId,
                    Role = dbUser.Role,
                    UserId = dbUser.UserId,
                    Fingerprint = dbUser.Fingerprint
                };
                return user;
            }
            return null;
        }

        public Trinity.BE.User GetUserByUserId(string userId, bool isLocal)
        {
            User dbUser = null;
            if (isLocal)
            {
                dbUser = _localUnitOfWork.DataContext.Users.FirstOrDefault(u => u.UserId == userId);

            }
            else
            {
                dbUser = _centralizedUnitOfWork.DataContext.Users.FirstOrDefault(u => u.UserId == userId);
            }
            if (dbUser != null)
            {
                Trinity.BE.User user = new BE.User()
                {
                    Fingerprint = dbUser.Fingerprint,
                    EnrolledDate = dbUser.EnrolledDate,
                    LastLoginTime = dbUser.LastLoginTime,
                    Name = dbUser.Name,
                    NRIC = dbUser.NRIC,
                    SmartCardId = dbUser.SmartCardId,
                    Role = dbUser.Role,
                    UserId = dbUser.UserId
                };
                return user;
            }
            return null;
        }

        public Trinity.BE.User GetUserByNRIC(string nric, bool isLocal)
        {
            User dbUser = null;
            if (isLocal)
            {
                dbUser = _localUnitOfWork.DataContext.Users.FirstOrDefault(u => u.NRIC == nric);

            }
            else
            {
                dbUser = _centralizedUnitOfWork.DataContext.Users.FirstOrDefault(u => u.NRIC == nric);
            }

            if (dbUser != null)
            {
                Trinity.BE.User user = new BE.User()
                {
                    Fingerprint = dbUser.Fingerprint,
                    EnrolledDate = dbUser.EnrolledDate,
                    LastLoginTime = dbUser.LastLoginTime,
                    Name = dbUser.Name,
                    NRIC = dbUser.NRIC,
                    SmartCardId = dbUser.SmartCardId,
                    Role = dbUser.Role,
                    UserId = dbUser.UserId
                };
                return user;
            }

            return null;
        }
        public bool UpdateUser(BE.User model, string userId, bool isLocal)
        {
            try
            {
                User dbUser = null;
                if (isLocal)
                {
                    var localUserRepo = _localUnitOfWork.GetRepository<User>();
                    dbUser = localUserRepo.GetById(userId);
                    SetInfo(dbUser, model);
                    localUserRepo.Update(dbUser);
                    _localUnitOfWork.Save();
                    return true;
                }
                else
                {
                    var centralUserRepo = _centralizedUnitOfWork.GetRepository<User>();
                    dbUser = centralUserRepo.GetById(userId);
                    SetInfo(dbUser, model);
                    centralUserRepo.Update(dbUser);
                    _centralizedUnitOfWork.Save();
                    return true;
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        protected void SetInfo(User dbUser, BE.User model)
        {
            if (dbUser != null)
            {
                dbUser.Fingerprint = model.Fingerprint;
                dbUser.EnrolledDate = model.EnrolledDate;
                dbUser.LastLoginTime = model.LastLoginTime;
                dbUser.Name = model.Name;
                dbUser.NRIC = model.NRIC;
                dbUser.SmartCardId = model.SmartCardId;
                dbUser.Role = model.Role;
                dbUser.UserId = model.UserId;
                if (model.Fingerprint != null)
                {
                    byte[] data = new byte[model.Fingerprint.Length];
                    for (int i = 0; i < data.Length; i++)
                    {
                        data[i] = model.Fingerprint[i];
                    }
                    dbUser.Fingerprint = data;
                }
            }
        }

        public bool CreateUser(BE.User user, bool isLocal)
        {
            try
            {
                User dbUser = null;
                if (isLocal)
                {
                    var localUserRepo = _localUnitOfWork.GetRepository<User>();
                    dbUser = new User();
                    SetInfo(dbUser, user);
                    localUserRepo.Add(dbUser);
                    _localUnitOfWork.Save();
                }
                else
                {
                    var centralUserRepo = _centralizedUnitOfWork.GetRepository<User>();
                    SetInfo(dbUser, user);
                    centralUserRepo.Add(dbUser);
                    _centralizedUnitOfWork.Save();
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
