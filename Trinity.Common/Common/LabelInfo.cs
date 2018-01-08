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
        
        public DateTime? Date { get; set; }
        
        public byte[] QRCode { get; set; }
        
        public string LastStation { get; set; }
        
        public int PrintCount { get; set; }
        
        public string ReprintReason { get; set; }
    }
}
