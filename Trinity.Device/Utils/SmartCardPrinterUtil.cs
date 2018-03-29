using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using ZMOTIFPRINTERLib;
using ZMTGraphics;

namespace Trinity.Device.Util
{
    public class SmartCardPrinterUtil : DeviceUtil
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
        private static volatile SmartCardPrinterUtil _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private SmartCardPrinterUtil()
        {
            _smartCardPrinterName = EnumDeviceNames.SmartCardPrinterName;
        }

        public static SmartCardPrinterUtil Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new SmartCardPrinterUtil();
                    }
                }

                return _instance;
            }
        }
        #endregion

        public bool PrintAndWriteSmartCard(PrintAndWriteSmartCardInfo cardInfo, Action<PrintAndWriteCardResult> OnCompleted)
        {
            try
            {

                // validate
                if (cardInfo == null)
                {
                    throw new Exception("cardInfo can not be null");
                }
                PrintAndWriteCardResult returnValue = new PrintAndWriteCardResult()
                {
                    Success = false
                };

                if (IsPrinterConnected(EnumDeviceNames.SmartCardPrinterName))
                {
                    #region use SmartCardPrinterName
                    byte[] img = null;
                    byte[] bmpFront = null;
                    byte[] bmpBack = null;

                    Job job = null;
                    ZMotifGraphics g = null;

                    string frontImagePath, backImagePath = null;

                    try
                    {
                        job = new Job();
                        g = new ZMotifGraphics();

                        // Opens a connection with a ZXP Printer
                        //     if it is in an alarm condition, exit function
                        // -------------------------------------------------

                        if (!Connect(ref job))
                        {
                            throw new Exception("Unable to open device [" + _smartCardPrinterName + "]\r\n");
                        }

                        // Determines if the ZXP device supports smart card encoding
                        // ---------------------------------------------------------
                        if (!GetPrinterConfiguration(ref job, out bool isContactless))
                        {
                            throw new Exception("Unable to get printer configuration");
                        }

                        if (!isContactless)
                        {
                            throw new Exception("Printer is not configured for Contactless Encoding");
                        }

                        if (_alarm != 0)
                        {
                            throw new Exception("Printer is in alarm condition\r\n");
                        }

                        frontImagePath = cardInfo.FrontCardImagePath;
                        backImagePath = cardInfo.BackCardImagePath;

                        // Builds the front side image (color)
                        // -----------------------------------

                        g.InitGraphics(0, 0, ZMotifGraphics.ImageOrientationEnum.Landscape,
                                       ZMotifGraphics.RibbonTypeEnum.Color);

                        img = g.ImageFileToByteArray(frontImagePath);
                        g.DrawImage(ref img, ZMotifGraphics.ImagePositionEnum.Centered, 1024, 648, 0);

                        int dataLen = 0;
                        bmpFront = g.CreateBitmap(out dataLen);
                        g.ClearGraphics();

                        // Builds the back side image (monochrome)
                        // ---------------------------------------

                        g.InitGraphics(0, 0, ZMotifGraphics.ImageOrientationEnum.Landscape,
                                       ZMotifGraphics.RibbonTypeEnum.MonoK);

                        img = g.ImageFileToByteArray(backImagePath);
                        g.DrawImage(ref img, ZMotifGraphics.ImagePositionEnum.Centered, 1024, 648, 0);

                        bmpBack = g.CreateBitmap(out dataLen);
                        g.ClearGraphics();

                        // Start a contactless smart card job
                        // -----------------------

                        //job.JobControl.CardType = GetContactlessCardType(ref job);
                        //job.JobControl.CardType = "PVC,MIFARE,4K";

                        job.JobControl.FeederSource = FeederSourceEnum.CardFeeder;
                        job.JobControl.Destination = DestinationTypeEnum.Eject;

                        job.JobControl.SmartCardConfiguration(SideEnum.Front, SmartCardTypeEnum.MIFARE, true);

                        job.BuildGraphicsLayers(SideEnum.Front, PrintTypeEnum.Color, 0, 0, 0,
                                                -1, GraphicTypeEnum.BMP, bmpFront);

                        job.BuildGraphicsLayers(SideEnum.Back, PrintTypeEnum.MonoK, 0, 0, 0,
                                                -1, GraphicTypeEnum.BMP, bmpBack);

                        int actionID = 0;
                        job.PrintGraphicsLayers(1, out actionID);

                        job.ClearGraphicsLayers();

                        // Waits for the card to reach the smart card station
                        // --------------------------------------------------
                        string status = string.Empty;
                        AtStation(ref job, actionID, 30, out status);

                        // ***** Smart Card Code goes here *****
                        SmartCardData_Original cardData = new SmartCardData_Original()
                        {
                            SuperviseeBiodata = cardInfo.SuperviseeBiodata,
                            DutyOfficerData = cardInfo.DutyOfficerData
                        };
                        // write data
                        bool writeDataResult = SmartCardReaderUtil.Instance.WriteData(cardData, EnumDeviceNames.SmartCardPrinterContactlessReader);
                        if (writeDataResult)
                        {
                            // get card UID
                            returnValue.CardUID = SmartCardReaderUtil.Instance.GetCardUID(EnumDeviceNames.SmartCardPrinterContactlessReader);
                        }

                        // At the completion of smart card process
                        //     if the smart card encoding was successful JobResume
                        //     if the smart card encoding was Unsuccessful JobAbort
                        // --------------------------------------------------------

                        if (writeDataResult)
                            JobResume(ref job);
                        else
                            JobAbort(ref job, true);

                        JobWait(ref job, actionID, 180, out status);

                        // need to verify printing status
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.Message);
                    }
                    finally
                    {
                        g.CloseGraphics();
                        g = null;

                        img = null;
                        bmpFront = null;
                        bmpBack = null;

                        Disconnect(ref job);
                    }
                    #endregion
                }
                else if (SmartCardReaderUtil.Instance.GetDeviceStatus().Contains(EnumDeviceStatus.Connected))
                {
                    #region use card reader
                    SmartCardData_Original cardData = new SmartCardData_Original()
                    {
                        SuperviseeBiodata = cardInfo.SuperviseeBiodata,
                        DutyOfficerData = cardInfo.DutyOfficerData
                    };
                    // write data
                    bool writeDataResult = SmartCardReaderUtil.Instance.WriteData(cardData);
                    if (writeDataResult)
                    {
                        // get card UID
                        returnValue.CardUID = SmartCardReaderUtil.Instance.GetCardUID(EnumDeviceNames.SmartCardContactlessReader);
                    }
                    #endregion
                }
                else
                {
                    returnValue.Description = "Smart card printer is not connected!";
                }


                if (!string.IsNullOrEmpty(returnValue.CardUID))
                {
                    returnValue.Success = true;
                }

                OnCompleted(returnValue);
                return returnValue.Success;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("PrintAndWriteSmartcardData exception: " + ex.ToString());
                OnCompleted(new PrintAndWriteCardResult() { Success = false, Description = "Oops!.. Something went wrong ..." });
                return false;
            }
        }

        /// <summary>
        /// Print smart card
        /// </summary>
        /// <param name="type">0: Supervisee, 1: Duty officer</param>
        /// <param name="data"></param>
        /// <returns></returns>

        public override EnumDeviceStatus[] GetDeviceStatus()
        {
            // create default returnValue
            //List< EnumDeviceStatuses> returnValue = new List<EnumDeviceStatuses>();

            // check printer is connected or not
            // check with Win32_Printer
            if (IsPrinterConnected(_smartCardPrinterName))
            {
                return new EnumDeviceStatus[] { EnumDeviceStatus.Connected };
            }
            else
            {
                return new EnumDeviceStatus[] { EnumDeviceStatus.Disconnected };
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
                    if (printerTempName.Equals(printerName.ToUpper()))
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

        #region Support

        // Waits for a smart card to be at the smart card programming station
        // --------------------------------------------------------------------------------------------------

        private void AtStation(ref Job job, int actionID, int loops, out string status)
        {
            bool timedOut = true;

            JobStatusStruct js = new JobStatusStruct();

            status = "";

            for (int i = 0; i < loops; i++)
            {
                try
                {
                    _alarm = job.GetJobStatus(actionID, out js.uuidJob, out js.printingStatus,
                                out js.cardPosition, out js.errorCode, out js.copiesCompleted,
                                out js.copiesRequested, out js.magStatus, out js.contactStatus,
                                out js.contactlessStatus);
                }
                catch (Exception e)
                {
                    status = "At Station Exception: " + e.Message;
                    break;
                }

                //if (js.printingStatus.Contains("error") || js.printingStatus == "at_station" ||
                //    js.contactStatus == "at_station" || js.contactlessStatus == "at_station")
                if (js.contactlessStatus == "at_station")
                {
                    timedOut = false;
                    break;
                }
                Thread.Sleep(1000);
            }
            if (timedOut)
                status = "At Station Timed Out";
        }

        // Gets the card types that the printer supports
        // --------------------------------------------------------------------------------------------------

        //public bool GetCardTypeList(ref Job job)
        //{
        //_cardTypeList = null;

        //try
        //{
        //    job.JobControl.GetAvailableCardTypes(out _cardTypeList);
        //    return true;
        //}
        //catch
        //{
        //    return false;
        //}
        //}

        private string GetContactlessCardType(ref Job job)
        {
            try
            {
                object cardTypes = null;

                job.JobControl.GetAvailableCardTypes(out cardTypes);

                System.Collections.ArrayList arrCardTypes = System.Collections.ArrayList.Adapter((string[])cardTypes);

                foreach (string card in arrCardTypes)
                {
                    if (card.ToUpper().Contains("MIFARE"))
                        return card;
                }
                return string.Empty;
            }
            catch
            {
                throw;
            }
        }

        private string GetContactCardType(ref Job job)
        {
            try
            {
                object cardTypes = null;

                job.JobControl.GetAvailableCardTypes(out cardTypes);

                System.Collections.ArrayList arrCardTypes = System.Collections.ArrayList.Adapter((string[])cardTypes);

                foreach (string card in arrCardTypes)
                {
                    if (card.Contains("SLE"))
                        return card;
                }
                return string.Empty;
            }
            catch
            {
                throw;
            }
        }

        private string GetMagneticCardType(ref Job job)
        {
            try
            {
                object cardTypes = null;

                job.JobControl.GetAvailableCardTypes(out cardTypes);

                System.Collections.ArrayList arrCardTypes = System.Collections.ArrayList.Adapter((string[])cardTypes);

                foreach (string card in arrCardTypes)
                {
                    if (card.ToUpper().Contains("HICO"))
                        return card;
                }
                return string.Empty;
            }
            catch
            {
                throw;
            }
        }

        // Gets a list of ZMotif devices
        //     ConnectionTypeEnum { USB, Ethernet, All }
        // --------------------------------------------------------------------------------------------------

        //public bool GetDeviceList(bool USB)
        //{
        //    bool bRet = true;
        //Job job = new Job();

        //try
        //{
        //    if (USB)
        //        job.GetPrinters(ConnectionTypeEnum.USB, out _deviceList);
        //    else
        //        job.GetPrinters(ConnectionTypeEnum.Ethernet, out _deviceList);
        //}
        //catch
        //{
        //    _deviceList = null;
        //    bRet = false;
        //}

        //Disconnect(ref job);
        //    return bRet;
        //}

        // Gets the printer configuration
        // --------------------------------------------------------------------------------------------------

        //private bool GetPrinterConfiguration(ref Job j)
        //{
        //    bool bRet = true;

        //    _isContact = _isContactless = _isMag = false;

        //    try
        //    {
        //        string headType, stripeLocation;
        //        j.Device.GetMagneticEncoderConfiguration(out headType, out stripeLocation);
        //        if (headType != "none" && headType != "")
        //            _isMag = true;

        //        string commChannel, contact, contactless;
        //        j.Device.GetSmartCardConfiguration(out commChannel, out contact, out contactless);

        //        if (contact != "" && contact != "none")
        //            _isContact = true;

        //        if (contactless != "" && contactless != "none")
        //            _isContactless = true;
        //    }
        //    catch
        //    {
        //        bRet = false;
        //    }
        //    return bRet;
        //}

        // Loads a byte array with image data from a file
        // --------------------------------------------------------------------------------------------------

        private byte[] ImageToByteArray(string filename)
        {
            Image img = System.Drawing.Image.FromFile(filename);

            MemoryStream ms = new MemoryStream();
            img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
            return ms.ToArray();
        }

        // Aborts a suspended job
        // --------------------------------------------------------------------------------------------------

        private bool JobAbort(ref Job job, bool eject)
        {
            try
            {
                job.JobAbort(eject);
                return true;
            }
            catch
            {
                return false;
            }
        }

        // Waits for a job to complete
        // --------------------------------------------------------------------------------------------------

        private void JobWait(ref Job job, int actionID, int loops, out string status)
        {
            status = string.Empty;

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


        // Resumes a suspended job
        // --------------------------------------------------------------------------------------------------

        private bool JobResume(ref Job job)
        {
            try
            {
                job.JobResume();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region ZMotif Device Connect

        // Connects to a ZMotif device
        // --------------------------------------------------------------------------------------------------

        private bool Connect(ref Job j)
        {
            bool bRet = true;

            try
            {
                if (j == null)
                    return false;

                if (!j.IsOpen)
                {
                    _alarm = j.Open(EnumDeviceNames.SmartCardPrinterSerialNumber);

                    IdentifyZMotifPrinter(ref j);
                }
            }
            catch (Exception e)
            {
                bRet = false;
            }
            return bRet;
        }

        // Disconnects from a ZMotif device
        // --------------------------------------------------------------------------------------------------

        private bool Disconnect(ref Job j)
        {
            bool bRet = true;

            try
            {
                if (j == null)
                    return false;

                if (j.IsOpen)
                {
                    j.Close();

                    do
                    {
                        Thread.Sleep(10);
                    } while (Marshal.FinalReleaseComObject(j) != 0);
                }
            }
            catch
            {
                bRet = false;
            }
            finally
            {
                j = null;
                GC.Collect();
            }
            return bRet;
        }
        #endregion


        // Gets the printer configuration
        // --------------------------------------------------------------------------------------------------

        private bool GetPrinterConfiguration(ref Job j, out bool isContactless)
        {
            bool bRet = true;
            isContactless = false;
            //_isContact = _isContactless = _isMag = false;

            try
            {
                string headType, stripeLocation;
                j.Device.GetMagneticEncoderConfiguration(out headType, out stripeLocation);
                //if (headType != "none" && headType != "")
                //    _isMag = true;

                string commChannel, contact, contactless;
                j.Device.GetSmartCardConfiguration(out commChannel, out contact, out contactless);

                //if (contact != "" && contact != "none")
                //    _isContact = true;

                if (contactless != "" && contactless != "none")
                    isContactless = true;
                //_isContactless = true;
            }
            catch
            {
                bRet = false;
            }
            return bRet;
        }
    }
}
