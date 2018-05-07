using PCSC;
using System;
using System.Diagnostics;
using Trinity.Common.Utils;
using Trinity.Device.Util;

namespace Trinity.Device.Authentication
{
    public class SmartCard
    {
        public event Action<string> GetCardInfoSucceeded;

        #region Singleton Implementation
        private static volatile SmartCard _instance;

        private static object syncRoot = new Object();

        private SmartCard()
        {

        }

        public static SmartCard Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new SmartCard();
                    }
                }
                return _instance;
            }
        }
        #endregion

        public void Start()
        {
            // StartSmartCardMonitor
            SmartCardReaderUtil.Instance.StartSmartCardMonitor(OnCardInitialized, OnCardInserted, OnCardRemoved);
        }


        private void OnCardInitialized(object sender, CardStatusEventArgs e)
        {
            LogManager.Debug("OnCardInitialized...");
            string cardUID = SmartCardReaderUtil.Instance.GetCardUID();
            LogManager.Debug("Smart Card UID: " + cardUID);

            if (!string.IsNullOrEmpty(cardUID) && GetCardInfoSucceeded != null)
            {
                GetCardInfoSucceeded(cardUID);
            }
        }

        private void OnCardInserted(object sender, CardStatusEventArgs e)
        {
            LogManager.Debug("OnCardInserted...");
            string cardUID = SmartCardReaderUtil.Instance.GetCardUID();
            LogManager.Debug("Smart Card UID: " + cardUID);
            if (!string.IsNullOrEmpty(cardUID) && GetCardInfoSucceeded != null)
            {
                GetCardInfoSucceeded(cardUID);
            }
        }

        private void OnCardRemoved(object sender, CardStatusEventArgs e)
        {
            LogManager.Debug("OnCardRemoved");
        }
    }
}
