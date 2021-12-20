using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Models.Api;
using ConnectApp.screens;
using Newtonsoft.Json;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;

namespace ConnectApp.Api {
    public static class LeaderBoardApi {
        public static Future<FetchLeaderBoardCollectionResponse> FetchLeaderBoardCollection(int page) {
            var url = CStringUtils.genApiUrl("/rankList/collection");
            var para = new Dictionary<string, object> {
                {"page", page}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchLeaderBoardCollectionResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var collectionResponse =
                    JsonConvert.DeserializeObject<FetchLeaderBoardCollectionResponse>(value: responseText);
                return FutureOr.value(value: collectionResponse);
            });
        }

        public static Future<FetchLeaderBoardColumnResponse> FetchLeaderBoardColumn(int page) {
            var url = CStringUtils.genApiUrl("/rankList/column");
            var para = new Dictionary<string, object> {
                {"page", page}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchLeaderBoardColumnResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var columnResponse = JsonConvert.DeserializeObject<FetchLeaderBoardColumnResponse>(value: responseText);
                return FutureOr.value(value: columnResponse);
            });
        }

        public static Future<FetchBloggerResponse> FetchLeaderBoardBlogger(int page) {
            var url = CStringUtils.genApiUrl("/rankList/blogger");
            var para = new Dictionary<string, object> {
                {"page", page}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchBloggerResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var bloggerResponse = JsonConvert.DeserializeObject<FetchBloggerResponse>(value: responseText);
                return FutureOr.value(value: bloggerResponse);
            });
        }

        public static Future<FetchBloggerResponse> FetchHomeBlogger(int page) {
            var url = CStringUtils.genApiUrl("/rankList/homeBlogger");
            var para = new Dictionary<string, object> {
                {"page", page}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchBloggerResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var homeBloggerResponse = JsonConvert.DeserializeObject<FetchBloggerResponse>(value: responseText);
                return FutureOr.value(value: homeBloggerResponse);
            });
        }

        public static Future<FetchLeaderBoardDetailResponse> FetchLeaderBoardDetail(
            string tagId, int page, LeaderBoardType leaderBoardType) {
            var url = CStringUtils.genApiUrl($"/rankList/{leaderBoardType.ToString()}/{tagId}");
            var para = new Dictionary<string, object> {
                {"page", page}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchLeaderBoardDetailResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var detailResponse = JsonConvert.DeserializeObject<FetchLeaderBoardDetailResponse>(value: responseText);
                return FutureOr.value(value: detailResponse);
            });
        }
    }
}