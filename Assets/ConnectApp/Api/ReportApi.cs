using System.Collections.Generic;
using ConnectApp.Common.Other;
using ConnectApp.Common.Util;
using ConnectApp.Models.Api;
using Newtonsoft.Json;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace ConnectApp.Api {
    public static class ReportApi {
        public static Future ReportItem(string itemId, string itemType, string reportContext) {
            var url = CStringUtils.genApiUrl("/report");
            var para = new ReportParameter {
                itemType = itemType,
                itemId = itemId,
                reasons = new List<string> {"other:" + reportContext}
            };
            var request = HttpManager.POST(url: url, parameter: para);
            return HttpManager.resume(request: request).then(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                return FutureOr.nil;
            });
        }

        public static Future Feedback(FeedbackType type, string content, string name = "", string contact = "") {
            var userId = UserInfoManager.isLoggedIn() ? UserInfoManager.getUserInfo().userId : "";
            var device = AnalyticsManager.deviceId() + (SystemInfo.deviceModel ?? "");
            var dict = new Dictionary<string, string> {
                {"userId", userId}, {"device", device}
            };
            var data = JsonConvert.SerializeObject(value: dict);
            var url = CStringUtils.genApiUrl("/feedback");
            var para = new FeedbackParameter {
                type = type.value,
                contact = contact,
                name = name,
                content = content,
                data = data
            };
            var request = HttpManager.POST(url: url, parameter: para);
            return HttpManager.resume(request: request).then(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                return FutureOr.nil;
            });
        }
    }
}