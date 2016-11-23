using DotNet4.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BaiduCloudSupport.API
{
    public class PCS
    {
        /// <summary>
        /// Baidu PCS api base URL
        /// </summary>
        private static string PCSBaseURL = "https://pcs.baidu.com/rest/2.0/pcs/";

        /// <summary>
        /// Baidu PCS download file api base URL
        /// </summary>
        private static string DownloadBaseURL = "https://d.pcs.baidu.com/rest/2.0/pcs/";

        /// <summary>
        /// Baidu OpenAPI base URL
        /// </summary>
        private static string OpenAPIBaseURL = "https://openapi.baidu.com/rest/2.0/";

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
        /// Convert local path format to url format
        /// </summary>
        /// <param name="path">Local file path</param>
        /// <returns>URL format</returns>
        private static string ConvertPath2URLFormat(string path)
        {
            string convertedPath = (PCS.BasePath + path).Replace("/", "%2F");
            if (convertedPath[0] != '%')
            {
                convertedPath = string.Format("%2F{0}", convertedPath);
            }
            return convertedPath;
        }

        /// <summary>
        /// Get user simple info
        /// </summary>
        /// <param name="access_token">Baidu access token</param>
        /// <returns>SimpleUserInfoStruct</returns>
        public static SimpleUserInfoStruct SimpleUser(string access_token)
        {
            try
            {
                HttpHelper http = new HttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = OpenAPIBaseURL + "passport/users/getLoggedInUser?access_token=" + access_token,
                    Encoding = Encoding.UTF8,
                    Timeout = PCS.Timeout
                };
                string result = http.GetHtml(item).Html;
                if (result.Contains("uid"))
                {
                    var json = JsonConvert.DeserializeObject<SimpleUserInfo>(result);
                    return new SimpleUserInfoStruct
                    {
                        uid = json.uid,
                        uname = json.uname,
                        portrait = json.portrait
                    };
                }
                else
                {
                    return new SimpleUserInfoStruct { uid = 0,uname = "", portrait = "" };
                }
            }catch(Exception ex)
            {
                LogHelper.WriteLog("PCS.SimpleUser", ex);
                return new SimpleUserInfoStruct { uid = 0, uname = "", portrait = "" };
            }
        }

        /// <summary>
        /// Get user simple info with default access_token in App.config
        /// </summary>
        /// <returns>SimpleUserInfoStruct</returns>
        public static SimpleUserInfoStruct SimpleUser()
        {
            return SimpleUser(Setting.Baidu_Access_Token);
        }

        /// <summary>
        /// Get small portrait url
        /// </summary>
        /// <param name="portrait">SimpleUserInfoStruct.portrait</param>
        /// <returns>URL</returns>
        public static string UserSmallPortrait(string portrait)
        {
            return string.Format("http://tb.himg.baidu.com/sys/portraitn/item/{0}", portrait);
        }

        /// <summary>
        /// Get large portrait url
        /// </summary>
        /// <param name="portrait">SimpleUserInfoStruct.portrait</param>
        /// <returns>URL</returns>
        public static string UserLargePortrait(string portrait)
        {
            return string.Format("http://tb.himg.baidu.com/sys/portrait/item/{0}", portrait);
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
                    URL = PCSBaseURL + "quota?method=info&access_token=" + access_token,
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
                HttpHelper http = new HttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = PCSBaseURL + "file?method=meta&access_token=" + access_token + "&path=" + ConvertPath2URLFormat(path),
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
                HttpHelper http = new HttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = PCSBaseURL + "file?method=list&access_token=" + access_token + "&path=" + ConvertPath2URLFormat(path),
                    Encoding = Encoding.UTF8,
                    Timeout = PCS.Timeout
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

        /// <summary>
        /// Search file in path with keyword
        /// </summary>
        /// <param name="access_token">Baidu access token</param>
        /// <param name="path">Floder path used to search file</param>
        /// <param name="keyword">Keyword used to search file</param>
        /// <param name="traversing">Traverse the folder:0.No, 1.Yes</param>
        /// <returns>FileListStruct[]</returns>
        public static FileListStruct[] SearchFile(string access_token, string path, string keyword, int traversing = 1)
        {
            try
            {
                HttpHelper http = new HttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = PCSBaseURL + "file?method=search&access_token=" + access_token + "&path=" + ConvertPath2URLFormat(path) + "&wd=" + keyword + "&re=" + traversing,
                    Encoding = Encoding.UTF8,
                    Timeout = PCS.Timeout
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
                LogHelper.WriteLog("PCS.SearchFile", ex);
                return new FileListStruct[1];
            }
        }

        /// <summary>
        /// Search file in path with keyword use default access_token in App.config
        /// </summary>
        /// <param name="path">Floder path used to search file</param>
        /// <param name="keyword">Keyword used to search file</param>
        /// <param name="traversing">Traverse the folder:0.No, 1.Yes</param>
        /// <returns>FileListStruct[]</returns>
        public static FileListStruct[] SearchFile(string path, string keyword, int traversing = 1)
        {
            return SearchFile(Setting.Baidu_Access_Token, path, keyword, traversing);
        }

        /// <summary>
        /// Download file from remoteFile and save it to localFile
        /// </summary>
        /// <param name="access_token">Baidu access token</param>
        /// <param name="remoteFile">Remote full file path</param>
        /// <param name="localFile">Local full file path</param>
        /// <returns>Task</returns>
        public static Task DownloadFile(string access_token, string remoteFile, string localFile)
        {
            return Task.Factory.StartNew(()=> {
                try
                {
                    HttpHelper http = new HttpHelper();
                    HttpItem item = new HttpItem()
                    {
                        URL = DownloadBaseURL + "file?method=download&access_token=" + access_token + "&path=" + ConvertPath2URLFormat(remoteFile),
                        Encoding = Encoding.UTF8,
                        Timeout = PCS.Timeout
                    };
                    var result = http.GetHtml(item);
                    string[] location = result.Header.GetValues("location");

                    WebClient web = new WebClient();
                    web.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) => {
                        MainWindow.totalData.SingleFileSize = e.TotalBytesToReceive;
                        MainWindow.totalData.SingleFileBytesReceived = e.BytesReceived;
                        MainWindow.totalData.SingleFileProgressPercentage = e.ProgressPercentage;
                    };
                    web.DownloadFileCompleted += (object sender, System.ComponentModel.AsyncCompletedEventArgs e) => {
                        MainWindow.totalData.SingleFileProgressPercentage = 100;
                    };
                    web.DownloadFileAsync(new Uri(location[0].Replace("\"", "")), localFile);
                }catch(Exception ex)
                {
                    LogHelper.WriteLog("PCS.DownloadFile", ex);
                }
            });
        }
    }
}
