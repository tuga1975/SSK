using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using Trinity.BE;
using Trinity.Common;
using Trinity.DAL;

namespace Enrolment.Utils
{
    class SignalR : Trinity.SignalRClient.Notification.SignalRBase
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
    }
}
