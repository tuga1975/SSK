using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{
    public class IssueCard
    {
        public string UserId { get; set; }
        public string SmartCardId { get; set; }
        public string NRIC { get; set; }
        public string Name { get; set; }
        public string Serial_Number { get; set; }
        public Nullable<System.DateTime> Date_Of_Issue { get; set; }

        public string Date_Of_Issue_Txt
        {
            get
            {
                return Date_Of_Issue.HasValue ? Date_Of_Issue.Value.ToString("dd/MM/yyyy") : string.Empty;
            }
        }
        public Nullable<System.DateTime> Expired_Date { get; set; }
        public string Expired_Date_Txt
        {
            get
            {
                return Expired_Date.HasValue ? Expired_Date.Value.ToString("dd/MM/yyyy") : string.Empty;
            }
        }

        public string Status { get; set; }
        public string CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public string Reprint_Reason { get; set; }
    }
}
