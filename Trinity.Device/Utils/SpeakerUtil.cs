using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Trinity.Common;

namespace Trinity.Util
{
    public class SpeakerUtil
    {
        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile SpeakerUtil _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private SpeakerUtil() { }

        public static SpeakerUtil Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new SpeakerUtil();
                    }
                }

                return _instance;
            }
        }
        #endregion

        public EnumDeviceStatuses[] GetDeviceStatus()
        {
            return new EnumDeviceStatuses[] { EnumDeviceStatuses.Disconnected };
        }
    }
}
