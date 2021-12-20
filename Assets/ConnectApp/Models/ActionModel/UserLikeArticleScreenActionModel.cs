using System;
using ConnectApp.Components;
using Unity.UIWidgets.async;

namespace ConnectApp.Models.ActionModel {
    public class UserLikeArticleScreenActionModel : BaseActionModel {
        public Action startFetchUserLikeArticle;
        public Func<int, Future> fetchUserLikeArticle;
        public Action<string> blockArticleAction;
    }
}