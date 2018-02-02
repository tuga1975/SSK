using System;
using System.Collections.Generic;
using System.Data.Entity;
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


        public DBContext.Label GetByDateAndUserId(DateTime Date,string UserId)
        {
            return _localUnitOfWork.DataContext.Labels.FirstOrDefault(d=> DbFunctions.TruncateTime(d.Date).Value==Date && d.UserId==UserId);
        }

        public Label GetLabelByUserIdAndType(string userId, string labelType)
        {
            return _localUnitOfWork.DataContext.Labels.FirstOrDefault(d => d.UserId == userId && d.Label_Type.Equals(labelType));
        }

        public bool UpdateLabel(BE.Label model, string userId, string labelType)
        {
            try
            {
                Label dbLabel = null;
                var centralizeLabelRepo = _centralizedUnitOfWork.GetRepository<Label>();
                dbLabel = _centralizedUnitOfWork.DataContext.Labels.FirstOrDefault(d => d.UserId == userId && d.Label_Type.Equals(labelType));
                if (dbLabel == null)
                {
                    dbLabel = new Label();
                    dbLabel.Label_ID = Guid.NewGuid();
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
                    dbLabel.PrintStatus = model.PrintStatus;
                    dbLabel.Message = model.Message;

                    centralizeLabelRepo.Add(dbLabel);
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
                    dbLabel.PrintStatus = model.PrintStatus;
                    dbLabel.Message = model.Message;

                    centralizeLabelRepo.Update(dbLabel);
                }

                if (_centralizedUnitOfWork.Save() > 0)
                {

                    var locallabelRepo = _localUnitOfWork.GetRepository<Label>();
                    dbLabel = GetLabelByUserIdAndType(userId, labelType);
                    if (dbLabel == null)
                    {
                        dbLabel = new Label();
                        dbLabel.Label_ID = Guid.NewGuid();
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
                        dbLabel.PrintStatus = model.PrintStatus;
                        dbLabel.Message = model.Message;

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
                        dbLabel.PrintStatus = model.PrintStatus;
                        dbLabel.Message = model.Message;

                        locallabelRepo.Update(dbLabel);
                    }

                    _localUnitOfWork.Save();
                }
                return true;
            }
            catch(Exception e)
            {
                return false;
            }
        }

        public List<BE.Label> GetAllLabelsForMUBAndTT()
        {
            try
            {
                var lstModels = _localUnitOfWork.DataContext.Labels.Include("Membership_Users")
                    .Where(l=>l.Label_Type.Equals(EnumLabelType.MUB) || l.Label_Type.Equals(EnumLabelType.TT))
                    .Select(d => new BE.Label()
                {
                    NRIC = d.Membership_Users.NRIC,
                    Name = d.Membership_Users.Name,
                    LastStation = d.LastStation,
                    UserId = d.UserId
                });
                //var lstModels = from a in _localUnitOfWork.DataContext.Labels
                //                join u in _localUnitOfWork.DataContext.Membership_Users on a.UserId equals u.UserId
                //                select new BE.Label()
                //                {
                //                    NRIC = u.NRIC,
                //                    Name = u.Name,
                //                    LastStation = a.LastStation
                //                };

                return lstModels.ToList();
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public List<BE.Label> GetAllLabelsForUB()
        {
            try
            {
                var lstModels = _localUnitOfWork.DataContext.Labels.Include("Membership_Users")
                    .Where(l => l.Label_Type.Equals(EnumLabelType.UB))
                    .Select(d => new BE.Label()
                    {
                        NRIC = d.Membership_Users.NRIC,
                        Name = d.Membership_Users.Name,
                        LastStation = d.LastStation,
                        UserId = d.UserId
                    });

                return lstModels.ToList();
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
