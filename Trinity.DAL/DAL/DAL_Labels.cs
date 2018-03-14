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
            catch(Exception e)
            {
                return null;
            }
        }

        public Label UpdateLabel(BE.Label model)
        {
            try
            {
                Label dbLabel = null;
                Guid labelID_NewGuid = Guid.NewGuid();
                var queue = new DAL_QueueNumber().GetMyQueueToday(model.UserId);
                
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
                        if (queue != null)
                        {
                            dbLabel.Queue_ID = queue.Queue_ID;
                        }

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
                        if (queue != null)
                        {
                            dbLabel.Queue_ID = queue.Queue_ID;
                        }

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
                        if (queue != null)
                        {
                            dbLabel.Queue_ID = queue.Queue_ID;
                        }

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
                        if (queue != null)
                        {
                            dbLabel.Queue_ID = queue.Queue_ID;
                        }

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
                    //var lstModels = _localUnitOfWork.DataContext.Labels.Include("Membership_Users")
                    //    .Where(l => l.Label_Type.Equals(EnumLabelType.MUB) || l.Label_Type.Equals(EnumLabelType.TT))
                    //    .Select(d => new BE.Label()
                    //    {
                    //        NRIC = d.Membership_Users.NRIC,
                    //        Name = d.Membership_Users.Name,
                    //        LastStation = d.LastStation,
                    //        UserId = d.UserId
                    //    });

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
                                    }).GroupBy(d=>d.UserId).Select(d=>d.FirstOrDefault()).ToList();

                    return lstModels;

                    //if ((lstModels != null && lstModels.Count() > 0) || EnumAppConfig.ByPassCentralizedDB)
                    //{
                    //    return lstModels.Distinct().ToList();
                    //}
                    //else
                    //{
                    //    bool centralizeStatus;
                    //    var centralUpdate = CallCentralized.Get<List<BE.Label>>(EnumAPIParam.Label, "GetAllLabelsForMUBAndTT", out centralizeStatus);
                    //    if (centralizeStatus)
                    //    {
                    //        return centralUpdate;
                    //    }
                    //    return lstModels.ToList();
                    //}
                }
                else
                {
                    //var lstModels = _centralizedUnitOfWork.DataContext.Labels.Include("Membership_Users")
                    //.Where(l => l.Label_Type.Equals(EnumLabelType.MUB) || l.Label_Type.Equals(EnumLabelType.TT))
                    //.Select(d => new BE.Label()
                    //{
                    //    NRIC = d.Membership_Users.NRIC,
                    //    Name = d.Membership_Users.Name,
                    //    LastStation = d.LastStation,
                    //    UserId = d.UserId
                    //});

                    var lstModels = from l in _centralizedUnitOfWork.DataContext.Labels
                                    join u in _centralizedUnitOfWork.DataContext.Membership_Users on l.UserId equals u.UserId
                                    join q in _centralizedUnitOfWork.DataContext.Queues on l.Queue_ID equals q.Queue_ID
                                    join t in _centralizedUnitOfWork.DataContext.Timeslots on q.Timeslot_ID equals t.Timeslot_ID
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
                                    };

                    return lstModels.Distinct().ToList();
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
                        .Join(_localUnitOfWork.DataContext.DrugResults,
                                l => l.Membership_Users.NRIC,
                                d => d.NRIC,
                                (l, d) => new BE.Label()
                                {
                                    NRIC = l.Membership_Users.NRIC,
                                    Name = l.Membership_Users.Name,
                                    LastStation = l.LastStation,
                                    UserId = l.UserId,
                                    IsSealed = d.IsSealed,
                                    MarkingNo = l.MarkingNo
                                })
                    .Where(d => d.IsSealed == true);
                    return lstModels.Distinct().ToList();

                    //if ((lstModels != null && lstModels.Count() > 0) || EnumAppConfig.ByPassCentralizedDB)
                    //{
                    //    return lstModels.ToList();
                    //}
                    //else
                    //{
                    //    bool centralizeStatus;
                    //    var centralUpdate = CallCentralized.Get<List<BE.Label>>(EnumAPIParam.Label, "GetAllLabelsForUB", out centralizeStatus);
                    //    if (centralizeStatus)
                    //    {
                    //        return centralUpdate;
                    //    }
                    //    return lstModels.ToList();
                    //}
                }
                else
                {
                    var lstModels = _localUnitOfWork.DataContext.Labels.Include("Membership_Users")
                        .Join(_localUnitOfWork.DataContext.DrugResults,
                                l => l.Membership_Users.NRIC,
                                d => d.NRIC,
                                (l, d) => new BE.Label()
                                {
                                    NRIC = l.Membership_Users.NRIC,
                                    Name = l.Membership_Users.Name,
                                    LastStation = l.LastStation,
                                    UserId = l.UserId,
                                    IsSealed = d.IsSealed,
                                    MarkingNo = l.MarkingNo
                                })
                    .Where(d => d.IsSealed == true);
                    return lstModels.Distinct().ToList();
                }
            }
            catch (Exception e)
            {
                return null;
            }
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
            catch(Exception e)
            {
                return string.Empty;
            }
        }
    }
}
