using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Newtonsoft.Json;
using Trinity.Common;

namespace Trinity.BackendAPI.Controllers
{

    

    [Route("api/TestJob/{Action}")]
    public class TestJobController : ApiController
    {
        [HttpGet]
        public void JobReminder()
        {
            Trinity.BackendAPI.ScheduledTask.JobReminder jobReminder = new ScheduledTask.JobReminder();
            jobReminder.Execute(null);
        }
        [HttpGet]
        public void JobAbsent()
        {
            Trinity.BackendAPI.ScheduledTask.JobAbsent jobAbsent = new ScheduledTask.JobAbsent();
            jobAbsent.Execute(null);
        }

        [HttpGet]
        public async void CheckSendNoti()
        {
            
            await System.Threading.Tasks.Task.Run(() => Trinity.SignalR.Client.Instance.SSACompleted("AAAA"));
        }
    }
}
