using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Components;
using ConnectApp.Main;
using ConnectApp.Models.ActionModel;
using ConnectApp.Models.Model;
using ConnectApp.Models.State;
using ConnectApp.Models.ViewModel;
using ConnectApp.Plugins;
using ConnectApp.redux.actions;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;
using Avatar = ConnectApp.Components.Avatar;

namespace ConnectApp.screens {
    public class PersonalScreenConnector : StatelessWidget {
        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, PersonalScreenViewModel>(
                converter: state => new PersonalScreenViewModel {
                    isLoggedIn = state.loginState.isLoggedIn,
                    user = state.loginState.loginInfo,
                    userDict = state.userState.userDict,
                    userLicenseDict = state.userState.userLicenseDict,
                    currentTabBarIndex = state.tabBarState.currentTabIndex,
                    hasUnreadNotifications = state.loginState.newNotifications != null
                },
                builder: (context1, viewModel, dispatcher) => {
                    return new PersonalScreen(
                        viewModel: viewModel,
                        new PersonalScreenActionModel {
                            updateNotifications = () => {}
                        }
                    );
                }
            );
        }
    }

    public class PersonalScreen : StatefulWidget {
        public PersonalScreen(
            PersonalScreenViewModel viewModel = null,
            PersonalScreenActionModel actionModel = null,
            Key key = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly PersonalScreenViewModel viewModel;
        public readonly PersonalScreenActionModel actionModel;

        public override State createState() {
            return new _PersonalScreenState();
        }
    }

    public class _PersonalScreenState : State<PersonalScreen>, RouteAware {
        string _loginSubId;
        string _logoutSubId;

        public override void initState() {
            base.initState();
            StatusBarManager.statusBarStyle(UserInfoManager.isLoggedIn());
            this._loginSubId = EventBus.subscribe(sName: EventBusConstant.login_success,
                _ => { StatusBarManager.statusBarStyle(true); });
            this._logoutSubId = EventBus.subscribe(sName: EventBusConstant.logout_success,
                _ => { StatusBarManager.statusBarStyle(false); });
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            Main.ConnectApp.routeObserver.subscribe(this, (PageRoute) ModalRoute.of(context: this.context));
        }

        public override void dispose() {
            Main.ConnectApp.routeObserver.unsubscribe(this);
            EventBus.unSubscribe(sName: EventBusConstant.login_success, id: this._loginSubId);
            EventBus.unSubscribe(sName: EventBusConstant.logout_success, id: this._logoutSubId);
            base.dispose();
        }

        public override Widget build(BuildContext context) {
            return new Container(
                color: CColors.Background,
                child: new Column(
                    children: new List<Widget> {
                        this.widget.viewModel.isLoggedIn
                            ? this._buildLoginInNavigationBar()
                            : this._buildNotLoginInNavigationBar(),
                        this._buildMidView(),
                        new Container(height: 16),
                        this._buildBottomView()
                    }
                )
            );
        }

        Widget _buildBottomView() {
            return new Container(
                child: new Column(
                    children: new List<Widget> {
                        new CustomListTile(
                            new Icon(icon: CIcons.outline_settings, size: 24, color: CColors.TextBody2),
                            "设置",
                            trailing: CustomListTileConstant.defaultTrailing,
                            onTap: () => Navigator.pushNamed(context: this.context, routeName: NavigatorRoutes.Setting)
                        ),
                        new CustomListTile(
                            new Icon(icon: CIcons.outline_mail, size: 24, color: CColors.TextBody2),
                            "意见反馈",
                            trailing: CustomListTileConstant.defaultTrailing,
                            onTap: () => {
                                Navigator.pushNamed(context: this.context, routeName: NavigatorRoutes.Feedback);
                            }),
                        new CustomListTile(
                            new Icon(icon: CIcons.outline_sentiment_smile, size: 24, color: CColors.TextBody2),
                            "关于我们",
                            trailing: CustomListTileConstant.defaultTrailing,
                            onTap: () => {
                                AnalyticsManager.ClickEnterAboutUs();
                                Navigator.pushNamed(context: this.context, routeName: NavigatorRoutes.AboutUs);
                            })
                    }
                )
            );
        }

        Widget _buildMidView() {
            return new Container(
                color: CColors.White,
                child: new Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: new List<Widget> {
                        new Expanded(
                            child: new CustomButton(
                                onPressed: () => {
                                    var routeName = this.widget.viewModel.isLoggedIn
                                        ? NavigatorRoutes.MyFavorite
                                        : NavigatorRoutes.Login;
                                    Navigator.pushNamed(context: this.context, routeName: routeName);
                                },
                                padding: EdgeInsets.symmetric(16),
                                child: new Column(
                                    mainAxisAlignment: MainAxisAlignment.center,
                                    children: new List<Widget> {
                                        Image.file(
                                            "image/mine-collection.png",
                                            width: 32,
                                            height: 32
                                        ),
                                        new Container(height: 4),
                                        new Text(
                                            "我的收藏",
                                            style: CTextStyle.PRegularTitle.defaultHeight()
                                        )
                                    }
                                )
                            )
                        ),
                        new Expanded(
                            child: new CustomButton(
                                onPressed: () =>
                                    Navigator.pushNamed(context: this.context, routeName: NavigatorRoutes.History),
                                padding: EdgeInsets.symmetric(16),
                                child: new Column(
                                    mainAxisAlignment: MainAxisAlignment.center,
                                    children: new List<Widget> {
                                        Image.file(
                                            "image/mine-history.png",
                                            width: 32,
                                            height: 32
                                        ),
                                        new Container(height: 4),
                                        new Text(
                                            "浏览历史",
                                            style: CTextStyle.PRegularTitle.defaultHeight()
                                        )
                                    }
                                )
                            )
                        )
                    }
                )
            );
        }

        Widget _buildNotLoginInNavigationBar() {
            return new Container(
                color: CColors.White,
                padding: EdgeInsets.only(top: CCommonUtils.getSafeAreaTopPadding(context: this.context)),
                margin: EdgeInsets.only(bottom: 16),
                child: new Column(
                    mainAxisAlignment: MainAxisAlignment.end,
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: new List<Widget> {
                        this._buildQrScanWidget(false),
                        new Container(
                            padding: EdgeInsets.only(16, 8, 16, 24),
                            child: new Row(
                                children: new List<Widget> {
                                    new Column(
                                        crossAxisAlignment: CrossAxisAlignment.start,
                                        children: new List<Widget> {
                                            new Text("欢迎来到", style: CTextStyle.H4.defaultHeight()),
                                            new Text("Unity Connect", style: CTextStyle.H2.defaultHeight()),
                                            new Container(
                                                margin: EdgeInsets.only(top: 24),
                                                child: new CustomButton(
                                                    padding: EdgeInsets.zero,
                                                    onPressed: () => {
                                                        Navigator.pushNamed(context: this.context,
                                                            routeName: NavigatorRoutes.Login);
                                                    },
                                                    child: new Container(
                                                        height: 40,
                                                        width: 120,
                                                        alignment: Alignment.center,
                                                        decoration: new BoxDecoration(
                                                            color: CColors.PrimaryBlue,
                                                            borderRadius: BorderRadius.all(20)
                                                        ),
                                                        child: new Text(
                                                            "登录/注册",
                                                            style: CTextStyle.PLargeMediumWhite.defaultHeight()
                                                        )
                                                    )
                                                )
                                            )
                                        }
                                    ),
                                    new Expanded(
                                        child: Image.file(
                                            "image/mine_mascot_u.png"
                                        )
                                    )
                                }
                            )
                        )
                    }
                )
            );
        }

        Widget _buildLoginInNavigationBar() {
            var user = this.widget.viewModel.userDict[key: this.widget.viewModel.user.userId];
            Widget titleWidget;
            if (user.title != null && user.title.isNotEmpty()) {
                titleWidget = new Container(
                    padding: EdgeInsets.only(top: 4),
                    child: new Text(
                        data: user.title,
                        style: new TextStyle(
                            fontSize: 14,
                            fontFamily: "Roboto-Regular",
                            color: CColors.Grey80
                        ),
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis
                    )
                );
            }
            else {
                titleWidget = new Container();
            }

            return new Stack(
                children: new List<Widget> {
                    new Container(
                        color: CColors.BgGrey,
                        padding: EdgeInsets.only(bottom: 56),
                        child: new GestureDetector(
                            onTap: () => {
                                Navigator.pushNamed(context: this.context, routeName: NavigatorRoutes.UserDetail,
                                    new UserDetailScreenArguments {
                                        id = user.id
                                    }
                                );
                            },
                            child: new Container(
                                padding: EdgeInsets.only(
                                    top: CCommonUtils.getSafeAreaTopPadding(context: this.context)),
                                decoration: new BoxDecoration(
                                    color: CColors.Black,
                                    new DecorationImage(
                                        new FileImage("image/default-background-cover.png"),
                                        fit: BoxFit.cover
                                    )
                                ),
                                child: new Column(
                                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                    children: new List<Widget> {
                                        this._buildQrScanWidget(true),
                                        new Container(
                                            padding: EdgeInsets.only(16, 16, 16, 64),
                                            child: new Row(
                                                children: new List<Widget> {
                                                    new Container(
                                                        margin: EdgeInsets.only(right: 12),
                                                        child: Avatar.User(
                                                            user: user,
                                                            64,
                                                            true
                                                        )
                                                    ),
                                                    new Expanded(
                                                        child: new Column(
                                                            crossAxisAlignment: CrossAxisAlignment.start,
                                                            children: new List<Widget> {
                                                                new Row(
                                                                    crossAxisAlignment: CrossAxisAlignment.start,
                                                                    children: new List<Widget> {
                                                                        new Flexible(
                                                                            child: new Text(
                                                                                user.fullName ?? user.name,
                                                                                style: CTextStyle.H4White
                                                                                    .defaultHeight(),
                                                                                maxLines: 1,
                                                                                overflow: TextOverflow.ellipsis
                                                                            )
                                                                        ),
                                                                        CImageUtils.GenBadgeImage(
                                                                            badges: user.badges,
                                                                            CCommonUtils.GetUserLicense(
                                                                                userId: user.id,
                                                                                userLicenseMap: this.widget.viewModel
                                                                                    .userLicenseDict
                                                                            ),
                                                                            EdgeInsets.only(4, 6)
                                                                        )
                                                                    }
                                                                ),
                                                                titleWidget
                                                            }
                                                        )
                                                    ),
                                                    new Padding(
                                                        padding: EdgeInsets.symmetric(horizontal: 4),
                                                        child: new Text(
                                                            "个人主页",
                                                            style: new TextStyle(
                                                                fontSize: 14,
                                                                fontFamily: "Roboto-Regular",
                                                                color: CColors.Grey80
                                                            )
                                                        )
                                                    ),
                                                    new Icon(
                                                        icon: CIcons.baseline_forward_arrow,
                                                        size: 16,
                                                        color: CColors.LightBlueGrey
                                                    )
                                                }
                                            )
                                        )
                                    }
                                )
                            )
                        )
                    ),
                    new Positioned(
                        left: 0,
                        right: 0,
                        bottom: 0,
                        child: new Container(
                            margin: EdgeInsets.only(16, 0, 16, 16),
                            height: 80,
                            decoration: new BoxDecoration(
                                color: CColors.White,
                                borderRadius: BorderRadius.all(8),
                                boxShadow: new List<BoxShadow> {
                                    new BoxShadow(
                                        CColors.Black.withOpacity(0.2f),
                                        blurRadius: 8
                                    )
                                }
                            ),
                            child: new Row(
                                children: new List<Widget> {
                                    new Expanded(
                                        child: new CustomButton(
                                            onPressed: () => {
                                                if (this.widget.viewModel.isLoggedIn && user.id.isNotEmpty()) {
                                                    Navigator.pushNamed(
                                                        context: this.context,
                                                        routeName: NavigatorRoutes.UserFollowing,
                                                        new UserFollowingScreenArguments {id = user.id}
                                                    );
                                                }
                                            },
                                            child: new Column(
                                                mainAxisAlignment: MainAxisAlignment.center,
                                                children: new List<Widget> {
                                                    new Text(
                                                        CStringUtils.CountToString(
                                                            (user.followingUsersCount ?? 0) +
                                                            (user.followingTeamsCount ?? 0), "0"),
                                                        style: CTextStyle.Bold20
                                                    ),
                                                    new Container(height: 4),
                                                    new Text(
                                                        "关注",
                                                        style: CTextStyle.PSmallBody3.defaultHeight()
                                                    )
                                                }
                                            )
                                        )
                                    ),
                                    new Container(height: 32, width: 1, color: CColors.Separator.withOpacity(0.5f)),
                                    new Expanded(
                                        child: new CustomButton(
                                            onPressed: () => {
                                                if (this.widget.viewModel.isLoggedIn && user.id.isNotEmpty()) {
                                                    Navigator.pushNamed(
                                                        context: this.context,
                                                        routeName: NavigatorRoutes.UserFollower,
                                                        new ScreenArguments {id = user.id}
                                                    );
                                                }
                                            },
                                            child: new Column(
                                                mainAxisAlignment: MainAxisAlignment.center,
                                                children: new List<Widget> {
                                                    new Text(
                                                        CStringUtils.CountToString(user.followCount ?? 0, "0"),
                                                        style: CTextStyle.Bold20
                                                    ),
                                                    new Container(height: 4),
                                                    new Text(
                                                        "粉丝",
                                                        style: CTextStyle.PSmallBody3.defaultHeight()
                                                    )
                                                }
                                            )
                                        )
                                    ),
                                    new Container(height: 32, width: 1, color: CColors.Separator.withOpacity(0.5f)),
                                    new Expanded(
                                        child: new CustomButton(
                                            onPressed: () => { },
                                            child: new Column(
                                                mainAxisAlignment: MainAxisAlignment.center,
                                                children: new List<Widget> {
                                                    new Text(
                                                        CStringUtils.CountToString(user.appArticleLikedCount ?? 0, "0"),
                                                        style: CTextStyle.Bold20
                                                    ),
                                                    new Container(height: 4),
                                                    new Text(
                                                        "赞",
                                                        style: CTextStyle.PSmallBody3.defaultHeight()
                                                    )
                                                }
                                            )
                                        )
                                    ),
                                    new Container(height: 32, width: 1, color: CColors.Separator.withOpacity(0.5f)),
                                    new Expanded(
                                        child: new CustomButton(
                                            onPressed: () => Navigator.pushNamed(
                                                context: this.context,
                                                routeName: NavigatorRoutes.WritingCenter,
                                                new ScreenArguments {id = user.id ?? ""}
                                            ),
                                            child: new Column(
                                                mainAxisAlignment: MainAxisAlignment.center,
                                                children: new List<Widget> {
                                                    new Text(
                                                        CStringUtils.CountToString(user.writingCenterCount ?? 0, "0"),
                                                        style: CTextStyle.Bold20
                                                    ),
                                                    new Container(height: 4),
                                                    new Text(
                                                        "创作中心",
                                                        style: CTextStyle.PSmallBody3.defaultHeight()
                                                    )
                                                }
                                            )
                                        )
                                    )
                                }
                            )
                        )
                    )
                }
            );
        }

        Widget _buildQrScanWidget(bool isLoggedIn) {
            return new Container(
                height: 44,
                child: new Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: new List<Widget> {
                        isLoggedIn
                            ? (Widget) new CustomButton(
                                padding: EdgeInsets.symmetric(8, 16),
                                onPressed: () => {
                                    Navigator.pushNamed(context: this.context, routeName: NavigatorRoutes.Notification)
                                        .then(
                                            _ => { this.widget.actionModel.updateNotifications(); }
                                        );
                                },
                                child: new Container(
                                    width: 28,
                                    height: 28,
                                    child: new Stack(
                                        children: new List<Widget> {
                                            new Icon(
                                                icon: CIcons.outline_notifications,
                                                color: CColors.LightBlueGrey,
                                                size: 28
                                            ),
                                            Positioned.fill(
                                                new Align(
                                                    alignment: Alignment.topRight,
                                                    child: new NotificationDot(
                                                        this.widget.viewModel.hasUnreadNotifications ? "" : null,
                                                        new BorderSide(color: CColors.White, 2)
                                                    )
                                                )
                                            )
                                        }
                                    )
                                )
                            )
                            : new Container(),
                        new CustomButton(
                            padding: EdgeInsets.symmetric(8, 16),
                            onPressed: () => QRScanPlugin.PushToQRScan(context: this.context),
                            child: new Icon(
                                icon: CIcons.outline_scan,
                                size: 28,
                                color: CColors.LightBlueGrey
                            )
                        )
                    }
                )
            );
        }

        public void didPopNext() {
            if (this.widget.viewModel.currentTabBarIndex == 3) {
                StatusBarManager.statusBarStyle(UserInfoManager.isLoggedIn());
            }
        }

        public void didPush() {
        }

        public void didPop() {
        }

        public void didPushNext() {
        }
    }
}