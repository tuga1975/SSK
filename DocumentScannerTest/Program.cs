using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Common.Common;
using Trinity.Common.DeviceMonitor;

namespace DocumentScannerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("DocumentScannerTest is starting...");

            DocumentScannerMonitor documentScannerMonitor = DocumentScannerMonitor.Instance;
            documentScannerMonitor.OnMonitorException += OnMonitorException;
            documentScannerMonitor.OnMonitorException -= OnMonitorException;
            documentScannerMonitor.OnMonitorException += OnMonitorException;
            documentScannerMonitor.OnMonitorException += OnMonitorException;
            var result = documentScannerMonitor.ScanDocument();

            if (result != null)
            {
                Console.WriteLine("Scan document is completed.");
            }

            Console.ReadKey();
        }

        private static void OnMonitorException(object sender, ExceptionArgs e)
        {
            Console.Write($"{e.ErrorCode}: {e.ErrorMessage}");
        }
    }
}
