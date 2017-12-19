using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Common.Common
{
    public class ExceptionArgs
    {
        public long ErrorCode { get; set; }
        public string ErrorMessage { get; set; }

        public ExceptionArgs() { }

        public ExceptionArgs(FailedInfo failedInfo)
        {
            ErrorCode = failedInfo.ErrorCode;
            ErrorMessage = failedInfo.ErrorMessage;
        }
    }
}
