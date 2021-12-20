using System;

namespace ConnectApp.Models.Model {
    [Serializable]
    public class LoginInfo {
        public string LSKey = "";
        public string userId = "";
        public string userFullName = "";
        public string userAvatar = "";
        public string authId = "";
        public bool anonymous = true;
        public string title = "";
        public string coverImageWithCDN= "";
        public bool isRealName;
    }
}