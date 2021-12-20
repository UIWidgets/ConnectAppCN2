using System.Collections.Generic;
using ConnectApp.Common.Constant;
using ConnectApp.Common.Util;
using ConnectApp.Models.Api;
using Newtonsoft.Json;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;

namespace ConnectApp.Api {
    public static class TeamApi {
        public static Future<FetchTeamResponse> FetchTeam(string teamId) {
            var url = CStringUtils.genApiUrl($"/teams/{teamId}");
            var request = HttpManager.GET(url: url);
            return HttpManager.resume(request: request).then_<FetchTeamResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var teamResponse = JsonConvert.DeserializeObject<FetchTeamResponse>(value: responseText);
                return FutureOr.value(value: teamResponse);
            });
        }

        public static Future<FetchTeamArticleResponse> FetchTeamArticle(string teamId, int pageNumber) {
            var url = CStringUtils.genApiUrl($"/teams/{teamId}/projects");
            var para = new Dictionary<string, object> {
                {"page", pageNumber}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchTeamArticleResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var teamArticleResponse = JsonConvert.DeserializeObject<FetchTeamArticleResponse>(value: responseText);
                return FutureOr.value(value: teamArticleResponse);
            });
        }

        public static Future<FetchFollowerResponse> FetchTeamFollower(string teamId, int offset) {
            var url = CStringUtils.genApiUrl($"/teams/{teamId}/followers");
            var para = new Dictionary<string, object> {
                {"teamId", teamId},
                {"offset", offset}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchFollowerResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var followerResponse = JsonConvert.DeserializeObject<FetchFollowerResponse>(value: responseText);
                return FutureOr.value(value: followerResponse);
            });
        }

        public static Future<FetchTeamMemberResponse> FetchTeamMember(string teamId, int pageNumber) {
            var url = CStringUtils.genApiUrl($"/teams/{teamId}/members");
            var para = new Dictionary<string, object> {
                {"page", pageNumber}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchTeamMemberResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var teamMemberResponse = JsonConvert.DeserializeObject<FetchTeamMemberResponse>(value: responseText);
                return FutureOr.value(value: teamMemberResponse);
            });
        }

        public static Future<bool> FetchFollowTeam(string teamId) {
            var url = CStringUtils.genApiUrl("/follow");
            var para = new FollowParameter {
                type = "team",
                followeeId = teamId
            };
            var request = HttpManager.POST(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<bool>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var followResponse = JsonConvert.DeserializeObject<Dictionary<string, bool>>(value: responseText);
                var follow = followResponse.GetValueOrDefault("success", false);
                return FutureOr.value(value: follow);
            });
        }

        public static Future<bool> FetchUnFollowTeam(string teamId) {
            var url = CStringUtils.genApiUrl("/unfollow");
            var para = new FollowParameter {
                followeeId = teamId
            };
            var request = HttpManager.POST($"{Config.apiAddress_cn}{Config.apiPath}/unfollow", parameter: para);
            return HttpManager.resume(request: request).then_<bool>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var unFollowResponse = JsonConvert.DeserializeObject<Dictionary<string, bool>>(value: responseText);
                var unfollow = unFollowResponse.GetValueOrDefault("success", false);
                return FutureOr.value(value: unfollow);
            });
        }
    }
}