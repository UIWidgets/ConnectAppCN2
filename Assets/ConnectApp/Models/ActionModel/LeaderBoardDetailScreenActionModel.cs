using System;
using ConnectApp.Components;
using Unity.UIWidgets.async;

namespace ConnectApp.Models.ActionModel {
    public class LeaderBoardDetailScreenActionModel : BaseActionModel {
        public Action startFetchDetailList;
        public Func<int, Future> fetchDetailList;
        public Func<string, string, Future> collectFavoriteTag;
        public Func<string, string, Future> cancelCollectFavoriteTag;
        public Action<string> startFollowUser;
        public Action<string> startUnFollowUser;
        public Func<string, Future> followUser;
        public Func<string, Future> unFollowUser;
        public Action<string> blockArticleAction;
    }
}