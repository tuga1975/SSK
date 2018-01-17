using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ZXing.QrCode;

namespace Trinity.Common
{
    public class CommonUtil
    {
        public static byte[] CreateLabelQRCode(LabelInfo labelInfo, string AESKey)
        {
            var width = 200; // width of the Qr Code    
            var height = 200; // height of the Qr Code    
            var margin = 0;
            var qrCodeWriter = new ZXing.BarcodeWriterPixelData
            {
                Format = ZXing.BarcodeFormat.DATA_MATRIX,
                //Format = ZXing.BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = height,
                    Width = width,
                    Margin = margin
                }
            };

            var contentQRCode = labelInfo.MarkingNo + "*" + labelInfo.NRIC + "*" + labelInfo.Name + "*" + labelInfo.DrugType + "*SA*URINE";
            var encryptContent = CommonUtil.EncryptString(contentQRCode, AESKey);
            var pixelData = qrCodeWriter.Write(encryptContent);
            // creating a bitmap from the raw pixel data; if only black and white colors are used it makes no difference    
            // that the pixel data ist BGRA oriented and the bitmap is initialized with RGB    
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            var ms = new System.IO.MemoryStream();

            var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, pixelData.Width, pixelData.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            try
            {
                // we assume that the row stride of the bitmap is aligned to 4 byte multiplied by the width of the image    
                System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
            bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

            return ms.ToArray();
        }

        public static string EncryptString(string message, string passphrase)
        {
            byte[] Results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();
            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(passphrase));
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();
            TDESAlgorithm.Key = TDESKey;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;
            byte[] DataToEncrypt = UTF8.GetBytes(message);
            try
            {
                ICryptoTransform Encryptor = TDESAlgorithm.CreateEncryptor();
                Results = Encryptor.TransformFinalBlock(DataToEncrypt, 0, DataToEncrypt.Length);
            }
            finally
            {
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }
            return Convert.ToBase64String(Results);
        }

        public static string DecryptString(string message, string passphrase)
        {
            byte[] Results;
            System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();
            MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
            byte[] TDESKey = HashProvider.ComputeHash(UTF8.GetBytes(passphrase));
            TripleDESCryptoServiceProvider TDESAlgorithm = new TripleDESCryptoServiceProvider();
            TDESAlgorithm.Key = TDESKey;
            TDESAlgorithm.Mode = CipherMode.ECB;
            TDESAlgorithm.Padding = PaddingMode.PKCS7;
            byte[] DataToDecrypt = Convert.FromBase64String(message);
            try
            {
                ICryptoTransform Decryptor = TDESAlgorithm.CreateDecryptor();
                Results = Decryptor.TransformFinalBlock(DataToDecrypt, 0, DataToDecrypt.Length);
            }
            finally
            {
                TDESAlgorithm.Clear();
                HashProvider.Clear();
            }
            return UTF8.GetString(Results);
        }

        public static string GenerateMarkingNumber()
        {
            var currentDate = DateTime.Now;
            var rand = new Random(Guid.NewGuid().GetHashCode());

            //return currentDate.Year.ToString() + currentDate.Month.ToString() + currentDate.Day.ToString() + "_" + rand.Next().ToString();
            return rand.Next().ToString();
        }

        public static string GetDeviceStatusText(EnumDeviceStatuses deviceStatus)
        {
            switch (deviceStatus)
            {
                case EnumDeviceStatuses.Connected:
                    return "Device connected";

                case EnumDeviceStatuses.None:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.Paused:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.Error:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.PendingDeletion:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.PaperJam:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.PaperOut:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.ManualFeed:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.PaperProblem:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.Offline:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.IOActive:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.Busy:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.Printing:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.OutputBinFull:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.NotAvailable:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.Waiting:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.Processing:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.Initializing:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.WarmingUp:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.TonerLow:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.NoToner:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.PagePunt:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.UserIntervention:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.OutOfMemory:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.DoorOpen:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.ServerUnknown:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.PowerSave:
                    return deviceStatus.ToString();

                case EnumDeviceStatuses.Disconnected:
                    return deviceStatus.ToString();

                default:
                    break;
            }
            return "Unknown status";
        }

        public static EnumDayOfWeek ConvertToCustomDateOfWeek(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Sunday:
                    return EnumDayOfWeek.Sunday;
                case DayOfWeek.Monday:
                    return EnumDayOfWeek.Monday;
                case DayOfWeek.Tuesday:
                    return EnumDayOfWeek.Tuesday;
                case DayOfWeek.Wednesday:
                    return EnumDayOfWeek.Wednesday;
                case DayOfWeek.Thursday:
                    return EnumDayOfWeek.Thursday;
                case DayOfWeek.Friday:
                    return EnumDayOfWeek.Friday;
                case DayOfWeek.Saturday:
                    return EnumDayOfWeek.Saturday;
                default:
                    return 0;
            }
        }
       

        public static string GetQueueNumber(string baseOnNRIC)
        {
            string queueNumber = "";
            if (!string.IsNullOrEmpty(baseOnNRIC) && baseOnNRIC.Length > 6)
            {
                //queueNumber += baseOnNRIC.Substring(0, 1) + baseOnNRIC.Substring(baseOnNRIC.Length - 5, 5).PadLeft(8, '*');
                queueNumber += baseOnNRIC.GetLast(5);
            }
            else if (!string.IsNullOrEmpty(baseOnNRIC) && baseOnNRIC.Length <= 6)
            {
                //queueNumber += baseOnNRIC.Substring(0, 1) + baseOnNRIC.PadLeft(8, '*');
                queueNumber += baseOnNRIC;
            }
            else
            {
                queueNumber += null;
            }
            return queueNumber;
        }
    }
}
