using System.Collections.Generic;
using ConnectApp.Models.Model;
using ConnectApp.screens;

namespace ConnectApp.Models.ViewModel {
    public class SearchScreenViewModel {
        public bool searchArticleLoading;
        public bool searchUserLoading;
        public bool searchTeamLoading;
        public bool searchQuestionLoading;
        public SearchType searchType;
        public string preSearchKeyword;
        public string searchKeyword;
        public string searchSuggest;
        public List<string> searchArticleIds;
        public List<string> searchUserIds;
        public List<string> searchTeamIds;
        public List<string> searchQuestionIds;
        public List<Question> searchQuestions;
        public bool searchArticleHasMore;
        public bool searchUserHasMore;
        public bool searchTeamHasMore;
        public bool searchQuestionHasMore;
        public Dictionary<string, bool> followMap;
        public List<string> searchArticleHistoryList;
        public List<string> searchUserHistoryList;
        public Dictionary<string, Article> articleDict;
        public Dictionary<string, User> userDict;
        public Dictionary<string, UserLicense> userLicenseDict;
        public Dictionary<string, Team> teamDict;
        public Dictionary<string, Question> questionDict;
        public Dictionary<string, Tag> tagDict;
        public List<PopularSearch> popularSearchArticleList;
        public List<PopularSearch> popularSearchUserList;
        public List<string> blockArticleList;
        public string currentUserId;
        public bool isLoggedIn;
    }
}