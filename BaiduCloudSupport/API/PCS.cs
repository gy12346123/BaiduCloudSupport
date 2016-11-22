using DotNet4.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaiduCloudSupport.API
{
    public class PCS
    {
        /// <summary>
        /// Baidu PCS api base URL
        /// </summary>
        private static string BaseURL = "https://pcs.baidu.com/rest/2.0/pcs/";

        /// <summary>
        /// Project base path
        /// </summary>
        private static string BasePath = "/apps/wp2pcs";

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

        /// <summary>
        /// Get single file or path meta
        /// </summary>
        /// <param name="access_token">Baidu access token</param>
        /// <param name="path">File or floder path</param>
        /// <returns>FileMetaStruct</returns>
        public static FileMetaStruct SingleFileMeta(string access_token, string path)
        {
            try
            {
                string convertedPath = (PCS.BasePath + path).Replace("/", "%2F");
                if (convertedPath[0] != '%')
                {
                    convertedPath = string.Format("%2F{0}", convertedPath);
                }
                HttpHelper http = new HttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = BaseURL + "file?method=meta&access_token=" + access_token + "&path=" + convertedPath,
                    Encoding = Encoding.UTF8,
                    Timeout = PCS.Timeout
                };
                string result = http.GetHtml(item).Html;
                if (result.Contains("list"))
                {
                    var json = (JObject)JsonConvert.DeserializeObject(result);
                    FileMetaStruct fileMetaStruct = new FileMetaStruct();
                    fileMetaStruct.fs_id = Convert.ToUInt64(json["list"][0]["fs_id"]);
                    fileMetaStruct.path = json["list"][0]["path"].ToString();
                    fileMetaStruct.ctime = Convert.ToUInt32(json["list"][0]["ctime"]);
                    fileMetaStruct.mtime = Convert.ToUInt32(json["list"][0]["mtime"]);
                    fileMetaStruct.block_list = json["list"][0]["block_list"].ToString();
                    fileMetaStruct.size = Convert.ToUInt64(json["list"][0]["size"]);
                    fileMetaStruct.isdir = Convert.ToUInt32(json["list"][0]["isdir"]);
                    fileMetaStruct.ifhassubdir = Convert.ToUInt32(json["list"][0]["ifhassubdir"]);
                    fileMetaStruct.filenum = Convert.ToUInt32(json["list"][0]["filenum"]);

                    return fileMetaStruct;
                }
                else
                {
                    return new FileMetaStruct();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("PCS.SingleFileMeta", ex);
                return new FileMetaStruct();
            }
        }

        /// <summary>
        /// Get single file or path meta with default access_token in App.config
        /// </summary>
        /// <param name="path">File or floder path</param>
        /// <returns>FileMetaStruct</returns>
        public static FileMetaStruct SingleFileMeta(string path)
        {
            return SingleFileMeta(Setting.Baidu_Access_Token, path);
        }

        /// <summary>
        /// Get File list
        /// </summary>
        /// <param name="access_token">Baidu access token</param>
        /// <param name="path">Floder path</param>
        /// <returns>FileListStruct[]</returns>
        public static FileListStruct[] SingleFloder(string access_token, string path)
        {
            try
            {
                string convertedPath = (PCS.BasePath + path).Replace("/", "%2F");
                if (convertedPath[0] != '%')
                {
                    convertedPath = string.Format("%2F{0}", convertedPath);
                }
                HttpHelper http = new HttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = BaseURL + "file?method=list&access_token=" + access_token + "&path=" + convertedPath,
                    Encoding = Encoding.UTF8,
                    Timeout = PCS.Timeout
                };
                string result = http.GetHtml(item).Html;
                if (result.Contains("list"))
                {
                    var json = (JObject)JsonConvert.DeserializeObject(result);
                    string str = json["list"][0]["path"].ToString();
                    int listCount = json["list"].Count();
                    FileListStruct[] fileListStruct = new FileListStruct[listCount];
                    for (int i = 0; i < listCount; i++)
                    {
                        fileListStruct[i].fs_id = Convert.ToUInt64(json["list"][i]["fs_id"]);
                        fileListStruct[i].path = json["list"][i]["path"].ToString();
                        fileListStruct[i].ctime = Convert.ToUInt32(json["list"][i]["ctime"]);
                        fileListStruct[i].mtime = Convert.ToUInt32(json["list"][i]["mtime"]);
                        fileListStruct[i].md5 = json["list"][i]["md5"].ToString();
                        fileListStruct[i].size = Convert.ToUInt64(json["list"][i]["size"]);
                        fileListStruct[i].isdir = Convert.ToUInt32(json["list"][i]["isdir"]);
                    }
                    return fileListStruct;
                }
                else
                {
                    return new FileListStruct[1];
                }
            }catch(Exception ex)
            {
                LogHelper.WriteLog("PCS.SingleFloder", ex);
                return new FileListStruct[1];
            }
        }

        /// <summary>
        /// Get File list with default access_token in App.config
        /// </summary>
        /// <param name="path">Floder path</param>
        /// <returns>FileListStruct[]</returns>
        public static FileListStruct[] SingleFloder(string path)
        {
            return SingleFloder(Setting.Baidu_Access_Token, path);
        }
    }
}
