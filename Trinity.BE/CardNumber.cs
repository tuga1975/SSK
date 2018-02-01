using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.BE
{
    
    public class CardNumber
    {
        [DataMember]
        public int Year { get; set; }

        [DataMember]
        public int MaxNumber { get; set; }

        [DataMember]
        public int Number { get; set; }

        [DataMember]
        public string FullCardNumber
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
    }
    
}
