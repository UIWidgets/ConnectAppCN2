using System;
using ConnectApp.Components;
using Unity.UIWidgets.async;

namespace ConnectApp.Models.ActionModel {
    public class BaseActionModel {
        public Action<string> copyText;
        public Func<ShareSheetItemType, string, string, string, string, string, Future> shareToWechat;
    }
}