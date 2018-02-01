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
        [Route("api/Settings/GetCardNumber")]
        public BE.CardNumber GetCardNumber()
        {
            return new DAL.DAL_GetCardNumber().GetCardNumber();
        }

    }
}
