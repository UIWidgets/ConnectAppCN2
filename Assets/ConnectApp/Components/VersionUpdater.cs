using System.Collections.Generic;
using ConnectApp.Api;
using ConnectApp.Common.Constant;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Components.Toast;
using ConnectApp.Main;
using ConnectApp.Models.Api;
using ConnectApp.Plugins;
using ConnectApp.redux;
using ConnectApp.redux.actions;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace ConnectApp.Components {
    public class VersionUpdater : StatefulWidget {
        public readonly Widget child;

        public VersionUpdater(
            Widget child = null,
            Key key = null
        ) : base(key: key) {
            this.child = child;
        }

        public override State createState() {
            return new _VersionUpdaterState();
        }
    }

    public class _VersionUpdaterState : State<VersionUpdater> {
        public override void initState() {
            base.initState();
            this.fetchInitData();
            StatusBarManager.hideStatusBar(false);
            SplashManager.fetchSplash();
            AnalyticsManager.AnalyticsOpenApp();
            SchedulerBinding.instance.addPostFrameCallback(_ => {
                CommonPlugin.init(buildContext: this.context);
                CommonPlugin.addListener();
                CustomToast.init(buildContext: this.context);
                if (UserInfoManager.isLoggedIn()) {
                    var userId = UserInfoManager.getUserInfo().userId ?? "";
                    if (userId.isNotEmpty()) {
                        StoreProvider.store.dispatcher.dispatch(CActions.fetchUserProfile(userId: userId));
                    }
                }

                StoreProvider.store.dispatcher.dispatch(CActions.fetchReviewUrl());
            });
        }

        void fetchInitData() {
            LoginApi.InitData().then(initDataResponse => {
                var initData = (FetchInitDataResponse)initDataResponse;
                var vs = initData.VS;
                var serverConfig = initData.config;
                var isRealName = initData.isRealName;
                if (vs.isNotEmpty()) {
                    HttpManager.updateCookie($"VS={vs}");
                }

                if (serverConfig.minVersionCode.isNotEmpty()) {
                    if (!int.TryParse(s: serverConfig.minVersionCode, out var minVersionCode)) {
                        return;
                    }

                    if (minVersionCode > 0 && minVersionCode > Config.versionCode) {
                        // need update
                        Navigator.pushNamedAndRemoveUntil<object>(
                            context: this.context,
                            newRouteName: NavigatorRoutes.ForceUpdate,
                            route => false
                        );
                        VersionManager.saveMinVersionCode(versionCode: minVersionCode);
                    }
                }

                if (serverConfig.hiddenWexinIOS) {
                    LocalDataManager.setHiddenWeixinLoginIOS(hiddenWexin: serverConfig.hiddenWexinIOS);
                }
                else {
                    LocalDataManager.setHiddenWeixinLoginIOS(false);
                }

                if (serverConfig.hiddenWexinAndroid) {
                    LocalDataManager.setHiddenWeixinLoginAndroid(hiddenWexin: serverConfig.hiddenWexinAndroid);
                }
                else {
                    LocalDataManager.setHiddenWeixinLoginAndroid(false);
                }

                if (serverConfig.hiddenRegister) {
                    LocalDataManager.setHiddenRegisterButton(hiddenRegister: serverConfig.hiddenRegister);
                }
                else {
                    LocalDataManager.setHiddenRegisterButton(false);
                }

                if (isRealName) {
                    UserInfoManager.passRealName();
                }

                if (LocalDataManager.agreeTermsTime().isEmpty()) {
                    this.showTermDialog(true, "", null);
                }
                else if (serverConfig.termsUpdateTime.isNotEmpty()) {
                    var localLastUpdateTimespan = long.Parse(LocalDataManager.agreeTermsTime());
                    var severLastUpdateTimespan = long.Parse(s: serverConfig.termsUpdateTime);
                    if (severLastUpdateTimespan > localLastUpdateTimespan) {
                        // show terms update dialog
                        this.showTermDialog(false, termsUpdateTime: serverConfig.termsUpdateTime,
                            termsUpdateItems: serverConfig.termsUpdateItems);
                    }
                    else {
                        CommonPlugin.completed();
                    }
                }
                else {
                    CommonPlugin.completed();
                }

                SplashManager.hiddenAndroidSplash();
            });
        }

        void showTermDialog(bool isFirst, string termsUpdateTime, List<string> termsUpdateItems) {
            string title;
            string message;
            Widget messageWidget;
            if (isFirst) {
                title = "欢迎您使用 Unity Connect ！";
                message = "在使用我们的产品及/或服务之前，请您先仔细阅读并了解《用户服务协议》和《个人信息处理规则》。" +
                          "我们将严格按照上述协议为您提供服务，保护您的个人信息安全。如您点击“同意”即表示您已阅读并同意全部条款，" +
                          "可以开始使用我们的产品及/或服务。如您对任何条款有异议，请点击“不同意并退出 App ”停止登录并退出 Unity Connect 。" +
                          "您对以上内容（包括我们的协议）有任何疑问的，您可以通过 legal_china@unity3d.com 联系我们。";
                messageWidget = new RichText(
                    text: new TextSpan(
                        children: new List<InlineSpan> {
                            new TextSpan(
                                "在使用我们的产品及/或服务之前，请您先仔细阅读并了解",
                                style: CTextStyle.PLargeBodyWideSpacing
                            ),
                            new TextSpan(
                                "《用户服务协议》",
                                CTextStyle.PLargeBodyWideSpacing.copyWith(color: CColors.PrimaryBlue,
                                    decoration: TextDecoration.underline),
                                recognizer: new TapGestureRecognizer {
                                    onTap = () =>
                                        OpenUrlUtil.OpenUrl(buildContext: this.context, url: Config.termsOfService)
                                }
                            ),
                            new TextSpan(
                                " 和 ",
                                style: CTextStyle.PLargeBodyWideSpacing
                            ),
                            new TextSpan(
                                "《个人信息处理规则》",
                                CTextStyle.PLargeBodyWideSpacing.copyWith(color: CColors.PrimaryBlue,
                                    decoration: TextDecoration.underline),
                                recognizer: new TapGestureRecognizer {
                                    onTap = () =>
                                        OpenUrlUtil.OpenUrl(buildContext: this.context, url: Config.privacyPolicy)
                                }
                            ),
                            new TextSpan(
                                "。我们将严格按照上述协议为您提供服务，保护您的个人信息安全。如您点击“同意”即表示您已阅读并同意全部条款，可以开始使用我们的产品及/或服务。如您对任何条款有异议，请点击“不同意并退出App”停止登录并退出Unity Connect。您对以上内容（包括我们的协议）有任何疑问的，您可以通过 ",
                                style: CTextStyle.PLargeBodyWideSpacing
                            ),
                            new TextSpan(
                                text: Config.legalEmail,
                                CTextStyle.PLargeBlue.copyWith(decoration: TextDecoration.underline),
                                recognizer: new TapGestureRecognizer {
                                    onTap = () => Application.OpenURL("mailto:" + Config.legalEmail)
                                }
                            ),
                            new TextSpan(
                                " 联系我们。",
                                style: CTextStyle.PLargeBodyWideSpacing
                            )
                        }
                    )
                );
            }
            else {
                title = "协议更新通知";
                message = "您好，为了切实保护您的权益，我们根据业务开展的实际情况和相关法律法规要求更新了《用户服务协议》和《个人信息处理规则》，请您认真阅读，尤其是以下条款：\n";
                var itemWidgets = new List<InlineSpan> {
                    new TextSpan(
                        "您好，为了切实保护您的权益，我们根据业务开展的实际情况和相关法律法规要求更新了",
                        style: CTextStyle.PLargeBodyWideSpacing
                    ),
                    new TextSpan(
                        "《用户服务协议》",
                        CTextStyle.PLargeBodyWideSpacing.copyWith(color: CColors.PrimaryBlue,
                            decoration: TextDecoration.underline),
                        recognizer: new TapGestureRecognizer {
                            onTap = () => OpenUrlUtil.OpenUrl(buildContext: this.context, url: Config.termsOfService)
                        }
                    ),
                    new TextSpan(
                        " 和 ",
                        style: CTextStyle.PLargeBodyWideSpacing
                    ),
                    new TextSpan(
                        "《个人信息处理规则》",
                        CTextStyle.PLargeBodyWideSpacing.copyWith(color: CColors.PrimaryBlue,
                            decoration: TextDecoration.underline),
                        recognizer: new TapGestureRecognizer {
                            onTap = () => OpenUrlUtil.OpenUrl(buildContext: this.context, url: Config.privacyPolicy)
                        }
                    ),
                    new TextSpan(
                        "，请您认真阅读，尤其是以下条款：\n",
                        style: CTextStyle.PLargeBodyWideSpacing
                    )
                };
                if (termsUpdateItems.isNotNullAndEmpty()) {
                    for (var i = 0; i < termsUpdateItems.Count; i++) {
                        var itemString = $"{i + 1}．{termsUpdateItems[index: i]}\n";
                        message += itemString;
                        itemWidgets.Add(new TextSpan(text: itemString, style: CTextStyle.PLargeBodyWideSpacing));
                    }
                }

                message += "如您点击“同意”即表示您已阅读并同意全部条款，可以继续使用我们的产品及/或服务。" +
                           "如您对任何条款有异议，请点击“不同意并退出 App”停止使用并退出 Unity Connect 。" +
                           "您对以上内容（包括我们的协议）有任何疑问的，您可以通过 legal_china@unity3d.com 联系我们。";
                itemWidgets.AddRange(new List<InlineSpan> {
                    new TextSpan(
                        "如您点击“同意”即表示您已阅读并同意全部条款，可以继续使用我们的产品及/或服务。" +
                        "如您对任何条款有异议，请点击“不同意并退出 App”停止使用并退出 Unity Connect 。" +
                        "您对以上内容（包括我们的协议）有任何疑问的，您可以通过 ",
                        style: CTextStyle.PLargeBodyWideSpacing
                    ),
                    new TextSpan(
                        text: Config.legalEmail,
                        CTextStyle.PLargeBodyWideSpacing.copyWith(color: CColors.PrimaryBlue,
                            decoration: TextDecoration.underline),
                        recognizer: new TapGestureRecognizer {
                            onTap = () => Application.OpenURL("mailto:" + Config.legalEmail)
                        }
                    ),
                    new TextSpan(
                        " 联系我们。",
                        style: CTextStyle.PLargeBodyWideSpacing
                    )
                });
                messageWidget = new RichText(
                    text: new TextSpan(
                        children: itemWidgets
                    )
                );
            }

            // show terms update
            CustomDialogUtils.showCustomDialog(
                context: this.context,
                barrierColor: CColors.Shadow,
                child: new CustomAlertDialog(
                    title: title,
                    messageWidget: messageWidget,
                    message: message,
                    messagePadding: EdgeInsets.symmetric(12, 24),
                    actions: new List<Widget> {
                        new CustomButton(
                            child: new Center(
                                child: new Text(
                                    "不同意并退出 App",
                                    style: CTextStyle.PLargeBody5.defaultHeight(),
                                    textAlign: TextAlign.center
                                )
                            ),
                            onPressed: Application.Quit
                        ),
                        new CustomButton(
                            child: new Center(
                                child: new Text(
                                    "同意",
                                    style: CTextStyle.PLargeBlue.defaultHeight(),
                                    textAlign: TextAlign.center
                                )
                            ),
                            onPressed: () => {
                                LocalDataManager.setAgreeTermsTime(termsUpdateTime.isNotEmpty()
                                    ? termsUpdateTime
                                    : DateTimeHelper.NowUnixTimestamp().ToString()
                                );
                                CustomDialogUtils.hiddenCustomDialog(context: this.context);
                                CommonPlugin.completed();
                            }
                        )
                    }
                )
            );
        }

        public override Widget build(BuildContext context) {
            return this.widget.child;
        }
    }
}