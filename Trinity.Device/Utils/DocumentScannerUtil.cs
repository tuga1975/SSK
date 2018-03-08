using System;

namespace Trinity.Util
{
    public class DocumentScannerUtil
    {
        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile DocumentScannerUtil _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private DocumentScannerUtil() { }

        public static DocumentScannerUtil Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new DocumentScannerUtil();
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
