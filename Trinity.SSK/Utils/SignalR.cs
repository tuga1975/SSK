﻿using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using Trinity.BE;
using Trinity.Common;
using Trinity.DAL;

namespace SSK.Utils
{
    class SignalR : Trinity.Utils.Notification.SignalRBase
    {
        public SignalR()
        {
            StartConnect();
        }
        public override void IncomingEvents()
        {
            HubProxy.On<Queue>("QueueFinished", (queue) => {
                // Xử lý status device
            });
        }
        public override void Connection_Closed()
        {

        }
        
    }
}
