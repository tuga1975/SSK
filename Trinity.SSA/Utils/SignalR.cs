using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using Trinity.BE;
using Trinity.Common;
using Trinity.DAL;

namespace SSA.Utils
{
    class SignalR : Trinity.SignalR.Client.Notification.SignalRBase
    {
        public SignalR()
        {
            StartConnect();
        }
        public override void IncomingEvents()
        {

        }
        public override void Connection_Closed()
        {

        }

        public void QueueFinished(string UserId)
        {
            if (IsConnected)
            {
                Queue queue = new DAL_QueueNumber().GetMyQueueToday(UserId);
                HubProxy.Invoke("QueueFinished", queue);
            }
        }


    }
}
