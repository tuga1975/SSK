using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Trinity.BE;
using Trinity.Common;
using Trinity.DAL.DBContext;
using Trinity.DAL.Repository;


namespace Trinity.DAL
{
    public class DAL_DeviceStatus
    {
        Local_UnitOfWork _localUnitOfWork = new Local_UnitOfWork();
        Centralized_UnitOfWork _centralizedUnitOfWork = new Centralized_UnitOfWork();

        public bool Update(int deviceId, EnumDeviceStatuses[] deviceStatuses,string Station = null)
        {
            try
            {
                // validate
                if (!DeviceIdExist(deviceId))
                {
                    throw new Exception("DeviceID is not valid");
                }

                // local db
                // delete old statuses
                string station = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
                var oldRows = _localUnitOfWork.DataContext.ApplicationDevice_Status.Where(item => item.DeviceID == deviceId && item.Station.Equals(station));
                _localUnitOfWork.DataContext.ApplicationDevice_Status.RemoveRange(oldRows);

                // insert new statuses
                if (deviceStatuses != null && deviceStatuses.Count() > 0)
                {
                    // create new status entites
                    ApplicationDevice_Status deviceStatus;
                    foreach (var status in deviceStatuses)
                    {
                        deviceStatus = new ApplicationDevice_Status();
                        deviceStatus.Station = station;
                        deviceStatus.DeviceID = deviceId;
                        deviceStatus.ID = Guid.NewGuid();
                        deviceStatus.StatusCode = (int)status;
                        deviceStatus.StatusMessage = CommonUtil.GetDeviceStatusText(status);

                        // insert new statuses
                        _localUnitOfWork.DataContext.ApplicationDevice_Status.Add(deviceStatus);
                    }
                }

                // savechanges
                if (_localUnitOfWork.DataContext.SaveChanges() < 0)
                {
                    throw new Exception("Save data to local database failed.");
                }
                // Send Noti server
                Lib.SignalR.DeviceStatusUpdate(deviceId, deviceStatuses);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DAL_DeviceStatus.Update exception: " + ex.ToString());
                return false;
            }
        }

        private bool DeviceIdExist(int deviceID)
        {
            if (EnumAppConfig.IsLocal)
            {
                try
                {
                    return _localUnitOfWork.DataContext.Devices.Any(item => item.DeviceID.Equals(deviceID));
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
            else
            {
                return _centralizedUnitOfWork.DataContext.Devices.Any(item => item.DeviceID.Equals(deviceID));
            }
        }

        public bool CheckStatusDevicesStation(string station)
        {
            if (!_localUnitOfWork.DataContext.ApplicationDevice_Status.Any(a=>a.Station.ToUpper() == station.ToUpper()))
            {
                return false;
            }
            return !_localUnitOfWork.DataContext.ApplicationDevice_Status.Any(d => d.Station.ToUpper() == station.ToUpper() && d.StatusCode == (int)EnumDeviceStatuses.Disconnected);
        }
        public void RemoveDevice(string Station)
        {
            _localUnitOfWork.GetRepository<ApplicationDevice_Status>().Delete(d => d.Station == Station);
            _localUnitOfWork.Save();
        }
    }
}
