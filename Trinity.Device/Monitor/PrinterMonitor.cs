using ImageConvertor;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Trinity.Common;

namespace Trinity.Device
{
    public class PrinterMonitor
    {
        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile PrinterMonitor _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private PrinterMonitor() { }

        public static PrinterMonitor Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new PrinterMonitor();
                    }
                }

                return _instance;
            }
        }
        #endregion

        public event EventHandler<PrintMUBAndTTLabelsEventArgs> OnPrintMUBLabelSucceeded;
        public event EventHandler<ExceptionArgs> OnMonitorException;
        public event EventHandler<PrintMUBAndTTLabelsEventArgs> OnPrintTTLabelSucceeded;

        protected virtual void RaisePrintMUBLabelSucceededEvent(PrintMUBAndTTLabelsEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            OnPrintMUBLabelSucceeded?.Invoke(this, e);
        }

        protected virtual void RaiseMonitorExceptionEvent(ExceptionArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            OnMonitorException?.Invoke(this, e);
        }

        protected virtual void RaisePrintTTLabelSucceededEvent(PrintMUBAndTTLabelsEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            OnPrintTTLabelSucceeded?.Invoke(this, e);
        }

        public void PrintBarcodeLabel(LabelInfo labelInfo)
        {

            // validation
            if (string.IsNullOrEmpty(labelInfo.Name))
            {
                // username is null
                RaiseMonitorExceptionEvent(new ExceptionArgs(new FailedInfo()
                {
                    ErrorCode = (int)EnumErrorCodes.UserNameNull,
                    ErrorMessage = new ErrorInfo().GetErrorMessage(EnumErrorCodes.UserNameNull)
                }));

                return;
            }
            else if (string.IsNullOrEmpty(labelInfo.NRIC))
            {
                // NRIC is null
                RaiseMonitorExceptionEvent(new ExceptionArgs(new FailedInfo()
                {
                    ErrorCode = (int)EnumErrorCodes.NRICNull,
                    ErrorMessage = new ErrorInfo().GetErrorMessage(EnumErrorCodes.NRICNull)
                }));

                return;
            }


            // print label
            BarcodePrinterUtil printerUtils = BarcodePrinterUtil.Instance;
            TTLabelInfo infoTTLabel = new TTLabelInfo();
            infoTTLabel.ID = labelInfo.NRIC;
            infoTTLabel.Name = labelInfo.Name;
            infoTTLabel.MarkingNumber = labelInfo.MarkingNo;

            if (printerUtils.PrintTTLabel(infoTTLabel))
            {
                // raise succeeded event
                RaisePrintTTLabelSucceededEvent(new PrintMUBAndTTLabelsEventArgs(labelInfo));
            }
            else
            {
                // raise failed event
                RaiseMonitorExceptionEvent(new ExceptionArgs(new FailedInfo()
                {
                    ErrorCode = (int)EnumErrorCodes.UnknownError,
                    ErrorMessage = new ErrorInfo().GetErrorMessage(EnumErrorCodes.UnknownError)
                }));
            }
        }
        public void WriteLog(string content)
        {
            string path = @"c:\temp\MyTest.txt";

            // This text is added only once to the file.
            if (!File.Exists(path))
            {
                // Create a file to write to.
                string createText = "2018" + Environment.NewLine;
                File.WriteAllText(path, createText);
            }

            // This text is always added, making the file longer over time
            // if it is not deleted.
            string appendText = content + Environment.NewLine;
            File.AppendAllText(path, appendText);
        }

        public void PrintMUBLabel(LabelInfo labelInfo)
        {
            // validation
            if (string.IsNullOrEmpty(labelInfo.Name))
            {
                // username is null
                RaiseMonitorExceptionEvent(new ExceptionArgs(new FailedInfo()
                {
                    ErrorCode = (int)EnumErrorCodes.UserNameNull,
                    ErrorMessage = new ErrorInfo().GetErrorMessage(EnumErrorCodes.UserNameNull)
                }));

                return;
            }
            else if (string.IsNullOrEmpty(labelInfo.NRIC))
            {
                // NRIC is null
                RaiseMonitorExceptionEvent(new ExceptionArgs(new FailedInfo()
                {
                    ErrorCode = (int)EnumErrorCodes.NRICNull,
                    ErrorMessage = new ErrorInfo().GetErrorMessage(EnumErrorCodes.NRICNull)
                }));

                return;
            }

            // print label
            BarcodePrinterUtil printerUtils = BarcodePrinterUtil.Instance;

            // create image file to print
            string filePath = string.Empty;
            string fileName = string.Empty;
            using (var ms = new System.IO.MemoryStream(labelInfo.BitmapLabel))
            {
                Bitmap bitmap = new System.Drawing.Bitmap(System.Drawing.Image.FromStream(ms));
                string curDir = Directory.GetCurrentDirectory();

                // create directory
                if (!Directory.Exists(curDir + "\\Temp"))
                {
                    Directory.CreateDirectory(curDir + "\\Temp");
                }

                // set file path
                filePath = curDir + "\\Temp\\mublabel.bmp";

                // create image file (bit depth must be 8)
                Bitmap target = Convertor1.ConvertTo8bppFormat(bitmap);
                target.Save(filePath, ImageFormat.Bmp);
            }

            // print mub label
            if (printerUtils.PrintMUBLabel(filePath))
            {
                // raise succeeded event
                RaisePrintMUBLabelSucceededEvent(new PrintMUBAndTTLabelsEventArgs(labelInfo));
            }
            else
            {
                // raise failed event
                RaiseMonitorExceptionEvent(new ExceptionArgs(new FailedInfo()
                {
                    ErrorCode = (int)EnumErrorCodes.UnknownError,
                    ErrorMessage = new ErrorInfo().GetErrorMessage(EnumErrorCodes.UnknownError)
                }));
            }
        }
    }
}
