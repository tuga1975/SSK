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
}
