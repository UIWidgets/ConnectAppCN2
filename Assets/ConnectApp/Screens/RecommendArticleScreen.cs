using System;
using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Components;
using ConnectApp.Components.PullToRefresh;
using ConnectApp.Components.Swiper;
using ConnectApp.Main;
using ConnectApp.Models.ActionModel;
using ConnectApp.Models.Model;
using ConnectApp.Models.State;
using ConnectApp.Models.ViewModel;
using ConnectApp.Plugins;
using ConnectApp.redux.actions;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.service;
using Unity.UIWidgets.widgets;

namespace ConnectApp.screens {
    public class RecommendArticleScreenConnector : StatelessWidget {
        public RecommendArticleScreenConnector(
            Key key = null
        ) : base(key: key) {
        }

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, ArticlesScreenViewModel>(
                converter: state => new ArticlesScreenViewModel {
                    articlesLoading = state.articleState.articlesLoading,
                    recommendArticleIds = state.articleState.recommendArticleIds,
                    randomRecommendArticleIds = state.articleState.randomRecommendArticleIds,
                    articleDict = state.articleState.articleDict,
                    blockArticleList = state.articleState.blockArticleList,
                    hottestHasMore = state.articleState.hottestHasMore,
                    userDict = state.userState.userDict,
                    teamDict = state.teamState.teamDict,
                    followMap = state.followState.followDict.ContainsKey(state.loginState.loginInfo.userId ?? "")
                        ? state.followState.followDict[state.loginState.loginInfo.userId ?? ""]
                        : new Dictionary<string, bool>(),
                    favoriteTagDict = state.favoriteState.favoriteTagDict,
                    favoriteTagArticleDict = state.favoriteState.favoriteTagArticleDict,
                    rankDict = state.leaderBoardState.rankDict,
                    homeSliderIds = state.articleState.homeSliderIds,
                    homeCollectionIds = state.articleState.homeCollectionIds,
                    homeBloggerIds = state.articleState.homeBloggerIds,
                    isLoggedIn = state.loginState.isLoggedIn,
                    hottestOffset = state.articleState.recommendArticleIds.Count,
                    currentUserId = state.loginState.loginInfo.userId ?? "",
                    currentHomePageTabIndex = state.tabBarState.currentHomePageTabIndex,
                    currentTabBarIndex = state.tabBarState.currentTabIndex,
                    swiperOnScreen = state.articleState.swiperOnScreen
                },
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new ArticlesScreenActionModel {
                        openUrl = url => {
                            CommonPlugin.init(buildContext: context1);
                            CommonPlugin.openUrlScheme(schemeUrl: url);
                        },
                        blockArticleAction = articleId => {
                            dispatcher.dispatch(new BlockArticleAction {articleId = articleId});
                            dispatcher.dispatch(new DeleteArticleHistoryAction {articleId = articleId});
                        },
                        startFetchArticles = () => dispatcher.dispatch(new StartFetchArticlesAction()),
                        fetchArticles = (userId, offset, random) =>
                            dispatcher.dispatch<Future>(CActions.fetchArticles(userId: userId, offset: offset, random: random)),
                        startFollowUser = userId =>
                            dispatcher.dispatch(new StartFollowUserAction {followUserId = userId}),
                        followUser = userId =>
                            dispatcher.dispatch<Future>(CActions.fetchFollowUser(followUserId: userId)),
                        startUnFollowUser = userId =>
                            dispatcher.dispatch(new StartUnFollowUserAction {unFollowUserId = userId}),
                        unFollowUser = userId =>
                            dispatcher.dispatch<Future>(CActions.fetchUnFollowUser(unFollowUserId: userId)),
                        shareToWechat = (type, title, description, linkUrl, imageUrl, path) => dispatcher.dispatch<Future>(
                            CActions.shareToWechat(sheetItemType: type, title: title, description: description,
                                linkUrl: linkUrl, imageUrl: imageUrl))
                    };
                    return new RecommendArticleScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }

    public class RecommendArticleScreen : StatefulWidget {
        public RecommendArticleScreen(
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
            return new _RecommendArticleScreenState();
        }
    }

    class _RecommendArticleScreenState : AutomaticKeepAliveClientMixin<RecommendArticleScreen>, RouteAware {
        const int initOffset = 0;
        int offset = initOffset;
        RefreshController _refreshController;
        bool _hasBeenLoadedData;
        string _articleTabSubId;
        bool _pageAppearing;
        bool _pageOnTopLayer;

        public override void initState() {
            base.initState();
            this._refreshController = new RefreshController();
            this._hasBeenLoadedData = false;
            this._pageAppearing = true;
            this._pageOnTopLayer = true;
            SchedulerBinding.instance.addPostFrameCallback(_ => {
                this.widget.actionModel.startFetchArticles();
                this.widget.actionModel.fetchArticles(arg1: this.widget.viewModel.currentUserId, arg2: initOffset, false)
                    .then(
                        v => {
                            if (this._hasBeenLoadedData) {
                                return;
                            }

                            this._hasBeenLoadedData = true;
                            this.setState(() => { });
                        }
                    );
            });

            this._articleTabSubId = EventBus.subscribe(sName: EventBusConstant.article_tab, args => {
                if (this.widget.viewModel.currentHomePageTabIndex == 1) {
                    this._refreshController.sendBack(true, mode: RefreshStatus.refreshing);
                    this._refreshController.animateTo(0.0f, TimeSpan.FromMilliseconds(300), curve: Curves.linear);
                }
            });
        }

        void didPageAppear() {
            this._pageAppearing = true;
            this.setState(() => { });
        }

        void didPageDisappear() {
            this._pageAppearing = false;
            this.setState(() => { });
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            Main.ConnectApp.routeObserver.subscribe(this, (PageRoute) ModalRoute.of(context: this.context));
        }

        public override void dispose() {
            EventBus.unSubscribe(sName: EventBusConstant.article_tab, id: this._articleTabSubId);
            Main.ConnectApp.routeObserver.unsubscribe(this);
            base.dispose();
        }

        protected override bool wantKeepAlive {
            get { return true; }
        }

        public override Widget build(BuildContext context) {
            base.build(context: context);
            if (this.widget.viewModel.currentTabBarIndex == 0 && this.widget.viewModel.currentHomePageTabIndex == 1 &&
                this._pageOnTopLayer && this.widget.viewModel.swiperOnScreen) {
                this.didPageAppear();
            }
            else {
                this.didPageDisappear();
            }

            return new Container(
                color: CColors.Background,
                child: this._buildArticleList(context: context)
            );
        }

        Widget _buildArticleList(BuildContext context) {
            Widget content;
            var articleIds = new List<string>();
            if (this.widget.viewModel.randomRecommendArticleIds.isEmpty()) {
                articleIds = this.widget.viewModel.recommendArticleIds;
            }
            else {
                articleIds.AddRange(collection: this.widget.viewModel.randomRecommendArticleIds);
                articleIds.AddRange(collection: this.widget.viewModel.recommendArticleIds);
            }
            if (!this._hasBeenLoadedData || this.widget.viewModel.articlesLoading && articleIds.isEmpty()) {
                content = ListView.builder(
                    physics: new NeverScrollableScrollPhysics(),
                    itemCount: 6,
                    itemBuilder: (cxt, index) => new ArticleLoadingCard()
                );
            }
            else if (articleIds.isEmpty()) {
                content = new Container(
                    padding: EdgeInsets.only(bottom: CSizes.TabBarHeight +
                                                     CCommonUtils.getSafeAreaBottomPadding(context: context)),
                    child: new BlankView(
                        "哎呀，暂无推荐文章",
                        imageName: BlankImage.common,
                        true,
                        () => {
                            this.widget.actionModel.startFetchArticles();
                            this.widget.actionModel.fetchArticles(arg1: this.widget.viewModel.currentUserId,
                                arg2: initOffset, false);
                        }
                    )
                );
            }
            else {
                var items = this._buildItems(articleIds: articleIds);
                var enablePullUp = this.widget.viewModel.hottestHasMore;
                content = new CustomListView(
                    controller: this._refreshController,
                    enablePullDown: true,
                    enablePullUp: enablePullUp,
                    onRefresh: this._onRefresh,
                    hasBottomMargin: true,
                    itemCount: items.Count,
                    itemBuilder: (cxt, index) => items[index: index],
                    headerWidget: new Column(
                        children: new List<Widget> {
                            this._buildSwiper(),
                            new SizedBox(height: 8)
                        }
                    ),
                    footerWidget: enablePullUp ? null : new EndView(hasBottomMargin: true),
                    hasScrollBar: false
                );
            }

            return new CustomScrollbar(child: content);
        }

        Widget _buildSwiper() {
            var homeSliderIds = this.widget.viewModel.homeSliderIds;
            if (homeSliderIds.isNullOrEmpty()) {
                return new Container();
            }

            Widget swiperContent;
            if (homeSliderIds.Count == 1) {
                var homeSliderId = homeSliderIds[0];
                var imageUrl = this.widget.viewModel.rankDict.ContainsKey(key: homeSliderId)
                    ? this.widget.viewModel.rankDict[key: homeSliderId].image
                    : "";
                swiperContent = new GestureDetector(
                    onTap: () => {
                        var sliderName = this.widget.viewModel.rankDict.ContainsKey(key: homeSliderId)
                            ? this.widget.viewModel.rankDict[key: homeSliderId].resetTitle
                            : "";
                        var redirectURL = this.widget.viewModel.rankDict.ContainsKey(key: homeSliderId)
                            ? this.widget.viewModel.rankDict[key: homeSliderId].redirectURL
                            : "";
                        if (redirectURL.isNotEmpty()) {
                            this.widget.actionModel.openUrl(obj: redirectURL);
                        }

                        AnalyticsManager.ClickHomePageBanner(id: homeSliderId, name: sliderName, url: redirectURL);
                    },
                    child: new PlaceholderImage(
                        CImageUtils.SizeToScreenImageUrl(buildContext: this.context, imageUrl: imageUrl),
                        fit: BoxFit.fill,
                        color: CColorUtils.GetSpecificDarkColorFromId(id: homeSliderId)
                    )
                );
            }
            else {
                swiperContent = new Swiper(
                    (cxt, index) => {
                        var homeSliderId = homeSliderIds[index: index];
                        var imageUrl = this.widget.viewModel.rankDict.ContainsKey(key: homeSliderId)
                            ? this.widget.viewModel.rankDict[key: homeSliderId].image
                            : "";
                        return new PlaceholderImage(
                            CImageUtils.SizeToScreenImageUrl(buildContext: this.context, imageUrl: imageUrl),
                            fit: BoxFit.fill,
                            color: CColorUtils.GetSpecificDarkColorFromId(id: homeSliderId)
                        );
                    },
                    itemCount: homeSliderIds.Count,
                    autoplay: true,
                    onTap: index => {
                        var homeSliderId = homeSliderIds[index: index];
                        var sliderName = this.widget.viewModel.rankDict.ContainsKey(key: homeSliderId)
                            ? this.widget.viewModel.rankDict[key: homeSliderId].resetTitle
                            : "";
                        var redirectURL = this.widget.viewModel.rankDict.ContainsKey(key: homeSliderId)
                            ? this.widget.viewModel.rankDict[key: homeSliderId].redirectURL
                            : "";
                        if (redirectURL.isNotEmpty()) {
                            this.widget.actionModel.openUrl(obj: redirectURL);
                        }
                    
                        AnalyticsManager.ClickHomePageBanner(id: homeSliderId, name: sliderName, url: redirectURL);
                    },
                    onIndexChanged: (index, isManualSwipe) => {
                        if (!this._pageAppearing) {
                            return;
                        }

                        var homeSliderId = homeSliderIds[index: index];
                        var sliderName = this.widget.viewModel.rankDict.ContainsKey(key: homeSliderId)
                            ? this.widget.viewModel.rankDict[key: homeSliderId].resetTitle
                            : "";
                        var redirectURL = this.widget.viewModel.rankDict.ContainsKey(key: homeSliderId)
                            ? this.widget.viewModel.rankDict[key: homeSliderId].redirectURL
                            : "";
                        if (isManualSwipe) {
                            AnalyticsManager.ManualShowHomePageBanner(id: homeSliderId, name: sliderName,
                                url: redirectURL);
                        }
                        else {
                            AnalyticsManager.ShowHomePageBanner(id: homeSliderId, name: sliderName, url: redirectURL);
                        }
                    },
                    pagination: new SwiperPagination(margin: EdgeInsets.only(bottom: 5))
                );
            }

            return new Container(
                padding: EdgeInsets.only(top: 8, left: 16, right: 16),
                decoration: new BoxDecoration(
                    borderRadius: BorderRadius.all(8),
                    color: CColors.White
                ),
                child: new AspectRatio(
                    aspectRatio: 3,
                    child: new ClipRRect(
                        borderRadius: BorderRadius.all(8),
                        child: swiperContent
                    )
                )
            );
        }

        List<Widget> _buildItems(List<string> articleIds) {
            var items = new List<Widget>();
            articleIds.ForEach(articleId => { items.Add(this._buildArticleCard(articleId: articleId)); });
            if (items.Count >= 6) {
                items.Insert(6, this._buildRecommendLeaderBoard());
                items.Insert(3, this._buildRecommendBlogger());
            }

            // if (!this.widget.viewModel.hasNewArticle) {
            //     items.Insert(0, this._buildLeaderBoard());
            // }

            return items;
        }


        Widget _buildArticleCard(string articleId) {
            if (this.widget.viewModel.blockArticleList.Contains(item: articleId)) {
                return new Container();
            }

            if (!this.widget.viewModel.articleDict.ContainsKey(key: articleId)) {
                return new Container();
            }

            var article = this.widget.viewModel.articleDict[key: articleId];
            var fullName = "";
            var userId = "";
            if (article.ownerType == OwnerType.user.ToString()) {
                userId = article.userId;
                if (this.widget.viewModel.userDict.ContainsKey(key: article.userId)) {
                    fullName = this.widget.viewModel.userDict[key: article.userId].fullName
                               ?? this.widget.viewModel.userDict[key: article.userId].name;
                }
            }

            if (article.ownerType == OwnerType.team.ToString()) {
                userId = article.teamId;
                if (this.widget.viewModel.teamDict.ContainsKey(key: article.teamId)) {
                    fullName = this.widget.viewModel.teamDict[key: article.teamId].name;
                }
            }

            var linkUrl = CStringUtils.JointProjectShareLink(projectId: article.id);
            return new ArticleCard(
                article: article,
                () => {
                    Navigator.pushNamed(context: this.context, routeName: NavigatorRoutes.ArticleDetail,
                        new ArticleDetailScreenArguments {id = article.id});
                    AnalyticsManager.ClickEnterArticleDetail("Home_Article", articleId: article.id,
                        articleTitle: article.title);
                },
                () => ShareManager.showDoubleDeckShareView(
                    context: this.context,
                    this.widget.viewModel.currentUserId != userId,
                    isLoggedIn: this.widget.viewModel.isLoggedIn,
                    () => {
                        Clipboard.setData(new ClipboardData(text: linkUrl));
                        CustomDialogUtils.showToast(context: this.context, "复制链接成功",
                            iconData: CIcons.check_circle_outline);
                    },
                    () => Navigator.pushNamed(
                        context: this.context,
                        routeName: NavigatorRoutes.Login
                    ),
                    () => this.widget.actionModel.blockArticleAction(obj: article.id),
                    () => Navigator.pushNamed(
                        context: this.context,
                        routeName: NavigatorRoutes.Report,
                        new ReportScreenArguments {
                            id = article.id,
                            reportType = ReportType.article
                        }
                    ),
                    type => {
                        CustomDialogUtils.showCustomDialog(
                            context: this.context,
                            child: new CustomLoadingDialog()
                        );
                        var imageUrl = CImageUtils.SizeTo200ImageUrl(imageUrl: article.thumbnail.url);
                        this.widget.actionModel.shareToWechat(arg1: type, arg2: article.title,
                                arg3: article.subTitle, arg4: linkUrl, arg5: imageUrl, "")
                            .then(_ => CustomDialogUtils.hiddenCustomDialog(context: this.context))
                            .catchError(_ => CustomDialogUtils.hiddenCustomDialog(context: this.context));
                    }
                ),
                fullName: fullName,
                new ObjectKey(value: article.id)
            );
        }

        Widget _buildRecommendBlogger() {
            return new RecommendBlogger(
                bloggerIds: this.widget.viewModel.homeBloggerIds,
                rankDict: this.widget.viewModel.rankDict,
                userDict: this.widget.viewModel.userDict,
                followMap: this.widget.viewModel.followMap,
                isLoggedIn: this.widget.viewModel.isLoggedIn,
                userId => {
                    this.widget.actionModel.startFollowUser(obj: userId);
                    this.widget.actionModel.followUser(arg: userId);
                },
                userId => {
                    this.widget.actionModel.startUnFollowUser(obj: userId);
                    this.widget.actionModel.unFollowUser(arg: userId);
                },
                () => Navigator.pushNamed(
                    context: this.context,
                    routeName: NavigatorRoutes.Login
                ),
                () => Navigator.pushNamed(
                    context: this.context, 
                    routeName: NavigatorRoutes.Blogger
                ),
                userId => {
                    Navigator.pushNamed(context: this.context, routeName: NavigatorRoutes.UserDetail,
                        new UserDetailScreenArguments {
                            id = userId
                        }
                    );
                });
        }

        Widget _buildRecommendLeaderBoard() {
            return new RecommendLeaderBoard(
                data: this.widget.viewModel.homeCollectionIds,
                rankDict: this.widget.viewModel.rankDict,
                favoriteTagDict: this.widget.viewModel.favoriteTagDict,
                favoriteTagArticleDict: this.widget.viewModel.favoriteTagArticleDict,
                () => Navigator.pushNamed(
                    context: this.context, 
                    routeName: NavigatorRoutes.LeaderBoard,
                    new LeaderBoardScreenArguments {
                        initIndex = 0
                    }
                ),
                collectionId => {
                    Navigator.pushNamed(
                        context: this.context, 
                        routeName: NavigatorRoutes.LeaderBoardDetail,
                        new LeaderBoardDetailScreenArguments {
                            id = collectionId,
                            type = LeaderBoardType.collection
                        }
                    );
                });
        }

        void _onRefresh(bool up) {
            this.offset = up ? initOffset : this.widget.viewModel.hottestOffset;
            this.widget.actionModel.fetchArticles(arg1: this.widget.viewModel.currentUserId, arg2: this.offset, arg3: up)
                .then(_ => this._refreshController.sendBack(up: up, up ? RefreshStatus.completed : RefreshStatus.idle))
                .catchError(_ => this._refreshController.sendBack(up: up, mode: RefreshStatus.failed));
        }

        public void didPopNext() {
            this._pageOnTopLayer = true;
            this.setState(() => { });
        }

        public void didPush() {
        }

        public void didPop() {
        }

        public void didPushNext() {
            this._pageOnTopLayer = false;
            this.setState(() => { });
        }
    }
}