using System;
using Unity.UIWidgets.async;

namespace ConnectApp.Models.ActionModel {
    public class LoginSwitchScreenActionModel : BaseActionModel {
        public Action popToMainRoute;
        public Func<string, Action<bool>, Future> loginByWechatAction;
        public Action<string> openUrl;
    }
}