using System.Collections.Generic;
using ConnectApp.Common.Constant;
using ConnectApp.Common.Util;
using ConnectApp.Models.Api;
using Newtonsoft.Json;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;

namespace ConnectApp.Api {
    public static class UserApi {
        public static Future<FetchUserProfileResponse> FetchUserProfile(string userId) {
            var url = CStringUtils.genApiUrl($"/u/{userId}");
            var request = HttpManager.GET(url: url);
            return HttpManager.resume(request: request).then_<FetchUserProfileResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var userProfileResponse = JsonConvert.DeserializeObject<FetchUserProfileResponse>(value: responseText);
                return FutureOr.value(value: userProfileResponse);
            });
        }

        public static Future<FetchUserArticleResponse> FetchUserArticle(string userId, int pageNumber) {
            var url = CStringUtils.genApiUrl($"/u/{userId}/activities");
            var para = new Dictionary<string, object> {
                {"page", pageNumber},
                {"type", "article"}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchUserArticleResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var userArticleResponse = JsonConvert.DeserializeObject<FetchUserArticleResponse>(value: responseText);
                return FutureOr.value(value: userArticleResponse);
            });
        }

        public static Future<FetchUserLikeArticleResponse> FetchUserLikeArticle(string userId, int pageNumber) {
            var url = CStringUtils.genApiUrl($"/u/{userId}/likes");
            var para = new Dictionary<string, object> {
                {"page", pageNumber}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchUserLikeArticleResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var userArticleResponse =
                    JsonConvert.DeserializeObject<FetchUserLikeArticleResponse>(value: responseText);
                return FutureOr.value(value: userArticleResponse);
            });
        }

        public static Future<bool> FetchFollowUser(string userId) {
            var url = CStringUtils.genApiUrl("/follow");
            var para = new FollowParameter {
                type = "user",
                followeeId = userId
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

        public static Future<bool> FetchUnFollowUser(string userId) {
            var url = CStringUtils.genApiUrl("/unfollow");
            var para = new FollowParameter {
                followeeId = userId
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

        public static Future<FetchFollowingUserResponse> FetchFollowingUser(string userId, int offset) {
            var url = CStringUtils.genApiUrl($"/u/{userId}/followingUsers");
            var para = new Dictionary<string, object> {
                {"offset", offset}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchFollowingUserResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var followingUserResponse =
                    JsonConvert.DeserializeObject<FetchFollowingUserResponse>(value: responseText);
                return FutureOr.value(value: followingUserResponse);
            });
        }

        public static Future<FetchFollowerResponse> FetchFollower(string userId, int offset) {
            var url = CStringUtils.genApiUrl($"/u/{userId}/followers");
            var para = new Dictionary<string, object> {
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

        public static Future<FetchFollowingTeamResponse> FetchFollowingTeam(string userId, int offset) {
            var url = CStringUtils.genApiUrl($"/u/{userId}/followingTeams");
            var para = new Dictionary<string, object> {
                {"offset", offset}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchFollowingTeamResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var followingTeamResponse =
                    JsonConvert.DeserializeObject<FetchFollowingTeamResponse>(value: responseText);
                return FutureOr.value(value: followingTeamResponse);
            });
        }

        public static Future<FetchFollowingResponse> FetchFollowing(string userId, int offset) {
            var url = CStringUtils.genApiUrl($"/u/{userId}/followings");
            var para = new Dictionary<string, object> {
                {"needTeam", "true"},
                {"offset", offset}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchFollowingResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var followingResponse = JsonConvert.DeserializeObject<FetchFollowingResponse>(value: responseText);
                return FutureOr.value(value: followingResponse);
            });
        }

        public static Future<FetchEditPersonalInfoResponse> EditPersonalInfo(string userId, string fullName,
            string title, string jobRoleId, string placeId) {
            var url = CStringUtils.genApiUrl($"/u/{userId}/updateUserBasicInfo");
            var para = new EditPersonalParameter {
                fullName = fullName,
                title = title,
                jobRoleId = jobRoleId,
                placeId = placeId
            };
            var request = HttpManager.POST(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchEditPersonalInfoResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var editPersonalInfoResponse =
                    JsonConvert.DeserializeObject<FetchEditPersonalInfoResponse>(value: responseText);
                return FutureOr.value(value: editPersonalInfoResponse);
            });
        }

        public static Future<UpdateAvatarResponse> UpdateAvatar(string userId, string avatar) {
            var url = CStringUtils.genApiUrl($"/u/{userId}/updateUserAvatar");
            var para = new UpdateAvatarParameter {
                avatar = $"data:image/jpeg;base64,{avatar}"
            };
            var request = HttpManager.POST(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<UpdateAvatarResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var updateAvatarResponse =
                    JsonConvert.DeserializeObject<UpdateAvatarResponse>(value: responseText);
                return FutureOr.value(value: updateAvatarResponse);
            });
        }

        public static Future RegisterToken(string token, string userId = "") {
            var url = CStringUtils.genApiUrl($"/registerToken");
            var para = new RegisterTokenParameter {
                token = token,
                userId = userId
            };
            var request = HttpManager.POST($"{Config.apiAddress_cn}{Config.apiPath}/registerToken",
                parameter: para);
            return HttpManager.resume(request: request).then(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                return FutureOr.nil;
            });
        }
    }
}