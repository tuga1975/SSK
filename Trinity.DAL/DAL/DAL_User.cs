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
    }
}
