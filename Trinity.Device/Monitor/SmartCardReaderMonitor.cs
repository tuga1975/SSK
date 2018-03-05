using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCSC;
using Trinity.Common;
using Trinity.DAL;
using Trinity.BE;
using System.Threading;
using Trinity.Device.Util;

namespace Trinity.Device
{
    public static class SmartCardReaderMonitor
    {
        public static bool Start()
        {
            try
            {
                // start card reader monitor
                bool startMonitorResult = SmartCardReaderUtil.Instance.StartSmartCardReaderMonitor(OnInitialized, OnStatusChanged, OnMonitorException);

                if (startMonitorResult)
                {
                    // start health checker
                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                    Task task = Repeat.Interval(() => ReportDeviceStatus(), cancellationTokenSource.Token);
                }

                return startMonitorResult;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("SmartCardReaderMonitor.Start exception: " + ex.ToString());
                return false;
            }
        }

        private static void ReportDeviceStatus()
        {
            try
            {
                // get statuses
                var deviceStatuses = SmartCardReaderUtil.Instance.GetDeviceStatus();

                // update local ApplicationDevice_Status
                DAL_DeviceStatus dAL_DeviceStatus = new DAL_DeviceStatus();
                dAL_DeviceStatus.Update((int)EnumDeviceIds.SmartCardReader, deviceStatuses);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private static void OnMonitorException(object sender, DeviceMonitorExceptionEventArgs args)
        {
            // notify to duty officer
        }

        private static void OnStatusChanged(object sender, DeviceChangeEventArgs e)
        {
            // DetachedReaders
            //foreach (var removed in e.DetachedReaders)
            //{
            //}

            // AttachedReaders
            //foreach (var added in e.AttachedReaders)
            //{
            //}

            Update_DeviceStatus(e.AllReaders);
        }

        private static void OnInitialized(object sender, DeviceChangeEventArgs e)
        {
            Update_DeviceStatus(e.AllReaders);
        }

        private static void Update_DeviceStatus(IEnumerable<string> readers)
        {
            EnumDeviceStatuses[] deviceStatuses = new EnumDeviceStatuses[] { EnumDeviceStatuses.Connected };

            // check status
            if (readers == null || readers.Count() == 0 || !readers.Contains(EnumDeviceNames.SmartCardContactlessReader))
            {
                // disconnected
                deviceStatuses = new EnumDeviceStatuses[] { EnumDeviceStatuses.Disconnected };
            }

            // update local ApplicationDevice_Status
            DAL_DeviceStatus dAL_DeviceStatus = new DAL_DeviceStatus();
            dAL_DeviceStatus.Update((int)EnumDeviceIds.SmartCardReader, deviceStatuses);
        }
    }
}
