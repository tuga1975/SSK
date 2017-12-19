using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


namespace Trinity.DAL.DBContext
{

    public partial class Appointment
    {
        public string GetDateTxt
        {
            get
            {
                return Date.ToString("dddd, dd MMM yyyy");
            }
        }
        public string FromTimeTxt
        {
            get
            {
                return FromTime.HasValue ? string.Format("{0:D2}:{1:D2}", FromTime.Value.Hours, FromTime.Value.Minutes): string.Empty;
            }
        }
        public string ToTimeTxt
        {
            get
            {
                return ToTime.HasValue ? string.Format("{0:D2}:{1:D2}", ToTime.Value.Hours, ToTime.Value.Minutes) : string.Empty;
            }
        }
    }
}
