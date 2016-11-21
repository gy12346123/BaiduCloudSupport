using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaiduCloudSupport.Login
{
    class BaiduLogin
    {
        public static string LoginWeb(string client_id,string redirect_uri = "oob",string scope = "netdisk")
        {
            return string.Format("https://openapi.baidu.com/oauth/2.0/authorize?response_type=token&client_id={0}&redirect_uri={1}&scope={2}", client_id,redirect_uri,scope);
        }
    }
}
