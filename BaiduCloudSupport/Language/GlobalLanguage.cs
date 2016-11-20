using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BaiduCloudSupport
{
    /// <summary>
    /// Set text language and search text with resource key
    /// </summary>
    class GlobalLanguage
    {
        /// <summary>
        /// Language list, add new language need create new file in Language file with Lang.*.xaml named
        /// </summary>
        public enum LanguageList { en, jp, zh };

        /// <summary>
        /// Read language data
        /// </summary>
        /// <param name="lang">Language Name</param>
        public static void SetLanguage(string lang)
        {
            List<ResourceDictionary> dictionaryList = new List<ResourceDictionary>();
            foreach (ResourceDictionary dictionary in Application.Current.Resources.MergedDictionaries)
            {
                dictionaryList.Add(dictionary);
            }
            string requestedCulture = string.Format(@"Language/Lang.{0}.xaml", lang);
            ResourceDictionary resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString.Equals(requestedCulture));
            if (resourceDictionary == null)
            {
                requestedCulture = @"Language/Lang.zh.xaml";
                resourceDictionary = dictionaryList.FirstOrDefault(d => d.Source.OriginalString.Equals(requestedCulture));
            }
            if (resourceDictionary != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(resourceDictionary);
                Application.Current.Resources.MergedDictionaries.Add(resourceDictionary);
            }
        }

        /// <summary>
        /// Read the language resource from /Language/Lang.*.xaml witch ResourceKey.
        /// </summary>
        /// <param name="ResourceKey">Key</param>
        /// <returns></returns>
        public static string FindText(string ResourceKey)
        {
            try
            {
                return Application.Current.FindResource(ResourceKey).ToString();
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog("GlobalLanguage.FindText", ex);
                return ResourceKey;
            }
        }
    }
}
