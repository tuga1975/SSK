using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


namespace Trinity.DAL.DBContext
{

    public partial class Timeslot
    {

        public int SortCategory
        {
            get
            {
                if (this.Category == EnumTimeshift.Morning)
                    return 0;
                else if (this.Category == EnumTimeshift.Afternoon)
                    return 1;
                else
                    return 2;
            }
        }
    }
}
