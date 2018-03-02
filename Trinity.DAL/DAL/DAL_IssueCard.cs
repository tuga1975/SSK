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
            array = _localUnitOfWork.DataContext.IssuedCards.Where(d => d.UserId == UserId).OrderByDescending(d => d.CreatedDate).ToList().Select(d => d.Map<BE.IssueCard>()).ToList();
            if (array == null)
                array = new List<BE.IssueCard>();
            return array;
        }

        public BE.IssueCard GetIssueCardBySmartCardId(string SmartCardId)
        {
            BE.IssueCard issueCard = null;
            issueCard = _localUnitOfWork.DataContext.IssuedCards.FirstOrDefault(d => d.SmartCardId == SmartCardId).Map<BE.IssueCard>();
            return issueCard;
        }

        public void Insert(Trinity.BE.IssueCard model)
        {
            BE.IssueCard issueCard = GetIssueCardBySmartCardId(model.SmartCardId);
            if (issueCard!=null)
            {
                throw new Exception(EnumMessage.SmartCardIsAlreadyInUse);
            }
            else
            {
                UpdateStatusByUserId(model.UserId, EnumIssuedCards.Inactive);
                _localUnitOfWork.GetRepository<DBContext.IssuedCard>().Add(model.Map<DBContext.IssuedCard>());
                _localUnitOfWork.Save();
            }
        }

        public void UpdateStatusByUserId(string userId, string Status)
        {
            _localUnitOfWork.DataContext.Database.ExecuteSqlCommand("Update IssuedCards set Status='" + Status + "' where UserId='" + userId + "'");
        }
        #endregion
    }
}
