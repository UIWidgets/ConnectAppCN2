using System;
using System.Collections.Generic;
using System.Web;
using ConnectApp.Api;
using ConnectApp.Common.Constant;
using ConnectApp.Common.Util;
using ConnectApp.Components;
using ConnectApp.Components.Toast;
using ConnectApp.Main;
using ConnectApp.Models.Model;
using ConnectApp.redux;
using Unity.UIWidgets.async;
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
    public static class QRScanPlugin {
        static bool isListen;
        public static string qrCodeToken;
        static string _loginSubId;
        static BuildContext _buildContext;

        static void addListener() {
            if (Application.isEditor) {
                return;
            }

            if (!isListen) {
                isListen = true;
                UIWidgetsMessageManager.instance.AddChannelMessageDelegate("QRScan",
                    (method, args) => _handleMethodCall(method: method, args: args));
                if (_loginSubId.isNotEmpty()) {
                    EventBus.unSubscribe(sName: EventBusConstant.login_success, id: _loginSubId);
                }

                _loginSubId = EventBus.subscribe(sName: EventBusConstant.login_success, _ => {
                    if (qrCodeToken.isNotEmpty()) {
                        checkToken(token: qrCodeToken);
                        qrCodeToken = null;
                    }
                });
            }
        }

        static void checkToken(string token) {
            CustomDialogUtils.showCustomDialog<object>(
                context: _buildContext,
                child: new CustomLoadingDialog(
                    message: "验证中"
                )
            );
            LoginApi.LoginByQr(token: token, "check").then(success => {
                CustomDialogUtils.hiddenCustomDialog(context: _buildContext);
                Navigator.pushNamed(
                    context: _buildContext,
                    routeName: NavigatorRoutes.QRScanLogin,
                    new QRScanLoginScreenArguments { token = token }
                );
                CustomToast.showToast(context: _buildContext, "验证成功");
                AnalyticsManager.AnalyticsQRScan(state: QRState.check);
            }).catchError(error => {
                CustomDialogUtils.hiddenCustomDialog(context: _buildContext);
                CustomToast.showToast(context: _buildContext, "验证失败", type: ToastType.Error);
                Future.delayed(new TimeSpan(0, 0, 1))
                    .then(_ => {
                        PushToQRScan(context: _buildContext);
                        AnalyticsManager.AnalyticsQRScan(state: QRState.check, false);
                    });
            });
        }

        static void _handleMethodCall(string method, List<JSONNode> args) {
            var isolate = UIWidgetsPanel.anyIsolate;
            if (isolate != null) {
                using (Isolate.getScope(isolate: isolate)) {
                    switch (method) {
                        case "OnReceiveQRCode": {
                            string qrCode = args[0];
                            if (qrCode.isUrl()) {
                                var uri = new Uri(uriString: qrCode);
                                if (uri.AbsoluteUri.StartsWith("https://connect") ||
                                    uri.AbsoluteUri.StartsWith(value: Config.unity_cn_url)) {
                                    var token = HttpUtility.ParseQueryString(query: uri.Query).Get("token");
                                    if (token.isNotEmpty()) {
                                        var isLoggedIn = StoreProvider.store.getState().loginState.isLoggedIn;
                                        if (isLoggedIn) {
                                            checkToken(token: token);
                                        }
                                        else {
                                            qrCodeToken = token;
                                            Navigator.pushNamed(
                                                context: _buildContext,
                                                routeName: NavigatorRoutes.Login
                                            );
                                        }
                                    }
                                    else {
                                        CustomToast.showToast(context: _buildContext, "暂不支持该二维码类型");
                                        // CustomToast.show(new CustomToastItem(context: _buildContext, "暂不支持该二维码类型"));
                                    }
                                }
                                else {
                                    CustomToast.showToast(context: _buildContext, "暂不支持该二维码类型");
                                    // CustomToast.show(new CustomToastItem(context: _buildContext, "暂不支持该二维码类型"));
                                }
                            }
                            else if (!qrCode.Equals("pop")) {
                                CustomToast.showToast(context: _buildContext, "暂不支持该二维码类型");
                                // CustomToast.show(new CustomToastItem(context: _buildContext, "暂不支持该二维码类型"));
                            }

                            StatusBarManager.hideStatusBar(false);
                            StatusBarManager.statusBarStyle(isLight: StoreProvider.store.getState().loginState
                                .isLoggedIn);
                        }
                            break;
                    }
                }
            }
        }

        public static void PushToQRScan(BuildContext context) {
            if (!Application.isEditor) {
                _buildContext = context;
                addListener();
                pushToQRScan();
                AnalyticsManager.AnalyticsQRScan(state: QRState.click);
            }
        }

#if UNITY_IOS
        [DllImport("__Internal")]
        static extern void pushToQRScan();
#elif UNITY_ANDROID
        static AndroidJavaObject _plugin;

        static AndroidJavaObject Plugin() {
            if (_plugin == null) {
                using (
                    var managerClass =
                        new AndroidJavaClass("com.unity3d.unityconnect.plugins.QRScanPlugin")
                ) {
                    _plugin = managerClass.CallStatic<AndroidJavaObject>("getInstance");
                }
            }

            return _plugin;
        }

        static void pushToQRScan() {
            Plugin().Call("pushToQRScan");
        }
#else
        static void pushToQRScan() {
        }
#endif
    }
}