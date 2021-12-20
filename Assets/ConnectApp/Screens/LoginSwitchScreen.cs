using System;
using System.Collections.Generic;
using ConnectApp.Common.Constant;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Components;
using ConnectApp.Components.Toast;
using ConnectApp.Models.ActionModel;
using ConnectApp.Models.State;
using ConnectApp.Models.ViewModel;
using ConnectApp.Plugins;
using ConnectApp.redux.actions;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Image = Unity.UIWidgets.widgets.Image;

namespace ConnectApp.screens {
    public class LoginSwitchScreenConnector : StatelessWidget {
        public LoginSwitchScreenConnector(
            Action popToMainRoute,
            Key key = null
        ) : base(key: key) {
            this.popToMainRoute = popToMainRoute;
        }

        Action popToMainRoute;

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, LoginSwitchScreenViewModel>(
                converter: state => new LoginSwitchScreenViewModel {
                    loginInfo = state.loginState.loginInfo
                },
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new LoginSwitchScreenActionModel {
                        popToMainRoute = this.popToMainRoute,
                        loginByWechatAction = (code, action) => dispatcher.dispatch<Future>(CActions.loginByWechat(code: code, action: action)),
                        openUrl = url => dispatcher.dispatch(new UtilsAction.OpenUrlAction {url = url})
                    };
                    return new LoginSwitchScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }

    public class LoginSwitchScreen : StatefulWidget {
        public LoginSwitchScreen(
            LoginSwitchScreenViewModel viewModel,
            LoginSwitchScreenActionModel actionModel,
            Key key = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly LoginSwitchScreenViewModel viewModel;
        public readonly LoginSwitchScreenActionModel actionModel;

        public override State createState() {
            return new _LoginSwitchScreen();
        }
    }

    class _LoginSwitchScreen : State<LoginSwitchScreen>, RouteAware {

        bool _anonymous;
        bool isCheck;
        
        public override void initState() {
            base.initState();
            this._anonymous = true;
            StatusBarManager.statusBarStyle(true);
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
            // TODO: doesn't work
            QRScanPlugin.qrCodeToken = null;
        }

        public void didPopNext() {
        }

        public void didPush() {
        }

        public void didPushNext() {
            // TODO: doesn't work
            StatusBarManager.statusBarStyle(true);
        }

        public override Widget build(BuildContext context) {
            return new Container(
                color: CColors.White,
                child: new Stack(
                    children: new List<Widget> {
                        new Container(
                            width: CCommonUtils.getScreenWidth(buildContext: context),
                            height: CCommonUtils.getScreenHeight(buildContext: context),
                            child: Image.file(
                                "image/img-bg-login.png",
                                fit: BoxFit.cover
                            )
                        ),
                        new Positioned(
                            child: new Column(
                                children: new List<Widget> {
                                    this._buildTopView(context: context),
                                    this._buildBottomView(context: context)
                                }
                            )
                        )
                    }
                )
            );
        }

        Widget _buildTopView(BuildContext context) {
            return new Flexible(
                child: new Stack(
                    children: new List<Widget> {
                        new Positioned(
                            top: CCommonUtils.getSafeAreaTopPadding(context: context),
                            left: 0,
                            child: new CustomButton(
                                padding: EdgeInsets.symmetric(10, 16),
                                onPressed: () => this.widget.actionModel.popToMainRoute(),
                                child: new Icon(
                                    icon: CIcons.close,
                                    size: 24,
                                    color: CColors.White
                                )
                            )
                        ),
                        new Align(
                            alignment: Alignment.center,
                            child: new Container(
                                height: 78,
                                child: new Column(
                                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                    children: new List<Widget> {
                                        new Container(
                                            width: 241,
                                            height: 53,
                                            child: Image.file(
                                                "image/img-logo-unity-connect-white-with-shadow.png",
                                                fit: BoxFit.cover
                                            )
                                        ),
                                        new Text(
                                            "Unity   问   题   全   搞   定",
                                            maxLines: 1,
                                            style: CTextStyle.H5.copyWith(color: CColors.White, height: 1)
                                        )
                                    }
                                )
                            )
                        )
                    }
                )
            );
        }

        Widget _buildBottomView(BuildContext context) {
            return new Container(
                padding: EdgeInsets.symmetric(horizontal: 16),
                child: new Column(
                    children: new List<Widget> {
                        this._buildWeChatButton(context: context),
                        new Container(height: 16),
                        new CustomButton(
                            onPressed: () => {
                                if (!this.isCheck) {
                                    this.showTipsView();
                                    return;
                                }
                                Navigator.pushNamed(context: context, routeName: LoginNavigatorRoutes.BindUnity);
                            },
                            padding: EdgeInsets.zero,
                            child: new Container(
                                height: 48,
                                decoration: new BoxDecoration(
                                    border: Border.all(color: CColors.White, 2),
                                    borderRadius: BorderRadius.all(24)
                                ),
                                child: new Row(
                                    mainAxisAlignment: MainAxisAlignment.center,
                                    children: new List<Widget> {
                                        new Text(
                                            "使用 Unity ID 登录",
                                            maxLines: 1,
                                            style: CTextStyle.PLargeMediumWhite
                                        )
                                    }
                                )
                            )
                        ),
                        new Container(
                            margin: EdgeInsets.only(top: 16),
                            child: new Row(
                                mainAxisAlignment: MainAxisAlignment.center,
                                children: new List<Widget> {
                                    new CustomCheckbox(
                                        value: this.isCheck,
                                        value => {
                                            this.isCheck = value;
                                            this.setState(() => {});
                                        },
                                        size: 16,
                                        shape: CheckboxShape.Square
                                    ),
                                    new RichText(
                                        text: new TextSpan(
                                            children: new List<InlineSpan> {
                                                new TextSpan(
                                                    "我已阅读并同意 ",
                                                    style: CTextStyle.PSmallWhite
                                                ),
                                                new TextSpan(
                                                    "用户服务协议",
                                                    CTextStyle.PSmallWhite.copyWith(decoration: TextDecoration.underline),
                                                    recognizer: new TapGestureRecognizer {
                                                        onTap = () =>
                                                            this.widget.actionModel.openUrl(obj: Config.termsOfService)
                                                    }
                                                ),
                                                new TextSpan(
                                                    " 和 ",
                                                    style: CTextStyle.PSmallWhite
                                                ),
                                                new TextSpan(
                                                    "个人信息处理规则",
                                                    CTextStyle.PSmallWhite.copyWith(decoration: TextDecoration.underline),
                                                    recognizer: new TapGestureRecognizer {
                                                        onTap = () => this.widget.actionModel.openUrl(obj: Config.privacyPolicy)
                                                    }
                                                )
                                            }
                                        )
                                    )
                                }
                            )
                        ),
                        new Container(
                            height: 16 + CCommonUtils.getSafeAreaBottomPadding(context: context)
                        )
                    }
                )
            );
        }

        Widget _buildWeChatButton(BuildContext context) {
            // should config weixin sdk for use.
            return new Container();

            if (!WechatPlugin.instance().isInstalled()) {
                return new Container();
            }

            WechatPlugin.instance().context = context;
            return new CustomButton(
                onPressed: () => {
                    if (!this.isCheck) {
                        this.showTipsView();
                        return;
                    }
                    WechatPlugin.instance(code => {
                            CustomDialogUtils.showCustomDialog(context: this.context, child: new CustomLoadingDialog());
                            this.widget.actionModel.loginByWechatAction(arg1: code, anonymous => {
                                this._anonymous = anonymous;
                                this.setState(() => {});
                            }).then(_ => {
                                CustomDialogUtils.hiddenCustomDialog(context: context);
                                if (this._anonymous) {
                                    Navigator.pushReplacementNamed(
                                        context: context, 
                                        routeName: LoginNavigatorRoutes.WechatBindUnity
                                    );
                                }
                                else {
                                    this.widget.actionModel.popToMainRoute();
                                }
                            }).catchError(err => {
                                CustomDialogUtils.hiddenCustomDialog(context: context);
                                CustomToast.showToast(context: this.context, "登录失败，请重试。", type: ToastType.Error);
                            });
                        }
                    ).login(Guid.NewGuid().ToString());
                },
                padding: EdgeInsets.zero,
                child: new Container(
                    height: 48,
                    decoration: new BoxDecoration(
                        color: CColors.White,
                        borderRadius: BorderRadius.all(24)
                    ),
                    child: new Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: new List<Widget> {
                            new Icon(
                                icon: CIcons.WechatIcon,
                                size: 24,
                                color: CColors.PrimaryBlue
                            ),
                            new Container(width: 8),
                            new Text(
                                "使用微信账号登录",
                                maxLines: 1,
                                style: CTextStyle.PLargeMediumBlue
                            )
                        }
                    )
                )
            );
        }

        void showTipsView() {
            CustomDialogUtils.showCustomDialog(
                context: this.context,
                barrierColor: Color.fromRGBO(0, 0, 0, 0.5f),
                child: new CustomAlertDialog(
                    "尊敬的用户，你需要仔细阅读并勾选下方的 用户服务协议 和 个人信息处理规则，才可进行登录。",
                    null,
                    new List<Widget> {
                        new CustomButton(
                            child: new Center(
                                child: new Text(
                                    "确定",
                                    style: CTextStyle.PLargeBlue,
                                    textAlign: TextAlign.center
                                )
                            ),
                            onPressed: () => {
                                CustomDialogUtils.hiddenCustomDialog(context: this.context);
                            }
                        )
                    }
                )
            );
        }
    }
}