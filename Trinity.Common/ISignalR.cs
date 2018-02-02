using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface ISignalR
{
    void UserLogined(string userID);
    void UserLogout(string userID);
    void DeviceStatusUpdate(int deviceId, EnumDeviceStatuses[] deviceStatuses);
}
