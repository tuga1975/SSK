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

                // report
                ApplicationStatusManager.Instance.ReportDeviceStatus(EnumDeviceId.DocumentScanner, statuses);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DocumentScannerMonitor.ReportDeviceStatus exception: " + ex.ToString());
            }
        }
    }
}
