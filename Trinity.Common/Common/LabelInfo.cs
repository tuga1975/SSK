using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Common
{
    public class LabelInfo
    {
        public string Label_Type { get; set; }
        
        public string CompanyName { get; set; }
        
        public string MarkingNo { get; set; }
        
        public string DrugType { get; set; }
        
        public string UserId { get; set; }
        
        public string NRIC { get; set; }
        
        public string Name { get; set; }
        
        public string Date { get; set; }
        
        public byte[] QRCode { get; set; }
        
        public string LastStation { get; set; }
        
        public int PrintCount { get; set; }
        
        public string ReprintReason { get; set; }

        public byte[] BitmapLabel { get; set; }

        public bool IsMUB { get; set; }

        public bool IsTT { get; set; }
    }

    public class TTLabelInfo
    {
        /// <summary>
        /// not null
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// not null
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// not null
        /// </summary>
        public string MarkingNumber { get; set; }

        internal bool IsValid()
        {
            if (string.IsNullOrEmpty(ID) || string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(MarkingNumber))
            {
                return false;
            }

            return true;
        }
    }

    public class MUBLabelInfo
    {
        /// <summary>
        /// not null
        /// </summary>
        public string ID { get; set; }
        /// <summary>
        /// not null
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// not null
        /// </summary>
        public string MarkingNumber { get; set; }

        internal bool IsValid()
        {
            if (string.IsNullOrEmpty(ID) || string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(MarkingNumber))
            {
                return false;
            }

            return true;
        }
    }

    public class AppointmentDetails
    {
        public string Name { get; set; }
        public string Venue { get; set; }
        public DateTime Date { get; set; }

        internal bool IsValid()
        {
            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Venue))
            {
                return false;
            }

            return true;
        }
    }
}
