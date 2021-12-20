using System;
using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Components;
using ConnectApp.Plugins;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace ConnectApp.screens {
    public class VideoViewScreen : StatefulWidget {
        public VideoViewScreen(
            string url,
            string verifyType,
            int limitSeconds,
            Key key = null
        ) : base(key: key) {
            this.url = url;
            this.verifyType = verifyType;
            this.limitSeconds = limitSeconds;
        }

        public readonly string url;
        public readonly string verifyType;
        public readonly int limitSeconds;

        public override State createState() {
            return new _VideoViewScreenState();
        }
    }

    public class _VideoViewScreenState : State<VideoViewScreen>, RouteAware {
        public override void initState() {
            base.initState();
            StatusBarManager.hideStatusBar(true);
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            Main.ConnectApp.routeObserver.subscribe(this, (PageRoute) ModalRoute.of(context: this.context));
        }

        public override void dispose() {
            Main.ConnectApp.routeObserver.unsubscribe(this);
            base.dispose();
        }

        public override Widget build(BuildContext context) {
            return new Container(
                color: CColors.Black,
                child: new CustomSafeArea(
                    child: new Container(
                        color: CColors.Black,
                        child: new Stack(
                            children: new List<Widget> {
                                new Positioned(
                                    top: 0, left: 16, right: 0, child: new Container(
                                        child: new Row(
                                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                            children: new List<Widget> {
                                                new CustomButton(
                                                    onPressed: () => Navigator.pop(context: context),
                                                    child: new Icon(
                                                        icon: CIcons.arrow_back,
                                                        size: 28,
                                                        color: CColors.White
                                                    )
                                                )
                                            }
                                        )
                                    ))
                            }
                        )
                    )
                )
            );
        }

        public void didPopNext() {
            AVPlayerPlugin.showPlayer();
        }

        public void didPush() {
            StatusBarManager.hideStatusBar(true);
            Future.delayed(TimeSpan.FromMilliseconds(400)).then(_ => {
                var width = MediaQuery.of(context: this.context).size.width;
                var height = width * 9 / 16;
                var originY = (MediaQuery.of(context: this.context).size.height - height) / 2;
                AVPlayerPlugin.initVideoPlayer(buildContext: this.context, url: this.widget.url, HttpManager.getCookie(), 0, top: originY,
                    width: width, height: height,
                    false,
                    verifyType: this.widget.verifyType, limitSeconds: this.widget.limitSeconds);
            });
        }

        public void didPop() {
            AVPlayerPlugin.removePlayer();
            StatusBarManager.hideStatusBar(false);
        }

        public void didPushNext() {
            AVPlayerPlugin.hiddenPlayer();
        }
    }
}