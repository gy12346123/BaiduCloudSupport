using DotNet4.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BaiduCloudSupport.API
{
    class ShortURL
    {
        public static string Shorten(string longUrl)
        {
            HttpHelper http = new HttpHelper();
            HttpItem item = new HttpItem()
            {
                // Use your own source(App Key)
                URL = string.Format("http://api.t.sina.com.cn/short_url/shorten.json?source=4288490654&url_long={0}", Other.Tools.URLEncoding(longUrl, Encoding.UTF8)),
                Encoding = Encoding.UTF8,
                Timeout = 30000,
            };
            string result = http.GetHtml(item).Html;
            if (result.Contains("url_short"))
            {
                Match match = Regex.Match(result, "(?<=url_short\":\").*?(?=\",\")");
                if (match.Success)
                {
                    return match.Value;
                }
            }
            throw new Exception("ShortURL.Shorten");
        }

        public static Task<string> ShortenAsync(string longUrl)
        {
            return Task.Factory.StartNew(()=> {
                return Shorten(longUrl);
            });
        }
    }
}
