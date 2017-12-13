using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{
    public class ProfileModel
    {
        public User User { get; set; }
        public UserProfile UserProfile { get; set; }
        public Address Addresses { get; set; }

        public ProfileModel()
        {

        }


    }

    [Serializable]
    public class Address
    {
        [DataMember]
        public int Address_ID { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string BlkHouse_Number { get; set; }
        [DataMember]
        public string FlrUnit_Number { get; set; }
        [DataMember]
        public string Street_Name { get; set; }
        [DataMember]
        public string Country { get; set; }
        [DataMember]
        public string Postal_Code { get; set; }

    }

}
