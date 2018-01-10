using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;


namespace Trinity.DAL.DBContext
{

    public partial class QueueDetail
    {
        public string Color
        {
            get
            {
                MemberInfo member =  typeof(EnumQueueStatuses).GetMembers(
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.Static |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly).Where(d => d.Name == this.Status).FirstOrDefault();
                if (member != null)
                {
                    CustomAttribute cusAttr =  member.GetMyCustomAttributes();
                    if (cusAttr != null)
                        return cusAttr.Color;
                }
                return "white";
            }
        }
    }
}
