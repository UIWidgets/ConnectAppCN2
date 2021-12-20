using System.Collections.Generic;
using ConnectApp.Common.Constant;
using ConnectApp.Common.Util;
using ConnectApp.Models.Api;
using Newtonsoft.Json;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;

namespace ConnectApp.Api {
    public static class NotificationApi {
        public static Future<FetchNotificationResponse> FetchNotifications(int pageNumber) {
            var url = CStringUtils.genApiUrl("/notifications");
            var para = new Dictionary<string, object> {
                {"page", pageNumber}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchNotificationResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var notificationResponse =
                    JsonConvert.DeserializeObject<FetchNotificationResponse>(value: responseText);
                return FutureOr.value(value: notificationResponse);
            });
        }

        public static Future<FetchNotificationResponse> FetchNotificationsByCategory(int pageNumber, string category) {
            var url = CStringUtils.genApiUrl("/notifications");
            var para = new Dictionary<string, object> {
                {"page", pageNumber},
                {"category", category}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchNotificationResponse>(responseText =>
            {
                if (responseText == null)
                {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var notificationResponse =
                    JsonConvert.DeserializeObject<FetchNotificationResponse>(value: responseText);
                return FutureOr.value(value: notificationResponse);
            });
        }
        
        public static Future FetchMakeAllSeen()
        {
            var url = CStringUtils.genApiUrl("/notifications/make-all-seen");
            var request = HttpManager.POST($"{Config.apiAddress_cn}{Config.apiPath}/notifications/make-all-seen");
            return HttpManager.resume(request: request).then(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                return FutureOr.nil;
            });
        }
    }
}