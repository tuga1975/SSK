using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;

namespace Trinity.DAL
{
    public class DAL_Labels
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        public Label GetLabelByUserId(string userId)
        {
            return _localUnitOfWork.DataContext.Labels.FirstOrDefault(d => d.UserId == userId);
        }

        public bool UpdateLabel(BE.Label model, string userId)
        {
            try
            {
                Label dbLabel;
                var locallabelRepo = _localUnitOfWork.GetRepository<Label>();
                dbLabel = GetLabelByUserId(userId);
                if (dbLabel == null)
                {
                    dbLabel = new Label();
                    dbLabel.Label_Type = model.Label_Type;
                    dbLabel.CompanyName = model.CompanyName;
                    dbLabel.MarkingNo = model.MarkingNo;
                    dbLabel.DrugType = model.DrugType;
                    dbLabel.UserId = model.UserId;
                    dbLabel.NRIC = model.NRIC;
                    dbLabel.Name = model.Name;
                    dbLabel.Date = model.Date.Value;
                    dbLabel.QRCode = model.QRCode;
                    dbLabel.LastStation = model.LastStation;
                    dbLabel.ReprintReason = model.ReprintReason;
                    dbLabel.PrintCount = 1;

                    locallabelRepo.Add(dbLabel);
                }
                else
                {
                    dbLabel.Label_Type = model.Label_Type;
                    dbLabel.CompanyName = model.CompanyName;
                    dbLabel.MarkingNo = model.MarkingNo;
                    dbLabel.DrugType = model.DrugType;
                    dbLabel.UserId = model.UserId;
                    dbLabel.NRIC = model.NRIC;
                    dbLabel.Name = model.Name;
                    dbLabel.Date = model.Date.Value;
                    dbLabel.QRCode = model.QRCode;
                    dbLabel.LastStation = model.LastStation;
                    dbLabel.PrintCount += 1;
                    dbLabel.ReprintReason = model.ReprintReason;

                    locallabelRepo.Update(dbLabel);
                }
                _localUnitOfWork.Save();

                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

    }
}
