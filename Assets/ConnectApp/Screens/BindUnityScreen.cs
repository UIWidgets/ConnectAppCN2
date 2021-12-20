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
using ConnectApp.redux.actions;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace ConnectApp.screens {
    public enum FromPage {
        wechat,
        login,
        setting
    }

    public class BindUnityScreenConnector : StatelessWidget {
        public BindUnityScreenConnector(
            FromPage fromPage,
            Action popToMainRoute = null,
            Key key = null
        ) : base(key: key) {
            this.fromPage = fromPage;
            this.popToMainRoute = popToMainRoute;
        }

        readonly Action popToMainRoute;
        readonly FromPage fromPage;

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, BindUnityScreenViewModel>(
                converter: state => new BindUnityScreenViewModel {
                    fromPage = this.fromPage,
                    loginEmail = state.loginState.email,
                    loginPassword = state.loginState.password,
                    loginBtnEnable = state.loginState.email.Length > 0 && state.loginState.password.Length > 0
                },
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new BindUnityScreenActionModel {
                        popToMainRoute = this.popToMainRoute,
                        openUrl = url =>
                            dispatcher.dispatch(new UtilsAction.OpenUrlAction {url = url}),
                        openCreateUnityIdUrl = () =>
                            dispatcher.dispatch<Future>(CActions.openCreateUnityIdUrl()),
                        changeEmail = text =>
                            dispatcher.dispatch(new LoginChangeEmailAction {changeText = text}),
                        changePassword = text =>
                            dispatcher.dispatch(new LoginChangePasswordAction {changeText = text}),
                        clearEmailAndPassword = () => dispatcher.dispatch(new CleanEmailAndPasswordAction()),
                        loginByEmail = () => dispatcher.dispatch<Future>(CActions.loginByEmail())
                    };
                    return new BindUnityScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }


    public class BindUnityScreen : StatefulWidget {
        public BindUnityScreen(
            BindUnityScreenViewModel viewModel = null,
            BindUnityScreenActionModel actionModel = null,
            Key key = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly BindUnityScreenViewModel viewModel;
        public readonly BindUnityScreenActionModel actionModel;

        public override State createState() {
            return new _BindUnityScreenState();
        }
    }

    public class _BindUnityScreenState : State<BindUnityScreen> {
        readonly FocusNode _emailFocusNode = new FocusNode();
        readonly FocusNode _passwordFocusNode = new FocusNode();
        FocusScopeNode _focusScopeNode;

        bool _isEmailFocus;
        bool _isPasswordFocus;
        bool _loginLoading;

        public override void initState() {
            base.initState();
            StatusBarManager.statusBarStyle(false);
            this._isEmailFocus = true;
            this._isPasswordFocus = false;
            this._loginLoading = false;
            this._emailFocusNode.addListener(listener: this._focusNodeListener);
            this._passwordFocusNode.addListener(listener: this._focusNodeListener);
            SchedulerBinding.instance.addPostFrameCallback(_ => {
                if (this.widget.viewModel.loginEmail.Length > 0 || this.widget.viewModel.loginPassword.Length > 0) {
                    this.widget.actionModel.clearEmailAndPassword();
                }
            });
        }

        void _focusNodeListener() {
            if (this._isEmailFocus == this._emailFocusNode.hasFocus &&
                this._isPasswordFocus == this._passwordFocusNode.hasFocus) {
                return;
            }

            if (!(this._emailFocusNode.hasFocus && this._passwordFocusNode.hasFocus)) {
                this._isEmailFocus = this._emailFocusNode.hasFocus;
                this._isPasswordFocus = this._passwordFocusNode.hasFocus;
                this.setState(() => { });
            }
        }

        void _login() {
            if (!this.widget.viewModel.loginBtnEnable || this._loginLoading) {
                return;
            }

            this._emailFocusNode.unfocus();
            this._passwordFocusNode.unfocus();
            this._loginLoading = true;
            this.setState(() => {});
            this.widget.actionModel.loginByEmail().then(_ => {
                this.widget.actionModel.popToMainRoute();
            }).catchError(err => {
                    this._loginLoading = false;
                    this.setState(() => {});
                    CustomToast.showToast(context: this.context, "登录失败", type: ToastType.Error);
                    // CustomToast.show(new CustomToastItem(context: this.context, "登录失败"));
                    // TODO: SnackBar
                }
            );
        }

        public override Widget build(BuildContext context) {
            return new Container(
                color: CColors.White,
                child: new CustomSafeArea(
                    top: false,
                    bottom: false,
                    child: this._buildContent(context: context)
                )
            );
        }

        Widget _buildContent(BuildContext context) {
            return new GestureDetector(
                onTap: () => {
                    if (this._emailFocusNode.hasFocus) {
                        this._emailFocusNode.unfocus();
                    }

                    if (this._passwordFocusNode.hasFocus) {
                        this._passwordFocusNode.unfocus();
                    }
                },
                child: new Container(
                    padding: EdgeInsets.only(top: CCommonUtils.getSafeAreaTopPadding(context: context)),
                    decoration: new BoxDecoration(
                        gradient: new LinearGradient(
                            colors: new List<Color> {
                                new Color(0xFFF7FBFF),
                                new Color(0xFFEAF5FD)
                            },
                            begin: Alignment.topLeft,
                            end: Alignment.bottomRight
                        )
                    ),
                    child: new Column(
                        children: new List<Widget> {
                            this._buildTopView(),
                            this._buildMiddleView(context: context),
                            this._buildBottomView()
                        }
                    )
                )
            );
        }

        Widget _buildTopView() {
            Widget leftWidget;
            switch (this.widget.viewModel.fromPage) {
                case FromPage.login: {
                    leftWidget = new CustomButton(
                        onPressed: () => {
                            Navigator.pop(context: this.context);
                            this.widget.actionModel.clearEmailAndPassword();
                        },
                        child: new Icon(
                            icon: CIcons.arrow_back,
                            size: 24,
                            color: CColors.Icon
                        )
                    );
                    break;
                }

                case FromPage.wechat: {
                    leftWidget = new CustomButton(
                        onPressed: () => {
                            this.widget.actionModel.popToMainRoute();
                            this.widget.actionModel.clearEmailAndPassword();
                        },
                        child: new Text(
                            "跳过",
                            style: CTextStyle.PLargeBody4
                        )
                    );
                    break;
                }

                case FromPage.setting: {
                    leftWidget = new CustomButton(
                        onPressed: () => Navigator.pop(context: this.context),
                        child: new Icon(
                            icon: CIcons.arrow_back,
                            size: 24,
                            color: CColors.Icon
                        )
                    );
                    break;
                }

                default:
                    leftWidget = new Container();
                    break;
            }

            return new Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: new List<Widget> {
                    new Container(
                        height: 44,
                        padding: EdgeInsets.only(8, 8, 8),
                        child: leftWidget
                    ),
                    new Container(height: 16),
                    new Container(
                        padding: EdgeInsets.symmetric(horizontal: 16),
                        child: new Text(
                            this.widget.viewModel.fromPage == FromPage.login ? "登录你的Unity账号" : "绑定你的Unity账号",
                            style: new TextStyle(
                                height: 1.11f,
                                fontSize: 32,
                                fontFamily: "Roboto-Bold",
                                color: CColors.PrimaryBlue
                            )
                        )
                    )
                }
            );
        }

        Widget _buildMiddleView(BuildContext context) {
            return new Container(
                padding: EdgeInsets.symmetric(horizontal: 16),
                child: new Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: new List<Widget> {
                        new Container(height: 32),
                        new Container(
                            height: 44,
                            decoration: new BoxDecoration(
                                color: CColors.Transparent,
                                border: new Border(
                                    bottom: new BorderSide(
                                        this._isEmailFocus ? CColors.PrimaryBlue : CColors.Separator,
                                        this._isEmailFocus ? 2 : 1
                                    )
                                )
                            ),
                            alignment: Alignment.center,
                            child: new InputField(
                                focusNode: this._emailFocusNode,
                                maxLines: 1,
                                autofocus: true,
                                height: 42,
                                enabled: !this._loginLoading,
                                style: CTextStyle.PLargeBody,
                                hintText: "请输入邮箱",
                                hintStyle: CTextStyle.PLargeBody5,
                                cursorColor: CColors.PrimaryBlue,
                                clearButtonMode: InputFieldClearButtonMode.whileEditing,
                                keyboardType: TextInputType.emailAddress,
                                onChanged: text => this.widget.actionModel.changeEmail(obj: text),
                                onSubmitted: _ => {
                                    if (null == this._focusScopeNode) {
                                        this._focusScopeNode = FocusScope.of(context: context);
                                    }

                                    this._focusScopeNode.requestFocus(node: this._passwordFocusNode);
                                }
                            )
                        ),
                        new Container(height: 24),
                        new Container(
                            height: 44,
                            decoration: new BoxDecoration(
                                color: CColors.Transparent,
                                border: new Border(
                                    bottom: new BorderSide(
                                        this._isPasswordFocus ? CColors.PrimaryBlue : CColors.Separator,
                                        this._isPasswordFocus ? 2 : 1
                                    )
                                )
                            ),
                            alignment: Alignment.center,
                            child: new InputField(
                                focusNode: this._passwordFocusNode,
                                maxLines: 1,
                                autofocus: false,
                                obscureText: true,
                                height: 42,
                                enabled: !this._loginLoading,
                                style: CTextStyle.PLargeBody,
                                hintText: "请输入密码",
                                hintStyle: CTextStyle.PLargeBody5,
                                cursorColor: CColors.PrimaryBlue,
                                clearButtonMode: InputFieldClearButtonMode.whileEditing,
                                onChanged: text => this.widget.actionModel.changePassword(obj: text),
                                onSubmitted: _ => this._login()
                            )
                        )
                    }
                )
            );
        }

        Widget _buildBottomView() {
            Widget right;
            Widget register;
            if (this._loginLoading) {
                right = new CustomActivityIndicator(
                    loadingColor: LoadingColor.white,
                    size: LoadingSize.small
                );
            }
            else {
                right = new Container();
            }

            if (LocalDataManager.hiddenRegisterButton()) {
                register = new Container();
            }
            else {
                register = new CustomButton(
                    padding: EdgeInsets.zero,
                    onPressed: () => this.widget.actionModel.openCreateUnityIdUrl(),
                    child: new Text(
                        "创建 Unity ID",
                        style: CTextStyle.PRegularBlue.defaultHeight()
                    )
                );
            }

            return new Container(
                padding: EdgeInsets.symmetric(horizontal: 16),
                margin: EdgeInsets.only(top: 40),
                child: new Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: new List<Widget> {
                        new CustomButton(
                            onPressed: this._login,
                            padding: EdgeInsets.zero,
                            child: new Container(
                                height: 48,
                                decoration: new BoxDecoration(this.widget.viewModel.loginBtnEnable
                                        ? this._loginLoading
                                            ? CColors.ButtonActive
                                            : CColors.PrimaryBlue
                                        : CColors.PrimaryBlue.withOpacity(0.5f),
                                    borderRadius: BorderRadius.all(24)
                                ),
                                child: new Stack(
                                    children: new List<Widget> {
                                        new Align(
                                            alignment: Alignment.center,
                                            child: new Text(
                                                "确定",
                                                maxLines: 1,
                                                style: CTextStyle.PLargeWhite
                                            )
                                        ),
                                        new Positioned(
                                            right: 24,
                                            height: 48,
                                            child: right
                                        )
                                    }
                                )
                            )
                        ),
                        new Container(height: 16),
                        new Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: new List<Widget> {
                                register,
                                new CustomButton(
                                    padding: EdgeInsets.zero,
                                    onPressed: () =>
                                        this.widget.actionModel.openUrl($"{Config.idBaseUrl}/password/new"),
                                    child: new Text(
                                        "忘记密码",
                                        style: CTextStyle.PRegularBody3.defaultHeight()
                                    )
                                )
                            }
                        )
                    }
                )
            );
        }
    }
}