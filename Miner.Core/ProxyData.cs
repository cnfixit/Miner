using System;
using System.Collections.Generic;
using System.Text;

namespace Miner
{
    public class ProxyData
    {

        private string _ProxyHost;

        public string ProxyHost
        {
            get { return _ProxyHost; }
            set { _ProxyHost = value; }
        }

        private int _ProxyPort;

        public int ProxyPort
        {
            get { return _ProxyPort; }
            set { _ProxyPort = value; }
        }

        private string _ProxyUser;

        public string ProxyUser
        {
            get { return _ProxyUser; }
            set { _ProxyUser = value; }
        }

        private string _ProxyPassword;

        public string ProxyPassword
        {
            get { return _ProxyPassword; }
            set { _ProxyPassword = value; }
        }
    }
}
