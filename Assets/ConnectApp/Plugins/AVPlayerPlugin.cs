using System.Collections.Generic;
using ConnectApp.Common.Constant;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.external.simplejson;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace ConnectApp.Plugins {
    public static class AVPlayerPlugin {
        public static bool isExistPlayer;
        public static bool isConfigPlayer;
        static BuildContext _buildContext;

        public static void initVideoPlayer(BuildContext buildContext, string url, string cookie, float left, float top, float width, float height,
            bool isPop, string verifyType, int limitSeconds) {
            if (Application.isEditor) {
                return;
            }

            if (isExistPlayer) {
                return;
            }

            isExistPlayer = true;
            if (Application.platform == RuntimePlatform.IPhonePlayer) {
                Screen.orientation = ScreenOrientation.AutoRotation;
            }
            _buildContext = buildContext;

            var ratio = Application.platform == RuntimePlatform.Android ? CSizes.DevicePixelRatio : 1.0f;
            InitPlayer(url: url, cookie: cookie, left: left, top * ratio, width * ratio, height * ratio, isPop: isPop,
                verifyType: verifyType, limitSeconds: limitSeconds);
            UIWidgetsMessageManager.instance.AddChannelMessageDelegate("player", del: _handleMethodCall);
        }

        public static void configVideoPlayer(string url, string cookie) {
            if (Application.isEditor) {
                return;
            }

            isConfigPlayer = true;
            ConfigPlayer(url: url, cookie: cookie);
        }

        public static void removePlayer() {
            if (Application.isEditor) {
                return;
            }

            Screen.orientation = ScreenOrientation.Portrait;
            isExistPlayer = false;
            isConfigPlayer = false;
            VideoRelease();
            UIWidgetsMessageManager.instance.RemoveChannelMessageDelegate("player", del: _handleMethodCall);
        }

        public static void hiddenPlayer() {
            if (Application.isEditor) {
                return;
            }

            Screen.orientation = ScreenOrientation.Portrait;
            VideoPause();
            VideoHidden();
        }

        public static void showPlayer() {
            if (Application.isEditor) {
                return;
            }

            Screen.orientation = ScreenOrientation.AutoRotation;
            VideoShow();
        }


        static void _handleMethodCall(string method, List<JSONNode> args) {
            var isolate = UIWidgetsPanel.anyIsolate;
            if (isolate != null) {
                using (Isolate.getScope(isolate: isolate)) {
                    switch (method) {
                        case "PopPage": {
                            Navigator.pop(context: _buildContext);
                            break;
                        }
                        case "Share": {
                            EventBus.publish(sName: EventBusConstant.shareAction, null);
                            break;
                        }
                        case "BuyLicense": {
                            Application.OpenURL(url: Config.unityStoreUrl);
                            break;
                        }
                        case "UpdateLicense": {
                            Application.OpenURL(url: Config.unityLearnPremiumUrl);
                            break;
                        }
                        case "LandscapeLeft": {
                            Screen.orientation = ScreenOrientation.LandscapeLeft;
                            break;
                        }
                        case "Portrait": {
                            Screen.orientation = ScreenOrientation.Portrait;
                            break;
                        }
                    }
                }
            }
        }


#if UNITY_IOS
        [DllImport("__Internal")]
        static extern void InitPlayer(string url, string cookie, float left, float top, float width,
            float height, bool isPop, string verifyType, int limitSeconds);

        [DllImport("__Internal")]
        static extern void ConfigPlayer(string url, string cookie);

        [DllImport("__Internal")]
        static extern void VideoRelease();

        [DllImport("__Internal")]
        static extern void VideoPause();

        [DllImport("__Internal")]
        static extern void VideoPlay();

        [DllImport("__Internal")]
        static extern void VideoHidden();

        [DllImport("__Internal")]
        static extern void VideoShow();

#elif UNITY_ANDROID
        static AndroidJavaObject _plugin;

        static AndroidJavaObject Plugin() {
            if (_plugin == null) {
                using (
                    var managerClass =
                        new AndroidJavaClass("com.unity3d.unityconnect.plugins.AVPlayerPlugin")
                ) {
                    _plugin = managerClass.CallStatic<AndroidJavaObject>("getInstance");
                }
            }

            return _plugin;
        }

        static void InitPlayer(string url, string cookie, float left, float top, float width, float height,
            bool isPop, string verifyType, int limitSeconds) {
            Plugin().Call("InitPlayer", url, cookie, left, top, width, height, isPop, verifyType, limitSeconds);
        }

        static void ConfigPlayer(string url, string cookie) {
            Plugin().Call("ConfigPlayer", url, cookie);
        }

        static void VideoRelease() {
            Plugin().Call("VideoRelease");
        }

        static void VideoPause() {
            Plugin().Call("VideoPause");
        }

        static void VideoPlay() {
            Plugin().Call("VideoPlay");
        }

        static void VideoHidden() {
            Plugin().Call("VideoHidden");
        }

        static void VideoShow() {
            Plugin().Call("VideoShow");
        }
#else
        static void InitPlayer(string url, string cookie, float left, float top, float width, float height,
            bool isPop, string verifyType, int limitSeconds) {
        }

        static void ConfigPlayer(string url, string cookie) {
        }

        static void VideoRelease() {
        }

        static void VideoPause() {
        }

        static void VideoPlay() {
        }

        static void VideoHidden() {
        }

        static void VideoShow() {
        }

#endif
    }
}