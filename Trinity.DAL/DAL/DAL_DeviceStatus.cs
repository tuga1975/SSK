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

        public bool Update(int deviceId, EnumDeviceStatus[] deviceStatuses,string Station = null)
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
                //Lib.SignalR.DeviceStatusUpdate(deviceId, deviceStatuses);
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

        public string CheckStatusDevicesStation(string station)
        {
            try
            {
                if (!_localUnitOfWork.DataContext.ApplicationDevice_Status.Any(a => a.Station.ToUpper() == station.ToUpper()))
                {
                    return EnumColors.Red;
                }

                if (_localUnitOfWork.DataContext.ApplicationDevice_Status.Any(d => d.Station.ToUpper() == station.ToUpper() && d.StatusCode == (int)EnumDeviceStatus.Disconnected))
                {
                    return EnumColors.Red;
                }

                return EnumColors.Green;
            }
            catch(Exception e)
            {
                return EnumColors.Red;
            }
        }

        /// <summary>
        /// Get application's health status
        /// </summary>
        /// <returns></returns>
        public EnumDeviceStatusSumary GetApplicationStatus()
        {
            try
            {
                string station = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;

                // get all devices statuses of station
                var deviceStatuses = _localUnitOfWork.DataContext.ApplicationDevice_Status.Where(item => item.Station.ToUpper() == station.ToUpper())
                    .Select(item => new {
                        DeviceId = item.DeviceID,
                        StatusCode = item.StatusCode
                    }).ToList();

                if (deviceStatuses == null || deviceStatuses.Count == 0)
                {
                    // application is down (notification server will delete all device status rows ) or cannot update status, return error
                    return EnumDeviceStatusSumary.Error;
                }
                else
                {
                    // if any device is disconnected, return error
                    if (deviceStatuses.Any(item => item.StatusCode == (int)EnumDeviceStatus.Disconnected))
                    {
                        return EnumDeviceStatusSumary.Error;
                    }

                    // if application have no device disconnected, and have any device status is diffirent connected, return caution
                    // Need to define caution statuses group
                    if (deviceStatuses.Any(item => item.StatusCode != (int)EnumDeviceStatus.Connected))
                    {
                        return EnumDeviceStatusSumary.Caution;
                    }

                    // if all devices are connected and have no caution, return ready
                    return EnumDeviceStatusSumary.Ready;
                }
            }
            catch (Exception ex)
            {
                return EnumDeviceStatusSumary.Error;
            }
        }

        public void RemoveDevice(string Station)
        {
            _localUnitOfWork.GetRepository<ApplicationDevice_Status>().Delete(d => d.Station == Station);
            _localUnitOfWork.Save();
        }
    }
}
