using System.Collections.Generic;
using ConnectApp.Api;
using ConnectApp.Common.Constant;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Components;
using ConnectApp.Models.ActionModel;
using ConnectApp.Models.Api;
using ConnectApp.Models.State;
using ConnectApp.Models.ViewModel;
using ConnectApp.redux.actions;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace ConnectApp.screens {
    public class RealNameScreenConnector : StatelessWidget {
        public RealNameScreenConnector(
            Key key = null
        ) : base(key: key) {
        }

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, RealNameScreenViewModel>(
                converter: state => new RealNameScreenViewModel(),
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new RealNameScreenActionModel {
                        openUrl = url => dispatcher.dispatch(new UtilsAction.OpenUrlAction {url = url})
                    };
                    return new RealNameScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }


    public class RealNameScreen : StatefulWidget {
        public RealNameScreen(
            Key key = null,
            RealNameScreenViewModel viewModel = null,
            RealNameScreenActionModel actionModel = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly RealNameScreenViewModel viewModel;
        public readonly RealNameScreenActionModel actionModel;

        public override State createState() {
            return new _RealNameScreenState();
        }
    }

    class _RealNameScreenState : State<RealNameScreen> {
        FocusNode _IDCardFocusNode;
        FocusNode _phoneNumberFocusNode;
        FocusNode _realNameFocusNode;
        FocusScopeNode _focusScopeNode;

        bool _isIDCardFocus;
        bool _isPhoneNumberFocus;
        bool _isRealNameFocus;

        string _idCard;
        string _realName;

        public override void initState() {
            base.initState();
            this._IDCardFocusNode = new FocusNode();
            this._phoneNumberFocusNode = new FocusNode();
            this._realNameFocusNode = new FocusNode();
            this._isPhoneNumberFocus = true;
            this._isIDCardFocus = false;
            this._isRealNameFocus = false;
            this._IDCardFocusNode.addListener(listener: this._focusNodeListener);
            this._phoneNumberFocusNode.addListener(listener: this._focusNodeListener);
            this._realNameFocusNode.addListener(listener: this._focusNodeListener);
            this._idCard = "";
            this._realName = "";
        }

        void _focusNodeListener() {
            if (this._isIDCardFocus == this._IDCardFocusNode.hasFocus
                && this._isPhoneNumberFocus == this._phoneNumberFocusNode.hasFocus
                && this._isRealNameFocus == this._realNameFocusNode.hasFocus) {
                return;
            }

            if (this._IDCardFocusNode.hasFocus && this._phoneNumberFocusNode.hasFocus &&
                this._realNameFocusNode.hasFocus) {
                return;
            }

            this._isIDCardFocus = this._IDCardFocusNode.hasFocus;
            this._isPhoneNumberFocus = this._phoneNumberFocusNode.hasFocus;
            this._isRealNameFocus = this._realNameFocusNode.hasFocus;
            this.setState(() => { });
        }

        public override Widget build(BuildContext context) {
            return new Container(
                color: CColors.White,
                child: new CustomSafeArea(
                    child: this._buildWidget(context: context)
                )
            );
        }

        Widget _buildWidget(BuildContext context) {
            return new GestureDetector(
                onTap: () => {
                    if (this._IDCardFocusNode.hasFocus) {
                        this._IDCardFocusNode.unfocus();
                    }

                    if (this._phoneNumberFocusNode.hasFocus) {
                        this._phoneNumberFocusNode.unfocus();
                    }

                    if (this._realNameFocusNode.hasFocus) {
                        this._realNameFocusNode.unfocus();
                    }
                },
                child: new Container(
                    color: CColors.White,
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
            return new Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: new List<Widget> {
                    new Container(
                        height: 44,
                        padding: EdgeInsets.only(8, 8, 8),
                        child: new Row(
                            mainAxisAlignment: MainAxisAlignment.end,
                            children: new List<Widget> {
                                new CustomButton(
                                    onPressed: () => Navigator.pop(context: this.context),
                                    child: new Icon(icon: CIcons.close, size: 24, color: CColors.Icon)
                                )
                            }
                        )
                    ),
                    new Container(
                        padding: EdgeInsets.only(16, 16, 16),
                        child: new Text(
                            "实名验证",
                            style: new TextStyle(
                                fontSize: 32,
                                fontFamily: "Roboto-Bold",
                                color: CColors.Black
                            )
                        )
                    ),
                    new Container(
                        padding: EdgeInsets.only(16, 16, 16),
                        child: new Text(
                            "根据相关法律法规，您需要完成真实身份验证后方可使用提问、回答、评论等功能",
                            style: CTextStyle.PLargeBody
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
                        new Row(
                            children: new List<Widget> {
                                new Container(
                                    width: 76,
                                    child: new Text(
                                        "真实姓名",
                                        style: CTextStyle.PLargeBody.defaultHeight()
                                    )
                                ),
                                new Expanded(
                                    child: new Container(
                                        height: 44,
                                        decoration: new BoxDecoration(
                                            color: CColors.Transparent,
                                            border: new Border(
                                                bottom: new BorderSide(
                                                    this._isRealNameFocus ? CColors.PrimaryBlue : CColors.Separator,
                                                    this._isRealNameFocus ? 2 : 1
                                                )
                                            )
                                        ),
                                        alignment: Alignment.center,
                                        child: new InputField(
                                            focusNode: this._realNameFocusNode,
                                            maxLines: 1,
                                            autofocus: false,
                                            height: 42,
                                            enabled: true,
                                            style: CTextStyle.PLargeBody,
                                            hintText: "请输入真实姓名",
                                            hintStyle: CTextStyle.PLargeBody5,
                                            cursorColor: CColors.PrimaryBlue,
                                            clearButtonMode: InputFieldClearButtonMode.whileEditing,
                                            keyboardType: TextInputType.text,
                                            onChanged: text => {
                                                this._realName = text;
                                                this.setState(() => { });
                                            },
                                            onSubmitted: _ => {
                                                if (null == this._focusScopeNode) {
                                                    this._focusScopeNode = FocusScope.of(context: context);
                                                }

                                                this._focusScopeNode.requestFocus(node: this._IDCardFocusNode);
                                            }
                                        )
                                    )
                                )
                            }
                        ),
                        new SizedBox(height: 24),
                        new Row(
                            children: new List<Widget> {
                                new Container(
                                    width: 76,
                                    child: new Text(
                                        "身份证号",
                                        style: CTextStyle.PLargeBody.defaultHeight()
                                    )
                                ),
                                new Expanded(
                                    child: new Container(
                                        height: 44,
                                        decoration: new BoxDecoration(
                                            color: CColors.Transparent,
                                            border: new Border(
                                                bottom: new BorderSide(
                                                    this._isIDCardFocus ? CColors.PrimaryBlue : CColors.Separator,
                                                    this._isIDCardFocus ? 2 : 1
                                                )
                                            )
                                        ),
                                        alignment: Alignment.center,
                                        child: new InputField(
                                            focusNode: this._IDCardFocusNode,
                                            maxLines: 1,
                                            autofocus: false,
                                            height: 42,
                                            enabled: true,
                                            style: CTextStyle.PLargeBody,
                                            hintText: "请输入身份证号",
                                            hintStyle: CTextStyle.PLargeBody5,
                                            cursorColor: CColors.PrimaryBlue,
                                            clearButtonMode: InputFieldClearButtonMode.whileEditing,
                                            keyboardType: TextInputType.text,
                                            onChanged: text => {
                                                this._idCard = text;
                                                this.setState(() => { });
                                            },
                                            onSubmitted: _ => { this.checkVerifyAll(); }
                                        )
                                    )
                                )
                            }
                        )
                    }
                )
            );
        }

        Widget _buildBottomView() {
            return new Container(
                margin: EdgeInsets.only(top: 40),
                child: new Column(
                    children: new List<Widget> {
                        new Row(
                            children: new List<Widget> {
                                new SizedBox(width: 16),
                                new Container(
                                    child: new RichText(
                                        text: new TextSpan(
                                            children: new List<InlineSpan> {
                                                new TextSpan(
                                                    "已阅读并同意 ",
                                                    CTextStyle.PLargeBody.defaultHeight()
                                                ),
                                                new TextSpan(
                                                    "《隐私政策》",
                                                    CTextStyle.PLargeBlue.copyWith(height: 1,
                                                        decoration: TextDecoration.underline),
                                                    recognizer: new TapGestureRecognizer {
                                                        onTap = () =>
                                                            this.widget.actionModel.openUrl(obj: Config.privacyPolicy)
                                                    }
                                                )
                                            }
                                        )
                                    )
                                )
                            }
                        ),
                        new CustomButton(
                            onPressed: this.checkVerifyAll,
                            padding: EdgeInsets.all(16),
                            child: new Container(
                                height: 48,
                                decoration: new BoxDecoration(
                                    color: CColors.PrimaryBlue,
                                    borderRadius: BorderRadius.all(8)
                                ),
                                child: new Row(
                                    mainAxisAlignment: MainAxisAlignment.center,
                                    children: new List<Widget> {
                                        new Text(
                                            "同意协议并认证",
                                            maxLines: 1,
                                            style: CTextStyle.PLargeMediumWhite.defaultHeight()
                                        )
                                    }
                                )
                            )
                        )
                    }
                )
            );
        }

        void checkVerifyAll() {
            if (!CCommonUtils.isRealName(realName: this._realName)) {
                CustomDialogUtils.showToast(context: this.context, "真实姓名输入有误", iconData: CIcons.sentiment_satisfied);
                return;
            }

            if (!CCommonUtils.isIDCard(idCard: this._idCard)) {
                CustomDialogUtils.showToast(context: this.context, "身份证信息输入有误", iconData: CIcons.sentiment_satisfied);
                return;
            }

            CustomDialogUtils.showCustomDialog(context: this.context, child: new CustomLoadingDialog(message: "核验中"));
            RealNameApi.CheckRealName(idCard: this._idCard, realName: this._realName)
                .then(data => {
                    if (!(data is CheckRealNameResponse response)) {
                        return;
                    }

                    if (response.success) {
                        Navigator.pop(context: this.context);
                        UserInfoManager.passRealName();
                    }

                    CustomDialogUtils.hiddenCustomDialog(context: this.context);
                    CustomDialogUtils.showToast(context: this.context, message: response.description,
                        iconData: CIcons.sentiment_satisfied);
                }).catchError(err => {
                    CustomDialogUtils.hiddenCustomDialog(context: this.context);
                    CustomDialogUtils.showToast(context: this.context, "服务出错", iconData: CIcons.sentiment_satisfied);
                });
        }
    }
}