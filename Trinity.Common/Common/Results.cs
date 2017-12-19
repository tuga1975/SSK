using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Common.Common
{
    public class DocumentScannerResult
    {
        public bool Success { get; set; }
        public object Value { get; set; }
        public FailedInfo FailedInfo { get; set; }

    }

    public class FailedInfo
    {
        public long ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class FingerprintScannerStartResult
    {
        public bool Success { get; set; }
        public FailedInfo FailedInfo { get; set; }
    }

    public class SCardReaderStartResult
    {
        public bool Success { get; set; }
        public FailedInfo FailedInfo { get; set; }
    }
}
