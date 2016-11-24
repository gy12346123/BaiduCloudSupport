using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaiduCloudSupport.Other
{
    public static class Tools
    {
        /// <summary>
        /// Return TimeStamp as string
        /// </summary>
        /// <returns>TimeStamp</returns>
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }

        /// <summary>
        /// Return TimeStamp as long use DateTime
        /// </summary>
        /// <param name="dateTime">DataTime</param>
        /// <returns>TimeStamp</returns>
        public static long GetTimeStamp(DateTime dateTime)
        {
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, dateTime.Kind);
            return Convert.ToInt64((dateTime - start).TotalSeconds);
        }

        /// <summary>
        /// Convert TimeStamp ot DateTime
        /// </summary>
        /// <param name="target">DateTime</param>
        /// <param name="timestamp">TimeStamp</param>
        /// <returns>DateTime</returns>
        public static DateTime TimeStamp2DateTime(this DateTime target, long timestamp)
        {
            var start = new DateTime(1970, 1, 1, 0, 0, 0, target.Kind);
            return start.AddSeconds(timestamp);
        }

        public static DateTime TimeStamp2DateTime(string timeStamp)
        {
            DateTime dateTimeStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long lTime = long.Parse(timeStamp + "0000000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dateTimeStart.Add(toNow);
        }
    }
}
