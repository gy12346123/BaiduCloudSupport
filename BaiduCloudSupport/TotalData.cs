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
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs("Scope"));
                    }
                }
            }
        }
    }
}
