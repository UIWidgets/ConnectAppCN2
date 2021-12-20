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
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Color = Unity.UIWidgets.ui.Color;
using Image = Unity.UIWidgets.widgets.Image;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace ConnectApp.screens {
    public class QAScreenConnector : StatelessWidget {
        public QAScreenConnector(
            Key key = null
        ) : base(key: key) {
        }

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, QAScreenViewModel>(
                converter: state => {
                    var latestQuestions = new List<Question>();
                    var hotQuestions = new List<Question>();
                    var pendingQuestions = new List<Question>();
                    var questionDict = state.qaState.questionDict;
                    var latestQuestionIds = state.qaState.latestQuestionIds;
                    var hotQuestionIds = state.qaState.hotQuestionIds;
                    var pendingQuestionIds = state.qaState.pendingQuestionIds;
                    var blockQuestionIds = state.qaState.blockQuestionList;
                    if (latestQuestionIds.isNotNullAndEmpty()) {
                        latestQuestionIds.ForEach(id => {
                            if (id.isNotEmpty() && blockQuestionIds.Contains(item: id)) {
                                return;
                            }

                            if (id.isNotEmpty() && questionDict.ContainsKey(key: id)) {
                                latestQuestions.Add(questionDict[key: id]);
                            }
                        });
                    }

                    if (hotQuestionIds.isNotNullAndEmpty()) {
                        hotQuestionIds.ForEach(id => {
                            if (id.isNotEmpty() && blockQuestionIds.Contains(item: id)) {
                                return;
                            }

                            if (id.isNotEmpty() && questionDict.ContainsKey(key: id)) {
                                hotQuestions.Add(questionDict[key: id]);
                            }
                        });
                    }

                    if (pendingQuestionIds.isNotNullAndEmpty()) {
                        pendingQuestionIds.ForEach(id => {
                            if (id.isNotEmpty() && blockQuestionIds.Contains(item: id)) {
                                return;
                            }

                            if (id.isNotEmpty() && questionDict.ContainsKey(key: id)) {
                                pendingQuestions.Add(questionDict[key: id]);
                            }
                        });
                    }

                    return new QAScreenViewModel {
                        latestQuestions = latestQuestions,
                        hotQuestions = hotQuestions,
                        pendingQuestions = pendingQuestions,
                        tagDict = state.tagState.tagDict,
                        latestQuestionListHasMore = state.qaState.latestQuestionListHasMore,
                        hotQuestionListHasMore = state.qaState.hotQuestionListHasMore,
                        pendingQuestionListHasMore = state.qaState.pendingQuestionListHasMore
                    };
                },
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new QAScreenActionModel {
                        fetchQuestions = (tab, type, page) => dispatcher.dispatch<Future>(
                            CActions.fetchQuestions(tab: tab, type: type, page: page)
                        ),
                        blockQuestionAction = questionId => {
                            dispatcher.dispatch(new BlockQuestionAction {questionId = questionId});
                        },
                        copyText = text => dispatcher.dispatch(new UtilsAction.CopyTextAction {text = text}),
                        shareToWechat = (type, title, description, linkUrl, imageUrl, path) =>
                            dispatcher.dispatch<Future>(
                                CActions.shareToWechat(
                                    sheetItemType: type,
                                    title: title,
                                    description: description,
                                    linkUrl: linkUrl
                                )
                            )
                    };
                    return new QAScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }

    public class QAScreen : StatefulWidget {
        public QAScreen(
            Key key = null,
            QAScreenViewModel viewModel = null,
            QAScreenActionModel actionModel = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly QAScreenViewModel viewModel;
        public readonly QAScreenActionModel actionModel;

        public override State createState() {
            return new _QAScreenState();
        }
    }

    class _QAScreenState : AutomaticKeepAliveClientMixin<QAScreen>, TickerProvider, RouteAware {
        const int firstPageNumber = 1;
        CustomTabController _tabController;
        RefreshController _latestRefreshController;
        RefreshController _hotRefreshController;
        RefreshController _pendingRefreshController;
        float _navBarHeight;
        // bool sValue;
        // QATab _tab;
        // string _type;
        int _latestPageNumber = firstPageNumber;
        int _hotPageNumber = firstPageNumber;
        int _pendingPageNumber = firstPageNumber;
        bool latestListLoading;
        bool hotListLoading;
        bool pendingListLoading;
        BuildContext _context;

        protected override bool wantKeepAlive {
            get { return true; }
        }

        public override void initState() {
            base.initState();
            StatusBarManager.statusBarStyle(false);
            this._tabController = new CustomTabController(length: tabsList.Count, this);
            this._latestRefreshController = new RefreshController();
            this._hotRefreshController = new RefreshController();
            this._pendingRefreshController = new RefreshController();
            this._navBarHeight = CustomAppBarUtil.appBarHeight;
            this._latestPageNumber = 1;
            this.latestListLoading = true;
            this.hotListLoading = true;
            this.pendingListLoading = true;
            SchedulerBinding.instance.addPostFrameCallback(_ => {
                this.widget.actionModel.fetchQuestions(arg1: QATab.hot, "", arg3: firstPageNumber)
                    .then(v => {
                            this.hotListLoading = false;
                            this.setState(() => { });
                        }
                    )
                    .catchError(
                        err => {
                            this.hotListLoading = false;
                            this.setState(() => { });
                        }
                    );
                this.widget.actionModel.fetchQuestions(arg1: QATab.latest, "", arg3: firstPageNumber)
                    .then(
                        v => {
                            this.latestListLoading = false;
                            this.setState(() => { });
                        }
                    )
                    .catchError(
                        err => {
                            this.latestListLoading = false;
                            this.setState(() => { });
                        }
                    );
                this.widget.actionModel.fetchQuestions(arg1: QATab.pending, "", arg3: firstPageNumber)
                    .then(
                        v => {
                            this.pendingListLoading = false;
                            this.setState(() => { });
                        }
                    )
                    .catchError(
                        err => {
                            this.pendingListLoading = false;
                            this.setState(() => { });
                        }
                    );
            });
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            Main.ConnectApp.routeObserver.subscribe(this, (PageRoute) ModalRoute.of(context: this.context));
        }

        public override void dispose() {
            Main.ConnectApp.routeObserver.unsubscribe(this);
            base.dispose();
        }
        
        void pushToLoginPage() {
            Navigator.pushNamed(
                context: this.context,
                routeName: NavigatorRoutes.Login
            );
        }

        public override Widget build(BuildContext context) {
            this._context = context;
            return new Container(
                color: CColors.White,
                child: new CustomSafeArea(
                    bottom: false,
                    child: new Column(
                        children: new List<Widget> {
                            this._buildNavigationBar(),
                            this._buildTagCards(),
                            this._buildContent()
                        }
                    )
                )
            );
        }

        static bool _onNotification(ScrollNotification notification) {
            return true;
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
                                    Navigator.pushNamed(
                                        context: this.context,
                                        routeName: NavigatorRoutes.Search,
                                        new SearchScreenArguments {
                                            searchType = SearchType.question,
                                            isModal = true
                                        }
                                    );
                                    AnalyticsManager.ClickEnterSearch("Home_QA");
                                },
                                child: new Container(
                                    height: 32,
                                    decoration: new BoxDecoration(
                                        color: CColors.EmojiBottomBar,
                                        borderRadius: BorderRadius.all(2)
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
                                                "你想找的答案都在这儿",
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
                        new CustomButton(
                            padding: EdgeInsets.symmetric(8, 16),
                            onPressed: () => {
                                if (!UserInfoManager.isLoggedIn()) {
                                    this.pushToLoginPage();
                                }
                                else if (!UserInfoManager.isRealName()) {
                                    Navigator.pushNamed(
                                        context: this.context,
                                        routeName: NavigatorRoutes.RealName
                                    );
                                }
                                else {
                                    Navigator.pushNamed(
                                        context: this.context,
                                        routeName: NavigatorRoutes.PostQuestion,
                                        new PostQuestionScreenArguments {id = "", isModal = true}
                                    );
                                }
                            },
                            child: new Icon(
                                icon: CIcons.outline_ask,
                                size: 28,
                                color: CColors.PrimaryBlue
                            )
                        )
                    }
                )
            );
        }

        static readonly List<string> tabsList = new List<string> {
            "热门",
            "最新",
            "待回答"
        };

        Widget _buildContent() {
            var tabs = new List<object>();
            for (var i = 0; i < tabsList.Count; i++) {
                tabs.Add(this._buildSelectItem(tabsList[index: i], index: i));
            }

            var tabViews = new List<Widget>();
            for (var i = 0; i < tabsList.Count; i++) {
                var tab = tabsList[index: i];
                if (tab.Equals("热门")) {
                    tabViews.Add(this._buildHotQuestionList());
                }
                else if (tab.Equals("最新")) {
                    tabViews.Add(this._buildLatestQuestionList());
                }
                else if (tab.Equals("待回答")) {
                    tabViews.Add(this._buildPendingQuestionList());
                }
            }

            return new Expanded(
                child: new CustomSegmentedControl(
                    items: tabs,
                    children: tabViews,
                    newValue => { },
                    0,
                    trailing: _buildTrailing(),
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
                        fontSize: 16,
                        fontFamily: "Roboto-Medium"
                    ),
                    selectedTextStyle: new TextStyle(
                        fontSize: 16,
                        fontFamily: "Roboto-Medium"
                    ),
                    controller: this._tabController,
                    physics: new BouncingScrollPhysics(),
                    onTap: index => { this._tabController.animateTo(value: index); }
                )
            );
        }

        Widget _buildSelectItem(string title, int index) {
            return new Container(
                height: 44,
                alignment: Alignment.bottomCenter,
                child: new Container(
                    padding: EdgeInsets.only(8, 4, 8),
                    color: CColors.Transparent,
                    child: new Text(
                        data: title
                    )
                )
            );
        }

        static Widget _buildTrailing() {
            return new Container(
                padding: EdgeInsets.only(right: 16)
            );
        }

        Widget _buildLatestQuestionList() {
            var questions = this.widget.viewModel.latestQuestions;
            var enablePullUp = this.widget.viewModel.latestQuestionListHasMore;
            if (questions.isNullOrEmpty() && this.latestListLoading) {
                return ListView.builder(
                    physics: new NeverScrollableScrollPhysics(),
                    itemCount: 6,
                    itemBuilder: (cxt, index) => new QuestionLoadingCard()
                );
            }

            if (questions.isNullOrEmpty()) {
                return new BlankView(
                    "哎呀，暂无问题",
                    imageName: BlankImage.common,
                    true,
                    () => {
                        this.latestListLoading = true;
                        this._latestPageNumber = firstPageNumber;
                        this.setState(() => { });
                        this.widget.actionModel.fetchQuestions(arg1: QATab.latest, "", arg3: firstPageNumber)
                            .then(
                                _ => {
                                    this.latestListLoading = false;
                                    this.setState(() => { });
                                }
                            )
                            .catchError(
                                err => {
                                    this.latestListLoading = false;
                                    this.setState(() => { });
                                }
                            );
                    }
                );
            }

            return new Container(
                color: CColors.BgGrey,
                child: new CustomListView(
                    controller: this._latestRefreshController,
                    enablePullDown: true,
                    enablePullUp: enablePullUp,
                    onRefresh: this._latestOnRefresh,
                    itemCount: questions.Count,
                    itemBuilder: (ctx, index) =>
                        this._buildQuestionCard(index: index, tab: QATab.latest),
                    hasBottomMargin: true,
                    footerWidget: enablePullUp ? null : new EndView(hasBottomMargin: true)
                )
            );
        }

        void _latestOnRefresh(bool up) {
            this._latestPageNumber = up ? firstPageNumber : this._latestPageNumber + 1;
            this.widget.actionModel.fetchQuestions(arg1: QATab.latest, "", arg3: this._latestPageNumber)
                .then(_ => this._latestRefreshController.sendBack(up: up,
                    up ? RefreshStatus.completed : RefreshStatus.idle))
                .catchError(_ => this._latestRefreshController.sendBack(up: up, mode: RefreshStatus.failed));
        }

        Widget _buildHotQuestionList() {
            var questions = this.widget.viewModel.hotQuestions;
            var enablePullUp = this.widget.viewModel.hotQuestionListHasMore;
            if (questions.isNullOrEmpty() && this.hotListLoading) {
                return ListView.builder(
                    physics: new NeverScrollableScrollPhysics(),
                    itemCount: 6,
                    itemBuilder: (cxt, index) => new QuestionLoadingCard()
                );
            }

            if (questions.isNullOrEmpty()) {
                return new BlankView(
                    "哎呀，暂无问题",
                    imageName: BlankImage.common,
                    true,
                    () => {
                        this.hotListLoading = true;
                        this._hotPageNumber = firstPageNumber;
                        this.setState(() => { });
                        this.widget.actionModel.fetchQuestions(arg1: QATab.hot, "", arg3: firstPageNumber)
                            .then(
                                _ => {
                                    this.hotListLoading = false;
                                    this.setState(() => { });
                                }
                            )
                            .catchError(
                                err => {
                                    this.hotListLoading = false;
                                    this.setState(() => { });
                                }
                            );
                    }
                );
            }

            return new Container(
                color: CColors.BgGrey,
                child: new CustomListView(
                    controller: this._hotRefreshController,
                    enablePullDown: true,
                    enablePullUp: enablePullUp,
                    onRefresh: this._hotOnRefresh,
                    itemCount: questions.Count,
                    itemBuilder: (ctx, index) =>
                        this._buildQuestionCard(index: index, tab: QATab.hot),
                    hasBottomMargin: true,
                    footerWidget: enablePullUp ? null : new EndView(hasBottomMargin: true)
                )
            );
        }

        void _hotOnRefresh(bool up) {
            this._hotPageNumber = up ? firstPageNumber : this._hotPageNumber + 1;
            this.widget.actionModel.fetchQuestions(arg1: QATab.hot, "", arg3: this._hotPageNumber)
                .then(_ => this._hotRefreshController.sendBack(up: up,
                    up ? RefreshStatus.completed : RefreshStatus.idle))
                .catchError(_ => this._hotRefreshController.sendBack(up: up, mode: RefreshStatus.failed));
        }

        Widget _buildPendingQuestionList() {
            var questions = this.widget.viewModel.pendingQuestions;
            var enablePullUp = this.widget.viewModel.pendingQuestionListHasMore;
            if (questions.isNullOrEmpty() && this.pendingListLoading) {
                return ListView.builder(
                    physics: new NeverScrollableScrollPhysics(),
                    itemCount: 6,
                    itemBuilder: (cxt, index) => new QuestionLoadingCard()
                );
            }

            if (questions.isNullOrEmpty()) {
                return new BlankView(
                    "哎呀，暂无问题",
                    imageName: BlankImage.common,
                    true,
                    () => {
                        this.pendingListLoading = true;
                        this._pendingPageNumber = firstPageNumber;
                        this.setState(() => { });
                        this.widget.actionModel.fetchQuestions(arg1: QATab.pending, "", arg3: firstPageNumber)
                            .then(
                                _ => {
                                    this.pendingListLoading = false;
                                    this.setState(() => { });
                                }
                            )
                            .catchError(
                                err => {
                                    this.pendingListLoading = false;
                                    this.setState(() => { });
                                }
                            );
                    }
                );
            }

            return new Container(
                color: CColors.BgGrey,
                child: new CustomListView(
                    controller: this._pendingRefreshController,
                    enablePullDown: true,
                    enablePullUp: enablePullUp,
                    onRefresh: this._pendingOnRefresh,
                    itemCount: questions.Count,
                    itemBuilder: (ctx, index) =>
                        this._buildQuestionCard(index: index, tab: QATab.pending),
                    hasBottomMargin: true,
                    footerWidget: enablePullUp ? null : new EndView(hasBottomMargin: true)
                )
            );
        }

        void _pendingOnRefresh(bool up) {
            this._pendingPageNumber = up ? firstPageNumber : this._pendingPageNumber + 1;
            this.widget.actionModel.fetchQuestions(arg1: QATab.pending, "", arg3: this._pendingPageNumber)
                .then(_ => this._pendingRefreshController.sendBack(up: up,
                    up ? RefreshStatus.completed : RefreshStatus.idle))
                .catchError(_ => this._pendingRefreshController.sendBack(up: up, mode: RefreshStatus.failed));
        }

        Widget _buildTags(List<string> tags) {
            if (tags.isNotNullAndEmpty()) {
                return new Container(
                    color: CColors.White,
                    constraints: new BoxConstraints(
                        maxHeight: 22,
                        maxWidth: CCommonUtils.getQuestionCardTagsWidth(buildContext: this._context),
                        minWidth: CCommonUtils.getQuestionCardTagsWidth(buildContext: this._context)
                    ),
                    child: new Wrap(
                        spacing: 8,
                        children: _buildTagItems(tags: tags)
                    )
                );
            }

            return new Container();
        }

        static List<Widget> _buildTagItems(List<string> tags) {
            var widgets = new List<Widget>();
            tags.ForEach(item => {
                Widget tag = new GestureDetector(
                    // onTap: () => { },
                    child: new Container(
                        decoration: new BoxDecoration(
                            color: CColors.TagBackground,
                            borderRadius: BorderRadius.all(2)
                        ),
                        height: 22,
                        padding: EdgeInsets.symmetric(0, 8),
                        child: new Row(
                            mainAxisSize: MainAxisSize.min,
                            children: new List<Widget> {
                                new Text(
                                    data: item,
                                    maxLines: 1,
                                    style: new TextStyle(
                                        fontSize: 12,
                                        fontFamily: "Roboto-Regular",
                                        color: CColors.PrimaryBlue
                                    ),
                                    overflow: TextOverflow.ellipsis,
                                    textAlign: TextAlign.center
                                )
                            }
                        )
                    )
                );
                widgets.Add(item: tag);
            });
            return widgets;
        }

        Widget _buildQuestionCard(int index, QATab tab) {
            var questions = new List<Question>();
            if (tab == QATab.latest) {
                questions = this.widget.viewModel.latestQuestions;
            }
            else if (tab == QATab.hot) {
                questions = this.widget.viewModel.hotQuestions;
            }
            else if (tab == QATab.pending) {
                questions = this.widget.viewModel.pendingQuestions;
            }

            var question = questions[index: index];
            var tagDict = this.widget.viewModel.tagDict;
            var tags = new List<string>();
            if (tagDict.isNotNullAndEmpty() && question.tagIds.isNotNullAndEmpty()) {
                question.tagIds.ForEach(id => { tags.Add(item: tagDict[key: id].name); });
            }

            return new GestureDetector(
                onTap: () => Navigator.pushNamed(
                    context: this.context,
                    routeName: NavigatorRoutes.QuestionDetail,
                    new ScreenArguments {
                        id = question.id
                    }
                ),
                child: new Container(
                    padding: EdgeInsets.only(16, 12),
                    decoration: new BoxDecoration(
                        color: CColors.White,
                        border: new Border(
                            bottom: new BorderSide(
                                color: CColors.BgGrey,
                                8
                            )
                        )
                    ),
                    child: new Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: new List<Widget> {
                            new Padding(
                                padding: EdgeInsets.only(right: 16, bottom: 12),
                                child: new Text(
                                    data: question.title,
                                    maxLines: 3,
                                    overflow: TextOverflow.ellipsis,
                                    style: CTextStyle.PXLargeMedium.copyWith(height: 1.44f)
                                )
                            ),
                            new Padding(
                                padding: EdgeInsets.only(right: 16, bottom: 12),
                                child: this._buildTags(tags: tags)
                            ),
                            new Row(
                                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                children: new List<Widget> {
                                    new Padding(
                                        padding: EdgeInsets.only(bottom: 12),
                                        child: new Text(
                                            $"{CStringUtils.CountToString(count: question.likeCount, "0")} 投票" +
                                            $" · {CStringUtils.CountToString(count: question.answerCount, "0")} 回答 " +
                                            $"· {CStringUtils.CountToString(count: question.viewCount, "0")} 浏览",
                                            style: CTextStyle.PSmallBody5
                                        )
                                    ),
                                    new CustomButton(
                                        padding: EdgeInsets.only(16, right: 16, bottom: 12),
                                        child: new Icon(
                                            icon: CIcons.ellipsis,
                                            size: 20,
                                            color: CColors.BrownGrey
                                        ),
                                        onPressed: () => {
                                            ActionSheetUtils.showModalActionSheet(
                                                context: this.context,
                                                new ShareView(
                                                    buildContext: this.context,
                                                    shareSheetStyle: ShareSheetStyle.doubleDeck,
                                                    onPressed: type => {
                                                        var linkUrl =
                                                            CStringUtils.JointQuestionShareLink(
                                                                questionId: question.id);
                                                        if (type == ShareSheetItemType.clipBoard) {
                                                            this.widget.actionModel.copyText(obj: linkUrl);
                                                            CustomDialogUtils.showToast(
                                                                context: this.context,
                                                                "复制链接成功",
                                                                iconData: CIcons.check_circle_outline);
                                                        }
                                                        else if (type == ShareSheetItemType.block) {
                                                            ReportManager.blockQuestion(
                                                                context: this.context,
                                                                UserInfoManager.isLoggedIn(),
                                                                () => this.pushToLoginPage(),
                                                                () => this.widget.actionModel.blockQuestionAction(
                                                                    obj: question.id),
                                                                () => Navigator.pop(context: this.context)
                                                            );
                                                        }
                                                        else if (type == ShareSheetItemType.report) {
                                                            ReportManager.report(
                                                                UserInfoManager.isLoggedIn(),
                                                                () => this.pushToLoginPage(),
                                                                () => Navigator.pushNamed(
                                                                    context: this.context,
                                                                    routeName: NavigatorRoutes.Report,
                                                                    new ReportScreenArguments {
                                                                        id = question.id,
                                                                        reportType = ReportType.question
                                                                    }
                                                                )
                                                            );
                                                        }
                                                        else if (type == ShareSheetItemType.friends ||
                                                                 type == ShareSheetItemType.moments) {
                                                            CustomDialogUtils.showCustomDialog(
                                                                context: this.context,
                                                                child: new CustomLoadingDialog()
                                                            );
                                                            this.widget.actionModel.shareToWechat(
                                                                    arg1: type,
                                                                    question.title ?? "",
                                                                    question.descriptionPlain ?? "",
                                                                    arg4: linkUrl,
                                                                    "",
                                                                    ""
                                                                ).then(_ =>
                                                                    CustomDialogUtils.hiddenCustomDialog(
                                                                        context: this.context))
                                                                .catchError(_ =>
                                                                    CustomDialogUtils.hiddenCustomDialog(
                                                                        context: this.context));
                                                        }
                                                    }
                                                )
                                            );
                                        }
                                    )
                                }
                            )
                        }
                    )
                )
            );
        }

        static readonly List<TagCard> tagCards = new List<TagCard> {
            new TagCard {
                index = 1,
                name = "教程",
                icon = "image/tag/icon-tutorial.png",
                bg = "image/tag/bg-red.jpg"
            },
            new TagCard {
                index = 2,
                name = "提问模板",
                icon = "image/tag/icon-template.png",
                bg = "image/tag/bg-indigo.jpg"
            },
            new TagCard {
                index = 3,
                name = "报错",
                icon = "image/tag/icon-error.png",
                bg = "image/tag/bg-blue-gray.jpg"
            },
            new TagCard {
                index = 4,
                name = "效果实现",
                icon = "image/tag/icon-effect.png",
                bg = "image/tag/bg-blue.jpg"
            },
            new TagCard {
                index = 5,
                name = "MWU",
                icon = "image/tag/icon-mwu.png",
                bg = "image/tag/bg-purple.jpg"
            },
            new TagCard {
                index = 6,
                name = "代码",
                icon = "image/tag/icon-code.png",
                bg = "image/tag/bg-light-blue.jpg"
            },
            new TagCard {
                index = 7,
                name = "美术",
                icon = "image/tag/icon-finearts.png",
                bg = "image/tag/bg-pink.jpg"
            },
            new TagCard {
                index = 8,
                name = "Hub",
                icon = "image/tag/icon-hub.png",
                bg = "image/tag/bg-orchid.jpg"
            },
            // new TagCard {
            //     index = 9,
            //     name = "查看全部",
            //     icon = "image/tag/icon-more.png",
            //     bg = "image/tag/bg-deep-purple.jpg"
            // }
        };

        Widget _buildTagCards() {
            var tagWidgets = new List<Widget>();
            tagCards.ForEach(tag => { tagWidgets.Add(this.buildTagCard(tag: tag)); });

            return new Container(
                color: CColors.White,
                child: new Column(
                    children: new List<Widget> {
                        new Container(
                            height: 90,
                            // width: CCommonUtils.getScreenWidth(),
                            padding: EdgeInsets.only(top: 10, bottom: 16),
                            child: new ListView(
                                scrollDirection: Axis.horizontal,
                                physics: new BouncingScrollPhysics(),
                                padding: EdgeInsets.only(16, right: 8),
                                children: tagWidgets
                            )
                        ),
                        new CustomDivider(height: 8, color: CColors.BgGrey)
                    }
                )
            );
        }

        Widget buildTagCard(TagCard tag) {
            return new GestureDetector(
                onTap: () => {
                    Navigator.pushNamed(
                        context: this.context, 
                        routeName: NavigatorRoutes.Search,
                        new SearchScreenArguments {
                            searchType = SearchType.question,
                            keyword = tag.name
                        }
                    );
                    AnalyticsManager.ClickQAHomeTag(tagIndex: tag.index, tagName: tag.name);
                },
                child: new Container(
                    height: 64,
                    width: 102,
                    margin: EdgeInsets.only(right: 8),
                    decoration: new BoxDecoration(
                        image: new DecorationImage(
                            new FileImage(file: tag.bg),
                            fit: BoxFit.cover
                        ),
                        borderRadius: BorderRadius.circular(2)
                    ),
                    child: new Stack(
                        children: new List<Widget> {
                            new Align(
                                alignment: Alignment.topLeft,
                                child: new Container(
                                    padding: EdgeInsets.only(12, 11),
                                    child: new Text(
                                        data: tag.name,
                                        style: CTextStyle.PLargeMediumWhite.defaultHeight()
                                    )
                                )
                            ),
                            new Align(
                                alignment: Alignment.bottomRight,
                                child: new Container(
                                    padding: EdgeInsets.only(right: 8, bottom: 8),
                                    child: Image.file(
                                        file: tag.icon,
                                        height: 24,
                                        width: 24
                                    )
                                )
                            )
                        }
                    )
                )
            );
        }

        public Ticker createTicker(TickerCallback onTick) {
            return new Ticker(onTick: onTick, $"created by {this}");
        }

        public void didPopNext() {
            StatusBarManager.statusBarStyle(false);
        }

        public void didPush() {
        }

        public void didPop() {
        }

        public void didPushNext() {
        }
    }
}