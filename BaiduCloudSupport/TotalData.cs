using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    Setting.WriteAppSetting("Baidu_Access_Token", value, true);
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
                    Setting.WriteAppSetting("Baidu_Expires_In", value, true);
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
                    Setting.WriteAppSetting("Baidu_Session_Secret", value, true);
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
                    Setting.WriteAppSetting("Baidu_Session_Key", value, true);
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
                    Setting.WriteAppSetting("Baidu_Scope", value, true);
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
    }
}
