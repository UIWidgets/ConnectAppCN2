using System;

namespace ConnectApp.Models.ActionModel {
    public class RealNameScreenActionModel : BaseActionModel {
        public Action<string> openUrl;
        public Action<string, string, string> checkRealName;
    }
}