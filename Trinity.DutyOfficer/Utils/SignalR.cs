﻿using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using Trinity.BE;
using Trinity.Common;
using Trinity.DAL;

namespace DutyOfficer.Utils
{
    class SignalR:Trinity.Utils.Notification.SignalRBase
    {
        public override void IncomingEvents()
        {
            
        }
        public override void Connection_Closed()
        {
            
        }

    }
}
