using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.DAL.Repository;

namespace Trinity.DAL
{
    public class DAL_Address
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();
        public int SaveAddress(BE.Address model, bool isLocal) {
            try
            {                
                if (isLocal)
                {
                    //var localUserRepo = _localUnitOfWork.GetRepository<Trinity.DAL.DBContext.Address>();
                    return ProcessSaveAddress(model, _localUnitOfWork);
                }
                else
                {
                    //var centralUserRepo = _centralizedUnitOfWork.GetRepository<Trinity.DAL.DBContext.Address>();
                    return ProcessSaveAddress(model, _centralizedUnitOfWork);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }

        private int ProcessSaveAddress(BE.Address model, IUnitOfWork unitOfWork)
        {
            try
            {
                var Repo = unitOfWork.GetRepository<Trinity.DAL.DBContext.Address>();
                if (model.Address_ID > 0)
                {                    
                    // Update
                    var dbAddress = Repo.GetById(model.Address_ID);
                    dbAddress.BlkHouse_Number = model.BlkHouse_Number;
                    dbAddress.FlrUnit_Number = model.FlrUnit_Number;
                    dbAddress.Street_Name = model.Street_Name;
                    dbAddress.Country = model.Country;
                    dbAddress.Postal_Code = model.Postal_Code;
                    Repo.Update(dbAddress);
                    unitOfWork.Save();
                    return model.Address_ID;
                }
                else
                {
                    // Insert
                    var dbAddress = new Trinity.DAL.DBContext.Address();
                    dbAddress.BlkHouse_Number = model.BlkHouse_Number;
                    dbAddress.FlrUnit_Number = model.FlrUnit_Number;
                    dbAddress.Street_Name = model.Street_Name;
                    dbAddress.Country = model.Country;
                    dbAddress.Postal_Code = model.Postal_Code;
                    Repo.Add(dbAddress);
                    unitOfWork.Save();
                    return 2;
                    // get lastest id and return;
                    //var address = unitOfWork.DataContext.Addresses.LastOrDefault();                    
                    //if (address != null) return address.Address_ID;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }
        }
    }
}
