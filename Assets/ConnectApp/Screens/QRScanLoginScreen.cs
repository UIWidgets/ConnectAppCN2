using System;
using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Components;
using ConnectApp.Models.State;
using ConnectApp.Models.ViewModel;
using ConnectApp.redux.actions;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace ConnectApp.screens {
    public class QRScanLoginScreenConnector : StatelessWidget {
        public QRScanLoginScreenConnector(
            string token,
            Key key = null
        ) : base(key: key) {
            this.token = token;
        }

        readonly string token;

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, QRScanLoginScreenViewModel>(
                converter: state => new QRScanLoginScreenViewModel {
                    userId = state.loginState.loginInfo.userId,
                    userDict = state.userState.userDict
                },
                builder: (context1, viewModel, dispatcher) => {
                    return new QRScanLoginScreen(
                        viewModel: viewModel,
                        () => dispatcher.dispatch<Future>(CActions.loginByQr(token: this.token, "confirm")),
                        () => dispatcher.dispatch<Future>(CActions.loginByQr(token: this.token, "cancel"))
                    );
                }
            );
        }
    }

    public class QRScanLoginScreen : StatefulWidget {
        public QRScanLoginScreen(
            QRScanLoginScreenViewModel viewModel = null,
            Func<Future> loginByQr = null,
            Func<Future> cancelLoginByQr = null,
            Key key = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.loginByQr = loginByQr;
            this.cancelLoginByQr = cancelLoginByQr;
        }

        public readonly QRScanLoginScreenViewModel viewModel;
        public readonly Func<Future> loginByQr;
        public readonly Func<Future> cancelLoginByQr;

        public override State createState() {
            return new _QRScanLoginScreenState();
        }
    }

    public class _QRScanLoginScreenState : State<QRScanLoginScreen>, RouteAware {
        bool _needCancel;

        public override void initState() {
            base.initState();
            this._needCancel = true;
            StatusBarManager.statusBarStyle(false);
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            Main.ConnectApp.routeObserver.subscribe(this, (PageRoute) ModalRoute.of(context: this.context));
        }

        public override void dispose() {
            Main.ConnectApp.routeObserver.unsubscribe(this);
            base.dispose();
        }

        public void didPop() {
            if (this._needCancel) {
                this.widget.cancelLoginByQr().catchError(error => {
                    AnalyticsManager.AnalyticsQRScan(state: QRState.cancel, false);
                });
            }
        }

        public void didPopNext() {
        }

        public void didPush() {
        }

        public void didPushNext() {
        }

        public override Widget build(BuildContext context) {
            return new Container(
                color: CColors.White,
                child: new CustomSafeArea(
                    child: new Container(
                        color: CColors.White,
                        child: new Column(
                            children: new List<Widget> {
                                this._buildNavigationBar(),
                                this._buildTopView(),
                                this._buildBottomView()
                            }
                        )
                    )
                )
            );
        }

        Widget _buildNavigationBar() {
            return new CustomAppBar(
                () => Navigator.pop(context: this.context),
                new Text(
                    "扫描结果",
                    style: CTextStyle.PXLargeMedium
                ),
                bottomSeparatorColor: CColors.Transparent
            );
        }

        Widget _buildTopView() {
            var user = this.widget.viewModel.userDict[key: this.widget.viewModel.userId];
            return new Flexible(
                child: new Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    children: new List<Widget> {
                        new Container(
                            margin: EdgeInsets.only(bottom: 12),
                            child: new Icon(
                                icon: CIcons.computer,
                                size: 120,
                                color: CColors.TextBody
                            )
                        ),
                        new RichText(
                            text: new TextSpan(
                                children: new List<InlineSpan> {
                                    new TextSpan(
                                        "即将使用",
                                        style: CTextStyle.PLargeBody5
                                    ),
                                    new TextSpan(
                                        $" {user.fullName ?? user.name} ",
                                        style: CTextStyle.PLargeBlue
                                    ),
                                    new TextSpan(
                                        "登录",
                                        style: CTextStyle.PLargeBody5
                                    )
                                }
                            )
                        )
                    }
                )
            );
        }

        Widget _buildBottomView() {
            return new Container(
                padding: EdgeInsets.only(bottom: 120),
                child: new Column(
                    children: new List<Widget> {
                        new CustomButton(
                            padding: EdgeInsets.zero,
                            onPressed: () => {
                                this._needCancel = false;
                                CustomDialogUtils.showCustomDialog<object>(
                                    context: this.context,
                                    false,
                                    new CustomLoadingDialog(
                                        message: "登录中"
                                    )
                                );
                                this.widget.loginByQr()
                                    .then(_ => {
                                        CustomDialogUtils.hiddenCustomDialog(context: this.context);
                                        Navigator.pop(context: this.context);
                                    })
                                    .catchError(error => {
                                        CustomDialogUtils.hiddenCustomDialog(context: this.context);
                                        AnalyticsManager.AnalyticsQRScan(state: QRState.confirm, false);
                                        Navigator.pop(context: this.context);
                                    });
                            },
                            child: new Container(
                                height: 48,
                                margin: EdgeInsets.only(16, right: 16),
                                alignment: Alignment.center,
                                decoration: new BoxDecoration(
                                    color: CColors.PrimaryBlue,
                                    borderRadius: BorderRadius.all(24)
                                ),
                                child: new Text(
                                    "确定登录",
                                    style: CTextStyle.PLargeWhite
                                )
                            )
                        ),
                        new Container(height: 12),
                        new CustomButton(
                            padding: EdgeInsets.zero,
                            onPressed: () => Navigator.pop(context: this.context),
                            child: new Container(
                                color: CColors.Transparent,
                                height: 48,
                                margin: EdgeInsets.only(16, right: 16),
                                alignment: Alignment.center,
                                child: new Text(
                                    "取消",
                                    style: CTextStyle.PLargeBody5
                                )
                            )
                        )
                    }
                )
            );
        }
    }
}