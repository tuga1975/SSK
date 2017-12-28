using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Common.Common
{
    public class EventCenter
    {
        public event EventHandler<NRICEventArgs> OnNRICFailed;
        public event EventHandler<ShowMessageEventArgs> OnShowMessage;
        public event Action OnLogOutCompleted;
        public event Action OnLogInSucceeded;
        public event EventHandler<LoginEventArgs> OnLogInFailed;

        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile EventCenter _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private EventCenter() { }

        public static EventCenter CreateEventCenter()
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
        #endregion

        #region Raise virtual events
        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        public virtual void RaiseOnNRICFailedEvent(NRICEventArgs e)
        {
            OnNRICFailed?.Invoke(this, e);
        }

        public virtual void RaiseOnShowMessageEvent(ShowMessageEventArgs e)
        {
            OnShowMessage?.Invoke(this, e);
        }

        public virtual void RaiseLogInSucceededEvent()
        {
            OnLogInSucceeded?.Invoke();
        }

        public virtual void RaiseLogInFailedEvent(LoginEventArgs e)
        {
            OnLogInFailed?.Invoke(this, e);
        }

        public virtual void RaiseLogOutCompletedEvent()
        {
            OnLogOutCompleted?.Invoke();
        }
        #endregion
    }
}
