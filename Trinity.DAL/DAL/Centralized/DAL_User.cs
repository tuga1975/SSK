using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;

namespace Trinity.DAL.Centralized
{
    public class DAL_User
    {
        Centralized_UnitOfWork _unitOfWork = new Centralized_UnitOfWork();

        public Trinity.BE.User GetUserBySmartCardId(string smartCardId)
        {
            User dbUser = _unitOfWork.DataContext.Users.FirstOrDefault(u => u.SmartCard_Id == smartCardId);
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
