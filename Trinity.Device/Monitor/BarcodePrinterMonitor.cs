using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Trinity.DAL;
using Trinity.Device.Util;

namespace Trinity.Device
{
    public static class MUBLabelPrinterMonitor
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
                Debug.WriteLine("MUBLabelPrinter.Start exception: " + ex.ToString());
                return false;
            }
        }

        private static void ReportDeviceStatus()
        {
            try
            {
                // get status
                var statuses = BarcodePrinterUtil.Instance.GetDeviceStatus(EnumDeviceNames.MUBLabelPrinter);
                
                // update local ApplicationDevice_Status
                DAL_DeviceStatus dAL_DeviceStatus = new DAL_DeviceStatus();
                dAL_DeviceStatus.Update((int)EnumDeviceIds.MUBLabelPrinter, statuses);
                Trinity.SignalR.Client.Instance.DeviceStatusChanged((int)EnumDeviceIds.MUBLabelPrinter, statuses);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("MUBLabelPrinter.ReportDeviceStatus exception: " + ex.ToString());
            }
        }
    }

    public static class UBLabelPrinterMonitor
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
                Debug.WriteLine("UBLabelPrinter.Start exception: " + ex.ToString());
                return false;
            }
        }

        private static void ReportDeviceStatus()
        {
            try
            {
                // get status
                var statuses = BarcodePrinterUtil.Instance.GetDeviceStatus(EnumDeviceNames.UBLabelPrinter);

                // update local ApplicationDevice_Status
                DAL_DeviceStatus dAL_DeviceStatus = new DAL_DeviceStatus();
                dAL_DeviceStatus.Update((int)EnumDeviceIds.UBLabelPrinter, statuses);
                Trinity.SignalR.Client.Instance.DeviceStatusChanged((int)EnumDeviceIds.UBLabelPrinter, statuses);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UBLabelPrinter.ReportDeviceStatus exception: " + ex.ToString());
            }
        }
    }

    public static class TTLabelPrinterMonitor
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
                Debug.WriteLine("TTLabelPrinter.Start exception: " + ex.ToString());
                return false;
            }
        }

        private static void ReportDeviceStatus()
        {
            try
            {
                // get status
                var statuses = BarcodePrinterUtil.Instance.GetDeviceStatus(EnumDeviceNames.TTLabelPrinter);

                // update local ApplicationDevice_Status
                DAL_DeviceStatus dAL_DeviceStatus = new DAL_DeviceStatus();
                dAL_DeviceStatus.Update((int)EnumDeviceIds.TTLabelPrinter, statuses);
                Trinity.SignalR.Client.Instance.DeviceStatusChanged((int)EnumDeviceIds.TTLabelPrinter, statuses);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TTLabelPrinter.ReportDeviceStatus exception: " + ex.ToString());
            }
        }
    }
}
