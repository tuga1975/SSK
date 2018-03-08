// 2015 Dec 08
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Trinity.Device
{

    /// <summary>
    /// PdiScan API enumerations
    /// </summary>

    public enum pdiscan_ranges : int
    {
        PD_RANGE_CONSTANT_MIN = -3,
        PD_RANGE_CONSTANT_MAX = -2,
        PD_RANGE_CONSTANT_STEP = -1
    }


    /* The only tag that is guaranteed to be supported is PDISCAN_TAG_SUPPORTED_TAGS. The scanner driver
       may choose to create or not create any of the other tags. That said, the driver will usually
       support all of the tags that have any relevance to the driver. If a tag is not created, it is
       probably because the driver itself has no idea what the current setting would be for that tag.
       For example, if a driver is unable to get or set the scanner's resolution, it would probably
       not create the PDISCAN_TAG_RESOLUTION tag, because that would make the parent application
       believe that scanning was occurring at a particular resolution, when it might actually be
       occurring at a different resolution. */
    public enum pdiscan_tags : int
    {
        PDISCAN_TAG_ENUM_LBOUND = 0,
        PDISCAN_TAG_SUPPORTED_TAGS,						// Enumeration, read-only.
        PDISCAN_TAG_SCANNER_NAME,						// String, unrestricted choices, read-only.
        PDISCAN_TAG_RESOLUTION,							// Long, list.
        PDISCAN_TAG_PAPER_SOURCE,						// Enumeration, list.
        PDISCAN_TAG_DUPLEX,								// Boolean, list.
        PDISCAN_TAG_COLOR_DEPTH,						// Enumeration, list.
        PDISCAN_TAG_ORIENTATION,						// Enumeration, list.
        PDISCAN_TAG_PAGE_SIZE,							// Enumeration, list.
        PDISCAN_TAG_CONTRAST_PANEL,						// Boolean, list.
        PDISCAN_TAG_CONTRAST_FRONT,						// Long, range.
        PDISCAN_TAG_CONTRAST_BACK,						// Long, range.
        PDISCAN_TAG_BRIGHTNESS_PANEL,					// Boolean, list.
        PDISCAN_TAG_BRIGHTNESS_FRONT,					// Long, range.
        PDISCAN_TAG_BRIGHTNESS_BACK,					// Long, range.
        PDISCAN_TAG_ENDORSING,							// Boolean, list.
        PDISCAN_TAG_IMPRINTING,							// Boolean, list.
        PDISCAN_TAG_IMPRINTER_VERTICAL_OFFSET,			// Long, range.
        PDISCAN_TAG_IMPRINTER_HORIZONTAL_OFFSET,		// Long, range.
        PDISCAN_TAG_DESKEW,								// Boolean, list.
        PDISCAN_TAG_AUTOCROP,							// Boolean, list.
        PDISCAN_TAG_READ_MICR_LINE,						// Boolean, list.
        PDISCAN_TAG_MICR_TYPE,							// Enumeration, list.
        PDISCAN_TAG_MICR_RESULTS,						// String, unrestricted choices, read-only.
        PDISCAN_TAG_DETECT_DOUBLE_FEEDS,				// Boolean, list.
        PDISCAN_TAG_DOUBLE_FEED_SENSITIVITY,			// Long, range.
        PDISCAN_TAG_DOCUMENT_READ_LENGTH,				// Long, range.
        PDISCAN_TAG_IMPRINTER_HORIZONTAL_OFFSET_BASE,   // Enumeration, list.
        PDISCAN_TAG_IMPRINTER_TEXT_ORIENTATION,			// Enumeration, list.
        PDISCAN_TAG_AUTO_EJECT,							// Boolean, list.
        PDISCAN_TAG_PICKON_COMMAND,						// Boolend, list
        PDISCAN_TAG_HALFSPEED_ON,						// Boolean
        PDISCAN_TAG_SCAN_DELAY,							// Long range
        PDISCAN_TAG_JAMLENGTH,							// Long range
        PDISCAN_TAG_IMAGE_TEMP_PATH,					// String,
        PDISCAN_TAG_EJECT_ON_JAM,						// Enumeration, list.
        PDISCAN_TAG_RETRY_COUNT,						// Long range
        PDISCAN_TAG_CONTROL_MESSAGES,					// Boolean, list
        PDISCAN_TAG_INTENSITY_RED_FRONT,				// Long, range.
        PDISCAN_TAG_INTENSITY_GREEN_FRONT,				// Long, range.
        PDISCAN_TAG_INTENSITY_BLUE_FRONT,				// Long, range.
        PDISCAN_TAG_INTENSITY_RED_BACK,					// Long, range.
        PDISCAN_TAG_INTENSITY_GREEN_BACK,				// Long, range.
        PDISCAN_TAG_INTENSITY_BLUE_BACK,				// Long, range.
        PDISCAN_TAG_GAMMA_RED_CORRECTION_FRONT,			// Long, range.
        PDISCAN_TAG_GAMMA_GREEN_CORRECTION_FRONT,		// Long, range.
        PDISCAN_TAG_GAMMA_BLUE_CORRECTION_FRONT,		// Long, range.
        PDISCAN_TAG_GAMMA_RED_CORRECTION_BACK,			// Long, range.
        PDISCAN_TAG_GAMMA_GREEN_CORRECTION_BACK,		// Long, range.
        PDISCAN_TAG_GAMMA_BLUE_CORRECTION_BACK,			// Long, range.
        PDISCAN_TAG_CONTRAST_RED_FRONT,					// Long, range.
        PDISCAN_TAG_CONTRAST_GREEN_FRONT,				// Long, range.
        PDISCAN_TAG_CONTRAST_BLUE_FRONT,				// Long, range.
        PDISCAN_TAG_CONTRAST_RED_BACK,					// Long, range.
        PDISCAN_TAG_CONTRAST_GREEN_BACK,				// Long, range.
        PDISCAN_TAG_CONTRAST_BLUE_BACK,					// Long, range.
        PDISCAN_TAG_EJECTPAUSE,							// Boolend, list
        PDISCAN_TAG_AUTO_EJECT_DIRECTION,				// Enumeration, list
        PDISCAN_TAG_DOUBLE_FEED_MINLENGTH,				// 
        PDISCAN_TAG_MIN_FRONT_SENSOR_COUNT,				// long, range
        PDISCAN_TAG_REAR_EJECT_SPEED, /// long, renage
        // PDISCAN_FIRMWARE .... TODO
        PDISCAN_TAG_ENUM_UBOUND							// Boolean, list.
    }

    public enum pd_data_types : int
    {
        PD_DATA_TYPE_ENUM_LBOUND = 0,
        PD_DATA_TYPE_LONG,
        PD_DATA_TYPE_STRING,
        PD_DATA_TYPE_BOOLEAN,
        PD_DATA_TYPE_ENUMERATION,
        PD_DATA_TYPE_ENUM_UBOUND
    }

    public enum pd_choice_types : int
    {
        PD_CHOICE_TYPE_ENUM_LBOUND = 0,
        PD_CHOICE_TYPE_LIST,		/* Can be used with any data type. The valid choices are represented as a
                            list of values. */
        PD_CHOICE_TYPE_RANGE,		/* Can be used only with DATA_TYPE_LONG. Any value which is equal to or
                             greater than the minimum value, equal to or less than the maximum value,
                             and can be reached by applying the step value to the minimum value is a
                             valid choice. */
        PD_CHOICE_TYPE_UNRESTRICTED, /* Can be used only with DATA_TYPE_STRING. Any value is a valid
                                    choice. */
        PD_CHOICE_TYPE_ENUM_UBOUND
    }


    /* Potential values of enumerated tags. These values may or may not be available, depending upon
       the particular scanner in use. */

    public enum pd_paper_sources : int
    {
        PD_PAPER_SOURCE_ENUM_LBOUND = 0,
        PD_PAPER_SOURCE_ADF,		// Scan from the scanner's automatic document feeder.
        PD_PAPER_SOURCE_FLATBED,	// Scan from the scanner's flatbed.
        PD_PAPER_SOURCE_MANUAL,		/* Pages must be inserted into the scanner one at a time, by hand. This
                               would also apply to a scanner which had a "feeder" so small that it
                               could only hold one page at a time. (One of the check scanner models
                               had a feeder like this.) */
        PD_PAPER_SOURCE_PANEL,		/* The location to scan from is controlled by the operator from the
                              scanner's panel. */
        PD_PAPER_SOURCE_AUTOMATIC,	/* The scanner has some way of detecting which of the available feeders
                                  or flatbeds have a page in them, and will automatically scan from a
                                  place that has a page. */
        PD_PAPER_SOURCE_ENUM_UBOUND
    }

    public enum pd_color_depths : int
    {
        PD_COLOR_DEPTH_ENUM_LBOUND = 0,
        PD_COLOR_DEPTH_BITONAL,
        PD_COLOR_DEPTH_4_BIT_GRAYSCALE,
        PD_COLOR_DEPTH_8_BIT_GRAYSCALE,
        PD_COLOR_DEPTH_8_BIT_COLOR,
        PD_COLOR_DEPTH_24_BIT_COLOR,
        PD_COLOR_DEPTH_8_BIT_GRAYDUAL,
        PD_COLOR_DEPTH_ENUM_UBOUND
    }

    public enum pd_orientations : int
    {
        PD_ORIENTATION_ENUM_LBOUND = 0,
        PD_ORIENTATION_PORTRAIT,
        PD_ORIENTATION_LANDSCAPE,
        PD_ORIENTATION_ENUM_UBOUND
    }

    /* The A0-A6 and B0-B6 page sizes are ISO standard page sizes and are officially defined in
       millimeters. The other page sizes are, as near as I can tell, American or nonstandard page
       sizes. There is a good discussion of the ISO page sizes at
       http://www.cl.cam.ac.uk/~mgk25/iso-paper.html. Note that ISO also defines a "C" series of
       envelope sizes which we do not support.
       The scanner drivers return images that are "close" to these dimensions. They may not return
       images that are exactly the correct size, but they are as close as reasonably possible. */
    public enum pd_page_sizes : int
    {
        PD_PAGE_SIZE_ENUM_LBOUND = 0,
        PD_PAGE_SIZE_A0,				// 841 x 1189 millimeters.
        PD_PAGE_SIZE_A1,				// 594 x 841 millimeters.
        PD_PAGE_SIZE_A2,				// 420 x 594 millimeters.
        PD_PAGE_SIZE_A3,				// 297 x 420 millimeters.
        PD_PAGE_SIZE_A4,				// 210 x 297 millimeters.
        PD_PAGE_SIZE_A5,				// 148 x 210 millimeters.
        PD_PAGE_SIZE_A6,				// 105 x 148 millimeters.
        PD_PAGE_SIZE_B0,				// 1000 x 1414 millimeters.
        PD_PAGE_SIZE_B1,				// 707 x 1000 millimeters.
        PD_PAGE_SIZE_B2,				// 500 x 707 millimeters.
        PD_PAGE_SIZE_B3,				// 353 x 500 millimeters.
        PD_PAGE_SIZE_B4,				// 250 x 353 millimeters.
        PD_PAGE_SIZE_B5,				// 176 x 250 millimeters.
        PD_PAGE_SIZE_B6,				// 125 x 176 millimeters.
        PD_PAGE_SIZE_LETTER,			// 8.5 x 11 inches. Also called "A".
        PD_PAGE_SIZE_LEGAL,				// 8.5 x 14 inches.
        PD_PAGE_SIZE_LEGAL_LONG,		// 8.5 x 17 inches.
        PD_PAGE_SIZE_B,					// 11 x 17 inches. Also called "Ledger" or "Double Letter".
        PD_PAGE_SIZE_PERSONAL_CHECK,	// 5 x 3 inches.
        PD_PAGE_SIZE_BUSINESS_CHECK,	// 8.5 x 4 inches.
        PD_PAGE_SIZE_COUPON,			// 1 x 4 inches.
        PD_PAGE_SIZE_MAXIMUM_SIZE,		/* The maximum area that the scanner can scan. This is different for
                               different scanners, and might also change depending upon other
                               things that are outside the library's control (control panel
                               settings, etc). */
        PD_PAGE_SIZE_ENUM_UBOUND
    }

    public enum pd_micr_types : int
    {
        PD_MICR_TYPE_ENUM_LBOUND = 0,
        PD_MICR_TYPE_E13B,
        PD_MICR_TYPE_CMC7,
        PD_MICR_TYPE_ENUM_UBOUND
    }

    public enum pd_imprinter_horizontal_offset_bases : int
    {
        PD_IMPRINTER_HORIZONTAL_OFFSET_BASE_ENUM_LBOUND = 0,
        PD_IMPRINTER_HORIZONTAL_OFFSET_BASE_LEADING_EDGE,
        PD_IMPRINTER_HORIZONTAL_OFFSET_BASE_TRAILING_EDGE,
        PD_IMPRINTER_HORIZONTAL_OFFSET_BASE_ENUM_UBOUND
    }

    public enum pd_imprinter_text_orientations : int
    {
        PD_IMPRINTER_TEXT_ORIENTATION_ENUM_LBOUND = 0,
        PD_IMPRINTER_TEXT_ORIENTATION_0,
        PD_IMPRINTER_TEXT_ORIENTATION_90,
        PD_IMPRINTER_TEXT_ORIENTATION_180,
        PD_IMPRINTER_TEXT_ORIENTATION_270,
        PD_IMPRINTER_TEXT_ORIENTATION_ENUM_UBOUND
    }

    public enum pdiscan_errors : int
    {
        PDISCAN_ERR_ENUM_LBOUND = -1,
        // No error; the function completed successfully.
        PDISCAN_ERR_NONE,
        PDISCAN_ERR_INTERNAL,
        PDISCAN_ERR_FUNCTION_CALL_SEQUENCE,
        PDISCAN_ERR_INVALID_PARAM,
        PDISCAN_ERR_FILE_OPEN_READ,
        PDISCAN_ERR_FILE_OPEN_WRITE,
        PDISCAN_ERR_FILE_READ,
        PDISCAN_ERR_FILE_WRITE,
        PDISCAN_ERR_OUT_OF_MEMORY,
        PDISCAN_ERR_CANNOT_LOAD_LIBRARY,
        PDISCAN_ERR_UNEXPECTED,									// 10
        PDISCAN_ERR_ASSERTION_FAILURE,
        PDISCAN_ERR_SCANNER_NOT_FOUND,
        PDISCAN_ERR_FEEDER_EMPTY,
        PDISCAN_ERR_PAPER_JAM,
        PDISCAN_ERR_SCANNER_ERROR,
        PDISCAN_ERR_DOUBLE_FEED,
        PDISCAN_ERR_COVER_OPEN,
        PDISCAN_ERR_INVALID_TAG,
        PDISCAN_ERR_STRING_BUFFER_TOO_SMALL,
        PDISCAN_ERR_THREAD_FATAL,								// 20
        PDISCAN_ERR_DISCONNECTED,
        PDISCAN_ERR_NOT_SUPPORTED_WINDOWS,
        PDISCAN_ERR_NOT_SUPPORTED_ASYNCH,
        PDISCAN_ERR_NOT_SUPPORTED_RAWMODE,
        PDISCAN_ERR_STATUS_NOT_OK,
        PDISCAN_ERR_BEGINSCAN,
        PDISCAN_ERR_ENDSCAN,
        PDISCAN_ERR_REAR_SENSORS_BLOCKED,
        PDISCAN_ERR_EJECT_PAUSED,
        PDISCAN_ERR_EJECT_RESUMED,								// 30
        PDISCAN_ERR_LATCH_RELEASED,
        PDISCAN_ERR_COVER_CLOSED,
        PDISCAN_ERR_IMAGE,
        PDISCAN_ERR_DESKEW,
        PDISCAN_ERR_AUTOCROP,
        PDISCAN_ERR_DISCONNECT_NEEDED,
        PDISCAN_ERR_IMAGE_ENHANCEMENT,
        PDISCAN_ERR_COMMAND_NOT_SUPPORTED,

        PDISCAN_ERR_OPTICON = 70,								// opticon barcode reader errors... 70 to 79...

        PDISCAN_ERR_ENUM_UBOUND
    }


    public enum pd_callback_types : int
    {
        PD_CALLBACK_TYPE_ENUM_LBOUND = 0,
        PD_CALLBACK_TYPE_IMPRINTER_STRING,
        PD_CALLBACK_TYPE_PAGE_EJECT,
        PD_CALLBACK_TYPE_PAGE_END,
        PD_CALLBACK_TYPE_SCANNING_ERROR,
        PD_CALLBACK_TYPE_IMAGE_INFORMATION,
        PD_CALLBACK_TYPE_REMOVED_IMAGE_FILENAME,
        PD_CALLBACK_TYPE_CUSTOM_MESSAGE,
        PD_CALLBACK_TYPE_PROCESSED_DATA,
        PD_CALLBACK_TYPE_ENUM_UBOUND
    }

    public enum pd_image_information_types : int
    {
        PD_IMAGE_INFORMATION_ENUM_LBOUND = 0,
        PD_IMAGE_INFORMATION_SKEW_ANGLE,
        PD_IMAGE_INFORMATION_LOCATION,
        PD_IMAGE_INFORMATION_ENUM_UBOUND
    }


    public enum pd_eject_directions : int
    {
        PD_EJECT_DIRECTION_ENUM_LBOUND = 0,
        PD_EJECT_DIRECTION_FRONT,
        PD_EJECT_DIRECTION_BACK,
        PD_EJECT_DIRECTION_WAIT,
        PD_EJECT_DIRECTION_RESCAN,
        PD_EJECT_DIRECTION_FRONT_HOLD,
        PD_EJECT_DIRECTION_FORCE_FRONT,
        PD_EJECT_DIRECTION_FORCE_BACK,
        PD_EJECT_DIRECTION_ENUM_UBOUND
    }


    public enum pd_diagnostic_function_types : int
    {
        PD_DIAG_FUNC_TYPE_ENUM_LBOUND = 0,
        PD_DIAG_FUNC_TYPE_CONNECT_IN_RAW_MODE,				//1
        PD_DIAG_FUNC_TYPE_SEND_RAW_MODE_MESSAGE,			//2
        PD_DIAG_FUNC_TYPE_ERASE_FIRMWARE,					//3
        PD_DIAG_FUNC_TYPE_UPLOAD_FIRMWARE_FROM_MEMORY,		//4
        PD_DIAG_FUNC_TYPE_UPLOAD_FIRMWARE_FROM_FILE,		//5
        PD_DIAG_FUNC_TYPE_GET_CALIBRATION_DATA,				//6
        PD_DIAG_FUNC_TYPE_SET_CALIBRATION_DATA,				//7
        PD_DIAG_FUNC_TYPE_CHANGE_DESCRAMBLING_STATE,		//8
        PD_DIAG_FUNC_TYPE_CHANGE_CALIBRATION_STATE,			//9
        PD_DIAG_FUNC_TYPE_DEALLOCATE_MEMORY_BUFFER,			//10
        PD_DIAG_FUNC_TYPE_DESCRAMBLE_IMAGES,				//11
        PD_DIAG_FUNC_TYPE_AUTOCROP_IMAGE,					//12
        PD_DIAG_FUNC_TYPE_GET_SCANNER_STATUS,				//13
        PD_DIAG_FUNC_TYPE_RESET_SCANNER,					//14
        PD_DIAG_FUNC_TYPE_RESET_CPLD,						//15
        PD_DIAG_FUNC_TYPE_CHANGE_LOGGING_STATE,				//16
        PD_DIAG_FUNC_TYPE_PERFORM_IMAGE_CALIBRATION,		//17
        PD_DIAG_FUNC_TYPE_PERFORM_SPEED_CALIBRATION,		//18
        PD_DIAG_FUNC_TYPE_DESKEW_IMAGE,						//19
        PD_DIAG_FUNC_TYPE_PERFORM_RAW_MODE_IMPRINTING,		//20
        PD_DIAG_FUNC_TYPE_GET_SERIAL_NUMBER,				//21
        PD_DIAG_FUNC_TYPE_SET_SERIAL_NUMBER,				//22
        PD_DIAG_FUNC_TYPE_SET_BRIGHTNESS_AS_DEFAULT,		//23
        PD_DIAG_FUNC_TYPE_EJECT_BACK,						//24
        PD_DIAG_FUNC_TYPE_EJECT_FRONT,						//25
        PD_DIAG_FUNC_TYPE_OVERRIDE_JAM,						//26
        PD_DIAG_FUNC_TYPE_UNLOCK_COVER,						//27
        PD_DIAG_FUNC_TYPE_GET_BARCODE_RESULT,				//28
        PD_DIAG_FUNC_TYPE_GET_ONE_LINE,						//29
        PD_DIAG_FUNC_TYPE_UPDATE_CALIBRATION_DATA,			//30
        PD_DIAG_FUNC_TYPE_PERFORM_DOUBLE_FEED_CALIBRATION, //31
        PD_DIAG_FUNC_TYPE_ENUM_UBOUND
    }

    public enum pd_imprint_types : int
    {
        PD_IMPRINT_TYPE_ENUM_LBOUND = 0,
        PD_IMPRINT_TYPE_TEXT,
        PD_IMPRINT_TYPE_DIB,
        PD_IMPRINT_ENUM_UBOUND
    }

    public enum pd_imprint_directions : int
    {
        PD_IMPRINT_DIRECTION_ENUM_LBOUND = 0,
        PD_IMPRINT_DIRECTION_FORWARD,
        PD_IMPRINT_DIRECTION_BACKWARD,
        PD_IMPRINT_DIRECTION_ENUM_UBOUND
    }


    [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
    public struct PD_IMPRINT_SETUP
    {
        public int iOffset;
        public pd_imprint_directions eDirection;
        public pd_imprinter_horizontal_offset_bases eBase;
        public pd_imprinter_text_orientations eTextOrientation;
    }

    public enum pd_process_types
    {
        PD_PROCESS_ENUM_LBOUND = 0,
        PD_PROCESS_OMR,
        PD_PROCESS_BARCODE,
        PD_ERROR_CODE,
        PD_PROCESS_ENUM_UBOUND,
    }

    public enum pd_buffer_types
    {
        PD_BUFFER_ENUM_LBOUND = 0,
        PD_BUFFER_ASCII,
        PD_BUFFER_BINARY,
        PD_BUFFER_ENUM_UBOUND,
    }

    public enum pd_page_types
    {
        PD_PAGE_ENUM_LBOUND = 0,
        PD_PAGE_FRONT,
        PD_PAGE_BACK,
        PD_PAGE_ENUM_UBOUND,
    }


    /// <summary>
    /// PdiScan API Wrapper For .NET
    /// </summary>
    public class PdiScanWrap
    {
        // PDIScan Callback functions
        public delegate void PageEndCallback(int PageNumber, IntPtr FrontSideDIB, IntPtr BackSideDIB, ref int AbortRequested, IntPtr UserData);
        public delegate void PageEjectCallback(int PageNumber, ref pd_eject_directions EjectDirection, IntPtr UserData);
        public delegate void ScanningErrorCallback(pdiscan_errors ScanningError, [MarshalAs(UnmanagedType.LPStr)] string ExtraInfo, IntPtr UserData);
        public delegate void ImprinterStringCallback(int PageNumber, IntPtr ImprinterString, IntPtr UserData);
        public delegate void PageProcessedDataCallback(int PageNumber, pd_page_types ePage, pd_process_types eType, int CurrentElement, int MaxElements, int DataSize, System.IntPtr Data, pd_buffer_types eBuf, System.IntPtr UserData);


        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdInstallCallback(IntPtr ScanningHandle, pd_callback_types WhichCallback, PageEndCallback CallbackFunction, IntPtr UserData);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdInstallCallback(IntPtr ScanningHandle, pd_callback_types WhichCallback, PageEjectCallback CallbackFunction, IntPtr UserData);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdInstallCallback(IntPtr ScanningHandle, pd_callback_types WhichCallback, ScanningErrorCallback CallbackFunction, IntPtr UserData);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdInstallCallback(IntPtr ScanningHandle, pd_callback_types WhichCallback, ImprinterStringCallback CallbackFunction, IntPtr UserData);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdInstallCallback(IntPtr ScanningHandle, pd_callback_types WhichCallback, PageProcessedDataCallback CallbackFunction, IntPtr UserData);

        // PDIScan Communication functions
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdAllocateScanningHandle(out IntPtr ScanningHandle, out IntPtr ExtraInfo);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdDeallocateScanningHandle(IntPtr ScanningHandle);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdGetScannerListCount(IntPtr ScanningHandle, out int Count);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdGetScannerListItem(IntPtr ScanningHandle, int ScannerListIndex, IntPtr pScannerName);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdConnectToScanner(IntPtr ScanningHandle, [MarshalAs(UnmanagedType.LPStr)] string ScannerName, uint AsynchronousMode);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdDisconnectFromScanner(IntPtr ScanningHandle);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdEnableFeeder(IntPtr ScanningHandle);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdDisableFeeder(IntPtr ScanningHandle);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdScan(IntPtr ScanningHandle, uint SinglePage);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdLoadScannerConfiguration(IntPtr ScanningHandle, [MarshalAs(UnmanagedType.LPStr)] string Filename);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdSaveScannerConfiguration(IntPtr ScanningHandle, [MarshalAs(UnmanagedType.LPStr)] string Filename);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdEjectDocument(IntPtr ScanningHandle, int EjectDirection);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdSetImprinterBitmap(IntPtr ScanningHandle, IntPtr DIBToImprint);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdImprintDocument(System.IntPtr ScanningHandle, pd_imprint_types eType, System.IntPtr pData, ref PD_IMPRINT_SETUP pInfo);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern pdiscan_errors PdGetScannerStatus(IntPtr ScanningHandle, IntPtr Status);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdResetScanner(IntPtr ScanningHandle, [MarshalAs(UnmanagedType.LPStr)] string ScannerName, int Type);

        // Helper functions
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdDibEnhancements(IntPtr ScanningHandle, System.IntPtr hDib,
                                                int iContrastR, int iContrastG, int iContrastB,
                                                int iIntensityR, int iIntensityG, int iIntensityB,
                                                float fGammaRed, float fGammaGreen, float fGammaBlue, out IntPtr pNewDib);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdSaveImageToDisk(IntPtr Dib, [MarshalAs(UnmanagedType.LPStr)] string Filename);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdLoadImageFromDisk([MarshalAs(UnmanagedType.LPStr)] string Filename, out IntPtr Dib);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdRotateImage(IntPtr Dib, int rot, out IntPtr ResDib);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdReleaseImageHandle(IntPtr Dib);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdDeskewDib(IntPtr ScanningHandle, IntPtr hDib, out IntPtr hNewDib);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdAutoCropDib(IntPtr ScanningHandle, IntPtr hDib, out IntPtr hNewDib);


        // PDIScan Scanner tag functions
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdGetTagDataType(IntPtr ScanningHandle, int TagID, out pd_data_types DataType);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdGetTagChoiceType(IntPtr ScanningHandle, int TagID, out pd_choice_types ChoiceType);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdIsTagReadOnly(IntPtr ScanningHandle, int TagID, out uint ReadOnly);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdGetTagChoiceCount(IntPtr ScanningHandle, int TagID, out int Count);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdGetTagChoiceLong(IntPtr ScanningHandle, int TagID, int Index, out int Value);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdGetTagChoiceString(IntPtr ScanningHandle, int TagID, int Index, IntPtr Value, ref int ValueBufferLength);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdGetTagDefaultLong(IntPtr ScanningHandle, int TagID, out int Value);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdGetTagDefaultString(IntPtr ScanningHandle, int TagID, IntPtr Value, ref int ValueBufferLength);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdGetTagLong(IntPtr ScanningHandle, int TagID, out int Value);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdGetTagString(IntPtr ScanningHandle, int TagID, IntPtr Value, ref int ValueBufferLength);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdSetTagLong(IntPtr ScanningHandle, int TagID, int Value);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdSetTagString(IntPtr ScanningHandle, int TagID, [MarshalAs(UnmanagedType.LPStr)] string Value);


        // PDIScan Error functions
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void PdGetErrorShortDescription(IntPtr ScanningHandle, pdiscan_errors ErrorCode, IntPtr ShortDescription);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void PdGetErrorLongDescription(IntPtr ScanningHandle, pdiscan_errors ErrorCode, IntPtr LongDescription);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void PdGetErrorExtraInfo(IntPtr ScanningHandle, IntPtr ExtraInfo);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void PdGetErrorSourceFileName(IntPtr ScanningHandle, IntPtr SourceFileName);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void PdGetErrorSourceFileLineNumber(IntPtr ScanningHandle, out int SourceFileLineNumber);


        // PDIScan Diagnostic functions
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdGetFirmwareInformation(IntPtr ScanningHandle, IntPtr ScannerType, IntPtr FirmwareVersion, IntPtr CPLDVersion);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdDiagnosticFunction(IntPtr ScanningHandle, int Type, IntPtr Param1, IntPtr Param2, IntPtr Param3, IntPtr Param4, IntPtr Param5);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern pdiscan_errors PdInternalFunction(IntPtr ScanningHandle, int Type, IntPtr Param1, IntPtr Param2, IntPtr Param3, IntPtr Param4, IntPtr Param5);
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
        public static extern pdiscan_errors PdSetDiagnosticOptions(IntPtr ScanningHandle, int iOptions);

        // UT --- life time info:
        [DllImport("pdiscan.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void PdGetLifeTimeInfo(IntPtr ScanningHandle, int Type, IntPtr LifeTimeInfo);

        // Windows functions
        [DllImport("kernel32.dll")]
        public static extern IntPtr GlobalLock(IntPtr hMem);
        [DllImport("kernel32.dll")]
        public static extern UIntPtr GlobalSize(IntPtr hMem);
        [DllImport("kernel32.dll")]
        public static extern bool GlobalUnlock(IntPtr hMem);
        [DllImport("kernel32.dll")]
        public static extern IntPtr GlobalFree(IntPtr handle);
    }
}
