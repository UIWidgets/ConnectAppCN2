using System;
using Unity.UIWidgets.async;

namespace ConnectApp.Models.ActionModel {
    public class NotificationScreenActionModel : BaseActionModel {
        public Action startFetchNotifications;
        public Func<int, Future> fetchNotifications;
    }
}