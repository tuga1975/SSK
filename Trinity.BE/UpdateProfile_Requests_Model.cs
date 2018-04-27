using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{
    public class UpdateProfile_Requests_Model
    {
        public BE.UserProfile User_Profiles { get; set; }
        public BE.Address Alternate_Addresses { get; set; }
        public List<Guid> UploadedDocuments { get; set; }
    }
}
