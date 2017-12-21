using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Lib
{
    public static int DayOfWeek(this DateTime date)
    {
        return (int)date.DayOfWeek == 0 ? 8 : ((int)date.DayOfWeek) + 1;
    }
    public static string GetQueueNumber(this string nric)
    {
        if (String.IsNullOrEmpty(nric))
        {
            return string.Empty;
        }
        if (nric.Length < 3)
        {
            return string.Empty;
        }
        string FirstChar = nric.Substring(0, 1);
        string LastChar = nric.Substring(1, nric.Length - 1);
        string CharRemove = LastChar.Remove(0, LastChar.Length - (nric.Length < 4 ? nric.Length : 4));
        return nric.Substring(0, 1) + string.Empty.PadLeft(LastChar.Length - CharRemove.Length, '*') + CharRemove;
    }
}
