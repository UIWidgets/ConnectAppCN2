using System;
using System.Collections.Generic;
using ConnectApp.Common.Constant;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.external.simplejson;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace ConnectApp.Plugins {
    public class WechatPlugin {
        public static WechatPlugin instance(Action<string> codeCallBack = null) {
            if (plugin == null) {
                plugin = new WechatPlugin();
            }
        
            if (codeCallBack != null) {
                plugin.codeCallBack = codeCallBack;
            }
            return plugin;
        }

        static WechatPlugin plugin;
        bool isListened;
        public BuildContext context;
        Action<string> codeCallBack;
        
        void addListener() {
            if (!this.isListened) {
                UIWidgetsMessageManager.instance.AddChannelMessageDelegate("wechat", del: this._handleMethodCall);
                this.isListened = true;
            }
        }

        void _handleMethodCall(string method, List<JSONNode> args) {
            var isolate = UIWidgetsPanel.anyIsolate;
            if (isolate != null) {
                using (Isolate.getScope(isolate: isolate)) {
                    switch (method) {
                        case "callback": {
                            if (args.isEmpty()) {
                                return;
                            }
                            var node = args[0];
                            var dict = JSON.Parse(aJSON: node);
                            var type = dict["type"];
                            if (type == "code") {
                                var code = dict["code"];
                                this.codeCallBack?.Invoke(obj: code);
                            }
                        }
                            break;
                        case "openUrl": {
                            if (args.isEmpty()) {
                                return;
                            }

                            this.openUrl(args.first());
                        }
                            break;
                    }
                }
            }
        }

        public void openUrl(string schemeUrl) {
            CommonPlugin.openUrlScheme(schemeUrl: schemeUrl);
        }

        public void login(string stateId) {
            if (Application.isEditor) {
                return;
            }

            this.addListener();
            loginWechat(stateId: stateId);
        }

        public void shareToFriend(string title, string description, string url, string imageBytes) {
            if (Application.isEditor) {
                return;
            }

            this.addListener();
            toFriends(title: title, description: description, url: url, imageBytes: imageBytes);
        }

        public void shareToTimeline(string title, string description, string url, string imageBytes) {
            if (Application.isEditor) {
                return;
            }

            this.addListener();
            toTimeline(title: title, description: description, url: url, imageBytes: imageBytes);
        }

        public void shareToMiniProgram(string title, string description, string url, string imageBytes, string path) {
            if (Application.isEditor) {
                return;
            }

            this.addListener();
            toMiNiProgram(title: title, description: description, url: url, imageBytes: imageBytes,
                ysId: Config.miniId, path: path, miniProgramType: Config.miniProgramType);
        }


        public bool isInstalled() {
            if (Application.isEditor) {
                return false;
            }

            this.addListener();
            return isInstallWechat();
        }

        public void toOpenMiNi(string path) {
            if (Application.isEditor) {
                return;
            }

            this.addListener();
            openMiNi(ysId: Config.miniId, path: path, miniProgramType: Config.miniProgramType);
        }
#if UNITY_IOS
        [DllImport("__Internal")]
        static extern void loginWechat(string stateId);

        [DllImport("__Internal")]
        static extern bool isInstallWechat();

        [DllImport("__Internal")]
        static extern void toFriends(string title, string description, string url, string imageBytes);

        [DllImport("__Internal")]
        static extern void toTimeline(string title, string description, string url, string imageBytes);

        [DllImport("__Internal")]
        static extern void toMiNiProgram(string title, string description, string url, string imageBytes,
            string ysId, string path, int miniProgramType);

        [DllImport("__Internal")]
        static extern void openMiNi(string ysId, string path, int miniProgramType);

#elif UNITY_ANDROID
        static AndroidJavaObject _plugin;

        static AndroidJavaObject Plugin() {
            if (_plugin == null) {
                using (
                    var managerClass =
                        new AndroidJavaClass("com.unity3d.unityconnect.plugins.WechatPlugin")
                ) {
                    _plugin = managerClass.CallStatic<AndroidJavaObject>("getInstance");
                }
            }

            return _plugin;
        }

        static void loginWechat(string stateId) {
            Plugin().Call("loginWechat", stateId);
        }

        static bool isInstallWechat() {
            return Plugin().Call<bool>("isInstallWechat");
        }

        static void toFriends(string title, string description, string url, string imageBytes) {
            Plugin().Call("shareToFriends", title, description, url, imageBytes);
        }

        static void toTimeline(string title, string description, string url, string imageBytes) {
            Plugin().Call("shareToTimeline", title, description, url, imageBytes);
        }

        static void toMiNiProgram(string title, string description, string url, string imageBytes, string ysId,
            string path,int miniProgramType) {
            Plugin().Call("shareToMiNiProgram", title, description, url, imageBytes, ysId, path,miniProgramType);
        }

        static void openMiNi(string ysId, string path,int miniProgramType) {
            Plugin().Call("openMiNi", Config.wechatAppId, ysId, path,miniProgramType);
        }
#else
        static bool isInstallWechat() {
            return true;
        }

        static void loginWechat(string stateId) {
        }

        static void toFriends(string title, string description, string url, string imageBytes) {
        }

        static void toTimeline(string title, string description, string url, string imageBytes) {
        }

        static void toMiNiProgram(string title, string description, string url, string imageBytes, string ysId,
            string path, int miniProgramType) {
        }

        static void openMiNi(string ysId, string path, int miniProgramType) {
        }
#endif
    }
}