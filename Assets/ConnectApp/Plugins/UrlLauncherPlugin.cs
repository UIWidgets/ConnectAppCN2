using ConnectApp.Common.Util;
using Unity.UIWidgets.foundation;
using UnityEngine;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace ConnectApp.Plugins {
    public static class UrlLauncherPlugin {
        public static void Launch(string urlString, bool forceSafariVC = true, bool forceWebView = true) {
            if (Application.isEditor || CCommonUtils.isAndroid) {
                Application.OpenURL(url: urlString);
                return;
            }

            launch(urlString: urlString, forceSafariVC: forceSafariVC, forceWebView: forceWebView, HttpManager.getCookie());
        }

        public static bool CanLaunch(string urlString) {
            if (urlString.isEmpty()) {
                return false;
            }

            return Application.isEditor || CCommonUtils.isAndroid || canLaunch(urlString: urlString);
        }

#if UNITY_IOS
        [DllImport("__Internal")]
        static extern void launch(string urlString, bool forceSafariVC = true, bool forceWebView = true, string cookie = null);

        [DllImport("__Internal")]
        static extern bool canLaunch(string urlString);
#elif UNITY_ANDROID
        static AndroidJavaClass _plugin;

        static AndroidJavaClass Plugin() {
            if (_plugin == null) {
                _plugin = new AndroidJavaClass("com.unity3d.unityconnect.plugins.UrlLauncherPlugin");
            }

            return _plugin;
        }

        static void launch(string urlString, bool forceSafariVC = true, bool forceWebView = true, string cookie = null) {
            // Plugin().CallStatic("launch", urlString, forceSafariVC, forceWebView, cookie);
        }

        static bool canLaunch(string urlString) {
            return true;
            // return Plugin().CallStatic<bool>("canLaunch", urlString);
        }
#else
        static void launch(string urlString, bool forceSafariVC = true, bool forceWebView = true, string cookie = null) { }
        static bool canLaunch(string urlString) { return true; }
#endif
    }
}