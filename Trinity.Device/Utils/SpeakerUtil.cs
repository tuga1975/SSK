using NAudio.CoreAudioApi;
using System;

namespace Trinity.Device.Util
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

        public EnumDeviceStatus[] GetDeviceStatus()
        {
            // refer: https://stackoverflow.com/questions/33872895/detect-if-headphones-are-plugged-in-or-not-via-c-sharp

            var enumerator = new MMDeviceEnumerator();
            var endpoints = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

            // check state
            //foreach (var endpoint in endpoints)
            //{
            //    Debug.WriteLine("{0} - {1}", endpoint.DeviceFriendlyName, endpoint.State);
            //}

            if (endpoints.Count > 0)
            {
                return new EnumDeviceStatus[] { EnumDeviceStatus.Connected };
            }
            else
            {
                return new EnumDeviceStatus[] { EnumDeviceStatus.Disconnected };
            }
        }
    }
}
