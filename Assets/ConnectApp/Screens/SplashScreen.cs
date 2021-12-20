using System;
using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Components;
using ConnectApp.Main;
using ConnectApp.Plugins;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Image = Unity.UIWidgets.widgets.Image;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace ConnectApp.screens {
    public class SplashScreen : StatefulWidget {
        public SplashScreen(
            Key key = null
        ) : base(key: key) {
        }

        public override State createState() {
            return new _SplashScreenState();
        }
    }

    class _SplashScreenState : State<SplashScreen> {
        bool _isShow;
        int _lastSecond = 5;
        Timer _timer;
        uint hexColor;

        public override void initState() {
            base.initState();
            StatusBarManager.hideStatusBar(true);
            this._isShow = SplashManager.isExistSplash();
            if (this._isShow) {
                this._lastSecond = SplashManager.getSplash().duration;
                this._timer = Timer.periodic(TimeSpan.FromSeconds(1), _ => {
                    this.timeDown();
                    return null;
                });
                var splash = SplashManager.getSplash();
                AnalyticsManager.ShowSplashPage(id: splash.id, name: splash.name, url: splash.url);
            }

            var isShowLogo = SplashManager.getSplash().isShowLogo;
            var hexColorStr = SplashManager.getSplash().color;
            if (isShowLogo) {
                this.hexColor = 0xFFFFFFFF;
                try {
                    this.hexColor = Convert.ToUInt32(value: hexColorStr, 16);
                }
                catch (Exception) {
                    // Console.WriteLine(e);
                }
            }
        }

        public override void dispose() {
            this._timer?.cancel();
            this._timer = null;
            base.dispose();
        }

        public override Widget build(BuildContext context) {
            CommonPlugin.init(buildContext: this.context);
            CommonPlugin.addListener();
            if (!this._isShow) {
                return new MainScreen();
            }

            var topPadding = CCommonUtils.getSafeAreaTopPadding(context: context);
            var isShowLogo = SplashManager.getSplash()?.isShowLogo ?? false;

            Widget logoWidget;
            if (isShowLogo) {
                logoWidget = new Positioned(
                    top: topPadding + 24,
                    left: 16,
                    child: new Icon(
                        icon: CIcons.LogoWithUnity,
                        size: 35,
                        color: new Color(value: this.hexColor)
                    )
                );
            }
            else {
                logoWidget = new Container();
            }

            return new Container(
                color: CColors.White,
                child: new CustomSafeArea(
                    top: false,
                    child: new Stack(
                        children: new List<Widget> {
                            new Column(
                                mainAxisAlignment: MainAxisAlignment.end,
                                children: new List<Widget> {
                                    new GestureDetector(
                                        child: new Container(
                                            width: MediaQuery.of(context: context).size.width,
                                            height: MediaQuery.of(context: context).size.height - 116 -
                                                    CCommonUtils.getSafeAreaBottomPadding(context: context),
                                            child: Image.memory(SplashManager.readImage(), fit: BoxFit.cover)
                                        ),
                                        onTap: this.pushPage
                                    ),
                                    new Container(
                                        width: 176,
                                        height: 32,
                                        margin: EdgeInsets.only(top: 36),
                                        child: Image.file("image/unityConnectBlack.png")
                                    ),
                                    new Container(
                                        width: 101,
                                        height: 22,
                                        margin: EdgeInsets.only(top: 6, bottom: 20),
                                        child: Image.file("image/madeWithUnity.png")
                                    )
                                }
                            ),
                            new Positioned(
                                top: topPadding + 24,
                                right: 16,
                                child: new GestureDetector(
                                    child: new Container(
                                        decoration: new BoxDecoration(
                                            Color.fromRGBO(0, 0, 0, 0.5f),
                                            borderRadius: BorderRadius.all(16)
                                        ),
                                        width: 65,
                                        height: 32,
                                        alignment: Alignment.center,
                                        child: new Text(
                                            $"跳过 {this._lastSecond}",
                                            style: new TextStyle(
                                                fontSize: 14,
                                                fontFamily: "Roboto-Regular",
                                                color: CColors.White
                                            )
                                        )
                                    ),
                                    onTap: () => this.pushCallback(true)
                                )
                            ),
                            logoWidget
                        }
                    ))
            );
        }

        void pushPage() {
            this.cancelTimer();
            var splash = SplashManager.getSplash();
            AnalyticsManager.ClickSplashPage(id: splash.id, name: splash.name, url: splash.url);
            Navigator.pushReplacementNamed(
                context: this.context,
                routeName: NavigatorRoutes.Main
            );
            CommonPlugin.init(buildContext: this.context);
            CommonPlugin.openUrlScheme(schemeUrl: splash.url);
        }

        void pushCallback(bool clickSkip) {
            this.cancelTimer();
            var splash = SplashManager.getSplash();
            if (clickSkip) {
                AnalyticsManager.ClickSkipSplashPage(id: splash.id, name: splash.name, url: splash.url);
            }
            else {
                AnalyticsManager.TimeUpDismissSplashPage(id: splash.id, name: splash.name, url: splash.url);
            }

            Navigator.pushReplacementNamed(
                context: this.context,
                routeName: NavigatorRoutes.Main
            );
        }

        void cancelTimer() {
            this._timer?.cancel();
        }

        void timeDown() {
            using (Isolate.getScope(isolate: Isolate.current)) {
                this.setState(() => { this._lastSecond -= 1; });
                if (this._lastSecond < 1) {
                    this.pushCallback(false);
                }
            }
        }
    }
}