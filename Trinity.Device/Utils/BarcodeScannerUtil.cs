using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Trinity.Common;

namespace Trinity.Device.Util
{
    public class BarcodeScannerUtil
    {
        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile BarcodeScannerUtil _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private BarcodeScannerUtil() { }

        public static BarcodeScannerUtil Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new BarcodeScannerUtil();
                    }
                }

                return _instance;
            }
        }
        #endregion

        public EnumDeviceStatus[] GetDeviceStatus()
        {
            return new EnumDeviceStatus[] { EnumDeviceStatus.Disconnected };
        }
    }
}
