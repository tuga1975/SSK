using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;


namespace Trinity.DAL
{
    public class DAL_DrugResults
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        public void UpdateDiscardDrugResult(string UserID,string UserDoID)
        {
            string NRIC = new DAL_User().GetUserByUserId(UserID).Data.NRIC;
            DateTime today = DateTime.Today;
            var drugResult = _localUnitOfWork.DataContext.DrugResults.FirstOrDefault(d => d.NRIC == NRIC && DbFunctions.TruncateTime(d.UploadedDate) == today);
            drugResult.IsSealed = false;
            drugResult.SealedOrDiscardedBy = UserDoID;
            drugResult.SealedOrDiscardedDate = DateTime.Now;
            _localUnitOfWork.GetRepository<Trinity.DAL.DBContext.DrugResult>().Update(drugResult);
            _localUnitOfWork.Save();
        }
        public List<Guid> CheckDrugResult()
        {
            DateTime today = DateTime.Today;
            var query = (from queue in _localUnitOfWork.DataContext.Queues.Where(d => DbFunctions.TruncateTime(d.CreatedTime) == today && d.QueueDetails.Any(c => c.Station == EnumStation.HSA && c.Status == EnumQueueStatuses.Processing))
                         join drugresult in _localUnitOfWork.DataContext.DrugResults.Where(d => DbFunctions.TruncateTime(d.UploadedDate) == today) on queue.Membership_Users1.NRIC equals drugresult.NRIC
                         select new { queue, drugresult }).Where(d => d.drugresult != null).ToList();
            List<Guid> arrayQueueUpdateed = new List<Guid>();
            DAL_QueueNumber DAL_Queue = new DAL.DAL_QueueNumber();
            foreach (var item in query)
            {
                arrayQueueUpdateed.Add(item.queue.Queue_ID);
                DAL_Queue.UpdateQueueStatusByUserId(item.queue.UserId, EnumStation.HSA, EnumQueueStatuses.SelectSealOrDiscard, EnumStation.HSA, EnumQueueStatuses.SelectSealOrDiscard, EnumMessage.SelectSealtOrDiscard, EnumQueueOutcomeText.Processing);
            }
            return arrayQueueUpdateed;
        }

        public DBContext.DrugResult GetByNRICAndDate(string NRIC, DateTime DateCreate)
        {
            DateCreate = DateCreate.Date;
            return _localUnitOfWork.DataContext.DrugResults.FirstOrDefault(d => d.NRIC.Equals(NRIC) && DbFunctions.TruncateTime(d.UploadedDate) == DateCreate);
        }

        public DBContext.DrugResult GetByMarkingNumber(string MarkingNumber)
        {
            return _localUnitOfWork.DataContext.DrugResults.FirstOrDefault(d => d.markingnumber.Equals(MarkingNumber));
        }

        public string GetDrugTypeByNRIC(string NRIC)
        {
            if (EnumAppConfig.IsLocal)
            {
                System.Text.StringBuilder result = new System.Text.StringBuilder();
                DrugResult drug = _localUnitOfWork.DataContext.DrugResults.FirstOrDefault(d => d.NRIC.Equals(NRIC));
                if (drug == null)
                {
                    result.Append("NA-");
                }
                else
                {
                    if (drug.AMPH == true)
                    {
                        result.Append("AMPH-");
                    }
                    if (drug.BENZ == true)
                    {
                        result.Append("BENZ-");
                    }
                    if (drug.OPI == true)
                    {
                        result.Append("OPI-");
                    }
                    if (drug.THC == true)
                    {
                        result.Append("THC-");
                    }
                    if (drug.COCA == true)
                    {
                        result.Append("COCA-");
                    }
                    if (drug.BARB == true)
                    {
                        result.Append("BARB-");
                    }
                    if (drug.LSD == true)
                    {
                        result.Append("LSD-");
                    }
                    if (drug.METH == true)
                    {
                        result.Append("METH-");
                    }
                    if (drug.MTQL == true)
                    {
                        result.Append("MTQL-");
                    }
                    if (drug.PCP == true)
                    {
                        result.Append("PCP-");
                    }
                    if (drug.KET == true)
                    {
                        result.Append("KET-");
                    }
                    if (drug.BUPRE == true)
                    {
                        result.Append("BUPRE-");
                    }
                    if (drug.CAT == true)
                    {
                        result.Append("CAT-");
                    }
                    if (drug.PPZ == true)
                    {
                        result.Append("PPZ-");
                    }
                    if (drug.NPS == true)
                    {
                        result.Append("NPS-");
                    }
                    if (string.IsNullOrEmpty(result.ToString()))
                    {
                        result.Append("NA-");
                    }
                }

                return result.ToString().Remove(result.ToString().Length - 1);
            }
            else
            {
                System.Text.StringBuilder result = new System.Text.StringBuilder();
                DrugResult drug = _centralizedUnitOfWork.DataContext.DrugResults.FirstOrDefault(d => d.NRIC.Equals(NRIC));
                if (drug == null)
                {
                    result.Append("NA-");
                }
                else
                {
                    if (drug.AMPH == true)
                    {
                        result.Append("AMPH-");
                    }
                    if (drug.BENZ == true)
                    {
                        result.Append("BENZ-");
                    }
                    if (drug.OPI == true)
                    {
                        result.Append("OPI-");
                    }
                    if (drug.THC == true)
                    {
                        result.Append("THC-");
                    }
                    if (drug.COCA == true)
                    {
                        result.Append("COCA-");
                    }
                    if (drug.BARB == true)
                    {
                        result.Append("BARB-");
                    }
                    if (drug.LSD == true)
                    {
                        result.Append("LSD-");
                    }
                    if (drug.METH == true)
                    {
                        result.Append("METH-");
                    }
                    if (drug.MTQL == true)
                    {
                        result.Append("MTQL-");
                    }
                    if (drug.PCP == true)
                    {
                        result.Append("PCP-");
                    }
                    if (drug.KET == true)
                    {
                        result.Append("KET-");
                    }
                    if (drug.BUPRE == true)
                    {
                        result.Append("BUPRE-");
                    }
                    if (drug.CAT == true)
                    {
                        result.Append("CAT-");
                    }
                    if (drug.PPZ == true)
                    {
                        result.Append("PPZ-");
                    }
                    if (drug.NPS == true)
                    {
                        result.Append("NPS-");
                    }
                    if (string.IsNullOrEmpty(result.ToString()))
                    {
                        result.Append("NA-");
                    }
                }

                return result.ToString().Remove(result.ToString().Length - 1);
            }
        }

        public void UpdateDrugSeal(string userId, bool COCA, bool BARB, bool LSD, bool METH, bool MTQL, bool PCP, bool KET, bool BUPRE, bool CAT, bool PPZ, bool NPS, string updatedBy)
        {
            string NRIC = new DAL_User().GetUserByUserId(userId).Data.NRIC;
            var localRepo = _localUnitOfWork.GetRepository<DrugResult>();
            DateTime today = DateTime.Today;
            DrugResult drug = _localUnitOfWork.DataContext.DrugResults.FirstOrDefault(d => d.NRIC.Equals(NRIC) && DbFunctions.TruncateTime(d.UploadedDate)== today);
            if (drug != null)
            {
                drug.COCA = COCA;
                drug.BARB = BARB;
                drug.LSD = LSD;
                drug.METH = METH;
                drug.MTQL = MTQL;
                drug.PCP = PCP;
                drug.KET = KET;
                drug.BUPRE = BUPRE;
                drug.CAT = CAT;
                drug.PPZ = PPZ;
                drug.NPS = NPS;
                drug.IsSealed = true;
                drug.SealedOrDiscardedBy = updatedBy;
                drug.SealedOrDiscardedDate = DateTime.Now;
                localRepo.Update(drug);
                _localUnitOfWork.Save();
            }
        }

        public string GetResultUTByNRIC(string NRIC, DateTime date)
        {
            date = date.Date;
            DrugResult drug = _localUnitOfWork.DataContext.DrugResults.FirstOrDefault(d => d.NRIC.Equals(NRIC) && DbFunctions.TruncateTime(d.UploadedDate) == date);
            if (drug == null)
                return string.Empty;
            else
            {
                if (
                        (drug.AMPH.HasValue && drug.AMPH.Value) ||
                        (drug.BENZ.HasValue && drug.BENZ.Value) ||
                        (drug.OPI.HasValue && drug.OPI.Value) ||
                        (drug.THC.HasValue && drug.THC.Value)
                   )
                {
                    return EnumUTResult.POS;
                }
                else
                {
                    return EnumUTResult.NEG;
                }
            }

            //if (EnumAppConfig.IsLocal)
            //{

            //}
            //else
            //{
            //    DrugResult drug = _centralizedUnitOfWork.DataContext.DrugResults.FirstOrDefault(d => d.NRIC.Equals(NRIC));
            //    if (drug == null)
            //        return EnumUTResult.NEG;
            //    else
            //    {
            //        if (drug.AMPH.Value || drug.BARB.Value || drug.BENZ.Value || drug.BUPRE.Value || drug.CAT.Value || drug.COCA.Value || drug.KET.Value || drug.LSD.Value
            //            || drug.METH.Value || drug.MTQL.Value || drug.NPS.Value || drug.OPI.Value || drug.PCP.Value || drug.PPZ.Value || drug.THC.Value)
            //        {
            //            return EnumUTResult.POS;
            //        }
            //        else
            //        {
            //            return EnumUTResult.NEG;
            //        }
            //    }
            //}
        }
        //public DrugResult UpdateSealForUser(string userId, bool seal, string uploadedBy, string sealedOrDiscardedBy)
        //{
        //    try
        //    {
        //        string NRIC = new DAL_User().GetUserByUserId(userId).Data.NRIC;

        //        if (EnumAppConfig.IsLocal)
        //        {
        //            var localRepo = _localUnitOfWork.GetRepository<DrugResult>();
        //            DrugResult drug = _localUnitOfWork.DataContext.DrugResults.FirstOrDefault(d => d.NRIC.Equals(NRIC));

        //            if (drug == null)
        //            {
        //                Trinity.DAL.DBContext.Label dbLabel = new Trinity.DAL.DAL_Labels().GetByDateAndUserId(DateTime.Now.Date, userId);
        //                drug = new DrugResult();
        //                drug.DrugResultsID = NRIC;
        //                drug.NRIC = NRIC;
        //                drug.IsSealed = seal;
        //                drug.UploadedBy = uploadedBy;
        //                drug.UploadedDate = DateTime.Now;
        //                drug.SealedOrDiscardedBy = sealedOrDiscardedBy;
        //                drug.SealedOrDiscardedDate = DateTime.Now;
        //                if (dbLabel != null)
        //                {
        //                    drug.markingnumber = dbLabel.MarkingNo;
        //                }
        //                localRepo.Add(drug);
        //            }
        //            else
        //            {
        //                drug.NRIC = NRIC;
        //                drug.IsSealed = seal;
        //                drug.UploadedBy = uploadedBy;
        //                drug.UploadedDate = DateTime.Now;
        //                drug.SealedOrDiscardedBy = sealedOrDiscardedBy;
        //                drug.SealedOrDiscardedDate = DateTime.Now;

        //                localRepo.Update(drug);
        //            }

        //            _localUnitOfWork.Save();

        //            if (EnumAppConfig.ByPassCentralizedDB)
        //            {
        //                return drug;
        //            }
        //            else
        //            {
        //                bool centralizeStatus;
        //                var centralData = CallCentralized.Post<DrugResult>(EnumAPIParam.DrugResult, "UpdateSealForUser", out centralizeStatus, "userId=" + userId, "seal=" + seal.ToString(), "uploadedBy=" + uploadedBy, "sealedOrDiscardedBy=" + sealedOrDiscardedBy);
        //                if (centralizeStatus)
        //                {
        //                    return centralData;
        //                }
        //                else
        //                {
        //                    throw new Exception(EnumMessage.NotConnectCentralized);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            var centralRepo = _centralizedUnitOfWork.GetRepository<DrugResult>();
        //            DrugResult drug = _centralizedUnitOfWork.DataContext.DrugResults.FirstOrDefault(d => d.NRIC.Equals(NRIC));

        //            if (drug == null)
        //            {
        //                drug = new DrugResult();
        //                drug.NRIC = NRIC;
        //                drug.IsSealed = seal;
        //                drug.UploadedBy = uploadedBy;
        //                drug.UploadedDate = DateTime.Now;
        //                drug.SealedOrDiscardedBy = sealedOrDiscardedBy;
        //                drug.SealedOrDiscardedDate = DateTime.Now;

        //                centralRepo.Add(drug);
        //            }
        //            else
        //            {
        //                drug.NRIC = NRIC;
        //                drug.IsSealed = seal;
        //                drug.UploadedBy = uploadedBy;
        //                drug.UploadedDate = DateTime.Now;
        //                drug.SealedOrDiscardedBy = sealedOrDiscardedBy;
        //                drug.SealedOrDiscardedDate = DateTime.Now;

        //                centralRepo.Update(drug);
        //            }
        //            _centralizedUnitOfWork.Save();

        //            return drug;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        return null;
        //    }
        //}
    }
}
