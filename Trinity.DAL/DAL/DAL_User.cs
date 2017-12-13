using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;

namespace Trinity.DAL.DAL
{
    public class DAL_User
    {
        UnitOfWork _unitOfWork = new UnitOfWork();

        public Trinity.BE.User GetUserBySmartCardId(string smartCardId)
        {
            User dbUser = _unitOfWork.DataContext.Users.FirstOrDefault(u => u.SmartCard_Id == smartCardId);
            if (dbUser != null)
            {
                Trinity.BE.User user = new BE.User()
                {
                    DutyOfficer_Id = dbUser.DutyOfficer_Id,
                    EnrolledDate = dbUser.EnrolledDate,
                    FingerprintFailedCount = dbUser.FingerprintFailedCount,
                    LastLoginTime = dbUser.LastLoginTime,
                    Name = dbUser.Name,
                    NRIC = dbUser.NRIC,
                    SmartCardFailedCount = dbUser.SmartCardFailedCount,
                    SmartCard_Id = dbUser.SmartCard_Id,
                    Type = dbUser.Type,
                    UserId = dbUser.UserId,
                    Fingerprint_Template = dbUser.Fingerprint_Template
                };
                return user;
            }
            return null;
        }
    }
}
