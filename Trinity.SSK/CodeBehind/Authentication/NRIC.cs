using SSK.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSK.CodeBehind.Authentication
{
    public class NRIC
    {
        WebBrowser _web;
        public event EventHandler<NavigateEventArgs> OnNavigate;
        public NRIC(WebBrowser web)
        {
            _web = web;
        }

        internal void Start()
        {
            _web.LoadPageHtml("Authentication/NRIC.html");
        }

        protected virtual void RaiseOnNavigateEvent(NavigateEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<NavigateEventArgs> handler = OnNavigate;

            // Event will be null if there are no subscribers
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }
    }
}
