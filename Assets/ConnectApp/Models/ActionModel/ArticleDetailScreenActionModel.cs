using System;
using ConnectApp.Models.Model;
using Unity.UIWidgets.async;

namespace ConnectApp.Models.ActionModel {
    public class ArticleDetailScreenActionModel : BaseActionModel {
        public Action<string> openUrl;
        public Action<string, string, int> playVideo;
        public Action<string> blockArticleAction;
        public Action<string> blockUserAction;
        public Action startFetchArticleDetail;
        public Func<string, Future> fetchArticleDetail;
        public Func<string, string, Future> fetchArticleComments;
        public Func<string, int, Future> likeArticle;
        public Func<Message, Future> likeComment;
        public Func<Message, Future> removeLikeComment;
        public Func<string, string, string, string, string, Future> sendComment;
        public Action<string> startFollowUser;
        public Func<string, Future> followUser;
        public Action<string> startUnFollowUser;
        public Func<string, Future> unFollowUser;
        public Action<string> startFollowTeam;
        public Func<string, Future> followTeam;
        public Action<string> startUnFollowTeam;
        public Func<string, Future> unFollowTeam;
    }
}