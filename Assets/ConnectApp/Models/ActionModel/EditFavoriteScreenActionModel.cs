using System;
using ConnectApp.Models.Model;
using Unity.UIWidgets.async;

namespace ConnectApp.Models.ActionModel {
    public class EditFavoriteScreenActionModel : BaseActionModel {
        public Func<string, IconStyle, string, string, Future> editFavoriteTag;
        public Func<IconStyle, string, string, Future> createFavoriteTag;
    }
}