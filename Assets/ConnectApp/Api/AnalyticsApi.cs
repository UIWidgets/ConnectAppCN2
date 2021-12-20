using System;
using System.Collections.Generic;
using ConnectApp.Common.Constant;
using ConnectApp.Common.Util;
using ConnectApp.Models.Api;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace ConnectApp.Api {
    public static class AnalyticsApi {
        public static Future AnalyticsApp(string eventType, List<Dictionary<string, string>> data) {
            var url = CStringUtils.genApiUrl("/statistic");
            var device = AnalyticsManager.deviceId() + (SystemInfo.deviceModel ?? "");
            var para = new OpenAppParameter {
                device = device,
                deviceModel = SystemInfo.deviceModel ?? "",
                store = Config.getStore(),
                eventType = eventType,
                appTime = DateTime.UtcNow,
                extraData = data
            };
            var request = HttpManager.POST(url: url, parameter: para);
            return HttpManager.resume(request: request).then_(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                return FutureOr.nil;
            });
        }
    }
}