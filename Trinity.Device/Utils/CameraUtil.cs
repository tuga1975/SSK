﻿using DirectShowLib;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Trinity.Common;

namespace Trinity.Device.Util
{
    public class CameraUtil
    {
        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile CameraUtil _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private CameraUtil() { }

        public static CameraUtil Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new CameraUtil();
                    }
                }

                return _instance;
            }
        }
        #endregion

        public EnumDeviceStatus[] GetDeviceStatus()
        {
            // refer: https://web.archive.org/web/20140716211944/http://www.barebonescoder.com/2012/01/finding-your-web-cam-with-c-directshow-net/
            DsDevice[] videoInputDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

            // connected
            if (videoInputDevices != null && videoInputDevices.Length > 0)
            {
                return new EnumDeviceStatus[] { EnumDeviceStatus.Connected };
            }
            else
            {
                // disconnected
                return new EnumDeviceStatus[] { EnumDeviceStatus.Disconnected };
            }
        }
    }
}
