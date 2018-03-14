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
using Trinity.Common;

namespace Trinity.BackendAPI.Controllers
{
    public class DeviceStatusController : ApiController
    {

        // PUT api/values/5
        //[Route("api/DeviceStatus/Update")]
        //[HttpPost]
        //public IHttpActionResult Update([FromBody] object[] data)
        //{
            
        //    try
        //    {
        //        // update local ApplicationDevice_Status
        //        DAL_DeviceStatus dAL_DeviceStatus = new DAL_DeviceStatus();
        //        bool updateResult = dAL_DeviceStatus.Update((int)data[0], (EnumDeviceStatuses[])data[1], data[2].ToString());

        //        // return value
        //        return Ok(updateResult);
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine("DeviceStatusController.Update exception" + ex.ToString());
        //        return Ok(false);
        //    }
        //}
    }
}
