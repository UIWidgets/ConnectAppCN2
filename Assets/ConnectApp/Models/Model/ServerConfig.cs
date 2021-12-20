using System;
using System.Collections.Generic;

namespace ConnectApp.Models.Model {
    [Serializable]
    public class ServerConfig {
        public string minVersionCode;
        public bool hiddenWexinIOS;
        public bool hiddenWexinAndroid;
        public bool hiddenRegister;
        public string termsUpdateTime;
        public List<string> termsUpdateItems;
    }
}