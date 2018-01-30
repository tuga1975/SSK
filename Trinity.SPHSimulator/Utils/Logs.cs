using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace uhpSim.Utils
{
    public class Logs
    {
        public static void SaveLog(string file,string[] lines)
        {
            string directoryName = new FileInfo(file).DirectoryName;
            if (!Directory.Exists(directoryName))
            {
                DirectoryInfo di = Directory.CreateDirectory(directoryName);
            }
            System.IO.File.AppendAllLines(file, lines);
        }
    }
}