using System;
using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Components;
using ConnectApp.Components.Markdown;
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
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Text = Unity.UIWidgets.widgets.Text;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace ConnectApp.screens {
    public class QuestionDetailScreenConnector : StatelessWidget {
        public QuestionDetailScreenConnector(
            string questionId,
            Key key = null
        ) : base(key: key) {
            D.assert(questionId != null);
            this.questionId = questionId;
        }

        readonly string questionId;

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, QuestionDetailScreenViewModel>(
                converter: state => {
                    var question = state.qaState.questionDict.getOrDefault(key: this.questionId, null);
                    var answerIds = state.qaState.answerIdsDict.getOrDefault(key: this.questionId, null);
                    state.qaState.answerListHasMoreDict.TryGetValue(key: this.questionId, out var answerHasMore);
                    var blockAnswerIds = state.qaState.blockAnswerList;
                    var answerDict = state.qaState.answerDict;
                    List<Answer> answers = null;
                    if (answerIds != null) {
                        answers = new List<Answer>();
                    }

                    if (answerIds.isNotNullAndEmpty()) {
                        answerIds.ForEach(answerId => {
                            if (answerId.isNotEmpty() && blockAnswerIds.Contains(item: answerId)) {
                                return;
                            }

                            if (answerDict.isNotNullAndEmpty() && answerDict.ContainsKey(key: answerId)) {
                                answers.Add(answerDict[key: answerId]);
                            }
                        });
                    }

                    var canAnswer = state.qaEditorState.canAnswerDict.GetValueOrDefault(key: this.questionId, false);
                    var currentUserId = state.loginState.loginInfo.userId ?? "";
                    var authorId = question?.authorId ?? "";
                    var author = state.userState.userDict.getOrDefault(key: authorId, null);
                    var canEdit = false;
                    if (currentUserId.isNotEmpty() && authorId.isNotEmpty() && currentUserId.Equals(value: authorId)) {
                        if (question.status.isNotEmpty() &&
                            (question.status.Equals("published") || question.status.Equals("resolved"))) {
                            canEdit = true;
                        }
                    }

                    return new QuestionDetailScreenViewModel {
                        questionId = this.questionId,
                        question = question,
                        answers = answers,
                        tagDict = state.tagState.tagDict,
                        userDict = state.userState.userDict,
                        imageDict = state.qaState.imageDict,
                        answerHasMore = answerHasMore,
                        isLoggedIn = state.loginState.isLoggedIn,
                        likeDict = state.qaState.likeDict,
                        canAnswer = canAnswer,
                        canEdit = canEdit,
                        author = author,
                        userLicenseDict = state.userState.userLicenseDict,
                    };
                },
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new QuestionDetailScreenActionModel {
                        fetchQuestionDetail = questionId =>
                            dispatcher.dispatch<Future>(CActions.fetchQuestionDetail(questionId: questionId)),
                        fetchAnswers = (questionId, page) =>
                            dispatcher.dispatch<Future>(CActions.fetchAnswers(questionId: questionId, page: page)),
                        likeQuestion = questionId => {
                            dispatcher.dispatch<Future>(CActions.qaLike(likeType: QALikeType.question,
                                questionId: questionId));
                        },
                        removeLikeQuestion = questionId => {
                            dispatcher.dispatch<Future>(CActions.qaRemoveLike(likeType: QALikeType.question,
                                questionId: questionId));
                        },
                        openUrl = url => OpenUrlUtil.OpenUrl(buildContext: context1, url: url),
                        blockQuestionAction = questionId => {
                            dispatcher.dispatch(new BlockQuestionAction {questionId = questionId});
                        },
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
                    return new QuestionDetailScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }

    public class QuestionDetailScreen : StatefulWidget {
        public QuestionDetailScreen(
            Key key = null,
            QuestionDetailScreenViewModel viewModel = null,
            QuestionDetailScreenActionModel actionModel = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly QuestionDetailScreenViewModel viewModel;
        public readonly QuestionDetailScreenActionModel actionModel;

        public override State createState() {
            return new _QuestionDetailScreenState();
        }
    }

    class _QuestionDetailScreenState : AutomaticKeepAliveClientMixin<QuestionDetailScreen>, TickerProvider, RouteAware {
        const float navBarHeight = 44;
        const int firstPageNumber = 1;

        RefreshController _refreshController;
        // bool sValue;
        Animation<RelativeRect> _animation;
        AnimationController _controller;
        bool _isHaveTitle;
        float _titleHeight;
        bool _expandQuestionDetail;
        bool _firstLoading;
        int _pageNumber;
        string _loginSubId;
        bool animationIsReady;

        protected override bool wantKeepAlive {
            get { return true; }
        }

        public override void initState() {
            base.initState();
            // this.sValue = false;
            this._isHaveTitle = false;
            this._titleHeight = 0f;
            this._expandQuestionDetail = false;
            this._refreshController = new RefreshController();
            this._controller = new AnimationController(
                duration: TimeSpan.FromMilliseconds(100),
                vsync: this
            );
            var rectTween = new RelativeRectTween(
                RelativeRect.fromLTRB(0, top: navBarHeight, 0, 0),
                RelativeRect.fromLTRB(0, 0, 0, 0)
            );
            this._animation = rectTween.animate(parent: this._controller);
            this._firstLoading = true;
            this._pageNumber = firstPageNumber;
            var questionId = this.widget.viewModel.questionId;
            SchedulerBinding.instance.addPostFrameCallback(_ => {
                var modalRoute = ModalRoute.of(context: this.context);
                modalRoute.animation.addStatusListener(listener: this._animationStatusListener);
            });
            this._loginSubId = EventBus.subscribe(sName: EventBusConstant.login_success,
                args => { this.widget.actionModel.fetchQuestionDetail(arg: questionId); });
        }

        void _animationStatusListener(AnimationStatus status) {
            if(status == AnimationStatus.completed) {
                this.animationIsReady = true;
                this.setState(() => {});
                var questionId = this.widget.viewModel.questionId;
                this.widget.actionModel.fetchQuestionDetail(arg: questionId)
                    .then(
                        v => {
                            this._firstLoading = false;
                            this.setState(() => { });
                        }
                    )
                    .catchError(
                        err => {
                            this._firstLoading = false;
                            this.setState(() => { });
                        }
                    );
            }
        }
        
        public override void didChangeDependencies() {
            base.didChangeDependencies();
            Main.ConnectApp.routeObserver.subscribe(this, (PageRoute) ModalRoute.of(context: this.context));
        }

        public override void dispose() {
            EventBus.unSubscribe(sName: EventBusConstant.login_success, id: this._loginSubId);
            Main.ConnectApp.routeObserver.unsubscribe(this);
            this._controller.dispose();
            base.dispose();
        }

        void modalQATopLevelComment(string channelId, string authorId, string questionId) {
            if (channelId.isNotEmpty()) {
                ActionSheetUtils.showModalActionSheet(
                    context: this.context,
                    new QATopLevelCommentScreenConnector(
                        channelId: channelId,
                        authorId: authorId,
                        messageType: QAMessageType.question,
                        questionId: questionId
                    )
                );
            }
        }

        public override Widget build(BuildContext context) {
            var question = this.widget.viewModel.question;
            var author = this.widget.viewModel.author;

            if (question == null && !this._firstLoading) {
                return new Container(
                    color: CColors.White,
                    child: new CustomSafeArea(
                        child: new Column(
                            children: new List<Widget> {
                                this._buildNavigationBar(false),
                                new Flexible(
                                    child: new BlankView(
                                        "问题不存在",
                                        imageName: BlankImage.common
                                    )
                                )
                            }
                        )
                    )
                );
            }

            if (!this.animationIsReady || question == null && this._firstLoading || author == null) {
                return new Container(
                    color: CColors.White,
                    child: new CustomSafeArea(
                        child: new Column(
                            children: new List<Widget> {
                                this._buildNavigationBar(false),
                                new QuestionDetailLoading()
                            }
                        )
                    )
                );
            }

            if (this._titleHeight == 0f && question.title.isNotEmpty()) {
                this._titleHeight = CTextUtils.CalculateTextHeight(
                    text: question.title,
                    textStyle: CTextStyle.H3,
                    MediaQuery.of(context: context).size.width - 16 * 2 // 16 is horizontal padding
                ) + 16; // 16 is top padding
            }

            var child = new Container(
                color: CColors.Background,
                child: new NotificationListener<ScrollNotification>(
                    onNotification: this._onNotification,
                    child: new Column(
                        children: new List<Widget> {
                            this._buildNavigationBar(),
                            new Expanded(
                                child: this._buildAnswerList()
                            ),
                            this._buildQuestionTabBar()
                        }
                    )
                )
            );

            return new Container(
                color: CColors.White,
                child: new CustomSafeArea(
                    child: child
                )
            );
        }

        Widget _buildQuestionTabBar() {
            var questionId = this.widget.viewModel.questionId;
            var question = this.widget.viewModel.question;
            var likeDict = this.widget.viewModel.likeDict;
            var isVote = likeDict.ContainsKey(key: questionId);
            var canAnswer = this.widget.viewModel.canAnswer;
            return new QuestionTabBar(
                vote: this.widget.viewModel.question.likeCount,
                isVote: isVote,
                comment: this.widget.viewModel.question.commentCount,
                likeCallback: () => {
                    if (this.widget.viewModel.isLoggedIn) {
                        if (isVote) {
                            this.widget.actionModel.removeLikeQuestion(obj: questionId);
                        }
                        else {
                            this.widget.actionModel.likeQuestion(obj: questionId);
                        }
                    }
                    else {
                        Navigator.pushNamed(
                            context: this.context,
                            routeName: NavigatorRoutes.Login
                        );
                    }
                },
                answerCallback: () => {
                    if (!UserInfoManager.isLoggedIn()) {
                        Navigator.pushNamed(
                            context: this.context,
                            routeName: NavigatorRoutes.Login
                        );
                    }
                    else if (!UserInfoManager.isRealName()) {
                        Navigator.pushNamed(
                            context: this.context,
                            routeName: NavigatorRoutes.RealName
                        );
                    }
                    else if (!canAnswer) {
                        CustomDialogUtils.showToast(context: this.context, "已回答过了, 请在 我的-创作中心 查看",
                            iconData: CIcons.sentiment_satisfied, 2);
                    }
                    else {
                        Navigator.pushNamed(
                            context: this.context,
                            routeName: NavigatorRoutes.PostAnswer,
                            new PostAnswerScreenArguments {
                                questionId = questionId,
                                answerId = null,
                                canSave = true,
                                isModal = true
                            }
                        );
                    }
                },
                commentCallback: () => this.modalQATopLevelComment(
                    channelId: question.channelId,
                    authorId: question.authorId,
                    questionId: question.id
                )
            );
        }

        bool _onNotification(ScrollNotification notification) {
            var axisDirection = notification.metrics.axisDirection;
            if (axisDirection == AxisDirection.left || axisDirection == AxisDirection.right) {
                return true;
            }

            var pixels = notification.metrics.pixels - notification.metrics.minScrollExtent;
            if (pixels > this._titleHeight) {
                if (this._isHaveTitle == false) {
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

        Widget _buildNavigationBar(bool showRightWidgets = true) {
            var question = this.widget.viewModel.question;
            Widget titleWidget = new Container();
            if (this._isHaveTitle) {
                titleWidget = new Center(
                    child: new Text(
                        data: question.title,
                        style: CTextStyle.PXLargeMedium,
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis,
                        textAlign: TextAlign.center
                    )
                );
            }

            var rightWidget = new Container();

            if (showRightWidgets) {
                var linkUrl = CStringUtils.JointQuestionShareLink(questionId: question.id);
                rightWidget = new Container(
                    child: new Row(
                        mainAxisAlignment: MainAxisAlignment.end,
                        children: new List<Widget> {
                            new CustomButton(
                                padding: EdgeInsets.only(8, 10, 16, 10),
                                onPressed: () =>
                                    ShareManager.showDoubleDeckShareView(
                                        context: this.context,
                                        true,
                                        UserInfoManager.isLoggedIn(),
                                        () => {
                                            Clipboard.setData(new ClipboardData(text: linkUrl));
                                            CustomDialogUtils.showToast(context: this.context, "复制链接成功",
                                                iconData: CIcons.check_circle_outline);
                                        },
                                        () => Navigator.pushNamed(
                                            context: this.context,
                                            routeName: NavigatorRoutes.Login
                                        ),
                                        () => this.widget.actionModel.blockQuestionAction(obj: question.id),
                                        () => Navigator.pushNamed(
                                            context: this.context,
                                            routeName: NavigatorRoutes.Report,
                                            new ReportScreenArguments {
                                                id = question.id ?? "",
                                                reportType = ReportType.mock
                                            }
                                        ),
                                        type => {
                                            CustomDialogUtils.showCustomDialog(
                                                context: this.context,
                                                child: new CustomLoadingDialog()
                                            );
                                            this.widget.actionModel.shareToWechat(
                                                    arg1: type,
                                                    arg2: question.title,
                                                    arg3: question.descriptionPlain,
                                                    arg4: linkUrl,
                                                    "",
                                                    ""
                                                ).then(_ =>
                                                    CustomDialogUtils.hiddenCustomDialog(context: this.context))
                                                .catchError(_ =>
                                                    CustomDialogUtils.hiddenCustomDialog(context: this.context));
                                        },
                                        canEdit: this.widget.viewModel.canEdit,
                                        () => Navigator.pushNamed(
                                            context: this.context,
                                            routeName: NavigatorRoutes.PostQuestion,
                                            new PostQuestionScreenArguments {id = question.id, isModal = false}
                                        ),
                                        () => Navigator.pop(context: this.context)
                                    ),
                                child: new Icon(icon: CIcons.baseline_more_vert, size: 24, color: CColors.TextBody4)
                            )
                        }
                    )
                );
            }

            return new CustomAppBar(
                () => Navigator.pop(context: this.context),
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
                ),
                rightWidget: rightWidget,
                bottomSeparatorColor: CColors.Transparent
            );
        }

        Widget _buildAnswerList() {
            var answers = this.widget.viewModel.answers;
            var height = CCommonUtils.getScreenHeight(buildContext: this.context) -
                         CCommonUtils.getSafeAreaBottomPadding(context: this.context) -
                         CCommonUtils.getSafeAreaTopPadding(context: this.context) - 44 - 48 - 48 - 8;

            if (answers == null) {
                return new ListView(
                    children: new List<Widget> {
                        this._buildQuestionHeader(),
                        new Container(
                            height: height,
                            child: new GlobalLoading()
                        )
                    }
                );
            }

            if (answers.isEmpty()) {
                return new ListView(
                    children: new List<Widget> {
                        this._buildQuestionHeader(),
                        new Container(
                            height: height,
                            child: new BlankView(
                                "快来写下第一条回答吧",
                                imageName: BlankImage.comment
                            )
                        )
                    }
                );
            }

            var enablePullUp = this.widget.viewModel.answerHasMore;

            return new Container(
                color: CColors.BgGrey,
                child: new CustomListView(
                    enablePullDown: false,
                    enablePullUp: enablePullUp,
                    controller: this._refreshController,
                    onRefresh: this._onRefresh,
                    itemCount: answers.Count,
                    itemBuilder: this._buildAnswerCard,
                    headerWidget: this._buildQuestionHeader(),
                    footerWidget: enablePullUp ? null : new EndView()
                )
            );
        }

        void _onRefresh(bool up) {
            this._pageNumber += 1;
            this.widget.actionModel.fetchAnswers(arg1: this.widget.viewModel.questionId, arg2: this._pageNumber)
                .then(_ => this._refreshController.sendBack(up: up, up ? RefreshStatus.completed : RefreshStatus.idle))
                .catchError(_ => this._refreshController.sendBack(up: up, mode: RefreshStatus.failed));
        }

        Widget _buildQuestionHeader() {
            var question = this.widget.viewModel.question;
            var dataBar = new Container(
                color: CColors.White,
                child: new Row(
                    children: new List<Widget> {
                        new Text("浏览 ", style: CTextStyle.PRegularBody4.defaultHeight()),
                        new Text(CStringUtils.CountToString(count: question.viewCount, "0"),
                            style: CTextStyle.PRegularBody2.defaultHeight()),
                        new Text(" · ", style: CTextStyle.PRegularBody5.defaultHeight()),
                        new GestureDetector(
                            onTap: () => this.modalQATopLevelComment(
                                channelId: question.channelId,
                                authorId: question.authorId,
                                questionId: question.id
                            ),
                            child: new Text("评论 ", style: CTextStyle.PRegularBody4.defaultHeight())
                        ),
                        new GestureDetector(
                            onTap: () => this.modalQATopLevelComment(
                                channelId: question.channelId,
                                authorId: question.authorId,
                                questionId: question.id
                            ),
                            child: new Text(CStringUtils.CountToString(count: question.commentCount, "0"),
                                style: CTextStyle.PRegularBody2.defaultHeight())
                        )
                    }
                )
            );
            var tagDict = this.widget.viewModel.tagDict;
            var tags = new List<Tag>();
            if (tagDict.isNotNullAndEmpty() && question.tagIds.isNotNullAndEmpty()) {
                question.tagIds.ForEach(id => {
                    if (id.isNotEmpty() && tagDict.ContainsKey(key: id)) {
                        tags.Add(tagDict[key: id]);
                    }
                });
            }

            return new Container(
                padding: EdgeInsets.only(top: 10),
                decoration: new BoxDecoration(
                    color: CColors.White,
                    border: new Border(
                        bottom: new BorderSide(CColors.Separator2.withOpacity(0.5f))
                    )
                ),
                child: new Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: new List<Widget> {
                        new Padding(
                            padding: EdgeInsets.only(bottom: 16),
                            child: this._buildTags(tags: tags)
                        ),
                        new Padding(
                            padding: EdgeInsets.only(16, right: 16, bottom: 16),
                            child: new Text(
                                data: question.title,
                                style: CTextStyle.H5.copyWith(height: 1.4f)
                            )
                        ),
                        new Padding(
                            padding: EdgeInsets.symmetric(horizontal: 16),
                            child: this._buildAvatar()
                        ),
                        this._buildQuestionInfo(),
                        new Container(
                            padding: EdgeInsets.only(16, right: 16, top: 4, bottom: 20),
                            child: new Row(
                                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                children: new List<Widget> {
                                    dataBar,
                                    this._expandQuestionDetail
                                        ? (Widget) new CustomButton(
                                            onPressed: () => {
                                                this._expandQuestionDetail = false;
                                                this.setState(() => { });
                                                this._refreshController.scrollTo(0);
                                            },
                                            child: new Row(
                                                children: new List<Widget> {
                                                    new Text("收起", style: CTextStyle.PRegularBody5.defaultHeight()),
                                                    new SizedBox(width: 2),
                                                    new Icon(icon: CIcons.round_keyboard_arrow_up, size: 18,
                                                        color: CColors.Icon)
                                                }
                                            )
                                        )
                                        : new Container()
                                }
                            )
                        ),
                        new CustomDivider(
                            color: CColors.BgGrey,
                            height: 8
                        ),
                        new Container(
                            height: 48,
                            padding: EdgeInsets.symmetric(horizontal: 16),
                            child: new Row(
                                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                crossAxisAlignment: CrossAxisAlignment.center,
                                children: new List<Widget> {
                                    new Text(
                                        $"{CStringUtils.CountToString(count: question.answerCount, "0")}个回答",
                                        style: CTextStyle.PLargeMedium.defaultHeight()
                                    ),
                                    new Container()
                                }
                            )
                        )
                    }
                )
            );
        }

        Widget _buildAvatar() {
            var question = this.widget.viewModel.question;
            var user = this.widget.viewModel.author;
            return new Container(
                child: new Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: new List<Widget> {
                        new GestureDetector(
                            child: Avatar.User(
                                user: user,
                                24
                            ),
                            onTap: () => {
                                Navigator.pushNamed(context: this.context, routeName: NavigatorRoutes.UserDetail,
                                    new UserDetailScreenArguments {
                                        id = user.id
                                    }
                                );
                            }),
                        new Expanded(
                            child: new GestureDetector(
                                onTap: () => {
                                    Navigator.pushNamed(context: this.context, routeName: NavigatorRoutes.UserDetail,
                                        new UserDetailScreenArguments {
                                            id = user.id
                                        }
                                    );
                                },
                                child: new Container(
                                    color: CColors.Transparent,
                                    margin: EdgeInsets.only(8, right: 16),
                                    child: new Column(
                                        crossAxisAlignment: CrossAxisAlignment.start,
                                        children: new List<Widget> {
                                            new Row(
                                                children: new List<Widget> {
                                                    new Flexible(
                                                        child: new Text(
                                                            user.fullName ?? user.name ?? "佚名",
                                                            style: CTextStyle.PRegularBody2.defaultHeight(),
                                                            maxLines: 1,
                                                            overflow: TextOverflow.ellipsis
                                                        )
                                                    ),
                                                    CImageUtils.GenBadgeImage(
                                                        badges: user.badges,
                                                        CCommonUtils.GetUserLicense(
                                                            userId: user.id,
                                                            userLicenseMap: this.widget.viewModel.userLicenseDict
                                                        ),
                                                        EdgeInsets.symmetric(horizontal: 4)
                                                    ),
                                                    new Text(
                                                        ", ",
                                                        style: CTextStyle.PRegularBody2.defaultHeight()
                                                    ),
                                                    new Text(
                                                        DateConvert.DateStringFromNow(question.createdTime ??
                                                            DateTime.Now),
                                                        style: CTextStyle.PRegularBody5.defaultHeight(),
                                                        maxLines: 1
                                                    )
                                                }
                                            )
                                        }
                                    )
                                )
                            )
                        )
                    }
                )
            );
        }

        Widget _buildImages() {
            var imagesWidth = CCommonUtils.getScreenWithoutPadding16Width(buildContext: this.context);
            const int imagePadding = 2;
            const int borderRadius = 2;
            var images = new List<string>();
            var contentIds = this.widget.viewModel.question.contentIds;
            var imageDict = this.widget.viewModel.imageDict;
            if (contentIds.isNotNullAndEmpty() && imageDict.isNotNullAndEmpty()) {
                contentIds.ForEach(id => {
                    if (imageDict.ContainsKey(key: id) && imageDict[key: id].isNotEmpty()) {
                        images.Add(imageDict[key: id]);
                    }
                });
            }

            if (images.isNullOrEmpty()) {
                return new Container();
            }

            Widget imagesWidget = new Container();

            switch (images.Count) {
                case 1: {
                    var imageWidth = imagesWidth;
                    var imageHeight = imagesWidth / 2;
                    var image = images.first();
                    imagesWidget = new PlaceholderImage(
                        imageUrl: image,
                        width: imageWidth,
                        height: imageHeight,
                        borderRadius: borderRadius,
                        fit: BoxFit.cover,
                        color: CColorUtils.GetSpecificDarkColorFromId(id: image)
                    );
                    break;
                }
                case 2: {
                    var imageWidth = (imagesWidth - imagePadding) / 2;
                    var imageHeight = imageWidth * 2 / 3;
                    var firstImage = images.first();
                    var lastImage = images.last();
                    imagesWidget = new ClipRRect(
                        borderRadius: BorderRadius.all(radius: borderRadius),
                        child: new Row(
                            children: new List<Widget> {
                                new PlaceholderImage(
                                    imageUrl: firstImage,
                                    width: imageWidth,
                                    height: imageHeight,
                                    fit: BoxFit.cover,
                                    color: CColorUtils.GetSpecificDarkColorFromId(id: firstImage)
                                ),
                                new SizedBox(width: 2),
                                new PlaceholderImage(
                                    imageUrl: lastImage,
                                    width: imageWidth,
                                    height: imageHeight,
                                    fit: BoxFit.cover,
                                    color: CColorUtils.GetSpecificDarkColorFromId(id: lastImage)
                                )
                            }
                        )
                    );
                    break;
                }
                default: {
                    if (images.Count >= 3) {
                        var overCount = images.Count - 3;
                        var imageWidth = (imagesWidth - imagePadding * 2) / 3;
                        var imageHeight = imageWidth * 2 / 3;
                        var firstImage = images[0];
                        var secondImage = images[1];
                        var thirdImage = images[2];
                        imagesWidget = new ClipRRect(
                            borderRadius: BorderRadius.all(radius: borderRadius),
                            child: new Row(
                                children: new List<Widget> {
                                    new PlaceholderImage(
                                        imageUrl: firstImage,
                                        width: imageWidth,
                                        height: imageHeight,
                                        fit: BoxFit.cover,
                                        color: CColorUtils.GetSpecificDarkColorFromId(id: firstImage)
                                    ),
                                    new SizedBox(width: 2),
                                    new PlaceholderImage(
                                        imageUrl: secondImage,
                                        width: imageWidth,
                                        height: imageHeight,
                                        fit: BoxFit.cover,
                                        color: CColorUtils.GetSpecificDarkColorFromId(id: secondImage)
                                    ),
                                    new SizedBox(width: 2),
                                    new Stack(
                                        children: new List<Widget> {
                                            new PlaceholderImage(
                                                imageUrl: thirdImage,
                                                width: imageWidth,
                                                height: imageHeight,
                                                fit: BoxFit.cover,
                                                color: CColorUtils.GetSpecificDarkColorFromId(id: thirdImage)
                                            ),
                                            Positioned.fill(
                                                overCount > 0
                                                    ? new Container(
                                                        alignment: Alignment.center,
                                                        color: CColors.Black.withOpacity(0.3f),
                                                        child: new Text(
                                                            $"+{overCount}",
                                                            style: CTextStyle.PXLargeMediumWhite.defaultHeight()
                                                        )
                                                    )
                                                    : new Container()
                                            )
                                        }
                                    )
                                }
                            )
                        );
                    }

                    break;
                }
            }

            return new Padding(
                padding: EdgeInsets.only(bottom: 16),
                child: imagesWidget
            );
        }

        Widget _buildQuestionInfo() {
            var question = this.widget.viewModel.question;
            if (this._expandQuestionDetail) {
                return new Markdown(
                    data: question.description,
                    nodes: question.markdownBodyNodes,
                    shrinkWrap: true,
                    physics: new NeverScrollableScrollPhysics(),
                    markdownStyleSheet: MarkdownUtils.qaStyleSheet(),
                    syntaxHighlighter: new CSharpSyntaxHighlighter(),
                    onTapLink: url => this.widget.actionModel.openUrl(obj: url),
                    browserImageInMarkdown: url => {
                        Navigator.pushNamed(
                            context: this.context,
                            routeName: NavigatorRoutes.PhotoView,
                            new PhotoViewScreenArguments {
                                url = url,
                                urls = MarkdownUtils.markdownImages
                            }
                        );
                    }
                );
            }

            var expandWidget = new Container();
            var height = CTextUtils.CalculateTextHeight(
                text: question.descriptionPlain,
                textStyle: CTextStyle.PLargeBody,
                CCommonUtils.getScreenWithoutPadding16Width(buildContext: this.context)
            );
            var tinyHeight = CTextUtils.CalculateTextHeight(
                text: question.descriptionPlain,
                textStyle: CTextStyle.PLargeBody,
                CCommonUtils.getScreenWithoutPadding16Width(buildContext: this.context),
                3
            );

            if (height > tinyHeight || question.contentIds.isNotNullAndEmpty()) {
                expandWidget = new Container(
                    padding: EdgeInsets.only(20),
                    decoration: new BoxDecoration(
                        color: CColors.White,
                        gradient: new LinearGradient(
                            begin: Alignment.centerLeft,
                            end: Alignment.centerRight,
                            new List<Color> {
                                new Color(0xFFFFFFFF).withOpacity(0.1f),
                                new Color(0xFFFFFFFF)
                            },
                            new List<float> {0, 0.2f}
                        )
                    ),
                    height: 22,
                    child: new Row(
                        children: new List<Widget> {
                            new Text("查看全部",
                                style: CTextStyle.PRegularBody5.defaultHeight()),
                            new Icon(icon: CIcons.round_keyboard_arrow_down, size: 18,
                                color: CColors.Icon)
                        }
                    )
                );
            }

            return new Container(
                padding: EdgeInsets.only(16, right: 16, top: 16),
                child: new Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: new List<Widget> {
                        new Container(
                            child: new Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: new List<Widget> {
                                    new Stack(
                                        children: new List<Widget> {
                                            new Container(
                                                width: CCommonUtils.getScreenWithoutPadding16Width(
                                                    buildContext: this.context),
                                                child: new Text(
                                                    this.widget.viewModel.question.descriptionPlain ?? "",
                                                    style: CTextStyle.PLargeBody2,
                                                    maxLines: 3,
                                                    overflow: TextOverflow.fade
                                                )
                                            ),
                                            new Positioned(
                                                right: 0,
                                                bottom: 0,
                                                child: new GestureDetector(
                                                    onTap: () => {
                                                        this._expandQuestionDetail = true;
                                                        this.setState(() => { });
                                                    },
                                                    child: expandWidget
                                                )
                                            )
                                        }
                                    ),
                                    new SizedBox(height: 16),
                                    this._buildImages()
                                }
                            )
                        )
                    }
                )
            );
        }

        Widget _buildTags(List<Tag> tags) {
            if (tags.isNotNullAndEmpty()) {
                return new Container(
                    color: CColors.White,
                    constraints: new BoxConstraints(
                        maxHeight: 28,
                        maxWidth: CCommonUtils.getScreenWidth(buildContext: this.context),
                        minWidth: CCommonUtils.getScreenWidth(buildContext: this.context)
                    ),
                    child: new NoScrollbar(
                        new ListView(
                            scrollDirection: Axis.horizontal,
                            padding: EdgeInsets.symmetric(horizontal: 16),
                            children: this._buildTagItems(tags: tags)
                        )
                    )
                );
            }

            return new Container();
        }

        List<Widget> _buildTagItems(List<Tag> tags) {
            var questionId = this.widget.viewModel.questionId;
            var widgets = new List<Widget>();
            for (var i = 0; i < tags.Count; i++) {
                var tag = tags[index: i];
                Widget tagWidget = new GestureDetector(
                    onTap: () => {
                        Navigator.pushNamed(
                            context: this.context,
                            routeName: NavigatorRoutes.Search,
                            new SearchScreenArguments {
                                searchType = SearchType.question,
                                keyword = tag.name
                            }
                        );
                        AnalyticsManager.ClickQuestionTag(questionId: questionId, tagId: tag.id);
                    },
                    child: new Container(
                        margin: EdgeInsets.only(right: i == tags.Count - 1 ? 0 : 8),
                        decoration: new BoxDecoration(
                            color: CColors.F4Bg,
                            borderRadius: BorderRadius.all(2)
                        ),
                        height: 28,
                        padding: EdgeInsets.symmetric(0, 12),
                        child: new Row(
                            mainAxisSize: MainAxisSize.min,
                            children: new List<Widget> {
                                new Text(
                                    data: tag.name,
                                    maxLines: 1,
                                    style: new TextStyle(
                                        fontSize: 14,
                                        fontFamily: "Roboto-Regular",
                                        color: CColors.TextBody2
                                    ),
                                    overflow: TextOverflow.ellipsis,
                                    textAlign: TextAlign.center
                                )
                            }
                        )
                    )
                );
                widgets.Add(item: tagWidget);
            }

            return widgets;
        }

        Widget _buildAnswerCard(BuildContext buildContext, int index) {
            var questionId = this.widget.viewModel.questionId;
            var answer = this.widget.viewModel.answers[index: index];
            var user = this.widget.viewModel.userDict[key: answer.authorId];
            var images = new List<string>();
            var contentIds = answer.contentIds;
            var imageDict = this.widget.viewModel.imageDict;
            if (contentIds.isNotNullAndEmpty() && imageDict.isNotNullAndEmpty()) {
                contentIds.ForEach(id => {
                    if (imageDict.ContainsKey(key: id) && imageDict[key: id].isNotEmpty()) {
                        images.Add(imageDict[key: id]);
                    }
                });
            }

            return new AnswerCard(
                answer: answer,
                user: user,
                images: images,
                userLicenseDict: this.widget.viewModel.userLicenseDict,
                () => Navigator.pushNamed(
                    context: this.context,
                    routeName: NavigatorRoutes.AnswerDetail,
                    new AnswerDetailScreenArguments {questionId = questionId, answerId = answer.id}
                )
            );
        }

        public void didPopNext() {
            if (this.widget.viewModel.questionId.isNotEmpty()) {
                CTemporaryValue.currentPageModelId = this.widget.viewModel.questionId;
            }

            StatusBarManager.statusBarStyle(false);
        }

        public void didPush() {
            if (this.widget.viewModel.questionId.isNotEmpty()) {
                CTemporaryValue.currentPageModelId = this.widget.viewModel.questionId;
            }
        }

        public void didPop() {
            if (CTemporaryValue.currentPageModelId.isNotEmpty() &&
                this.widget.viewModel.questionId == CTemporaryValue.currentPageModelId) {
                CTemporaryValue.currentPageModelId = null;
            }
        }

        public void didPushNext() {
            CTemporaryValue.currentPageModelId = null;
        }

        public Ticker createTicker(TickerCallback onTick) {
            return new Ticker(onTick: onTick, $"created by {this}");
        }
    }
}