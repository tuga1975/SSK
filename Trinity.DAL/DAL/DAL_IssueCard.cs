using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.DAL.Repository;

namespace Trinity.DAL
{
    public class DAL_IssueCard
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();
        #region 2018
        public List<BE.IssueCard> GetMyIssueCards(string UserId)
        {
            List<BE.IssueCard> array;
            if (EnumAppConfig.IsLocal)
            {
                array = _localUnitOfWork.DataContext.IssuedCards.Where(d => d.UserId == UserId).OrderByDescending(d => d.CreatedDate).ToList().Select(d => d.Map<BE.IssueCard>()).ToList();
                if(array==null || array.Count == 0)
                {
                    array = CallCentralized.Get<List<BE.IssueCard>>("IssueCard", "GetMyIssueCards", "UserId="+ UserId);
                    if (array == null)
                        array = new List<BE.IssueCard>();
                }
            }
            else
            {
                array = _centralizedUnitOfWork.DataContext.IssuedCards.Where(d => d.UserId == UserId).OrderByDescending(d => d.CreatedDate).ToList().Select(d => d.Map<BE.IssueCard>()).ToList();
            }
            return array;
        }

        public BE.IssueCard GetIssueCardBySmartCardId(string SmartCardId)
        {
            BE.IssueCard issueCard = null;
            if (EnumAppConfig.IsLocal)
            {
                issueCard = _localUnitOfWork.DataContext.IssuedCards.FirstOrDefault(d => d.SmartCardId == SmartCardId).Map<BE.IssueCard>();
                if (issueCard == null)
                {
                    issueCard = CallCentralized.Get<BE.IssueCard>("IssueCard", "GetIssueCardBySmartCardId", "SmartCardId="+ SmartCardId);
                }
            }
            else
            {
                issueCard =  _centralizedUnitOfWork.DataContext.IssuedCards.FirstOrDefault(d => d.SmartCardId == SmartCardId).Map<BE.IssueCard>();
            }
            return issueCard;
        }
        public void Insert(Trinity.BE.IssueCard model)
        {
            if (EnumAppConfig.IsLocal)
            {
                BE.IssueCard issueCard = GetIssueCardBySmartCardId(model.SmartCardId);
                if (issueCard!=null)
                {
                    throw new Exception(EnumMessage.SmartCardIsAlreadyInUse);
                }
                else
                {
                    UpdateStatusByUserId(model.UserId, EnumIssuedCards.Inactive);

                    bool statusCentralized;
                    CallCentralized.Post("IssueCard", "Insert", out statusCentralized, model);
                    if (!statusCentralized)
                    {
                        throw new Exception(EnumMessage.NotConnectCentralized);
                    }
                    else
                    {
                        _localUnitOfWork.GetRepository<DBContext.IssuedCard>().Add(model.Map<DBContext.IssuedCard>());
                        _localUnitOfWork.Save();
                    }
                }
            }
            else
            {
                _centralizedUnitOfWork.GetRepository<DBContext.IssuedCard>().Add(model.Map<DBContext.IssuedCard>());
                _centralizedUnitOfWork.Save();
            }
            
        }
        public void UpdateStatusByUserId(string userId, string Status)
        {
            if (EnumAppConfig.IsLocal)
            {
                bool statusCentralized;
                CallCentralized.Post("IssueCard", "UpdateStatusByUserId", out statusCentralized, "userId="+ userId, "Status="+ Status);
                if (!statusCentralized)
                {
                    throw new Exception(EnumMessage.NotConnectCentralized);
                }
                else
                {
                    _localUnitOfWork.DataContext.Database.ExecuteSqlCommand("Update IssuedCards set Status='" + Status + "' where UserId='" + userId + "'");
                }
            }
            else
            {
                _centralizedUnitOfWork.DataContext.Database.ExecuteSqlCommand("Update IssuedCards set Status='" + Status + "' where UserId='" + userId + "'");
            }
            
        }
        #endregion
    }
}
