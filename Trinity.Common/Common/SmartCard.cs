using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Common.Common
{
    public class PrintAndWriteSmartcardInfo
    {
        public Bitmap FrontCard { get; set; }
        public Bitmap BackCard { get; set; }
        public SmartCardData SmartCardData { get; set; }
    }

    public class PrintAndWriteSmartcardResult
    {
        public bool Success { get; set; }
        public string Description { get; set; }
        public SmartCardData SmartCardData { get; set; }
    }

    public class SmartCardData
    {
        public CardInfo CardInfo { get; set; }
        public CardHolderInfo CardHolderInfo { get; set; }
        public List<CardHolderActivity> CardHolderActivities { get; set; }
    }

    public class CardInfo
    {
        /// <summary>
        /// set UID = null when send it to print
        /// </summary>
        public string UID { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class CardHolderInfo
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string NRIC { get; set; }
        public DateTime DOB { get; set; }
    }

    public class CardHolderActivity
    {
        public DateTime Date { get; set; }
        public string Station { get; set; }
        public string Description { get; set; }
    }
}
