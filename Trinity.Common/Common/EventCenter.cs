using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Common.Common
{
    // An event dispatch mechanism that enables the broadcast of information to registered observers.
    public class EventCenter
    {
        public event EventHandler<EventInfo> OnNewEvent;

        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile EventCenter _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private EventCenter() { }

        public static EventCenter Default
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new EventCenter();
                    }
                }
                return _instance;
            }
        }
        #endregion

        public void RaiseEvent(EventInfo eventInfo)
        {
            OnNewEvent?.Invoke(this, eventInfo);
        }
    }
}
