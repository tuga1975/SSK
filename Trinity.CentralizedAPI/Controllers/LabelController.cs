using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Trinity.CentralizedAPI.Controllers
{
    public class LabelController : ApiController
    {
        [HttpGet]
        [Route("api/Label/GetSettingSystemByYear")]
        public IHttpActionResult GetSettingSystemByYear(string year)
        {
            int _year = 0;
            if (Int32.TryParse(year, out _year))
            {
                var result = new DAL.DAL_SettingSystem().GetSettingSystemByYear(_year);
                return Ok(result);
            }
            else
            {
                return null;
            }

        }

        [HttpPost]
        [Route("api/Label/UpdateLabel")]
        public IHttpActionResult UpdateLabel(BE.Label label)
        {
            var result = new DAL.DAL_Labels().UpdateLabel(label);
            return Ok(result);
        }

        [HttpGet]
        [Route("api/Label/GetByDateAndUserId")]
        public IHttpActionResult GetByDateAndUserId(string date, string UserId)
        {
            DateTime _date = Convert.ToDateTime(date);
            var result = new DAL.DAL_Labels().GetByDateAndUserId(_date, UserId);
            return Ok(result);

        }

        [HttpGet]
        [Route("api/Label/GetAllLabelsForMUBAndTT")]
        public IHttpActionResult GetAllLabelsForMUBAndTT()
        {
            var result = new DAL.DAL_Labels().GetAllLabelsForMUBAndTT();
            return Ok(result);

        }

        [HttpGet]
        [Route("api/Label/GetAllLabelsForUB")]
        public IHttpActionResult GetAllLabelsForUB()
        {
            var result = new DAL.DAL_Labels().GetAllLabelsForMUBAndTT();
            return Ok(result);

        }
    }
}
