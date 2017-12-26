using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;


namespace Trinity.DAL
{
    public class DAL_DeviceStatus
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        public bool Insert(List<BE.DeviceStatus> listModel)
        {
            try
            {
                var repo = _localUnitOfWork.GetRepository<ApplicationDevice_Status>();

                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }


        private void SetInfo(BE.DeviceStatus model,List<ApplicationDevice_Status> dbDeviceStatus)
        {
            foreach (var item in model.StatusCode)
            {
                var deviceStatus = new ApplicationDevice_Status();
                deviceStatus.ApplicationType = model.ApplicationType;
                deviceStatus.DeviceID = model.DeviceID;
                deviceStatus.ID = Guid.NewGuid();
                deviceStatus.StatusCode = (int)item;
                
            }
        }

        public int GetDeviceId(string deviceType)
        {
            switch (deviceType)
            {
                case EnumDeviceTypes.SmartCardReader:
                    return 1;
                case EnumDeviceTypes.FingerprintScanner:
                    return 2;
                case EnumDeviceTypes.DocumentScanner:
                    return 3;
                case EnumDeviceTypes.ReceiptPrinter:
                    return 4;
                case EnumDeviceTypes.BarcodeScanner:
                    return 5;
                case EnumDeviceTypes.LEDDisplayMonitor:
                    return 6;
                case EnumDeviceTypes.Camera:
                    return 7;
                default:
                    return 0;
            }

        }
    }
}
