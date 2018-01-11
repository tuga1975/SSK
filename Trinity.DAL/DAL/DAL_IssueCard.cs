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
        public void Insert(Trinity.BE.IssueCard model)
        {
            var issueCardRepo = _localUnitOfWork.GetRepository<DBContext.IssuedCard>();
            var dbIssueCard = new DBContext.IssuedCard();
            SetInfo(model, dbIssueCard);

            issueCardRepo.Add(dbIssueCard);
            _localUnitOfWork.Save();
        }

        private static void SetInfo(BE.IssueCard model, DBContext.IssuedCard dbIssueCard)
        {
            dbIssueCard.CreatedBy = model.CreatedBy;
            dbIssueCard.CreatedDate = model.CreatedDate;
            dbIssueCard.Date_Of_Issue = model.Date_Of_Issue;
            dbIssueCard.Name = model.Name;
            dbIssueCard.NRIC = model.NRIC;
            dbIssueCard.Reprint_Reason = model.Reprint_Reason;
            dbIssueCard.Serial_Number = model.Serial_Number;
            dbIssueCard.SmartCardId = model.SmartCardId;
            dbIssueCard.Status = model.Status;
            dbIssueCard.UserId = model.UserId;
        }
    }
}
