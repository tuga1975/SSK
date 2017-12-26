using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Common
{
    public class CommonUtil
    {
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
    }
}
