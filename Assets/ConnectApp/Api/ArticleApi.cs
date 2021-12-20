using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Models.Api;
using ConnectApp.Models.Model;
using Newtonsoft.Json;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;

namespace ConnectApp.Api {
    public static class ArticleApi {
        public static Future<FetchArticlesResponse> FetchArticles(string userId, int offset, bool random) {
            var url = CStringUtils.genApiUrl("/recommends");
            var para = new Dictionary<string, object> {
                {"language", "zh_CN"},
                {"hottestOffset", offset},
                {"random", random},
                {"needRank", true},
                {"afterTime", HistoryManager.homeAfterTime(userId: userId)}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchArticlesResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var articlesResponse = JsonConvert.DeserializeObject<FetchArticlesResponse>(value: responseText);
                return FutureOr.value(value: articlesResponse);
            });
        }

        public static Future<FetchFollowArticlesResponse> FetchFollowArticles(int pageNumber, string beforeTime,
            string afterTime, bool isFirst, bool isHot) {
            var url = CStringUtils.genApiUrl("/feeds");
            Dictionary<string, object> para;
            if (isFirst) {
                para = null;
            }
            else {
                if (isHot) {
                    para = new Dictionary<string, object> {
                        {"hotPage", pageNumber}
                    };
                }
                else {
                    para = new Dictionary<string, object> {
                        {"beforeTime", beforeTime},
                        {"afterTime", afterTime}
                    };
                }
            }

            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchFollowArticlesResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var followArticlesResponse =
                    JsonConvert.DeserializeObject<FetchFollowArticlesResponse>(value: responseText);
                return FutureOr.value(value: followArticlesResponse);
            });
        }

        public static Future<FetchArticleDetailResponse> FetchArticleDetail(string articleId, bool isPush = false) {
            var url = CStringUtils.genApiUrl($"/p/{articleId}");
            var para = new Dictionary<string, object> {
                {"view", "true"}
            };
            if (isPush) {
                para.Add("isPush", "true");
            }

            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchArticleDetailResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var articleDetailResponse =
                    JsonConvert.DeserializeObject<FetchArticleDetailResponse>(value: responseText);
                return FutureOr.value(value: articleDetailResponse);
            });
        }

        public static Future<FetchCommentsResponse> FetchArticleComments(string channelId, string currOldestMessageId) {
            var url = CStringUtils.genApiUrl($"/channels/{channelId}/messages");
            var para = new Dictionary<string, object> {
                {"limit", 5}
            };
            if (currOldestMessageId != null && currOldestMessageId.isNotEmpty()) {
                para.Add("before", value: currOldestMessageId);
            }

            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchCommentsResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var responseComments = JsonConvert.DeserializeObject<FetchCommentsResponse>(value: responseText);
                return FutureOr.value(value: responseComments);
            });
        }

        public static Future LikeArticle(string articleId, int addCount) {
            var url = CStringUtils.genApiUrl($"/like?isAppRepeatLike=true&addCount={addCount}");
            var para = new HandleArticleParameter {
                type = "project",
                itemId = articleId
            };
            var request = HttpManager.POST(url: url, parameter: para);
            return HttpManager.resume(request: request).then_(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                return FutureOr.nil;
            });
        }

        public static Future<List<Favorite>> FavoriteArticle(string articleId, List<string> tagIds) {
            var url = CStringUtils.genApiUrl("/favorites");
            var para = new HandleArticleParameter {
                type = "article",
                itemId = articleId,
                tagIds = tagIds
            };
            var request = HttpManager.POST(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<List<Favorite>>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var favoriteArticleResponse = JsonConvert.DeserializeObject<List<Favorite>>(value: responseText);
                return FutureOr.value(value: favoriteArticleResponse);
            });
        }

        public static Future<Favorite> UnFavoriteArticle(string favoriteId) {
            var url = CStringUtils.genApiUrl("/unfavorite");
            var para = new Dictionary<string, object> {
                {"id", favoriteId}
            };
            var request = HttpManager.POST(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<Favorite>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var unFavoriteArticleResponse = JsonConvert.DeserializeObject<Favorite>(value: responseText);
                return FutureOr.value(value: unFavoriteArticleResponse);
            });
        }

        public static Future<Message> LikeComment(string commentId) {
            var url = CStringUtils.genApiUrl($"/messages/{commentId}/addReaction");
            var para = new ReactionParameter {
                reactionType = "like"
            };
            var request = HttpManager.POST(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<Message>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var message = JsonConvert.DeserializeObject<Message>(value: responseText);
                return FutureOr.value(value: message);
            });
        }

        public static Future<Message> RemoveLikeComment(string commentId) {
            var url = CStringUtils.genApiUrl($"/messages/{commentId}/removeReaction");
            var para = new ReactionParameter {
                reactionType = "like"
            };
            var request = HttpManager.POST(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<Message>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var message = JsonConvert.DeserializeObject<Message>(value: responseText);
                return FutureOr.value(value: message);
            });
        }


        public static Future<Message> SendComment(string channelId, string content, string nonce,
            string parentMessageId = "", string upperMessageId = "") {
            var url = CStringUtils.genApiUrl($"/channels/{channelId}/messages");
            var para = new SendCommentParameter {
                content = content,
                parentMessageId = parentMessageId,
                upperMessageId = upperMessageId,
                nonce = nonce,
                app = true
            };
            var request = HttpManager.POST(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<Message>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var message = JsonConvert.DeserializeObject<Message>(value: responseText);
                return FutureOr.value(value: message);
            });
        }
    }
}