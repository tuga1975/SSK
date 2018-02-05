using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using Trinity.BE;
using Trinity.Common;
using Trinity.DAL;

namespace DutyOfficer.Utils
{
    class SignalR:Trinity.Utils.Notification.SignalRBase
    {
        public SignalR()
        {
            StartConnect();
        }
        public override void IncomingEvents()
        {
            //
            HubProxy.On<int, EnumDeviceStatuses[]>("DeviceStatusUpdate", (deviceId, deviceStatuses) => {
                // Xử lý status device
            });
        }

        public override void Connection_Closed()
        {
            
        }


    }
}
