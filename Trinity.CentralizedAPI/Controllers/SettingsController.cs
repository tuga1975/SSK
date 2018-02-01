using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;

namespace Trinity.CentralizedAPI.Controllers
{
    public class SettingsController : ApiController
    {

        [HttpGet]
        [Route("api/Settings/GetCardInfo")]
        public BE.CardInfo GetCardInfo()
        {
            return new DAL.DAL_GetCardInfo().GetCardInfo();
        }

    }
}
