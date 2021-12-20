using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ConnectApp.Common.Constant;
using ConnectApp.redux;
using ConnectApp.redux.actions;
using Newtonsoft.Json;
using Unity.UIWidgets.async;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
using UnityEngine.Networking;

namespace ConnectApp.Common.Util {
    public static class Method {
        public const string GET = "GET";
        public const string POST = "POST";
    }

    public class HttpResponseContent {
        public string text;
        public Dictionary<string, string> headers;
    }

    public static class HttpManager {
        public const string COOKIE = "Cookie";

        static string authenticate(string username, string password)
        {
            var auth = username + ":" + password;
            auth = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(s: auth));
            auth = "Basic " + auth;
            return auth;
        }
        
        static UnityWebRequest initRequest(
            string url,
            string method,
            bool isLearn = false
        ) {
            var request = new UnityWebRequest {
                url = url,
                method = method,
                downloadHandler = new DownloadHandlerBuffer()
            };
            request.SetRequestHeader("X-Requested-With", "XmlHttpRequest");
            UnityWebRequest.ClearCookieCache();
            request.SetRequestHeader(name: COOKIE, _cookieHeader());
            request.SetRequestHeader("ConnectAppVersion", value: Config.versionName);
            if (Application.isEditor) {
                request.SetRequestHeader("ConnectAppDebug", "true");
                request.SetRequestHeader("DeviceSystem", "editor");
            }
            else {
                request.SetRequestHeader("DeviceSystem", Config.getPlatform());
            }
            if (isLearn) {
                var authorization = authenticate(username: Config.auth_username_learn, password: Config.auth_password_learn);
                request.SetRequestHeader("AUTHORIZATION", value: authorization);
            }
            
            return request;
        }

        public static UnityWebRequest GET(string url, object parameter = null, bool isLearn = false) {
            var newUrl = url;
            if (parameter != null) {
                var parameterString = "";
                var par = JsonHelper.ToDictionary(json: parameter);
                foreach (var keyValuePair in par) {
                    parameterString += $"{keyValuePair.Key}={keyValuePair.Value}&";
                }

                if (parameterString.Length > 0) {
                    var newParameterString = parameterString.Remove(parameterString.Length - 1);
                    newUrl += $"?{newParameterString}";
                }
            }

            return initRequest(url: newUrl, method: Method.GET, isLearn: isLearn);
        }

        public static UnityWebRequest POST(string url, object parameter = null, bool multipart = false,
            string filename = "", string fileType = "") {
            var request = initRequest(url: url, method: Method.POST);
            if (parameter != null) {
                var boundary = $"----WebKitFormBoundary{Snowflake.CreateNonce()}";
                if (multipart) {
                    var results = new List<byte[]>();
                    var size = 0;
                    if (parameter is List<List<object>> list) {
                        foreach (var item in list) {
                            D.assert(item.Count == 2);
                            D.assert(item[0] is string);
                            if (item[1] == null) {
                                continue;
                            }

                            if (item[1] is byte[]) {
                                var itemStr =
                                    $"--{boundary}\r\nContent-Disposition: form-data; name=\"{item[0]}\"; filename=\"{filename}\"\r\n" +
                                    $"Content-Type: {fileType}\r\n\r\n";
                                results.Add(Encoding.UTF8.GetBytes(s: itemStr));
                                size += results.last().Length;
                                results.Add(item[1] as byte[]);
                                size += results.last().Length;
                                results.Add(Encoding.UTF8.GetBytes("\r\n"));
                                size += results.last().Length;
                            }
                            else {
                                var s = $"{item[1]}";
                                var itemStr =
                                    $"--{boundary}\r\nContent-Disposition: form-data; name=\"{item[0]}\"\r\n\r\n{s}\r\n";
                                results.Add(Encoding.UTF8.GetBytes(s: itemStr));
                                size += results.last().Length;
                            }
                        }
                    }
                    else {
                        D.assert(false, () => "Parameter must be list of lists");
                    }

                    results.Add(Encoding.UTF8.GetBytes($"--{boundary}--"));
                    size += results.last().Length;
                    var bodyRaw = new byte[size];
                    var offset = 0;
                    foreach (var bytes in results) {
                        Buffer.BlockCopy(src: bytes, 0, dst: bodyRaw, dstOffset: offset, count: bytes.Length);
                        offset += bytes.Length;
                    }

                    request.uploadHandler = new UploadHandlerRaw(data: bodyRaw);
                    request.SetRequestHeader("Content-Type", $"multipart/form-data; boundary={boundary}");
                }
                else {
                    var body = JsonConvert.SerializeObject(value: parameter);
                    var bodyRaw = Encoding.UTF8.GetBytes(s: body);
                    request.uploadHandler = new UploadHandlerRaw(data: bodyRaw);
                    request.SetRequestHeader("Content-Type", "application/json");
                }
            }

            return request;
        }

        public static Future<Texture2D> DownloadImage(string url) {
            var completer = Completer.create();
            var isolate = Isolate.current;
            var panel = UIWidgetsPanelWrapper.current.window;
            if (panel.isActive()) {
                panel.startCoroutine(fetchImageBytes(completer: completer, url: url, isolate: isolate));
            }

            return completer.future.to<Texture2D>();
        }

        public static Future<string> resume(UnityWebRequest request) {
            var completer = Completer.create();
            var isolate = Isolate.current;
            var panel = UIWidgetsPanelWrapper.current.window;
            if (panel.isActive()) {
                panel.startCoroutine(sendRequest(request: request, completer: completer, isolate: isolate));
            }

            return completer.future.to<string>();
        }

        static IEnumerator sendRequest(UnityWebRequest request, Completer completer, Isolate isolate) {
            yield return request.SendWebRequest();

            using (Isolate.getScope(isolate: isolate)) {
                
                if (request.responseCode == 401) {
                    StoreProvider.store.dispatcher.dispatch(new LogoutAction());
                    completer.completeError(new Exception("Unauthorized, please login again"));
                    yield break;
                }
                
                if (isNetWorkError() || request.isNetworkError || request.isHttpError) {
                    completer.completeError(new Exception($"Failed to load from url \"{request.url}\": {request.error}"));
                    yield break;
                }

                if (request.responseCode != 200) {
                    completer.completeError(
                        new Exception($"Failed to load from url \"{request.url}\": {request.error}"));
                    yield break;
                }

                if (request.GetResponseHeaders().ContainsKey("Set-Cookie")) {
                    var cookie = request.GetResponseHeaders()["Set-Cookie"];
                    updateCookie(newCookie: cookie);
                }

                var data = request.downloadHandler.text;
                
                completer.complete(value: data);
            }
        }

        public static Future<HttpResponseContent> resumeAll(UnityWebRequest request) {
            var completer = Completer.create();
            var isolate = Isolate.current;
            var panel = UIWidgetsPanelWrapper.current.window;
            panel.startCoroutine(sendRequestAll(request: request, completer: completer, isolate: isolate));
            return completer.future.to<HttpResponseContent>();
        }

        static IEnumerator sendRequestAll(UnityWebRequest request, Completer completer, Isolate isolate) {
            yield return request.SendWebRequest();

            using (Isolate.getScope(isolate: isolate)) {
                if (request.responseCode == 401) {
                    StoreProvider.store.dispatcher.dispatch(new LogoutAction());
                    completer.completeError(new Exception("Unauthorized, please login again"));
                    yield break;
                }

                if (request.responseCode != 200 || request.isNetworkError || request.isHttpError) {
                    completer.completeError(
                        new Exception($"Failed to load from url \"{request.url}\": {request.error}"));
                    yield break;
                }

                if (request.GetResponseHeaders().ContainsKey("Set-Cookie")) {
                    var cookie = request.GetResponseHeaders()["Set-Cookie"];
                    updateCookie(newCookie: cookie);
                }

                var content = new HttpResponseContent {
                    text = request.downloadHandler.text,
                    headers = request.GetResponseHeaders()
                };
                
                completer.complete(FutureOr.value(value: content));
            }
        }

        static IEnumerator fetchImageBytes(Completer completer, string url, Isolate isolate) {
            var request = UnityWebRequestTexture.GetTexture(uri: url);
            request.SetRequestHeader("X-Requested-With", "XmlHttpRequest");
            yield return request.SendWebRequest();

            using (Isolate.getScope(isolate: isolate)) {
                if (request.responseCode != 200 || request.isNetworkError || request.isHttpError) {
                    completer.completeError(
                        new Exception($"Failed to load from url \"{request.url}\": {request.error}"));
                    yield break;
                }

                var texture = ((DownloadHandlerTexture) request.downloadHandler).texture;
                if (texture) {
                    completer.complete(FutureOr.value(value: texture));
                }
                else {
                    completer.completeError(new Exception("no picture"));
                }
            }
        }

        static string _cookieHeader() {
            return PlayerPrefs.GetString(key: COOKIE).isNotEmpty() ? PlayerPrefs.GetString(key: COOKIE) : "";
        }

        public static void clearCookie() {
            PlayerPrefs.SetString(key: COOKIE, "");
            PlayerPrefs.Save();
            // SocketApi.OnCookieChanged();
        }

        public static string getCookie() {
            return _cookieHeader();
        }

        public static string getCookie(string key) {
            var cookie = getCookie();
            if (cookie.isEmpty()) {
                return "";
            }

            var cookieArr = cookie.Split(';');
            foreach (var c in cookieArr) {
                var carr = c.Split('=');

                if (carr.Length != 2) {
                    continue;
                }

                var name = carr[0].Trim();
                var value = carr[1].Trim();
                if (name == key) {
                    return value;
                }
            }

            return "";
        }

        public static void updateCookie(string newCookie) {
            var cookie = PlayerPrefs.GetString(key: COOKIE);
            var cookieDict = new Dictionary<string, string>();
            var updateCookie = "";
            if (cookie.isNotEmpty()) {
                var cookieArr = cookie.Split(';');
                foreach (var c in cookieArr) {
                    var name = c.Split('=').first();
                    cookieDict.Add(key: name, value: c);
                }
            }

            if (newCookie.isNotEmpty()) {
                var newCookieArr = newCookie.Split(',');
                foreach (var c in newCookieArr) {
                    var item = c.Split(';').first();
                    var name = item.Split('=').first();
                    if (cookieDict.ContainsKey(key: name)) {
                        cookieDict[key: name] = item;
                    }
                    else {
                        cookieDict.Add(key: name, value: item);
                    }
                }

                var updateCookieArr = cookieDict.Values;
                updateCookie = string.Join(";", values: updateCookieArr);
            }

            if (updateCookie.isEmpty()) {
                return;
            }

            PlayerPrefs.SetString(key: COOKIE, value: updateCookie);
            PlayerPrefs.Save();
        }

        public static bool isNetWorkError() {
            return Application.internetReachability == NetworkReachability.NotReachable;
        }
    }
}