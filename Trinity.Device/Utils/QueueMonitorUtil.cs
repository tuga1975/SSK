using System;
using System.Linq;
using System.Windows.Forms;

namespace Trinity.Device.Util
{
    public class QueueMonitorUtil
    {
        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile QueueMonitorUtil _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private QueueMonitorUtil() { }

        public static QueueMonitorUtil Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new QueueMonitorUtil();
                    }
                }

                return _instance;
            }
        }
        #endregion

        public EnumDeviceStatuses[] GetDeviceStatus()
        {
            // count all screens, if > 1, its connected
            if (Screen.AllScreens.Count() > 1)
            {
                return new EnumDeviceStatuses[] { EnumDeviceStatuses.Connected };
            }
            else
            {
                return new EnumDeviceStatuses[] { EnumDeviceStatuses.Disconnected };
            }
        }
    }
}
