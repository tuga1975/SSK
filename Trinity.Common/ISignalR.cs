using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface ISignalR
{
    void UserLoggedIn(string userId);
    void UserLoggedOut(string userId);
    void DeviceStatusChanged(int deviceId, EnumDeviceStatuses[] deviceStatuses);
    void SendToDutyOfficer(string fromUserId, string dutyOfficerID, string subject, string content, string notificationType);
    void SendToAllDutyOfficers(string fromUserId, string subject, string content, string notificationType);
}
