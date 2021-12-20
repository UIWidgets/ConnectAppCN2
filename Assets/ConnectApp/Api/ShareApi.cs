using ConnectApp.Common.Util;
using Unity.UIWidgets.async;
using UnityEngine;

namespace ConnectApp.Api {
    public static class ShareApi {
        public static Future<byte[]> FetchImageBytes(string url) {
            return HttpManager.DownloadImage(url: url).then_<byte[]>(responseText => {
                if (url.EndsWith(".jpg") || url.EndsWith(".png")) {
                    var quality = 75;
                    var data = responseText.EncodeToJPG(quality: quality);
                    while (data.Length > 32 * 1024) {
                        quality -= 1;
                        data = responseText.EncodeToJPG(quality: quality);
                    }

                    return FutureOr.value(value: data);
                }

                return FutureOr.nil;
            });
        }
    }
}