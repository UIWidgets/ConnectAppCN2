using System;
using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace ConnectApp.Components {
    public class CustomDialog : StatelessWidget {
        readonly Color backgroundColor;
        readonly Widget child;
        readonly Widget header;
        readonly Curve insetAnimationCurve;
        readonly TimeSpan insetAnimationDuration;
        readonly float radius;

        public CustomDialog(
            Key key = null,
            Color backgroundColor = null,
            TimeSpan? insetAnimationDuration = null,
            Curve insetAnimationCurve = null,
            float? radius = null,
            Widget child = null,
            Widget header = null
        ) : base(key: key) {
            this.backgroundColor = backgroundColor;
            this.insetAnimationDuration = insetAnimationDuration ?? TimeSpan.FromMilliseconds(100);
            this.insetAnimationCurve = insetAnimationCurve ?? Curves.decelerate;
            this.radius = radius ?? 0;
            this.child = child;
            this.header = header;
        }

        public override Widget build(BuildContext context) {
            return new AnimatedPadding(
                padding: MediaQuery.of(context: context).viewInsets +
                         EdgeInsets.symmetric(horizontal: 40.0f, vertical: 24.0f),
                duration: this.insetAnimationDuration,
                curve: this.insetAnimationCurve,
                child: MediaQuery.removeViewInsets(
                    removeLeft: true,
                    removeTop: true,
                    removeRight: true,
                    removeBottom: true,
                    context: context,
                    child: new Center(
                        child: new ConstrainedBox(
                            constraints: new BoxConstraints(
                                280.0f,
                                CCommonUtils.getScreenWidth(buildContext: context) - 80
                            ),
                            child: new Column(
                                mainAxisAlignment: MainAxisAlignment.center,
                                children: new List<Widget> {
                                    this.header ?? new Container(),
                                    new Container(
                                        decoration: new BoxDecoration(
                                            color: this.backgroundColor,
                                            borderRadius: this.header == null
                                                ? BorderRadius.all(radius: this.radius)
                                                : BorderRadius.only(
                                                    bottomRight: this.radius,
                                                    bottomLeft: this.radius
                                                )
                                        ),
                                        child: this.child
                                    )
                                }
                            )
                        )
                    )
                )
            );
        }
    }

    public class CustomLoadingDialog : StatelessWidget {
        
        readonly TimeSpan? duration;
        readonly string message;
        readonly Widget widget;

        public CustomLoadingDialog(
            Widget widget = null,
            string message = null,
            TimeSpan? duration = null,
            Key key = null
        ) : base(key: key) {
            this.widget = widget;
            this.message = message;
            this.duration = duration;
        }

        public override Widget build(BuildContext context) {
            if (this.duration != null) {
                Future.delayed((TimeSpan)this.duration)
                    .then(_ => Navigator.pop(context: context));
            }

            return new GestureDetector(
                onTap: () => { },
                child: new Container(
                    child: new Center(
                        child: new Container(
                            width: 132,
                            height: 110,
                            decoration: new BoxDecoration(
                                Color.fromRGBO(0, 0, 0, 0.8f),
                                borderRadius: BorderRadius.circular(4)
                            ),
                            child: new Column(
                                mainAxisSize: MainAxisSize.min,
                                mainAxisAlignment: MainAxisAlignment.center,
                                children: new List<Widget> {
                                    this.widget ?? new CustomActivityIndicator(loadingColor: LoadingColor.white),
                                    _buildMessage(message: this.message)
                                }
                            )
                        )
                    )
                )
            );
        }

        static Widget _buildMessage(string message) {
            if (message == null || message.Length <= 0) {
                return new Container();
            }

            return new Container(
                margin: EdgeInsets.only(top: 8, left: 8, right: 8),
                child: new Text(
                    data: message,
                    style: CTextStyle.PRegularWhite,
                    textAlign: TextAlign.center
                )
            );
        }
    }

    public class CustomAlertDialog : StatelessWidget {
        readonly TextStyle _messageStyle = CTextStyle.PLargeBody;
        readonly List<Widget> actions;
        readonly Widget header;
        readonly string message;
        readonly EdgeInsets messagePadding;
        readonly Widget messageWidget;

        readonly string title;

        public CustomAlertDialog(
            string title = null,
            string message = null,
            List<Widget> actions = null,
            Widget header = null,
            Widget messageWidget = null,
            EdgeInsets messagePadding = null,
            Key key = null
        ) : base(key: key) {
            this.title = title;
            this.message = message;
            this.actions = actions;
            this.header = header;
            this.messageWidget = messageWidget;
            this.messagePadding = messagePadding;
        }

        public override Widget build(BuildContext context) {
            var children = new List<Widget>();
            if (this.title.isNotEmpty()) {
                children.Add(
                    new Container(
                        padding: EdgeInsets.only(16, 16, 16, this.message == null ? 24 : 8),
                        alignment: Alignment.center,
                        child: new Text(
                            data: this.title,
                            style: CTextStyle.PLargeMedium,
                            textAlign: TextAlign.center
                        )
                    ));
            }
            else {
                children.Add(new Container(height: 1, color: CColors.White));
            }

            if (this.message.isNotEmpty() || this.messageWidget != null) {
                var mediaQuery = MediaQuery.of(context: context);
                var horizontalPadding = mediaQuery.viewInsets.left + mediaQuery.viewInsets.right + 40 + 40;
                var width = mediaQuery.size.width - horizontalPadding - 32;
                var maxHeight = CTextUtils.CalculateTextHeight(
                    text: this.message, textStyle: this._messageStyle, textWidth: width, 7);
                var totalHeight = CTextUtils.CalculateTextHeight(
                    text: this.message, textStyle: this._messageStyle, textWidth: width);
                if (maxHeight < totalHeight) {
                    maxHeight += 16;
                }
                else {
                    maxHeight += 48;
                }

                children.Add(
                    new Container(
                        height: maxHeight,
                        child: new CustomScrollbar(
                            child: new SingleChildScrollView(
                                child: new Padding(
                                    padding: this.messagePadding ?? EdgeInsets.all(24),
                                    child: this.messageWidget ?? new Text(
                                        data: this.message,
                                        style: this._messageStyle
                                    )
                                )
                            )
                        )
                    ));
            }

            if (this.actions != null) {
                children.Add(new CustomDivider(
                    height: 1
                ));

                var _children = new List<Widget>();
                foreach (var _child in this.actions) {
                    _children.Add(new Expanded(
                        child: new Stack(
                            fit: StackFit.expand,
                            children: new List<Widget> {
                                _child
                            }
                        )
                    ));
                    var index = this.actions.IndexOf(item: _child);
                    if (index < this.actions.Count - 1) {
                        _children.Add(new Container(
                            width: 1,
                            color: CColors.Separator
                        ));
                    }
                }

                Widget child = new Container(
                    height: 48,
                    child: new Row(
                        mainAxisAlignment: MainAxisAlignment.start,
                        children: _children
                    )
                );
                children.Add(item: child);
            }

            Widget dialogChild = new IntrinsicWidth(
                child: new Column(
                    mainAxisSize: MainAxisSize.min,
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: children
                ),
                stepWidth: MediaQuery.of(context: context).size.width - 80
            );

            return new CustomDialog(
                backgroundColor: CColors.White,
                radius: 8,
                child: dialogChild,
                header: this.header
            );
        }
    }

    public static class CustomDialogUtils {
        public static void showCustomDialog(
            BuildContext context,
            bool barrierDismissible = false,
            Color barrierColor = null,
            Widget child = null,
            VoidCallback onPop = null
        ) {
            var route = new _DialogRoute(
                (context1, animation, secondaryAnimation) => new CustomSafeArea(child: child),
                barrierDismissible: barrierDismissible,
                barrierColor ?? Color.fromRGBO(0, 0, 0, 0.01f),
                new TimeSpan(0, 0, 0, 0, 150),
                transitionBuilder: _transitionBuilder
            );
            Navigator.of(context: context, true).push(route: route).then(_ => onPop?.Invoke());
        }

        public static void hiddenCustomDialog(BuildContext context) {
            Navigator.of(context: context, true).pop<object>();
            // // TODO: Finn can pop
            // // if (Navigator.canPop(context: context)) {
            //     Navigator.pop(context: context);
            // // }
        }

        public static void showToast(BuildContext context, string message, IconData iconData, int seconds = 1) {
            showCustomDialog(
                context: context,
                child: new CustomLoadingDialog(
                    new Icon(
                        icon: iconData,
                        size: 27,
                        color: Color.fromRGBO(199, 203, 207, 1)
                    ),
                    message: message,
                    TimeSpan.FromSeconds(value: seconds)
                )
            );
        }

        static Widget _transitionBuilder(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation, Widget child) {
            return new FadeTransition(
                opacity: new CurvedAnimation(
                    parent: animation,
                    curve: Curves.easeOut
                ),
                child: child
            );
        }

        public static Future<T> showCustomDialog<T>(
            BuildContext context = null,
            bool barrierDismissible = false,
            Widget child = null,
            WidgetBuilder builder = null,
            bool useRootNavigator = true,
            RouteSettings routeSettings = null
        ) {
            D.assert(child == null || builder == null);
            return DialogUtils.showGeneralDialog<T>(
                context: context,
                (buildContext, animation, secondaryAnimation) => {
                    var pageChild = child ?? new Builder(builder: builder);
                    return new SafeArea(
                        child: new Builder(
                            builder: _ => pageChild
                        )
                    );
                },
                barrierDismissible: barrierDismissible,
                barrierColor: CColors.Black54,
                transitionDuration: new TimeSpan(0, 0, 0, 0, 150),
                transitionBuilder: _transitionBuilder,
                useRootNavigator: useRootNavigator,
                routeSettings: routeSettings
            );
        }
    }

    class _DialogRoute : PopupRoute {
        readonly RoutePageBuilder pageBuilder;

        readonly RouteTransitionsBuilder transitionBuilder;

        public _DialogRoute(
            RoutePageBuilder pageBuilder = null,
            bool barrierDismissible = true,
            Color barrierColor = null,
            TimeSpan? transitionDuration = null,
            RouteTransitionsBuilder transitionBuilder = null,
            RouteSettings setting = null
        ) : base(settings: setting) {
            this.pageBuilder = pageBuilder;
            this.barrierDismissible = barrierDismissible;
            this.barrierColor = barrierColor ?? new Color(0x80000000);
            this.transitionDuration = transitionDuration ?? TimeSpan.FromMilliseconds(200);
            this.transitionBuilder = transitionBuilder;
        }

        public override bool barrierDismissible { get; }

        public override Color barrierColor { get; }

        public override TimeSpan transitionDuration { get; }

        public override Widget buildPage(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation) {
            return this.pageBuilder(context: context, animation: animation, secondaryAnimation: secondaryAnimation);
        }

        public override Widget buildTransitions(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation, Widget child) {
            if (this.transitionBuilder == null) {
                return new FadeTransition(
                    opacity: new CurvedAnimation(
                        parent: animation,
                        curve: Curves.linear
                    ),
                    child: child
                );
            }

            return this.transitionBuilder(
                context: context, animation: animation, secondaryAnimation: secondaryAnimation, child: child);
        }
    }

    class RawDialogRoute : PopupRoute {
        readonly RoutePageBuilder pageBuilder;

        readonly RouteTransitionsBuilder transitionBuilder;

        public RawDialogRoute(
            RoutePageBuilder pageBuilder = null,
            bool barrierDismissible = true,
            Color barrierColor = null,
            string barrierLabel = null,
            TimeSpan? transitionDuration = null,
            RouteTransitionsBuilder transitionBuilder = null,
            RouteSettings setting = null
        ) : base(settings: setting) {
            this.pageBuilder = pageBuilder;
            this.barrierDismissible = barrierDismissible;
            this.barrierColor = barrierColor ?? new Color(0x80000000);
            this.barrierLabel = barrierLabel;
            this.transitionDuration = transitionDuration ?? TimeSpan.FromMilliseconds(200);
            this.transitionBuilder = transitionBuilder;
        }

        public override bool barrierDismissible { get; }

        public override string barrierLabel { get; }

        public override Color barrierColor { get; }

        public override TimeSpan transitionDuration { get; }

        public override Widget buildPage(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation) {
            return this.pageBuilder(context: context, animation: animation, secondaryAnimation: secondaryAnimation);
        }

        public override Widget buildTransitions(BuildContext context, Animation<float> animation,
            Animation<float> secondaryAnimation, Widget child) {
            if (this.transitionBuilder == null) {
                return new FadeTransition(
                    opacity: new CurvedAnimation(
                        parent: animation,
                        curve: Curves.linear
                    ),
                    child: child
                );
            }

            return this.transitionBuilder(
                context: context, animation: animation, secondaryAnimation: secondaryAnimation, child: child);
        }
    }
}