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

        public bool Insert(List<ApplicationDevice_Status> listModel)
        {
            try
            {

                var repo = _localUnitOfWork.GetRepository<ApplicationDevice_Status>();
                
                repo.AddRange(listModel);
                _localUnitOfWork.Save();
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }
       

        public int GetDeviceId(string deviceType)
        {
            switch (deviceType)
            {
                case EnumDeviceType.SmartCardReader:
                    return 1;
                case EnumDeviceType.FingerprintScanner:
                    return 2;
                case EnumDeviceType.DocumentScanner:
                    return 3;
                case EnumDeviceType.ReceiptPrinter:
                    return 4;
                case EnumDeviceType.BarcodeScanner:
                    return 5;
                case EnumDeviceType.LEDDisplayMonitor:
                    return 6;
                case EnumDeviceType.Camera:
                    return 7;
                default:
                    return 0;
            }

        }
    }
}
