using System;
using Unity.UIWidgets.async;

namespace ConnectApp.Models.ActionModel {
    public class ReportScreenActionModel : BaseActionModel {
        public Func<string, Future> reportItem;
    }
}