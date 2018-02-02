using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;


namespace Trinity.DAL
{
    public class DAL_GetCardInfo
    {
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        public BE.CardInfo GetCardInfo()
        {
            if (EnumAppConfig.IsLocal)
            {
                return CallCentralized.Get<BE.CardInfo>("Settings", "GetCardInfo");
            }
            else
            {
                SqlParameter CardNumber = new SqlParameter("@CardNumber", SqlDbType.Int);
                CardNumber.Direction = ParameterDirection.Output;

                SqlParameter MaxCardNumber = new SqlParameter("@MaxCardNumber", SqlDbType.Int);
                MaxCardNumber.Direction = ParameterDirection.Output;

                SqlParameter CompanyName = new SqlParameter("@CompanyName", SqlDbType.NVarChar,100);
                CompanyName.Direction = ParameterDirection.Output;

                SqlParameter VenueName = new SqlParameter("@VenueName", SqlDbType.NVarChar,50);
                VenueName.Direction = ParameterDirection.Output;

                SqlParameter Address = new SqlParameter("@Address", SqlDbType.NVarChar,255);
                Address.Direction = ParameterDirection.Output;

                SqlParameter ContactNumber = new SqlParameter("@ContactNumber", SqlDbType.NVarChar,50);
                ContactNumber.Direction = ParameterDirection.Output;

                _centralizedUnitOfWork.DataContext.Database.SqlQuery<object>(("exec GetCardInfo " +
                    "@CardNumber = @CardNumber OUTPUT," +
                    "@MaxCardNumber = @MaxCardNumber OUTPUT," +
                    "@CompanyName = @CompanyName OUTPUT," +
                    "@VenueName = @VenueName OUTPUT," +
                    "@Address = @Address OUTPUT," +
                    "@ContactNumber = @ContactNumber OUTPUT"), CardNumber, MaxCardNumber, CompanyName, VenueName, Address, ContactNumber).FirstOrDefault();
                return new BE.CardInfo()
                {
                    MaxNumber = Convert.ToInt32(MaxCardNumber.Value),
                    Number = Convert.ToInt32(CardNumber.Value),
                    Year = DateTime.Now.Year,
                    CompanyName = CompanyName.Value.ToString(),
                    VenueName = VenueName.Value.ToString(),
                    Address = Address.Value.ToString(),
                    ContactNumber = ContactNumber.Value.ToString()
                };
            }
        }
        
    }
}
