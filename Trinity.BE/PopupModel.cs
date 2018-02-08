using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{
    public class PopupModel
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsShowLoading { get; set; }
        public bool IsShowOK { get; set; }
    }
}
