using PCSC;
using System;
using System.Diagnostics;

namespace Trinity.Util.Authentication
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
            Debug.WriteLine("onCardInitialized");
            string cardUID = SmartCardReaderUtil.Instance.GetCardUID();
            if (!string.IsNullOrEmpty(cardUID) && GetCardInfoSucceeded!=null)
            {
                GetCardInfoSucceeded(cardUID);
            }
            Debug.WriteLine($"Card UID: {cardUID}");
        }

        private void OnCardInserted(object sender, CardStatusEventArgs e)
        {
            Debug.WriteLine("onCardInitialized");
            string cardUID = SmartCardReaderUtil.Instance.GetCardUID();
            if (!string.IsNullOrEmpty(cardUID) && GetCardInfoSucceeded != null)
            {
                GetCardInfoSucceeded(cardUID);
            }
            Debug.WriteLine($"Card UID: {cardUID}");
        }

        private void OnCardRemoved(object sender, CardStatusEventArgs e)
        {
            Debug.WriteLine("OnCardRemoved");
        }
        
    }
    
}
