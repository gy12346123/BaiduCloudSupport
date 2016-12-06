using DotNet4.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BaiduCloudSupport.API
{
    public class BDC
    {
        /// <summary>
        /// Baidu cloud api base URL
        /// </summary>
        private static string BDCBaseURL = "http://pan.baidu.com/api/";

        private static string BDCShareURL = "http://pan.baidu.com/share/";

        private static string BDCDownloadBaseURL = "http://d.pcs.baidu.com/rest/2.0/pcs/";

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

        private static string bdstoken;

        private static string ConvertFileName(string file)
        {
            string convertedPath = file;

            if (convertedPath.Contains("#"))
            {
                convertedPath = convertedPath.Replace("#", @"\u0023");
            }
            if (convertedPath.Contains("+"))
            {
                convertedPath = convertedPath.Replace("+", @"\u002b");
            }
            if (convertedPath.Contains("&"))
            {
                convertedPath = convertedPath.Replace("&", @"\u0026");
            }
            if (convertedPath.Contains(" "))
            {
                convertedPath = convertedPath.Replace(" ", @"\u0020");
            }
            if (convertedPath.Contains("?"))
            {
                convertedPath = convertedPath.Replace("?", @"\u003f");
            }
            if (convertedPath.Contains("%"))
            {
                convertedPath = convertedPath.Replace("%", @"\u0025");
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

        public static Task LoadCookieAsync(string path, bool overwrite = false)
        {
            return Task.Factory.StartNew(()=> {
                LoadCookie(path, overwrite);
            });
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

        public static FileListStruct[] SingleFolder(string path, int page, int maxNum = 100, int desc = 0, Order order = Order.name)
        {
            if (!CheckCookie()) LoadCookie(Setting.Baidu_CookiePath);
            string ConvertedPath = Other.Tools.URLEncoding(path, Encoding.UTF8);
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

        public static Task<FileListStruct[]> SingleFolderAsync(string path, int page, int maxNum = 100, int desc = 0, Order order = Order.name)
        {
            return Task.Factory.StartNew(()=> {
                return SingleFolder(path, page, maxNum, desc, order);
            });
        }

        public static Task<DBCFolderListStruct[]> OnlyFolderInfo(string path, int page = 0, int maxNum = 500, int desc = 0, Order order = Order.name)
        {
            return Task.Factory.StartNew(()=> {
                if (!CheckCookie()) LoadCookie(Setting.Baidu_CookiePath);
                string ConvertedPath = Other.Tools.URLEncoding(path, Encoding.UTF8);
                HttpHelper http = new HttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = string.Format("{0}list?folder=1&dir={1}&page={2}&num={3}&desc={4}&order={5}&clienttype=0&showempty=0&web=1&channel=chunlei&app_id=250528", BDCBaseURL, ConvertedPath, page, maxNum, desc, order.ToString()),
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
                    DBCFolderListStruct[] folderListStruct = new DBCFolderListStruct[listCount];
                    for (int i = 0; i < listCount; i++)
                    {
                        folderListStruct[i].dir_empty = Convert.ToInt32(json["list"][i]["dir_empty"]);
                        folderListStruct[i].path = json["list"][i]["path"].ToString();
                    }
                    return folderListStruct;
                }
                else
                {
                    throw new ErrorCodeException();
                }
            });
        }

        public static Task<FileListStruct[]> SearchFile(string keyword, int page = 1, int maxNum = 100, int desc = 0, Order order = Order.name)
        {
            return Task.Factory.StartNew(()=> {
                if (!CheckCookie()) LoadCookie(Setting.Baidu_CookiePath);
                string ConvertedKeyword = Other.Tools.URLEncoding(keyword, Encoding.UTF8);
                HttpHelper http = new HttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = string.Format("{0}search?recursion=1&key={1}&page={2}&num={3}&desc={4}&order={5}&clienttype=0&showempty=0&web=1&channel=chunlei&app_id=250528", BDCBaseURL, ConvertedKeyword, page, maxNum, desc, order.ToString()),
                    Encoding = Encoding.UTF8,
                    Timeout = BDC.Timeout,
                    Referer = "http://pan.baidu.com/disk/home#search/key=" + ConvertedKeyword,
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

        private static bool GetParamFromHtml()
        {
            HttpHelper http = new HttpHelper();
            HttpItem item = new HttpItem()
            {
                URL = "http://pan.baidu.com/disk/home#list/path=%2F&vmode=list",
                Encoding = Encoding.UTF8,
                Timeout = BDC.Timeout,
                Referer = "http://pan.baidu.com/disk/home",
                Host = "pan.baidu.com",
                Cookie = Cookies
            };
            string result = http.GetHtml(item).Html;
            Regex regex = new Regex("\"bdstoken\":\"[a-fA-F0-9]{32}");
            Match match = regex.Match(result);
            if (match.Success)
            {
                bdstoken = match.Value.Replace("\"bdstoken\":\"","");
                return true;
            }
            return false;
        }

        public static Task<bool> CopyTo(List<DBCCopyStruct> list)
        {
            return Task.Factory.StartNew(()=> {
                if (!CheckCookie()) LoadCookie(Setting.Baidu_CookiePath);
                if (bdstoken == null || bdstoken.Equals(""))
                {
                    if (!GetParamFromHtml())
                    {
                        return false;
                    }
                }
                StringBuilder SB = new StringBuilder();
                //SB.Append("filelist=[");
                SB.Append("[");
                foreach (DBCCopyStruct file in list)
                {
                    SB.Append("{");
                    SB.Append(string.Format("\"path\":\"{0}\",\"dest\":\"{1}\",\"newname\":\"{2}\"",file.path, file.dest, file.newname));
                    SB.Append("},");
                }
                SB.Remove(SB.Length - 1, 1);
                SB.Append("]");
                HttpHelper http = new HttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = string.Format("{0}filemanager?opera=copy&async=2&channel=chunlei&web=1&app_id=250528clienttype=0&bdstoken={1}", BDCBaseURL, bdstoken),
                    Method = "POST",
                    Encoding = Encoding.UTF8,
                    Timeout = BDC.Timeout,
                    Referer = "http://pan.baidu.com/disk/home#list/vmode=list&path=%2F",
                    Host = "pan.baidu.com",
                    Cookie = Cookies,
                    Postdata = "filelist=" + Other.Tools.URLEncoding(SB.ToString(), Encoding.UTF8),
                    Accept = " application/json, text/javascript, */*; q=0.01",
                    PostEncoding = Encoding.UTF8,
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                };
                string result = http.GetHtml(item).Html;
                if (result.Contains("errno\":0"))
                {
                    return true;
                }
                return false;
            });
        }

        public static Task<bool> MoveTo(List<DBCCopyStruct> list)
        {
            return Task.Factory.StartNew(() => {
                if (!CheckCookie()) LoadCookie(Setting.Baidu_CookiePath);
                if (bdstoken == null || bdstoken.Equals(""))
                {
                    if (!GetParamFromHtml())
                    {
                        return false;
                    }
                }
                StringBuilder SB = new StringBuilder();
                //SB.Append("filelist=[");
                SB.Append("[");
                foreach (DBCCopyStruct file in list)
                {
                    SB.Append("{");
                    SB.Append(string.Format("\"path\":\"{0}\",\"dest\":\"{1}\",\"newname\":\"{2}\"", file.path, file.dest, file.newname));
                    SB.Append("},");
                }
                SB.Remove(SB.Length - 1, 1);
                SB.Append("]");
                HttpHelper http = new HttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = string.Format("{0}filemanager?opera=move&async=2&channel=chunlei&web=1&app_id=250528clienttype=0&bdstoken={1}", BDCBaseURL, bdstoken),
                    Method = "POST",
                    Encoding = Encoding.UTF8,
                    Timeout = BDC.Timeout,
                    Referer = "http://pan.baidu.com/disk/home#list/vmode=list&path=%2F",
                    Host = "pan.baidu.com",
                    Cookie = Cookies,
                    Postdata = "filelist=" + Other.Tools.URLEncoding(SB.ToString(), Encoding.UTF8),
                    Accept = " application/json, text/javascript, */*; q=0.01",
                    PostEncoding = Encoding.UTF8,
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                };
                string result = http.GetHtml(item).Html;
                if (result.Contains("errno\":0"))
                {
                    return true;
                }
                return false;
            });
        }

        public static Task<DBCFileShareStruct> Share(ulong fs_id, string password = "")
        {
            return Task.Factory.StartNew(()=> {
                if (!CheckCookie()) LoadCookie(Setting.Baidu_CookiePath);
                if (bdstoken == null || bdstoken.Equals(""))
                {
                    if (!GetParamFromHtml())
                    {
                        throw new Exception("Get param from html error.");
                    }
                }
                StringBuilder SB = new StringBuilder();
                SB.Append(string.Format("fid_list=[{0}]&schannel=", fs_id));
                if (password.Equals(""))
                {
                    // No password
                    SB.Append("0&channel_list=[]");
                }else if (password.Count() == 4)
                {
                    // Set password
                    SB.Append(string.Format("4&channel_list=[]&pwd={0}", password));
                }else
                {
                    throw new Exception("Password error, set 4 characters.");
                }

                HttpHelper http = new HttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = string.Format("{0}set?channel=chunlei&web=1&app_id=250528&clienttype=0&bdstoken={1}", BDCShareURL, bdstoken),
                    Method = "POST",
                    Encoding = Encoding.UTF8,
                    Timeout = BDC.Timeout,
                    Referer = "http://pan.baidu.com/disk/home#list/vmode=list&path=%2F",
                    Host = "pan.baidu.com",
                    Cookie = Cookies,
                    Postdata = SB.ToString(),
                    PostEncoding = Encoding.UTF8,
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8"
                };
                string result = http.GetHtml(item).Html;

                if (result.Contains("errno\":0"))
                {
                    var json = (JObject)JsonConvert.DeserializeObject(result);
                    return new DBCFileShareStruct {
                        ctime = Convert.ToUInt32(json["ctime"]),
                        shareid = Convert.ToUInt64(json["shareid"]),
                        link = json["link"].ToString(),
                        shorturl = json["shorturl"].ToString(),
                        password = password
                    };
                }
                else
                {
                    throw new ErrorCodeException();
                }
            });
        }

        public static Task<string> Transfer(string shareLink, string toFolder = "/apps/wp2pcs")
        {
            return Task.Factory.StartNew(()=> {
                if (!CheckCookie()) LoadCookie(Setting.Baidu_CookiePath);
                if (bdstoken == null || bdstoken.Equals(""))
                {
                    if (!GetParamFromHtml())
                    {
                        throw new Exception("Get param from html error.");
                    }
                }
                HttpHelper http = new HttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = shareLink,
                    Timeout = BDC.Timeout,
                    Referer = shareLink,
                    Host = "pan.baidu.com",
                    Accept = "*/*",
                    Cookie = Cookies
                };
                string result = http.GetHtml(item).Html;
                //string cookie = http.GetHtml(item).Cookie;
                Match match_ShareId = Regex.Match(result, "shareid\":[0-9]{1,}(?=,)");
                if (!match_ShareId.Success)
                {
                    throw new Exception("Can not match shareid.");
                }
                ulong shareid = Convert.ToUInt64(match_ShareId.Value.Replace("shareid\":", ""));
                Match match_FromUserId = Regex.Match(result, "/share/home\\?uk=[0-9]{1,}(?=\")");
                if (!match_FromUserId.Success)
                {
                    throw new Exception("Can not match fromuserid.");
                }
                ulong fromuserid = Convert.ToUInt64(match_FromUserId.Value.Replace("/share/home?uk=", ""));
                Match match_File = Regex.Match(result, "server_filename\":\".*?(?=\",\")");
                if (!match_File.Success)
                {
                    throw new Exception("Can not match file name.");
                }
                string file = match_File.Value.Replace("server_filename\":\"", "");
                Match Match_ParentPath = Regex.Match(result, "parent_path\":\".*?(?=\",\")");
                if (!Match_ParentPath.Success)
                {
                    throw new Exception("Can not match file parent path.");
                }
                string parentPath = Match_ParentPath.Value;

                if (parentPath.Equals("parent_path\":\""))
                {
                    parentPath = "%2F";
                }else
                {
                    parentPath = parentPath.Replace("parent_path\":\"", "") + "%2F";
                }

                StringBuilder SB = new StringBuilder();
                //SB.Append("filelist=[\"/");
                //SB.Append(ConvertFileName(file));
                SB.Append("filelist=[\"");
                SB.Append(parentPath);
                SB.Append(Other.Tools.URLEncoding(Regex.Unescape(file), Encoding.UTF8));
                SB.Append("\"]&path=");
                //SB.Append(toFolder);
                SB.Append(Other.Tools.URLEncoding(toFolder, Encoding.UTF8));
                HttpItem item_Transfer = new HttpItem()
                {
                    URL = string.Format("{0}transfer?shareid={1}&from={2}&ondup=newcopy&async=1&bdstoken={3}&channel=chunlei&clienttype=0&web=1&app_id=250528", BDCShareURL, shareid, fromuserid, bdstoken),
                    Method = "POST",
                    Encoding = Encoding.UTF8,
                    Timeout = BDC.Timeout,
                    Referer = shareLink,
                    Host = "pan.baidu.com",
                    Cookie = Cookies,
                    Postdata = SB.ToString(),
                    PostEncoding = Encoding.UTF8,
                    ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                    Accept = "*/*",
                    ProtocolVersion = new Version(1,1),
                    KeepAlive = true
                };
                string result_Transfer = http.GetHtml(item_Transfer).Html;

                if (result_Transfer.Contains("errno\":0"))
                {
                    return string.Format("{0}/{1}", toFolder, Regex.Unescape(file));
                }
                throw new ErrorCodeException("BDC.Transfer");
            });
        }

        /// <summary>
        /// Get download url use remoteFile
        /// </summary>
        /// <param name="remoteFile">Remote full file path</param>
        /// <returns>string URL</returns>
        public static string DownloadURL(string remoteFile)
        {
            try
            {
                if (!CheckCookie()) LoadCookie(Setting.Baidu_CookiePath);
                string convertedFile = Other.Tools.URLEncoding(remoteFile, Encoding.UTF8);
                string[] pathParam = remoteFile.Split('/');
                HttpHelper http = new HttpHelper();
                HttpItem item = new HttpItem()
                {
                    URL = BDCDownloadBaseURL + "file?method=download&app_id=250528&check_blue=1&ec=1&err_ver=1.0&es=1&sup=1&path=" + convertedFile,
                    Encoding = Encoding.UTF8,
                    Timeout = BDC.Timeout,
                    Cookie = Cookies,
                    Referer = "http://pan.baidu.com/disk/home#list/vmode=list&path=" + Other.Tools.URLEncoding(remoteFile.Replace("/" + pathParam[pathParam.Count() - 1], ""), Encoding.UTF8),
                };
                var result = http.GetHtml(item);
                string[] location = result.Header.GetValues("location");
                return location[0];
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("BDC.DownloadURL", ex);
                throw new Exception("BDC.DownloadURL", ex);
            }
        }

        public static Task<string> DownloadURLAsync(string remoteFile)
        {
            return Task.Factory.StartNew(()=> {
                return BDC.DownloadURL(remoteFile);
            });
        }

        /// <summary>
        /// Get baidu cloud quota
        /// </summary>
        /// <param name="access_token">Baidu access token</param>
        /// <returns>[0]:quota, [1]:used</returns>
        public static ulong[] Quota()
        {
            if (!CheckCookie()) LoadCookie(Setting.Baidu_CookiePath);
            if (bdstoken == null || bdstoken.Equals(""))
            {
                if (!GetParamFromHtml())
                {
                    throw new Exception("Get param from html error.");
                }
            }
            HttpHelper http = new HttpHelper();
            HttpItem item = new HttpItem()
            {
                URL = BDCBaseURL + "quota?checkexpire=1&checkfree=1&channel=chunlei&web=1&app_id=250528&bdstoken={0}&clienttype=0" + bdstoken,
                Encoding = Encoding.UTF8,
                Timeout = BDC.Timeout,
                Cookie = Cookies,
                Referer = "http://pan.baidu.com/disk/home#list/path=%2F&vmode=list"
            };
            string result = http.GetHtml(item).Html;
            if (result.Contains("errno\":0"))
            {
                var json = (JObject)JsonConvert.DeserializeObject(result);
                return new ulong[2] { Convert.ToUInt64(json["total"]) / Convert.ToUInt64(Math.Pow(1024d, 3d)), Convert.ToUInt64(json["used"]) / Convert.ToUInt64(Math.Pow(1024d, 3d)) };
            }
            else
            {
                throw new ErrorCodeException();
            }
        }

        public static Task<ulong[]> QuotaAsync()
        {
            return Task.Factory.StartNew(()=> {
                return Quota();
            });
        }

        public static bool Delete(string[] path)
        {
            if (!CheckCookie()) LoadCookie(Setting.Baidu_CookiePath);
            if (bdstoken == null || bdstoken.Equals(""))
            {
                if (!GetParamFromHtml())
                {
                    throw new Exception("Get param from html error.");
                }
            }
            StringBuilder SB = new StringBuilder();
            SB.Append("[");
            foreach (string p in path)
            {
                SB.Append(string.Format("\"{0}\",", p));
            }
            SB.Remove(SB.Length - 1, 1);
            SB.Append("]");
            string[] param = path[0].Split('/');
            string GetOnePath = path[0].Replace("/" + param[param.Count() - 1], "");
            HttpHelper http = new HttpHelper();
            HttpItem item = new HttpItem()
            {
                URL = string.Format("{0}filemanager?opera=delete&async=2&channel=chunlei&web=1&app_id=250528&bdstoken={1}&clienttype=0", BDCBaseURL, bdstoken),
                Method = "POST",
                Timeout = BDC.Timeout,
                Referer = "http://pan.baidu.com/disk/home#list/vmode=list&path=" + Other.Tools.URLEncoding(GetOnePath, Encoding.UTF8),
                Host = "pan.baidu.com",
                Cookie = Cookies,
                Postdata = "filelist=" + Other.Tools.URLEncoding(SB.ToString(), Encoding.UTF8),
                PostEncoding = Encoding.UTF8,
                ContentType = "application/x-www-form-urlencoded; charset=UTF-8"
            };
            string result = http.GetHtml(item).Html;
            if (result.Contains("errno\":0"))
            {
                return true;
            }
            else
            {
                throw new ErrorCodeException();
            }
        }

        public static Task<bool> DeleteAsync(string[] path)
        {
            return Task.Factory.StartNew(()=> {
                return Delete(path);
            });
        }

    }
}
