using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Common;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;


namespace Trinity.DAL
{
    public class DAL_DeviceStatus
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        public bool UpdateHealthStatus(List<BE.DeviceStatus> listModel)
        {
            try
            {
                var localRepo = _localUnitOfWork.GetRepository<ApplicationDevice_Status>();
                var centralRepo = _centralizedUnitOfWork.GetRepository<ApplicationDevice_Status>();

                var listdbDeviceStatus = new List<ApplicationDevice_Status>();
                if (!_localUnitOfWork.DataContext.ApplicationDevice_Status.Any())
                {

                    foreach (var item in listModel)
                    {
                        SetInfoToInsert(item, listdbDeviceStatus);
                    }

                    centralRepo.AddRange(listdbDeviceStatus);
                    localRepo.AddRange(listdbDeviceStatus);
                    _centralizedUnitOfWork.Save();
                    _localUnitOfWork.Save();
                    return true;
                }
                else
                {

                    foreach (var item in listModel)
                    {
                        var dbDeviceStatus = localRepo.Get(d => d.DeviceID == item.DeviceID);
                        if (dbDeviceStatus != null)
                        {
                            SetInfoToUpdate(item, dbDeviceStatus);
                            localRepo.Update(dbDeviceStatus);
                            _localUnitOfWork.Save();
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {

                return false;
            }
        }


        private void SetInfoToInsert(BE.DeviceStatus model, List<ApplicationDevice_Status> dbDeviceStatus)
        {
            foreach (var item in model.StatusCode)
            {
                var deviceStatus = new ApplicationDevice_Status();
                deviceStatus.Station = model.Station;
                deviceStatus.DeviceID = model.DeviceID;
                deviceStatus.ID = Guid.NewGuid();
                deviceStatus.StatusCode = (int)item;
                deviceStatus.StatusMessage = CommonUtil.GetDeviceStatusText(item);
                dbDeviceStatus.Add(deviceStatus);
            }
        }

        private void SetInfoToUpdate(BE.DeviceStatus model, ApplicationDevice_Status dbDeviceStatus)
        {
            foreach (var item in model.StatusCode)
            {
                var deviceStatus = new ApplicationDevice_Status();
                deviceStatus.Station = model.Station;
                deviceStatus.DeviceID = model.DeviceID;
                deviceStatus.StatusCode = (int)item;
                deviceStatus.StatusMessage = CommonUtil.GetDeviceStatusText(item);

            }
        }

        public BE.DeviceStatus SetInfo(string appName, int? deviceId, EnumDeviceStatuses[] statusCode)
        {
            return new BE.DeviceStatus
            {
                Station = appName,
                DeviceID = deviceId,
                StatusCode = statusCode
            };
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
