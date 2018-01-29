using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trinity.BE;
using Trinity.Common;
using Trinity.DAL;

namespace Trinity.Util
{
    public static class FingerprintReaderMonitor
    {
        public static bool Start()
        {
            try
            {
                // start health checker
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                Task task = Repeat.Interval(() => ReportDeviceStatus(), cancellationTokenSource.Token);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("FingerprintReaderMonitor.Start exception: " + ex.ToString());
                return false;
            }
        }

        private static void ReportDeviceStatus()
        {
            try
            {
                Debug.WriteLine("FingerprintReaderMonitor.ReportDeviceStatus " + DateTime.Now.ToString());
                var status = FingerprintReaderUtil.Instance.GetDeviceStatus();

                // create entity
                string station = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
                DeviceStatus deviceStatus = new DeviceStatus()
                {
                    DeviceID = (int)EnumDeviceIds.FingerprintScanner,
                    Station = station,
                    StatusCode = status
                };

                // update local ApplicationDevice_Status
                DAL_DeviceStatus dAL_DeviceStatus = new DAL_DeviceStatus();
                dAL_DeviceStatus.Update(deviceStatus);

                // update centralized db ApplicationDevice_Status
                // if update failed, notify duty officer
            }
            catch (Exception ex)
            {
                Debug.WriteLine("FingerprintReaderMonitor.ReportDeviceStatus exception: " + ex.ToString());
            }
        }
    }
}
