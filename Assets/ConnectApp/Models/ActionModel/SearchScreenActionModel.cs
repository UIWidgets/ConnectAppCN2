using System;
using Unity.UIWidgets.async;

namespace ConnectApp.Models.ActionModel {
    public class SearchScreenActionModel : BaseActionModel {
        public Action<string> startSearchArticle;
        public Action<string> startSearchUser;
        public Action<string> startSearchTeam;
        public Action<string> startSearchQuestion;
        public Func<string, int, Future> searchArticle;
        public Func<string, int, Future> searchUser;
        public Func<string, int, Future> searchTeam;
        public Func<string, int, Future> searchQuestion;
        public Func<Future> fetchPopularSearch;
        public Action clearSearchResult;
        public Action<string> saveSearchArticleHistory;
        public Action<string> deleteSearchArticleHistory;
        public Action deleteAllSearchArticleHistory;
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