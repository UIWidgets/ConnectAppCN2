using System.Collections.Generic;
using System.Linq;
using ConnectApp.Common.Other;
using ConnectApp.Common.Util;
using ConnectApp.Models.Model;
using ConnectApp.Models.State;
using ConnectApp.Plugins;
using ConnectApp.redux.actions;
using ConnectApp.screens;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.service;
using UnityEngine;

namespace ConnectApp.redux.reducers {
    public static class AppReducer {
        static readonly List<string> _nonce = new List<string>();

        public static AppState Reduce(AppState state, object bAction) {
            switch (bAction) {
                case LoginChangeEmailAction action: {
                    state.loginState.email = action.changeText;
                    break;
                }

                case LoginChangePasswordAction action: {
                    state.loginState.password = action.changeText;
                    break;
                }

                case LoginByEmailSuccessAction action: {
                    state.loginState.loginInfo = action.loginInfo;
                    state.loginState.isLoggedIn = true;
                    state.loginState.newNotifications =
                        NewNotificationManager.getNewNotification(userId: state.loginState.loginInfo.userId);
                    state.articleState.feedHasNew = true;
                    state.articleState.articleHistory =
                        HistoryManager.articleHistoryList(userId: action.loginInfo.userId);
                    state.searchState.searchArticleHistoryList =
                        HistoryManager.searchArticleHistoryList(userId: action.loginInfo.userId);
                    state.articleState.blockArticleList =
                        HistoryManager.blockArticleList(userId: action.loginInfo.userId);
                    state.userState.blockUserIdSet =
                        HistoryManager.blockUserIdSet(currentUserId: action.loginInfo.userId);
                    state.qaState.blockQuestionList = HistoryManager.blockQuestionList(userId: action.loginInfo.userId);
                    state.qaState.blockAnswerList = HistoryManager.blockAnswerList(userId: action.loginInfo.userId);
                    break;
                }

                case LoginByWechatSuccessAction action: {
                    state.loginState.loginInfo = action.loginInfo;
                    state.loginState.isLoggedIn = true;
                    state.loginState.newNotifications =
                        NewNotificationManager.getNewNotification(userId: state.loginState.loginInfo.userId);
                    state.articleState.feedHasNew = true;
                    state.articleState.articleHistory =
                        HistoryManager.articleHistoryList(userId: action.loginInfo.userId);
                    state.searchState.searchArticleHistoryList =
                        HistoryManager.searchArticleHistoryList(userId: action.loginInfo.userId);
                    state.articleState.blockArticleList =
                        HistoryManager.blockArticleList(userId: action.loginInfo.userId);
                    state.userState.blockUserIdSet =
                        HistoryManager.blockUserIdSet(currentUserId: action.loginInfo.userId);
                    state.qaState.blockQuestionList = HistoryManager.blockQuestionList(userId: action.loginInfo.userId);
                    state.qaState.blockAnswerList = HistoryManager.blockAnswerList(userId: action.loginInfo.userId);
                    break;
                }

                case LogoutAction _: {
                    EventBus.publish(sName: EventBusConstant.logout_success, args: new List<object>());
                    HistoryManager.deleteHomeAfterTime(userId: state.loginState.loginInfo.userId);
                    HttpManager.clearCookie();
                    state.loginState.loginInfo = new LoginInfo();
                    state.loginState.isLoggedIn = false;
                    state.loginState.newNotifications = null;
                    UserInfoManager.clearUserInfo();
                    state.articleState.articleHistory = HistoryManager.articleHistoryList();
                    state.searchState.searchArticleHistoryList = HistoryManager.searchArticleHistoryList();
                    state.articleState.blockArticleList = HistoryManager.blockArticleList();
                    state.qaState.blockQuestionList = HistoryManager.blockQuestionList();
                    state.qaState.blockAnswerList = HistoryManager.blockAnswerList();
                    state.userState.blockUserIdSet = HistoryManager.blockUserIdSet();
                    state.favoriteState.favoriteTagIdDict = new Dictionary<string, List<string>>();
                    state.favoriteState.favoriteTagDict = new Dictionary<string, FavoriteTag>();
                    state.favoriteState.favoriteDetailArticleIdDict = new Dictionary<string, List<string>>();
                    state.qaState.likeDict.Clear();
                    break;
                }

                case CleanEmailAndPasswordAction _: {
                    state.loginState.email = "";
                    state.loginState.password = "";
                    break;
                }

                case StartFetchArticlesAction _: {
                    state.articleState.articlesLoading = true;
                    break;
                }

                case FetchArticleSuccessAction action: {
                    if (action.offset == 0) {
                        state.articleState.homeSliderIds = action.homeSliderIds;
                        state.articleState.homeTopCollectionIds = action.homeTopCollectionIds;
                        state.articleState.homeCollectionIds = action.homeCollectionIds;
                        state.articleState.homeBloggerIds = action.homeBloggerIds;
                        state.articleState.searchSuggest = action.searchSuggest;
                        state.articleState.dailySelectionId = action.dailySelectionId;
                        state.articleState.leaderBoardUpdatedTime = action.leaderBoardUpdatedTime;
                        state.articleState.recommendArticleIds.Clear();
                    }

                    if (action.isRandomItem) {
                        state.articleState.randomRecommendArticleIds.Clear();
                        foreach (var article in action.articleList) {
                            if (!state.articleState.randomRecommendArticleIds.Contains(item: article.id)) {
                                state.articleState.randomRecommendArticleIds.Add(item: article.id);
                            }

                            if (!state.articleState.articleDict.ContainsKey(key: article.id)) {
                                state.articleState.articleDict.Add(key: article.id, value: article);
                            }
                            else {
                                var oldArticle = state.articleState.articleDict[key: article.id];
                                state.articleState.articleDict[key: article.id] = oldArticle.Merge(other: article);
                            }
                        }
                    }
                    else {
                        foreach (var article in action.articleList) {
                            if (!state.articleState.recommendArticleIds.Contains(item: article.id)) {
                                state.articleState.recommendArticleIds.Add(item: article.id);
                            }

                            if (!state.articleState.articleDict.ContainsKey(key: article.id)) {
                                state.articleState.articleDict.Add(key: article.id, value: article);
                            }
                            else {
                                var oldArticle = state.articleState.articleDict[key: article.id];
                                state.articleState.articleDict[key: article.id] = oldArticle.Merge(other: article);
                            }
                        }
                    }

                    if (action.offset == 0) {
                        state.articleState.recommendHasNewArticle =
                            state.articleState.recommendLastRefreshArticleId.isEmpty() ||
                            state.articleState.recommendLastRefreshArticleId.isNotEmpty() &&
                            state.articleState.recommendLastRefreshArticleId !=
                            state.articleState.recommendArticleIds.first();
                        state.articleState.recommendLastRefreshArticleId =
                            state.articleState.recommendArticleIds.first();
                    }

                    state.articleState.feedHasNew = action.feedHasNew;
                    state.articleState.hottestHasMore = action.hottestHasMore;
                    state.articleState.articlesLoading = false;
                    break;
                }

                case FetchArticleFailureAction _: {
                    state.articleState.articlesLoading = false;
                    break;
                }

                case StartFetchFollowArticlesAction _: {
                    state.articleState.followArticlesLoading = true;
                    break;
                }

                case FetchFollowArticleSuccessAction action: {
                    var currentUserId = state.loginState.loginInfo.userId ?? "";
                    if (currentUserId.isNotEmpty()) {
                        if (action.feeds != null && action.feeds.Count > 0) {
                            var followArticleIds = new List<string>();
                            action.feeds.ForEach(feed => {
                                if (feed.itemIds != null && feed.itemIds.Count > 0) {
                                    followArticleIds.Add(feed.itemIds[0]);
                                }
                            });
                            if (state.articleState.followArticleIdDict.ContainsKey(key: currentUserId)) {
                                if (action.pageNumber == 1) {
                                    state.articleState.beforeTime = action.feeds.last().actionTime;
                                    state.articleState.afterTime = action.feeds.first().actionTime;
                                    if (state.loginState.isLoggedIn) {
                                        HistoryManager.saveHomeAfterTime(afterTime: state.articleState.afterTime,
                                            userId: state.loginState.loginInfo.userId);
                                    }

                                    state.articleState.followArticleIdDict[key: currentUserId] = followArticleIds;
                                }
                                else {
                                    state.articleState.beforeTime = action.feeds.last().actionTime;
                                    var projectIds = state.articleState.followArticleIdDict[key: currentUserId];
                                    projectIds.AddRange(collection: followArticleIds);
                                    state.articleState.followArticleIdDict[key: currentUserId] = projectIds;
                                }
                            }
                            else {
                                state.articleState.beforeTime = action.feeds.last().actionTime;
                                state.articleState.afterTime = action.feeds.first().actionTime;
                                if (state.loginState.isLoggedIn) {
                                    HistoryManager.saveHomeAfterTime(afterTime: state.articleState.afterTime,
                                        userId: state.loginState.loginInfo.userId);
                                }

                                state.articleState.followArticleIdDict.Add(key: currentUserId, value: followArticleIds);
                            }
                        }

                        var hotArticleIds = new List<string>();
                        foreach (var hotItem in action.hotItems) {
                            hotArticleIds.Add(item: hotItem.itemId);
                        }

                        if (state.articleState.hotArticleIdDict.ContainsKey(key: currentUserId)) {
                            if (action.pageNumber == 1) {
                                state.articleState.hotArticleIdDict[key: currentUserId] = hotArticleIds;
                            }
                            else {
                                var hotIds = state.articleState.hotArticleIdDict[key: currentUserId];
                                hotIds.AddRange(collection: hotArticleIds);
                                state.articleState.hotArticleIdDict[key: currentUserId] = hotIds;
                            }
                        }
                        else {
                            state.articleState.hotArticleIdDict.Add(key: currentUserId, value: hotArticleIds);
                        }

                        state.articleState.feedHasNew = action.feedHasNew;
                        state.articleState.feedIsFirst = action.feedIsFirst;
                        state.articleState.followArticleHasMore = action.feedHasMore;
                        state.articleState.hotArticleHasMore = action.hotHasMore;
                        state.articleState.hotArticlePage = action.hotPage;
                    }

                    state.articleState.followArticlesLoading = false;
                    break;
                }

                case FetchFollowArticleFailureAction _: {
                    state.articleState.followArticlesLoading = false;
                    break;
                }

                case StartFetchArticleDetailAction _: {
                    state.articleState.articleDetailLoading = true;
                    break;
                }

                case FetchArticleDetailSuccessAction action: {
                    state.articleState.articleDetailLoading = false;
                    var relatedArticles = action.articleDetail.projects.FindAll(item => item.type == "article");
                    var projectIds = new List<string>();
                    relatedArticles.ForEach(project => {
                        projectIds.Add(item: project.id);
                        if (!state.articleState.articleDict.ContainsKey(key: project.id)) {
                            state.articleState.articleDict.Add(key: project.id, value: project);
                        }
                        else {
                            var oldArticle = state.articleState.articleDict[key: project.id];
                            state.articleState.articleDict[key: project.id] = oldArticle.Merge(other: project);
                        }
                    });
                    var article = action.articleDetail.projectData;
                    article.like = action.articleDetail.like;
                    article.appCurrentUserLikeCount = action.articleDetail.appCurrentUserLikeCount;
                    article.projectIds = projectIds;
                    article.channelId = action.articleDetail.channelId;
                    article.contentMap = action.articleDetail.contentMap;
                    article.hasMore = action.articleDetail.comments.hasMore;
                    article.favorites = action.articleDetail.favoriteList;
                    article.isNotFirst = true;
                    article.currOldestMessageId = action.articleDetail.comments.currOldestMessageId;
                    article.videoSliceMap = action.articleDetail.videoSliceMap;
                    article.videoPosterMap = action.articleDetail.videoPosterMap;
                    var dict = state.articleState.articleDict;
                    if (dict.ContainsKey(key: article.id)) {
                        state.articleState.articleDict[key: article.id] = article;
                    }
                    else {
                        state.articleState.articleDict.Add(key: article.id, value: article);
                    }

                    if (!article.id.Equals(value: action.articleId)) {
                        if (dict.ContainsKey(key: action.articleId)) {
                            state.articleState.articleDict[key: action.articleId] = article;
                        }
                        else {
                            state.articleState.articleDict.Add(key: action.articleId, value: article);
                        }
                    }

                    break;
                }

                case FetchArticleDetailFailureAction _: {
                    state.articleState.articleDetailLoading = false;
                    break;
                }

                case SaveArticleHistoryAction action: {
                    var article = action.article;
                    var fullName = "";
                    if (article.ownerType == "user") {
                        var userDict = state.userState.userDict;
                        if (article.userId != null && userDict.ContainsKey(key: article.userId)) {
                            fullName = userDict[key: article.userId].fullName;
                        }
                    }

                    if (article.ownerType == "team") {
                        var teamDict = state.teamState.teamDict;
                        if (article.teamId != null && teamDict.ContainsKey(key: article.teamId)) {
                            fullName = teamDict[key: article.teamId].name;
                        }
                    }

                    var simplyArticle = new Article {
                        id = article.id,
                        slug = article.slug,
                        fullName = fullName,
                        title = article.title,
                        subTitle = article.subTitle,
                        publishedTime = article.publishedTime,
                        thumbnail = article.thumbnail,
                        viewCount = article.viewCount
                    };

                    var articleHistoryList = HistoryManager.saveArticleHistory(article: simplyArticle,
                        state.loginState.isLoggedIn ? state.loginState.loginInfo.userId : null);
                    state.articleState.articleHistory = articleHistoryList;
                    break;
                }

                case DeleteArticleHistoryAction action: {
                    var articleHistoryList = HistoryManager.deleteArticleHistory(articleId: action.articleId,
                        state.loginState.isLoggedIn ? state.loginState.loginInfo.userId : null);
                    state.articleState.articleHistory = articleHistoryList;
                    break;
                }

                case DeleteAllArticleHistoryAction _: {
                    state.articleState.articleHistory = new List<Article>();
                    HistoryManager.deleteAllArticleHistory(state.loginState.isLoggedIn
                        ? state.loginState.loginInfo.userId
                        : null);
                    break;
                }

                case LikeArticleSuccessAction action: {
                    if (state.articleState.articleDict.ContainsKey(key: action.articleId)) {
                        var article = state.articleState.articleDict[key: action.articleId];
                        article.like = true;
                        article.appLikeCount += action.likeCount;
                        article.appCurrentUserLikeCount += action.likeCount;
                        state.articleState.articleDict[key: action.articleId] = article;
                    }

                    var currentUserId = state.loginState.loginInfo.userId ?? "";
                    if (currentUserId.isNotEmpty()) {
                        var likeMap = new Dictionary<string, bool>();
                        if (state.likeState.likeDict.ContainsKey(key: currentUserId)) {
                            likeMap = state.likeState.likeDict[key: currentUserId];
                        }

                        if (!likeMap.ContainsKey(key: action.articleId)) {
                            likeMap.Add(key: action.articleId, true);
                        }

                        state.likeState.likeDict[key: currentUserId] = likeMap;
                    }

                    break;
                }

                case FavoriteArticleSuccessAction action: {
                    if (state.articleState.articleDict.ContainsKey(key: action.articleId)) {
                        var article = state.articleState.articleDict[key: action.articleId];
                        article.favorites = action.favorites;
                        state.articleState.articleDict[key: action.articleId] = article;
                    }

                    if (action.favorites != null && action.favorites.Count > 0) {
                        action.favorites.ForEach(favorite => {
                            if (state.favoriteState.favoriteTagDict.ContainsKey(key: favorite.tagId)) {
                                var favoriteTag = state.favoriteState.favoriteTagDict[key: favorite.tagId];
                                var statistics = favoriteTag.stasitics ?? new Statistics { count = 0 };
                                statistics.count += 1;
                                favoriteTag.stasitics = statistics;
                                state.favoriteState.favoriteTagDict[key: favorite.tagId] = favoriteTag;
                            }

                            if (state.favoriteState.favoriteDetailArticleIdDict.ContainsKey(key: favorite.tagId)) {
                                var favoriteDetailArticleIds =
                                    state.favoriteState.favoriteDetailArticleIdDict[key: favorite.tagId];
                                favoriteDetailArticleIds.Insert(0, item: action.articleId);
                                state.favoriteState.favoriteDetailArticleIdDict[key: favorite.tagId] =
                                    favoriteDetailArticleIds;
                            }
                        });
                    }

                    break;
                }

                case UnFavoriteArticleSuccessAction action: {
                    if (state.articleState.articleDict.ContainsKey(key: action.articleId)) {
                        var article = state.articleState.articleDict[key: action.articleId];
                        if (article.favorites.Contains(item: action.favorite)) {
                            article.favorites.Remove(item: action.favorite);
                        }

                        state.articleState.articleDict[key: action.articleId] = article;
                    }

                    if (state.favoriteState.favoriteTagDict.ContainsKey(key: action.favorite.tagId)) {
                        var favoriteTag = state.favoriteState.favoriteTagDict[key: action.favorite.tagId];
                        var statistics = favoriteTag.stasitics ?? new Statistics { count = 1 };
                        statistics.count -= 1;
                        favoriteTag.stasitics = statistics;
                        state.favoriteState.favoriteTagDict[key: action.favorite.tagId] = favoriteTag;
                    }

                    if (state.favoriteState.favoriteDetailArticleIdDict.ContainsKey(key: action.favorite.tagId)) {
                        var favoriteDetailArticleIds =
                            state.favoriteState.favoriteDetailArticleIdDict[key: action.favorite.tagId];
                        favoriteDetailArticleIds.Remove(item: action.articleId);
                        state.favoriteState.favoriteDetailArticleIdDict[key: action.favorite.tagId] =
                            favoriteDetailArticleIds;
                    }

                    break;
                }

                case BlockArticleAction action: {
                    var blockArticleList =
                        HistoryManager.saveBlockArticleList(articleId: action.articleId,
                            userId: state.loginState.loginInfo.userId);
                    state.articleState.blockArticleList = blockArticleList;
                    break;
                }

                case BlockQuestionAction action: {
                    var blockQuestionList =
                        HistoryManager.saveBlockQuestionList(questionId: action.questionId,
                            userId: state.loginState.loginInfo.userId);
                    state.qaState.blockQuestionList = blockQuestionList;
                    break;
                }

                case BlockAnswerAction action: {
                    var blockAnswerList =
                        HistoryManager.saveBlockAnswerList(answerId: action.answerId,
                            userId: state.loginState.loginInfo.userId);
                    state.qaState.blockAnswerList = blockAnswerList;
                    break;
                }

                case BlockUserAction action: {
                    state.userState.blockUserIdSet = HistoryManager.updateBlockUserId(blockUserId: action.blockUserId,
                        currentUserId: state.loginState.loginInfo.userId, remove: action.remove);
                    break;
                }

                case FetchArticleCommentsSuccessAction action: {
                    var channelMessageList = new Dictionary<string, List<string>>();
                    var channelMessageDict = new Dictionary<string, Dictionary<string, Message>>();

                    channelMessageList.Add(key: action.channelId, value: action.itemIds);
                    channelMessageDict.Add(key: action.channelId, value: action.messageItems);

                    if (action.channelId.isNotEmpty()) {
                        foreach (var dict in state.articleState.articleDict) {
                            if (dict.Value.channelId == action.channelId) {
                                dict.Value.hasMore = action.hasMore;
                                dict.Value.currOldestMessageId = action.currOldestMessageId;
                            }
                        }
                    }

                    foreach (var keyValuePair in channelMessageList) {
                        if (state.messageState.channelMessageList.ContainsKey(key: keyValuePair.Key)) {
                            var oldList = state.messageState.channelMessageList[key: keyValuePair.Key];
                            if (action.isRefreshList) {
                                oldList.Clear();
                            }

                            oldList.AddRange(collection: keyValuePair.Value);
                            var newList = oldList.Distinct().ToList();
                            state.messageState.channelMessageList[key: keyValuePair.Key] = newList;
                        }
                        else {
                            state.messageState.channelMessageList.Add(key: keyValuePair.Key, value: keyValuePair.Value);
                        }
                    }

                    foreach (var keyValuePair in channelMessageDict) {
                        if (state.messageState.channelMessageDict.ContainsKey(key: keyValuePair.Key)) {
                            var oldDict = state.messageState.channelMessageDict[key: keyValuePair.Key];
                            var newDict = keyValuePair.Value;
                            foreach (var valuePair in newDict) {
                                if (oldDict.ContainsKey(key: valuePair.Key)) {
                                    oldDict[key: valuePair.Key] = valuePair.Value;
                                }
                                else {
                                    oldDict.Add(key: valuePair.Key, value: valuePair.Value);
                                }
                            }

                            state.messageState.channelMessageDict[key: keyValuePair.Key] = oldDict;
                        }
                        else {
                            state.messageState.channelMessageDict.Add(key: keyValuePair.Key, value: keyValuePair.Value);
                        }
                    }

                    break;
                }

                case LikeCommentSuccessAction action: {
                    var user = new User { id = state.loginState.loginInfo.userId };
                    var reaction = new Reaction { user = user };
                    action.message.reactions.Add(item: reaction);
                    state.messageState.channelMessageDict[key: action.message.channelId][key: action.message.id] =
                        action.message;
                    break;
                }

                case RemoveLikeCommentSuccessAction action: {
                    var reactions = action.message.reactions;
                    foreach (var reaction in reactions) {
                        if (reaction.user.id == state.loginState.loginInfo.userId) {
                            action.message.reactions.Remove(item: reaction);
                            break;
                        }
                    }

                    state.messageState.channelMessageDict[key: action.message.channelId][key: action.message.id] =
                        action.message;
                    break;
                }

                case SendCommentSuccessAction action: {
                    if (action.message.deleted) {
                        break;
                    }

                    if (state.messageState.channelMessageList.ContainsKey(key: action.message.channelId)) {
                        var list = state.messageState.channelMessageList[key: action.message.channelId];
                        list.Insert(0, item: action.message.id);
                        state.messageState.channelMessageList[key: action.message.channelId] = list;
                    }
                    else {
                        state.messageState.channelMessageList.Add(key: action.message.channelId,
                            new List<string> { action.message.id });
                    }

                    if (state.messageState.channelMessageDict.ContainsKey(key: action.message.channelId)) {
                        var dict = state.messageState.channelMessageDict[key: action.message.channelId];
                        dict.Add(key: action.message.id, value: action.message);
                        state.messageState.channelMessageDict[key: action.message.channelId] = dict;
                    }
                    else {
                        state.messageState.channelMessageDict.Add(
                            key: action.message.channelId,
                            new Dictionary<string, Message> {
                                { action.message.id, action.message }
                            }
                        );
                    }

                    if (state.articleState.articleDict.ContainsKey(key: action.articleId)) {
                        var article = state.articleState.articleDict[key: action.articleId];
                        article.commentCount += 1;
                        state.articleState.articleDict[key: action.articleId] = article;
                    }

                    if (state.messageState.channelMessageDict.ContainsKey(key: action.channelId)) {
                        var messageDict = state.messageState.channelMessageDict[key: action.channelId];
                        if (action.upperMessageId.isNotEmpty()) {
                            if (messageDict.ContainsKey(key: action.upperMessageId)) {
                                var message = messageDict[key: action.upperMessageId];
                                (message.lowerMessageIds ?? new List<string>()).Add(item: action.message.id);
                                messageDict[key: action.upperMessageId] = message;
                            }
                        }
                        else {
                            if (messageDict.ContainsKey(key: action.parentMessageId)) {
                                var message = messageDict[key: action.parentMessageId];
                                (message.replyMessageIds ?? new List<string>()).Add(item: action.message.id);
                                messageDict[key: action.parentMessageId] = message;
                            }
                        }

                        state.messageState.channelMessageDict[key: action.channelId] = messageDict;
                    }

                    break;
                }

                case StartFetchNotificationsAction action: {
                    switch (action.category) {
                        case NotificationCategory.All: {
                            state.notificationState.allNotificationState.loading = true;
                            break;
                        }
                        case NotificationCategory.Follow: {
                            state.notificationState.followNotificationState.loading = true;
                            break;
                        }
                        case NotificationCategory.Involve: {
                            state.notificationState.involveNotificationState.loading = true;
                            break;
                        }
                        case NotificationCategory.Participate: {
                            state.notificationState.participateNotificationState.loading = true;
                            break;
                        }
                        case NotificationCategory.System: {
                            state.notificationState.systemNotificationState.loading = true;
                            break;
                        }
                    }

                    break;
                }

                case FetchNotificationsSuccessAction action: {
                    List<Notification> notifications;
                    List<User> mentions;

                    var page = action.pageNumber;
                    var pageTotal = action.pageTotal;

                    if (action.pageNumber == 1) {
                        notifications = action.notifications;
                        mentions = action.mentions;
                    }
                    else {
                        var newNotifications = new List<Notification>();
                        var newMentions = new List<User>();
                        switch (action.category) {
                            case NotificationCategory.All: {
                                newNotifications = state.notificationState.allNotificationState.notifications;
                                newMentions = state.notificationState.allNotificationState.mentions;
                                break;
                            }
                            case NotificationCategory.Follow: {
                                newNotifications = state.notificationState.followNotificationState.notifications;
                                newMentions = state.notificationState.followNotificationState.mentions;
                                break;
                            }
                            case NotificationCategory.Involve: {
                                newNotifications = state.notificationState.involveNotificationState.notifications;
                                newMentions = state.notificationState.involveNotificationState.mentions;
                                break;
                            }
                            case NotificationCategory.Participate: {
                                newNotifications = state.notificationState.participateNotificationState.notifications;
                                newMentions = state.notificationState.participateNotificationState.mentions;
                                break;
                            }
                            case NotificationCategory.System: {
                                newNotifications = state.notificationState.systemNotificationState.notifications;
                                newMentions = state.notificationState.systemNotificationState.mentions;
                                break;
                            }
                        }

                        if (action.pageNumber <= action.pageTotal) {
                            newNotifications.AddRange(collection: action.notifications);
                        }

                        foreach (var user in action.mentions) {
                            if (!newMentions.Contains(item: user)) {
                                newMentions.Add(item: user);
                            }
                        }

                        notifications = newNotifications;
                        mentions = newMentions;
                    }

                    switch (action.category) {
                        case NotificationCategory.All: {
                            state.notificationState.allNotificationState.loading = false;
                            state.notificationState.allNotificationState.page = page;
                            state.notificationState.allNotificationState.pageTotal = pageTotal;
                            state.notificationState.allNotificationState.notifications = notifications;
                            state.notificationState.allNotificationState.mentions = mentions;
                            break;
                        }
                        case NotificationCategory.Follow: {
                            state.notificationState.followNotificationState.loading = false;
                            state.notificationState.followNotificationState.page = page;
                            state.notificationState.followNotificationState.pageTotal = pageTotal;
                            state.notificationState.followNotificationState.notifications = notifications;
                            state.notificationState.followNotificationState.mentions = mentions;
                            break;
                        }
                        case NotificationCategory.Involve: {
                            state.notificationState.involveNotificationState.loading = false;
                            state.notificationState.involveNotificationState.page = page;
                            state.notificationState.involveNotificationState.pageTotal = pageTotal;
                            state.notificationState.involveNotificationState.notifications = notifications;
                            state.notificationState.involveNotificationState.mentions = mentions;
                            break;
                        }
                        case NotificationCategory.Participate: {
                            state.notificationState.participateNotificationState.loading = false;
                            state.notificationState.participateNotificationState.page = page;
                            state.notificationState.participateNotificationState.pageTotal = pageTotal;
                            state.notificationState.participateNotificationState.notifications = notifications;
                            state.notificationState.participateNotificationState.mentions = mentions;
                            break;
                        }
                        case NotificationCategory.System: {
                            state.notificationState.systemNotificationState.loading = false;
                            state.notificationState.systemNotificationState.page = page;
                            state.notificationState.systemNotificationState.pageTotal = pageTotal;
                            state.notificationState.systemNotificationState.notifications = notifications;
                            state.notificationState.systemNotificationState.mentions = mentions;
                            break;
                        }
                    }

                    state.loginState.newNotifications = null;
                    break;
                }

                case FetchNotificationsFailureAction action: {
                    switch (action.category) {
                        case NotificationCategory.All: {
                            state.notificationState.allNotificationState.loading = false;
                            break;
                        }
                        case NotificationCategory.Follow: {
                            state.notificationState.followNotificationState.loading = false;
                            break;
                        }
                        case NotificationCategory.Involve: {
                            state.notificationState.involveNotificationState.loading = false;
                            break;
                        }
                        case NotificationCategory.Participate: {
                            state.notificationState.participateNotificationState.loading = false;
                            break;
                        }
                        case NotificationCategory.System: {
                            state.notificationState.systemNotificationState.loading = false;
                            break;
                        }
                    }

                    break;
                }

                case ArticleMapAction action: {
                    if (action.articleMap.isNotNullAndEmpty()) {
                        var articleDict = state.articleState.articleDict;
                        foreach (var keyValuePair in action.articleMap) {
                            if (articleDict.ContainsKey(key: keyValuePair.Key)) {
                                var oldArticle = articleDict[key: keyValuePair.Key];
                                articleDict[key: keyValuePair.Key] = oldArticle.Merge(other: keyValuePair.Value);
                            }
                            else {
                                articleDict.Add(key: keyValuePair.Key, value: keyValuePair.Value);
                            }
                        }

                        state.articleState.articleDict = articleDict;
                    }

                    break;
                }

                case TagMapAction action: {
                    if (action.tagMap.isNotNullAndEmpty()) {
                        var tagDict = state.tagState.tagDict;
                        foreach (var keyValuePair in action.tagMap) {
                            if (tagDict.ContainsKey(key: keyValuePair.Key)) {
                                tagDict[key: keyValuePair.Key] = keyValuePair.Value;
                            }
                            else {
                                tagDict.Add(key: keyValuePair.Key, value: keyValuePair.Value);
                            }
                        }

                        state.tagState.tagDict = tagDict;
                    }

                    break;
                }

                case UserMapAction action: {
                    if (action.userMap.isNotNullAndEmpty()) {
                        var userDict = state.userState.userDict;
                        foreach (var keyValuePair in action.userMap) {
                            if (userDict.ContainsKey(key: keyValuePair.Key)) {
                                var oldUser = userDict[key: keyValuePair.Key];
                                userDict[key: keyValuePair.Key] = oldUser.Merge(other: keyValuePair.Value);
                            }
                            else {
                                userDict.Add(key: keyValuePair.Key, value: keyValuePair.Value);
                            }
                        }

                        state.userState.userDict = userDict;
                    }

                    break;
                }

                case UserLicenseMapAction action: {
                    if (action.userLicenseMap.isNotNullAndEmpty()) {
                        var userLicenseDict = state.userState.userLicenseDict;
                        foreach (var keyValuePair in action.userLicenseMap) {
                            if (userLicenseDict.ContainsKey(key: keyValuePair.Key)) {
                                userLicenseDict[key: keyValuePair.Key] = keyValuePair.Value;
                            }
                            else {
                                userLicenseDict.Add(key: keyValuePair.Key, value: keyValuePair.Value);
                            }
                        }

                        state.userState.userLicenseDict = userLicenseDict;
                    }

                    break;
                }

                case TeamMapAction action: {
                    if (action.teamMap.isNotNullAndEmpty()) {
                        var teamDict = state.teamState.teamDict;
                        foreach (var keyValuePair in action.teamMap) {
                            if (teamDict.ContainsKey(key: keyValuePair.Key)) {
                                var oldTeam = teamDict[key: keyValuePair.Key];
                                var newTeam = oldTeam.Merge(other: keyValuePair.Value);
                                teamDict[key: keyValuePair.Key] = newTeam;
                            }
                            else {
                                teamDict.Add(key: keyValuePair.Key, value: keyValuePair.Value);
                            }
                        }

                        state.teamState.teamDict = teamDict;
                    }

                    break;
                }

                case PlaceMapAction action: {
                    if (action.placeMap.isNotNullAndEmpty()) {
                        var placeDict = state.placeState.placeDict;
                        foreach (var keyValuePair in action.placeMap) {
                            if (placeDict.ContainsKey(key: keyValuePair.Key)) {
                                placeDict[key: keyValuePair.Key] = keyValuePair.Value;
                            }
                            else {
                                placeDict.Add(key: keyValuePair.Key, value: keyValuePair.Value);
                            }
                        }

                        state.placeState.placeDict = placeDict;
                    }

                    break;
                }

                case FollowMapAction action: {
                    if (action.followMap.isNotNullAndEmpty()) {
                        var userId = state.loginState.loginInfo.userId ?? "";
                        if (userId.isNotEmpty()) {
                            var followDict = state.followState.followDict;
                            var followMap = followDict.ContainsKey(key: userId)
                                ? followDict[key: userId]
                                : new Dictionary<string, bool>();
                            foreach (var keyValuePair in action.followMap) {
                                if (!followMap.ContainsKey(key: keyValuePair.Key)) {
                                    followMap.Add(key: keyValuePair.Key, value: keyValuePair.Value);
                                }
                            }

                            if (followDict.ContainsKey(key: userId)) {
                                followDict[key: userId] = followMap;
                            }
                            else {
                                followDict.Add(key: userId, value: followMap);
                            }

                            state.followState.followDict = followDict;
                        }
                    }

                    break;
                }

                case LikeMapAction action: {
                    if (action.likeMap.isNotNullAndEmpty()) {
                        var userId = state.loginState.loginInfo.userId ?? "";
                        if (userId.isNotEmpty()) {
                            var likeDict = state.likeState.likeDict;
                            var likeMap = likeDict.ContainsKey(key: userId)
                                ? likeDict[key: userId]
                                : new Dictionary<string, bool>();
                            foreach (var keyValuePair in action.likeMap) {
                                if (!likeMap.ContainsKey(key: keyValuePair.Key)) {
                                    likeMap.Add(key: keyValuePair.Key, value: keyValuePair.Value);
                                }
                            }

                            if (likeDict.ContainsKey(key: userId)) {
                                likeDict[key: userId] = likeMap;
                            }
                            else {
                                likeDict.Add(key: userId, value: likeMap);
                            }

                            state.likeState.likeDict = likeDict;
                        }
                    }

                    break;
                }

                case RankListAction action: {
                    if (action.rankList.isNotEmpty()) {
                        var rankDict = state.leaderBoardState.rankDict;
                        action.rankList.ForEach(rankData => {
                            if (rankDict.ContainsKey(key: rankData.id)) {
                                rankDict[key: rankData.id] = rankData;
                            }
                            else {
                                rankDict.Add(key: rankData.id, value: rankData);
                            }
                        });
                        state.leaderBoardState.rankDict = rankDict;
                    }

                    break;
                }

                case FavoriteTagMapAction action: {
                    if (action.favoriteTagMap.isNotNullAndEmpty()) {
                        var favoriteTagDict = state.favoriteState.favoriteTagDict;
                        foreach (var keyValuePair in action.favoriteTagMap) {
                            if (favoriteTagDict.ContainsKey(key: keyValuePair.Key)) {
                                favoriteTagDict[key: keyValuePair.Key] = keyValuePair.Value;
                            }
                            else {
                                favoriteTagDict.Add(key: keyValuePair.Key, value: keyValuePair.Value);
                            }
                        }

                        state.favoriteState.favoriteTagDict = favoriteTagDict;
                    }

                    break;
                }
                case MyFavoriteTagMapAction action: {
                    if (state.loginState.isLoggedIn && action.favoriteTagMap.isNotNullAndEmpty()) {
                        var favoriteIds = new List<string>();
                        foreach (var keyValuePair in action.favoriteTagMap) {
                            favoriteIds.Add(item: keyValuePair.Value.id);
                        }

                        if (state.favoriteState.favoriteTagIdDict.ContainsKey(key: state.loginState.loginInfo.userId)) {
                            var oldFavoriteIds =
                                state.favoriteState.favoriteTagIdDict[key: state.loginState.loginInfo.userId];
                            favoriteIds.ForEach(favoriteId => {
                                if (!oldFavoriteIds.Contains(item: favoriteId)) {
                                    oldFavoriteIds.Add(item: favoriteId);
                                }
                            });
                            state.favoriteState.favoriteTagIdDict[key: state.loginState.loginInfo.userId] = favoriteIds;
                        }
                        else {
                            state.favoriteState.favoriteTagIdDict.Add(key: state.loginState.loginInfo.userId,
                                value: favoriteIds);
                        }
                    }

                    break;
                }
                case PopularSearchArticleSuccessAction action: {
                    state.popularSearchState.popularSearchArticles = action.popularSearchArticles;
                    break;
                }

                case PopularSearchUserSuccessAction action: {
                    state.popularSearchState.popularSearchUsers = action.popularSearchUsers;
                    break;
                }

                case StartSearchArticleAction action: {
                    state.searchState.searchArticleLoading = true;
                    state.searchState.keyword = action.keyword;
                    break;
                }

                case SearchArticleSuccessAction action: {
                    state.searchState.searchArticleLoading = false;
                    state.searchState.searchArticleHasMore = action.hasMore;
                    var articleIds = new List<string>();
                    (action.searchArticles ?? new List<Article>()).ForEach(searchArticle => {
                        articleIds.Add(item: searchArticle.id);
                        if (!state.articleState.articleDict.ContainsKey(key: searchArticle.id)) {
                            state.articleState.articleDict.Add(key: searchArticle.id, value: searchArticle);
                        }
                        else {
                            var oldArticle = state.articleState.articleDict[key: searchArticle.id];
                            state.articleState.articleDict[key: searchArticle.id] =
                                oldArticle.Merge(other: searchArticle);
                        }
                    });

                    if (state.searchState.searchArticleIdDict.ContainsKey(key: action.keyword)) {
                        if (action.pageNumber == 1) {
                            state.searchState.searchArticleIdDict[key: action.keyword] = articleIds;
                        }
                        else {
                            var searchArticleIds = state.searchState.searchArticleIdDict[key: action.keyword];
                            searchArticleIds.AddRange(collection: articleIds);
                            state.searchState.searchArticleIdDict[key: action.keyword] = searchArticleIds;
                        }
                    }
                    else {
                        state.searchState.searchArticleIdDict.Add(key: action.keyword, value: articleIds);
                    }

                    break;
                }

                case SearchArticleFailureAction _: {
                    state.searchState.searchArticleLoading = false;
                    break;
                }

                case ClearSearchResultAction _: {
                    state.searchState.keyword = "";
                    state.searchState.searchArticleIdDict = new Dictionary<string, List<string>>();
                    state.searchState.searchUserIdDict = new Dictionary<string, List<string>>();
                    state.searchState.searchTeamIdDict = new Dictionary<string, List<string>>();
                    state.searchState.searchQuestionIdsDict = new Dictionary<string, List<string>>();
                    break;
                }

                case SaveSearchArticleHistoryAction action: {
                    var searchArticleHistoryList = HistoryManager.saveSearchArticleHistoryList(keyword: action.keyword,
                        state.loginState.isLoggedIn ? state.loginState.loginInfo.userId : null);
                    state.searchState.searchArticleHistoryList = searchArticleHistoryList;
                    break;
                }

                case DeleteSearchArticleHistoryAction action: {
                    var searchArticleHistoryList = HistoryManager.deleteSearchArticleHistoryList(
                        keyword: action.keyword,
                        state.loginState.isLoggedIn ? state.loginState.loginInfo.userId : null);
                    state.searchState.searchArticleHistoryList = searchArticleHistoryList;
                    break;
                }

                case DeleteAllSearchArticleHistoryAction _: {
                    state.searchState.searchArticleHistoryList = new List<string>();
                    HistoryManager.deleteAllSearchArticleHistory(state.loginState.isLoggedIn
                        ? state.loginState.loginInfo.userId
                        : null);
                    break;
                }

                case StartSearchUserAction action: {
                    state.searchState.searchUserLoading = true;
                    state.searchState.keyword = action.keyword;
                    break;
                }

                case SearchUserSuccessAction action: {
                    state.searchState.searchUserLoading = false;
                    state.searchState.searchUserHasMore = action.hasMore;
                    if (state.searchState.searchUserIdDict.ContainsKey(key: action.keyword)) {
                        if (action.pageNumber == 1) {
                            state.searchState.searchUserIdDict[key: action.keyword] = action.searchUserIds;
                        }
                        else {
                            var searchUserIds = state.searchState.searchUserIdDict[key: action.keyword] ??
                                                new List<string>();
                            searchUserIds.AddRange(collection: action.searchUserIds);
                            state.searchState.searchUserIdDict[key: action.keyword] = searchUserIds;
                        }
                    }
                    else {
                        state.searchState.searchUserIdDict.Add(key: action.keyword, value: action.searchUserIds);
                    }

                    break;
                }

                case SearchUserFailureAction _: {
                    state.searchState.searchUserLoading = false;
                    break;
                }

                case StartSearchFollowingAction _: {
                    state.searchState.searchFollowingLoading = true;
                    break;
                }

                case SearchFollowingSuccessAction action: {
                    state.searchState.searchFollowingLoading = false;
                    state.searchState.searchFollowingKeyword = action.keyword;
                    state.searchState.searchFollowingHasMore = action.hasMore;
                    if (action.pageNumber == 1) {
                        state.searchState.searchFollowings = action.users;
                    }
                    else {
                        var searchUsers = state.searchState.searchFollowings;
                        searchUsers.AddRange(collection: action.users);
                        state.searchState.searchFollowings = searchUsers;
                    }

                    break;
                }

                case SearchFollowingFailureAction action: {
                    state.searchState.searchFollowingLoading = false;
                    state.searchState.searchFollowingKeyword = action.keyword;
                    break;
                }

                case ClearSearchFollowingResultAction _: {
                    state.searchState.searchFollowingKeyword = "";
                    state.searchState.searchFollowings = new List<User>();
                    break;
                }

                case StartSearchTeamAction action: {
                    state.searchState.searchTeamLoading = true;
                    state.searchState.keyword = action.keyword;
                    break;
                }

                case SearchTeamSuccessAction action: {
                    state.searchState.searchTeamLoading = false;
                    state.searchState.searchTeamHasMore = action.hasMore;
                    if (state.searchState.searchTeamIdDict.ContainsKey(key: action.keyword)) {
                        if (action.pageNumber == 1) {
                            state.searchState.searchTeamIdDict[key: action.keyword] = action.searchTeamIds;
                        }
                        else {
                            var searchTeamIds = state.searchState.searchTeamIdDict[key: action.keyword] ??
                                                new List<string>();
                            searchTeamIds.AddRange(collection: action.searchTeamIds);
                            state.searchState.searchTeamIdDict[key: action.keyword] = searchTeamIds;
                        }
                    }
                    else {
                        state.searchState.searchTeamIdDict.Add(key: action.keyword, value: action.searchTeamIds);
                    }

                    break;
                }

                case SearchTeamFailureAction _: {
                    state.searchState.searchTeamLoading = false;
                    break;
                }

                case StartSearchQuestionAction action: {
                    state.searchState.searchQuestionLoading = true;
                    state.searchState.keyword = action.keyword;
                    break;
                }

                case SearchQuestionsSuccessAction action: {
                    state.searchState.searchQuestionLoading = false;
                    state.searchState.searchQuestionHasMore = action.hasMore;
                    var searchQuestionIdsDict = state.searchState.searchQuestionIdsDict;
                    var keyword = action.keyword;
                    if (searchQuestionIdsDict.ContainsKey(key: keyword)) {
                        if (action.page == 1) {
                            searchQuestionIdsDict[key: keyword] = action.questionIds;
                        }
                        else {
                            var questionIds = searchQuestionIdsDict[key: action.keyword] ??
                                              new List<string>();
                            questionIds.AddRange(collection: action.questionIds);
                            searchQuestionIdsDict[key: keyword] = questionIds;
                        }
                    }
                    else {
                        searchQuestionIdsDict.Add(key: keyword, value: action.questionIds);
                    }

                    break;
                }

                case SearchQuestionsFailureAction _: {
                    state.searchState.searchQuestionLoading = false;
                    break;
                }

                case SearchTagsSuccessAction action: {
                    state.searchState.searchTagList = action.tagIds;
                    break;
                }

                case SearchTagsFailureAction _: {
                    break;
                }

                case UtilsAction.OpenUrlAction action: {
                    if (action.url.isNotEmpty()) {
                        if (!action.url.StartsWith("http")) {
                            Application.OpenURL(url: action.url);
                        }
                        else {
                            if (UrlLauncherPlugin.CanLaunch(urlString: action.url)) {
                                UrlLauncherPlugin.Launch(urlString: action.url);
                            }
                        }
                    }

                    break;
                }

                case UtilsAction.CopyTextAction action: {
                    Clipboard.setData(new ClipboardData(text: action.text));
                    break;
                }

                case StartFetchLeaderBoardCollectionAction _: {
                    state.leaderBoardState.collectionLoading = true;
                    break;
                }

                case FetchLeaderBoardCollectionSuccessAction action: {
                    state.leaderBoardState.collectionLoading = false;


                    if (action.pageNumber == 1) {
                        state.leaderBoardState.collectionIds = action.collectionIds;
                    }
                    else {
                        var collectionIds = state.leaderBoardState.collectionIds;
                        collectionIds.AddRange(collection: action.collectionIds);
                        state.leaderBoardState.collectionIds = collectionIds;
                    }

                    state.leaderBoardState.collectionHasMore = action.hasMore;
                    state.leaderBoardState.collectionPageNumber = action.pageNumber;
                    break;
                }

                case FetchLeaderBoardCollectionFailureAction _: {
                    state.leaderBoardState.collectionLoading = false;
                    break;
                }

                case StartFetchLeaderBoardColumnAction _: {
                    state.leaderBoardState.columnLoading = true;
                    break;
                }

                case FetchLeaderBoardColumnSuccessAction action: {
                    state.leaderBoardState.columnLoading = false;

                    if (action.pageNumber == 1) {
                        state.leaderBoardState.columnIds = action.columnIds;
                    }
                    else {
                        var columnIds = state.leaderBoardState.columnIds;
                        columnIds.AddRange(collection: action.columnIds);
                        state.leaderBoardState.columnIds = columnIds;
                    }

                    if (action.userArticleMap != null && action.userArticleMap.isNotEmpty()) {
                        var userArticleDict = state.articleState.userArticleDict;
                        foreach (var keyValuePair in action.userArticleMap) {
                            if (userArticleDict.ContainsKey(key: keyValuePair.Key)) {
                                userArticleDict[key: keyValuePair.Key] = keyValuePair.Value;
                            }
                            else {
                                userArticleDict.Add(key: keyValuePair.Key, value: keyValuePair.Value);
                            }
                        }

                        state.articleState.userArticleDict = userArticleDict;
                    }

                    state.leaderBoardState.columnHasMore = action.hasMore;
                    state.leaderBoardState.columnPageNumber = action.pageNumber;
                    break;
                }

                case FetchLeaderBoardColumnFailureAction _: {
                    state.leaderBoardState.columnLoading = false;
                    break;
                }

                case StartFetchLeaderBoardBloggerAction _: {
                    state.leaderBoardState.bloggerLoading = true;
                    break;
                }

                case FetchLeaderBoardBloggerSuccessAction action: {
                    state.leaderBoardState.bloggerLoading = false;
                    if (action.pageNumber == 1) {
                        state.leaderBoardState.bloggerIds = action.bloggerIds;
                    }
                    else {
                        var bloggerIds = new List<string>();
                        if (state.leaderBoardState.bloggerIds.isNotEmpty()) {
                            bloggerIds = state.leaderBoardState.bloggerIds;
                            action.bloggerIds.ForEach(bloggerId => {
                                if (!bloggerIds.Contains(item: bloggerId)) {
                                    bloggerIds.Add(item: bloggerId);
                                }
                            });
                        }
                        else {
                            state.leaderBoardState.bloggerIds = action.bloggerIds;
                        }

                        state.leaderBoardState.bloggerIds = bloggerIds;
                    }

                    state.leaderBoardState.bloggerHasMore = action.hasMore;
                    state.leaderBoardState.bloggerPageNumber = action.pageNumber;
                    break;
                }

                case FetchLeaderBoardBloggerFailureAction _: {
                    state.leaderBoardState.bloggerLoading = false;
                    break;
                }

                case StartFetchHomeBloggerAction _: {
                    state.leaderBoardState.homeBloggerLoading = true;
                    break;
                }

                case FetchHomeBloggerSuccessAction action: {
                    state.leaderBoardState.homeBloggerLoading = false;
                    if (action.pageNumber == 1) {
                        state.leaderBoardState.homeBloggerIds = action.bloggerIds;
                    }
                    else {
                        var bloggerIds = state.leaderBoardState.homeBloggerIds;
                        bloggerIds.AddRange(collection: action.bloggerIds);
                        state.leaderBoardState.homeBloggerIds = bloggerIds;
                    }

                    state.leaderBoardState.homeBloggerHasMore = action.hasMore;
                    state.leaderBoardState.homeBloggerPageNumber = action.pageNumber;
                    break;
                }

                case FetchHomeBloggerFailureAction _: {
                    state.leaderBoardState.homeBloggerLoading = false;
                    break;
                }
                case StartFetchLeaderBoardDetailAction _: {
                    state.leaderBoardState.detailLoading = true;
                    break;
                }

                case FetchLeaderBoardDetailSuccessAction action: {
                    if (action.type == LeaderBoardType.collection) {
                        if (state.leaderBoardState.collectionDict.ContainsKey(key: action.albumId)) {
                            var articles = state.leaderBoardState.collectionDict[key: action.albumId];
                            if (action.pageNumber == 1) {
                                articles.Clear();
                                articles.AddRange(collection: action.articleList);
                            }
                            else {
                                action.articleList.ForEach(articleId => {
                                    if (!articles.Contains(item: articleId)) {
                                        articles.Add(item: articleId);
                                    }
                                });
                            }

                            state.leaderBoardState.collectionDict[key: action.albumId] = articles;
                        }
                        else {
                            state.leaderBoardState.collectionDict.Add(key: action.albumId, value: action.articleList);
                        }
                    }
                    else {
                        if (state.leaderBoardState.columnDict.ContainsKey(key: action.albumId)) {
                            var articles = state.leaderBoardState.columnDict[key: action.albumId];
                            if (action.pageNumber == 1) {
                                articles.Clear();
                                articles.AddRange(collection: action.articleList);
                            }
                            else {
                                action.articleList.ForEach(articleId => {
                                    if (!articles.Contains(item: articleId)) {
                                        articles.Add(item: articleId);
                                    }
                                });
                            }

                            state.leaderBoardState.columnDict[key: action.albumId] = articles;
                        }
                        else {
                            state.leaderBoardState.columnDict.Add(key: action.albumId, value: action.articleList);
                        }
                    }

                    state.leaderBoardState.detailHasMore = action.hasMore;
                    state.leaderBoardState.detailLoading = false;
                    break;
                }

                case FetchLeaderBoardDetailFailureAction _: {
                    state.leaderBoardState.detailLoading = false;

                    break;
                }

                case FetchReviewUrlSuccessAction action: {
                    state.settingState.reviewUrl = action.url;
                    state.settingState.hasReviewUrl = action.url.isNotEmpty();
                    break;
                }

                case FetchReviewUrlFailureAction _: {
                    state.settingState.reviewUrl = "";
                    state.settingState.hasReviewUrl = false;
                    break;
                }

                case SettingVibrateAction action: {
                    state.settingState.vibrate = action.vibrate;
                    break;
                }

                case SettingClearCacheAction _: {
                    state.articleState.articleHistory = new List<Article>();
                    state.searchState.searchArticleHistoryList = new List<string>();
                    HistoryManager.deleteAllArticleHistory(state.loginState.isLoggedIn
                        ? state.loginState.loginInfo.userId
                        : null);
                    HistoryManager.deleteAllSearchArticleHistory(state.loginState.isLoggedIn
                        ? state.loginState.loginInfo.userId
                        : null);
                    break;
                }

                case StartFetchUserProfileAction _: {
                    state.userState.userLoading = true;
                    break;
                }

                case FetchUserProfileSuccessAction action: {
                    state.userState.userLoading = false;
                    if (!state.userState.userDict.ContainsKey(key: action.user.id)) {
                        state.userState.userDict.Add(key: action.user.id, value: action.user);
                    }
                    else {
                        var oldUser = state.userState.userDict[key: action.user.id];
                        state.userState.userDict[key: action.user.id] = oldUser.Merge(other: action.user);
                    }

                    if (action.userId != action.user.id) {
                        if (state.userState.slugDict.ContainsKey(key: action.userId)) {
                            state.userState.slugDict[key: action.userId] = action.user.id;
                        }
                        else {
                            state.userState.slugDict.Add(key: action.userId, value: action.user.id);
                        }
                    }

                    break;
                }

                case FetchUserProfileFailureAction action: {
                    state.userState.userLoading = false;
                    if (!state.userState.userDict.ContainsKey(key: action.userId)) {
                        var user = new User {
                            errorCode = action.errorCode
                        };
                        state.userState.userDict.Add(key: action.userId, value: user);
                    }
                    else {
                        var user = state.userState.userDict[key: action.userId];
                        user.errorCode = action.errorCode;
                        state.userState.userDict[key: action.userId] = user;
                    }

                    break;
                }

                case StartFetchUserArticleAction _: {
                    state.userState.userArticleLoading = true;
                    break;
                }

                case FetchUserArticleSuccessAction action: {
                    state.userState.userArticleLoading = false;
                    var articleIds = new List<string>();
                    action.articles.ForEach(article => {
                        articleIds.Add(item: article.id);
                        if (!state.articleState.articleDict.ContainsKey(key: article.id)) {
                            state.articleState.articleDict.Add(key: article.id, value: article);
                        }
                        else {
                            var oldArticle = state.articleState.articleDict[key: article.id];
                            state.articleState.articleDict[key: article.id] = oldArticle.Merge(other: article);
                        }
                    });
                    if (state.userState.userDict.ContainsKey(key: action.userId)) {
                        var user = state.userState.userDict[key: action.userId];
                        user.articlesHasMore = action.hasMore;
                        if (action.pageNumber == 1) {
                            user.articleIds = articleIds;
                        }
                        else {
                            var userArticleIds = user.articleIds;
                            userArticleIds.AddRange(collection: articleIds);
                            user.articleIds = userArticleIds;
                        }

                        state.userState.userDict[key: action.userId] = user;
                    }
                    else {
                        var user = new User { articlesHasMore = action.hasMore };
                        if (action.pageNumber == 1) {
                            user.articleIds = articleIds;
                        }
                        else {
                            var userArticleIds = user.articleIds;
                            userArticleIds.AddRange(collection: articleIds);
                            user.articleIds = userArticleIds;
                        }

                        state.userState.userDict.Add(key: action.userId, value: user);
                    }

                    break;
                }

                case FetchUserArticleFailureAction _: {
                    state.userState.userArticleLoading = false;
                    break;
                }

                case StartFetchUserLikeArticleAction _: {
                    state.userState.userLikeArticleLoading = true;
                    break;
                }

                case FetchUserLikeArticleSuccessAction action: {
                    state.userState.userLikeArticleLoading = false;
                    var articleIds = new List<string>();
                    action.articles.ForEach(article => {
                        articleIds.Add(item: article.id);
                        if (!state.articleState.articleDict.ContainsKey(key: article.id)) {
                            state.articleState.articleDict.Add(key: article.id, value: article);
                        }
                        else {
                            var oldArticle = state.articleState.articleDict[key: article.id];
                            state.articleState.articleDict[key: article.id] = oldArticle.Merge(other: article);
                        }
                    });
                    if (state.userState.userDict.ContainsKey(key: action.userId)) {
                        var user = state.userState.userDict[key: action.userId];
                        user.likeArticlesHasMore = action.hasMore;
                        user.likeArticlesPage = action.pageNumber;
                        if (action.pageNumber == 1) {
                            user.likeArticleIds = articleIds;
                        }
                        else {
                            var userLikeArticleIds = user.likeArticleIds;
                            userLikeArticleIds.AddRange(collection: articleIds);
                            user.likeArticleIds = userLikeArticleIds;
                        }

                        state.userState.userDict[key: action.userId] = user;
                    }

                    break;
                }

                case FetchUserLikeArticleFailureAction _: {
                    state.userState.userLikeArticleLoading = false;
                    break;
                }

                case StartFollowUserAction action: {
                    if (state.userState.userDict.ContainsKey(key: action.followUserId)) {
                        var user = state.userState.userDict[key: action.followUserId];
                        user.followUserLoading = true;
                        state.userState.userDict[key: action.followUserId] = user;
                    }

                    break;
                }

                case FollowUserSuccessAction action: {
                    if (state.userState.userDict.ContainsKey(key: action.followUserId)) {
                        var user = state.userState.userDict[key: action.followUserId];
                        user.followUserLoading = false;
                        state.userState.userDict[key: action.followUserId] = user;
                    }

                    if (state.followState.followDict.ContainsKey(key: action.currentUserId)) {
                        var followMap = state.followState.followDict[key: action.currentUserId];
                        if (!followMap.ContainsKey(key: action.followUserId)) {
                            followMap.Add(key: action.followUserId, value: action.success);
                        }

                        state.followState.followDict[key: action.currentUserId] = followMap;
                    }
                    else {
                        var followMap = new Dictionary<string, bool>();
                        if (!followMap.ContainsKey(key: action.followUserId)) {
                            followMap.Add(key: action.followUserId, value: action.success);
                        }

                        state.followState.followDict.Add(key: action.currentUserId, value: followMap);
                    }

                    if (state.userState.userDict.ContainsKey(key: action.currentUserId)) {
                        var user = state.userState.userDict[key: action.currentUserId];
                        user.followingUsersCount += 1;
                        state.userState.userDict[key: action.currentUserId] = user;
                    }

                    if (state.userState.userDict.ContainsKey(key: action.followUserId)) {
                        var user = state.userState.userDict[key: action.followUserId];
                        user.followCount += 1;
                        state.userState.userDict[key: action.followUserId] = user;
                    }

                    EventBus.publish(sName: EventBusConstant.follow_user, args: new List<object>());

                    break;
                }

                case FollowUserFailureAction action: {
                    if (state.userState.userDict.ContainsKey(key: action.followUserId)) {
                        var user = state.userState.userDict[key: action.followUserId];
                        user.followUserLoading = false;
                        state.userState.userDict[key: action.followUserId] = user;
                    }

                    break;
                }

                case StartUnFollowUserAction action: {
                    if (state.userState.userDict.ContainsKey(key: action.unFollowUserId)) {
                        var user = state.userState.userDict[key: action.unFollowUserId];
                        user.followUserLoading = true;
                        state.userState.userDict[key: action.unFollowUserId] = user;
                    }

                    break;
                }

                case UnFollowUserSuccessAction action: {
                    if (state.userState.userDict.ContainsKey(key: action.unFollowUserId)) {
                        var user = state.userState.userDict[key: action.unFollowUserId];
                        user.followUserLoading = false;
                        state.userState.userDict[key: action.unFollowUserId] = user;
                    }

                    if (state.followState.followDict.ContainsKey(key: action.currentUserId)) {
                        var followMap = state.followState.followDict[key: action.currentUserId];
                        if (followMap.ContainsKey(key: action.unFollowUserId)) {
                            followMap.Remove(key: action.unFollowUserId);
                        }

                        state.followState.followDict[key: action.currentUserId] = followMap;
                    }

                    if (state.userState.userDict.ContainsKey(key: action.currentUserId)) {
                        var user = state.userState.userDict[key: action.currentUserId];
                        user.followingUsersCount -= 1;
                        state.userState.userDict[key: action.currentUserId] = user;
                    }

                    if (state.userState.userDict.ContainsKey(key: action.unFollowUserId)) {
                        var user = state.userState.userDict[key: action.unFollowUserId];
                        user.followCount -= 1;
                        state.userState.userDict[key: action.unFollowUserId] = user;
                    }

                    EventBus.publish(sName: EventBusConstant.follow_user, args: new List<object>());

                    break;
                }

                case UnFollowUserFailureAction action: {
                    if (state.userState.userDict.ContainsKey(key: action.unFollowUserId)) {
                        var user = state.userState.userDict[key: action.unFollowUserId];
                        user.followUserLoading = false;
                        state.userState.userDict[key: action.unFollowUserId] = user;
                    }

                    break;
                }

                case StartFetchFollowingAction _: {
                    state.userState.followingLoading = true;
                    break;
                }

                case FetchFollowingSuccessAction action: {
                    state.userState.followingLoading = false;
                    if (state.userState.userDict.ContainsKey(key: action.userId)) {
                        var user = state.userState.userDict[key: action.userId];
                        user.followingsHasMore = action.followingHasMore;
                        if (action.offset == 0) {
                            user.followings = action.followings;
                        }
                        else {
                            var followings = user.followings;
                            followings.AddRange(collection: action.followings);
                            user.followings = followings;
                        }

                        state.userState.userDict[key: action.userId] = user;
                    }

                    break;
                }

                case FetchFollowingFailureAction _: {
                    state.userState.followingLoading = false;
                    break;
                }

                case StartFetchFollowingUserAction _: {
                    state.userState.followingUserLoading = true;
                    break;
                }

                case FetchFollowingUserSuccessAction action: {
                    state.userState.followingUserLoading = false;
                    if (state.userState.userDict.ContainsKey(key: action.userId)) {
                        var user = state.userState.userDict[key: action.userId];
                        user.followingUsersHasMore = action.followingUsersHasMore;
                        if (action.offset == 0) {
                            user.followingUsers = action.followingUsers;
                        }
                        else {
                            var followingUsers = user.followingUsers;
                            followingUsers.AddRange(collection: action.followingUsers);
                            user.followingUsers = followingUsers;
                        }

                        state.userState.userDict[key: action.userId] = user;
                    }

                    break;
                }

                case FetchFollowingUserFailureAction _: {
                    state.userState.followingUserLoading = false;
                    break;
                }

                case StartFetchFollowerAction _: {
                    state.userState.followerLoading = true;
                    break;
                }

                case FetchFollowerSuccessAction action: {
                    state.userState.followerLoading = false;
                    if (state.userState.userDict.ContainsKey(key: action.userId)) {
                        var user = state.userState.userDict[key: action.userId];
                        user.followersHasMore = action.followersHasMore;
                        if (action.offset == 0) {
                            user.followers = action.followers;
                        }
                        else {
                            var followers = user.followers;
                            followers.AddRange(collection: action.followers);
                            user.followers = followers;
                        }

                        state.userState.userDict[key: action.userId] = user;
                    }

                    break;
                }

                case FetchFollowerFailureAction _: {
                    state.userState.followerLoading = false;
                    break;
                }

                case StartFetchFollowingTeamAction _: {
                    state.userState.followingTeamLoading = true;
                    break;
                }

                case FetchFollowingTeamSuccessAction action: {
                    state.userState.followingTeamLoading = false;
                    if (state.userState.userDict.ContainsKey(key: action.userId)) {
                        var user = state.userState.userDict[key: action.userId];
                        user.followingTeamsHasMore = action.followingTeamsHasMore;
                        if (action.offset == 0) {
                            user.followingTeams = action.followingTeams;
                        }
                        else {
                            var followingTeams = user.followingTeams;
                            followingTeams.AddRange(collection: action.followingTeams);
                            user.followingTeams = followingTeams;
                        }

                        state.userState.userDict[key: action.userId] = user;
                    }

                    break;
                }

                case FetchFollowingTeamFailureAction _: {
                    state.userState.followingTeamLoading = false;
                    break;
                }

                case ChangePersonalFullNameAction action: {
                    state.userState.fullName = action.fullName;
                    break;
                }

                case ChangePersonalTitleAction action: {
                    state.userState.title = action.title;
                    break;
                }

                case ChangePersonalRoleAction action: {
                    state.userState.jobRole = action.jobRole;
                    break;
                }

                case CleanPersonalInfoAction _: {
                    state.userState.fullName = "";
                    state.userState.title = "";
                    state.userState.jobRole = new JobRole();
                    break;
                }

                case EditPersonalInfoSuccessAction action: {
                    if (state.userState.userDict.ContainsKey(key: action.user.id)) {
                        var oldUser = state.userState.userDict[key: action.user.id];
                        state.userState.userDict[key: action.user.id] = oldUser.Merge(other: action.user);
                    }

                    break;
                }

                case UpdateAvatarSuccessAction action: {
                    var userId = state.loginState.loginInfo.userId;
                    var user = state.userState.userDict[key: userId];
                    user.avatar = action.avatar;
                    state.userState.userDict[key: userId] = user;
                    state.loginState.loginInfo.userAvatar = action.avatar;
                    UserInfoManager.saveUserInfo(loginInfo: state.loginState.loginInfo);
                    break;
                }

                case StartFetchTeamAction _: {
                    state.teamState.teamLoading = true;
                    break;
                }

                case FetchTeamSuccessAction action: {
                    state.teamState.teamLoading = false;
                    var team = action.team;
                    if (!state.teamState.teamDict.ContainsKey(key: action.team.id)) {
                        state.teamState.teamDict.Add(key: action.team.id, value: team);
                    }
                    else {
                        var oldTeam = state.teamState.teamDict[key: action.team.id];
                        state.teamState.teamDict[key: action.team.id] = oldTeam.Merge(other: team);
                    }

                    if (action.teamId != action.team.id) {
                        if (state.teamState.slugDict.ContainsKey(key: action.teamId)) {
                            state.teamState.slugDict[key: action.teamId] = action.team.id;
                        }
                        else {
                            state.teamState.slugDict.Add(key: action.teamId, value: action.team.id);
                        }
                    }

                    break;
                }

                case FetchTeamFailureAction _: {
                    state.teamState.teamLoading = false;
                    break;
                }

                case StartFetchTeamArticleAction _: {
                    state.teamState.teamArticleLoading = true;
                    break;
                }

                case FetchTeamArticleSuccessAction action: {
                    state.teamState.teamArticleLoading = false;
                    var articleIds = new List<string>();
                    action.articles.ForEach(article => {
                        articleIds.Add(item: article.id);
                        if (!state.articleState.articleDict.ContainsKey(key: article.id)) {
                            state.articleState.articleDict.Add(key: article.id, value: article);
                        }
                        else {
                            var oldArticle = state.articleState.articleDict[key: article.id];
                            state.articleState.articleDict[key: article.id] = oldArticle.Merge(other: article);
                        }
                    });
                    if (state.teamState.teamDict.ContainsKey(key: action.teamId)) {
                        var team = state.teamState.teamDict[key: action.teamId];
                        team.articlesHasMore = action.hasMore;
                        if (action.pageNumber == 1) {
                            team.articleIds = articleIds;
                        }
                        else {
                            var teamArticleIds = team.articleIds;
                            teamArticleIds.AddRange(collection: articleIds);
                            team.articleIds = teamArticleIds;
                        }

                        state.teamState.teamDict[key: action.teamId] = team;
                    }
                    else {
                        var team = new Team { articlesHasMore = action.hasMore };
                        if (action.pageNumber == 1) {
                            team.articleIds = articleIds;
                        }
                        else {
                            var teamArticleIds = team.articleIds;
                            teamArticleIds.AddRange(collection: articleIds);
                            team.articleIds = teamArticleIds;
                        }

                        state.teamState.teamDict.Add(key: action.teamId, value: team);
                    }

                    break;
                }

                case FetchTeamArticleFailureAction action: {
                    state.teamState.teamArticleLoading = false;
                    if (!state.teamState.teamDict.ContainsKey(key: action.teamId)) {
                        var team = new Team {
                            errorCode = action.errorCode
                        };
                        state.teamState.teamDict.Add(key: action.teamId, value: team);
                    }
                    else {
                        var team = state.teamState.teamDict[key: action.teamId];
                        team.errorCode = action.errorCode;
                        state.teamState.teamDict[key: action.teamId] = team;
                    }

                    break;
                }

                case StartFetchTeamFollowerAction _: {
                    state.teamState.followerLoading = true;
                    break;
                }

                case FetchTeamFollowerSuccessAction action: {
                    state.teamState.followerLoading = false;
                    if (state.teamState.teamDict.ContainsKey(key: action.teamId)) {
                        var team = state.teamState.teamDict[key: action.teamId];
                        team.followersHasMore = action.followersHasMore;
                        if (action.offset == 0) {
                            team.followers = action.followers;
                        }
                        else {
                            var followers = team.followers;
                            followers.AddRange(collection: action.followers);
                            team.followers = followers;
                        }

                        state.teamState.teamDict[key: action.teamId] = team;
                    }

                    break;
                }

                case FetchTeamFollowerFailureAction _: {
                    state.teamState.followerLoading = false;
                    break;
                }

                case StartFetchTeamMemberAction _: {
                    state.teamState.memberLoading = true;
                    break;
                }

                case FetchTeamMemberSuccessAction action: {
                    state.teamState.memberLoading = false;
                    if (state.teamState.teamDict.ContainsKey(key: action.teamId)) {
                        var team = state.teamState.teamDict[key: action.teamId];
                        team.membersHasMore = action.membersHasMore;
                        if (action.pageNumber == 1) {
                            team.members = action.members;
                        }
                        else {
                            var members = team.members;
                            members.AddRange(collection: action.members);
                            team.members = members;
                        }

                        state.teamState.teamDict[key: action.teamId] = team;
                    }

                    break;
                }

                case FetchTeamMemberFailureAction _: {
                    state.teamState.memberLoading = false;
                    break;
                }

                case StartFetchFollowTeamAction action: {
                    if (state.teamState.teamDict.ContainsKey(key: action.followTeamId)) {
                        var team = state.teamState.teamDict[key: action.followTeamId];
                        team.followTeamLoading = true;
                        state.teamState.teamDict[key: action.followTeamId] = team;
                    }

                    break;
                }

                case FetchFollowTeamSuccessAction action: {
                    if (state.teamState.teamDict.ContainsKey(key: action.followTeamId)) {
                        var team = state.teamState.teamDict[key: action.followTeamId];
                        team.followTeamLoading = false;
                        state.teamState.teamDict[key: action.followTeamId] = team;
                    }

                    if (state.followState.followDict.ContainsKey(key: action.currentUserId)) {
                        var followMap = state.followState.followDict[key: action.currentUserId];
                        if (!followMap.ContainsKey(key: action.followTeamId)) {
                            followMap.Add(key: action.followTeamId, value: action.success);
                        }

                        state.followState.followDict[key: action.currentUserId] = followMap;
                    }
                    else {
                        var followMap = new Dictionary<string, bool>();
                        if (!followMap.ContainsKey(key: action.followTeamId)) {
                            followMap.Add(key: action.followTeamId, value: action.success);
                        }

                        state.followState.followDict.Add(key: action.currentUserId, value: followMap);
                    }

                    if (state.userState.userDict.ContainsKey(key: action.currentUserId)) {
                        var user = state.userState.userDict[key: action.currentUserId];
                        user.followingUsersCount += 1;
                        state.userState.userDict[key: action.currentUserId] = user;
                    }

                    if (state.teamState.teamDict.ContainsKey(key: action.followTeamId)) {
                        var team = state.teamState.teamDict[key: action.followTeamId];
                        if (team.stats != null) {
                            team.stats.followCount += 1;
                            state.teamState.teamDict[key: action.followTeamId] = team;
                        }
                    }

                    EventBus.publish(sName: EventBusConstant.follow_user, args: new List<object>());

                    break;
                }

                case FetchFollowTeamFailureAction action: {
                    if (state.teamState.teamDict.ContainsKey(key: action.followTeamId)) {
                        var team = state.teamState.teamDict[key: action.followTeamId];
                        team.followTeamLoading = false;
                        state.teamState.teamDict[key: action.followTeamId] = team;
                    }

                    break;
                }

                case StartFetchUnFollowTeamAction action: {
                    if (state.teamState.teamDict.ContainsKey(key: action.unFollowTeamId)) {
                        var team = state.teamState.teamDict[key: action.unFollowTeamId];
                        team.followTeamLoading = true;
                        state.teamState.teamDict[key: action.unFollowTeamId] = team;
                    }

                    break;
                }

                case FetchUnFollowTeamSuccessAction action: {
                    if (state.teamState.teamDict.ContainsKey(key: action.unFollowTeamId)) {
                        var team = state.teamState.teamDict[key: action.unFollowTeamId];
                        team.followTeamLoading = false;
                        state.teamState.teamDict[key: action.unFollowTeamId] = team;
                    }

                    if (state.followState.followDict.ContainsKey(key: action.currentUserId)) {
                        var followMap = state.followState.followDict[key: action.currentUserId];
                        if (followMap.ContainsKey(key: action.unFollowTeamId)) {
                            followMap.Remove(key: action.unFollowTeamId);
                        }

                        state.followState.followDict[key: action.currentUserId] = followMap;
                    }

                    if (state.userState.userDict.ContainsKey(key: action.currentUserId)) {
                        var user = state.userState.userDict[key: action.currentUserId];
                        user.followingUsersCount -= 1;
                        state.userState.userDict[key: action.currentUserId] = user;
                    }

                    if (state.teamState.teamDict.ContainsKey(key: action.unFollowTeamId)) {
                        var team = state.teamState.teamDict[key: action.unFollowTeamId];
                        if (team.stats != null) {
                            team.stats.followCount -= 1;
                            state.teamState.teamDict[key: action.unFollowTeamId] = team;
                        }
                    }

                    EventBus.publish(sName: EventBusConstant.follow_user, args: new List<object>());

                    break;
                }

                case FetchUnFollowTeamFailureAction action: {
                    if (state.teamState.teamDict.ContainsKey(key: action.unFollowTeamId)) {
                        var team = state.teamState.teamDict[key: action.unFollowTeamId];
                        team.followTeamLoading = false;
                        state.teamState.teamDict[key: action.unFollowTeamId] = team;
                    }

                    break;
                }

                case ChangeFeedbackTypeAction action: {
                    state.feedbackState.feedbackType = action.type;
                    break;
                }

                case StartFetchFavoriteTagAction _: {
                    state.favoriteState.favoriteTagLoading = true;
                    break;
                }

                case FetchFavoriteTagSuccessAction action: {
                    if (action.offset == 0) {
                        if (state.favoriteState.favoriteTagIdDict.ContainsKey(key: action.userId)) {
                            state.favoriteState.favoriteTagIdDict[key: action.userId] = action.favoriteTagIds;
                        }
                        else {
                            state.favoriteState.favoriteTagIdDict.Add(key: action.userId, value: action.favoriteTagIds);
                        }
                    }
                    else {
                        var oldFavoriteTagIds = state.favoriteState.favoriteTagIdDict[key: action.userId];
                        oldFavoriteTagIds.AddRange(collection: action.favoriteTagIds);
                        state.favoriteState.favoriteTagIdDict[key: action.userId] = oldFavoriteTagIds;
                    }

                    state.favoriteState.favoriteTagHasMore = action.hasMore;
                    state.favoriteState.favoriteTagLoading = false;
                    break;
                }

                case FetchFavoriteTagFailureAction _: {
                    state.favoriteState.favoriteTagLoading = false;
                    break;
                }

                case StartFetchFollowFavoriteTagAction _: {
                    state.favoriteState.followFavoriteTagLoading = true;
                    break;
                }

                case FetchFollowFavoriteTagSuccessAction action: {
                    if (action.offset == 0) {
                        state.favoriteState.followFavoriteTagIdDict = new Dictionary<string, List<string>> {
                            { action.userId, action.favoriteTagIds }
                        };
                    }
                    else {
                        var oldFavoriteTagIds = state.favoriteState.followFavoriteTagIdDict[key: action.userId];
                        oldFavoriteTagIds.AddRange(collection: action.favoriteTagIds);
                        state.favoriteState.followFavoriteTagIdDict[key: action.userId] = oldFavoriteTagIds;
                    }


                    state.favoriteState.followFavoriteTagHasMore = action.hasMore;
                    state.favoriteState.followFavoriteTagLoading = false;
                    break;
                }

                case FetchFollowFavoriteTagFailureAction _: {
                    state.favoriteState.followFavoriteTagLoading = false;
                    break;
                }

                case StartFetchFavoriteDetailAction _: {
                    state.favoriteState.favoriteDetailLoading = true;
                    break;
                }

                case FetchFavoriteDetailSuccessAction action: {
                    var favoriteDetailArticleIdDict = state.favoriteState.favoriteDetailArticleIdDict;
                    var tagId = action.tagId.isNotEmpty() ? action.tagId : $"{action.userId}all";
                    var favoriteDetailArticleIds = favoriteDetailArticleIdDict.ContainsKey(key: tagId)
                        ? favoriteDetailArticleIdDict[key: tagId]
                        : new List<string>();
                    if (action.offset == 0) {
                        favoriteDetailArticleIds.Clear();
                    }

                    if (action.favorites != null && action.favorites.isNotEmpty()) {
                        var articleDict = state.articleState.articleDict;
                        action.favorites.ForEach(favorite => {
                            favoriteDetailArticleIds.Add(item: favorite.itemId);
                            if (articleDict.ContainsKey(key: favorite.itemId)) {
                                var article = articleDict[key: favorite.itemId];

                                if (article.favorites == null) {
                                    article.favorites = new List<Favorite> { favorite };
                                }
                                else {
                                    if (!article.favorites.Contains(item: favorite)) {
                                        article.favorites.Add(item: favorite);
                                    }
                                }

                                articleDict[key: favorite.itemId] = article;
                            }
                        });

                        state.articleState.articleDict = articleDict;
                    }

                    if (favoriteDetailArticleIdDict.ContainsKey(key: tagId)) {
                        favoriteDetailArticleIdDict[key: tagId] = favoriteDetailArticleIds;
                    }
                    else {
                        favoriteDetailArticleIdDict.Add(key: tagId, value: favoriteDetailArticleIds);
                    }

                    state.favoriteState.favoriteDetailArticleIdDict = favoriteDetailArticleIdDict;
                    state.favoriteState.favoriteDetailHasMore = action.hasMore;
                    state.favoriteState.favoriteDetailLoading = false;
                    break;
                }

                case FetchFavoriteDetailFailureAction _: {
                    state.favoriteState.favoriteDetailLoading = false;
                    break;
                }

                case ChangeFavoriteTagStateAction action: {
                    state.leaderBoardState.detailCollectLoading = action.isLoading;
                    break;
                }

                case CollectFavoriteTagSuccessAction action: {
                    state.leaderBoardState.detailCollectLoading = false;

                    if (action.rankDataId.isNotEmpty() && state.leaderBoardState.rankDict.isNotEmpty() &&
                        state.leaderBoardState.rankDict.ContainsKey(key: action.rankDataId)) {
                        var rankData = state.leaderBoardState.rankDict[key: action.rankDataId];
                        rankData.myFavoriteTagId = action.myFavoriteTagId;
                        state.leaderBoardState.rankDict[key: action.myFavoriteTagId] = rankData;
                    }

                    if (state.loginState.isLoggedIn && action.itemId != null) {
                        if (state.favoriteState.collectedTagMap.isNotNullAndEmpty() &&
                            state.favoriteState.collectedTagMap.ContainsKey(key: state.loginState.loginInfo.userId)) {
                            var dict = state.favoriteState.collectedTagMap[key: state.loginState.loginInfo.userId];
                            if (dict.ContainsKey(key: action.itemId)) {
                                dict[key: action.itemId] = true;
                            }
                            else {
                                dict.Add(key: action.itemId, true);
                            }

                            state.favoriteState.collectedTagMap[key: state.loginState.loginInfo.userId] = dict;
                        }
                        else {
                            var collectedTagMap = new Dictionary<string, bool> {
                                { action.itemId, true }
                            };
                            state.favoriteState.collectedTagMap.Add(key: state.loginState.loginInfo.userId,
                                value: collectedTagMap);
                        }
                    }

                    if (action.itemId != action.tagId && action.tagId.isNotEmpty()) {
                        if (state.favoriteState.collectedTagChangeMap.isNotEmpty() &&
                            state.favoriteState.collectedTagChangeMap.ContainsKey(key: action.tagId)) {
                            state.favoriteState.collectedTagChangeMap[key: action.tagId] = action.myFavoriteTagId;
                        }
                        else {
                            state.favoriteState.collectedTagChangeMap.Add(key: action.tagId,
                                value: action.myFavoriteTagId);
                        }
                    }


                    break;
                }

                case CancelCollectFavoriteTagSuccessAction action: {
                    state.leaderBoardState.detailCollectLoading = false;
                    if (state.loginState.isLoggedIn && action.itemId != null) {
                        if (state.favoriteState.collectedTagMap.isNotNullAndEmpty() &&
                            state.favoriteState.collectedTagMap.ContainsKey(key: state.loginState.loginInfo.userId)) {
                            var dict = state.favoriteState.collectedTagMap[key: state.loginState.loginInfo.userId];
                            if (dict.ContainsKey(key: action.itemId)) {
                                dict.Remove(key: action.itemId);
                            }

                            state.favoriteState.collectedTagMap[key: state.loginState.loginInfo.userId] = dict;
                        }
                    }

                    break;
                }
                case FavoriteTagArticleMapAction action: {
                    if (action.favoriteTagArticleMap.isNotNullAndEmpty()) {
                        var favoriteTagArticleDict = state.favoriteState.favoriteTagArticleDict;
                        foreach (var keyValuePair in action.favoriteTagArticleMap) {
                            if (favoriteTagArticleDict.ContainsKey(key: keyValuePair.Key)) {
                                favoriteTagArticleDict[key: keyValuePair.Key] = keyValuePair.Value;
                            }
                            else {
                                favoriteTagArticleDict.Add(key: keyValuePair.Key, value: keyValuePair.Value);
                            }
                        }

                        state.favoriteState.favoriteTagArticleDict = favoriteTagArticleDict;
                    }

                    break;
                }

                case CollectedTagMapAction action: {
                    if (state.loginState.isLoggedIn && action.collectedTagMap.isNotNullAndEmpty()) {
                        if (state.favoriteState.collectedTagMap.isNotNullAndEmpty() &&
                            state.favoriteState.collectedTagMap.ContainsKey(key: state.loginState.loginInfo.userId)) {
                            state.favoriteState.collectedTagMap[key: state.loginState.loginInfo.userId] =
                                action.collectedTagMap;
                        }
                        else {
                            state.favoriteState.collectedTagMap.Add(key: state.loginState.loginInfo.userId,
                                value: action.collectedTagMap);
                        }
                    }

                    state.favoriteState.collectedTagChangeMap.Clear();

                    break;
                }

                case CreateFavoriteTagSuccessAction action: {
                    var currentUserId = state.loginState.loginInfo.userId ?? "";
                    var favoriteTagIds = new List<string>();
                    if (action.isCollection) {
                        favoriteTagIds = state.favoriteState.followFavoriteTagIdDict.ContainsKey(key: currentUserId)
                            ? state.favoriteState.followFavoriteTagIdDict[key: currentUserId]
                            : new List<string>();
                        if (!favoriteTagIds.Contains(item: action.favoriteTag.id)) {
                            favoriteTagIds.Add(item: action.favoriteTag.id);
                        }

                        state.favoriteState.followFavoriteTagIdDict[key: currentUserId] = favoriteTagIds;
                    }

                    favoriteTagIds = state.favoriteState.favoriteTagIdDict.ContainsKey(key: currentUserId)
                        ? state.favoriteState.favoriteTagIdDict[key: currentUserId]
                        : new List<string>();
                    if (!favoriteTagIds.Contains(item: action.favoriteTag.id)) {
                        if (favoriteTagIds.Count <= 1) {
                            favoriteTagIds.Add(item: action.favoriteTag.id);
                        }
                        else {
                            favoriteTagIds.Insert(1, item: action.favoriteTag.id);
                        }
                    }

                    state.favoriteState.favoriteTagIdDict[key: currentUserId] = favoriteTagIds;

                    if (state.favoriteState.favoriteTagDict.ContainsKey(key: action.favoriteTag.id)) {
                        state.favoriteState.favoriteTagDict[key: action.favoriteTag.id] = action.favoriteTag;
                    }
                    else {
                        state.favoriteState.favoriteTagDict.Add(key: action.favoriteTag.id, value: action.favoriteTag);
                    }

                    break;
                }

                case EditFavoriteTagSuccessAction action: {
                    if (state.favoriteState.favoriteTagDict.ContainsKey(key: action.favoriteTag.id)) {
                        state.favoriteState.favoriteTagDict[key: action.favoriteTag.id] = action.favoriteTag;
                    }
                    else {
                        state.favoriteState.favoriteTagDict.Add(key: action.favoriteTag.id, value: action.favoriteTag);
                    }

                    break;
                }

                case DeleteFavoriteTagSuccessAction action: {
                    var currentUserId = state.loginState.loginInfo.userId ?? "";
                    var favoriteTagIds = state.favoriteState.favoriteTagIdDict.ContainsKey(key: currentUserId)
                        ? state.favoriteState.favoriteTagIdDict[key: currentUserId]
                        : new List<string>();
                    var followFavoriteIds = state.favoriteState.followFavoriteTagIdDict.ContainsKey(key: currentUserId)
                        ? state.favoriteState.followFavoriteTagIdDict[key: currentUserId]
                        : new List<string>();
                    if (favoriteTagIds.Contains(item: action.favoriteTag.id)) {
                        favoriteTagIds.Remove(item: action.favoriteTag.id);
                    }

                    if (followFavoriteIds.Contains(item: action.favoriteTag.id)) {
                        followFavoriteIds.Remove(item: action.favoriteTag.id);
                    }

                    if (currentUserId.isNotEmpty() &&
                        state.favoriteState.collectedTagMap.ContainsKey(key: currentUserId)) {
                        var collectMap = state.favoriteState.collectedTagMap[key: currentUserId];
                        if (collectMap.ContainsKey(key: action.favoriteTag.quoteTagId)) {
                            collectMap.Remove(key: action.favoriteTag.quoteTagId);
                            state.favoriteState.collectedTagMap[key: currentUserId] = collectMap;
                        }
                    }

                    state.favoriteState.favoriteTagIdDict[key: currentUserId] = favoriteTagIds;
                    state.favoriteState.followFavoriteTagIdDict[key: currentUserId] = followFavoriteIds;

                    break;
                }

                case SwitchTabBarIndexAction action: {
                    state.tabBarState.currentTabIndex = action.index;
                    break;
                }

                case SwitchHomePageTabBarIndexAction action: {
                    state.tabBarState.currentHomePageTabIndex = action.index;
                    break;
                }

                case ChangeSwiperStatusAction action: {
                    state.articleState.swiperOnScreen = action.isShowing;
                    break;
                }

                case FetchLearnCourseListSuccessAction action: {
                    if (action.currentPage == 0) {
                        state.learnState.courses = action.courses;
                    }
                    else {
                        var courses = state.learnState.courses;
                        courses.AddRange(collection: action.courses);
                        state.learnState.courses = courses;
                    }

                    state.learnState.hasMore = action.hasMore;
                    break;
                }

                case QuestionAction action: {
                    if (action.questions.isNotNullAndEmpty()) {
                        var questionDict = state.qaState.questionDict;
                        action.questions.ForEach(question => {
                            if (questionDict.ContainsKey(key: question.id)) {
                                var oldQuestion = questionDict[key: question.id];
                                questionDict[key: question.id] = oldQuestion.Merge(other: question);
                            }
                            else {
                                questionDict.Add(key: question.id, value: question);
                            }
                        });
                        state.qaState.questionDict = questionDict;
                    }

                    break;
                }

                case FetchQuestionsSuccessAction action: {
                    var questionIds = action.questionIds;
                    var hasMore = action.hasMore;
                    var tab = action.tab;
                    var page = action.page;
                    if (tab == QATab.latest) {
                        state.qaState.latestQuestionListHasMore = hasMore;
                        if (page == 1) {
                            state.qaState.latestQuestionIds = questionIds;
                        }
                        else {
                            state.qaState.latestQuestionIds.AddRange(collection: questionIds);
                        }
                    }
                    else if (tab == QATab.hot) {
                        state.qaState.hotQuestionListHasMore = hasMore;
                        if (page == 1) {
                            state.qaState.hotQuestionIds = questionIds;
                        }
                        else {
                            state.qaState.hotQuestionIds.AddRange(collection: questionIds);
                        }
                    }
                    else if (tab == QATab.pending) {
                        state.qaState.pendingQuestionListHasMore = hasMore;
                        if (page == 1) {
                            state.qaState.pendingQuestionIds = questionIds;
                        }
                        else {
                            state.qaState.pendingQuestionIds.AddRange(collection: questionIds);
                        }
                    }

                    break;
                }

                case AnswerAction action: {
                    var answerDict = state.qaState.answerDict;
                    if (action.answers.isNotNullAndEmpty()) {
                        action.answers.ForEach(answer => {
                            if (answerDict.ContainsKey(key: answer.id)) {
                                var oldAnswer = answerDict[key: answer.id];
                                answerDict[key: answer.id] = oldAnswer.Merge(other: answer);
                            }
                            else {
                                answerDict.Add(key: answer.id, value: answer);
                            }
                        });
                        state.qaState.answerDict = answerDict;
                    }

                    break;
                }

                case AnswerDraftAction action: {
                    var answerDraftDict = state.qaEditorState.answerDraftDict;
                    var drafts = action.drafts;
                    if (drafts.isNotNullAndEmpty()) {
                        drafts.ForEach(draft => { answerDraftDict[key: draft.questionId] = draft; });
                        state.qaEditorState.answerDraftDict = answerDraftDict;
                    }

                    break;
                }

                case FetchQuestionDetailSuccessAction action: {
                    var questionId = action.questionId;
                    var questionDict = state.qaState.questionDict;
                    var question = action.question;

                    if (questionId.isNotEmpty() && action.answerIds != null) {
                        state.qaState.answerIdsDict[key: questionId] = action.answerIds;
                        var hasMore = question.answerCount > action.answerIds.Count;
                        state.qaState.answerListHasMoreDict[key: questionId] = hasMore;
                    }

                    if (question != null) {
                        if (questionDict.ContainsKey(key: question.id)) {
                            questionDict[key: question.id] = question;
                        }
                        else {
                            questionDict.Add(key: question.id, value: question);
                        }

                        state.qaState.questionDict = questionDict;
                    }

                    break;
                }

                case FetchAnswersSuccessAction action: {
                    var questionId = action.questionId;
                    if (questionId.isNotEmpty() && action.answerIds.isNotNullAndEmpty()) {
                        state.qaState.answerIdsDict.TryGetValue(key: questionId, out var answerIds);
                        if (answerIds.isNullOrEmpty()) {
                            answerIds = new List<string>();
                        }

                        if (action.page == 1) {
                            answerIds = action.answerIds;
                        }
                        else {
                            answerIds.AddRange(collection: action.answerIds);
                        }

                        state.qaState.answerIdsDict[key: questionId] = answerIds;
                        state.qaState.answerListHasMoreDict[key: questionId] = action.hasMore;
                    }

                    break;
                }

                case FetchAnswerDetailSuccessAction action: {
                    if (action.channelId.isNotEmpty() && action.messageIds.isNotNullAndEmpty()) {
                        state.qaState.messageToplevelSimpleListDict[key: action.channelId] = action.messageIds;
                    }

                    break;
                }

                case FetchAnswerDetailFailureAction action: {
                    break;
                }

                case FetchQATopCommentSuccessAction action: {
                    var channelId = action.channelId;
                    var messageIds = action.messageIds;
                    var hasMore = action.hasMore;
                    var childMessageMap = action.childMessageMap;

                    if (channelId.isNotEmpty() && messageIds.isNotNullAndEmpty()) {
                        var messageToplevelListDict = state.qaState.messageToplevelListDict;
                        if (action.after.isNotEmpty()) {
                            var messageList =
                                messageToplevelListDict.GetValueOrDefault(key: channelId, new NewMessageList());
                            messageList.list.AddRange(collection: messageIds);
                            messageList.hasMore = hasMore;
                            messageList.lastId = messageList.list.last() ?? "";
                            messageToplevelListDict[key: channelId] = messageList;
                        }
                        else {
                            state.qaState.messageToplevelListDict[key: channelId] = new NewMessageList {
                                list = messageIds,
                                lastId = messageIds.last(),
                                hasMore = hasMore
                            };
                        }
                    }

                    var messageSecondLevelSimpleListDict =
                        state.qaState.messageSecondLevelSimpleListDict;
                    if (childMessageMap.isNotNullAndEmpty()) {
                        foreach (var topLevelMessageId in childMessageMap.Keys) {
                            messageSecondLevelSimpleListDict[key: topLevelMessageId] = new NewMessageList {
                                list = childMessageMap[key: topLevelMessageId].list
                            };
                        }

                        state.qaState.messageSecondLevelSimpleListDict = messageSecondLevelSimpleListDict;
                    }

                    break;
                }

                case FetchQATopCommentFailureAction _: {
                    break;
                }

                case FetchQACommentDetailSuccessAction action: {
                    var messageId = action.messageId;
                    var messageIds = action.messageIds;
                    var hasMore = action.hasMore;

                    if (messageId.isNotEmpty() && messageIds.isNotNullAndEmpty()) {
                        var messageSecondLevelListDict =
                            state.qaState.messageSecondLevelListDict;
                        if (action.after.isNotEmpty()) {
                            var messageList =
                                messageSecondLevelListDict.GetValueOrDefault(key: messageId, new NewMessageList());
                            messageList.list.AddRange(collection: messageIds);
                            messageList.hasMore = hasMore;
                            messageList.lastId = messageList.list.last() ?? "";
                            state.qaState.messageSecondLevelListDict[key: messageId] = messageList;
                        }
                        else {
                            messageSecondLevelListDict[key: messageId] = new NewMessageList {
                                list = messageIds,
                                lastId = messageIds.last(),
                                hasMore = hasMore
                            };
                        }
                    }

                    break;
                }

                case FetchQACommentDetailFailureAction _: {
                    break;
                }

                case SendQAMessageSuccessAction action: {
                    var type = action.type;
                    var itemId = action.itemId;
                    var messageId = action.messageId;
                    if (type == QAMessageType.question) {
                        var questionDict = state.qaState.questionDict;
                        var question = state.qaState.questionDict.GetValueOrDefault(key: itemId, new Question());
                        question.commentCount += 1;
                        state.qaState.questionDict = questionDict;
                    }
                    else if (type == QAMessageType.answer) {
                        var answerDict = state.qaState.answerDict;
                        var answer = state.qaState.answerDict.GetValueOrDefault(key: itemId, new Answer());
                        answer.commentCount += 1;
                        state.qaState.answerDict = answerDict;
                    }

                    break;
                }

                case SendQAMessageFailureAction _: {
                    break;
                }

                case LikeSuccessAction action: {
                    var likeType = action.likeType;
                    var like = action.like;
                    var likeDict = state.qaState.likeDict;
                    if (likeType == QALikeType.question) {
                        var questionId = like.itemId;
                        var questionDict = state.qaState.questionDict;
                        if (!likeDict.ContainsKey(key: questionId)) {
                            likeDict[key: questionId] = like;
                            if (questionDict.ContainsKey(key: questionId)) {
                                var question = questionDict.GetValueOrDefault(key: questionId, new Question());
                                question.likeCount += 1;
                                questionDict[key: questionId] = question;
                            }

                            state.qaState.likeDict = likeDict;
                            state.qaState.questionDict = questionDict;
                        }
                    }
                    else if (likeType == QALikeType.answer) {
                        var answerId = like.itemId;
                        var answerDict = state.qaState.answerDict;
                        if (!likeDict.ContainsKey(key: answerId)) {
                            likeDict[key: answerId] = like;
                            if (answerDict.ContainsKey(key: answerId)) {
                                var answer = answerDict.GetValueOrDefault(key: answerId, new Answer());
                                answer.likeCount += 1;
                                answerDict[key: answerId] = answer;
                            }

                            state.qaState.likeDict = likeDict;
                            state.qaState.answerDict = answerDict;
                        }
                    }
                    else if (likeType == QALikeType.message) {
                        var messageId = like.itemId;
                        var messageDict = state.qaState.messageDict;
                        if (like != null && !likeDict.ContainsKey(key: messageId)) {
                            likeDict[key: messageId] = like;
                            if (messageDict.ContainsKey(key: messageId)) {
                                var message = messageDict.GetValueOrDefault(key: messageId, new NewMessage());
                                message.likeCount += 1;
                                messageDict[key: messageId] = message;
                            }

                            state.qaState.likeDict = likeDict;
                            state.qaState.messageDict = messageDict;
                        }
                    }

                    break;
                }

                case LikeFailureAction _: {
                    break;
                }

                case RemoveLikeSuccessAction action: {
                    var likeType = action.likeType;
                    var likeDict = state.qaState.likeDict;
                    if (likeType == QALikeType.question) {
                        var questionId = action.itemId;
                        var questionDict = state.qaState.questionDict;
                        if (questionId.isNotEmpty() && likeDict.ContainsKey(key: questionId)) {
                            likeDict.Remove(key: questionId);
                            if (questionDict.ContainsKey(key: questionId)) {
                                var question = questionDict.GetValueOrDefault(key: questionId, new Question());
                                question.likeCount -= 1;
                                questionDict[key: questionId] = question;
                            }

                            state.qaState.likeDict = likeDict;
                            state.qaState.questionDict = questionDict;
                        }
                    }
                    else if (likeType == QALikeType.answer) {
                        var answerId = action.itemId;
                        var answerDict = state.qaState.answerDict;
                        if (answerId.isNotEmpty() && likeDict.ContainsKey(key: answerId)) {
                            likeDict.Remove(key: answerId);
                            if (answerDict.ContainsKey(key: answerId)) {
                                var answer = answerDict.GetValueOrDefault(key: answerId, new Answer());
                                answer.likeCount -= 1;
                                answerDict[key: answerId] = answer;
                            }

                            state.qaState.likeDict = likeDict;
                            state.qaState.answerDict = answerDict;
                        }
                    }
                    else if (likeType == QALikeType.message) {
                        var messageId = action.itemId;
                        var messageDict = state.qaState.messageDict;
                        if (messageId.isNotEmpty() && likeDict.ContainsKey(key: messageId)) {
                            likeDict.Remove(key: messageId);
                            if (messageDict.ContainsKey(key: messageId)) {
                                var message = messageDict.GetValueOrDefault(key: messageId, new NewMessage());
                                message.likeCount -= 1;
                                messageDict[key: messageId] = message;
                            }

                            state.qaState.likeDict = likeDict;
                            state.qaState.messageDict = messageDict;
                        }
                    }

                    break;
                }

                case RemoveLikeFailureAction _: {
                    break;
                }


                case QAImageAction action: {
                    var contentMap = action.contentMap;
                    if (contentMap.isNotNullAndEmpty()) {
                        var imageDict = state.qaState.imageDict;
                        foreach (var key in contentMap.Keys) {
                            imageDict[key: key] = contentMap[key: key].originalImage.url;
                        }

                        state.qaState.imageDict = imageDict;
                    }

                    break;
                }

                case NewMessageAction action: {
                    var messageMap = action.messageMap;
                    if (messageMap.isNotNullAndEmpty()) {
                        var messageDict = state.qaState.messageDict;
                        foreach (var key in messageMap.Keys) {
                            messageDict[key: key] = messageMap[key: key];
                        }

                        state.qaState.messageDict = messageDict;
                    }

                    break;
                }

                case QALikeAction action: {
                    var likeMap = action.likeMap;
                    if (likeMap.isNotNullAndEmpty()) {
                        var likeDict = state.qaState.likeDict;
                        foreach (var key in likeMap.Keys) {
                            likeDict[key: key] = likeMap[key: key];
                        }

                        state.qaState.likeDict = likeDict;
                    }

                    break;
                }

                case NextAnswerIdAction action: {
                    var answerId = action.answerId;
                    var nextAnswerId = action.nextAnswerId;
                    var nextAnswerIdDict = state.qaState.nextAnswerIdDict;
                    nextAnswerIdDict[key: answerId] = nextAnswerId;
                    state.qaState.nextAnswerIdDict = nextAnswerIdDict;
                    break;
                }

                case StartCreateQuestionDraftAction _: {
                    state.qaEditorState.createQuestionDraftLoading = true;
                    state.qaEditorState.questionDraft = null;
                    break;
                }

                case CreateQuestionDraftSuccessAction action: {
                    state.qaEditorState.createQuestionDraftLoading = false;
                    state.qaEditorState.questionDraft = action.questionDraft;
                    break;
                }

                case CreateQuestionDraftFailureAction _: {
                    state.qaEditorState.createQuestionDraftLoading = false;
                    break;
                }

                case FetchAllPlatesSuccessAction action: {
                    state.qaEditorState.plates = action.plates;
                    break;
                }

                case ChangeQuestionPlateAction action: {
                    state.qaEditorState.questionDraft.plate = action.plate;
                    break;
                }

                case StartFetchQuestionDraftAction _: {
                    state.qaEditorState.fetchQuestionDraftLoading = true;
                    state.qaEditorState.questionDraft = null;
                    break;
                }

                case FetchQuestionDraftSuccessAction action: {
                    state.qaEditorState.fetchQuestionDraftLoading = false;
                    state.qaEditorState.questionDraft = action.questionDraft;
                    break;
                }

                case FetchQuestionDraftFailureAction _: {
                    state.qaEditorState.fetchQuestionDraftLoading = false;
                    break;
                }

                case UploadQuestionImageSuccessAction action: {
                    var draft = state.qaEditorState.questionDraft;
                    var imageDict = state.qaEditorState.uploadImageDict;
                    if (draft != null) {
                        if (draft.contentIds.isNullOrEmpty()) {
                            draft.contentIds = new List<string> { action.contentId };
                        }
                        else {
                            draft.contentIds.Add(item: action.contentId);
                        }

                        state.qaEditorState.questionDraft = draft;
                        imageDict[key: action.tmpId] = action.contentId;
                        state.qaEditorState.uploadImageDict = imageDict;
                    }

                    break;
                }

                case UpdateQuestionTagsSuccessAction action: {
                    state.qaEditorState.questionDraft.tagIds = action.tagList;
                    break;
                }

                case RestQuestionDraftAction _: {
                    state.qaEditorState.createQuestionDraftLoading = false;
                    state.qaEditorState.fetchQuestionDraftLoading = false;
                    state.qaEditorState.questionDraft = null;
                    break;
                }

                case UpdateQuestionSuccessAction action: {
                    state.qaEditorState.questionDraft = action.questionDraft;
                    break;
                }

                case PublishQuestionSuccessAction action: {
                    state.qaEditorState.questionDraft = action.questionDraft;
                    break;
                }

                case StartCreateAnswerAction _: {
                    state.qaEditorState.createAnswerDraftLoading = true;
                    break;
                }

                case CreateAnswerSuccessAction action: {
                    state.qaEditorState.createAnswerDraftLoading = false;
                    state.qaEditorState.answerDraft = action.answerDraft;
                    break;
                }

                case CreateAnswerFailureAction _: {
                    state.qaEditorState.createAnswerDraftLoading = false;
                    break;
                }

                case StartFetchAnswerDraftAction _: {
                    state.qaEditorState.fetchAnswerDraftLoading = true;
                    break;
                }

                case FetchAnswerDraftSuccessAction action: {
                    state.qaEditorState.fetchAnswerDraftLoading = false;
                    state.qaEditorState.answerDraft = action.answerDraft;

                    break;
                }

                case FetchAnswerDraftFailureAction _: {
                    state.qaEditorState.fetchAnswerDraftLoading = false;
                    break;
                }

                case UploadAnswerImageSuccessAction action: {
                    var draft = state.qaEditorState.answerDraft;
                    var imageDict = state.qaEditorState.uploadImageDict;
                    if (draft != null) {
                        if (draft.contentIds.isNullOrEmpty()) {
                            draft.contentIds = new List<string> { action.contentId };
                        }
                        else {
                            draft.contentIds.Add(item: action.contentId);
                        }

                        state.qaEditorState.answerDraft = draft;
                        imageDict[key: action.tmpId] = action.contentId;
                        state.qaEditorState.uploadImageDict = imageDict;
                    }

                    break;
                }

                case UpdateAnswerSuccessAction action: {
                    state.qaEditorState.answerDraft = action.answerDraft;
                    break;
                }

                case PublishAnswerSuccessAction action: {
                    state.qaEditorState.answerDraft = action.answerDraft;
                    break;
                }

                case RemoveAnswerDraftAction action: {
                    var questionId = action.questionId;
                    var answerDraftDict = state.qaEditorState.answerDraftDict;
                    if (questionId.isNotEmpty() && answerDraftDict.ContainsKey(key: questionId)) {
                        state.qaEditorState.answerDraftDict.Remove(key: questionId);
                    }

                    break;
                }

                case ResetAnswerDraftAction _: {
                    state.qaEditorState.answerDraft = null;
                    break;
                }

                case AllowAnswerQuestionAction action: {
                    var questionId = action.questionId;
                    var canAnswer = action.canAnswer;
                    var canAnswerDict = state.qaEditorState.canAnswerDict;
                    if (questionId.isNotEmpty()) {
                        canAnswerDict[key: questionId] = canAnswer;
                    }

                    break;
                }

                case FetchUserQuestionsSuccessAction action: {
                    state.qaState.userQuestionListHasMore = action.hasMore;
                    var userId = action.userId;
                    var userQuestions =
                        state.qaState.userQuestionsDict.GetValueOrDefault(key: userId, new List<string>());
                    if (action.page == 1) {
                        userQuestions = action.questionIds;
                    }
                    else {
                        userQuestions.AddRange(collection: action.questionIds);
                    }

                    state.qaState.userQuestionsDict[key: userId] = userQuestions;
                    break;
                }

                case FetchUserAnswersSuccessAction action: {
                    state.qaState.userAnswerListHasMore = action.hasMore;
                    var userId = action.userId;
                    var userAnswers = state.qaState.userAnswersDict.GetValueOrDefault(key: userId, new List<string>());
                    if (action.page == 1) {
                        userAnswers = action.answerIds;
                    }
                    else {
                        userAnswers.AddRange(collection: action.answerIds);
                    }

                    state.qaState.userAnswersDict[key: userId] = userAnswers;
                    break;
                }

                case FetchUserAnswerDraftsSuccessAction action: {
                    state.qaEditorState.userAnswerDraftListHasMore = action.hasMore;
                    var userAnswerDraftIds = state.qaEditorState.userAnswerDraftIds;
                    if (action.page == 1) {
                        userAnswerDraftIds = action.answerDraftIds;
                    }
                    else {
                        userAnswerDraftIds.AddRange(collection: action.answerDraftIds);
                    }

                    state.qaEditorState.userAnswerDraftIds = userAnswerDraftIds;
                    break;
                }

                case DeleterAnswerDraftSuccessAction action: {
                    var questionId = action.questionId;
                    var answerDraftIds = state.qaEditorState.userAnswerDraftIds;
                    var answerDraftDict = state.qaEditorState.answerDraftDict;
                    if (answerDraftIds.Contains(item: questionId)) {
                        answerDraftIds.Remove(item: questionId);
                    }

                    if (answerDraftDict.ContainsKey(key: questionId)) {
                        answerDraftDict.Remove(key: questionId);
                    }

                    break;
                }
            }

            return state;
        }
    }
}