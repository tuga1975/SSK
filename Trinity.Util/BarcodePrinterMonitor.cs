using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trinity.BE;
using Trinity.Common.Utils;
using Trinity.DAL;

namespace Trinity.Util
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
                var status = BarcodePrinterUtils.Instance.GetDeviceStatus(EnumDeviceNames.MUBLabelPrinter);
                
                // update local ApplicationDevice_Status
                DAL_DeviceStatus dAL_DeviceStatus = new DAL_DeviceStatus();
                dAL_DeviceStatus.Update((int)EnumDeviceIds.MUBLabelPrinter, status);
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
                var status = BarcodePrinterUtils.Instance.GetDeviceStatus(EnumDeviceNames.UBLabelPrinter);

                // update local ApplicationDevice_Status
                DAL_DeviceStatus dAL_DeviceStatus = new DAL_DeviceStatus();
                dAL_DeviceStatus.Update((int)EnumDeviceIds.UBLabelPrinter, status);
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
                var status = BarcodePrinterUtils.Instance.GetDeviceStatus(EnumDeviceNames.TTLabelPrinter);

                // update local ApplicationDevice_Status
                DAL_DeviceStatus dAL_DeviceStatus = new DAL_DeviceStatus();
                dAL_DeviceStatus.Update((int)EnumDeviceIds.TTLabelPrinter, status);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("TTLabelPrinter.ReportDeviceStatus exception: " + ex.ToString());
            }
        }
    }
}
