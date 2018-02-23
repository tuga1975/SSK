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


        public DBContext.Label GetByDateAndUserId(DateTime Date, string UserId)
        {
            if (EnumAppConfig.IsLocal)
            {
                DBContext.Label label = _localUnitOfWork.DataContext.Labels.FirstOrDefault(d => DbFunctions.TruncateTime(d.Date).Value == Date && d.UserId == UserId);
                if (label != null || EnumAppConfig.ByPassCentralizedDB)
                {
                    return label;
                }
                else
                {
                    bool centralizeStatus;
                    var centralUpdate = CallCentralized.Get<Label>(EnumAPIParam.Label, "GetByDateAndUserId", out centralizeStatus, "date=" + Date.ToString(), "UserId=" + UserId);
                    if (centralizeStatus)
                    {
                        return centralUpdate;
                    }
                    return null;
                }
            }
            else
            {
                return _centralizedUnitOfWork.DataContext.Labels.FirstOrDefault(d => DbFunctions.TruncateTime(d.Date).Value == Date && d.UserId == UserId);
            }
        }

        public Label UpdateLabel(BE.Label model)
        {
            try
            {
                Label dbLabel = null;
                Guid labelID_NewGuid = Guid.NewGuid();
                var queueID = new DAL_QueueNumber().GetMyQueueToday(model.UserId).Queue_ID;
                if (EnumAppConfig.IsLocal)
                {
                    var locallabelRepo = _localUnitOfWork.GetRepository<Label>();
                    dbLabel = _localUnitOfWork.DataContext.Labels.FirstOrDefault(d => d.UserId == model.UserId && d.Label_Type.Equals(model.Label_Type));
                    if (dbLabel == null)
                    {
                        dbLabel = new Label();
                        dbLabel.Label_ID = labelID_NewGuid;
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

                    if (EnumAppConfig.ByPassCentralizedDB)
                    {
                        return dbLabel;
                    }
                    else
                    {

                        bool centralizeStatus;
                        var centralUpdate = CallCentralized.Post<Label>(EnumAPIParam.Label, EnumAPIParam.UpdateLabel, out centralizeStatus, model);
                        if (centralizeStatus)
                        {                            
                            return centralUpdate;
                        }
                        else
                        {
                            throw new Exception(EnumMessage.NotConnectCentralized);
                        }
                    }
                }
                else
                {
                    var centralizeLabelRepo = _centralizedUnitOfWork.GetRepository<Label>();
                    dbLabel = _centralizedUnitOfWork.DataContext.Labels.FirstOrDefault(d => d.UserId == model.UserId && d.Label_Type.Equals(model.Label_Type));
                    if (dbLabel == null)
                    {
                        dbLabel = new Label();
                        dbLabel.Label_ID = labelID_NewGuid;
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
                    _centralizedUnitOfWork.Save();
                }
                return dbLabel;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public List<BE.Label> GetAllLabelsForMUBAndTT()
        {
            try
            {
                if (EnumAppConfig.IsLocal)
                {
                    var lstModels = _localUnitOfWork.DataContext.Labels.Include("Membership_Users")
                        .Where(l => l.Label_Type.Equals(EnumLabelType.MUB) || l.Label_Type.Equals(EnumLabelType.TT))
                        .Select(d => new BE.Label()
                        {
                            NRIC = d.Membership_Users.NRIC,
                            Name = d.Membership_Users.Name,
                            LastStation = d.LastStation,
                            UserId = d.UserId
                        });

                    if ((lstModels != null && lstModels.Count() > 0) || EnumAppConfig.ByPassCentralizedDB)
                    {
                        return lstModels.ToList();
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralUpdate = CallCentralized.Get<List<BE.Label>>(EnumAPIParam.Label, "GetAllLabelsForMUBAndTT", out centralizeStatus);
                        if (centralizeStatus)
                        {
                            return centralUpdate;
                        }
                        return lstModels.ToList();
                    }
                }
                else
                {
                    var lstModels = _centralizedUnitOfWork.DataContext.Labels.Include("Membership_Users")
                    .Where(l => l.Label_Type.Equals(EnumLabelType.MUB) || l.Label_Type.Equals(EnumLabelType.TT))
                    .Select(d => new BE.Label()
                    {
                        NRIC = d.Membership_Users.NRIC,
                        Name = d.Membership_Users.Name,
                        LastStation = d.LastStation,
                        UserId = d.UserId
                    });

                    return lstModels.ToList();
                }
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
                if (EnumAppConfig.IsLocal)
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

                    if ((lstModels != null && lstModels.Count() > 0) || EnumAppConfig.ByPassCentralizedDB)
                    {
                        return lstModels.ToList();
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralUpdate = CallCentralized.Get<List<BE.Label>>(EnumAPIParam.Label, "GetAllLabelsForUB", out centralizeStatus);
                        if (centralizeStatus)
                        {
                            return centralUpdate;
                        }
                        return lstModels.ToList();
                    }
                }
                else
                {
                    var lstModels = _centralizedUnitOfWork.DataContext.Labels.Include("Membership_Users")
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
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
