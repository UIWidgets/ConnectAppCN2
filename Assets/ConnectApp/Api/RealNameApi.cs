using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Models.Api;
using Newtonsoft.Json;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;

namespace ConnectApp.Api {
    public static class RealNameApi {
        public static Future<CheckRealNameResponse> CheckRealName(string idCard, string realName) {
            var url = CStringUtils.genApiUrl("/auth/checkRealName");
            var para = new Dictionary<string, object> {
                {"idCard", idCard},
                {"name", realName}
            };
            var request = HttpManager.POST(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<CheckRealNameResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var checkRealNameResponse =
                    JsonConvert.DeserializeObject<CheckRealNameResponse>(value: responseText);
                return FutureOr.value(value: checkRealNameResponse);
            });
        }
    }
}