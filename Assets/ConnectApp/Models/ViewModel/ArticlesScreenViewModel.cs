using System.Collections.Generic;
using ConnectApp.Models.Model;

namespace ConnectApp.Models.ViewModel {
    public class ArticlesScreenViewModel {
        public bool articlesLoading;
        public bool followArticlesLoading;
        public bool followingLoading;
        public List<string> recommendArticleIds;
        public List<string> randomRecommendArticleIds;
        public List<string> followArticleIds;
        public List<string> hotArticleIds;
        public List<Following> followings;
        public Dictionary<string, Article> articleDict;
        public Dictionary<string, User> userDict;
        public Dictionary<string, UserLicense> userLicenseDict;
        public Dictionary<string, Team> teamDict;
        public Dictionary<string, bool> likeMap;
        public Dictionary<string, bool> followMap;
        public Dictionary<string, FavoriteTag> favoriteTagDict;
        public Dictionary<string, FavoriteTagArticle> favoriteTagArticleDict;
        public Dictionary<string, RankData> rankDict;
        public List<string> blockArticleList;
        public List<string> homeSliderIds;
        public List<string> homeCollectionIds;
        public List<string> homeBloggerIds;
        public string searchSuggest;
        public bool hottestHasMore;
        public bool followArticleHasMore;
        public bool hotArticleHasMore;
        public int hottestOffset;
        public int hotArticlePage;
        public bool isLoggedIn;
        public string currentUserId;
        public bool feedHasNew;
        public string beforeTime;
        public string afterTime;
        public int currentTabBarIndex;
        public int currentHomePageTabIndex;
        public bool swiperOnScreen;
    }
}