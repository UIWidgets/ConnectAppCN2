using System;
using ConnectApp.Components;
using Unity.UIWidgets.async;

namespace ConnectApp.Models.ActionModel {
    public class UserDetailScreenActionModel : BaseActionModel {
        public Action<string> blockArticleAction;
        public Action startFetchUserProfile;
        public Func<Future> fetchUserProfile;
        public Action startFetchUserArticle;
        public Func<string, int, Future> fetchUserArticle;
        public Action startFetchUserFavorite;
        public Func<string, int, Future> fetchUserFavorite;
        public Action startFetchUserFollowFavorite;
        public Func<string, int, Future> fetchUserFollowFavorite;
        public Action<string> startFollowUser;
        public Func<string, Future> followUser;
        public Action<string> startUnFollowUser;
        public Func<string, Future> unFollowUser;
        public Func<string, Future> deleteFavoriteTag;
        public Action<bool> blockUserAction;
    }
}