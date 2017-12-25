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

        public bool Insert(Trinity.BE.DeviceStatus model)
        {
            try
            {
                var dbDeviceStatus = new ApplicationDevice_Status();
                SetInfoToInsert(model, dbDeviceStatus);
                _localUnitOfWork.GetRepository<ApplicationDevice_Status>().Add(dbDeviceStatus);
                _localUnitOfWork.Save();
                return true;
            }
            catch (Exception ex)
            {

                return false;
            }
        }
        public void SetInfoToInsert(Trinity.BE.DeviceStatus deviceStatus, ApplicationDevice_Status dbDeviceStatus)
        {

            dbDeviceStatus.StatusCode = deviceStatus.StatusCode;
            dbDeviceStatus.ApplicationType = deviceStatus.ApplicationType;
            dbDeviceStatus.DeviceID = deviceStatus.DeviceID;
            dbDeviceStatus.StatusMessage = deviceStatus.StatusMessage;
        }

        /// <summary>
        /// Set status info of a device
        /// </summary>
        /// <param name="appType">appType</param>
        /// <param name="deviceID">deviceID</param>
        /// <param name="statusMessage">statusMessage</param>
        /// <param name="statusCode">statusCode</param>
        /// <returns>DeviceStatus</returns>
        public Trinity.BE.DeviceStatus SetInfo(string appType, int? deviceID, string statusMessage, int statusCode)
        {
            var deviceStatus = new Trinity.BE.DeviceStatus();
            deviceStatus.ApplicationType = appType;
            deviceStatus.StatusCode = statusCode;
            deviceStatus.StatusMessage = statusMessage;
            deviceStatus.DeviceID = deviceID;

            return deviceStatus;
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
