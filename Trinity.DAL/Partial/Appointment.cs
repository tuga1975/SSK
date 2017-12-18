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
    }
}
