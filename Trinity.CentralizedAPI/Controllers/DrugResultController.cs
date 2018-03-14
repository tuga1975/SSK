using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Trinity.BackendAPI.Controllers
{
    public class DrugResultController : ApiController
    {
        [HttpPost]
        [Route("api/DrugResult/UpdateDrugSeal")]
        public IHttpActionResult UpdateDrugSeal(string userId, string COCA, string BARB, string LSD, string METH, string MTQL, string PCP, string KET, string BUPRE, string CAT, string PPZ, string NPS, string updatedBy)
        {
            var result = new DAL.DAL_DrugResults().UpdateDrugSeal(userId, Convert.ToBoolean(COCA), Convert.ToBoolean(BARB), Convert.ToBoolean(LSD), Convert.ToBoolean(METH), Convert.ToBoolean(MTQL), 
                                                                Convert.ToBoolean(PCP), Convert.ToBoolean(KET), Convert.ToBoolean(BUPRE), Convert.ToBoolean(CAT), Convert.ToBoolean(PPZ), Convert.ToBoolean(NPS), updatedBy);
            return Ok(result);
        }

        [HttpPost]
        [Route("api/DrugResult/UpdateSealForUser")]
        public IHttpActionResult UpdateSealForUser(string userId, string seal, string uploadedBy, string sealedOrDiscardedBy)
        {
            var result = new DAL.DAL_DrugResults().UpdateSealForUser(userId, Convert.ToBoolean(seal), uploadedBy, sealedOrDiscardedBy);
            return Ok(result);
        }
    }
}
