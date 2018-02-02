using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Trinity.BE;
using Trinity.DAL;

namespace Trinity.CentralizedAPI.Controllers
{
    public class DeviceStatusController : ApiController
    {

        // PUT api/values/5
        [Route("Update")]
        [HttpPost]
        [ResponseType(responseType: typeof(BE.ResponseModel))]
        public IHttpActionResult Update(int deviceId, EnumDeviceStatuses[] deviceStatuses)
        {
            try
            {
                // update local ApplicationDevice_Status
                DAL_DeviceStatus dAL_DeviceStatus = new DAL_DeviceStatus();
                bool updateResult = dAL_DeviceStatus.Update(deviceId, deviceStatuses);

                // return value
                return Ok(updateResult);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DeviceStatusController.Update exception" + ex.ToString());
                return Ok(false);
            }
        }
    }
}
