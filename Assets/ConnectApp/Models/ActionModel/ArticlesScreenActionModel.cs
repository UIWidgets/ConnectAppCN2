using System;
using Unity.UIWidgets.async;

namespace ConnectApp.Models.ActionModel {
    public class ArticlesScreenActionModel : BaseActionModel {
        public Action<string> openUrl;
        public Action<string> blockArticleAction;
        public Action<string> startFollowUser;
        public Func<string, Future> followUser;
        public Action<string> startUnFollowUser;
        public Func<string, Future> unFollowUser;
        public Action<string> startFollowTeam;
        public Func<string, Future> followTeam;
        public Action<string> startUnFollowTeam;
        public Func<string, Future> unFollowTeam;
        public Func<string, string, string, string, string, string, Future> sendComment;
        public Func<string, Future> likeArticle;
        public Action startFetchArticles;
        public Func<string, int, bool, Future> fetchArticles;
        public Action startFetchFollowing;
        public Func<string, int, Future> fetchFollowing;
        public Action startFetchFollowArticles;
        public Func<int, bool, bool, Future> fetchFollowArticles;
        public Action<int> switchHomePageTabBarIndex;
        public Action<bool> changeSwiperStatus;
    }
}