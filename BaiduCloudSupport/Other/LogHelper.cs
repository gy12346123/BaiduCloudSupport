using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaiduCloudSupport
{
    /// <summary>
    /// Log helper
    /// </summary>
    class LogHelper
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Write log with information if IsInfoEnabled
        /// </summary>
        /// <param name="info">string information</param>
        public static void WriteLog(string info)
        {
            if (log.IsInfoEnabled)
            {
                log.Info(info);
            }
        }

        /// <summary>
        /// Write log with information and exception if IsErrorEnabled
        /// </summary>
        /// <param name="info">string information</param>
        /// <param name="ex">Exception</param>
        public static void WriteLog(string info,Exception ex)
        {
            if (log.IsErrorEnabled)
            {
                log.Error(info,ex);
            }
        }
    }
}
