﻿using PCSC;
using PCSC.Iso7816;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Trinity.Common.Common;

namespace Trinity.Common.Monitor
{
    public class SCardMonitor
    {
        SmartCardReaderUtils _smartCardReaderUtils = SmartCardReaderUtils.Instance;

        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile SCardMonitor _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private SCardMonitor() { }

        public static SCardMonitor Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new SCardMonitor();
                    }
                }

                return _instance;
            }
        }
        #endregion


        // Start monitor
        public void Start()
        {
            // start a thread for health check

            // StartSmartCardReaderMonitor
            bool startCardReaderMonitorResult = _smartCardReaderUtils.StartSmartCardReaderMonitor();

            // if CardReaderMonitor is false, start a thread to restart
        }

        public SCardReaderStartResult StartCardMonitor(CardInitializedEvent onCardInitialized, CardInsertedEvent onCardInserted, CardRemovedEvent onCardRemoved)
        {
            return _smartCardReaderUtils.StartSmartCardMonitor(onCardInitialized, onCardInserted, onCardRemoved);
        }

        public string GetCardUID()
        {
            return _smartCardReaderUtils.GetCardUID();
        }

        public void Stop()
        {
            _smartCardReaderUtils.StopSmartCardMonitor();
        }
    }

    //public class SCardMonitor_Old
    //{
    //    static SmartCardReaderUtils _smartCardReaderUtils;
    //    static bool _smartCardReaderMonitorStarted;
    //    static bool _smartCardMonitorStarted;

    //    #region Monitor Smart Card

    //    public static void Start()
    //    {
    //        //Start StartSmartCardReaderMonitor thread
    //        _smartCardReaderUtils = new SmartCardReaderUtils();
    //        Thread thread = new Thread(new ThreadStart(() => _smartCardReaderMonitorStarted = _smartCardReaderUtils.StartSmartCardReaderMonitor()));
    //        thread.Start();
    //    }

    //    public static void StartCardMonitor(CardInitializedEvent onCardInitialized, CardInsertedEvent onCardInserted, CardRemovedEvent onCardRemoved)
    //    {
    //        //Thread threadMonitor = new Thread(new ParameterizedThreadStart(MonitorCard));
    //        Thread thread = new Thread(new ThreadStart(() => _smartCardMonitorStarted = _smartCardReaderUtils.StartSmartCardMonitor(onCardInitialized, onCardInserted, onCardRemoved)));
    //        thread.Start();
    //    }

    //    #endregion

    //    public static string GetCardUID()
    //    {
    //        if (!_smartCardReaderMonitorStarted)
    //        {
    //            Debug.WriteLine("SCardMonitor first please...");
    //            return string.Empty;
    //        }

    //        return _smartCardReaderUtils.GetCardUID();
    //    }

    //    public static void Stop()
    //    {
    //        if (!_smartCardReaderMonitorStarted)
    //        {
    //            return;
    //        }

    //        _smartCardReaderUtils.StopSCardMonitor();
    //    }
    //}
}
