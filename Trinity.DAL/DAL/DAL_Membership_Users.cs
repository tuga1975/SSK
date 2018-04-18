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


        #region 2018
        public void UpdateSmartCardId(string UserId, string SmartCardId)
        {
            if (EnumAppConfig.IsLocal)
            {
                bool statusCentralized;
                CallCentralized.Post("User", "UpdateSmartCardId", out statusCentralized, "UserId="+ UserId, "SmartCardId="+ SmartCardId);
                if (!statusCentralized)
                {
                    throw new Trinity.Common.ExceptionArgs(EnumMessage.NotConnectCentralized);
                }
                else
                {
                    this._localUnitOfWork.DataContext.Database.ExecuteSqlCommand("Update Membership_Users set SmartCardId='" + SmartCardId + "' where UserId='" + UserId + "'");
                }


            }
            else
            {
                this._centralizedUnitOfWork.DataContext.Database.ExecuteSqlCommand("Update Membership_Users set SmartCardId='" + SmartCardId + "' where UserId='" + UserId + "'");
            }

        }
        public void UpdateFingerprint(string userId, byte[] left, byte[] right)
        {
            if (EnumAppConfig.IsLocal)
            {
                bool statusCentralized;
                CallCentralized.Post<bool>("User", "UpdateFingerprint", out statusCentralized, new object[] { userId , left, right });
                if (!statusCentralized)
                {
                    throw new Trinity.Common.ExceptionArgs(EnumMessage.NotConnectCentralized);
                }
                else
                {
                    Membership_Users user = this._localUnitOfWork.DataContext.Membership_Users.FirstOrDefault(d => d.UserId == userId);
                    if (left != null && left.Length > 0)
                        user.LeftThumbFingerprint = left;
                    if (right != null && right.Length > 0)
                        user.RightThumbFingerprint = right;
                    if ((left != null && left.Length > 0) || (right != null && right.Length > 0))
                    {
                        this._localUnitOfWork.GetRepository<Membership_Users>().Update(user);
                        this._localUnitOfWork.Save();
                    }
                }

                
            }
            else
            {
                Membership_Users user = this._centralizedUnitOfWork.DataContext.Membership_Users.FirstOrDefault(d => d.UserId == userId);
                if (left != null && left.Length > 0)
                    user.LeftThumbFingerprint = left;
                if (right != null && right.Length > 0)
                    user.RightThumbFingerprint = right;
                if ((left != null && left.Length > 0) || (right != null && right.Length > 0))
                {
                    this._centralizedUnitOfWork.GetRepository<Membership_Users>().Update(user);
                    this._centralizedUnitOfWork.Save();
                }
            }
            
        }
        public Trinity.BE.Membership_Users GetByUserId(string UserId)
        {
            if (EnumAppConfig.IsLocal)
            {

                Trinity.BE.Membership_Users data = _localUnitOfWork.DataContext.Membership_Users.FirstOrDefault(d => d.UserId == UserId).Map<Trinity.BE.Membership_Users>();
                if (data == null)
                {
                    data = CallCentralized.Get<BE.Membership_Users>(EnumAPIParam.User, "GetMembershipByUserId", "userId" + UserId);
                }
                return data;
            }
            else
            {
                return _centralizedUnitOfWork.DataContext.Membership_Users.FirstOrDefault(d => d.UserId == UserId).Map<Trinity.BE.Membership_Users>();
            }
        }
        #endregion

        
       
        
    }
}
