using System;
using System.Collections.Generic;
using ConnectApp.Common.Util;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.external.simplejson;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using UnityEngine;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace ConnectApp.Plugins {
    public enum ImageSource {
        Camera,
        Gallery
    }

    public static class PickImagePlugin {
        static Action<byte[]> _imageCallBack;
        static Action<byte[]> _videoCallBack;
        public static bool isListened;

        static void addListener() {
            if (Application.isEditor) {
                return;
            }

            if (!isListened) {
                UIWidgetsMessageManager.instance.AddChannelMessageDelegate("pickImage", del: _handleMethodCall);
            }
        }

        static void _handleMethodCall(string method, List<JSONNode> args) {
            var isolate = UIWidgetsPanel.anyIsolate;
            if (isolate != null) {
                using (Isolate.getScope(isolate: isolate)) {
                    switch (method) {
                        case "pickImageSuccess": {
                            var node = args[0];
                            var dict = JSON.Parse(aJSON: node);
                            if (dict["image"] != null) {
                                var image = (string) dict["image"];
                                var imageData = Convert.FromBase64String(s: image);
                                _imageCallBack?.Invoke(obj: imageData);
                            }
                            else if (dict["imagePath"] != null) {
                                var imagePath = (string) dict["imagePath"];
                                CImageUtils.asyncLoadImageFile(path: imagePath).then_(bytes => {
                                    _imageCallBack?.Invoke(obj: bytes);
                                });
                            }
                            StatusBarManager.hideStatusBar(false);
                            break;
                        }

                        case "pickVideoSuccess": {
                            var node = args[0];
                            var dict = JSON.Parse(aJSON: node);
                            if (dict["videoData"] != null) {
                                var videoData = (string) dict["videoData"];
                                var data = Convert.FromBase64String(s: videoData);
                                _videoCallBack?.Invoke(obj: data);
                            }
                            else if (dict["videoPath"] != null) {
                                var videoPath = (string) dict["videoPath"];
                                CImageUtils.asyncLoadImageFile(path: videoPath).then_(bytes => {
                                    _videoCallBack?.Invoke(obj: bytes);
                                    // return FutureOr.nil;
                                });
                            }
                            StatusBarManager.hideStatusBar(false);
                            break;
                        }

                        case "cancel": {
                            StatusBarManager.hideStatusBar(false);
                            break;
                        }
                    }
                }
            }
        }

        public static void PickImage(
            ImageSource source,
            Action<byte[]> imageCallBack = null,
            bool cropped = true,
            int? maxSize = null
        ) {
            if (Application.isEditor) {
                return;
            }

            addListener();
            _imageCallBack = imageCallBack;
            var sourceInt = (int) source;
            pickImage(sourceInt.ToString(), cropped: cropped, maxSize ?? 0);
        }

        public static void PickVideo(
            ImageSource source,
            Action<byte[]> videoCallBack = null
        ) {
            if (Application.isEditor) {
                return;
            }

            addListener();
            _videoCallBack = videoCallBack;
            var sourceInt = (int) source;
            pickVideo(sourceInt.ToString());
        }

        public static void SaveImage(string imagePath, byte[] image) {
            if (Application.isEditor) {
                return;
            }

            if (image.isEmpty()) {
                return;
            }

            if (Application.platform == RuntimePlatform.Android) {
                var imageBase64String = Convert.ToBase64String(inArray: image);
                saveImage(image: imageBase64String);
                return;
            }

            saveImage(image: imagePath);
        }
        
#if UNITY_IOS
        [DllImport("__Internal")]
        static extern void pickImage(string source, bool cropped = true, int maxSize = 0);

        [DllImport("__Internal")]
        static extern void pickVideo(string source);
        
        [DllImport("__Internal")]
        static extern void saveImage(string image);
#elif UNITY_ANDROID
        static AndroidJavaClass _plugin;

        static AndroidJavaClass Plugin() {
            if (_plugin == null) {
                _plugin = new AndroidJavaClass("com.unity3d.unityconnect.plugins.CommonPlugin");
            }

            return _plugin;
        }

        static void pickImage(string source, bool cropped = true, int maxSize = 0) {
            Plugin().CallStatic("pickImage", source, cropped, maxSize);
        }

        static void pickVideo(string source) {
            Plugin().CallStatic("pickVideo", source);
        }

        static void saveImage(string image) {
            Plugin().CallStatic("saveImage", image);
        }
#else
        static void pickImage(string source, bool cropped = true, int maxSize = 0) {
        }

        static void pickVideo(string source) {
        }

        static void saveImage(string image) {
        }

#endif
    }
}