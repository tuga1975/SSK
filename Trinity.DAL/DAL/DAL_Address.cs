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
        public string SaveAddress(BE.Address model, bool isLocal) {
            if (isLocal)
            {
                string LastestId = ProcessSaveAddress(model, _localUnitOfWork);
                //if (LastestId == 0) {
                //    LastestId = _localUnitOfWork.DataContext.Addresses.Max(a => a.Address_ID);
                //}
                return LastestId;
            }
            else
            {
                string LastestId = ProcessSaveAddress(model, _centralizedUnitOfWork);
                //if (LastestId == 0)
                //{
                //    LastestId = _centralizedUnitOfWork.DataContext.Addresses.Max(a => a.Address_ID);
                //}
                return LastestId;
            }
        }

        private string ProcessSaveAddress(BE.Address model, IUnitOfWork unitOfWork)
        {
            var Repo = unitOfWork.GetRepository<Trinity.DAL.DBContext.Address>();
            if (!string.IsNullOrEmpty(model.Address_ID))
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
                dbAddress.Address_ID = Guid.NewGuid().ToString().Trim();
                dbAddress.BlkHouse_Number = model.BlkHouse_Number;
                dbAddress.FlrUnit_Number = model.FlrUnit_Number;
                dbAddress.Street_Name = model.Street_Name;
                dbAddress.Country = model.Country;
                dbAddress.Postal_Code = model.Postal_Code;
                Repo.Add(dbAddress);
                unitOfWork.Save();
                return dbAddress.Address_ID;
            }
        }
    }
}
