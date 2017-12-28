using System;
using System.Collections.Generic;

namespace Trinity.Common
{
    public class Session
    {
        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile Session _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private Session() { }

        public static Session Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new Session();
                    }
                }

                return _instance;
            }
        }
        #endregion

        public bool IsSmartCardAuthenticated { get; set; }

        public bool IsFingerprintAuthenticated { get; set; }

        public bool IsUserNamePasswordAuthenticated { get; set; }

        public bool IsAuthenticated => (IsSmartCardAuthenticated & IsFingerprintAuthenticated) | IsUserNamePasswordAuthenticated;

        public string Role { get; set; }

        private Dictionary<string, object> _properties = new Dictionary<string, object>();
        #region Public methods
        public object this[string key]
        {
            get
            {
                if (_properties.ContainsKey(key))
                {
                    return _properties[key];
                }
                else
                {
                    return null;
                }
            }
            set
            {
                _properties[key] = value;
            }
        }
        #endregion
    }
}
