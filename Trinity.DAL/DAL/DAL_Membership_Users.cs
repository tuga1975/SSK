using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;
using Trinity.Identity;

namespace Trinity.DAL
{
    public class DAL_Membership_Users
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        public void UpdateFingerprint(string userId, byte[] left, byte[] right)
        {
            Membership_Users user = this._localUnitOfWork.DataContext.Membership_Users.FirstOrDefault(d => d.UserId == userId);
            if (left!=null && left.Length > 0)
                user.LeftThumbFingerprint = left;
            if (right!=null && right.Length > 0)
                user.RightThumbFingerprint = right;
            if ((left != null && left.Length > 0) || (right != null && right.Length > 0))
            {
                this._localUnitOfWork.GetRepository<Membership_Users>().Update(user);
                this._localUnitOfWork.Save();
            }
        }
        public Trinity.BE.Membership_Users GetByUserId(string UserId)
        {
            try
            {
                if (EnumAppConfig.IsLocal)
                {

                    var data = _localUnitOfWork.DataContext.Membership_Users.FirstOrDefault(d => d.UserId == UserId).Map<Trinity.BE.Membership_Users>();
                    if (data != null)
                    {
                        return data;
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Get<BE.Membership_Users>(EnumAPIParam.User, "GetMembershipByUserId", out centralizeStatus, "userId" + UserId);
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                    }
                }
                else
                {
                    return _centralizedUnitOfWork.DataContext.Membership_Users.FirstOrDefault(d => d.UserId == UserId).Map<Trinity.BE.Membership_Users>();
                }
                return null;
            }
            catch (Exception)
            {

                return null;
            }
        }
        public void UpdateSmartCardId(string UserId, string SmartCardId)
        {
            this._localUnitOfWork.DataContext.Database.ExecuteSqlCommand("Update Membership_Users set SmartCardId='"+ SmartCardId + "' where UserId='"+ UserId + "'");
        }
    }
}
