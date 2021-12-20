using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Models.Api;
using Newtonsoft.Json;
using Unity.UIWidgets.async;

namespace ConnectApp.Api {
    public static class SettingApi {
        public static Future<string> FetchReviewUrl(string platform, string store) {
            var url = CStringUtils.genApiUrl("/reviewUrl");
            var para = new Dictionary<string, object> {
                {"platform", platform},
                {"store", store}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<string>(responseText => {
                var urlDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(value: responseText);
                var reviewUrl = urlDictionary.GetValueOrDefault("url", "");
                return FutureOr.value(value: reviewUrl);
            });
        }

        public static Future<CheckNewVersionResponse> CheckNewVersion(string platform, string store, string version) {
            var url = CStringUtils.genApiUrl("/version");
            var para = new Dictionary<string, object> {
                {"platform", platform},
                {"store", store},
                {"version", version}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<CheckNewVersionResponse>(responseText => {
                var versionDictionary = JsonConvert.DeserializeObject<CheckNewVersionResponse>(value: responseText);
                return FutureOr.value(value: versionDictionary);
            });
        }
    }
}