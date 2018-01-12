using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{
    [Serializable]

    public class Setting
    {
        [DataMember]
        public string Setting_ID { get; set; }

        [DataMember]
        public int WeekNum { get; set; }

        [DataMember]
        public int Year { get; set; }

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public DateTime Mon_Open_Time { get; set; }

        [DataMember]
        public DateTime Mon_Close_Time { get; set; }

        [DataMember]
        public int Mon_Interval { get; set; }

        [DataMember]
        public int Mon_MaximumNum { get; set; }

        [DataMember]
        public int Mon_ReservedForSpare { get; set; }


        [DataMember]
        public DateTime Tue_Open_Time { get; set; }

        [DataMember]
        public DateTime Tue_Close_Time { get; set; }

        [DataMember]
        public int Tue_Interval { get; set; }

        [DataMember]
        public int Tue_MaximumNum { get; set; }

        [DataMember]
        public int Tue_ReservedForSpare { get; set; }

        [DataMember]
        public DateTime Wed_Open_Time { get; set; }

        [DataMember]
        public DateTime Wed_Close_Time { get; set; }

        [DataMember]
        public int Wed_Interval { get; set; }

        [DataMember]
        public int Wed_MaximumNum { get; set; }

        [DataMember]
        public int Wed_ReservedForSpare { get; set; }

        [DataMember]
        public DateTime Thu_Open_Time { get; set; }

        [DataMember]
        public DateTime Thu_Close_Time { get; set; }

        [DataMember]
        public int Thu_Interval { get; set; }

        [DataMember]
        public int Thu_MaximumNum { get; set; }

        [DataMember]
        public int Thu_ReservedForSpare { get; set; }

        [DataMember]
        public DateTime Fri_Open_Time { get; set; }

        [DataMember]
        public DateTime Fri_Close_Time { get; set; }

        [DataMember]
        public int Fri_Interval { get; set; }

        [DataMember]
        public int Fri_MaximumNum { get; set; }

        [DataMember]
        public int Fri_ReservedForSpare { get; set; }

        [DataMember]
        public DateTime Sat_Open_Time { get; set; }

        [DataMember]
        public DateTime Sat_Close_Time { get; set; }

        [DataMember]
        public int Sat_Interval { get; set; }

        [DataMember]
        public int Sat_MaximumNum { get; set; }

        [DataMember]
        public int Sat_ReservedForSpare { get; set; }

        [DataMember]
        public DateTime Sun_Open_Time { get; set; }

        [DataMember]
        public DateTime Sun_Close_Time { get; set; }

        [DataMember]
        public int Sun_Interval { get; set; }

        [DataMember]
        public int Sun_MaximumNum { get; set; }

        [DataMember]
        public int Sun_ReservedForSpare { get; set; }

        [DataMember]
        public string Last_Updated_By { get; set; }

        [DataMember]
        public DateTime Last_Updated_Date{ get; set; }

        [DataMember]
        public string Description { get; set; }
    }
}
