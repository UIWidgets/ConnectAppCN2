using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Components;
using ConnectApp.Main;
using ConnectApp.Models.ActionModel;
using ConnectApp.Models.Model;
using ConnectApp.Models.State;
using ConnectApp.Models.ViewModel;
using ConnectApp.redux.actions;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Image = Unity.UIWidgets.widgets.Image;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace ConnectApp.screens {
    public class ArticlesScreenConnector : StatelessWidget {
        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, ArticlesScreenViewModel>(
                converter: state => new ArticlesScreenViewModel {
                    isLoggedIn = state.loginState.isLoggedIn,
                    feedHasNew = state.articleState.feedHasNew,
                    searchSuggest = state.articleState.searchSuggest,
                    currentTabBarIndex = state.tabBarState.currentTabIndex,
                    currentHomePageTabIndex = state.tabBarState.currentHomePageTabIndex,
                    swiperOnScreen = state.articleState.swiperOnScreen
                },
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new ArticlesScreenActionModel {
                        switchHomePageTabBarIndex = index =>
                            dispatcher.dispatch(new SwitchHomePageTabBarIndexAction {index = index}),
                        changeSwiperStatus = isShowing =>
                            dispatcher.dispatch(new ChangeSwiperStatusAction {isShowing = isShowing})
                    };
                    return new ArticlesScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }

    public class ArticlesScreen : StatefulWidget {
        public ArticlesScreen(
            ArticlesScreenViewModel viewModel = null,
            ArticlesScreenActionModel actionModel = null,
            Key key = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly ArticlesScreenViewModel viewModel;
        public readonly ArticlesScreenActionModel actionModel;

        public override State createState() {
            return new _ArticlesScreenState();
        }
    }

    public class _ArticlesScreenState : AutomaticKeepAliveClientMixin<ArticlesScreen>, RouteAware, TickerProvider {
        CustomTabController _tabController;
        float _navBarHeight;
        string _logoutSubId;
        bool _isRefresh;
        float _recommendArticlePixels;
        float _followArticlePixels;
        int _swiperHeight;

        protected override bool wantKeepAlive {
            get { return true; }
        }

        public override void initState() {
            base.initState();
            StatusBarManager.statusBarStyle(false);
            var currentHomePageTabIndex = this.widget.viewModel.currentHomePageTabIndex;
            this._tabController =
                new CustomTabController(length: tabsList.Count, this, initialIndex: currentHomePageTabIndex);
            this._navBarHeight = CustomAppBarUtil.appBarHeight;
            this._isRefresh = false;
            this._recommendArticlePixels = 0;
            this._followArticlePixels = 0;
            this._logoutSubId = EventBus.subscribe(sName: EventBusConstant.logout_success, args => {
                var currentTabIndex = this.widget.viewModel.currentHomePageTabIndex;
                if (currentTabIndex != 1) {
                    this.widget.actionModel.switchHomePageTabBarIndex(1);
                    this._tabController.animateTo(1);
                }
            });
            VersionManager.checkForUpdates(context: this.context, type: CheckVersionType.initialize);
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            Main.ConnectApp.routeObserver.subscribe(this, (PageRoute) ModalRoute.of(context: this.context));
        }

        public override void dispose() {
            EventBus.unSubscribe(sName: EventBusConstant.logout_success, id: this._logoutSubId);
            Main.ConnectApp.routeObserver.unsubscribe(this);
            base.dispose();
        }

        public Ticker createTicker(TickerCallback onTick) {
            return new Ticker(onTick: onTick, $"created by {this}");
        }

        bool _onNotification(ScrollNotification notification) {
            var axisDirection = notification.metrics.axisDirection;
            if (axisDirection == AxisDirection.left || axisDirection == AxisDirection.right) {
                return true;
            }

            var pixels = notification.metrics.pixels;
            var currentHomePageTabIndex = this.widget.viewModel.currentHomePageTabIndex;
            if (currentHomePageTabIndex == 0) {
                this._followArticlePixels = pixels;
            }
            else if (currentHomePageTabIndex == 1) {
                this._recommendArticlePixels = pixels;
                if (pixels > this._swiperHeight) {
                    if (this.widget.viewModel.swiperOnScreen) {
                        this.widget.actionModel.changeSwiperStatus(false);
                    }
                }
                else {
                    if (!this.widget.viewModel.swiperOnScreen) {
                        this.widget.actionModel.changeSwiperStatus(true);
                    }
                }
            }

            this._changeTabBarItemStatus(pixels: pixels, status: TabBarItemStatus.toHome);
            return true;
        }

        void _changeTabBarItemStatus(float pixels, TabBarItemStatus status) {
            if (pixels > MediaQuery.of(context: this.context).size.height) {
                if (!this._isRefresh) {
                    this._isRefresh = true;
                    EventBus.publish(sName: EventBusConstant.article_refresh,
                        new List<object> {TabBarItemStatus.toRefresh});
                }
            }
            else {
                if (this._isRefresh) {
                    this._isRefresh = false;
                    EventBus.publish(sName: EventBusConstant.article_refresh, new List<object> {status});
                }
            }
        }

        public override Widget build(BuildContext context) {
            if (this._swiperHeight == 0) {
                this._swiperHeight = (int) CCommonUtils.getScreenWidth(buildContext: context) / 3 + 8;
            }

            return new Container(
                color: CColors.White,
                child: new CustomSafeArea(
                    bottom: false,
                    child: new NotificationListener<ScrollNotification>(
                        onNotification: this._onNotification,
                        child: new Column(
                            children: new List<Widget> {
                                this._buildNavigationBar(),
                                this._buildContent()
                            }
                        )
                    )
                )
            );
        }

        Widget _buildNavigationBar() {
            return new Container(
                color: CColors.White,
                height: this._navBarHeight,
                child: new Row(
                    children: new List<Widget> {
                        new SizedBox(width: 16),
                        new Expanded(
                            child: new GestureDetector(
                                onTap: () => {
                                    Navigator.pushNamed(context: this.context, routeName: NavigatorRoutes.Search, new SearchScreenArguments {isModal = true});
                                    AnalyticsManager.ClickEnterSearch("Home_Article");
                                },
                                child: new Container(
                                    height: 32,
                                    decoration: new BoxDecoration(
                                        color: CColors.EmojiBottomBar,
                                        borderRadius: BorderRadius.all(16)
                                    ),
                                    child: new Row(
                                        children: new List<Widget> {
                                            new Padding(
                                                padding: EdgeInsets.only(16, right: 8),
                                                child: new Icon(
                                                    icon: CIcons.outline_search,
                                                    size: 16,
                                                    color: CColors.Icon
                                                )
                                            ),
                                            new Text(
                                                this.widget.viewModel.searchSuggest.isNotEmpty()
                                                    ? this.widget.viewModel.searchSuggest
                                                    : "搜索",
                                                style: new TextStyle(
                                                    fontSize: 14,
                                                    fontFamily: "Roboto-Regular",
                                                    color: CColors.TextBody5
                                                )
                                            )
                                        }
                                    )
                                )
                            )
                        ),
                        new SizedBox(width: 16)
                    }
                )
            );
        }

        static readonly List<string> tabsList = new List<string> {
            "关注",
            "推荐"
        };

        Widget _buildContent() {
            ScrollPhysics physics;
            if (this.widget.viewModel.isLoggedIn) {
                physics = new BouncingScrollPhysics();
            }
            else {
                physics = new NeverScrollableScrollPhysics();
            }

            var tabs = new List<object>();
            for (var i = 0; i < tabsList.Count; i++) {
                tabs.Add(this._buildSelectItem(tabsList[index: i], index: i));
            }

            var tabViews = new List<Widget>();
            for (var i = 0; i < tabsList.Count; i++) {
                var tab = tabsList[index: i];
                if (tab.Equals("关注")) {
                    tabViews.Add(new FollowArticleScreenConnector());
                }
                else if (tab.Equals("推荐")) {
                    tabViews.Add(new RecommendArticleScreenConnector());
                }
            }

            return new Expanded(
                child: new CustomSegmentedControl(
                    items: tabs,
                    children: tabViews,
                    newValue => {
                        this.widget.actionModel.switchHomePageTabBarIndex(obj: newValue);
                        if (newValue == 0) {
                            AnalyticsManager.AnalyticsClickHomeFocus();
                            this._changeTabBarItemStatus(pixels: this._followArticlePixels,
                                status: TabBarItemStatus.normal);
                        }

                        if (newValue == 1) {
                            this._changeTabBarItemStatus(pixels: this._recommendArticlePixels,
                                status: TabBarItemStatus.normal);
                        }
                    },
                    1,
                    trailing: this._buildTrailing(),
                    headerDecoration: new BoxDecoration(
                        color: CColors.White,
                        border: new Border(bottom: new BorderSide(this._navBarHeight <= 0
                            ? CColors.Separator2
                            : CColors.Transparent))
                    ),
                    indicator: new CustomGradientsTabIndicator(
                        insets: EdgeInsets.symmetric(horizontal: 8),
                        height: 8,
                        gradient: new LinearGradient(
                            begin: Alignment.centerLeft,
                            end: Alignment.centerRight,
                            new List<Color> {
                                new Color(0xFFB1E0FF),
                                new Color(0xFF6EC6FF)
                            }
                        )
                    ),
                    headerPadding: EdgeInsets.only(8, bottom: 8),
                    labelPadding: EdgeInsets.zero,
                    selectedColor: CColors.TextTitle,
                    unselectedColor: CColors.TextBody4,
                    unselectedTextStyle: new TextStyle(
                        fontSize: 18,
                        fontFamily: "Roboto-Medium"
                    ),
                    selectedTextStyle: new TextStyle(
                        fontSize: 18,
                        fontFamily: "Roboto-Medium"
                    ),
                    controller: this._tabController,
                    physics: physics,
                    onTap: index => {
                        if (this.widget.viewModel.currentHomePageTabIndex != index) {
                            if (index == 0) {
                                if (!this.widget.viewModel.isLoggedIn) {
                                    Navigator.pushNamed(
                                        context: this.context,
                                        routeName: NavigatorRoutes.Login
                                    );
                                    return;
                                }
                            }
                            this.widget.actionModel.switchHomePageTabBarIndex(obj: index);
                            this._tabController.animateTo(value: index);
                        }
                    }
                )
            );
        }

        Widget _buildSelectItem(string title, int index) {
            Widget redDot;
            if (index == 0 && this.widget.viewModel.isLoggedIn && this.widget.viewModel.feedHasNew) {
                redDot = new Positioned(
                    top: 0,
                    right: 0,
                    child: new Container(
                        width: 8,
                        height: 8,
                        decoration: new BoxDecoration(
                            color: CColors.Error,
                            borderRadius: BorderRadius.circular(4)
                        )
                    )
                );
            }
            else {
                redDot = new Positioned(
                    top: 0,
                    right: 0,
                    child: new Container(
                        width: 8,
                        height: 8
                    )
                );
            }

            return new Container(
                alignment: Alignment.bottomCenter,
                child: new Stack(
                    children: new List<Widget> {
                        new Container(
                            padding: EdgeInsets.only(8, 4, 8),
                            child: new Text(
                                data: title
                            )
                        ),
                        redDot
                    }
                )
            );
        }

        Widget _buildTrailing() {
            return new Opacity(
                opacity: (CustomAppBarUtil.appBarHeight - this._navBarHeight) / CustomAppBarUtil.appBarHeight,
                child: new Row(
                    children: new List<Widget> {
                        new CustomButton(
                            padding: EdgeInsets.only(16, 8, 8, 8),
                            onPressed: () => {
                                Navigator.pushNamed(context: this.context, routeName: NavigatorRoutes.Search, new SearchScreenArguments {isModal = true});
                                AnalyticsManager.ClickEnterSearch("Home_Article");
                            },
                            child: new Icon(
                                icon: CIcons.outline_search,
                                size: 28,
                                color: CColors.Icon
                            )
                        )
                    }
                )
            );
        }

        public void didPopNext() {
            if (this.widget.viewModel.currentTabBarIndex == 0) {
                StatusBarManager.statusBarStyle(false);
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