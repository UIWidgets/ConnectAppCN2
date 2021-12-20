using System;
using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Components;
using ConnectApp.Components.Markdown;
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
    public class AnswerDetailScreenConnector : StatelessWidget {
        public AnswerDetailScreenConnector(
            string questionId,
            string answerId,
            Key key = null
        ) : base(key: key) {
            this.questionId = questionId;
            this.answerId = answerId;
        }

        readonly string questionId;
        readonly string answerId;

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, AnswerDetailScreenViewModel>(
                converter: state => {
                    state.qaState.questionDict.TryGetValue(key: this.questionId, out var question);
                    state.qaState.answerDict.TryGetValue(key: this.answerId, out var answer);
                    var messageList = new List<string>();
                    if (answer != null && answer.channelId.isNotEmpty()) {
                        messageList =
                            state.qaState.messageToplevelSimpleListDict.GetValueOrDefault(key: answer.channelId,
                                new List<string>());
                    }

                    var messages = new List<NewMessage>();
                    var messageDict = state.qaState.messageDict;
                    foreach (var messageId in messageList) {
                        if (messageDict.ContainsKey(key: messageId)) {
                            messages.Add(messageDict[key: messageId]);
                        }
                    }

                    var nextAnswerIdDict = state.qaState.nextAnswerIdDict;
                    var nextAnswerId = nextAnswerIdDict.GetValueOrDefault(key: this.answerId, "");

                    var currentUserId = state.loginState.loginInfo.userId ?? "";
                    var followMap =
                        state.followState.followDict.GetValueOrDefault(key: currentUserId,
                            new Dictionary<string, bool>());

                    var canAnswer = state.qaEditorState.canAnswerDict.GetValueOrDefault(key: this.questionId, false);
                    var authorId = answer?.authorId ?? "";
                    var author = state.userState.userDict.getOrDefault(key: authorId, null);
                    var canEdit = false;
                    if (currentUserId.isNotEmpty() && authorId.isNotEmpty() && currentUserId.Equals(value: authorId)) {
                        if (answer.status.isNotEmpty() && answer.status.Equals("published")) {
                            canEdit = true;
                        }
                    }

                    return new AnswerDetailScreenViewModel {
                        questionId = this.questionId,
                        answerId = this.answerId,
                        nextAnswerId = nextAnswerId,
                        question = question,
                        answer = answer,
                        userDict = state.userState.userDict,
                        imageDict = state.qaState.imageDict,
                        messages = messages,
                        isLoggedIn = state.loginState.isLoggedIn,
                        likeDict = state.qaState.likeDict,
                        followMap = followMap,
                        currentUserId = currentUserId,
                        canAnswer = canAnswer,
                        canEdit = canEdit,
                        author = author,
                        userLicenseDict = state.userState.userLicenseDict
                    };
                },
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new AnswerDetailScreenActionModel {
                        fetchAnswerDetail = (questionId, answerId) => dispatcher.dispatch<Future>(
                            CActions.fetchAnswerDetail(questionId: questionId, answerId: answerId)
                        ),
                        openUrl = url => OpenUrlUtil.OpenUrl(buildContext: context1, url: url),
                        likeAnswer = (questionId, answerId) => {
                            dispatcher.dispatch<Future>(CActions.qaLike(likeType: QALikeType.answer,
                                questionId: questionId, answerId: answerId));
                        },
                        removeLikeAnswer = (questionId, answerId) => {
                            dispatcher.dispatch<Future>(CActions.qaRemoveLike(likeType: QALikeType.answer,
                                questionId: questionId, answerId: answerId));
                        },
                        likeMessage = (channelId, messageId) => {
                            dispatcher.dispatch<Future>(CActions.qaLike(likeType: QALikeType.message,
                                channelId: channelId, messageId: messageId));
                        },
                        removeLikeMessage = (channelId, messageId) => {
                            dispatcher.dispatch<Future>(CActions.qaRemoveLike(likeType: QALikeType.message,
                                channelId: channelId, messageId: messageId));
                        },
                        startFollowUser = followUserId => dispatcher.dispatch(new StartFollowUserAction {
                            followUserId = followUserId
                        }),
                        followUser = followUserId =>
                            dispatcher.dispatch<Future>(CActions.fetchFollowUser(followUserId: followUserId)),
                        startUnFollowUser = unFollowUserId => dispatcher.dispatch(new StartUnFollowUserAction {
                            unFollowUserId = unFollowUserId
                        }),
                        unFollowUser = unFollowUserId =>
                            dispatcher.dispatch<Future>(CActions.fetchUnFollowUser(unFollowUserId: unFollowUserId)),
                        blockAnswerAction = answerId => {
                            dispatcher.dispatch(new BlockAnswerAction {answerId = answerId});
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
                    return new AnswerDetailScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }

    public class AnswerDetailScreen : StatefulWidget {
        public AnswerDetailScreen(
            Key key = null,
            AnswerDetailScreenViewModel viewModel = null,
            AnswerDetailScreenActionModel actionModel = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly AnswerDetailScreenViewModel viewModel;
        public readonly AnswerDetailScreenActionModel actionModel;

        public override State createState() {
            return new _AnswerDetailScreenState();
        }
    }

    class _AnswerDetailScreenState : AutomaticKeepAliveClientMixin<AnswerDetailScreen>, TickerProvider, RouteAware {
        // float _navBarHeight;
        // bool sValue;
        Animation<RelativeRect> _animation;
        AnimationController _controller;
        const float navBarHeight = 44;
        bool _isHaveTitle;
        float _titleHeight;
        bool _loading;
        string _loginSubId;
        bool animationIsReady;

        protected override bool wantKeepAlive {
            get { return true; }
        }
        
        public override void initState() {
            base.initState();
            // this._navBarHeight = CustomAppBarUtil.appBarHeight;
            // this.sValue = false;
            this._isHaveTitle = false;
            this._titleHeight = 0f;
            this._controller = new AnimationController(
                duration: TimeSpan.FromMilliseconds(100),
                vsync: this
            );
            var rectTween = new RelativeRectTween(
                RelativeRect.fromLTRB(0, top: navBarHeight, 0, 0),
                RelativeRect.fromLTRB(0, 0, 0, 0)
            );
            this._animation = rectTween.animate(parent: this._controller);
            this._loading = true;
            var questionId = this.widget.viewModel.questionId;
            var answerId = this.widget.viewModel.answerId;
            SchedulerBinding.instance.addPostFrameCallback(_ => {
                var modalRoute = ModalRoute.of(context: this.context);
                modalRoute.animation.addStatusListener(listener: this._animationStatusListener);
            });
            this._loginSubId = EventBus.subscribe(sName: EventBusConstant.login_success,
                args => { this.widget.actionModel.fetchAnswerDetail(arg1: questionId, arg2: answerId); });
        }

        void _animationStatusListener(AnimationStatus status) {
            if(status == AnimationStatus.completed) {
                this.animationIsReady = true;
                this.setState(() => {});
                var questionId = this.widget.viewModel.questionId;
                var answerId = this.widget.viewModel.answerId;
                this.widget.actionModel.fetchAnswerDetail(arg1: questionId, arg2: answerId)
                    .then(
                        v => {
                            this._loading = false;
                            this.setState(() => { });
                        }
                    )
                    .catchError(
                        err => {
                            this._loading = false;
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

        void modalQATopLevelComment(string channelId, string authorId, string answerId) {
            if (channelId.isNotEmpty()) {
                ActionSheetUtils.showModalActionSheet(
                    context: this.context,
                    new QATopLevelCommentScreenConnector(
                        channelId: channelId,
                        authorId: authorId,
                        messageType: QAMessageType.answer,
                        null,
                        answerId: answerId
                    )
                );
            }
        }

        public override Widget build(BuildContext context) {
            var question = this.widget.viewModel.question;
            var answer = this.widget.viewModel.answer;
            var questionId = this.widget.viewModel.questionId;
            var nextAnswerId = this.widget.viewModel.nextAnswerId;
            var author = this.widget.viewModel.author;

            if ((question == null || answer == null) && !this._loading) {
                return new Container(
                    color: CColors.White,
                    child: new CustomSafeArea(
                        child: new Column(
                            children: new List<Widget> {
                                this._buildNavigationBar(false),
                                new Flexible(
                                    child: new BlankView(
                                        "回答不存在",
                                        imageName: BlankImage.common
                                    )
                                )
                            }
                        )
                    )
                );
            }

            if (!this.animationIsReady || (question == null || answer == null) && this._loading || author == null ||
                answer?.description == null || answer.markdownBodyNodes == null) {
                return new Container(
                    color: CColors.White,
                    child: new CustomSafeArea(
                        child: new Column(
                            children: new List<Widget> {
                                this._buildNavigationBar(false),
                                new AnswerDetailLoading()
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
                                child: new Stack(
                                    fit: StackFit.expand,
                                    children: new List<Widget> {
                                        new Container(
                                            color: CColors.White,
                                            child: this._buildAnswerContent()
                                        ),
                                        nextAnswerId.isNotEmpty()
                                            ? (Widget) new Positioned(
                                                right: 16,
                                                bottom: 16,
                                                child: new CustomButton(
                                                    width: 120,
                                                    height: 40,
                                                    decoration: new BoxDecoration(
                                                        color: CColors.White,
                                                        borderRadius: BorderRadius.all(20),
                                                        border: Border.all(CColors.Separator2.withOpacity(0.2f)),
                                                        boxShadow: new List<BoxShadow> {
                                                            new BoxShadow(
                                                                CColors.TextTitle.withOpacity(0.15f),
                                                                blurRadius: 6,
                                                                spreadRadius: 0,
                                                                offset: new Offset(0, 2)
                                                            )
                                                        }
                                                    ),
                                                    onPressed: () => {
                                                        Navigator.pushReplacementNamed(
                                                            context: this.context,
                                                            routeName: NavigatorRoutes.AnswerDetail,
                                                            arguments: new AnswerDetailScreenArguments {
                                                                questionId = questionId,
                                                                answerId = nextAnswerId,
                                                                isModal = true
                                                            }
                                                        );
                                                    },
                                                    child: new Row(
                                                        mainAxisAlignment: MainAxisAlignment.center,
                                                        children: new List<Widget> {
                                                            new Icon(icon: CIcons.outline_arrow_double, size: 20),
                                                            new SizedBox(width: 4),
                                                            new Text(
                                                                "下一个回答",
                                                                style: CTextStyle.PMediumBody2.defaultHeight()
                                                            )
                                                        }
                                                    )
                                                )
                                            )
                                            : new Container()
                                    }
                                )
                            ),
                            this._buildAnswerTabBar()
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

        Widget _buildAnswerContent() {
            var answer = this.widget.viewModel.answer;
            return new CustomMarkdown(
                markdownStyleSheet: MarkdownUtils.qaStyleSheet(),
                data: answer.description,
                nodes: answer.markdownBodyNodes,
                syntaxHighlighter: new CSharpSyntaxHighlighter(),
                onTapLink: url => { this.widget.actionModel.openUrl(obj: url); },
                contentHead: this._buildAnswerHeader(),
                commentList: this._buildComments(),
                enablePullUp: false,
                enablePullDown: false,
                loginAction: () => Navigator.pushNamed(
                    context: this.context,
                    routeName: NavigatorRoutes.Login
                ),
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

        List<Widget> _buildComments() {
            var answer = this.widget.viewModel.answer;
            var answerComments = this.widget.viewModel.messages;
            var userDict = this.widget.viewModel.userDict;
            var likeDict = this.widget.viewModel.likeDict;

            if (answer == null) {
                return new List<Widget>();
            }

            var dataBar = new Container(
                color: CColors.White,
                padding: EdgeInsets.symmetric(16, 16),
                child: new Text(
                    $"赞成 {CStringUtils.CountToString(count: answer.likeCount, "0")} · " +
                    $"评论 {CStringUtils.CountToString(count: answer.commentCount, "0")} · " +
                    $"{DateConvert.DateStringFromNow(answer.createdTime ?? DateTime.Now)}",
                    style: CTextStyle.PRegularBody4.defaultHeight()
                )
            );

            var comments = new List<Widget> {
                dataBar,
                new Container(
                    color: CColors.White,
                    padding: EdgeInsets.only(16, 16, 16, 24),
                    child: new Text(
                        $"{answer.commentCount}条评论",
                        style: CTextStyle.H5.defaultHeight(),
                        textAlign: TextAlign.left
                    )
                )
            };

            if (answerComments.isEmpty()) {
                var user = this.widget.viewModel.userDict.GetValueOrDefault(key: this.widget.viewModel.currentUserId,
                    new User());
                var widget = new GestureDetector(
                    onTap: () => this.modalQATopLevelComment(
                        channelId: answer.channelId,
                        authorId: answer.authorId,
                        answerId: answer.id
                    ),
                    child: new Container(
                        padding: EdgeInsets.symmetric(horizontal: 16),
                        child: new Row(
                            children: new List<Widget> {
                                user.id.isNotEmpty()
                                    ? (Widget) Avatar.User(user: user, 24)
                                    : new Container(
                                        height: 24,
                                        width: 24,
                                        padding: EdgeInsets.only(1, 1, 2, 1),
                                        decoration: new BoxDecoration(
                                            color: CColors.White,
                                            border: new Border(
                                                new BorderSide(color: CColors.Icon),
                                                new BorderSide(color: CColors.Icon),
                                                new BorderSide(color: CColors.Icon),
                                                new BorderSide(color: CColors.Icon)
                                            ),
                                            borderRadius: BorderRadius.circular(12)
                                        ),
                                        child: new Icon(icon: CIcons.tab_mine_line, size: 20, color: CColors.Icon)
                                    ),
                                new Expanded(
                                    child: new Container(
                                        height: 32,
                                        decoration: new BoxDecoration(
                                            // color: CColors.White,
                                            border: new Border(
                                                new BorderSide(color: CColors.Separator),
                                                new BorderSide(color: CColors.Separator),
                                                new BorderSide(color: CColors.Separator),
                                                new BorderSide(color: CColors.Separator)
                                            ),
                                            borderRadius: BorderRadius.all(2)
                                        ),
                                        margin: EdgeInsets.only(8),
                                        padding: EdgeInsets.only(16, 8),
                                        child: new Text("快来写下你的见解吧~", style: CTextStyle.PRegularBody4.defaultHeight())
                                    )
                                )
                            }
                        )
                    )
                );
                comments.Add(item: widget);
                comments.Add(new Container(height: 24));
            }

            var commentsClip = new List<NewMessage>();
            commentsClip = answerComments.Count > 3 ? answerComments.GetRange(0, 3) : answerComments;
            foreach (var comment in commentsClip) {
                var isPraise = likeDict.ContainsKey(key: comment.id);
                var author = userDict.GetValueOrDefault(key: comment.authorId, new User());
                var commentWidget = new QACommentLiteCard(
                    message: comment,
                    author: author,
                    contentMaxLine: 3,
                    isPraised: isPraise,
                    onTap: () => this.modalQATopLevelComment(
                        channelId: answer.channelId,
                        authorId: answer.authorId,
                        answerId: answer.id
                    ),
                    praiseCallBack: () => {
                        if (this.widget.viewModel.isLoggedIn) {
                            if (isPraise) {
                                this.widget.actionModel.removeLikeMessage(arg1: comment.channelId, arg2: comment.id);
                            }
                            else {
                                this.widget.actionModel.likeMessage(arg1: comment.channelId, arg2: comment.id);
                            }
                        }
                        else {
                            Navigator.pushNamed(
                                context: this.context,
                                routeName: NavigatorRoutes.Login
                            );
                        }
                    },
                    pushToUserDetail: userId => {
                        Navigator.pushNamed(context: this.context, routeName: NavigatorRoutes.UserDetail,
                            new UserDetailScreenArguments {
                                id = userId
                            }
                        );
                    }
                );
                comments.Add(item: commentWidget);
            }

            if (commentsClip.Count > 0) {
                var showMoreWidget = new Container(
                    color: CColors.White,
                    padding: EdgeInsets.only(48, bottom: 16),
                    child: new CustomButton(
                        onPressed: () =>
                            this.modalQATopLevelComment(
                                channelId: answer.channelId,
                                authorId: answer.authorId,
                                answerId: answer.id
                            ),
                        padding: EdgeInsets.zero,
                        child: new Text(
                            $"查看全部 {answer.commentCount} 条评论",
                            style: CTextStyle.PMediumBody4.defaultHeight(),
                            maxLines: 1
                        )
                    )
                );
                comments.Add(item: showMoreWidget);
            }

            return comments;
        }

        Widget _buildAnswerTabBar() {
            var questionId = this.widget.viewModel.questionId;
            var answerId = this.widget.viewModel.answerId;
            var answer = this.widget.viewModel.answer;
            var canAnswer = this.widget.viewModel.canAnswer;
            var canEdit = this.widget.viewModel.canEdit;
            var likeDict = this.widget.viewModel.likeDict;
            var isVote = likeDict.ContainsKey(key: answerId);
            return new AnswerTabBar(
                vote: answer.likeCount,
                isVote: isVote,
                comment: answer.commentCount,
                canAnswer: canAnswer,
                canEdit: canEdit,
                likeCallback: () => {
                    if (this.widget.viewModel.isLoggedIn) {
                        if (isVote) {
                            this.widget.actionModel.removeLikeAnswer(arg1: questionId, arg2: answerId);
                        }
                        else {
                            this.widget.actionModel.likeAnswer(arg1: questionId, arg2: answerId);
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
                        if (canEdit) {
                            Navigator.pushNamed(
                                context: this.context,
                                routeName: NavigatorRoutes.PostAnswer,
                                new PostAnswerScreenArguments {
                                    questionId = questionId,
                                    answerId = answerId,
                                    canSave = true,
                                    isModal = true
                                }
                            );
                        }
                        else {
                            CustomDialogUtils.showToast(context: this.context, "已回答过了, 请在 我的-创作中心 查看",
                                iconData: CIcons.sentiment_satisfied, 2);
                        }
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
                    channelId: answer.channelId,
                    authorId: answer.authorId,
                    answerId: answer.id
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
            var answer = this.widget.viewModel.answer;
            Widget titleWidget = new Container();
            if (this._isHaveTitle) {
                titleWidget = new Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: new List<Widget> {
                        new Text(
                            data: question.title,
                            style: CTextStyle.PLargeMedium.defaultHeight(),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                            textAlign: TextAlign.left
                        ),
                        new SizedBox(height: 4),
                        new Text(
                            $"{CStringUtils.CountToString(count: question.answerCount, "0")}个回答",
                            style: CTextStyle.PRegularBody4.defaultHeight(),
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                            textAlign: TextAlign.left
                        )
                    }
                );
            }

            var rightWidget = new Container();
            if (showRightWidgets) {
                var linkUrl = CStringUtils.JointAnswerShareLink(questionId: question.id, answerId: answer.id);
                rightWidget = new Container(
                    child: new Row(
                        mainAxisAlignment: MainAxisAlignment.end,
                        children: new List<Widget> {
                            // new CustomButton(
                            //     padding: EdgeInsets.only(16, 10, 8, 10),
                            //     onPressed: () =>
                            //         CustomDialogUtils.showToast("未实现功能：分享", iconData: CIcons.sentiment_satisfied),
                            //     child: new Icon(icon: CIcons.outline_share2, size: 24, color: CColors.TextBody4)
                            // ),
                            new CustomButton(
                                padding: EdgeInsets.only(8, 10, 16, 10),
                                onPressed: () => {
                                    ShareManager.showDoubleDeckShareView(context: this.context,
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
                                        () => this.widget.actionModel.blockAnswerAction(obj: answer.id),
                                        () => {
                                            Navigator.pushNamed(
                                                context: this.context,
                                                routeName: NavigatorRoutes.Report,
                                                new ReportScreenArguments {
                                                    id = answer.id ?? "",
                                                    reportType = ReportType.mock
                                                }
                                            );
                                        },
                                        type => {
                                            CustomDialogUtils.showCustomDialog(
                                                context: this.context,
                                                child: new CustomLoadingDialog()
                                            );
                                            this.widget.actionModel.shareToWechat(
                                                    arg1: type,
                                                    arg2: question.title,
                                                    arg3: answer.descriptionPlain,
                                                    arg4: linkUrl,
                                                    "",
                                                    ""
                                                ).then(_ => CustomDialogUtils.hiddenCustomDialog(context: this.context))
                                                .catchError(_ =>
                                                    CustomDialogUtils.hiddenCustomDialog(context: this.context));
                                        },
                                        canEdit: this.widget.viewModel.canEdit,
                                        () => Navigator.pushNamed(
                                            context: this.context,
                                            routeName: NavigatorRoutes.PostAnswer,
                                            new PostAnswerScreenArguments {
                                                questionId = answer.questionId,
                                                answerId = answer.id,
                                                canSave = true,
                                                isModal = false
                                            }
                                        ),
                                        () => Navigator.pop(context: this.context)
                                    );
                                },
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
                this._isHaveTitle ? CColors.Separator2 : CColors.Transparent
            );
        }

        Widget _buildAnswerHeader() {
            var question = this.widget.viewModel.question;
            return new Container(
                color: CColors.White,
                child: new Column(
                    children: new List<Widget> {
                        new GestureDetector(
                            onTap: () => Navigator.pushNamed(
                                context: this.context,
                                routeName: NavigatorRoutes.QuestionDetail,
                                new ScreenArguments {
                                    id = question.id
                                }
                            ),
                            child: new Container(
                                width: CCommonUtils.getScreenWidth(buildContext: this.context),
                                padding: EdgeInsets.only(16, 8, 16, 16),
                                child: new Text(
                                    data: question.title,
                                    style: CTextStyle.H5.copyWith(height: 1.4f)
                                )
                            )
                        ),

                        new GestureDetector(
                            onTap: () => Navigator.pushNamed(
                                context: this.context,
                                routeName: NavigatorRoutes.QuestionDetail,
                                new ScreenArguments {
                                    id = question.id
                                }
                            ),
                            child: new Padding(
                                padding: EdgeInsets.only(16, right: 16, bottom: 16),
                                child: new Row(
                                    children: new List<Widget> {
                                        new Text(
                                            $"{CStringUtils.CountToString(count: question.answerCount, "0")}个回答",
                                            style: CTextStyle.PLargeBody4.defaultHeight()
                                        ),
                                        new SizedBox(width: 2),
                                        new Icon(icon: CIcons.baseline_forward_arrow, size: 14, color: CColors.Icon)
                                    }
                                )
                            )
                        ),
                        new CustomDivider(
                            color: CColors.BgGrey,
                            height: 8
                        ),
                        new Padding(
                            padding: EdgeInsets.all(16),
                            child: this._buildAvatar()
                        ),
                        new Padding(
                            padding: EdgeInsets.only(16, 0, 16, 16),
                            child: this._buildBestAnswerBanner()
                        )
                    }
                )
            );
        }

        Widget _buildAvatar() {
            var answer = this.widget.viewModel.answer;
            var user = this.widget.viewModel.author;
            Widget titleWidget = new Container();
            if (user.title.isNotEmpty()) {
                titleWidget = new Text(
                    data: user.title,
                    style: CTextStyle.PSmallBody4.defaultHeight(),
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis
                );
            }

            var userType = UserType.unFollow;
            if (!this.widget.viewModel.isLoggedIn) {
                userType = UserType.unFollow;
            }
            else {
                if (UserInfoManager.getUserInfo().userId == user.id) {
                    userType = UserType.me;
                }
                else if (user.followUserLoading ?? false) {
                    userType = UserType.loading;
                }
                else if (this.widget.viewModel.followMap.ContainsKey(key: user.id)) {
                    userType = UserType.follow;
                }
            }

            Widget rightWidget = new FollowButton(
                userType: userType,
                () => this._onFollow(userType: userType, userId: user.id)
            );

            return new Container(
                child: new Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: new List<Widget> {
                        new GestureDetector(
                            child: Avatar.User(
                                user: user,
                                38
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
                                                            style: CTextStyle.PMediumBody.defaultHeight(),
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
                                                        EdgeInsets.only(4)
                                                    )
                                                }
                                            ),
                                            new SizedBox(height: 6),
                                            titleWidget
                                        }
                                    )
                                )
                            )
                        ),
                        rightWidget
                    }
                )
            );
        }

        void _onFollow(UserType userType, string userId) {
            if (this.widget.viewModel.isLoggedIn) {
                if (userType == UserType.follow) {
                    ActionSheetUtils.showModalActionSheet(context: this.context,
                        new ActionSheet(
                            title: "确定不再关注？",
                            items: new List<ActionSheetItem> {
                                new ActionSheetItem("确定", type: ActionType.normal,
                                    () => {
                                        this.widget.actionModel.startUnFollowUser(obj: userId);
                                        this.widget.actionModel.unFollowUser(arg: userId);
                                    }),
                                new ActionSheetItem("取消", type: ActionType.cancel)
                            }
                        )
                    );
                }

                if (userType == UserType.unFollow) {
                    this.widget.actionModel.startFollowUser(obj: userId);
                    this.widget.actionModel.followUser(arg: userId);
                }
            }
            else {
                Navigator.pushNamed(
                    context: this.context,
                    routeName: NavigatorRoutes.Login
                );
            }
        }

        Widget _buildBestAnswerBanner() {
            var answer = this.widget.viewModel.answer;
            if (!answer.isAccepted) {
                return new Container();
            }

            return new Container(
                height: 40,
                alignment: Alignment.center,
                decoration: new BoxDecoration(
                    CColors.BestAnswerText.withOpacity(0.1f),
                    borderRadius: BorderRadius.all(2)
                ),
                child: new Padding(
                    padding: EdgeInsets.symmetric(horizontal: 12),
                    child: new Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        crossAxisAlignment: CrossAxisAlignment.center,
                        children: new List<Widget> {
                            new Row(
                                children: new List<Widget> {
                                    new Text(
                                        "该回答被作者标记为 ",
                                        style: new TextStyle(
                                            fontSize: 14,
                                            fontFamily: "Roboto-Regular",
                                            color: CColors.BestAnswerText
                                        )
                                    ),
                                    new Text(
                                        "已解决",
                                        style: new TextStyle(
                                            fontSize: 14,
                                            fontFamily: "Roboto-Medium",
                                            color: CColors.BestAnswerText
                                        )
                                    )
                                }
                            ),
                            new Icon(icon: CIcons.round_check_circle, size: 18, color: CColors.BestAnswerText)
                        }
                    )
                )
            );
        }

        public void didPopNext() {
            if (this.widget.viewModel.answerId.isNotEmpty()) {
                CTemporaryValue.currentPageModelId = this.widget.viewModel.answerId;
            }

            StatusBarManager.statusBarStyle(false);
        }

        public void didPush() {
            if (this.widget.viewModel.answerId.isNotEmpty()) {
                CTemporaryValue.currentPageModelId = this.widget.viewModel.answerId;
            }
        }

        public void didPop() {
            if (CTemporaryValue.currentPageModelId.isNotEmpty() &&
                this.widget.viewModel.answerId == CTemporaryValue.currentPageModelId) {
                CTemporaryValue.currentPageModelId = null;
            }
        }

        public void didPushNext() {
            CTemporaryValue.currentPageModelId = null;
        }

        public Ticker createTicker(TickerCallback onTick) {
            return new Ticker(onTick: onTick, "created by {this}");
        }
    }
}