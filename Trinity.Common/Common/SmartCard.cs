using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Common.Common
{
    public class SmartCardData
    {
        string _cardUID;
        SmartCardData_Original _cardData_Original;

        public string CardUID
        {
            get { return _cardUID; }
        }

        public string UserRole
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

        public DutyOfficerData DutyOfficerData
        {
            get { return _cardData_Original?.DutyOfficerData; }
        }

        public SuperviseeBiodata SuperviseeBiodata
        {
            get { return _cardData_Original?.SuperviseeBiodata; }
        }

        public List<HistoricalRecord> HistoricalRecords
        {
            get { return _cardData_Original?.HistoricalRecords; }
        }

        #region Singleton Implementation
        // The variable is declared to be volatile to ensure that assignment to the instance variable completes before the instance variable can be accessed
        private static volatile SmartCardData _instance;

        // Uses a syncRoot instance to lock on, rather than locking on the type itself, to avoid deadlocks.
        private static object syncRoot = new Object();

        private SmartCardData()
        {
            _cardUID = string.Empty;
            _cardData_Original = null;
        }

        public static SmartCardData Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                            _instance = new SmartCardData();
                    }
                }

                return _instance;
            }
        }
        #endregion

        public bool ReadData_FromSmartCard()
        {
            try
            {
                SmartCardReaderUtils smartCardReaderUtils = SmartCardReaderUtils.Instance;

                // get card UID
                string cardUID = smartCardReaderUtils.GetCardUID();
                if (string.IsNullOrEmpty(cardUID))
                {
                    throw new Exception("Can not get card UID");
                }

                // get card data
                SmartCardData_Original smartCardData_Original = null;
                bool readDataResult = smartCardReaderUtils.ReadAllData_MifareClassic(ref smartCardData_Original);
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

        public SuperviseeBiodata GetSuperviseeBiodata()
        {
            return _cardData_Original?.SuperviseeBiodata;
        }

        public List<HistoricalRecord> GetHistoricalRecords()
        {
            return _cardData_Original?.HistoricalRecords;
        }

        public List<HistoricalRecord> GetHistoricalRecords(DateTime reportingDate)
        {
            if (_cardData_Original == null || _cardData_Original.HistoricalRecords == null)
            {
                return null;
            }

            return _cardData_Original.HistoricalRecords
                .Where(record => DbFunctions.TruncateTime(record.ReportingDate) == DbFunctions.TruncateTime(reportingDate)).ToList();
        }

        public List<HistoricalRecord> GetHistoricalRecords(DateTime fromDate, DateTime toDate)
        {
            if (_cardData_Original == null || _cardData_Original.HistoricalRecords == null)
            {
                return null;
            }

            return _cardData_Original.HistoricalRecords
                .Where(record => record.ReportingDate >= fromDate && record.ReportingDate <= toDate).ToList();
        }

        public bool WriteHistoricalRecord(HistoricalRecord record)
        {
            try
            {
                // write data to smart card
                bool actionResult = SmartCardReaderUtils.Instance.WriteHistoricalRecord(_cardData_Original, record);

                // get new data from smart card
                if (actionResult)
                {
                    actionResult = ReadData_FromSmartCard();
                }

                // return value
                return actionResult;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("WriteHistoricalRecord exception: " + ex.ToString());
                return false;
            }

        }

        public void ResetInstance()
        {
            if (_instance != null)
            {
                _instance = new SmartCardData();
            }
        }
    }

    public class PrintAndWriteSmartCardInfo
    {
        public string FrontCardImagePath { get; set; }
        public string BackCardImagePath { get; set; }
        public SuperviseeBiodata SuperviseeBiodata { get; set; }
    }

    public class PrintAndWriteSmartcardResult
    {
        public bool Success { get; set; }
        public string Description { get; set; }
        public string CardUID { get; set; }
        public SmartCardData_Original SmartCardData { get; set; }
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
    }

    public class HistoricalRecord
    {
        public DateTime ReportingDate { get; set; }
        public string IUTResult { get; set; }
        public string HSAResult { get; set; }
        public string CNB { get; set; }
    }
}
