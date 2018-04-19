using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


namespace Trinity.DAL.DBContext
{

    public partial class DrugResult
    {
     public string Result
        {
            get
            {
                if (
                        (this.AMPH.HasValue && this.AMPH.Value) ||
                        (this.BENZ.HasValue && this.BENZ.Value) ||
                        (this.OPI.HasValue && this.OPI.Value) ||
                        (this.THC.HasValue && this.THC.Value)
                   )
                {
                    return EnumUTResult.POS;
                }
                else
                {
                    return EnumUTResult.NEG;
                }
            }
        }
    }
}
