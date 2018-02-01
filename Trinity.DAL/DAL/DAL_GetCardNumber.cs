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
    public class DAL_GetCardNumber
    {
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        public BE.CardNumber GetCardNumber()
        {
            if (EnumAppConfig.IsLocal)
            {
                return CallCentralized.Instance.Get<BE.CardNumber>("Settings", "GetCardNumber");
            }
            else
            {
                SqlParameter CardNumber = new SqlParameter("@CardNumber", SqlDbType.Int);
                CardNumber.Direction = ParameterDirection.Output;
                SqlParameter MaxCardNumber = new SqlParameter("@MaxCardNumber", SqlDbType.Int);
                MaxCardNumber.Direction = ParameterDirection.Output;
                _centralizedUnitOfWork.DataContext.Database.SqlQuery<object>("exec GetCardNumber @CardNumber = @CardNumber OUTPUT,@MaxCardNumber = @MaxCardNumber OUTPUT", CardNumber, MaxCardNumber).FirstOrDefault();
                return new BE.CardNumber()
                {
                    MaxNumber = Convert.ToInt32(MaxCardNumber.Value),
                    Number = Convert.ToInt32(CardNumber.Value),
                    Year = DateTime.Now.Year
                };
            }
        }
        
    }
}
