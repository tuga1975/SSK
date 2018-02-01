using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class ProfileConnected
{
    public string Station { get; set; }
    public string UserID { get; set; }
    public HubCallerContext Context { get; set; }
    public string ConnectionId
    {
        get
        {
            return Context.ConnectionId;
        }
    }
    public bool isApp
    {
        get
        {
            if (!string.IsNullOrEmpty(Station) && string.IsNullOrEmpty(UserID))
                return true;
            return false;
        }
    }
    public bool isUser
    {
        get
        {
            if (!string.IsNullOrEmpty(UserID))
                return true;
            return false;
        }
    }
    public Nullable<DateTime> DateOffline { get; set; }
    public double HoursOffline
    {
        get
        {
            if (DateOffline.HasValue)
                return (DateTime.Now - DateOffline.Value).TotalHours;
            return 0;
        }
    }
    public bool isOffline
    {
        get
        {
            if (DateOffline.HasValue)
                return true;
            return false;
        }
    }
}
