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
        public void UpdateStatusByUserId(string userId, string Status)
        {
            _localUnitOfWork.DataContext.Database.ExecuteSqlCommand("Update IssuedCards set Status='" + Status + "' where UserId='" + userId + "'");
        }
        public void Update(string smartCardId, string userId, BE.IssueCard model)
        {
            var issueCardRepo = _localUnitOfWork.GetRepository<DBContext.IssuedCard>();
            var dbIssueCard = issueCardRepo.Get(u => u.UserId == userId && u.SmartCardId == smartCardId);
            if (dbIssueCard != null)
            {
                SetInfo(model, dbIssueCard);
                issueCardRepo.Update(dbIssueCard);
                _localUnitOfWork.Save();
            }
        }

        public BE.IssueCard GetIssueCardById(string issueCardId)
        {
            var dbIssueCard = _localUnitOfWork.DataContext.IssuedCards.Find(issueCardId);
            return SetInfoForBE(new BE.IssueCard(), dbIssueCard);

        }
        public List<BE.IssueCard> GetMyIssueCard(string UserId)
        {
            var dbIssueCard = _localUnitOfWork.DataContext.IssuedCards.Where(d=>d.UserId==UserId).OrderByDescending(d=>d.CreatedDate);
            return dbIssueCard.ToList().Select(d=> SetInfoForBE(new BE.IssueCard(), d)).ToList();

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

        private static BE.IssueCard SetInfoForBE(BE.IssueCard model, DBContext.IssuedCard dbIssueCard)
        {
            model.CreatedBy = dbIssueCard.CreatedBy;
            model.CreatedDate = dbIssueCard.CreatedDate;
            model.Date_Of_Issue = dbIssueCard.Date_Of_Issue;
            model.Name = dbIssueCard.Name;
            model.NRIC = dbIssueCard.NRIC;
            model.Reprint_Reason = dbIssueCard.Reprint_Reason;
            model.Serial_Number = dbIssueCard.Serial_Number;
            model.SmartCardId = dbIssueCard.SmartCardId;
            model.Status = dbIssueCard.Status;
            model.UserId = dbIssueCard.UserId;

            return model;
        }
    }
}
