using System;
using System.Collections.Generic;
using ConnectApp.Common.Other;
using ConnectApp.Common.Util;
using ConnectApp.Models.Model;

namespace ConnectApp.Models.State {
    [Serializable]
    public class AppState {
        public LoginState loginState { get; set; }
        public ArticleState articleState { get; set; }
        public PopularSearchState popularSearchState { get; set; }
        public SearchState searchState { get; set; }
        public NotificationState notificationState { get; set; }
        public UserState userState { get; set; }
        public TeamState teamState { get; set; }
        public PlaceState placeState { get; set; }
        public FollowState followState { get; set; }
        public LikeState likeState { get; set; }
        public MineState mineState { get; set; }
        public MessageState messageState { get; set; }
        public SettingState settingState { get; set; }
        public ReportState reportState { get; set; }
        public FeedbackState feedbackState { get; set; }
        public TabBarState tabBarState { get; set; }
        public FavoriteState favoriteState { get; set; }
        public LeaderBoardState leaderBoardState { get; set; }
        public TagState tagState { get; set; }
        public QAState qaState { get; set; }
        public QAEditorState qaEditorState { get; set; }
        public LearnState learnState { get; set; }
        
        public static AppState initialState() {
            var loginInfo = UserInfoManager.getUserInfo();
            var isLogin = UserInfoManager.isLoggedIn();

            return new AppState {
                loginState = new LoginState {
                    email = "",
                    password = "",
                    loginInfo = loginInfo,
                    isLoggedIn = isLogin,
                    newNotifications = isLogin
                        ? NewNotificationManager.getNewNotification(userId: loginInfo.userId)
                        : null
                },
                articleState = new ArticleState {
                    recommendArticleIds = new List<string>(),
                    randomRecommendArticleIds = new List<string>(),
                    followArticleIdDict = new Dictionary<string, List<string>>(),
                    hotArticleIdDict = new Dictionary<string, List<string>>(),
                    articleDict = new Dictionary<string, Article>(),
                    userArticleDict = new Dictionary<string, UserArticle>(),
                    articlesLoading = false,
                    followArticlesLoading = false,
                    articleDetailLoading = false,
                    hottestHasMore = true,
                    feedHasNew = false,
                    feedIsFirst = false,
                    followArticleHasMore = false,
                    hotArticleHasMore = false,
                    hotArticlePage = 0,
                    beforeTime = "",
                    afterTime = "",
                    articleHistory = HistoryManager.articleHistoryList(isLogin ? loginInfo.userId : null),
                    blockArticleList = HistoryManager.blockArticleList(isLogin ? loginInfo.userId : null),
                    homeSliderIds = new List<string>(),
                    homeTopCollectionIds = new List<string>(),
                    homeCollectionIds = new List<string>(),
                    homeBloggerIds = new List<string>(),
                    recommendLastRefreshArticleId = "",
                    recommendHasNewArticle = true,
                    swiperOnScreen = true
                },
                popularSearchState = new PopularSearchState {
                    popularSearchArticles = new List<PopularSearch>(),
                    popularSearchUsers = new List<PopularSearch>()
                },
                searchState = new SearchState {
                    searchArticleLoading = false,
                    searchUserLoading = false,
                    searchTeamLoading = false,
                    searchQuestionLoading = false,
                    searchFollowingLoading = false,
                    keyword = "",
                    searchFollowingKeyword = "",
                    searchArticleIdDict = new Dictionary<string, List<string>>(),
                    searchUserIdDict = new Dictionary<string, List<string>>(),
                    searchTeamIdDict = new Dictionary<string, List<string>>(),
                    searchQuestionIdsDict = new Dictionary<string, List<string>>(),
                    searchFollowings = new List<User>(),
                    searchArticleHasMore = false,
                    searchUserHasMore = false,
                    searchTeamHasMore = false,
                    searchQuestionHasMore = false,
                    searchFollowingHasMore = false,
                    searchArticleHistoryList =
                        HistoryManager.searchArticleHistoryList(isLogin ? loginInfo.userId : null),
                    searchTagList = new List<string>()
                },
                notificationState = new NotificationState {
                    allNotificationState = new NotificationCategoryState {
                        loading = false,
                        page = 1,
                        pageTotal = 1,
                        notifications = new List<Notification>(),
                        mentions = new List<User>()
                    },
                    followNotificationState = new NotificationCategoryState {
                        loading = false,
                        page = 1,
                        pageTotal = 1,
                        notifications = new List<Notification>(),
                        mentions = new List<User>()
                    },
                    involveNotificationState = new NotificationCategoryState {
                        loading = false,
                        page = 1,
                        pageTotal = 1,
                        notifications = new List<Notification>(),
                        mentions = new List<User>()
                    },
                    participateNotificationState = new NotificationCategoryState {
                        loading = false,
                        page = 1,
                        pageTotal = 1,
                        notifications = new List<Notification>(),
                        mentions = new List<User>()
                    },
                    systemNotificationState = new NotificationCategoryState {
                        loading = false,
                        page = 1,
                        pageTotal = 1,
                        notifications = new List<Notification>(),
                        mentions = new List<User>()
                    }
                },
                userState = new UserState {
                    userLoading = false,
                    userArticleLoading = false,
                    userLikeArticleLoading = false,
                    followingLoading = false,
                    followingUserLoading = false,
                    followingTeamLoading = false,
                    followerLoading = false,
                    userDict = UserInfoManager.getUserInfoDict(),
                    slugDict = new Dictionary<string, string>(),
                    userLicenseDict = new Dictionary<string, UserLicense>(),
                    fullName = "",
                    title = "",
                    jobRole = new JobRole(),
                    place = "",
                    blockUserIdSet = HistoryManager.blockUserIdSet(isLogin ? loginInfo.userId : null)
                },
                teamState = new TeamState {
                    teamLoading = false,
                    teamArticleLoading = false,
                    followerLoading = false,
                    memberLoading = false,
                    teamDict = new Dictionary<string, Team>(),
                    slugDict = new Dictionary<string, string>()
                },
                placeState = new PlaceState {
                    placeDict = new Dictionary<string, Place>()
                },
                followState = new FollowState {
                    followDict = new Dictionary<string, Dictionary<string, bool>>()
                },
                likeState = new LikeState {
                    likeDict = new Dictionary<string, Dictionary<string, bool>>()
                },
                mineState = new MineState {
                    futureEventIds = new List<string>(),
                    pastEventIds = new List<string>(),
                    futureListLoading = false,
                    pastListLoading = false,
                    futureEventTotal = 0,
                    pastEventTotal = 0
                },
                messageState = new MessageState {
                    channelMessageDict = new Dictionary<string, Dictionary<string, Message>>(),
                    channelMessageList = new Dictionary<string, List<string>>()
                },
                settingState = new SettingState {
                    hasReviewUrl = false,
                    vibrate = true,
                    reviewUrl = ""
                },
                reportState = new ReportState(),
                feedbackState = new FeedbackState {
                    feedbackType = FeedbackType.Advice
                },
                tabBarState = new TabBarState {
                    currentTabIndex = 0,
                    currentHomePageTabIndex = 1,
                },
                favoriteState = new FavoriteState {
                    favoriteTagLoading = false,
                    followFavoriteTagLoading = false,
                    favoriteDetailLoading = false,
                    favoriteTagIdDict = new Dictionary<string, List<string>>(),
                    followFavoriteTagIdDict = new Dictionary<string, List<string>>(),
                    favoriteDetailArticleIdDict = new Dictionary<string, List<string>>(),
                    favoriteTagHasMore = false,
                    followFavoriteTagHasMore = false,
                    favoriteDetailHasMore = false,
                    favoriteTagDict = new Dictionary<string, FavoriteTag>(),
                    favoriteTagArticleDict = new Dictionary<string, FavoriteTagArticle>(),
                    collectedTagMap = new Dictionary<string, Dictionary<string, bool>>(),
                    collectedTagChangeMap = new Dictionary<string, string>()
                },
                leaderBoardState = new LeaderBoardState {
                    collectionLoading = false,
                    columnLoading = false,
                    bloggerLoading = false,
                    homeBloggerLoading = false,
                    collectionIds = new List<string>(),
                    columnIds = new List<string>(),
                    bloggerIds = new List<string>(),
                    homeBloggerIds = new List<string>(),
                    collectionHasMore = false,
                    columnHasMore = false,
                    bloggerHasMore = false,
                    homeBloggerHasMore = false,
                    collectionPageNumber = 1,
                    columnPageNumber = 1,
                    bloggerPageNumber = 1,
                    homeBloggerPageNumber = 1,
                    rankDict = new Dictionary<string, RankData>(),
                    detailLoading = false,
                    detailHasMore = false,
                    columnDict = new Dictionary<string, List<string>>(),
                    collectionDict = new Dictionary<string, List<string>>(),
                    detailCollectLoading = false
                },
                tagState = new TagState {
                    tagDict = new Dictionary<string, Tag>()
                },
                qaState = new QAState {
                    questionDict = new Dictionary<string, Question>(),
                    latestQuestionListHasMore = false,
                    latestQuestionIds = new List<string>(),
                    hotQuestionListHasMore = false,
                    hotQuestionIds = new List<string>(),
                    pendingQuestionListHasMore = false,
                    pendingQuestionIds = new List<string>(),
                    answerListHasMoreDict = new Dictionary<string, bool>(),
                    answerIdsDict = new Dictionary<string, List<string>>(),
                    answerDict = new Dictionary<string, Answer>(),
                    imageDict = new Dictionary<string, string>(),
                    messageToplevelSimpleListDict = new Dictionary<string, List<string>>(),
                    messageToplevelListDict = new Dictionary<string, NewMessageList>(),
                    messageSecondLevelSimpleListDict = new Dictionary<string, NewMessageList>(),
                    messageSecondLevelListDict = new Dictionary<string, NewMessageList>(),
                    messageDict = new Dictionary<string, NewMessage>(),
                    likeDict = new Dictionary<string, QALike>(),
                    nextAnswerIdDict = new Dictionary<string, string>(),
                    userQuestionsDict = new Dictionary<string, List<string>>(),
                    userQuestionListHasMore = false,
                    userAnswersDict = new Dictionary<string, List<string>>(),
                    userAnswerListHasMore = false,
                    blockQuestionList = HistoryManager.blockQuestionList(isLogin ? loginInfo.userId : null),
                    blockAnswerList = HistoryManager.blockAnswerList(isLogin ? loginInfo.userId : null)
                },
                qaEditorState = new QAEditorState {
                    createQuestionDraftLoading = false,
                    fetchQuestionDraftLoading = false,
                    questionDraft = null,
                    answerDraftDict = new Dictionary<string, AnswerDraft>(),
                    createAnswerDraftLoading = false,
                    fetchAnswerDraftLoading = false,
                    canAnswerDict = new Dictionary<string, bool>(),
                    userAnswerDraftIds = new List<string>(),
                    userAnswerDraftListHasMore = false,
                    uploadImageDict = new Dictionary<string, string>(),
                    plates = new List<Plate>()
                },
                learnState = new LearnState {
                    courses = new List<LearnCourse>(),
                    hasMore = false
                }
            };
        }
    }
}