using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{
    
    public class CardInfo
    {
        [DataMember]
        public int Year { get; set; }

        [DataMember]
        public int MaxNumber { get; set; }

        [DataMember]
        public int Number { get; set; }

        [DataMember]
        public string CardNumberFull
        {
            get
            {
                string lastNumber = Number.ToString();
                string firstNumber = string.Empty;
                for (int i = 0; i < (MaxNumber.ToString().Length - lastNumber.Length); i++)
                {
                    firstNumber += "0";
                }

                return string.Format("{0}/{1}{2}",Year,firstNumber,lastNumber);
            }
        }

        public string CompanyName { get; set; }
        public string VenueName { get; set; }
        public string Address { get; set; }
        public string ContactNumber { get; set; }
        
        public DateTime Date_Of_Issue
        {
            get
            {
                return DateTime.Today;
            }
        }
        public string Date_Of_Issue_Txt
        {
            get
            {
                return Date_Of_Issue.ToString(EnumAppConfig.DateFormat);
            }
        }

        public DateTime Expired_Date
        {
            get
            {
                return Date_Of_Issue.AddYears(EnumAppConfig.Card_Expired_Date);
            }
        }
        public string Expired_Date_Txt
        {
            get
            {
                return Expired_Date.ToString(EnumAppConfig.DateFormat);
            }
        }
    }
    
}
