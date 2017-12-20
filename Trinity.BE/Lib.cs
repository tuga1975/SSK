using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Lib
{
    public static int DayOfWeek(this DateTime date)
    {
        return (int)date.DayOfWeek==0?8: ((int)date.DayOfWeek) +1;
    }
    public static string EncoderQueueNumber(this string queuenumber)
    {
        string NICR = queuenumber;
        if (NICR.Length < 3)
            return string.Empty;
        string FirstChar = NICR.Substring(0, 1);
        string LastChar = NICR.Substring(1, NICR.Length - 1);
        string CharRemove = LastChar.Remove(0, LastChar.Length - (NICR.Length<4? NICR.Length:4));
        return  NICR.Substring(0, 1) + string.Empty.PadLeft(LastChar.Length - CharRemove.Length, '*') + CharRemove;
    }
}
