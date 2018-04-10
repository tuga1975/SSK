using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.Common.Utils
{

    public class LogManager
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Debug(string debugInfo)
        {
            _log.Debug(debugInfo);
        }

        public static void Info(string info)
        {
            _log.Info(info);

        }

        public static void Warn(string warning)
        {
            _log.Warn(warning);

        }

        public static void Error(string error)
        {
            _log.Error(error);

        }

        public static void Fatal(string fatal)
        {
            _log.Fatal(fatal);

        }
    }
}
