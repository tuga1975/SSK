using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Device.Util
{
    public class SmartCardUtil
    {
        private static string _cardUID;
        private static SmartCardData_Original _cardData_Original;

        public static bool IsDataValid
        {
            get
            {
                if (!string.IsNullOrEmpty(UserRole))
                {
                    return true;
                }

                return false;
            }
        }

        public static string CardUID
        {
            get { return _cardUID; }
        }

        public static string UserRole
        {
            get
            {
                if (DutyOfficerData != null)
                {
                    return EnumUserRoles.DutyOfficer;
                }
                else if (SuperviseeBiodata != null)
                {
                    return EnumUserRoles.Supervisee;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public static DutyOfficerData DutyOfficerData
        {
            get { return _cardData_Original?.DutyOfficerData; }
        }

        public static SuperviseeBiodata SuperviseeBiodata
        {
            get { return _cardData_Original?.SuperviseeBiodata; }
        }

        public static List<HistoricalRecord> HistoricalRecords
        {
            get { return _cardData_Original?.HistoricalRecords; }
        }

        public static bool ReadData()
        {
            try
            {
                SmartCardReaderUtil smartCardReaderUtil = SmartCardReaderUtil.Instance;

                // get card UID
                string cardUID = smartCardReaderUtil.GetCardUID();
                if (string.IsNullOrEmpty(cardUID))
                {
                    throw new Exception("Can not get card UID");
                }

                // get card data
                SmartCardData_Original smartCardData_Original = null;
                bool readDataResult = smartCardReaderUtil.ReadAllData_MifareClassic(ref smartCardData_Original);
                if (!readDataResult)
                {
                    throw new Exception("Can not get card data");
                }

                if (smartCardData_Original == null)
                {
                    throw new Exception("Card data is null");
                }

                _cardUID = cardUID;
                _cardData_Original = smartCardData_Original;

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetData exception: " + ex.ToString());
                return false;
            }
        }

        public static SuperviseeBiodata GetSuperviseeBiodata()
        {
            return _cardData_Original?.SuperviseeBiodata;
        }

        public static List<HistoricalRecord> GetAllHistoricalRecords()
        {
            return _cardData_Original?.HistoricalRecords;
        }

        public static List<HistoricalRecord> GetHistoricalRecords(DateTime reportingDate)
        {
            if (_cardData_Original == null || _cardData_Original.HistoricalRecords == null)
            {
                return null;
            }

            return _cardData_Original.HistoricalRecords
                .Where(record => DbFunctions.TruncateTime(record.ReportingDate) == DbFunctions.TruncateTime(reportingDate)).ToList();
        }

        public static List<HistoricalRecord> GetHistoricalRecords(DateTime fromDate, DateTime toDate)
        {
            if (_cardData_Original == null || _cardData_Original.HistoricalRecords == null)
            {
                return null;
            }

            return _cardData_Original.HistoricalRecords
                .Where(record => record.ReportingDate >= fromDate && record.ReportingDate <= toDate).ToList();
        }

        public static bool AppendHistoricalRecord(HistoricalRecord record)
        {
            try
            {
                // validate


                // write data to smart card
                bool actionResult = SmartCardReaderUtil.Instance.AppendHistoricalRecord(_cardData_Original, record);

                // get new data from smart card
                //if (actionResult)
                //{
                //    actionResult = ReadData_FromSmartCard();
                //}

                // return value
                return actionResult;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WriteHistoricalRecord exception: " + ex.ToString());
                return false;
            }

        }

        public static bool UpdateSuperviseeBiodata(SuperviseeBiodata superviseeBiodata)
        {
            try
            {
                // validate


                // write data to smart card
                bool actionResult = SmartCardReaderUtil.Instance.WriteSuperviseeBiodata(superviseeBiodata);

                // get new data from smart card
                //if (actionResult)
                //{
                //    actionResult = ReadData_FromSmartCard();
                //}

                // return value
                return actionResult;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WriteHistoricalRecord exception: " + ex.ToString());
                return false;
            }
        }

        public static void Remove()
        {
            _cardUID = null;
            _cardData_Original = null;
        }
    }

    public class PrintAndWriteSmartCardInfo
    {
        public string FrontCardImagePath { get; set; }
        public string BackCardImagePath { get; set; }
        public SuperviseeBiodata SuperviseeBiodata { get; set; }
        public DutyOfficerData DutyOfficerData { get; set; }
    }

    public class PrintAndWriteCardResult
    {
        public bool Success { get; set; }
        public string Description { get; set; }
        public string CardUID { get; set; }
        //public SmartCardData_Original SmartCardData { get; set; }
    }

    public class DutyOfficerCardInfo
    {
        public string FrontCardImagePath { get; set; }
        public string BackCardImagePath { get; set; }
        public DutyOfficerData DutyOfficerData { get; set; }
    }

    public class SmartCardData_Original
    {
        public DutyOfficerData DutyOfficerData { get; set; }
        public SuperviseeBiodata SuperviseeBiodata { get; set; }
        public List<HistoricalRecord> HistoricalRecords { get; set; }
    }

    public class DutyOfficerData
    {
        public string Name { get; set; }
        public string NRIC { get; set; }
    }

    public class SuperviseeBiodata
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string NRIC { get; set; }
        public DateTime SupervisionFrom { get; set; }
        public DateTime SupervisionTo { get; set; }
        public string DrugProfile { get; set; }
        public string SupervisionOfficer { get; set; }
        public string SupervisionContactNo { get; set; }
        public string DOB { get; set; }
    }

    public class HistoricalRecord
    {
        public DateTime ReportingDate { get; set; }
        public string IUTResult { get; set; }
        public string HSAResult { get; set; }
        public string CNB { get; set; }
    }
}
