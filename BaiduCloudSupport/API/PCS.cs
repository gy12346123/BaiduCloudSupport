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

        /// <summary>
        /// Get baidu cloud quota with default access_token in App.config
        /// </summary>
        /// <returns>[0]:quota, [1]:used</returns>
        public static ulong[] Quota()
        {
            return Quota(Setting.Baidu_Access_Token);
        }

        public static FileMetaStruct SingleFileMeta(string access_token, FilePath filePath)
        {
            //string convertedPath = path.Replace("/", "%2F");
            //if (convertedPath[0] != '%')
            //{
            //    convertedPath = string.Format("%2F{0}", convertedPath);
            //}
            string jsonPost = JsonConvert.SerializeObject(filePath);
            jsonPost = jsonPost.Replace(@"/", @"\/");
            HttpHelper http = new HttpHelper();
            HttpItem item = new HttpItem() {
                URL = BaseURL + "file?method=meta&access_token=" + access_token,
                Method = "POST",
                PostEncoding = Encoding.Default,
                Postdata = jsonPost,
                Encoding = Encoding.UTF8,
                Timeout = PCS.Timeout
            };
            string result = http.GetHtml(item).Html;
            if (result.Contains("list"))
            {
                var json = JsonConvert.DeserializeObject<FileMeta>(result);
                return new FileMetaStruct() {
                    fs_id = json.fs_id,
                    path = json.path,
                    ctime = json.ctime,
                    mtime = json.mtime,
                    block_list = json.block_list,
                    size = json.size,
                    isdir = json.isdir,
                    ifhassubdir = json.ifhassubdir
                };
            }else
            {
                return new FileMetaStruct();
            }
        }
    }
}
