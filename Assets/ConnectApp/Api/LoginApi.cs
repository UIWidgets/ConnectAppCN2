using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Models.Api;
using ConnectApp.Models.Model;
using Newtonsoft.Json;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;

namespace ConnectApp.Api {
    public static class LoginApi {
        public static Future<LoginInfo> LoginByEmail(string email, string password) {
            var url = CStringUtils.genApiUrl("/auth/live/login");
            var para = new LoginParameter {
                email = email,
                password = password
            };
            var request = HttpManager.POST(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<LoginInfo>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var loginInfo = JsonConvert.DeserializeObject<LoginInfo>(value: responseText);
                return FutureOr.value(value: loginInfo);
            });
        }

        public static Future<LoginInfo> LoginByWechat(string code) {
            var url = CStringUtils.genApiUrl("/auth/live/wechat");
            var para = new WechatLoginParameter {
                code = code
            };
            var request = HttpManager.POST(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<LoginInfo>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var loginInfo = JsonConvert.DeserializeObject<LoginInfo>(value: responseText);
                return FutureOr.value(value: loginInfo);
            });
        }

        public static Future<bool> LoginByQr(string token, string action) {
            var url = CStringUtils.genApiUrl("/auth/qrlogin");
            var para = new QRLoginParameter {
                token = token,
                action = action
            };
            var request = HttpManager.POST(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<bool>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var successDictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(value: responseText);
                var success = successDictionary.ContainsKey("success") ? successDictionary["success"] : false;
                return FutureOr.value(value: success);
            });
        }

        public static Future<string> FetchCreateUnityIdUrl() {
            var url = CStringUtils.genApiUrl("/authUrl");
            var request = HttpManager.GET(url: url);
            return HttpManager.resume(request: request).then_<string>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var urlDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(value: responseText);
                var createUnityIdUrl = urlDictionary.GetValueOrDefault("url", "");
                return FutureOr.value(value: createUnityIdUrl);
            });
        }

        public static Future<FetchInitDataResponse> InitData() {
            var url = CStringUtils.genApiUrl("/initData");
            var request = HttpManager.GET(url: url);
            return HttpManager.resume(request: request).then_<FetchInitDataResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var initDataResponse = JsonConvert.DeserializeObject<FetchInitDataResponse>(value: responseText);
                return FutureOr.value(value: initDataResponse);
            });
        }
    }
}