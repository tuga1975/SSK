using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace uhpSim.Models
{
    public partial class noticeRequest
    {
        public Nullable<System.DateTime> requestDate { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}