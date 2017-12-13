using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                dbUser = _localUnitOfWork.DataContext.Users.FirstOrDefault(u => u.SmartCard_Id == smartCardId);
            }
            else
            {
                dbUser = _centralizedUnitOfWork.DataContext.Users.FirstOrDefault(u => u.SmartCard_Id == smartCardId);
            }
            if (dbUser != null)
            {
                Trinity.BE.User user = new BE.User()
                {
                    EnrolledDate = dbUser.EnrolledDate,
                    FingerprintFailedCount = dbUser.FingerprintFailedCount,
                    LastLoginTime = dbUser.LastLoginTime,
                    Name = dbUser.Name,
                    NRIC = dbUser.NRIC,
                    SmartCardFailedCount = dbUser.SmartCardFailedCount,
                    SmartCard_Id = dbUser.SmartCard_Id,
                    Type = dbUser.Type,
                    UserId = dbUser.UserId
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
                    FingerprintFailedCount = dbUser.FingerprintFailedCount,
                    LastLoginTime = dbUser.LastLoginTime,
                    Name = dbUser.Name,
                    NRIC = dbUser.NRIC,
                    SmartCardFailedCount = dbUser.SmartCardFailedCount,
                    SmartCard_Id = dbUser.SmartCard_Id,
                    Type = dbUser.Type,
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
                dbUser.FingerprintFailedCount = model.FingerprintFailedCount;
                dbUser.LastLoginTime = model.LastLoginTime;
                dbUser.Name = model.Name;
                dbUser.NRIC = model.NRIC;
                dbUser.SmartCardFailedCount = model.SmartCardFailedCount;
                dbUser.SmartCard_Id = model.SmartCard_Id;
                dbUser.Type = model.Type;
                dbUser.UserId = model.UserId;

            }
        }
    }
}
