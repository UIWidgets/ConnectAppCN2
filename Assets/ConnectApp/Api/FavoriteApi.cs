using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Models.Api;
using ConnectApp.Models.Model;
using Newtonsoft.Json;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;

namespace ConnectApp.Api {
    public static class FavoriteApi {
        public static Future<FetchFavoriteTagsResponse> FetchMyFavoriteTags(string userId, int offset) {
            var url = CStringUtils.genApiUrl($"/favorite-tag/{userId}/list");
            var para = new Dictionary<string, object> {
                {"offset", offset},
                {"list", "my"}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchFavoriteTagsResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var favoritesResponse = JsonConvert.DeserializeObject<FetchFavoriteTagsResponse>(value: responseText);
                return FutureOr.value(value: favoritesResponse);
            });
        }

        public static Future<FetchFavoriteTagsResponse> FetchFollowFavoriteTags(string userId, int offset) {
            var url = CStringUtils.genApiUrl($"/favorite-tag/{userId}/list");
            var para = new Dictionary<string, object> {
                {"offset", offset},
                {"list", "other"}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchFavoriteTagsResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var favoritesResponse = JsonConvert.DeserializeObject<FetchFavoriteTagsResponse>(value: responseText);
                return FutureOr.value(value: favoritesResponse);
            });
        }

        public static Future<FetchFavoriteDetailResponse>
            FetchFavoriteDetail(string userId, string tagId, int offset) {
            var url = CStringUtils.genApiUrl($"/favorite/{userId}/list");
            var para = new Dictionary<string, object> {
                {"tagId", tagId},
                {"offset", offset}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchFavoriteDetailResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var favoriteDetailResponse =
                    JsonConvert.DeserializeObject<FetchFavoriteDetailResponse>(value: responseText);
                return FutureOr.value(value: favoriteDetailResponse);
            });
        }

        public static Future<FavoriteTag>
            CreateFavoriteTag(IconStyle iconStyle, string name, string description = "") {
            var url = CStringUtils.genApiUrl("/favorite-tag");
            var para = new Dictionary<string, object> {
                {"name", name},
                {"description", description},
                {"iconStyle", iconStyle}
            };
            var request = HttpManager.POST(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FavoriteTag>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var favoriteTag = JsonConvert.DeserializeObject<FavoriteTag>(value: responseText);
                return FutureOr.value(value: favoriteTag);
            });
        }

        public static Future<FavoriteTag> EditFavoriteTag(string tagId, IconStyle iconStyle, string name,
            string description = "") {
            var url = CStringUtils.genApiUrl("/favorite-tag/edit");
            var para = new Dictionary<string, object> {
                {"id", tagId},
                {"name", name},
                {"description", description},
                {"iconStyle", iconStyle}
            };
            var request = HttpManager.POST(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FavoriteTag>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var favoriteTag = JsonConvert.DeserializeObject<FavoriteTag>(value: responseText);
                return FutureOr.value(value: favoriteTag);
            });
        }

        public static Future<FavoriteTag> DeleteFavoriteTag(string tagId = "", string quoteTagId = "") {
            var url = CStringUtils.genApiUrl("/favorite-tag/delete");
            var para = new Dictionary<string, object>();
            if (quoteTagId.isNotEmpty()) {
                para.Add("quoteTagId", value: quoteTagId);
            }
            else {
                para.Add("id", value: tagId);
            }

            var request = HttpManager.POST(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FavoriteTag>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var favoriteTag = JsonConvert.DeserializeObject<FavoriteTag>(value: responseText);
                return FutureOr.value(value: favoriteTag);
            });
        }

        public static Future<CollectFavoriteTagResponse> CollectFavoriteTag(string tagId) {
            var url = CStringUtils.genApiUrl("/favorite-tag/collect");
            var para = new Dictionary<string, object> {
                {"tagId", tagId}
            };
            var request = HttpManager.POST(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<CollectFavoriteTagResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var collectFavoriteTagResponse =
                    JsonConvert.DeserializeObject<CollectFavoriteTagResponse>(value: responseText);
                return FutureOr.value(value: collectFavoriteTagResponse);
            });
        }
    }
}