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
            return _centralizedUnitOfWork.DataContext.DrugResults.FirstOrDefault(d => d.markingnumber == MarkingNumber);
        }

        public string GetDrugTypeByNRIC(string NRIC)
        {
            System.Text.StringBuilder result = new System.Text.StringBuilder();
            DrugResult drug = _centralizedUnitOfWork.DataContext.DrugResults.FirstOrDefault(d => d.NRIC.Equals(NRIC));
            if(drug == null)
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
                if(string.IsNullOrEmpty(result.ToString()))
                {
                    result.Append("NA-");
                }
            }

            return result.ToString().Remove(result.ToString().Length - 1);
        }
    }
}
