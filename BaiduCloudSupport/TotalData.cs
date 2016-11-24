using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace BaiduCloudSupport
{
    public class TotalData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _Access_Token;
        public string Access_Token
        {
            get { return _Access_Token; }
            set
            {
                if (_Access_Token != value)
                {
                    _Access_Token = value;
                    if (Setting.Baidu_Access_Token != value)
                    {
                        Setting.WriteAppSetting("Baidu_Access_Token", value, true);
                    }
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Access_Token"));
                    }
                }
            }
        }

        private string _Expires_In;
        public string Expires_In
        {
            get { return _Expires_In; }
            set
            {
                if (_Expires_In != value)
                {
                    _Expires_In = value;
                    if (Setting.Baidu_Expires_In != value)
                    {
                        Setting.WriteAppSetting("Baidu_Expires_In", value, true);
                    }
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Expires_In"));
                    }
                }
            }
        }

        private string _Session_Secret;
        public string Session_Secret
        {
            get { return _Session_Secret; }
            set
            {
                if (_Session_Secret != value)
                {
                    _Session_Secret = value;
                    if (Setting.Baidu_Session_Secret != value)
                    {
                        Setting.WriteAppSetting("Baidu_Session_Secret", value, true);
                    }
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Session_Secret"));
                    }
                }
            }
        }

        private string _Session_Key;
        public string Session_Key
        {
            get { return _Session_Key; }
            set
            {
                if (_Session_Key != value)
                {
                    _Session_Key = value;
                    if (Setting.Baidu_Session_Key != value)
                    {
                        Setting.WriteAppSetting("Baidu_Session_Key", value, true);
                    }
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Session_Key"));
                    }
                }
            }
        }

        private string _Scope;
        public string Scope
        {
            get { return _Scope; }
            set
            {
                if (_Scope != value)
                {
                    _Scope = value;
                    if (Setting.Baidu_Scope != value)
                    {
                        Setting.WriteAppSetting("Baidu_Scope", value, true);
                    }
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Scope"));
                    }
                }
            }
        }

        private long _SingleFileSize;
        public long SingleFileSize
        {
            get { return _SingleFileSize; }
            set
            {
                if (_SingleFileSize != value)
                {
                    _SingleFileSize = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("SingleFileSize"));
                    }
                }
            }
        }

        private long _SingleFileBytesReceived;
        public long SingleFileBytesReceived
        {
            get { return _SingleFileBytesReceived; }
            set
            {
                if (_SingleFileBytesReceived != value)
                {
                    _SingleFileBytesReceived = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("SingleFileBytesReceived"));
                    }
                }
            }
        }

        private int _SingleFileProgressPercentage;
        public int SingleFileProgressPercentage
        {
            get { return _SingleFileProgressPercentage; }
            set
            {
                if (_SingleFileProgressPercentage != value)
                {
                    _SingleFileProgressPercentage = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("SingleFileProgressPercentage"));
                    }
                }
            }
        }

        private ulong _uid;
        public ulong uid
        {
            get { return _uid; }
            set
            {
                if (_uid != value)
                {
                    _uid = value;
                    if (Setting.Baidu_uid != value.ToString())
                    {
                        Setting.WriteAppSetting("Baidu_uid", value.ToString(), true);
                    }
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("uid"));
                    }
                }
            }
        }

        private string _uname;
        public string uname
        {
            get { return _uname; }
            set
            {
                if (_uname != value)
                {
                    _uname = value;
                    if (Setting.Baidu_uname != value)
                    {
                        Setting.WriteAppSetting("Baidu_uname", value, true);
                    }
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("uname"));
                    }
                }
            }
        }

        private string _portrait;
        public string portrait
        {
            get { return _portrait; }
            set
            {
                if (_portrait != value)
                {
                    _portrait = value;
                    if (Setting.Baidu_portrait != value)
                    {
                        Setting.WriteAppSetting("Baidu_portrait", value, true);
                    }
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("portrait"));
                    }
                }
            }
        }

        private BitmapSource _UserPortrait;
        public BitmapSource UserPortrait
        {
            get { return _UserPortrait; }
            set
            {
                if (_UserPortrait != value)
                {
                    _UserPortrait = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("UserPortrait"));
                    }
                }
            }
        }

        private ulong _Quota_Total;
        public ulong Quota_Total
        {
            get { return _Quota_Total; }
            set
            {
                if (_Quota_Total != value)
                {
                    _Quota_Total = value;
                    if (Setting.Baidu_Quota_Total != value.ToString())
                    {
                        Setting.WriteAppSetting("Baidu_Quota_Total", value.ToString(), true);
                    }
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Quota_Total"));
                    }
                }
            }
        }

        private ulong _Quota_Used;
        public ulong Quota_Used
        {
            get { return _Quota_Used; }
            set
            {
                if (_Quota_Used != value)
                {
                    _Quota_Used = value;
                    if (Setting.Baidu_Quota_Used != value.ToString())
                    {
                        Setting.WriteAppSetting("Baidu_Quota_Used", value.ToString(), true);
                    }
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Quota_Used"));
                    }
                }
            }
        }

        private bool _ProgressRing_IsActive;
        public bool ProgressRing_IsActive
        {
            get { return _ProgressRing_IsActive; }
            set
            {
                if (_ProgressRing_IsActive != value)
                {
                    _ProgressRing_IsActive = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("ProgressRing_IsActive"));
                    }
                }
            }
        }

        private List<API.FileListDataItem> _FileListDataItems;
        public List<API.FileListDataItem> FileListDataItems
        {
            get { return _FileListDataItems; }
            set
            {
                if (_FileListDataItems != value)
                {
                    _FileListDataItems = value;
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("FileListDataItems"));
                    }
                }
            }
        }
    }
}
