using System;
using Unity.UIWidgets.async;

namespace ConnectApp.Models.ActionModel {
    public class BindUnityScreenActionModel : BaseActionModel {
        public Action popToMainRoute;
        public Action<string> openUrl;
        public Func<Future> openCreateUnityIdUrl;
        public Action<string> changeEmail;
        public Action<string> changePassword;
        public Action clearEmailAndPassword;
        public Func<Future> loginByEmail;
    }
}