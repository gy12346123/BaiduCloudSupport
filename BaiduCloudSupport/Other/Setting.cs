using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaiduCloudSupport
{
    class Setting
    {
        /// <summary>
        /// Program base path,with "\" at the last.
        /// </summary>
        public static readonly string BasePath = System.AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Program text language
        /// </summary>
        public static string MainLanguage = ConfigurationManager.AppSettings["MainLanguage"];

        public static string Baidu_client_id = ConfigurationManager.AppSettings["Baidu_client_id"];

        public static string Baidu_redirect_uri = ConfigurationManager.AppSettings["Baidu_redirect_uri"];

        /// <summary>
        /// Reload setting data
        /// </summary>
        public static void Reload()
        {
            ConfigurationManager.RefreshSection("appSettings");
            MainLanguage = ConfigurationManager.AppSettings["MainLanguage"];
            Baidu_client_id = ConfigurationManager.AppSettings["Baidu_client_id"];
            Baidu_redirect_uri = ConfigurationManager.AppSettings["Baidu_redirect_uri"];
        }
    }
}
