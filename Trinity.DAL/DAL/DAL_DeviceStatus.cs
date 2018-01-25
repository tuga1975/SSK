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

        public bool Update(DeviceStatus model)
        {
            try
            {
                // validate
                if (!DeviceIdExist(model.DeviceID))
                {
                    throw new Exception("DeviceID is not valid");
                }

                // local db
                // delete old statuses
                var oldRows = _localUnitOfWork.DataContext.ApplicationDevice_Status.Where(item => item.DeviceID == model.DeviceID && item.Station.Equals(model.Station));
                _localUnitOfWork.DataContext.ApplicationDevice_Status.RemoveRange(oldRows);

                // insert new statuses
                if (model.StatusCode!= null && model.StatusCode.Count() > 0)
                {
                    // create new status entites
                    ApplicationDevice_Status deviceStatus;
                    foreach (var item in model.StatusCode)
                    {
                        deviceStatus = new ApplicationDevice_Status();
                        deviceStatus.Station = model.Station;
                        deviceStatus.DeviceID = model.DeviceID;
                        deviceStatus.ID = Guid.NewGuid();
                        deviceStatus.StatusCode = (int)item;
                        deviceStatus.StatusMessage = CommonUtil.GetDeviceStatusText(item);

                        // insert new statuses
                        _localUnitOfWork.DataContext.ApplicationDevice_Status.Add(deviceStatus);
                    }
                }

                // savechanges
                if(_localUnitOfWork.DataContext.SaveChanges() < 0)
                {
                    throw new Exception("Save data to local database failed.");
                }

                // update centralized db

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
            try
            {
                return _localUnitOfWork.DataContext.Devices.Any(item => item.DeviceID.Equals(deviceID));
            }
            catch
            {
                return false;
            }
        }
    }
}
