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
            try
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
            catch (Exception e)
            {
                return null;
            }
        }

        //public Label UpdateLabel(BE.Label model)
        //{
        //    try
        //    {
        //        Label dbLabel = null;
        //        Guid labelID_NewGuid = Guid.NewGuid();
        //        var queue = new DAL_QueueNumber().GetMyQueueToday(model.UserId);

        //        if (EnumAppConfig.IsLocal)
        //        {
        //            var locallabelRepo = _localUnitOfWork.GetRepository<Label>();
        //            dbLabel = _localUnitOfWork.DataContext.Labels.FirstOrDefault(d => d.UserId == model.UserId && d.Label_Type.Equals(model.Label_Type));
        //            string lastStation = model.LastStation;
        //            //if(model.LastStation == EnumStation.DUTYOFFICER)
        //            //{
        //            //    lastStation = "DO";
        //            //}

        //            if (dbLabel == null)
        //            {
        //                dbLabel = new Label();
        //                dbLabel.Label_ID = labelID_NewGuid;
        //                dbLabel.Label_Type = model.Label_Type;
        //                dbLabel.CompanyName = model.CompanyName;
        //                dbLabel.MarkingNo = model.MarkingNo;
        //                dbLabel.DrugType = model.DrugType;
        //                dbLabel.UserId = model.UserId;
        //                dbLabel.NRIC = model.NRIC;
        //                dbLabel.Name = model.Name;
        //                dbLabel.Date = model.Date.Value;
        //                dbLabel.QRCode = model.QRCode;
        //                dbLabel.LastStation = lastStation;
        //                dbLabel.ReprintReason = model.ReprintReason;
        //                dbLabel.PrintCount = 1;
        //                dbLabel.PrintStatus = model.PrintStatus;
        //                dbLabel.Message = model.Message;
        //                if (queue != null)
        //                {
        //                    dbLabel.Queue_ID = queue.Queue_ID;
        //                }

        //                locallabelRepo.Add(dbLabel);
        //            }
        //            else
        //            {
        //                dbLabel.Label_Type = model.Label_Type;
        //                dbLabel.CompanyName = model.CompanyName;
        //                dbLabel.MarkingNo = model.MarkingNo;
        //                dbLabel.DrugType = model.DrugType;
        //                dbLabel.UserId = model.UserId;
        //                dbLabel.NRIC = model.NRIC;
        //                dbLabel.Name = model.Name;
        //                dbLabel.Date = model.Date.Value;
        //                dbLabel.QRCode = model.QRCode;
        //                dbLabel.LastStation = lastStation;
        //                dbLabel.PrintCount += 1;
        //                dbLabel.ReprintReason = model.ReprintReason;
        //                dbLabel.PrintStatus = model.PrintStatus;
        //                dbLabel.Message = model.Message;
        //                if (queue != null)
        //                {
        //                    dbLabel.Queue_ID = queue.Queue_ID;
        //                }

        //                locallabelRepo.Update(dbLabel);
        //            }

        //            _localUnitOfWork.Save();

        //            return dbLabel;

        //            //if (EnumAppConfig.ByPassCentralizedDB)
        //            //{
        //            //    return dbLabel;
        //            //}
        //            //else
        //            //{

        //            //    bool centralizeStatus;
        //            //    var centralUpdate = CallCentralized.Post<Label>(EnumAPIParam.Label, EnumAPIParam.UpdateLabel, out centralizeStatus, model);
        //            //    if (centralizeStatus)
        //            //    {                            
        //            //        return centralUpdate;
        //            //    }
        //            //    else
        //            //    {
        //            //        throw new Exception(EnumMessage.NotConnectCentralized);
        //            //    }
        //            //}
        //        }
        //        else
        //        {
        //            var centralizeLabelRepo = _centralizedUnitOfWork.GetRepository<Label>();
        //            dbLabel = _centralizedUnitOfWork.DataContext.Labels.FirstOrDefault(d => d.UserId == model.UserId && d.Label_Type.Equals(model.Label_Type));
        //            string lastStation = model.LastStation;
        //            if (model.LastStation == EnumStation.DUTYOFFICER)
        //            {
        //                lastStation = "DO";
        //            }
        //            if (dbLabel == null)
        //            {
        //                dbLabel = new Label();
        //                dbLabel.Label_ID = labelID_NewGuid;
        //                dbLabel.Label_Type = model.Label_Type;
        //                dbLabel.CompanyName = model.CompanyName;
        //                dbLabel.MarkingNo = model.MarkingNo;
        //                dbLabel.DrugType = model.DrugType;
        //                dbLabel.UserId = model.UserId;
        //                dbLabel.NRIC = model.NRIC;
        //                dbLabel.Name = model.Name;
        //                dbLabel.Date = model.Date.Value;
        //                dbLabel.QRCode = model.QRCode;
        //                dbLabel.LastStation = lastStation;
        //                dbLabel.ReprintReason = model.ReprintReason;
        //                dbLabel.PrintCount = 1;
        //                dbLabel.PrintStatus = model.PrintStatus;
        //                dbLabel.Message = model.Message;
        //                if (queue != null)
        //                {
        //                    dbLabel.Queue_ID = queue.Queue_ID;
        //                }

        //                centralizeLabelRepo.Add(dbLabel);
        //            }
        //            else
        //            {
        //                dbLabel.Label_Type = model.Label_Type;
        //                dbLabel.CompanyName = model.CompanyName;
        //                dbLabel.MarkingNo = model.MarkingNo;
        //                dbLabel.DrugType = model.DrugType;
        //                dbLabel.UserId = model.UserId;
        //                dbLabel.NRIC = model.NRIC;
        //                dbLabel.Name = model.Name;
        //                dbLabel.Date = model.Date.Value;
        //                dbLabel.QRCode = model.QRCode;
        //                dbLabel.LastStation = lastStation;
        //                dbLabel.PrintCount += 1;
        //                dbLabel.ReprintReason = model.ReprintReason;
        //                dbLabel.PrintStatus = model.PrintStatus;
        //                dbLabel.Message = model.Message;
        //                if (queue != null)
        //                {
        //                    dbLabel.Queue_ID = queue.Queue_ID;
        //                }

        //                centralizeLabelRepo.Update(dbLabel);
        //            }
        //            _centralizedUnitOfWork.Save();
        //        }
        //        return dbLabel;
        //    }
        //    catch (Exception e)
        //    {
        //        return null;
        //    }
        //}


        public string GetMarkingNumber(string UserId,DateTime date)
        {
            date = date.Date;
            var _label = _localUnitOfWork.DataContext.Labels.FirstOrDefault(d=>d.UserId.Equals(UserId) && DbFunctions.TruncateTime(d.Date)== date);
            if (_label != null)
            {
                return _label.MarkingNo;
            }
            return string.Empty;
        }
        public void DeleteLabel(string UserId, DateTime date,string Type)
        {
            date = date.Date;
            var _label = _localUnitOfWork.DataContext.Labels.FirstOrDefault(d => d.UserId.Equals(UserId) && d.Label_Type.Equals(Type) && DbFunctions.TruncateTime(d.Date) == date);
            if (_label != null)
            {
                _localUnitOfWork.GetRepository<DAL.DBContext.Label>().Delete(_label);
                _localUnitOfWork.Save();
            }
        }
        public void Insert(BE.Label model)
        {
            Label dbLabel = new Label();
            dbLabel.Label_ID = Guid.NewGuid();
            dbLabel.UserId = model.UserId;
            dbLabel.Label_Type = model.Label_Type;
            dbLabel.CompanyName = model.CompanyName;
            dbLabel.MarkingNo = model.MarkingNo;
            dbLabel.NRIC = model.NRIC;
            dbLabel.Name = model.Name;
            dbLabel.Date = DateTime.Now;
            dbLabel.LastStation = model.LastStation;
            dbLabel.PrintCount = 0;
            dbLabel.Queue_ID = model.Queue_ID;
            dbLabel.QRCode = Common.CommonUtil.CreateLabelQRCode(new Common.LabelInfo()
            {
                MarkingNo = model.MarkingNo,
                NRIC = model.NRIC,
                Name = model.Name
            }, "AESKey");
            _localUnitOfWork.GetRepository<Trinity.DAL.DBContext.Label>().Add(dbLabel);
            _localUnitOfWork.Save();
        }

        public Trinity.DAL.DBContext.Label GetByUserID(string UserID,string Label_Type, DateTime date)
        {
            date = date.Date;
           return  _localUnitOfWork.DataContext.Labels.FirstOrDefault(d => d.UserId.Equals(UserID) && d.Label_Type.Equals(Label_Type) && DbFunctions.TruncateTime(d.Date) == date);
        }
        public void UpdatePrinting(string UserID, string Label_Type, string PrintStatus, string LastStation, DateTime date)
        {
            date = date.Date;
            var lableUpdate = GetByUserID(UserID, Label_Type, date);
            if (lableUpdate != null)
            {
                lableUpdate.PrintCount += 1;
                lableUpdate.PrintStatus = PrintStatus;
                lableUpdate.LastStation = LastStation;
                _localUnitOfWork.GetRepository<Trinity.DAL.DBContext.Label>().Update(lableUpdate);
                _localUnitOfWork.Save();
            }
        }
        public List<BE.Label> GetAllLabelsForMUBAndTT()
        {
            var lstModels = (from l in _localUnitOfWork.DataContext.Labels
                             join u in _localUnitOfWork.DataContext.Membership_Users on l.UserId equals u.UserId
                             join q in _localUnitOfWork.DataContext.Queues on l.Queue_ID equals q.Queue_ID
                             join t in _localUnitOfWork.DataContext.Timeslots on q.Timeslot_ID equals t.Timeslot_ID
                             where (l.Label_Type.Equals(EnumLabelType.MUB) || l.Label_Type.Equals(EnumLabelType.TT))
                             orderby System.Data.Entity.DbFunctions.TruncateTime(l.Date) descending, t.StartTime descending, l.PrintCount
                             select new BE.Label()
                             {
                                 NRIC = u.NRIC,
                                 Name = u.Name,
                                 LastStation = l.LastStation,
                                 UserId = l.UserId,
                                 //TimeSlot_ID = q.Timeslot_ID,
                                 //StartTime = t.StartTime,
                                 //EndTime = t.EndTime,
                                 PrintCount = l.PrintCount,
                                 MarkingNo = l.MarkingNo
                             }).GroupBy(d => d.UserId).Select(d => d.FirstOrDefault()).ToList();

            return lstModels;
        }

        public List<BE.Label> GetAllLabelsForUBToday()
        {
            return _localUnitOfWork.DataContext.Labels.Where(d =>  d.Label_Type == EnumLabelType.UB &&  DbFunctions.TruncateTime(d.Date) == DateTime.Today).Select(d => new BE.Label()
            {
                NRIC = d.Membership_Users.NRIC,
                Name = d.Membership_Users.Name,
                LastStation = d.LastStation,
                UserId = d.UserId,
                MarkingNo = d.MarkingNo,
                Date = d.Date
            }).ToList();
            
        }

        public string GetMarkingNoByUserId(string userId)
        {
            try
            {
                if (EnumAppConfig.IsLocal)
                {
                    DBContext.Label label = _localUnitOfWork.DataContext.Labels.FirstOrDefault(d => d.UserId == userId);
                    if (label == null)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return label.MarkingNo;
                    }
                }
                else
                {
                    DBContext.Label label = _centralizedUnitOfWork.DataContext.Labels.FirstOrDefault(d => d.UserId == userId);
                    if (label == null)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return label.MarkingNo;
                    }
                }
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }
    }
}
