using System;
using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Components;
using ConnectApp.Components.PullToRefresh;
using ConnectApp.Main;
using ConnectApp.Models.ActionModel;
using ConnectApp.Models.Model;
using ConnectApp.Models.State;
using ConnectApp.Models.ViewModel;
using ConnectApp.redux.actions;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.service;
using Unity.UIWidgets.widgets;
using Avatar = ConnectApp.Components.Avatar;
using Color = Unity.UIWidgets.ui.Color;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace ConnectApp.screens {
    public class TeamDetailScreenConnector : StatelessWidget {
        public TeamDetailScreenConnector(
            string teamId,
            bool isSlug,
            Key key = null
        ) : base(key: key) {
            this.teamId = teamId;
            this.isSlug = isSlug;
        }

        readonly string teamId;
        readonly bool isSlug;

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, TeamDetailScreenViewModel>(
                converter: state => {
                    var currentUserId = state.loginState.loginInfo.userId ?? "";
                    var team = this.FetchTeam(teamId: this.teamId, teamDict: state.teamState.teamDict,
                        slugDict: state.teamState.slugDict);
                    var followMap = state.followState.followDict.ContainsKey(key: currentUserId)
                        ? state.followState.followDict[key: currentUserId]
                        : new Dictionary<string, bool>();
                    return new TeamDetailScreenViewModel {
                        teamLoading = state.teamState.teamLoading,
                        teamArticleLoading = state.teamState.teamArticleLoading,
                        team = team,
                        teamId = this.teamId,
                        articleDict = state.articleState.articleDict,
                        followMap = followMap,
                        currentUserId = state.loginState.loginInfo.userId ?? "",
                        isLoggedIn = state.loginState.isLoggedIn
                    };
                },
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new TeamDetailScreenActionModel {
                        startFetchTeam = () => dispatcher.dispatch(new StartFetchTeamAction()),
                        fetchTeam = () => dispatcher.dispatch<Future>(CActions.fetchTeam(teamId: this.teamId)),
                        startFetchTeamArticle = () => dispatcher.dispatch(new StartFetchTeamArticleAction()),
                        fetchTeamArticle = pageNumber =>
                            dispatcher.dispatch<Future>(CActions.fetchTeamArticle(teamId: viewModel.team.id,
                                pageNumber: pageNumber)),
                        blockArticleAction = articleId => {
                            dispatcher.dispatch(new BlockArticleAction {articleId = articleId});
                            dispatcher.dispatch(new DeleteArticleHistoryAction {articleId = articleId});
                        },
                        startFollowTeam = () =>
                            dispatcher.dispatch(new StartFetchFollowTeamAction {followTeamId = viewModel.team.id}),
                        followTeam = teamId =>
                            dispatcher.dispatch<Future>(CActions.fetchFollowTeam(followTeamId: teamId)),
                        startUnFollowTeam = () => dispatcher.dispatch(new StartFetchUnFollowTeamAction
                            {unFollowTeamId = viewModel.team.id}),
                        unFollowTeam = teamId =>
                            dispatcher.dispatch<Future>(CActions.fetchUnFollowTeam(unFollowTeamId: teamId)),
                        shareToWechat = (type, title, description, linkUrl, imageUrl, path) => dispatcher.dispatch<Future>(
                            CActions.shareToWechat(sheetItemType: type, title: title, description: description,
                                linkUrl: linkUrl, imageUrl: imageUrl))
                    };
                    return new TeamDetailScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }

        Team FetchTeam(string teamId, Dictionary<string, Team> teamDict, Dictionary<string, string> slugDict) {
            if (teamDict.ContainsKey(key: teamId)) {
                return teamDict[key: teamId];
            }

            if (this.isSlug && slugDict.ContainsKey(key: teamId)) {
                return teamDict[slugDict[key: teamId]];
            }

            return null;
        }
    }

    public class TeamDetailScreen : StatefulWidget {
        public TeamDetailScreen(
            TeamDetailScreenViewModel viewModel = null,
            TeamDetailScreenActionModel actionModel = null,
            Key key = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly TeamDetailScreenViewModel viewModel;
        public readonly TeamDetailScreenActionModel actionModel;

        public override State createState() {
            return new _TeamDetailScreenState();
        }
    }

    class _TeamDetailScreenState : State<TeamDetailScreen>, TickerProvider, RouteAware {
        const float imageBaseHeight = 212;
        const float navBarHeight = 44;
        int _articlePageNumber;
        RefreshController _refreshController;
        bool _isHaveTitle;
        bool _hideNavBar;
        Animation<RelativeRect> _animation;
        AnimationController _controller;

        public override void initState() {
            base.initState();
            StatusBarManager.statusBarStyle(true);
            this._articlePageNumber = 1;
            this._refreshController = new RefreshController();
            this._isHaveTitle = false;
            this._hideNavBar = true;
            this._controller = new AnimationController(
                duration: TimeSpan.FromMilliseconds(100),
                vsync: this
            );
            var rectTween = new RelativeRectTween(
                RelativeRect.fromLTRB(0, top: navBarHeight, 0, 0),
                RelativeRect.fromLTRB(0, 0, 0, 0)
            );
            this._animation = rectTween.animate(parent: this._controller);
            SchedulerBinding.instance.addPostFrameCallback(_ => {
                this.widget.actionModel.startFetchTeam();
                this.widget.actionModel.fetchTeam().then(v => this.setState(() => {}));
                this.widget.actionModel.startFetchTeamArticle();
            });
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            Main.ConnectApp.routeObserver.subscribe(this, (PageRoute) ModalRoute.of(context: this.context));
        }

        public override void dispose() {
            Main.ConnectApp.routeObserver.unsubscribe(this);
            this._controller.dispose();
            base.dispose();
        }

        public Ticker createTicker(TickerCallback onTick) {
            return new Ticker(onTick: onTick, $"created by {this}");
        }

        bool _onNotification(ScrollNotification notification) {
            var pixels = notification.metrics.pixels;
            var navBarBottomPosition = navBarHeight + CCommonUtils.getSafeAreaTopPadding(context: this.context);

            if (pixels >= navBarBottomPosition) {
                if (this._hideNavBar) {
                    this.setState(() => this._hideNavBar = false);
                    StatusBarManager.statusBarStyle(false);
                }
            }
            else {
                if (!this._hideNavBar) {
                    this.setState(() => this._hideNavBar = true);
                    StatusBarManager.statusBarStyle(true);
                }
            }

            if (pixels > imageBaseHeight - navBarHeight - 24) {
                if (!this._isHaveTitle) {
                    this._controller.forward();
                    this.setState(() => this._isHaveTitle = true);
                }
            }
            else {
                if (this._isHaveTitle) {
                    this._controller.reverse();
                    this.setState(() => this._isHaveTitle = false);
                }
            }

            return true;
        }

        void _onRefresh(bool up) {
            if (up) {
                this._articlePageNumber = 1;
            }
            else {
                this._articlePageNumber++;
            }

            this.widget.actionModel.fetchTeamArticle(arg: this._articlePageNumber)
                .then(_ => this._refreshController.sendBack(up: up, up ? RefreshStatus.completed : RefreshStatus.idle))
                .catchError(_ => this._refreshController.sendBack(up: up, mode: RefreshStatus.failed));
        }

        void _share(Article article) {
            ActionSheetUtils.showModalActionSheet(
                context: this.context,
                new ShareView(
                    buildContext: this.context,
                    shareSheetStyle: ShareSheetStyle.doubleDeck,
                    onPressed: type => {
                        var linkUrl = CStringUtils.JointProjectShareLink(projectId: article.id);
                        if (type == ShareSheetItemType.clipBoard) {
                            Clipboard.setData(new ClipboardData(text: linkUrl));
                            CustomDialogUtils.showToast(context: this.context, "复制链接成功",
                                iconData: CIcons.check_circle_outline);
                        }
                        else if (type == ShareSheetItemType.block) {
                            ReportManager.blockObject(
                                context: this.context,
                                isLoggedIn: this.widget.viewModel.isLoggedIn,
                                () => Navigator.pushNamed(context: this.context, routeName: NavigatorRoutes.Login),
                                () => this.widget.actionModel.blockArticleAction(obj: article.id),
                                () => Navigator.pop(context: this.context)
                            );
                        }
                        else if (type == ShareSheetItemType.report) {
                            ReportManager.report(
                                isLoggedIn: this.widget.viewModel.isLoggedIn,
                                () => Navigator.pushNamed(context: this.context, routeName: NavigatorRoutes.Login),
                                () => Navigator.pushNamed(
                                    context: this.context, 
                                    routeName: NavigatorRoutes.Report,
                                    new ReportScreenArguments {
                                        id = article.id,
                                        reportType = ReportType.article
                                    }
                                ));
                        }
                        else {
                            CustomDialogUtils.showCustomDialog(
                                context: this.context,
                                child: new CustomLoadingDialog()
                            );
                            var imageUrl = CImageUtils.SizeTo200ImageUrl(imageUrl: article.thumbnail.url);
                            this.widget.actionModel.shareToWechat(arg1: type, arg2: article.title,
                                    arg3: article.subTitle,
                                    arg4: linkUrl,
                                    arg5: imageUrl, "").then(_ =>
                                    CustomDialogUtils.hiddenCustomDialog(context: this.context))
                                .catchError(_ => CustomDialogUtils.hiddenCustomDialog(context: this.context));
                        }
                    }
                ));
        }

        public override Widget build(BuildContext context) {
            Widget content = new Container();
            if (this.widget.viewModel.teamLoading && this.widget.viewModel.team == null) {
                content = new GlobalLoading();
            }
            else if (this.widget.viewModel.team == null || this.widget.viewModel.team.errorCode == "ResourceNotFound") {
                content = new BlankView("公司不存在");
            }
            else {
                content = this._buildContent(context: context);
            }

            return new Container(
                color: CColors.White,
                child: new Stack(
                    children: new List<Widget> {
                        content,
                        this._buildNavigationBar(context: context)
                    }
                )
            );
        }

        Widget _buildNavigationBar(BuildContext context) {
            Widget titleWidget = new Container();
            if (this._isHaveTitle) {
                var team = this.widget.viewModel.team ?? new Team();
                titleWidget = new Row(
                    children: new List<Widget> {
                        new Expanded(
                            child: new Text(
                                data: team.name,
                                style: CTextStyle.PXLargeMedium,
                                maxLines: 1,
                                overflow: TextOverflow.ellipsis
                            )
                        ),
                        new SizedBox(width: 8),
                        this._buildFollowButton(true),
                        new SizedBox(width: 16)
                    }
                );
            }

            var hasTeam = !(this.widget.viewModel.team == null ||
                            this.widget.viewModel.team.errorCode == "ResourceNotFound");

            return new Positioned(
                left: 0,
                top: 0,
                right: 0,
                height: navBarHeight + CCommonUtils.getSafeAreaTopPadding(context: context),
                child: new Container(
                    padding: EdgeInsets.only(top: CCommonUtils.getSafeAreaTopPadding(context: context)),
                    decoration: new BoxDecoration(
                        this._hideNavBar ? CColors.Transparent : CColors.White,
                        border: new Border(
                            bottom: new BorderSide(this._isHaveTitle ? CColors.Separator2 : CColors.Transparent))
                    ),
                    child: new Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: new List<Widget> {
                            new GestureDetector(
                                onTap: () => Navigator.pop(context: this.context),
                                child: new Container(
                                    padding: EdgeInsets.only(16, 8, 8, 8),
                                    color: CColors.Transparent,
                                    child: new Icon(
                                        icon: CIcons.arrow_back,
                                        size: 24,
                                        color: hasTeam
                                            ? this._hideNavBar ? CColors.White : CColors.Icon
                                            : CColors.Icon
                                    )
                                )
                            ),
                            new Expanded(
                                child: new Stack(
                                    fit: StackFit.expand,
                                    children: new List<Widget> {
                                        new PositionedTransition(
                                            rect: this._animation,
                                            child: titleWidget
                                        )
                                    }
                                )
                            )
                        }
                    )
                )
            );
        }

        Widget _buildContent(BuildContext context) {
            var articleIds = this.widget.viewModel.team.articleIds;
            var articlesHasMore = this.widget.viewModel.team.articlesHasMore ?? false;
            var teamArticleLoading = this.widget.viewModel.teamArticleLoading && articleIds == null;
            int itemCount;
            if (teamArticleLoading) {
                itemCount = 3;
            }
            else {
                if (articleIds == null) {
                    itemCount = 3;
                }
                else {
                    var articleCount = articlesHasMore ? articleIds.Count : articleIds.Count + 1;
                    itemCount = 2 + (articleIds.Count == 0 ? 1 : articleCount);
                }
            }

            var headerHeight = imageBaseHeight + 44 + CCommonUtils.getSafeAreaTopPadding(context: context);

            return new Container(
                color: CColors.Background,
                child: new CustomScrollbar(
                    child: new SmartRefresher(
                        controller: this._refreshController,
                        enablePullDown: false,
                        enablePullUp: articlesHasMore,
                        onRefresh: this._onRefresh,
                        onNotification: this._onNotification,
                        child: ListView.builder(
                            padding: EdgeInsets.zero,
                            physics: new AlwaysScrollableScrollPhysics(),
                            itemCount: itemCount,
                            itemBuilder: (cxt, index) => {
                                if (index == 0) {
                                    return this._buildTeamInfo(context: context);
                                }

                                if (index == 1) {
                                    return _buildTeamArticleTitle();
                                }

                                if (teamArticleLoading && index == 2) {
                                    var height = MediaQuery.of(context: context).size.height - headerHeight;
                                    return new Container(
                                        height: height,
                                        child: new GlobalLoading()
                                    );
                                }

                                if ((articleIds == null || articleIds.Count == 0) && index == 2) {
                                    var height = MediaQuery.of(context: context).size.height - headerHeight;
                                    return new Container(
                                        height: height,
                                        child: new BlankView(
                                            "哎呀，暂无已发布的文章",
                                            imageName: BlankImage.common
                                        )
                                    );
                                }

                                if (index == itemCount - 1 && !articlesHasMore) {
                                    return new EndView();
                                }

                                var articleId = articleIds[index - 2];
                                if (!this.widget.viewModel.articleDict.ContainsKey(key: articleId)) {
                                    return new Container();
                                }

                                var article = this.widget.viewModel.articleDict[key: articleId];
                                return new ArticleCard(
                                    article: article,
                                    () => {
                                        Navigator.pushNamed(
                                            context: context, 
                                            routeName: NavigatorRoutes.ArticleDetail,
                                            new ArticleDetailScreenArguments {
                                                id = article.id
                                            }
                                        );
                                    },
                                    () => this._share(article: article),
                                    fullName: this.widget.viewModel.team.name,
                                    new ObjectKey(value: article.id)
                                );
                            }
                        )
                    )
                )
            );
        }

        Widget _buildTeamInfo(BuildContext context) {
            var team = this.widget.viewModel.team;
            Widget titleWidget;
            if (team.description.isNotEmpty()) {
                titleWidget = new Padding(
                    padding: EdgeInsets.only(top: 8),
                    child: new Text(
                        data: team.description,
                        style: new TextStyle(
                            fontSize: 14,
                            fontFamily: "Roboto-Regular",
                            color: CColors.BgGrey
                        ),
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis
                    )
                );
            }
            else {
                titleWidget = new Container();
            }

            return new CoverImage(
                coverImage: team.coverImage,
                imageBaseHeight + CCommonUtils.getSafeAreaTopPadding(context: context),
                new Container(
                    padding: EdgeInsets.only(16, 0, 16, 24),
                    child: new Column(
                        mainAxisAlignment: MainAxisAlignment.end,
                        children: new List<Widget> {
                            new Row(
                                children: new List<Widget> {
                                    new Container(
                                        margin: EdgeInsets.only(right: 16),
                                        child: Avatar.Team(
                                            team: team,
                                            80,
                                            true
                                        )
                                    ),
                                    new Expanded(
                                        child: new Column(
                                            mainAxisAlignment: MainAxisAlignment.center,
                                            crossAxisAlignment: CrossAxisAlignment.start,
                                            children: new List<Widget> {
                                                new Row(
                                                    crossAxisAlignment: CrossAxisAlignment.start,
                                                    children: new List<Widget> {
                                                        new Flexible(
                                                            child: new Text(
                                                                data: team.name,
                                                                style: CTextStyle.H4White.defaultHeight(),
                                                                maxLines: 1,
                                                                overflow: TextOverflow.ellipsis
                                                            )
                                                        ),
                                                        CImageUtils.GenBadgeImage(
                                                            badges: team.badges,
                                                            CCommonUtils.GetUserLicense(
                                                                userId: team.id
                                                            ),
                                                            EdgeInsets.only(4, 6)
                                                        )
                                                    }
                                                ),
                                                titleWidget
                                            }
                                        )
                                    )
                                }
                            ),
                            new Container(
                                margin: EdgeInsets.only(top: 16),
                                child: new Row(
                                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                    children: new List<Widget> {
                                        new Row(
                                            children: new List<Widget> {
                                                _buildFollowCount(
                                                    "粉丝",
                                                    CStringUtils.CountToString(team.stats?.followCount ?? 0),
                                                    () => {
                                                        if (this.widget.viewModel.isLoggedIn) {
                                                            Navigator.pushNamed(
                                                                context: this.context,
                                                                routeName: NavigatorRoutes.TeamFollower,
                                                                new ScreenArguments {
                                                                    id = team.id
                                                                }
                                                            );
                                                        }
                                                        else {
                                                            Navigator.pushNamed(context: this.context, routeName: NavigatorRoutes.Login);
                                                        }
                                                    }
                                                ),
                                                new SizedBox(width: 16),
                                                _buildFollowCount(
                                                    "成员",
                                                    CStringUtils.CountToString(team.stats?.membersCount ?? 0),
                                                    () =>  Navigator.pushNamed(
                                                        context: this.context,
                                                        routeName: NavigatorRoutes.TeamMember,
                                                        new ScreenArguments {
                                                            id = team.id
                                                        }
                                                    )
                                                )
                                            }
                                        ),
                                        this._buildFollowButton()
                                    }
                                )
                            )
                        }
                    )
                )
            );
        }

        static Widget _buildTeamArticleTitle() {
            return new Container(
                height: 44,
                color: CColors.White,
                alignment: Alignment.centerLeft,
                child: new Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: new List<Widget> {
                        new Expanded(
                            child: new Container(
                                padding: EdgeInsets.only(16),
                                alignment: Alignment.centerLeft,
                                child: new Text("文章", style: CTextStyle.PLargeTitle)
                            )
                        ),
                        new Container(height: 1, color: CColors.Separator2)
                    }
                )
            );
        }

        static Widget _buildFollowCount(string title, string subTitle, GestureTapCallback onTap) {
            return new GestureDetector(
                onTap: onTap,
                child: new Container(
                    height: 32,
                    alignment: Alignment.center,
                    color: CColors.Transparent,
                    child: new Row(
                        children: new List<Widget> {
                            new Text(data: title, style: CTextStyle.PRegularWhite),
                            new SizedBox(width: 2),
                            new Text(
                                data: subTitle,
                                style: new TextStyle(
                                    height: 1.27f,
                                    fontSize: 20,
                                    fontFamily: "Roboto-Bold",
                                    color: CColors.White
                                )
                            )
                        }
                    )
                )
            );
        }

        Widget _buildFollowButton(bool isTop = false) {
            if (this.widget.viewModel.isLoggedIn
                && this.widget.viewModel.currentUserId == this.widget.viewModel.team.id) {
                return new Container();
            }

            var isFollow = false;
            var followText = "关注";
            var followBgColor = CColors.PrimaryBlue;
            GestureTapCallback onTap = () => {
                this.widget.actionModel.startFollowTeam();
                this.widget.actionModel.followTeam(arg: this.widget.viewModel.team.id);
            };
            if (this.widget.viewModel.isLoggedIn
                && this.widget.viewModel.followMap.ContainsKey(key: this.widget.viewModel.team.id)) {
                isFollow = true;
                followText = "已关注";
                followBgColor = CColors.Transparent;
                onTap = () => {
                    ActionSheetUtils.showModalActionSheet(
                        context: this.context,
                        new ActionSheet(
                            title: "确定不再关注？",
                            items: new List<ActionSheetItem> {
                                new ActionSheetItem("确定", type: ActionType.normal, () => {
                                    this.widget.actionModel.startUnFollowTeam();
                                    this.widget.actionModel.unFollowTeam(arg: this.widget.viewModel.team.id);
                                }),
                                new ActionSheetItem("取消", type: ActionType.cancel)
                            }
                        )
                    );
                };
            }

            Widget buttonChild;
            bool isEnable;
            if (this.widget.viewModel.team.followTeamLoading ?? false) {
                buttonChild = new CustomActivityIndicator(
                    loadingColor: isTop ? LoadingColor.black : LoadingColor.white,
                    size: LoadingSize.small
                );
                isEnable = false;
            }
            else {
                buttonChild = new Text(
                    data: followText,
                    style: isTop
                        ? new TextStyle(
                            fontSize: 14,
                            fontFamily: "Roboto-Medium",
                            color: isFollow ? new Color(0xFF959595) : CColors.PrimaryBlue
                        )
                        : CTextStyle.PMediumWhite
                );
                isEnable = true;
            }

            if (isTop) {
                return new CustomButton(
                    padding: EdgeInsets.zero,
                    child: new Container(
                        width: 60,
                        height: 28,
                        alignment: Alignment.center,
                        decoration: new BoxDecoration(
                            color: CColors.Transparent,
                            borderRadius: BorderRadius.circular(14),
                            border: isFollow
                                ? Border.all(color: CColors.Disable2)
                                : Border.all(color: CColors.PrimaryBlue)
                        ),
                        child: buttonChild
                    ),
                    onPressed: () => {
                        if (!isEnable) {
                            return;
                        }

                        if (this.widget.viewModel.isLoggedIn) {
                            onTap();
                        }
                        else {
                            Navigator.pushNamed(context: this.context, routeName: NavigatorRoutes.Login);
                        }
                    }
                );
            }

            return new CustomButton(
                padding: EdgeInsets.zero,
                child: new Container(
                    width: 100,
                    height: 32,
                    alignment: Alignment.center,
                    decoration: new BoxDecoration(
                        color: followBgColor,
                        borderRadius: BorderRadius.all(4),
                        border: isFollow ? Border.all(color: CColors.White) : null
                    ),
                    child: buttonChild
                ),
                onPressed: () => {
                    if (!isEnable) {
                        return;
                    }

                    if (this.widget.viewModel.isLoggedIn) {
                        onTap();
                    }
                    else {
                        Navigator.pushNamed(context: this.context, routeName: NavigatorRoutes.Login);
                    }
                }
            );
        }

        public void didPopNext() {
            if (this.widget.viewModel.teamId.isNotEmpty()) {
                CTemporaryValue.currentPageModelId = this.widget.viewModel.teamId;
            }

            StatusBarManager.statusBarStyle(isLight: this._hideNavBar);
        }

        public void didPush() {
            if (this.widget.viewModel.teamId.isNotEmpty()) {
                CTemporaryValue.currentPageModelId = this.widget.viewModel.teamId;
            }
        }

        public void didPop() {
            if (CTemporaryValue.currentPageModelId.isNotEmpty() &&
                this.widget.viewModel.teamId == CTemporaryValue.currentPageModelId) {
                CTemporaryValue.currentPageModelId = null;
            }
        }

        public void didPushNext() {
            CTemporaryValue.currentPageModelId = null;
        }
    }
}