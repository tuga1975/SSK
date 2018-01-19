﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{
    public class Statistics
    {
        public int Timeslot_ID { get; set; }
        public Nullable<System.TimeSpan> StartTime { get; set; }
        
        public Nullable<System.TimeSpan> EndTime { get; set; }

        public int? Max { get; set; }

        public int? Booked { get; set; }

        public int? Reported { get; set; }

        public int? No_Show { get; set; }

        public int? Available { get; set; }

        public Nullable<System.DateTime> Date { get; set; }
    }
}