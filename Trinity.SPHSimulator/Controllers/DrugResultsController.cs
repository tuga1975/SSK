using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using uhpSim;
using uhpSim.Models;
using System.Globalization;
using Newtonsoft.Json;

namespace uhpSim.Controllers
{
    public class DrugResultsController : ApiController
    {
        private UHPSimulatorIIEntities db = new UHPSimulatorIIEntities();
        [HttpPost]
        [Route("api/uhp-sim/drugresult")]
        public IEnumerable<USP_DrugResult_SELECT_MarkingNumber_Result> Get([FromBody] resultRequest request)
        {
            string[] lines = { "==================", "Requested Time: " + DateTime.Now.ToString(), "Requested markingnumber:" + request.markingnumber, "==================" };
            System.IO.File.AppendAllLines(System.Web.Hosting.HostingEnvironment.MapPath("~/STE") + @"\DrugResults.log", lines);
            return db.USP_DrugResult_SELECT_MarkingNumber(request.markingnumber).AsEnumerable();
        }
    }
}
