using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Trinity.DAL;
using Trinity.Util;

namespace Trinity.Device
{
    public static class DocumentScannerMonitor
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
                Debug.WriteLine("DocumentScannerMonitor.Start exception: " + ex.ToString());
                return false;
            }
        }

        private static void ReportDeviceStatus()
        {
            try
            {
                // get statuses
                var statuses = DocumentScannerUtil.Instance.GetDeviceStatus();

                // update local ApplicationDevice_Status
                DAL_DeviceStatus dAL_DeviceStatus = new DAL_DeviceStatus();
                dAL_DeviceStatus.Update((int)EnumDeviceId.DocumentScanner, statuses);
                Trinity.SignalR.Client.Instance.DeviceStatusChanged((int)EnumDeviceId.DocumentScanner, statuses);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DocumentScannerMonitor.ReportDeviceStatus exception: " + ex.ToString());
            }
        }
    }
}
