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
using Trinity.Device.Util;

namespace Trinity.Device
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
                // get status
                var statuses = FingerprintReaderUtil.Instance.GetDeviceStatus();

                // update local ApplicationDevice_Status
                DAL_DeviceStatus dAL_DeviceStatus = new DAL_DeviceStatus();
                dAL_DeviceStatus.Update((int)EnumDeviceIds.FingerprintScanner, statuses);
                Trinity.SignalR.Client.Instance.DeviceStatusChanged((int)EnumDeviceIds.FingerprintScanner, statuses);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("FingerprintReaderMonitor.ReportDeviceStatus exception: " + ex.ToString());
            }
        }
    }
}
