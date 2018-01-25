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

namespace Trinity.Util
{
    public static class SmartCardReaderMonitor
    {
        public static bool Start()
        {
            try
            {
                // start card reader monitor
                bool startMonitorResult = SmartCardReaderUtils.Instance.StartSmartCardReaderMonitor(OnInitialized, OnStatusChanged, OnMonitorException);

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
                Debug.WriteLine("SmartCardReaderMonitor.ReportDeviceStatus " + DateTime.Now.ToString());
                var deviceStatus = SmartCardReaderUtils.Instance.GetDeviceStatus();
                Update_DeviceStatus(deviceStatus);
            }
            catch (Exception)
            {

                throw;
            }
        }

        private static void Update_DeviceStatus(EnumDeviceStatuses[] status)
        {
            // create entity
            string station = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            DeviceStatus deviceStatus = new DeviceStatus()
            {
                DeviceID = (int)EnumDeviceIds.SmartCardReader,
                Station = station,
                StatusCode = status
            };

            // update local ApplicationDevice_Status
            DAL_DeviceStatus dAL_DeviceStatus = new DAL_DeviceStatus();
            dAL_DeviceStatus.Update(deviceStatus);

            // update centralized db ApplicationDevice_Status
            // if update failed, notify duty officer
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

            // check status
            if (readers == null || readers.Count() == 0 || !readers.Contains(EnumDeviceNames.SmartCardContactlessReader))
            {
                // disconnected
                Update_DeviceStatus(new EnumDeviceStatuses[] { EnumDeviceStatuses.Disconnected });
            }
            else
            {
                // connected
                Update_DeviceStatus(new EnumDeviceStatuses[] { EnumDeviceStatuses.Connected });
            }
        }
    }
}
