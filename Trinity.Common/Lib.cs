using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

public static class Lib
{
    //public static ISignalR SignalR { get; set; }
    public static System.Windows.Forms.WebBrowser LayerWeb { get; set; }

    public static System.Collections.Generic.Dictionary<string, Nullable<bool>> ArrayIDWaitMessage = new System.Collections.Generic.Dictionary<string, Nullable<bool>>();
    
    public static int DayOfWeek(this DateTime date)
    {
        return (int)date.DayOfWeek == 0 ? 8 : ((int)date.DayOfWeek) + 1;
    }


    public static string Station
    {
        get
        {
            string station = System.Configuration.ConfigurationManager.AppSettings["Station"];
            if (string.IsNullOrEmpty(station))
                station = System.Reflection.Assembly.GetEntryAssembly().GetName().Name;
            System.Reflection.MemberInfo member = typeof(EnumStation).GetMembers().Where(d => d.Name.ToLower() == station.ToLower()).FirstOrDefault();
            if (member != null)
            {
                station = ((System.Reflection.FieldInfo)member).GetValue(member).ToString();
            }
            return station;
        }
    }
    public static byte[] ImageToByte(this Image img)
    {
        using (var stream = new MemoryStream())
        {
            img.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
            return stream.ToArray();
        }
    }
    public static Image ResizeImage(this Image imgToResize, int width, int height)
    {
        var originalWidth = imgToResize.Width;
        var originalHeight = imgToResize.Height;

        //how many units are there to make the original length
        var hRatio = (float)originalHeight / height;
        var wRatio = (float)originalWidth / width;

        //get the shorter side
        var ratio = Math.Min(hRatio, wRatio);

        var hScale = Convert.ToInt32(height * ratio);
        var wScale = Convert.ToInt32(width * ratio);

        //start cropping from the center
        var startX = (originalWidth - wScale) / 2;
        var startY = (originalHeight - hScale) / 2;

        //crop the image from the specified location and size
        var sourceRectangle = new Rectangle(startX, startY, wScale, hScale);

        //the future size of the image
        var bitmap = new Bitmap(width, height);

        //fill-in the whole bitmap
        var destinationRectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);

        //generate the new image
        using (var g = Graphics.FromImage(bitmap))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(imgToResize, destinationRectangle, sourceRectangle, GraphicsUnit.Pixel);
        }

        return bitmap;

    }
    //public static Image ScaleImage(this Image image, int maxWidth, int maxHeight)
    //{
    //    var ratioX = (double)maxWidth / image.Width;
    //    var ratioY = (double)maxHeight / image.Height;
    //    var ratio = Math.Min(ratioX, ratioY);

    //    var newWidth = (int)(image.Width * ratio);
    //    var newHeight = (int)(image.Height * ratio);

    //    var newImage = new Bitmap(maxWidth, maxWidth);
    //    using (var graphics = Graphics.FromImage(newImage))
    //    {
    //        // Calculate x and y which center the image
    //        int y = (maxHeight / 2) - newHeight / 2;
    //        int x = (maxWidth / 2) - newWidth / 2;

    //        // Draw image on x and y with newWidth and newHeight
    //        graphics.DrawImage(image, x, y, newWidth, newHeight);
    //    }

    //    return newImage;
    //}
    //public static Bitmap ResizeImage(this Image image, int width, int height)
    //{
    //    var destRect = new Rectangle(0, 0, width, height);
    //    var destImage = new Bitmap(width, height);

    //    destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

    //    using (var graphics = Graphics.FromImage(destImage))
    //    {
    //        graphics.CompositingMode = CompositingMode.SourceCopy;
    //        graphics.CompositingQuality = CompositingQuality.HighSpeed;
    //        graphics.InterpolationMode = InterpolationMode.Low;
    //        graphics.SmoothingMode = SmoothingMode.HighSpeed;
    //        graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;
    //        using (var wrapMode = new ImageAttributes())
    //        {
    //            wrapMode.SetWrapMode(WrapMode.TileFlipXY);
    //            graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
    //        }
    //    }

    //    return destImage;
    //}

    public static byte[] ReadAllBytes(string fileName)
    {
        byte[] buffer = null;
        using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        {
            buffer = new byte[fs.Length];
            fs.Read(buffer, 0, (int)fs.Length);
        }
        return buffer;
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

    public static TConvert Map<TConvert>(this object entity) where TConvert : new()
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
