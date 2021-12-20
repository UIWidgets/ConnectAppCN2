using System.Collections.Generic;
using ConnectApp.Common.Constant;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Components;
using ConnectApp.Models.ActionModel;
using ConnectApp.Models.State;
using ConnectApp.redux.actions;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace ConnectApp.screens {
    public class AboutUsScreenConnector : StatelessWidget {
        public AboutUsScreenConnector(
            Key key = null
        ) : base(key: key) {
        }

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, object>(
                converter: state => null,
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new AboutUsScreenActionModel {
                        toOriginCode = () => dispatcher.dispatch(new UtilsAction.OpenUrlAction {url = Config.originCodeUrl}),
                        toWidgetOriginCode = () =>
                            dispatcher.dispatch(new UtilsAction.OpenUrlAction {url = Config.widgetOriginCodeUrl}),
                        openUrl = url => dispatcher.dispatch(new UtilsAction.OpenUrlAction {url = url})
                    };
                    return new AboutUsScreen(actionModel: actionModel);
                }
            );
        }
    }

    public class AboutUsScreen : StatelessWidget {
        public AboutUsScreen(
            AboutUsScreenActionModel actionModel = null,
            Key key = null
        ) : base(key: key) {
            this.actionModel = actionModel;
        }

        readonly AboutUsScreenActionModel actionModel;

        public override Widget build(BuildContext context) {
            StatusBarManager.statusBarStyle(false);
            return new Container(
                color: CColors.White,
                child: new CustomSafeArea(
                    child: new Container(
                        color: CColors.Background,
                        child: new Column(
                            children: new List<Widget> {
                                this._buildNavigationBar(context: context),
                                new Expanded(
                                    child: this._buildContent()
                                )
                            }
                        )
                    )
                )
            );
        }

        Widget _buildNavigationBar(BuildContext context) {
            return new CustomAppBar(
                () => Navigator.pop(context: context),
                new Text(
                    "关于我们",
                    style: CTextStyle.PXLargeMedium
                )
            );
        }

        Widget _buildContent() {
            return new Container(
                color: CColors.White,
                child: new ListView(
                    children: new List<Widget> {
                        new Container(
                            margin: EdgeInsets.only(top: 44, bottom: 16),
                            alignment: Alignment.center,
                            child: Image.file(
                                "image/unity-icon-logo.png",
                                width: 128,
                                height: 128,
                                fit: BoxFit.cover
                            )
                        ),
                        new Container(
                            alignment: Alignment.center,
                            child: Image.file(
                                "image/unity-text-logo.png",
                                width: 149,
                                height: 24,
                                fit: BoxFit.cover
                            )
                        ),
                        new Container(height: 8),
                        new Row(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: new List<Widget> {
                                new Text(
                                    $"版本号：{Config.versionName} ({Config.versionCode})",
                                    style: CTextStyle.PRegularBody4
                                )
                            }
                        ),
                        new Container(
                            padding: EdgeInsets.only(32, 24, 32, 32),
                            child: new Text(
                                "Unity Connect 是使用 UIWidgets 开发的移动端项目，是一个开放而友好的社区，每个开发者都能在这里学习或者分享自己的作品。",
                                style: CTextStyle.PRegularBody
                            )
                        ),
                        new Container(color: CColors.Background, height: 16),
                        new CustomListTile(
                            title: "关注本项目源代码",
                            trailing: CustomListTileConstant.defaultTrailing,
                            onTap: () => this.actionModel.toOriginCode()
                        ),
                        new CustomListTile(
                            title: "关注 UIWidgets 项目源代码",
                            trailing: CustomListTileConstant.defaultTrailing,
                            onTap: () => this.actionModel.toWidgetOriginCode()
                        ),
                        new CustomListTile(
                            title: "用户服务协议",
                            trailing: CustomListTileConstant.defaultTrailing,
                            onTap: () => this.actionModel.openUrl(obj: Config.termsOfService)
                        ),
                        new CustomListTile(
                            title: "个人信息处理规则",
                            trailing: CustomListTileConstant.defaultTrailing,
                            onTap: () => this.actionModel.openUrl(obj: Config.privacyPolicy)
                        )
                    }
                )
            );
        }
    }
}