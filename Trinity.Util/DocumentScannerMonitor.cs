using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trinity.Common.Utils;
using Trinity.DAL;

namespace Trinity.Util
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
                var statuses = DocumentScannerUtils.Instance.GetDeviceStatus();

                // update local ApplicationDevice_Status
                DAL_DeviceStatus dAL_DeviceStatus = new DAL_DeviceStatus();
                dAL_DeviceStatus.Update((int)EnumDeviceIds.SmartCardPrinter, statuses);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DocumentScannerMonitor.ReportDeviceStatus exception: " + ex.ToString());
            }
        }
    }
}
