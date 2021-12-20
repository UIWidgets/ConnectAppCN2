using System.Collections.Generic;
using System.Web;
using ConnectApp.Common.Util;
using ConnectApp.Models.Api;
using ConnectApp.Models.Model;
using Newtonsoft.Json;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;

namespace ConnectApp.Api {
    public static class SearchApi {
        public static Future<List<PopularSearch>> PopularSearchArticle() {
            var url = CStringUtils.genApiUrl("/search/popularSearch");
            var para = new Dictionary<string, object> {
                {"searchType", "project"}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<List<PopularSearch>>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var popularSearch = JsonConvert.DeserializeObject<List<PopularSearch>>(value: responseText);
                return FutureOr.value(value: popularSearch);
            });
        }

        public static Future<List<PopularSearch>> PopularSearchUser() {
            var url = CStringUtils.genApiUrl("/search/popularSearch");
            var para = new Dictionary<string, object> {
                {"searchType", "user"}
            };
            var request = HttpManager.GET(url: url,
                parameter: para);
            return HttpManager.resume(request: request).then_<List<PopularSearch>>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var popularSearch = JsonConvert.DeserializeObject<List<PopularSearch>>(value: responseText);
                return FutureOr.value(value: popularSearch);
            });
        }

        public static Future<FetchSearchArticleResponse> SearchArticle(string keyword, int pageNumber) {
            var url = CStringUtils.genApiUrl($"/search/projects");
            var para = new Dictionary<string, object> {
                {"q", HttpUtility.UrlEncode(str: keyword)},
                {"page", pageNumber}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchSearchArticleResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var searchResponse = JsonConvert.DeserializeObject<FetchSearchArticleResponse>(value: responseText);
                return FutureOr.value(value: searchResponse);
            });
        }

        public static Future<FetchSearchUserResponse> SearchUser(string keyword, int pageNumber) {
            var url = CStringUtils.genApiUrl($"/search/users");
            var para = new Dictionary<string, object> {
                {"q", HttpUtility.UrlEncode(str: keyword)},
                {"page", pageNumber}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchSearchUserResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var searchUserResponse = JsonConvert.DeserializeObject<FetchSearchUserResponse>(value: responseText);
                return FutureOr.value(value: searchUserResponse);
            });
        }

        public static Future<FetchSearchTeamResponse> SearchTeam(string keyword, int pageNumber) {
            var url = CStringUtils.genApiUrl("/search/teams");
            var para = new Dictionary<string, object> {
                {"q", HttpUtility.UrlEncode(str: keyword)},
                {"page", pageNumber}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchSearchTeamResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var searchTeamResponse = JsonConvert.DeserializeObject<FetchSearchTeamResponse>(value: responseText);
                return FutureOr.value(value: searchTeamResponse);
            });
        }

        public static Future<SearchQuestionsResponse> SearchQuestions(string keyword, int pageNumber) {
            var url = CStringUtils.genApiUrl($"/search/questions");
            var para = new Dictionary<string, object> {
                {"q", HttpUtility.UrlEncode(str: keyword)},
                {"page", pageNumber}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<SearchQuestionsResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var searchResponse = JsonConvert.DeserializeObject<SearchQuestionsResponse>(value: responseText);
                return FutureOr.value(value: searchResponse);
            });
        }

        public static Future<SearchTagsResponse> SearchTags(string keyword) {
            var url = CStringUtils.genApiUrl("/ask/search/skillSuggest");
            var para = new Dictionary<string, object> {
                {"q", HttpUtility.UrlEncode(str: keyword)}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<SearchTagsResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var searchResponse = JsonConvert.DeserializeObject<SearchTagsResponse>(value: responseText);
                return FutureOr.value(value: searchResponse);
            });
        }
    }
}