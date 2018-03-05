using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;


namespace Trinity.DAL
{
    public class DAL_DrugResults
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        public DBContext.DrugResult GetByMarkingNumber(string MarkingNumber)
        {
            if (EnumAppConfig.IsLocal)
            {
                return _localUnitOfWork.DataContext.DrugResults.FirstOrDefault(d => d.markingnumber == MarkingNumber);
            }
            else
            {
                return _centralizedUnitOfWork.DataContext.DrugResults.FirstOrDefault(d => d.markingnumber == MarkingNumber);
            }
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

        public DrugResult UpdateDrugSeal(string userId,bool COCA, bool BARB, bool LSD, bool METH, bool MTQL, bool PCP, bool KET, bool BUPRE, bool CAT, bool PPZ, bool NPS, string updatedBy)
        {
            try
            {
                string NRIC = new DAL_User().GetUserByUserId(userId).Data.NRIC;

                if (EnumAppConfig.IsLocal)
                {
                    var localRepo = _localUnitOfWork.GetRepository<DrugResult>();
                    DrugResult drug = _localUnitOfWork.DataContext.DrugResults.FirstOrDefault(d => d.NRIC.Equals(NRIC));
                    
                    if(drug != null)
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
                        drug.UploadedBy = updatedBy;

                        localRepo.Update(drug);
                        _localUnitOfWork.Save();                                               
                    }

                    if(EnumAppConfig.ByPassCentralizedDB)
                    {
                        return drug;
                    }
                    else 
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Post<DrugResult>(EnumAPIParam.DrugResult, "UpdateDrugSeal", out centralizeStatus, "userId=" + userId, "COCA=" + COCA.ToString(),
                                            "BARB=" + BARB.ToString(), "LSD=" + LSD.ToString(), "METH=" + METH.ToString(), "MTQL=" + MTQL.ToString(), "PCP=" + PCP.ToString(), "KET=" + KET.ToString(),
                                            "BUPRE=" + BUPRE.ToString(), "CAT=" + CAT.ToString(), "PPZ=" + PPZ.ToString(), "NPS=" + NPS.ToString(), "updatedBy=" + updatedBy);
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                        else
                        {
                            throw new Exception(EnumMessage.NotConnectCentralized);
                        }
                    }                    
                }
                else
                {
                    var centralRepo = _centralizedUnitOfWork.GetRepository<DrugResult>();
                    DrugResult drug = _centralizedUnitOfWork.DataContext.DrugResults.FirstOrDefault(d => d.NRIC.Equals(NRIC));

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
                        drug.UploadedBy = updatedBy;

                        centralRepo.Update(drug);
                        _centralizedUnitOfWork.Save();
                    }

                    return drug;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public string GetResultUTByNRIC(string NRIC)
        {
            if (EnumAppConfig.IsLocal)
            {
                DrugResult drug = _localUnitOfWork.DataContext.DrugResults.FirstOrDefault(d => d.NRIC.Equals(NRIC));
                if (drug == null)
                    return EnumUTResult.NEG;
                else
                {
                    if ((drug.AMPH.HasValue && drug.AMPH.Value) || (drug.BARB.HasValue && drug.BARB.Value) || (drug.BENZ.HasValue && drug.BENZ.Value) || (drug.BUPRE.HasValue && drug.BUPRE.Value) || (drug.CAT.HasValue && drug.CAT.Value) 
                        || (drug.COCA.HasValue && drug.COCA.Value) || (drug.KET.HasValue && drug.KET.Value) || (drug.LSD.HasValue && drug.LSD.Value) || (drug.METH.HasValue && drug.METH.Value) || (drug.MTQL.HasValue && drug.MTQL.Value) 
                        || (drug.NPS.HasValue && drug.NPS.Value) || (drug.OPI.HasValue) && drug.OPI.Value || (drug.PCP.HasValue && drug.PCP.Value) || (drug.PPZ.HasValue && drug.PPZ.Value) || (drug.THC.HasValue && drug.THC.Value))
                    {
                        return EnumUTResult.POS;
                    }
                    else
                    {
                        return EnumUTResult.NEG;
                    }
                }
            }
            else
            {
                DrugResult drug = _centralizedUnitOfWork.DataContext.DrugResults.FirstOrDefault(d => d.NRIC.Equals(NRIC));
                if (drug == null)
                    return EnumUTResult.NEG;
                else
                {
                    if (drug.AMPH.Value || drug.BARB.Value || drug.BENZ.Value || drug.BUPRE.Value || drug.CAT.Value || drug.COCA.Value || drug.KET.Value || drug.LSD.Value
                        || drug.METH.Value || drug.MTQL.Value || drug.NPS.Value || drug.OPI.Value || drug.PCP.Value || drug.PPZ.Value || drug.THC.Value)
                    {
                        return EnumUTResult.POS;
                    }
                    else
                    {
                        return EnumUTResult.NEG;
                    }
                }
            }
        }
        public DrugResult UpdateSealForUser(string userId, bool seal, string uploadedBy, string sealedOrDiscardedBy)
        {
            try
            {
                string NRIC = new DAL_User().GetUserByUserId(userId).Data.NRIC;

                if (EnumAppConfig.IsLocal)
                {
                    var localRepo = _localUnitOfWork.GetRepository<DrugResult>();
                    DrugResult drug = _localUnitOfWork.DataContext.DrugResults.FirstOrDefault(d => d.NRIC.Equals(NRIC));

                    if (drug == null)
                    {
                        Trinity.DAL.DBContext.Label dbLabel = new Trinity.DAL.DAL_Labels().GetByDateAndUserId(DateTime.Now.Date, userId);
                        drug = new DrugResult();
                        drug.DrugResultsID = NRIC;
                        drug.NRIC = NRIC;
                        drug.IsSealed = seal;
                        drug.UploadedBy = uploadedBy;
                        drug.UploadedDate = DateTime.Now;
                        drug.SealedOrDiscardedBy = sealedOrDiscardedBy;
                        drug.SealedOrDiscardedDate = DateTime.Now;
                        if (dbLabel != null)
                        {
                            drug.markingnumber = dbLabel.MarkingNo;
                        }
                        localRepo.Add(drug);
                    }
                    else
                    {
                        drug.NRIC = NRIC;
                        drug.IsSealed = seal;
                        drug.UploadedBy = uploadedBy;
                        drug.UploadedDate = DateTime.Now;
                        drug.SealedOrDiscardedBy = sealedOrDiscardedBy;
                        drug.SealedOrDiscardedDate = DateTime.Now;

                        localRepo.Update(drug);
                    }

                    _localUnitOfWork.Save();

                    if (EnumAppConfig.ByPassCentralizedDB)
                    {
                        return drug;
                    }
                    else
                    {
                        bool centralizeStatus;
                        var centralData = CallCentralized.Post<DrugResult>(EnumAPIParam.DrugResult, "UpdateSealForUser", out centralizeStatus, "userId=" + userId, "seal=" + seal.ToString(), "uploadedBy=" + uploadedBy, "sealedOrDiscardedBy=" + sealedOrDiscardedBy);
                        if (centralizeStatus)
                        {
                            return centralData;
                        }
                        else
                        {
                            throw new Exception(EnumMessage.NotConnectCentralized);
                        }
                    }
                }
                else
                {
                    var centralRepo = _centralizedUnitOfWork.GetRepository<DrugResult>();
                    DrugResult drug = _centralizedUnitOfWork.DataContext.DrugResults.FirstOrDefault(d => d.NRIC.Equals(NRIC));

                    if (drug == null)
                    {
                        drug = new DrugResult();
                        drug.NRIC = NRIC;
                        drug.IsSealed = seal;
                        drug.UploadedBy = uploadedBy;
                        drug.UploadedDate = DateTime.Now;
                        drug.SealedOrDiscardedBy = sealedOrDiscardedBy;
                        drug.SealedOrDiscardedDate = DateTime.Now;

                        centralRepo.Add(drug);
                    }
                    else
                    {
                        drug.NRIC = NRIC;
                        drug.IsSealed = seal;
                        drug.UploadedBy = uploadedBy;
                        drug.UploadedDate = DateTime.Now;
                        drug.SealedOrDiscardedBy = sealedOrDiscardedBy;
                        drug.SealedOrDiscardedDate = DateTime.Now;

                        centralRepo.Update(drug);
                    }
                    _centralizedUnitOfWork.Save();

                    return drug;
                }
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
