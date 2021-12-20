using ConnectApp.Common.Util;
using ConnectApp.Models.Model;
using Newtonsoft.Json;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace ConnectApp.Api {
    public static class SplashApi {
        public static Future<Splash> FetchSplash() {
            var url = CStringUtils.genApiUrl("/ads");
            var request = HttpManager.GET(url: url);
            return HttpManager.resume(request: request).then_<Splash>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var splashResponse = JsonConvert.DeserializeObject<Splash>(value: responseText);
                return FutureOr.value(value: splashResponse);
            });
        }

        public static Future<byte[]> FetchSplashImage(string url) {
            return HttpManager.DownloadImage(url: url).then_<byte[]>(responseText => {
                var pngData = responseText.EncodeToPNG();
                return pngData != null ? FutureOr.value(value: pngData) : FutureOr.nil;
            });
        }
    }
}