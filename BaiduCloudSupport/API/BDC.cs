using DotNet4.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaiduCloudSupport.API
{
    public class BDC
    {
        /// <summary>
        /// Baidu cloud api base URL
        /// </summary>
        private static string BDCBaseURL = "http://pan.baidu.com/api/";

        /// <summary>
        /// Http time out
        /// </summary>
        private static int Timeout = 30000;

        /// <summary>
        /// Set http Get or Post timeout value
        /// </summary>
        /// <param name="time">Millisecond</param>
        public static void SetTimeout(int time)
        {
            Timeout = time;
        }

        public enum Order { name, time, size };

        private static string Cookies;

        /// <summary>
        /// Convert local path format to url format
        /// </summary>
        /// <param name="path">Local file path</param>
        /// <returns>URL format</returns>
        private static string ConvertPath2URLFormat(string path)
        {
            string convertedPath;
            if (path.StartsWith("/"))
            {
                convertedPath = path.Replace("/", "%2F");
            }
            else
            {
                convertedPath = ("%2F" + path).Replace("/", "%2F");
            }
            if (convertedPath.Contains("#"))
            {
                convertedPath = convertedPath.Replace("#", "%23");
            }
            if (convertedPath.Contains("+"))
            {
                convertedPath = convertedPath.Replace("+", "%2B");
            }
            return convertedPath;
        }

        public static void LoadCookie(string path, bool overwrite = false)
        {
            if (Cookies == null || Cookies.Equals("") || overwrite)
            {
                using (StreamReader SR = new StreamReader(new FileStream(path, FileMode.Open)))
                {
                    StringBuilder SB = new StringBuilder();
                    while (!SR.EndOfStream)
                    {
                        string[] param = SR.ReadLine().Split('$');
                        SB.Append(string.Format("{0}={1}; ", param[1], param[2]));
                    }
                    Cookies = SB.ToString();
                }
            }
        }

        public static bool CheckCookie()
        {
            if (Cookies == null || Cookies.Equals(""))
            {
                return false;
            }
            return true;
        }

        public static bool IsCookieFileExist(string path)
        {
            if (File.Exists(path))
            {
                return true;
            }
            return false;
        }

        public static FileListStruct[] SingleFloder(string path, int page, int maxNum = 100, int desc = 0, Order order = Order.name)
        {
            if (!CheckCookie()) LoadCookie(Setting.Baidu_CookiePath);
            string ConvertedPath = ConvertPath2URLFormat(path);
            HttpHelper http = new HttpHelper();
            HttpItem item = new HttpItem()
            {
                URL = string.Format("{0}list?dir={1}&page={2}&num={3}&desc={4}&order={5}&clienttype=0&showempty=0&web=1&channel=chunlei&app_id=250528", BDCBaseURL, ConvertedPath, page, maxNum, desc, order.ToString()),
                Encoding = Encoding.UTF8,
                Timeout = BDC.Timeout,
                Referer = "http://pan.baidu.com/disk/home#list/vmode=list&path=" + ConvertedPath,
                Host = "pan.baidu.com",
                Cookie = Cookies
            };
            string result = http.GetHtml(item).Html;

            if (result.Contains("list"))
            {
                var json = (JObject)JsonConvert.DeserializeObject(result);
                int listCount = json["list"].Count();
                FileListStruct[] fileListStruct = new FileListStruct[listCount];
                for (int i = 0; i < listCount; i++)
                {
                    fileListStruct[i].fs_id = Convert.ToUInt64(json["list"][i]["fs_id"]);
                    fileListStruct[i].path = json["list"][i]["path"].ToString();
                    fileListStruct[i].ctime = Convert.ToUInt32(json["list"][i]["server_ctime"]);
                    fileListStruct[i].mtime = Convert.ToUInt32(json["list"][i]["server_mtime"]);
                    fileListStruct[i].size = Convert.ToUInt64(json["list"][i]["size"]);
                    fileListStruct[i].isdir = Convert.ToUInt32(json["list"][i]["isdir"]);
                    if (json["list"][i]["md5"] != null)
                    {
                        fileListStruct[i].md5 = json["list"][i]["md5"].ToString();
                    }
                }
                return fileListStruct;
            }
            else
            {
                throw new ErrorCodeException();
            }
        }

        public static Task<FileListStruct[]> SearchFile(string keyword, int page = 1, int maxNum = 100, int desc = 0, Order order = Order.name)
        {
            return Task.Factory.StartNew(()=> {
                if (!CheckCookie()) LoadCookie(Setting.Baidu_CookiePath);
                HttpHelper http = new HttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = string.Format("{0}search?recursion=1&key={1}&page={2}&num={3}&desc={4}&order={5}&clienttype=0&showempty=0&web=1&channel=chunlei&app_id=250528", BDCBaseURL, keyword, page, maxNum, desc, order.ToString()),
                    Encoding = Encoding.UTF8,
                    Timeout = BDC.Timeout,
                    Referer = "http://pan.baidu.com/disk/home#search/key=" + keyword,
                    Host = "pan.baidu.com",
                    Cookie = Cookies
                };
                string result = http.GetHtml(item).Html;

                if (result.Contains("list"))
                {
                    var json = (JObject)JsonConvert.DeserializeObject(result);
                    int listCount = json["list"].Count();
                    FileListStruct[] fileListStruct = new FileListStruct[listCount];
                    for (int i = 0; i < listCount; i++)
                    {
                        fileListStruct[i].fs_id = Convert.ToUInt64(json["list"][i]["fs_id"]);
                        fileListStruct[i].path = json["list"][i]["path"].ToString();
                        fileListStruct[i].ctime = Convert.ToUInt32(json["list"][i]["server_ctime"]);
                        fileListStruct[i].mtime = Convert.ToUInt32(json["list"][i]["server_mtime"]);
                        fileListStruct[i].size = Convert.ToUInt64(json["list"][i]["size"]);
                        fileListStruct[i].isdir = Convert.ToUInt32(json["list"][i]["isdir"]);
                        if (json["list"][i]["md5"] != null)
                        {
                            fileListStruct[i].md5 = json["list"][i]["md5"].ToString();
                        }
                    }
                    return fileListStruct;
                }
                else
                {
                    throw new ErrorCodeException();
                }
            });
        }
    }
}
