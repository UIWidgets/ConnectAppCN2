using System;
using System.Collections.Generic;
using System.Web;
using ConnectApp.Api;
using ConnectApp.Common.Util;
using ConnectApp.Components.Toast;
using ConnectApp.Main;
using ConnectApp.Models.Model;
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
    public static class CommonPlugin {
        static bool isListen;
        static string hmsToken;
        static BuildContext _buildContext;

        public static void init(BuildContext buildContext) {
            _buildContext = buildContext;
        }

        public static void addListener() {
            if (Application.isEditor) {
                return;
            }

            if (!isListen) {
                isListen = true;
                UIWidgetsMessageManager.instance.AddChannelMessageDelegate("common", del: _handleMethodCall);
            }
        }

        static void _handleMethodCall(string method, List<JSONNode> args) {
            var isolate = UIWidgetsPanel.anyIsolate;
            if (isolate != null) {
                using (Isolate.getScope(isolate: isolate)) {
                    switch (method) {
                        case "CompletedCallback": {
                            if (args.isEmpty()) {
                                return;
                            }

                            var node = args.first();
                            var dict = JSON.Parse(aJSON: node);
                            var isPush = (bool)dict["push"];
                            if (isPush) {
                                Navigator.pushReplacementNamed(
                                    context: _buildContext,
                                    routeName: NavigatorRoutes.Main
                                );
                            }
                            else {
                                if (VersionManager.needForceUpdate()) {
                                    SplashManager.hiddenAndroidSplash();
                                }
                                else {
                                    Navigator.pushReplacementNamed(
                                        context: _buildContext,
                                        SplashManager.isExistSplash()
                                            ? NavigatorRoutes.Splash
                                            : NavigatorRoutes.Main
                                    );
                                }
                            }

                            break;
                        }
                        case "OnOpenNotification": {
                            if (args.isEmpty()) {
                                return;
                            }

                            //点击应用通知栏
                            var node = args.first();
                            var dict = JSON.Parse(aJSON: node);
                            var type = dict["type"];
                            var subType = dict["subtype"];
                            string id = dict["id"] ?? "";
                            if (id.isEmpty()) {
                                id = dict["channelId"] ?? "";
                            }

                            AnalyticsManager.AnalyticsWakeApp("OnOpenNotification", id: id, type: type,
                                subtype: subType);
                            AnalyticsManager.ClickNotification(type: type, subtype: subType, id: id);
                            pushPage(type: type, subType: subType, id: id, true);
                            break;
                        }
                        case "OnReceiveNotification": {
                            //接收到推送
                            if (args.isEmpty()) {
                            }

                            break;
                        }
                        case "OnReceiveMessage": {
                            //接收到应用内消息
                            break;
                        }
                        case "OnOpenUrl": {
                            if (args.isEmpty()) {
                                return;
                            }

                            AnalyticsManager.AnalyticsWakeApp("OnOpenUrl", args.first());
                            openUrlScheme(args.first());
                            break;
                        }
                        case "OnOpenUniversalLinks": {
                            if (args.isEmpty()) {
                                return;
                            }

                            AnalyticsManager.AnalyticsWakeApp("OnOpenUniversalLinks", args.first());
                            openUniversalLink(args.first());
                            break;
                        }
                        case "RegisterToken": {
                            if (args.isEmpty()) {
                                return;
                            }

                            var node = args.first();
                            var dict = JSON.Parse(aJSON: node);
                            var token = (string)dict["token"];
                            hmsToken = token;
                            registerHmsToken(UserInfoManager.isLoggedIn() ? UserInfoManager.getUserInfo().userId : "");
                            break;
                        }
                        case "SaveImageSuccess": {
                            CustomToast.showToast(context: _buildContext, "保存成功");
                            break;
                        }
                        case "SaveImageError": {
                            CustomToast.showToast(context: _buildContext, "保存失败，请检查权限", type: ToastType.Error);
                            break;
                        }
                    }
                }
            }
        }

        public static void openUniversalLink(string link) {
            if (link.isEmpty()) {
                return;
            }

            var uri = new Uri(uriString: link);
            if (uri.AbsolutePath.StartsWith("/connectapplink/")) {
                var type = "";
                if (uri.AbsolutePath.Equals("/connectapplink/project_detail")) {
                    type = "project";
                }
                else if (uri.AbsolutePath.Equals("/connectapplink/game")) {
                    type = "game";
                }
                else if (uri.AbsolutePath.Equals("/connectapplink/ask")) {
                    type = "ask";
                }
                else {
                    return;
                }

                var subType = HttpUtility.ParseQueryString(query: uri.Query).Get("type");
                var id = HttpUtility.ParseQueryString(query: uri.Query).Get("id");
                pushPage(type: type, subType: subType, id: id);
            }
            else {
                pushPage("webView", "", id: link);
            }
        }

        public static void openUrlScheme(string schemeUrl) {
            if (schemeUrl.isEmpty()) {
                return;
            }

            var uri = new Uri(uriString: schemeUrl);
            if (uri.Scheme.Equals("unityconnect")) {
                AnalyticsManager.EnterOnOpenUrl(url: schemeUrl);
                if (uri.Host.Equals("connectapp")) {
                    var type = "";
                    if (uri.AbsolutePath.Equals("/project_detail")) {
                        type = "project";
                    }
                    else if (uri.AbsolutePath.Equals("/user")) {
                        type = "user";
                    }
                    else if (uri.AbsolutePath.Equals("/team")) {
                        type = "team";
                    }
                    else if (uri.AbsolutePath.Equals("/rank")) {
                        type = "rank";
                    }
                    else if (uri.AbsolutePath.Equals("/game")) {
                        type = "game";
                    }
                    else if (uri.AbsolutePath.Equals("/ask")) {
                        type = "ask";
                    }
                    else if (uri.AbsolutePath.Equals("/web")) {
                        type = "web";
                    }
                    else {
                        return;
                    }

                    var subType = HttpUtility.ParseQueryString(query: uri.Query).Get("type");
                    var id = HttpUtility.ParseQueryString(query: uri.Query).Get("id");
                    pushPage(type: type, subType: subType, id: id);
                }
            }
            else {
                pushPage("web", "external", id: schemeUrl);
            }
        }

        static void pushPage(string type, string subType, string id, bool isPush = false) {
            if (id.isEmpty()) {
                if (!(type == "rank" || type == "notification")) {
                    return;
                }
            }

            if (VersionManager.needForceUpdate()) {
                return;
            }

            if (type == "project") {
                if (subType == "article") {
                    AnalyticsManager.ClickEnterArticleDetail("Push_Article", articleId: id, $"PushArticle_{id}");
                    if (CTemporaryValue.currentPageModelId.isNotEmpty() && id == CTemporaryValue.currentPageModelId) {
                        return;
                    }

                    Navigator.pushNamed(
                        context: _buildContext,
                        routeName: NavigatorRoutes.ArticleDetail,
                        new ArticleDetailScreenArguments {
                            id = id,
                            isPush = isPush
                        }
                    );
                }
            }
            else if (type == "team") {
                if (CTemporaryValue.currentPageModelId.isNotEmpty() && id == CTemporaryValue.currentPageModelId) {
                    return;
                }

                if (subType == "follower") {
                    Navigator.pushNamed(
                        context: _buildContext,
                        routeName: NavigatorRoutes.TeamDetail,
                        new TeamDetailScreenArguments {
                            id = id
                        }
                    );
                }
            }
            else if (type == "user") {
                if (CTemporaryValue.currentPageModelId.isNotEmpty() && id == CTemporaryValue.currentPageModelId) {
                    return;
                }

                if (subType == "follower") {
                    Navigator.pushNamed(
                        context: _buildContext,
                        routeName: NavigatorRoutes.UserDetail,
                        new UserDetailScreenArguments {
                            id = id
                        }
                    );
                }
            }
            else if (type == "ask") {
                if (subType == "question") {
                    if (CTemporaryValue.currentPageModelId.isNotEmpty() && id == CTemporaryValue.currentPageModelId) {
                        return;
                    }

                    Navigator.pushNamed(
                        context: _buildContext,
                        routeName: NavigatorRoutes.QuestionDetail,
                        new ScreenArguments {
                            id = id
                        }
                    );
                }
                else if (subType == "answer") {
                    var idStrings = id.Split('_');
                    if (idStrings.isNotNullAndEmpty() && idStrings.Length == 2) {
                        var questionId = idStrings.first();
                        var answerId = idStrings.last();
                        if (CTemporaryValue.currentPageModelId.isNotEmpty() &&
                            answerId == CTemporaryValue.currentPageModelId) {
                            return;
                        }

                        Navigator.pushNamed(
                            context: _buildContext,
                            routeName: NavigatorRoutes.AnswerDetail,
                            new AnswerDetailScreenArguments {
                                questionId = questionId,
                                answerId = answerId
                            }
                        );
                    }
                }
            }
            else if (type == "notification") {
                if (CTemporaryValue.currentPageModelId.isNotEmpty() &&
                    NavigatorRoutes.Notification.Equals(value: CTemporaryValue.currentPageModelId)) {
                    return;
                }

                Navigator.pushNamed(
                    context: _buildContext,
                    routeName: NavigatorRoutes.Notification
                );
            }
            else if (type == "rank") {
                var initIndex = 0;
                switch (subType) {
                    case "column": {
                        initIndex = 1;
                        break;
                    }
                    case "blogger": {
                        initIndex = 2;
                        break;
                    }
                }

                Navigator.pushNamed(
                    context: _buildContext,
                    routeName: NavigatorRoutes.LeaderBoard,
                    new LeaderBoardScreenArguments {
                        initIndex = initIndex
                    }
                );
            }
            else if (type.Equals("web")) {
                if (subType.Equals("internal")) {
                    Application.OpenURL(url: id);
                }
                else if (subType.Equals("external")) {
                    Application.OpenURL(url: id);
                }
            }
        }

        public static void registerHmsToken(string userId = "") {
            // if userid is "" mean unregister token
            if (hmsToken.isNotEmpty()) {
                UserApi.RegisterToken(token: hmsToken, userId: userId);
            }
        }

        public static void completed() {
            if (Application.isEditor) {
                Navigator.pushReplacementNamed(
                    context: _buildContext,
                    SplashManager.isExistSplash()
                        ? NavigatorRoutes.Splash
                        : NavigatorRoutes.Main
                );
                return;
            }

            listenCompleted();
        }

#if UNITY_IOS
        [DllImport("__Internal")]
        static extern void listenCompleted();

#elif UNITY_ANDROID
        static AndroidJavaObject _plugin;

        static AndroidJavaObject Plugin() {
            if (_plugin == null) {
                using (
                    var managerClass = new AndroidJavaClass("com.unity3d.unityconnect.plugins.CommonPlugin")
                ) {
                    _plugin = managerClass.CallStatic<AndroidJavaObject>("getInstance");
                }
            }

            return _plugin;
        }

        static void listenCompleted() {
            Plugin().Call("listenCompleted");
        }
#else
        static void listenCompleted() {
        }
#endif
    }
}