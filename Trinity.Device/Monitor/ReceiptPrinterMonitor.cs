﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Trinity.DAL;
using Trinity.Device.Util;
using Trinity.Util;

namespace Trinity.Device
{
    public static class ReceiptPrinterMonitor
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
                Debug.WriteLine("ReceiptPrinterMonitor.Start exception: " + ex.ToString());
                return false;
            }
        }

        private static void ReportDeviceStatus()
        {
            try
            {
                // get statuses
                var statuses = ReceiptPrinterUtil.Instance.GetDeviceStatus();

                // report
                ApplicationStatusManager.Instance.ReportDeviceStatus(EnumDeviceId.ReceiptPrinter, statuses);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("ReceiptPrinterMonitor.ReportDeviceStatus exception: " + ex.ToString());
            }
        }
    }
}
