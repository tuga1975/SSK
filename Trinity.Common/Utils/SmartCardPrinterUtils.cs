using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Trinity.Common.Common;
using ZMOTIFPRINTERLib;
using ZMTGraphics;

namespace Trinity.Common.Utils
{
    public class SmartCardPrinterUtils : DeviceUtils
    {
        private string _smartCardPrinterName;
        private short _alarm = 0;

        private struct JobStatusStruct
        {
            public int copiesCompleted,
                          copiesRequested,
                          errorCode;
            public string cardPosition,
                          contactlessStatus,
                          contactStatus,
                          magStatus,
                          printingStatus,
                          uuidJob;
        }

        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile SmartCardPrinterUtils _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private SmartCardPrinterUtils()
        {
            _smartCardPrinterName = ConfigurationManager.AppSettings["TTLabelPrinterName"];
        }

        public static SmartCardPrinterUtils Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new SmartCardPrinterUtils();
                    }
                }

                return _instance;
            }
        }
        #endregion

        public void PrintAndWriteSmartcardData(PrintAndWriteSmartCardInfo printAndWriteSmartcardInfo, Action<PrintAndWriteSmartcardResult> OnCompleted)
        {
            try
            {
                PrintAndWriteSmartcardResult printAndWriteSmartcardResult = PrintCard(printAndWriteSmartcardInfo);

                OnCompleted(printAndWriteSmartcardResult);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("PrintAndWriteSmartcardData exception: " + ex.ToString());

                OnCompleted(new PrintAndWriteSmartcardResult() { Success = false, Description = "Oops!.. Something went wrong ..." });
            }
        }

        public void PrintLabel(string frontImagePath, string backImagePath, Action<bool> OnCompleted)
        {
            string printerName = EnumDeviceNames.SmartCardPrinterSerialNumber;

            bool bRet = false;
            byte[] img = null;
            byte[] bmpFront = null;
            byte[] bmpBack = null;
            Job job = null;
            ZMotifGraphics g = null;
            try
            {
                // Opens a connection with a ZXP Printer
                //     if it is in an alarm condition, exit function
                // -------------------------------------------------
                job = new Job();
                g = new ZMotifGraphics();

                if (!Connect(printerName, ref job))
                {
                    Debug.WriteLine("Unable to open device [" + printerName + "]\r\n");
                    OnCompleted(bRet);
                    return;
                }

                if (_alarm != 0)
                {
                    Debug.WriteLine("Printer is in alarm condition\r\n" + "Error: " + job.Device.GetStatusMessageString(_alarm));
                    OnCompleted(bRet);
                    return;
                }

                #region Builds the front side image (color)
                g.InitGraphics(0, 0, ZMotifGraphics.ImageOrientationEnum.Landscape, ZMotifGraphics.RibbonTypeEnum.Color);

                img = g.ImageFileToByteArray(frontImagePath);
                g.DrawImage(ref img, ZMotifGraphics.ImagePositionEnum.Centered, 0, 0, 0);
                int dataLen;

                bmpFront = g.CreateBitmap(out dataLen);
                g.ClearGraphics();
                #endregion

                #region Builds the front side image (color)
                g.InitGraphics(0, 0, ZMotifGraphics.ImageOrientationEnum.Landscape, ZMotifGraphics.RibbonTypeEnum.MonoK);

                img = g.ImageFileToByteArray(backImagePath);
                g.DrawImage(ref img, ZMotifGraphics.ImagePositionEnum.Centered, 0, 0, 0);

                bmpBack = g.CreateBitmap(out dataLen);
                g.ClearGraphics();
                #endregion

                #region Print the images
                job.JobControl.FeederSource = FeederSourceEnum.CardFeeder;
                job.JobControl.Destination = DestinationTypeEnum.Eject;

                job.BuildGraphicsLayers(SideEnum.Front, PrintTypeEnum.Color, 0, 0, 0, -1, GraphicTypeEnum.BMP, bmpFront);

                job.BuildGraphicsLayers(SideEnum.Back, PrintTypeEnum.MonoK, 0, 0, 0, -1, GraphicTypeEnum.BMP, bmpBack);


                int actionID = 0;
                job.PrintGraphicsLayers(1, out actionID);

                job.ClearGraphicsLayers();

                string status = string.Empty;
                JobWait(ref job, actionID, 180, out status, out bRet);

                Debug.WriteLine(bRet);
                #endregion

                OnCompleted(bRet);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("PrintLabel exception: " + ex.ToString());
                OnCompleted(bRet);
            }
        }

        public void WriteData(Action<string> OnCompleted)
        {
            try
            {
                string cardUID = string.Empty;
                string deviceSerialNumber = EnumDeviceNames.SmartCardPrinterSerialNumber;

                Job job = new Job();
                job.Open(deviceSerialNumber);

                // Move card to smart card reader and suspend ZMotif SDK control of printer (using ZMotif SDK)
                int actionID = 0;
                job.JobControl.SmartCardConfiguration(SideEnum.Front, SmartCardTypeEnum.MIFARE, true);
                job.SmartCardDataOnly(1, out actionID);
                // refer: https://km.zebra.com/kb/index?page=content&id=SA280&actp=LIST
                //  The goal of this program is to establish a connection with 
                // a Mifare 4k contactless microprocessor smart card through a ZXP printer.

                // Wait while card moves into encode position 
                Thread.Sleep(4000);

                try
                {
                    // write data
                    SmartCardReaderUtils smartCardReaderUtils = SmartCardReaderUtils.Instance;
                    string readerName = EnumDeviceNames.SmartCardPrinterContactlessReader;

                    // get card UID
                    cardUID = GetMifareCardUID();
                    string cardData = string.Empty;
                    bool readCardResult = smartCardReaderUtils.ReadAllData_MifareClassic_ZXPSeries9(ref cardData);
                    Console.WriteLine(cardData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                finally
                {
                    // Resume ZMotif SDK control of printer (using ZMotif SDK)
                    job.JobResume();

                    // Close ZMotif SDK control of job (using ZMotif SDK)
                    job.Close();

                    // Wait while eject the card
                    Thread.Sleep(2000);
                }

                OnCompleted(cardUID);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WriteData exception: " + ex.ToString());
                OnCompleted(string.Empty);
            }
        }

        private PrintAndWriteSmartcardResult PrintCard(PrintAndWriteSmartCardInfo data)
        {
            PrintAndWriteSmartcardResult printAndWriteSmartcardResult = new PrintAndWriteSmartcardResult()
            {
                Success = false
            };

            Job job = new Job();

            // Begin SDK communication with printer (using ZMotif SDK)
            //string deviceSerialNumber = "06C104500004";
            string deviceSerialNumber = EnumDeviceNames.SmartCardPrinterSerialNumber;
            job.Open(deviceSerialNumber);

            // Move card to smart card reader and suspend ZMotif SDK control of printer (using ZMotif SDK)
            int actionID = 0;
            job.JobControl.SmartCardConfiguration(SideEnum.Front, SmartCardTypeEnum.MIFARE, true);
            job.SmartCardDataOnly(1, out actionID);
            // refer: https://km.zebra.com/kb/index?page=content&id=SA280&actp=LIST
            //  The goal of this program is to establish a connection with 
            // a Mifare 4k contactless microprocessor smart card through a ZXP printer.

            // Wait while card moves into encode position 
            Thread.Sleep(4000);

            try
            {
                // write data
                SmartCardReaderUtils smartCardReaderUtils = SmartCardReaderUtils.Instance;
                string readerName = EnumDeviceNames.SmartCardPrinterContactlessReader;

                #region write data
                //PrintAndWriteSmartcardInfo_Demo data_Demo = new PrintAndWriteSmartcardInfo_Demo()
                //{
                //    Name = data.SmartCardData.CardHolderInfo.Name,
                //    PrintedDate = DateTime.Now
                //};

                //bool writeSuccessful = smartCardReaderUtils.WriteData(readerName, data_Demo);
                //if (!writeSuccessful)
                //{
                //    // Resume ZMotif SDK control of printer (using ZMotif SDK)
                //    job.JobResume();

                //    // Close ZMotif SDK control of job (using ZMotif SDK)
                //    job.Close();

                //    // return value
                //    return null;
                //}
                #endregion

                // read data for self verification
                //string getData = smartCardReaderUtils.ReadAllData_MifareClassic(readerName);

                // get card UID
                string cardUID = GetMifareCardUID();

                // set cardUID and success flag 
                if (!string.IsNullOrEmpty(cardUID))
                {
                    printAndWriteSmartcardResult.SmartCardData = new SmartCardData_Original()
                    {
                        //CardInfo = new CardInfo() { UID = cardUID }
                    };
                    printAndWriteSmartcardResult.Success = true;
                }

                string status = string.Empty;
                //bool printImageResult = Print_Type1(EnumDeviceNames.SmartCardPrinterSerialNumber, job, data.FrontCardImagePath, data.BackCardImagePath, ref status);

                //printAndWriteSmartcardResult.Success = printImageResult;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
            finally
            {
                // Resume ZMotif SDK control of printer (using ZMotif SDK)
                job.JobResume();

                // Close ZMotif SDK control of job (using ZMotif SDK)
                job.Close();

                // Wait while eject the card
                Thread.Sleep(2000);
            }

            return printAndWriteSmartcardResult;
        }

        private string GetMifareCardUID()
        {
            try
            {

                string cardUID = SmartCardReaderUtils.Instance.GetCardUID(EnumDeviceNames.SmartCardPrinterContactlessReader);

                return cardUID;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetMifareCardUID: " + ex.ToString());
                return string.Empty;
            }
        }

        public override EnumDeviceStatuses[] GetDeviceStatus()
        {
            // create default returnValue
            //List< EnumDeviceStatuses> returnValue = new List<EnumDeviceStatuses>();

            // check printer is connected or not
            // check with Win32_Printer
            if (IsPrinterConnected(_smartCardPrinterName))
            {
                return new EnumDeviceStatuses[] { EnumDeviceStatuses.Connected };
            }
            else
            {
                return new EnumDeviceStatuses[] { EnumDeviceStatuses.Disconnected };
            }

            // can not check printer status with PrintServer
            #region check printer status with PrintServer
            //// double check connected status with PrintServer
            //// Get a list of available printers (installed printers).
            //var printServer = new PrintServer();
            //var printQueues = printServer.GetPrintQueues(new[] { EnumeratedPrintQueueTypes.Local, EnumeratedPrintQueueTypes.Connections });

            //// Get barcode printer
            //PrintQueue printQueue = printQueues.FirstOrDefault(p => p.Name.ToUpper() == printerName);

            //// if barcode printer is null, return disconnected
            //if (printQueue == null)
            //{
            //    return new EnumDeviceStatuses[] { EnumDeviceStatuses.Disconnected };
            //}
            //else
            //{
            //    //return list status here
            //    var status = printQueue.QueueStatus;
            //    return new EnumDeviceStatuses[] { (EnumDeviceStatuses)status };
            //}
            #endregion
        }

        private bool IsPrinterConnected(string printerName)
        {
            try
            {
                //ManagementScope scope = new ManagementScope(@"\root\cimv2");
                //scope.Connect();

                // Select Printers from WMI Object Collections
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer");

                string printerTempName = string.Empty;
                foreach (ManagementObject printer in searcher.Get())
                {
                    printerTempName = printer["Name"].ToString().ToUpper();
                    if (printerTempName.Equals(printerName))
                    {
                        if (printer["WorkOffline"].ToString().ToLower().Equals("true"))
                        {
                            // printer is offline by user
                            Debug.WriteLine(printerName + ": printer is not connected.");
                            return false;
                        }
                        else
                        {
                            // printer is not offline
                            Debug.WriteLine(printerName + ": printer is connected.");
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("IsPrinterConnected exception: " + ex.ToString());
                return false;
            }
        }

        private bool Print_Label(string printerName, string frontImagePath, string backImagePath, ref string status)
        {
            bool bRet = true;

            byte[] img = null;
            byte[] bmpFront = null;
            byte[] bmpBack = null;
            Job job = null;
            ZMotifGraphics g = null;
            try
            {
                // Opens a connection with a ZXP Printer
                //     if it is in an alarm condition, exit function
                // -------------------------------------------------
                job = new Job();
                g = new ZMotifGraphics();

                if (!Connect(printerName, ref job))
                {
                    Debug.WriteLine("Unable to open device [" + printerName + "]\r\n");
                    return false;
                }

                if (_alarm != 0)
                {
                    Debug.WriteLine("Printer is in alarm condition\r\n" + "Error: " + job.Device.GetStatusMessageString(_alarm));
                    return false;
                }

                #region Builds the front side image (color)
                g.InitGraphics(0, 0, ZMotifGraphics.ImageOrientationEnum.Landscape, ZMotifGraphics.RibbonTypeEnum.Color);

                img = g.ImageFileToByteArray(frontImagePath);
                g.DrawImage(ref img, ZMotifGraphics.ImagePositionEnum.Centered, 0, 0, 0);

                //g.DrawLine(160, 70, 250, 70, g.IntegerFromColor(System.Drawing.Color.Red), 5.0f);
                //g.DrawRectangle(300, 20, 100, 100, 5.0f, g.IntegerFromColorName("Green"));
                //g.DrawEllipse(450, 20, 100, 100, 5.0f, g.IntegerFromColor(System.Drawing.Color.Blue));

                //g.DrawTextString(50.0f, 580.0f, "Print 1: Front Side: Image, Shapes, Text", "Arial", 10.0f,
                //    ZMotifGraphics.FontTypeEnum.Regular, g.IntegerFromColor(System.Drawing.Color.Navy));

                int dataLen;

                bmpFront = g.CreateBitmap(out dataLen);
                g.ClearGraphics();
                #endregion

                #region Builds the front side image (color)
                g.InitGraphics(0, 0, ZMotifGraphics.ImageOrientationEnum.Landscape, ZMotifGraphics.RibbonTypeEnum.MonoK);

                img = g.ImageFileToByteArray(backImagePath);
                g.DrawImage(ref img, ZMotifGraphics.ImagePositionEnum.Centered, 0, 0, 0);
                //g.DrawImage(ref img, 50.0f, 50.0f, 275, 200, 0);

                //g.DrawLine(350, 50, 475, 120, g.IntegerFromColor(System.Drawing.Color.Black), 5.0f);
                //g.DrawRectangle(500, 20, 100, 100, 5.0f, g.IntegerFromColorName("Black"));
                //g.DrawEllipse(650, 20, 100, 100, 5.0f, g.IntegerFromColor(System.Drawing.Color.Black));

                //g.DrawTextString(50.0f, 580.0f, "Print 1: Back Side: Image, Shapes, Text", "Arial", 10.0f,
                //                 ZMotifGraphics.FontTypeEnum.Regular, g.IntegerFromColor(System.Drawing.Color.DarkBlue));

                bmpBack = g.CreateBitmap(out dataLen);
                g.ClearGraphics();
                #endregion

                #region Print the images
                //if (!_isZXP7)
                //    job.JobControl.CardType = cardType;

                job.JobControl.FeederSource = FeederSourceEnum.CardFeeder;
                job.JobControl.Destination = DestinationTypeEnum.Eject;

                job.BuildGraphicsLayers(SideEnum.Front, PrintTypeEnum.Color, 0, 0, 0, -1, GraphicTypeEnum.BMP, bmpFront);

                job.BuildGraphicsLayers(SideEnum.Back, PrintTypeEnum.MonoK, 0, 0, 0, -1, GraphicTypeEnum.BMP, bmpBack);

                //string cardUID = SmartCardPrinterUtils.Instance.GetMifareCardUID(EnumDeviceNames.SmartCardPrinterContactlessReader);

                int actionID = 0;
                job.PrintGraphicsLayers(1, out actionID);

                job.ClearGraphicsLayers();

                JobWait(ref job, actionID, 180, out status, out bRet);
                #endregion

                //return true;
            }
            catch (Exception ex)
            {
                //return false;
            }
            return bRet;
        }

        private bool Connect(string printerName, ref Job j)
        {
            bool bRet = true;

            try
            {
                if (j == null)
                    return false;

                if (!j.IsOpen)
                {
                    _alarm = j.Open(printerName);

                    IdentifyZMotifPrinter(ref j);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                bRet = false;
            }
            return bRet;
        }

        #region Identify ZXP Printer Type
        private void IdentifyZMotifPrinter(ref Job job)
        {
            try
            {
                string vendor = string.Empty;
                string model = string.Empty;
                string serialNo = string.Empty;
                string MAC = string.Empty;
                string headSerialNo = string.Empty;
                string OemCode = string.Empty;
                string fwVersion = string.Empty;
                string mediaVersion = string.Empty;
                string heaterVersion = string.Empty;
                string zmotifVer = string.Empty;

                GetDeviceInfo(ref job, out vendor, out model, out serialNo, out MAC,
                              out headSerialNo, out OemCode, out fwVersion,
                              out mediaVersion, out heaterVersion, out zmotifVer);

                //if (model.Contains("7"))
                //if (model.Contains("9"))
                //    _isZXP7 = true;
                //else
                //    _isZXP7 = false;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private short GetDeviceInfo(ref Job job, out string vender, out string model, out string serialNo, out string MAC,
                                    out string headSerialNo, out string OemCode, out string fwVersion, out string mediaVersion,
                                    out string heaterVersion, out string zmotifVersion)
        {
            vender = string.Empty;
            model = string.Empty;
            serialNo = string.Empty;
            MAC = string.Empty;
            headSerialNo = string.Empty;
            OemCode = string.Empty;
            fwVersion = string.Empty;
            mediaVersion = string.Empty;
            heaterVersion = string.Empty;
            zmotifVersion = string.Empty;

            try
            {
                return job.Device.GetDeviceInfo(out vender, out model, out serialNo, out MAC,
                                                 out headSerialNo, out OemCode, out fwVersion,
                                                 out mediaVersion, out heaterVersion, out zmotifVersion);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion Identify ZXP Printer Type

        // Waits for a job to complete
        // --------------------------------------------------------------------------------------------------
        private void JobWait(ref Job job, int actionID, int loops, out string status, out bool bRet)
        {
            status = string.Empty;
            bRet = false;

            try
            {
                JobStatusStruct js = new JobStatusStruct();

                while (loops > 0)
                {
                    try
                    {
                        _alarm = job.GetJobStatus(actionID, out js.uuidJob, out js.printingStatus,
                                    out js.cardPosition, out js.errorCode, out js.copiesCompleted,
                                    out js.copiesRequested, out js.magStatus, out js.contactStatus,
                                    out js.contactlessStatus);

                        if (js.printingStatus == "done_ok" || js.printingStatus == "cleaning_up")
                        {
                            status = js.printingStatus + ": " + "Indicates a job completed successfully";
                            bRet = true;
                            break;
                        }
                        else if (js.printingStatus.Contains("cancelled"))
                        {
                            status = js.printingStatus;
                            break;
                        }

                        if (js.contactStatus.ToLower().Contains("error"))
                        {
                            status = js.contactStatus;
                            break;
                        }

                        if (js.printingStatus.ToLower().Contains("error"))
                        {
                            status = "Printing Status Error";
                            break;
                        }

                        if (js.contactlessStatus.ToLower().Contains("error"))
                        {
                            status = js.contactlessStatus;
                            break;
                        }

                        if (js.magStatus.ToLower().Contains("error"))
                        {
                            status = js.magStatus;
                            break;
                        }

                        if (_alarm != 0 && _alarm != 4016) //no error or out of cards
                        {
                            status = "Error: " + job.Device.GetStatusMessageString(_alarm);
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        status = "Job Wait Exception: " + e.Message;
                        break;
                    }

                    if (_alarm == 0)
                    {
                        if (--loops <= 0)
                        {
                            status = "Job Status Timeout";
                            break;
                        }
                    }
                    Thread.Sleep(1000);
                }
            }
            finally
            {
                string _msg = status;
            }
        }
    }
}
