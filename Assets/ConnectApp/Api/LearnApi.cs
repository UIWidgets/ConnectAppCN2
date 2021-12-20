using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Models.Api;
using Newtonsoft.Json;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;

namespace ConnectApp.Api {
    public static class LearnApi {
        public static Future<FetchLearnCourseListResponse> FetchLearnCourseList(int page) {
            var url = CStringUtils.genLearnApiUrl("/thirdPartySearch");
            var para = new Dictionary<string, object> {
                {"page", page}
            };
            var request = HttpManager.GET(url: url, parameter: para, true);
            return HttpManager.resume(request: request).then_<FetchLearnCourseListResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var learnCourseListResponse = JsonConvert.DeserializeObject<FetchLearnCourseListResponse>(value: responseText);
                return FutureOr.value(value: learnCourseListResponse);
            });
        }
    }
}