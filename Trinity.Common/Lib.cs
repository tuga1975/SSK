using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

public static class Lib
{
    public static ISignalR SignalR { get; set; }
    public static System.Windows.Forms.WebBrowser LayerWeb { get; set; }

    public static int DayOfWeek(this DateTime date)
    {
        return (int)date.DayOfWeek == 0 ? 8 : ((int)date.DayOfWeek) + 1;
    }

    public static int WeekNum(this DateTime date)
    {
        DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
        Calendar cal = dfi.Calendar;
        int weekNum = cal.GetWeekOfYear(date, CalendarWeekRule.FirstDay, System.DayOfWeek.Monday);
        return weekNum;
    }
    public static string GetLast(this string source, int tail_length)
    {
        if (tail_length >= source.Length)
            return source;
        return source.Substring(source.Length - tail_length);
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
    public static CustomAttribute GetMyCustomAttributes(this MemberInfo type)
    {
        object[] attributes = type.GetCustomAttributes(true);
        if (attributes.Length != 0)
        {
            foreach (object attribute in attributes)
            {
                CustomAttribute cusAttr = attribute as CustomAttribute;
                if (cusAttr != null)
                    return cusAttr;
            }
        }
        return null;
    }

    public static TConvert Map<TConvert>(this object entity) where TConvert :new()
    {
        if (entity != null)
        {
            var convert = new TConvert();
            var sourceProps = entity.GetType().GetProperties().Where(x => x.CanRead).ToList();
            var destProps = typeof(TConvert).GetProperties()
                    .Where(x => x.CanWrite)
                    .ToList();
            foreach (var sourceProp in sourceProps)
            {
                if (destProps.Any(x => x.Name == sourceProp.Name))
                {
                    var p = destProps.First(x => x.Name == sourceProp.Name);
                    if (p.CanWrite)
                    {
                        p.SetValue(convert, sourceProp.GetValue(entity, null), null);
                    }
                }
            }
            return convert;
        }
        else
        {
            return default(TConvert);   
        }
    }
    public static string JsonString(this object data)
    {
        return JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
    }
}
