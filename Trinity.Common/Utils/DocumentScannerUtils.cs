using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Trinity.Common.Common;
using Trinity.BE;
using WIA;

namespace Trinity.Common.Utils
{
    class DocumentScannerUtils
    {
        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile DocumentScannerUtils _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private DocumentScannerUtils() { }

        public static DocumentScannerUtils Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new DocumentScannerUtils();
                    }
                }

                return _instance;
            }
        }
        #endregion

        public DocumentScannerResult StartScanning()
        {
            // Create returnValue
            DocumentScannerResult returnValue = new DocumentScannerResult()
            {
                Success = false,
                Value = null,
                FailedInfo = null
            };

            try
            {
                // Create a DeviceManager instance
                DeviceManager deviceManager = new DeviceManager();

                // Create an empty variable to store the scanner instance
                DeviceInfo firstScannerAvailable = null;

                // Loop through the list of devices
                for (int i = 1; i <= deviceManager.DeviceInfos.Count; i++)
                {
                    // Skip the device if it's not a scanner
                    if (deviceManager.DeviceInfos[i].Type != WiaDeviceType.ScannerDeviceType)
                    {
                        continue;
                    }


                    firstScannerAvailable = deviceManager.DeviceInfos[i];

                    // refer https://msdn.microsoft.com/en-us/library/windows/desktop/ms630318(v=vs.85).aspx
                    // Print something like e.g "WIA Canoscan 4400F"
                    Debug.WriteLine(deviceManager.DeviceInfos[i].Properties["Name"].get_Value());
                    // e.g Canoscan 4400F
                    //Console.WriteLine(deviceManager.DeviceInfos[i].Properties["Description"].get_Value());
                    // e.g \\.\Usbscan0
                    //Console.WriteLine(deviceManager.DeviceInfos[i].Properties["Port"].get_Value());
                    break;
                }

                // return if there is no avaiable scanner
                if (firstScannerAvailable == null)
                {
                    returnValue.FailedInfo = new FailedInfo()
                    {
                        ErrorCode = -1,
                        ErrorMessage = new ErrorInfo().GetErrorMessage(EnumErrorCodes.DocumentScannerNull)
                    };
                    return returnValue;
                }

                // Connect to the first available scanner
                var device = firstScannerAvailable.Connect();

                // Select the scanner
                var scannerItem = device.Items[1];

                //Set the scanner settings
                int resolution = 150;
                int width_pixel = 1250;
                int height_pixel = 1700;
                int color_mode = 1;
                AdjustScannerSettings(scannerItem, resolution, 0, 0, width_pixel, height_pixel, 0, 0, color_mode);

                // Retrieve a image in JPEG format and store it into a variable
                var imageFile = (ImageFile)scannerItem.Transfer(FormatID.wiaFormatJPEG);

                // Save the image in some path with filename
                var path = @"C:\Users\DucTu\Desktop\scan.jpeg";

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                // Save image !
                imageFile.SaveFile(path);

                // Set returnValue
                returnValue.Value = imageFile;
                return returnValue;
            }
            catch (COMException e)
            {
                // Convert the error code to UINT
                long errorCode = (uint)e.ErrorCode;

                returnValue.FailedInfo = new FailedInfo()
                {
                    ErrorCode = e.ErrorCode
                };

                // See the error codes
                Debug.WriteLine("DocumentScannerUtils.Start() Exception: ");
                if (errorCode == 0x80210006)
                {
                    Debug.WriteLine("The scanner is busy or isn't ready.");
                    returnValue.FailedInfo.ErrorMessage = "The scanner is busy or isn't ready.";
                }
                else if (errorCode == 0x80210064)
                {
                    Debug.WriteLine("The scanning process has been cancelled.");
                    returnValue.FailedInfo.ErrorMessage = "The scanning process has been cancelled.";
                }
                else if (errorCode == 0x8021000C)
                {
                    Debug.WriteLine("There is an incorrect setting on the WIA device.");
                    returnValue.FailedInfo.ErrorMessage = "There is an incorrect setting on the WIA device.";
                }
                else if (errorCode == 0x80210005)
                {
                    Debug.WriteLine("The device is offline. Make sure the device is powered on and connected to the PC.");
                    returnValue.FailedInfo.ErrorMessage = "The device is offline. Make sure the device is powered on and connected to the PC.";
                }
                else if (errorCode == 0x80210001)
                {
                    Debug.WriteLine("An unknown error has occurred with the WIA device.");
                    returnValue.FailedInfo.ErrorMessage = "An unknown error has occurred with the WIA device.";
                }

                return returnValue;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DocumentScannerUtils.Start() Exception: ");
                Debug.WriteLine(ex.Message);

                returnValue.FailedInfo = new FailedInfo()
                {
                    ErrorCode = -2,
                    ErrorMessage = "An unknown error has occurred."
                };
                return returnValue;
            }
        }

        // for testing purpose
        public EnumDeviceStatuses[] GetDeviceStatus()
        {
            return new EnumDeviceStatuses[] { EnumDeviceStatuses.Connected };
        }

        private static void AdjustScannerSettings(IItem scannnerItem, int scanResolutionDPI, int scanStartLeftPixel, int scanStartTopPixel, int scanWidthPixels, int scanHeightPixels, int brightnessPercents, int contrastPercents, int colorMode)
        {
            const string WIA_SCAN_COLOR_MODE = "6146";
            const string WIA_HORIZONTAL_SCAN_RESOLUTION_DPI = "6147";
            const string WIA_VERTICAL_SCAN_RESOLUTION_DPI = "6148";
            const string WIA_HORIZONTAL_SCAN_START_PIXEL = "6149";
            const string WIA_VERTICAL_SCAN_START_PIXEL = "6150";
            const string WIA_HORIZONTAL_SCAN_SIZE_PIXELS = "6151";
            const string WIA_VERTICAL_SCAN_SIZE_PIXELS = "6152";
            const string WIA_SCAN_BRIGHTNESS_PERCENTS = "6154";
            const string WIA_SCAN_CONTRAST_PERCENTS = "6155";
            SetWIAProperty(scannnerItem.Properties, WIA_HORIZONTAL_SCAN_RESOLUTION_DPI, scanResolutionDPI);
            SetWIAProperty(scannnerItem.Properties, WIA_VERTICAL_SCAN_RESOLUTION_DPI, scanResolutionDPI);
            SetWIAProperty(scannnerItem.Properties, WIA_HORIZONTAL_SCAN_START_PIXEL, scanStartLeftPixel);
            SetWIAProperty(scannnerItem.Properties, WIA_VERTICAL_SCAN_START_PIXEL, scanStartTopPixel);
            SetWIAProperty(scannnerItem.Properties, WIA_HORIZONTAL_SCAN_SIZE_PIXELS, scanWidthPixels);
            SetWIAProperty(scannnerItem.Properties, WIA_VERTICAL_SCAN_SIZE_PIXELS, scanHeightPixels);
            SetWIAProperty(scannnerItem.Properties, WIA_SCAN_BRIGHTNESS_PERCENTS, brightnessPercents);
            SetWIAProperty(scannnerItem.Properties, WIA_SCAN_CONTRAST_PERCENTS, contrastPercents);
            SetWIAProperty(scannnerItem.Properties, WIA_SCAN_COLOR_MODE, colorMode);
        }

        private static void SetWIAProperty(IProperties properties, object propName, object propValue)
        {
            Property prop = properties.get_Item(ref propName);
            prop.set_Value(ref propValue);
        }
    }
}
