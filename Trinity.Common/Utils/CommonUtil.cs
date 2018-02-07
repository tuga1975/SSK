﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ZXing.QrCode;

namespace Trinity.Common
{
    public class CommonUtil
    {
        public static byte[] CreateLabelQRCode(LabelInfo labelInfo, string AESKey, bool printUB = false)
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
            string contentQRCode = string.Empty;
            //QR code on both MUB and UB label will follow the data size = 91 characters in total 
            if (printUB)
            {
                contentQRCode = labelInfo.MarkingNo + "*" + labelInfo.NRIC.PadLeft(9, '0') + "*" + labelInfo.Name.PadLeft(60, '_') + "*" + labelInfo.DrugType.PadLeft(7, '_') + "*";
            }
            else
            {
                contentQRCode = labelInfo.MarkingNo + "*" + labelInfo.NRIC.PadLeft(9, '0') + "*" + labelInfo.Name.PadLeft(60, '_') + "*" + "_".PadLeft(8, '_');
            }

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

        private static string GenerateKey()
        {
            RijndaelManaged aesEncryption = new RijndaelManaged();
            aesEncryption.KeySize = 256;
            aesEncryption.BlockSize = 128;
            aesEncryption.Mode = CipherMode.CBC;
            aesEncryption.Padding = PaddingMode.PKCS7;
            aesEncryption.GenerateIV();
            string ivStr = Convert.ToBase64String(aesEncryption.IV);
            aesEncryption.GenerateKey();
            string keyStr = Convert.ToBase64String(aesEncryption.Key);
            //string completeKey = ivStr + "," + keyStr;

            //return Convert.ToBase64String(ASCIIEncoding.UTF8.GetBytes(completeKey));

            return keyStr;
        }

        public static string EncryptString(string message, string passphrase)
        {
            byte[] encrypted;
            byte[] IV;

            using (Aes aesAlg = Aes.Create())
            {
                System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();
                MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
                byte[] Key = HashProvider.ComputeHash(UTF8.GetBytes(passphrase));
                aesAlg.Key = Key;

                aesAlg.GenerateIV();
                IV = aesAlg.IV;

                aesAlg.Mode = CipherMode.CBC;

                var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption. 
                using (var msEncrypt = new MemoryStream())
                {
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(message);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            var combinedIvCt = new byte[IV.Length + encrypted.Length];
            Array.Copy(IV, 0, combinedIvCt, 0, IV.Length);
            Array.Copy(encrypted, 0, combinedIvCt, IV.Length, encrypted.Length);

            // Return the encrypted bytes from the memory stream. 
            return Convert.ToBase64String(combinedIvCt);
        }

        public static string DecryptString(string message, string passphrase)
        {
            string plaintext = null;

            // Create an Aes object 
            // with the specified key and IV. 
            using (Aes aesAlg = Aes.Create())
            {
                System.Text.UTF8Encoding UTF8 = new System.Text.UTF8Encoding();
                MD5CryptoServiceProvider HashProvider = new MD5CryptoServiceProvider();
                byte[] Key = HashProvider.ComputeHash(UTF8.GetBytes(passphrase));
                aesAlg.Key = Key;

                byte[] IV = new byte[aesAlg.BlockSize / 8];
                byte[] dataToDecrypt = Convert.FromBase64String(message);
                byte[] cipherText = new byte[dataToDecrypt.Length - IV.Length];

                Array.Copy(dataToDecrypt, IV, IV.Length);
                Array.Copy(dataToDecrypt, IV.Length, cipherText, 0, cipherText.Length);

                aesAlg.IV = IV;

                aesAlg.Mode = CipherMode.CBC;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption. 
                using (var msDecrypt = new MemoryStream(cipherText))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
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

        public static string GetQueueNumber(string nric)
        {
            if (string.IsNullOrEmpty(nric))
            {
                return string.Empty;
            }
            if (nric.Length <= 5)
            {
                return nric;
            }
            return nric.Substring(nric.Length - 5);
        }
    }
}
