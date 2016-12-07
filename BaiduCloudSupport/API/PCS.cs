using DotNet4.Utilities;
using MyDownloader.Core;
using MyDownloader.Extension.Protocols;
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

        private static string DownloadOldURL = "https://pcs.baidu.com/rest/2.0/pcs/";

        /// <summary>
        /// Baidu OpenAPI base URL
        /// </summary>
        private static string OpenAPIBaseURL = "https://openapi.baidu.com/rest/2.0/";

        /// <summary>
        /// Project base path
        /// </summary>
        public static string BasePath = "/apps/wp2pcs";

        /// <summary>
        /// Http time out
        /// </summary>
        private static int Timeout = 30000;

        private static bool ForFirst = true;

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
            string convertedPath;
            if (path.Contains("%"))
            {
                path = path.Replace("%", "%25");
            }
            if (path.StartsWith(PCS.BasePath))
            {
                convertedPath = path.Replace("/", "%2F");
            }
            else
            {
                convertedPath = (PCS.BasePath + path).Replace("/", "%2F");
            }
            if (convertedPath.Contains("#"))
            {
                convertedPath = convertedPath.Replace("#", "%23");
            }
            if (convertedPath.Contains("+"))
            {
                convertedPath = convertedPath.Replace("+", "%2B");
            }
            if (convertedPath.Contains("&"))
            {
                convertedPath = convertedPath.Replace("&", "%26");
            }
            if (convertedPath.Contains(" "))
            {
                convertedPath = convertedPath.Replace(" ", "%20");
            }
            if (convertedPath.Contains("?"))
            {
                convertedPath = convertedPath.Replace("?", "%3f");
            }
            return convertedPath;
        }

        private static string CheckPath (string path)
        {
            if (path == null || path.Equals("") || path.Equals("/"))
            {
                return PCS.BasePath;
            }
            return path;
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
                    //return new SimpleUserInfoStruct { uid = 0,uname = "", portrait = "" };
                    throw new ErrorCodeException();
                }
            }catch(Exception ex)
            {
                LogHelper.WriteLog("PCS.SimpleUser", ex);
                //return new SimpleUserInfoStruct { uid = 0, uname = "", portrait = "" };
                throw new Exception("PCS.SimpleUser", ex);
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
        [Obsolete]
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
                    throw new ErrorCodeException();
                }
            }catch(Exception ex)
            {
                LogHelper.WriteLog("PCS.Quota", ex);
                throw new Exception("PCS.Quota", ex);
            }
        }

        /// <summary>
        /// Get baidu cloud quota with default access_token in App.config
        /// </summary>
        /// <returns>[0]:quota, [1]:used</returns>
        [Obsolete]
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
        [Obsolete]
        public static PCSFileMetaStruct SingleFileMeta(string access_token, string path)
        {
            try
            {
                HttpHelper http = new HttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = PCSBaseURL + "file?method=meta&access_token=" + access_token + "&path=" + Other.Tools.URLEncoding(CheckPath(path), Encoding.UTF8),
                    Encoding = Encoding.UTF8,
                    Timeout = PCS.Timeout
                };
                string result = http.GetHtml(item).Html;
                if (result.Contains("list"))
                {
                    var json = (JObject)JsonConvert.DeserializeObject(result);
                    PCSFileMetaStruct fileMetaStruct = new PCSFileMetaStruct();
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
                    throw new ErrorCodeException();
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("PCS.SingleFileMeta", ex);
                throw new Exception("PCS.SingleFileMeta", ex);
            }
        }

        /// <summary>
        /// Get single file or path meta with default access_token in App.config
        /// </summary>
        /// <param name="path">File or floder path</param>
        /// <returns>FileMetaStruct</returns>
        [Obsolete]
        public static Task<PCSFileMetaStruct> SingleFileMeta(string path)
        {
            return Task.Factory.StartNew(()=> {
                return SingleFileMeta(Setting.Baidu_Access_Token, path);
            });
        }

        /// <summary>
        /// Get File list
        /// </summary>
        /// <param name="access_token">Baidu access token</param>
        /// <param name="path">Floder path</param>
        /// <returns>FileListStruct[]</returns>
        [Obsolete]
        public static FileListStruct[] SingleFloder(string access_token, string path)
        {
            HttpHelper http = new HttpHelper();
            HttpItem item = new HttpItem()
            {
                URL = PCSBaseURL + "file?method=list&access_token=" + access_token + "&path=" + Other.Tools.URLEncoding(CheckPath(path), Encoding.UTF8),
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
                throw new ErrorCodeException();
            }
        }

        /// <summary>
        /// Get File list with default access_token in App.config
        /// </summary>
        /// <param name="path">Floder path</param>
        /// <returns>FileListStruct[]</returns>
        [Obsolete]
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
        /// <returns>Task<FileListStruct[]></returns>
        [Obsolete]
        public static Task<FileListStruct[]> SearchFile(string access_token, string path, string keyword, int traversing = 1)
        {
            return Task.Factory.StartNew(() => {
                HttpHelper http = new HttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = PCSBaseURL + "file?method=search&access_token=" + access_token + "&path=" + Other.Tools.URLEncoding(CheckPath(path), Encoding.UTF8) + "&wd=" + Other.Tools.URLEncoding(keyword, Encoding.UTF8) + "&re=" + traversing,
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
                    throw new ErrorCodeException();
                }
            });
        }

        /// <summary>
        /// Search file in path with keyword use default access_token in App.config
        /// </summary>
        /// <param name="path">Floder path used to search file</param>
        /// <param name="keyword">Keyword used to search file</param>
        /// <param name="traversing">Traverse the folder:0.No, 1.Yes</param>
        /// <returns>Task<FileListStruct[]></returns>
        [Obsolete]
        public static Task<FileListStruct[]> SearchFile(string path, string keyword, int traversing = 1)
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
        [Obsolete]
        public static Task DownloadFile(string access_token, ulong fs_id, string remoteFile, string localFile)
        {
            return Task.Factory.StartNew(()=> {
                try
                {
                    HttpHelper http = new HttpHelper();
                    HttpItem item = new HttpItem()
                    {
                        URL = DownloadBaseURL + "file?method=download&access_token=" + access_token + "&path=" + Other.Tools.URLEncoding(remoteFile, Encoding.UTF8),
                        Encoding = Encoding.UTF8,
                        Timeout = PCS.Timeout
                    };
                    var result = http.GetHtml(item);
                    string[] location = result.Header.GetValues("location");
                    using (WebClient web = new WebClient())
                    {
                        web.DownloadProgressChanged += (object sender, DownloadProgressChangedEventArgs e) => {
                            //MainWindow.totalData.SingleFileSize = e.TotalBytesToReceive;
                            //MainWindow.totalData.SingleFileBytesReceived = e.BytesReceived;
                            //MainWindow.totalData.SingleFileProgressPercentage = e.ProgressPercentage;
                            double percent = Math.Round(Convert.ToDouble(e.BytesReceived) / Convert.ToDouble(e.TotalBytesToReceive) * 100, 1);
                            MainWindow.DownloadListChangeItems(fs_id, e.TotalBytesToReceive, e.BytesReceived, percent, 0d);
                        };
                        web.DownloadFileCompleted += (object sender, System.ComponentModel.AsyncCompletedEventArgs e) => {
                            //MainWindow.totalData.SingleFileProgressPercentage = 100;
                        };
                        web.DownloadFileAsync(new Uri(location[0].Replace("\"", "")), localFile);
                    }
                }catch(Exception ex)
                {
                    LogHelper.WriteLog("PCS.DownloadFile", ex);
                    throw new Exception("PCS.DownloadFile", ex);
                }
            });
        }

        /// <summary>
        /// Download file use multi segments
        /// </summary>
        /// <param name="fs_id">fs_id</param>
        /// <param name="remoteFile">Download URL</param>
        /// <param name="localFile">Local save path</param>
        /// <param name="remotePath">path</param>
        public static void DownloadFileSegment(ulong fs_id, string remoteFile, string localFile, string remotePath = null)
        {
            // For first register protocol
            if (ForFirst)
            {
                ForFirst = false;
                ProtocolProviderFactory.RegisterProtocolHandler("http", typeof(MyDownloader.Extension.Protocols.HttpProtocolProvider));
                ProtocolProviderFactory.RegisterProtocolHandler("https", typeof(MyDownloader.Extension.Protocols.HttpProtocolProvider));
                ProtocolProviderFactory.RegisterProtocolHandler("ftp", typeof(MyDownloader.Extension.Protocols.FtpProtocolProvider));
                new HttpFtpProtocolExtension();
            }

            // Download item added event
            DownloadManager.Instance.DownloadAdded += (object sender, DownloaderEventArgs e) =>
            {
                DownloadManager.Instance.ClearEnded();
                MainWindow.totalData.TotalDownload = DownloadManager.Instance.Downloads.Count();
                GetDownloadInfo();
            };
            // Download item ended event
            DownloadManager.Instance.DownloadEnded += (object sender, DownloaderEventArgs e) =>
            {
                DownloadManager.Instance.ClearEnded();
                MainWindow.totalData.TotalDownload = DownloadManager.Instance.Downloads.Count();
                GetDownloadInfo();
            };
            // If remoteFile not a url, get the download url
            string URL;
            if (remoteFile.Contains("http://") || remoteFile.Contains("https://"))
            {
                URL = remoteFile;
            }else
            {
                URL = BDC.DownloadURL(remoteFile);
            }
            // Instance downloader
            Downloader downloader = DownloadManager.Instance.Add(
                ResourceLocation.FromURL(URL),
                new ResourceLocation[] { },
                localFile,
                Convert.ToInt32(Setting.DownloadSegment),
                true,
                fs_id);
            double lastProgress = 0d;
            bool forOnce = true;
            // Downloader info received event
            downloader.InfoReceived += (object sender, EventArgs e) =>
            {
                forOnce = true;
                MainWindow.DownloadListChangeItems(fs_id, ((Downloader)sender).FileSize / 1048576L, 0L, 0d, 0d);
            };
            // Downloader ending event
            downloader.Ending += (object sender, EventArgs e) =>
            {
                MainWindow.DownloadListChangeItems(fs_id, ((Downloader)sender).FileSize / 1048576L, ((Downloader)sender).Transfered / 1048576L, 100d, 0d);
            };
            // Downloader data received event
            downloader.DataReceived += (object sender, DownloaderEventArgs e) => {
                if (e.Downloader.Progress - lastProgress > 1d)
                {
                    MainWindow.DownloadListChangeItems(fs_id, e.Downloader.FileSize / 1048576L, e.Downloader.Transfered / 1048576L, Math.Round(e.Downloader.Progress, 1), Math.Round(e.Downloader.Rate / 1000d, 1));
                    lastProgress = e.Downloader.Progress;
                    GetDownloadInfo();
                }
            };
            // Downloader state changed event
            downloader.StateChanged += (object sender, EventArgs e) => {
                MainWindow.DownloadListChangeItems(fs_id, ((Downloader)sender).FileSize / 1048576L, ((Downloader)sender).Transfered / 1048576L, Math.Round(((Downloader)sender).Progress, 1), Math.Round(((Downloader)sender).Rate / 1000d, 1));
            };
            // Downloader segment failed event
            downloader.SegmentFailed += async(object sender, SegmentEventArgs e) =>
            {
                if (forOnce)
                {
                    forOnce = false;
                    e.Downloader.ResourceLocation = ResourceLocation.FromURL(await BDC.DownloadURLAsync(remotePath));
                    ResourceLocation[] mirrors = new ResourceLocation[2] { ResourceLocation.FromURL(await BDC.DownloadURLAsync(remotePath)) ,
                        ResourceLocation.FromURL(await BDC.DownloadURLAsync(remotePath)) };
                    e.Downloader.Mirrors = mirrors.ToList();
                }
            };
        }

        private static void GetDownloadInfo()
        {
            MainWindow.totalData.TotalRate = Math.Round(DownloadManager.Instance.TotalDownloadRate / 1000d, 1);
        }

        /// <summary>
        /// Get download url use remoteFile
        /// </summary>
        /// <param name="access_token">Baidu access token</param>
        /// <param name="remoteFile">Remote full file path</param>
        /// <returns>Task<string></returns>
        [Obsolete]
        public static Task<string> DownloadURL(string access_token, string remoteFile)
        {
            return Task.Factory.StartNew(()=> {
                try
                {
                    HttpHelper http = new HttpHelper();
                    HttpItem item = new HttpItem()
                    {
                        URL = DownloadBaseURL + "file?method=download&access_token=" + access_token + "&path=" + Other.Tools.URLEncoding(remoteFile, Encoding.UTF8),
                        Encoding = Encoding.UTF8,
                        Timeout = PCS.Timeout
                    };
                    var result = http.GetHtml(item);
                    string[] location = result.Header.GetValues("location");
                    return location[0].Replace("\"", "");
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog("PCS.DownloadURL", ex);
                    throw new Exception("PCS.DownloadURL", ex);
                }
            });
        }

        /// <summary>
        /// Get download url use remoteFile
        /// </summary>
        /// <param name="remoteFile">Remote full file path</param>
        /// <returns>string URL</returns>
        [Obsolete]
        public static string DownloadURL(string remoteFile)
        {
            try
            {
                HttpHelper http = new HttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = DownloadBaseURL + "file?method=download&access_token=" + Setting.Baidu_Access_Token + "&path=" + Other.Tools.URLEncoding(remoteFile, Encoding.UTF8),
                    Encoding = Encoding.UTF8,
                    Timeout = PCS.Timeout
                };
                var result = http.GetHtml(item);
                string[] location = result.Header.GetValues("location");
                return location[0].Replace("\"", "");
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("PCS.DownloadURL", ex);
                throw new Exception("PCS.DownloadURL", ex);
            }
        }

        [Obsolete]
        public static Task<bool> DeleteSingleFile(string access_token, string path)
        {
            return Task.Factory.StartNew(()=>{
                HttpHelper http = new HttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = DownloadBaseURL + "file?method=delete&access_token=" + access_token + "&path=" + Other.Tools.URLEncoding(path, Encoding.UTF8),
                    Encoding = Encoding.UTF8,
                    Timeout = PCS.Timeout
                };
                var result = http.GetHtml(item);
                if (result.Html.Contains("error_code"))
                {
                    return false;
                }
                return true;
            });
        }
        [Obsolete]
        public static Task<bool> DeleteSingleFile(string path)
        {
            return DeleteSingleFile(Setting.Baidu_Access_Token, path);
        }
    }
}
