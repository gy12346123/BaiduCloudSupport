using DotNet4.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaiduCloudSupport.API
{
    public class PCS
    {
        private static string BaseURL = "https://pcs.baidu.com/rest/2.0/pcs/";

        private static int Timeout = 30000;

        /// <summary>
        /// Set http Get or Post timeout value
        /// </summary>
        /// <param name="time">Millisecond</param>
        public static void SetTimeout(int time)
        {
            Timeout = time;
        }

        /// <summary>
        /// Get baidu cloud quota
        /// </summary>
        /// <param name="access_token">Baidu access token</param>
        /// <returns>[0]:quota, [1]:used</returns>
        public static ulong[] Quota(string access_token)
        {
            try
            {
                HttpHelper http = new HttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = BaseURL + "quota?method=info&access_token=" + access_token,
                    Encoding = Encoding.UTF8,
                    Timeout = PCS.Timeout
                };
                string result = http.GetHtml(item).Html;
                if (result.Contains("quota"))
                {
                    var json = JsonConvert.DeserializeObject<QuotaInfo>(result);
                    return new ulong[2] { json.quota / Convert.ToUInt64(Math.Pow(1024d, 3d)), json.used / Convert.ToUInt64(Math.Pow(1024d, 3d)) };
                }
                else
                {
                    return null;
                }
            }catch(Exception ex)
            {
                LogHelper.WriteLog("PCS.Quota", ex);
                return null;
            }
        }
    }
}
