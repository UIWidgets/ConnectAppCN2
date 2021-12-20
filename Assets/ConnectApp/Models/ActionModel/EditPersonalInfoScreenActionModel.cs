using System;
using ConnectApp.Models.Model;
using Unity.UIWidgets.async;

namespace ConnectApp.Models.ActionModel {
    public class EditPersonalInfoScreenActionModel : BaseActionModel {
        public Func<string, string, string, string, Future> editPersonalInfo;
        public Func<string, Future> updateAvatar;
        public Action<string> changeFullName;
        public Action<string> changeTitle;
        public Action<JobRole> changeJobRole;
        public Action cleanPersonalInfo;
    }
}