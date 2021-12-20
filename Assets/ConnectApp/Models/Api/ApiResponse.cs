using System;
using System.Collections.Generic;
using ConnectApp.Models.Model;

namespace ConnectApp.Models.Api {
    [Serializable]
    public class FetchArticlesResponse {
        public List<HottestItem> hottests;
        public Dictionary<string, Article> projectMap;
        public Dictionary<string, User> userMap;
        public Dictionary<string, Team> teamMap;
        public Dictionary<string, bool> followMap;
        public Dictionary<string, bool> likeMap;
        public bool hottestHasMore;
        public bool feedHasNew;
        public HomeRankData rankData;
    }

    [Serializable]
    public class FetchFollowArticlesResponse {
        public Dictionary<string, User> userMap;
        public Dictionary<string, UserLicense> userLicenseMap;
        public Dictionary<string, Team> teamMap;
        public Dictionary<string, bool> followMap;
        public Dictionary<string, bool> likeMap;
        public Dictionary<string, Article> projectSimpleMap;
        public List<Feed> feeds;
        public bool feedHasNew;
        public bool feedIsFirst;
        public bool feedHasMore;
        public List<HottestItem> hotItems;
        public bool hotHasMore;
        public int hotPage;
    }

    [Serializable]
    public class FetchArticleDetailResponse {
        public Project project;
    }
    
    [Serializable]
    public class FetchLearnCourseListResponse {
        public List<LearnCourse> results;
        public bool hasMore;
    }

    

    [Serializable]
    public class FetchNotificationResponse {
        public int page;
        public int pageTotal;
        public List<Notification> results;
        public Dictionary<string, User> userMap;
    }

    [Serializable]
    public class FetchSearchArticleResponse {
        public bool hasMore;
        public List<Article> projects;
        public Dictionary<string, User> userMap;

        public Dictionary<string, Team> teamMap;

        // public Dictionary<string, Place> placeMap;
        public Dictionary<string, bool> likeMap;
    }

    [Serializable]
    public class FetchSearchUserResponse {
        public bool hasMore;
        public List<User> users;
        public Dictionary<string, UserLicense> userLicenseMap;
        public Dictionary<string, bool> followingMap;
    }

    [Serializable]
    public class FetchSearchTeamResponse {
        public bool hasMore;
        public List<Team> teams;
        public Dictionary<string, bool> followingMap;
    }

    [Serializable]
    public class SearchQuestionsResponse {
        public bool hasMore;
        public int currentPage;
        public List<Question> simpleQuestions;
        public Dictionary<string, Tag> tagSimpleMap;
    }

    [Serializable]
    public class SearchTagsResponse {
        public List<Tag> candidates;
    }
    
    [Serializable]
    public class FetchSendMessageResponse
    {
        public string channelId;
        public string content;
        public string nonce;
    }

    [Serializable]
    public class FetchCommentsResponse {
        public List<Message> items;
        public List<Message> parents;
        public List<Message> uppers;
        public Dictionary<string, UserLicense> userLicenseMap;
        public string currOldestMessageId;
        public bool hasMore;
        public bool hasMoreNew;
    }

    [Serializable]
    public class FetchUserProfileResponse {
        public User user;
        public Dictionary<string, bool> followMap;
        public int followingCount;
        public bool followingsHasMore;
        public bool followersHasMore;
        public int followingTeamsCount;
        public List<Team> followingTeams;
        public bool followingTeamsHasMore;
        public string currentUserId;
        public Dictionary<string, Team> teamMap;
        public Dictionary<string, Place> placeMap;
        public Dictionary<string, JobRole> jobRoleMap;
        public Dictionary<string, UserLicense> userLicenseMap;
    }

    [Serializable]
    public class FetchUserArticleResponse {
        public List<string> projectList;
        public Dictionary<string, User> userMap;
        public Dictionary<string, Team> teamMap;
        public Dictionary<string, Place> placeMap;
        public Dictionary<string, bool> likeMap;
        public Dictionary<string, bool> followMap;
        public Dictionary<string, Article> projectMap;
        public bool hasMore;
    }

    [Serializable]
    public class FetchUserLikeArticleResponse {
        public List<Article> projectSimpleList;
        public Dictionary<string, User> userSimpleV2Map;
        public Dictionary<string, Team> teamSimpleMap;
        public int currentPage;
    }

    [Serializable]
    public class FetchFollowingResponse {
        public List<Following> followings;
        public bool hasMore;
        public Dictionary<string, bool> followMap;
        public Dictionary<string, User> userMap;
        public Dictionary<string, Team> teamMap;
    }

    [Serializable]
    public class FetchFollowingUserResponse {
        public List<User> followings;
        public Dictionary<string, UserLicense> userLicenseMap;
        public bool followingsHasMore;
        public Dictionary<string, bool> followMap;
    }

    [Serializable]
    public class FetchFollowerResponse {
        public List<User> followers;
        public Dictionary<string, UserLicense> userLicenseMap;
        public bool followersHasMore;
        public Dictionary<string, bool> followMap;
    }

    [Serializable]
    public class FetchFollowingTeamResponse {
        public List<Team> followingTeams;
        public bool followingTeamsHasMore;
        public Dictionary<string, bool> followMap;
        public Dictionary<string, Place> placeMap;
    }

    [Serializable]
    public class FetchEditPersonalInfoResponse {
        public User user;
        public Dictionary<string, Place> placeMap;
    }

    [Serializable]
    public class FetchTeamResponse {
        public Team team;
        public Dictionary<string, Place> placeMap;
        public Dictionary<string, bool> followMap;
    }

    [Serializable]
    public class FetchTeamArticleResponse {
        public Dictionary<string, bool> likeMap;
        public List<Article> projects;
        public bool projectsHasMore;
    }

    [Serializable]
    public class FetchTeamMemberResponse {
        public List<TeamMember> members;
        public Dictionary<string, User> userMap;
        public Dictionary<string, bool> followMap;
        public bool hasMore;
    }

    [Serializable]
    public class ErrorResponse {
        public string errorCode;
    }

    [Serializable]
    public class FetchInitDataResponse {
        public string VS;
        public ServerConfig config;
        public bool isRealName;
    }

    [Serializable]
    public class UpdateAvatarResponse {
        public string avatar;
    }

    [Serializable]
    public class FetchFavoriteTagsResponse {
        public List<FavoriteTag> favoriteTags;
        public Dictionary<string, bool> collectedMap;
        public Dictionary<string, FavoriteTag> myFavoriteTagMap;
        public bool hasMore;
    }

    [Serializable]
    public class FetchFavoriteDetailResponse {
        public Dictionary<string, User> userMap;
        public Dictionary<string, Team> teamMap;
        public Dictionary<string, FavoriteTag> tagMap;
        public Dictionary<string, Article> projectSimpleMap;
        public List<Favorite> favorites;
        public bool hasMore;
    }

    [Serializable]
    public class CollectFavoriteTagResponse {
        public FavoriteTag favoriteTag;
    }

    [Serializable]
    public class CheckNewVersionResponse {
        public string platform;
        public string store;
        public string versionName;
        public string versionCode;
        public string forceUpdateVersionCode;
        public string status;
        public string url;
        public string changeLog;
    }

    [Serializable]
    public class FetchLeaderBoardCollectionResponse {
        public List<RankData> rankList;
        public Dictionary<string, FavoriteTagArticle> favoriteTagArticleMap;
        public Dictionary<string, FavoriteTag> favoriteTagMap;
        public Dictionary<string, bool> collectedTagMap;
        public bool hasMore;
        public int currentPage;
    }

    [Serializable]
    public class FetchLeaderBoardColumnResponse {
        public List<RankData> rankList;
        public Dictionary<string, UserArticle> userArticleMap;
        public Dictionary<string, User> userSimpleV2Map;
        public bool hasMore;
        public int currentPage;
    }

    [Serializable]
    public class FetchBloggerResponse {
        public List<RankData> rankList;
        public Dictionary<string, User> userFullMap;
        public Dictionary<string, bool> followMap;
        public Dictionary<string, UserLicense> userLicenseMap;
        public bool hasMore;
        public int currentPage;
    }

    [Serializable]
    public class FetchLeaderBoardDetailResponse {
        public List<Article> projectSimples;
        public Dictionary<string, User> userSimpleV2Map;
        public Dictionary<string, Team> teamSimpleMap;
        public Dictionary<string, FavoriteTagArticle> favoriteTagArticleMap;
        public Dictionary<string, FavoriteTag> favoriteTagMap;
        public FavoriteTag myFavoriteTag;
        public Dictionary<string, bool> collectedTagMap;
        public bool hasMore;
        public int currentPage;
        public RankData rankData;
    }

    [Serializable]
    public class FetchGamesResponse {
        public List<GameData> rankList;
        public bool hasMore;
        public int currentPage;
    }

    [Serializable]
    public class FetchQuestionsResponse {
        public int currentPage;
        public bool hasMore;
        public List<Question> questionSimpleList;
        public Dictionary<string, Tag> tagSimpleMap;
    }

    [Serializable]
    public class FetchQuestionDetailResponse {
        public Question question;
        public Dictionary<string, ContentData> contentMap;
        public Dictionary<string, UserLicense> userLicenseMap;
        public Dictionary<string, User> userSimpleV2Map;
        public Dictionary<string, Tag> tagSimpleMap;
        public Dictionary<string, QALike> likeItemMap;
        public List<Answer> answerSimpleList;
        public bool canAnswer;
    }

    [Serializable]
    public class FetchAnswersResponse {
        public int currentPage;
        public bool hasMore;
        public Dictionary<string, ContentData> contentMap;
        public Dictionary<string, UserLicense> userLicenseMap;
        public Dictionary<string, User> userSimpleV2Map;
        public List<Answer> answerSimpleList;
    }

    [Serializable]
    public class FetchAnswerDetailResponse {
        public Answer answer;
        public string nextAnswerId;
        public Dictionary<string, ContentData> contentMap;
        public List<NewMessage> messages;
        public Dictionary<string, UserLicense> userLicenseMap;
        public Dictionary<string, User> userSimpleV2Map;
        public Question questionSimple;
        public Dictionary<string, QALike> likeItemMap;
        public Dictionary<string, bool> followMap;
        public bool canAnswer;
    }

    [Serializable]
    public class FetchTopLevelMessageResponse {
        public bool hasMore;
        public List<string> messageIdList;
        public Dictionary<string, NewMessage> messageMap;
        public Dictionary<string, ChildNewMessage> childMessageMap;
        public Dictionary<string, UserLicense> userLicenseMap;
        public Dictionary<string, User> userSimpleV2Map;
        public Dictionary<string, QALike> likeItemMap;
    }

    [Serializable]
    public class FetchSecondLevelMessageResponse {
        public NewMessage currentMessage;
        public bool hasMore;
        public List<string> childMessageList;
        public Dictionary<string, NewMessage> allMessageMap;
        public Dictionary<string, UserLicense> userLicenseMap;
        public Dictionary<string, User> userSimpleV2Map;
        public Dictionary<string, QALike> likeItemMap;
    }

    [Serializable]
    public class QuestionDraftResponse {
        public Question questionData;
        public Dictionary<string, ContentData> contentMap;
        public Dictionary<string, Tag> tagSimpleMap;
        public Dictionary<string, Plate> plateSimpleMap;
    }

    [Serializable]
    public class AnswerDraftResponse {
        public Answer answerData;
        public Dictionary<string, ContentData> contentMap;
    }

    [Serializable]
    public class UpdateQuestionTagsResponse {
        public List<string> skillIds;
        public List<Tag> skills;
    }

    [Serializable]
    public class CheckRealNameResponse {
        public bool success;
        public string description = "";
    }

    [Serializable]
    public class UserQuestionResponse {
        public bool hasMore;
        public List<Question> questions;
        public int currentPage;
    }

    [Serializable]
    public class UserAnswerResponse {
        public bool hasMore;
        public List<Answer> answers;
        public Dictionary<string, Question> questionSimpleMap;
        public int currentPage;
    }

    [Serializable]
    public class UserAnswerDraftResponse {
        public bool hasMore;
        public List<Answer> answers;
        public Dictionary<string, Question> questionSimpleMap;
        public int currentPage;
    }
}