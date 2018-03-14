using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Trinity.Device;

namespace Trinity.Util
{
    public class DocumentScannerUtil
    {
        private bool scanner_connected = false;
        private bool enablefeeder = false;
        private IntPtr scanning_handle;
        private PdiScanWrap.PageEndCallback page_end_callback_object = null;
        private PdiScanWrap.PageEjectCallback page_eject_callback_object = null;
        private PdiScanWrap.ScanningErrorCallback scanning_error_callback_object = null;
        private PdiScanWrap.ImprinterStringCallback imprinter_callback_object = null;

        private Action<string, string> _documentScannerCallback;
        
        public bool EnableFeeder
        {
            get
            {
                return enablefeeder;
            }
        }

        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile DocumentScannerUtil _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private DocumentScannerUtil()
        {
            IntPtr extra_info;
            IntPtr scanner_name = Marshal.AllocHGlobal(256);
            pdiscan_errors pdiscan_error;
            string error_string;

            //string working_folder = ProgramFilesx86();
            ////Quick check to determine if we are running in x86 or x64 mode.
            //if (IntPtr.Size == 8)
            //    working_folder += @"\Peripheral Dynamics Inc\PDIScanSDK\Bin\Scanning\x64";
            //else
            //    working_folder += @"\Peripheral Dynamics Inc\PDIScanSDK\Bin\Scanning\x86";
            //if (Directory.Exists(working_folder))
            //    Directory.SetCurrentDirectory(working_folder);

            pdiscan_error = PdiScanWrap.PdAllocateScanningHandle(out scanning_handle, out extra_info);

            if (pdiscan_error != pdiscan_errors.PDISCAN_ERR_NONE)
            {
                error_string = string.Format("The PDIScan library returned error #{0:D} with extra info \"{1}\".",
                            pdiscan_error, Marshal.PtrToStringAnsi(extra_info));
                MessageBox.Show(error_string);
                return;
            }

            page_end_callback_object = new PdiScanWrap.PageEndCallback(page_end_callback);
            pdiscan_error = PdiScanWrap.PdInstallCallback(scanning_handle, pd_callback_types.PD_CALLBACK_TYPE_PAGE_END, page_end_callback_object, IntPtr.Zero);
            if (pdiscan_error != pdiscan_errors.PDISCAN_ERR_NONE)
                display_pdiscan_error(pdiscan_error);

            page_eject_callback_object = new PdiScanWrap.PageEjectCallback(page_eject_callback);
            pdiscan_error = PdiScanWrap.PdInstallCallback(scanning_handle, pd_callback_types.PD_CALLBACK_TYPE_PAGE_EJECT, page_eject_callback_object, IntPtr.Zero);
            if (pdiscan_error != pdiscan_errors.PDISCAN_ERR_NONE)
                display_pdiscan_error(pdiscan_error);

            scanning_error_callback_object = new PdiScanWrap.ScanningErrorCallback(scanning_error_callback);
            pdiscan_error = PdiScanWrap.PdInstallCallback(scanning_handle, pd_callback_types.PD_CALLBACK_TYPE_SCANNING_ERROR, scanning_error_callback_object, IntPtr.Zero);
            if (pdiscan_error != pdiscan_errors.PDISCAN_ERR_NONE)
                display_pdiscan_error(pdiscan_error);

            imprinter_callback_object = new PdiScanWrap.ImprinterStringCallback(imprinter_string_callback);
            pdiscan_error = PdiScanWrap.PdInstallCallback(scanning_handle, pd_callback_types.PD_CALLBACK_TYPE_IMPRINTER_STRING, imprinter_callback_object, IntPtr.Zero);
            if (pdiscan_error != pdiscan_errors.PDISCAN_ERR_NONE)
                display_pdiscan_error(pdiscan_error);

            //Make sure the garbage collector doesn't get the callback objects or the scanning handle
            GC.KeepAlive(page_end_callback_object);
            GC.KeepAlive(page_eject_callback_object);
            GC.KeepAlive(scanning_error_callback_object);
            GC.KeepAlive(imprinter_callback_object);
            GC.KeepAlive(scanning_handle);
        }

        public static DocumentScannerUtil Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new DocumentScannerUtil();
                    }
                }

                return _instance;
            }
        }
        #endregion

        public EnumDeviceStatus[] GetDeviceStatus()
        {
            return new EnumDeviceStatus[] { EnumDeviceStatus.Connected };
        }

        public bool Connect()
        {
            try
            {
                //MessageBox.Show("Connect");
                pdiscan_errors pdiscan_error;

                pdiscan_error = PdiScanWrap.PdConnectToScanner(scanning_handle, "", 1);
                if (pdiscan_error != pdiscan_errors.PDISCAN_ERR_NONE)
                {
                    MessageBox.Show("Connect PDISCAN_ERR_NONE");
                    if (pdiscan_error == pdiscan_errors.PDISCAN_ERR_SCANNER_NOT_FOUND)
                    {
                        MessageBox.Show("The Document scanner (DPI) is not connected to the computer.");
                        return false;
                    }
                    else
                    {
                        display_pdiscan_error(pdiscan_error);
                        return false;
                    }
                }

                int default_color;
                pdiscan_error = PdiScanWrap.PdGetTagDefaultLong(scanning_handle, (int)pdiscan_tags.PDISCAN_TAG_COLOR_DEPTH, out default_color);
                if (pdiscan_error != pdiscan_errors.PDISCAN_ERR_NONE)
                {
                    display_pdiscan_error(pdiscan_error);
                    MessageBox.Show("Connect PDISCAN_ERR_NONE 1");
                    return false;
                }
                //if (default_color == (int)pd_color_depths.PD_COLOR_DEPTH_BITONAL)
                //    radioBitonal.Checked = true;
                //else
                //    radioGreyscale.Checked = true;

                //Detect the scanner dpi options
                int num_dpi, default_dpi;
                PdiScanWrap.PdGetTagDefaultLong(scanning_handle, (int)pdiscan_tags.PDISCAN_TAG_RESOLUTION, out default_dpi);
                PdiScanWrap.PdGetTagChoiceCount(scanning_handle, (int)pdiscan_tags.PDISCAN_TAG_RESOLUTION, out num_dpi);
                for (int i = 1; i <= num_dpi; i++)
                {
                    int dpi_option;
                    PdiScanWrap.PdGetTagChoiceLong(scanning_handle, (int)pdiscan_tags.PDISCAN_TAG_RESOLUTION, i, out dpi_option);
                    //if (i == 1)
                    //{
                    //    radioDPI1.Text = dpi_option.ToString() + " dpi";
                    //    radioDPI1.Tag = dpi_option;
                    //    if (default_dpi == dpi_option)
                    //        radioDPI1.Checked = true;
                    //}
                    //else if (i == 2)
                    //{
                    //    radioDPI2.Text = dpi_option.ToString() + " dpi";
                    //    radioDPI2.Tag = dpi_option;
                    //    if (default_dpi == dpi_option)
                    //        radioDPI2.Checked = true;
                    //}
                    //else if (i == 3)
                    //{
                    //    radioDPI3.Text = dpi_option.ToString() + " dpi";
                    //    radioDPI3.Tag = dpi_option;
                    //    if (default_dpi == dpi_option)
                    //        radioDPI3.Checked = true;
                    //}
                }
                //if (num_dpi < 3)
                //    radioDPI3.Visible = false;

                int default_duplex_on;
                pdiscan_error = PdiScanWrap.PdGetTagDefaultLong(scanning_handle, (int)pdiscan_tags.PDISCAN_TAG_DUPLEX, out default_duplex_on);
                if (pdiscan_error != pdiscan_errors.PDISCAN_ERR_NONE)
                {
                    display_pdiscan_error(pdiscan_error);
                    MessageBox.Show("Connect PDISCAN_ERR_NONE 2");
                    return false;
                }
                //if (default_duplex_on == 1)
                //    radioDuplex.Checked = true;
                //else
                //    radioSimplex.Checked = false;

                //This shows how to get a tag's range and current value.
                //Populate the brightness combobox
                int minimum, maximum, step, current, selected = 0;
                //Get the minumum brightness
                PdiScanWrap.PdGetTagChoiceLong(scanning_handle, (int)pdiscan_tags.PDISCAN_TAG_BRIGHTNESS_FRONT, -3, out minimum);
                //Get the maximum brightness
                PdiScanWrap.PdGetTagChoiceLong(scanning_handle, (int)pdiscan_tags.PDISCAN_TAG_BRIGHTNESS_FRONT, -2, out maximum);
                //Get the step between different brightness levels
                PdiScanWrap.PdGetTagChoiceLong(scanning_handle, (int)pdiscan_tags.PDISCAN_TAG_BRIGHTNESS_FRONT, -1, out step);
                //Get the current brightness
                PdiScanWrap.PdGetTagLong(scanning_handle, (int)pdiscan_tags.PDISCAN_TAG_BRIGHTNESS_FRONT, out current);
                //cboBrightness.Items.Clear();
                //for (int value = minimum, j = 0; value < maximum; j++, value += step)
                //{
                //    if (value == current)
                //        selected = j;
                //    cboBrightness.Items.Add(value);
                //}
                //cboBrightness.SelectedIndex = selected;

                scanner_connected = true;
                //MessageBox.Show("Connect OK");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connect exception: " + ex.ToString());
                return false;
            }
        }

        public bool Disconnect()
        {
            try
            {
                if (scanner_connected)
                {
                    pdiscan_errors pdiscan_error = PdiScanWrap.PdDisconnectFromScanner(scanning_handle); // this command will make document stuck
                    scanner_connected = false;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Disconnect exception: " + ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="documentScannerCallback">DocumentScannerCallback(string frontPath, string error)</param>
        public bool StartScanning(Action<string, string> documentScannerCallback)
        {
            try
            {
                // If scanner is not initiated, run Connect() first
                if (!scanner_connected)
                {
                    if(!Connect())
                    {
                        MessageBox.Show("StartScanning.PdEnableFeeder failed");
                        return false;
                    }
                }

                if (scanner_connected)
                {
                    //MessageBox.Show("StartScanning");
                    //_documentScannerCallback = documentScannerCallback;
                    pdiscan_errors pdiscan_error = PdiScanWrap.PdEnableFeeder(scanning_handle);
                    if (pdiscan_error != pdiscan_errors.PDISCAN_ERR_NONE)
                    {
                        display_pdiscan_error(pdiscan_error);
                        //MessageBox.Show("StartScanning.PdEnableFeeder failed");
                        return false;
                    }

                    //MessageBox.Show("StartScanning.PdEnableFeeder OK");
                    enablefeeder = true;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("StartScanning exception: " + ex.ToString());
                return false;
            }
        }

        public bool StopScanning()
        {
            try
            {
                //pdiscan_error = PdiScanWrap.PdDisconnectFromScanner(scanning_handle); // this command will make document stuck
                pdiscan_errors pdiscan_error = PdiScanWrap.PdDisableFeeder(scanning_handle);
                if (pdiscan_error != pdiscan_errors.PDISCAN_ERR_NONE)
                {
                    //MessageBox.Show("StopScanning failed");
                    display_pdiscan_error(pdiscan_error);
                    return false;
                }

                enablefeeder = false;
                //MessageBox.Show("StopScanning OK");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("StopScanning exception: " + ex.ToString());
                return false;
            }
        }

        public void display_pdiscan_error(pdiscan_errors pdiscan_error)
        {
            string error_string;
            IntPtr short_description = Marshal.AllocHGlobal(256);
            IntPtr long_description = Marshal.AllocHGlobal(256);
            IntPtr extra_info = Marshal.AllocHGlobal(256);

            if (pdiscan_error != pdiscan_errors.PDISCAN_ERR_NONE)
            {
                PdiScanWrap.PdGetErrorShortDescription(scanning_handle, pdiscan_error, short_description);
                PdiScanWrap.PdGetErrorLongDescription(scanning_handle, pdiscan_error, long_description);
                PdiScanWrap.PdGetErrorExtraInfo(scanning_handle, extra_info);

                error_string = "Error Number: " + ((int)pdiscan_error).ToString() + "\n"
                    + "Short Description: " + Marshal.PtrToStringAnsi(short_description) + "\n"
                    + "Long Description: " + Marshal.PtrToStringAnsi(long_description) + "\n"
                    + "Extra Info: " + Marshal.PtrToStringAnsi(extra_info) + "\n";
                MessageBox.Show("display_pdiscan_error: " + error_string);
            }

            Marshal.FreeHGlobal(short_description);
            Marshal.FreeHGlobal(long_description);
            Marshal.FreeHGlobal(extra_info);
        }

        private string Get_PDIScan_Error_Details(pdiscan_errors pdiscan_error)
        {
            try
            {
                string error_string = string.Empty;
                IntPtr short_description = Marshal.AllocHGlobal(256);
                IntPtr long_description = Marshal.AllocHGlobal(256);
                IntPtr extra_info = Marshal.AllocHGlobal(256);

                if (pdiscan_error != pdiscan_errors.PDISCAN_ERR_NONE)
                {
                    PdiScanWrap.PdGetErrorShortDescription(scanning_handle, pdiscan_error, short_description);
                    PdiScanWrap.PdGetErrorLongDescription(scanning_handle, pdiscan_error, long_description);
                    PdiScanWrap.PdGetErrorExtraInfo(scanning_handle, extra_info);

                    error_string = "Error Number: " + ((int)pdiscan_error).ToString() + "\n"
                        + "Short Description: " + Marshal.PtrToStringAnsi(short_description) + "\n"
                        + "Long Description: " + Marshal.PtrToStringAnsi(long_description) + "\n"
                        + "Extra Info: " + Marshal.PtrToStringAnsi(extra_info) + "\n";
                }

                Marshal.FreeHGlobal(short_description);
                Marshal.FreeHGlobal(long_description);
                Marshal.FreeHGlobal(extra_info);

                return error_string;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Get_PDIScan_Error_Details exception: " + ex.ToString());
                return string.Empty;
            }
        }

        //This function will always return the path to the x86 program files directory.
        static string ProgramFilesx86()
        {
            if (8 == IntPtr.Size || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
            {
                //64 bit os
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }
            //32 bit os
            return Environment.GetEnvironmentVariable("ProgramFiles");
        }

        #region CallbackFunctions
        //Callbacks occur on a different thread.  In order to return to the main thread we must use delegates.
        public void page_end_callback(int PageNumber, IntPtr FrontSideDIB, IntPtr BackSideDIB, ref int AbortRequested, IntPtr UserData)
        {
            try
            {
                Debug.Assert(FrontSideDIB != IntPtr.Zero);
                
                string curDir = Directory.GetCurrentDirectory();

                // create directory
                if (!Directory.Exists(curDir + "\\Temp"))
                {
                    Directory.CreateDirectory(curDir + "\\Temp");
                }

                // set file path
                string frontPath = string.Empty;
                string error = string.Empty;
                //Front
                if (FrontSideDIB != IntPtr.Zero)
                {
                    pdiscan_errors retval = PdiScanWrap.PdSaveImageToDisk(FrontSideDIB, curDir + "\\Temp\\document_front.bmp");
                    if (retval == pdiscan_errors.PDISCAN_ERR_NONE)
                    {
                        frontPath = curDir + "\\Temp\\document_front.bmp";

                        // Scale image
                        Image image = Image.FromFile(frontPath);
                        Image newImage = Trinity.Common.CommonUtil.ScaleImage(image, 768, 1024);
                        image.Dispose();
                        newImage.Save(frontPath, ImageFormat.Jpeg);
                        newImage.Dispose();
                    }
                    else
                    {
                        error = Get_PDIScan_Error_Details(retval);
                    }
                }

                //Back
                //if (BackSideDIB != IntPtr.Zero)
                //{
                //    PdiScanWrap.PdSaveImageToDisk(BackSideDIB, temp_path + "testback.bmp");
                //}

                // callback
                _documentScannerCallback(frontPath, error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("page_end_callback exception: " + ex.ToString());
                // callback
                _documentScannerCallback(string.Empty, ex.Message);
            }
        }

        // This callback function is called for the page eject information.
        private void page_eject_callback(int PageNumber, ref pd_eject_directions EjectDirection, IntPtr UserData)
        {
            // Set EjectFront as default
            EjectDirection = pd_eject_directions.PD_EJECT_DIRECTION_FRONT;
            //MessageBox.Show("eject");
            //if (radioEjectFront.Checked)
            //    EjectDirection = pd_eject_directions.PD_EJECT_DIRECTION_FRONT;
            //else if (radioEjectBack.Checked)
            //    EjectDirection = pd_eject_directions.PD_EJECT_DIRECTION_BACK;
            //else
            //    EjectDirection = pd_eject_directions.PD_EJECT_DIRECTION_WAIT;
        }

        private void scanning_error_callback(pdiscan_errors ScanningError, string ExtraInfo, IntPtr UserData)
        {
            MessageBox.Show("scanning_error_callback: " + ExtraInfo);
        }

        private void imprinter_string_callback(int PageNumber, IntPtr ImprinterString, IntPtr UserData)
        {
            try
            {
                MessageBox.Show("imprinter_string_callback");
                IntPtr impstr = Marshal.StringToHGlobalAnsi("imprinttest we do\n word wrap");
                PD_IMPRINT_SETUP setup = new PD_IMPRINT_SETUP();
                setup.eBase = pd_imprinter_horizontal_offset_bases.PD_IMPRINTER_HORIZONTAL_OFFSET_BASE_LEADING_EDGE;
                setup.eDirection = pd_imprint_directions.PD_IMPRINT_DIRECTION_FORWARD;
                setup.eTextOrientation = pd_imprinter_text_orientations.PD_IMPRINTER_TEXT_ORIENTATION_90;
                setup.iOffset = 0;
                pdiscan_errors pdiscan_error = PdiScanWrap.PdImprintDocument(scanning_handle, pd_imprint_types.PD_IMPRINT_TYPE_TEXT, impstr, ref setup);
            }
            catch (Exception ex)
            {
                MessageBox.Show("imprinter_string_callback exception: " + ex.ToString());
            }
        }
        #endregion
    }
}
