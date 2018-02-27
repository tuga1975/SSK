using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Trinity.CentralizedAPI.Controllers
{
    public class DrugResultController : ApiController
    {
        [HttpPost]
        [Route("api/Label/UpdateDrugSeal")]
        public IHttpActionResult UpdateDrugSeal(string userId, string COCA, string BARB, string LSD, string METH, string MTQL, string PCP, string KET, string BUPRE, string CAT, string PPZ, string NPS)
        {
            var result = new DAL.DAL_DrugResults().UpdateDrugSeal(userId, Convert.ToBoolean(COCA), Convert.ToBoolean(BARB), Convert.ToBoolean(LSD), Convert.ToBoolean(METH), Convert.ToBoolean(MTQL), 
                                                                Convert.ToBoolean(PCP), Convert.ToBoolean(KET), Convert.ToBoolean(BUPRE), Convert.ToBoolean(CAT), Convert.ToBoolean(PPZ), Convert.ToBoolean(NPS));
            return Ok(result);
        }
    }
}
