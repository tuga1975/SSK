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
        #region 2018
        private void UpdateOrInsert(BE.Address model, IUnitOfWork unitOfWork)
        {
            var AddressRespon = unitOfWork.GetRepository<Trinity.DAL.DBContext.Address>();
            DBContext.Address dbAddress = AddressRespon.GetById(model.Address_ID);
            if (dbAddress == null)
            {
                dbAddress = new Trinity.DAL.DBContext.Address();
                dbAddress.Address_ID = model.Address_ID;
                dbAddress.BlkHouse_Number = model.BlkHouse_Number;
                dbAddress.FlrUnit_Number = model.FlrUnit_Number;
                dbAddress.Street_Name = model.Street_Name;
                dbAddress.Country = model.Country;
                dbAddress.Postal_Code = model.Postal_Code;
                AddressRespon.Add(dbAddress);
                _localUnitOfWork.Save();
            }
            else
            {
                dbAddress.BlkHouse_Number = model.BlkHouse_Number;
                dbAddress.FlrUnit_Number = model.FlrUnit_Number;
                dbAddress.Street_Name = model.Street_Name;
                dbAddress.Country = model.Country;
                dbAddress.Postal_Code = model.Postal_Code;
                AddressRespon.Update(dbAddress);
                _localUnitOfWork.Save();
            }
        }
        public string SaveAddress(BE.Address model)
        {
            if (string.IsNullOrEmpty(model.Address_ID))
            {
                model.Address_ID = Guid.NewGuid().ToString().Trim();
            }
            if (EnumAppConfig.IsLocal)
            {
                //bool statusCentralized;
                //CallCentralized.Post("Address", "SaveAddress", out statusCentralized, model);
                //if (!statusCentralized)
                //{
                //    throw new Exception(EnumMessage.NotConnectCentralized);
                //}
                //else
                //{
                UpdateOrInsert(model, _localUnitOfWork);
                //}
            }
            else
            {
                UpdateOrInsert(model, _centralizedUnitOfWork);
            }
            return model.Address_ID;
        }

        public string SaveOtherAddress(BE.OtherAddress model)
        {
            if (string.IsNullOrEmpty(model.OAddress_ID))
            {
                model.OAddress_ID = Guid.NewGuid().ToString();
            }

            var add = new BE.Address() { Address_ID = model.OAddress_ID, BlkHouse_Number = model.OBlkHouse_Number, Country = model.OCountry, FlrUnit_Number = model.OFlrUnit_Number, Postal_Code = model.OPostal_Code, Street_Name = model.OStreet_Name };
            UpdateOrInsert(add, _localUnitOfWork);


            return model.OAddress_ID;
        }
        #endregion


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
