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
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.widgets;

namespace ConnectApp.screens {
    public class QATopLevelCommentScreenConnector : StatelessWidget {
        public QATopLevelCommentScreenConnector(
            string channelId,
            string authorId,
            QAMessageType messageType,
            string questionId = null,
            string answerId = null,
            Key key = null
        ) : base(key: key) {
            this.channelId = channelId;
            this.authorId = authorId;
            this.messageType = messageType;
            this.questionId = questionId;
            this.answerId = answerId;
        }

        string channelId;
        string authorId;
        QAMessageType messageType;
        string questionId;
        string answerId;

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, QATopLevelCommentScreenViewModel>(
                converter: state => {
                    var commentCount = 0;
                    var itemId = "";
                    if (this.questionId.isNotEmpty()) {
                        var question = state.qaState.questionDict.GetValueOrDefault(
                            key: this.questionId,
                            new Question()
                        );
                        commentCount = question.commentCount;
                        itemId = this.questionId;
                    }
                    else if (this.answerId.isNotEmpty()) {
                        var answer = state.qaState.answerDict.GetValueOrDefault(
                            key: this.answerId,
                            new Answer()
                        );
                        commentCount = answer.commentCount;
                        itemId = this.answerId;
                    }

                    var userDict = state.userState.userDict;
                    var author = userDict.GetValueOrDefault(key: this.authorId, new User());

                    var messageDict = state.qaState.messageDict;
                    var topLevelCommentList = state.qaState.messageToplevelListDict.GetValueOrDefault(
                        key: this.channelId,
                        new NewMessageList()
                    );
                    var topLevelCommentIds = new List<string>();
                    if (topLevelCommentList.list.isNotNullAndEmpty()) {
                        topLevelCommentIds = topLevelCommentList.list;
                    }

                    var hasMore = topLevelCommentList.hasMore;
                    var lastId = topLevelCommentList.lastId;
                    var topLevelComments = new List<NewMessage>();
                    foreach (var topLevelCommentId in topLevelCommentIds) {
                        if (messageDict.ContainsKey(key: topLevelCommentId)) {
                            var message = messageDict[key: topLevelCommentId];
                            var childMessageIds = state.qaState.messageSecondLevelSimpleListDict.GetValueOrDefault(
                                key: topLevelCommentId,
                                new NewMessageList()
                            ).list;
                            var childMessages = new List<NewMessage>();
                            foreach (var childMessageId in childMessageIds) {
                                if (messageDict.ContainsKey(key: childMessageId)) {
                                    childMessages.Add(messageDict[key: childMessageId]);
                                }
                            }

                            message.childMessages = childMessages;
                            topLevelComments.Add(item: message);
                        }
                    }

                    return new QATopLevelCommentScreenViewModel {
                        channelId = this.channelId,
                        itemId = itemId,
                        author = author,
                        messages = topLevelComments,
                        messageDict = messageDict,
                        userDict = userDict,
                        userLicenseDict = state.userState.userLicenseDict,
                        commentCount = commentCount,
                        lastId = lastId,
                        hasMore = hasMore,
                        isLoggedIn = state.loginState.isLoggedIn,
                        likeDict = state.qaState.likeDict,
                        messageType = this.messageType
                    };
                },
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new QATopLevelCommentScreenActionModel {
                        fetchQATopLevelComment = (channelId, after) =>
                            dispatcher.dispatch<Future>(
                                CActions.fetchQATopLevelComment(channelId: channelId, after: after)
                            ),
                        sendComment =
                            (messageType, itemId, channelId, content, nonce, parentMessageId, upperMessageId) => {
                                AnalyticsManager.ClickPublishComment("QA", channelId: channelId, commentId: parentMessageId);
                                return dispatcher.dispatch<Future>(
                                    CActions.sendQAMessage(messageType: messageType, itemId: itemId,
                                        channelId: channelId, content: content,
                                        nonce: nonce, parentMessageId: parentMessageId,
                                        upperMessageId: upperMessageId));
                            },
                        likeMessage = (channelId, messageId) => {
                            dispatcher.dispatch<Future>(CActions.qaLike(likeType: QALikeType.message,
                                channelId: channelId, messageId: messageId));
                        },
                        removeLikeMessage = (channelId, messageId) => {
                            dispatcher.dispatch<Future>(CActions.qaRemoveLike(likeType: QALikeType.message,
                                channelId: channelId, messageId: messageId));
                        }
                    };
                    return new QATopLevelCommentScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }


    public class QATopLevelCommentScreen : StatefulWidget {
        public QATopLevelCommentScreen(
            Key key = null,
            QATopLevelCommentScreenViewModel viewModel = null,
            QATopLevelCommentScreenActionModel actionModel = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly QATopLevelCommentScreenViewModel viewModel;
        public readonly QATopLevelCommentScreenActionModel actionModel;

        public override State createState() {
            return new _QATopLevelCommentScreenState();
        }
    }

    class _QATopLevelCommentScreenState : State<QATopLevelCommentScreen> {
        RefreshController _refreshController;
        bool _firstLoading;

        public override void initState() {
            base.initState();
            StatusBarManager.statusBarStyle(false);
            this._refreshController = new RefreshController();
            this._firstLoading = true;
            var channelId = this.widget.viewModel.channelId;
            Future.delayed(TimeSpan.FromMilliseconds(500), () => {
                SchedulerBinding.instance.addPostFrameCallback(_ => {
                    this.widget.actionModel.fetchQATopLevelComment(arg1: channelId, "")
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
                });
                return FutureOr.nil;
            });
        }
        
        void pushToLoginPage() {
            Navigator.pushNamed(
                context: this.context,
                routeName: NavigatorRoutes.Login
            );
        }

        public override Widget build(BuildContext context) {
            return new Container(
                color: CColors.Grey99,
                padding: EdgeInsets.only(top: CCommonUtils.getSafeAreaTopPadding(context: context)),
                child: new Column(
                    children: new List<Widget> {
                        this._buildHeader(),
                        new Expanded(
                            child: this._buildCommentList()
                        ),
                        this._buildCommentTabBar(),
                        new Container(
                            color: CColors.White,
                            height: CCommonUtils.getSafeAreaBottomPadding(context: context)
                        )
                    }
                )
            );
        }

        Widget _buildCommentTabBar() {
            return new Container(
                height: 49,
                padding: EdgeInsets.only(16, right: 8),
                decoration: new BoxDecoration(
                    border: new Border(new BorderSide(color: CColors.Separator, 0.5f)),
                    color: CColors.White
                ),
                child: new Row(
                    crossAxisAlignment: CrossAxisAlignment.center,
                    children: new List<Widget> {
                        new Expanded(
                            child: new GestureDetector(
                                onTap: () => this._sendComment(replyUserId: this.widget.viewModel.author.id),
                                child: new Container(
                                    padding: EdgeInsets.only(16),
                                    height: 32,
                                    decoration: new BoxDecoration(
                                        color: CColors.Separator2,
                                        borderRadius: BorderRadius.all(2)
                                    ),
                                    alignment: Alignment.centerLeft,
                                    child: new Container(
                                        child: new Text(
                                            "友好的评论是交流的起点...",
                                            style: CTextStyle.PKeyboardTextStyle
                                        )
                                    )
                                )
                            )
                        ),
                        //评论
                        new Padding(
                            padding: EdgeInsets.symmetric(0, 16),
                            child: new Text(
                                "发送",
                                style: new TextStyle(
                                    fontSize: 16,
                                    fontFamily: "Roboto-Regular",
                                    color: CColors.Disable2
                                )
                            )
                        )
                    }
                )
            );
        }

        void _sendComment(string parentMessageId = "", string upperMessageId = "", string replyUserId = "") {
            if (!this.widget.viewModel.isLoggedIn) {
                this.pushToLoginPage();
            }
            else if (!UserInfoManager.isRealName()) {
                Navigator.pushNamed(
                    context: this.context,
                    routeName: NavigatorRoutes.RealName
                );
            }
            else {
                var channelId = this.widget.viewModel.channelId;
                var itemId = this.widget.viewModel.itemId;
                var messageType = this.widget.viewModel.messageType;
                var author = this.widget.viewModel.userDict.GetValueOrDefault(key: replyUserId, new User());
                var authorName = author.fullName ?? "";
                if (author.id == this.widget.viewModel.author.id) {
                    authorName = $"作者 {authorName}";
                }

                // AnalyticsManager.ClickComment(
                //     type: type,
                //     channelId: channelId,
                //     title: this._article.title,
                //     commentId: parentMessageId
                // );
                ActionSheetUtils.showModalActionSheet(
                    context: this.context,
                    new CustomInput(
                        replyUserName: authorName,
                        text => {
                            ActionSheetUtils.hiddenModalPopup(context: this.context);
                            this.widget.actionModel.sendComment(
                                parentMessageId.isEmpty() ? messageType : QAMessageType.other,
                                arg2: itemId,
                                arg3: channelId,
                                arg4: text,
                                Snowflake.CreateNonce(),
                                arg6: parentMessageId,
                                arg7: upperMessageId
                            ).then(_ => {
                                CustomDialogUtils.showToast(context: this.context, "评论成功，会在审核通过后展示",
                                    iconData: CIcons.sentiment_satisfied, 2);
                                this.widget.actionModel.fetchQATopLevelComment(arg1: channelId, "");
                            }).catchError(_ => {
                                CustomDialogUtils.showToast(context: this.context, "评论失败",
                                    iconData: CIcons.sentiment_dissatisfied, 2);
                            });
                        })
                );
            }
        }

        Widget _buildCommentList() {
            var comments = this.widget.viewModel.messages;
            var enablePullUp = this.widget.viewModel.hasMore;
            if (this._firstLoading) {
                return new GlobalLoading();
            }

            if (comments.isNullOrEmpty()) {
                return new BlankView(
                    "快来写下你的第一条评论",
                    imageName: BlankImage.comment
                );
            }

            return new Container(
                color: CColors.BgGrey,
                child: new CustomListView(
                    enablePullDown: true,
                    enablePullUp: enablePullUp,
                    controller: this._refreshController,
                    onRefresh: this._onRefresh,
                    itemCount: comments.Count,
                    itemBuilder: this._buildCommentCard,
                    footerWidget: enablePullUp ? null : new EndView()
                )
            );
        }

        void _onRefresh(bool up) {
            var channelId = this.widget.viewModel.channelId;
            this.widget.actionModel.fetchQATopLevelComment(arg1: channelId, arg2: this.widget.viewModel.lastId)
                .then(_ => { this._refreshController.sendBack(up: up, mode: RefreshStatus.completed); })
                .catchError(_ => { this._refreshController.sendBack(up: up, mode: RefreshStatus.failed); });
        }

        Widget _buildHeader() {
            var commentCount = this.widget.viewModel.commentCount;
            return new Container(
                decoration: new BoxDecoration(
                    color: CColors.White
                ),
                child: new Column(
                    children: new List<Widget> {
                        new Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: new List<Widget> {
                                new CustomButton(
                                    padding: EdgeInsets.all(16),
                                    onPressed: () => Navigator.pop(context: this.context),
                                    child: new Icon(
                                        icon: CIcons.close,
                                        size: 24,
                                        color: CColors.Cancel
                                    )
                                ),
                                new Text(
                                    $"全部 {commentCount} 条评论",
                                    style: CTextStyle.PXLargeMedium
                                ),
                                new SizedBox(width: 56)
                            }
                        ),
                        new CustomDivider(color: CColors.Separator2, height: 1)
                    }
                )
            );
        }

        Widget _buildCommentCard(BuildContext buildContext, int index) {
            var comment = this.widget.viewModel.messages[index: index];
            var author = this.widget.viewModel.userDict.GetValueOrDefault(key: comment.authorId, new User());
            var userLicense = CCommonUtils.GetUserLicense(userId: author.id,
                userLicenseMap: this.widget.viewModel.userLicenseDict);
            var likeDict = this.widget.viewModel.likeDict;
            var isPraise = likeDict.ContainsKey(key: comment.id);
            if (comment.childMessages.isNullOrEmpty()) {
                return new QACommentCard(
                    message: comment,
                    author: author,
                    avatarSize: 32,
                    userLicense: userLicense,
                    isPraised: isPraise,
                    separatorWidget: new CustomDivider(color: CColors.Separator2, height: 1),
                    onTap: () => { this._sendComment(parentMessageId: comment.id, replyUserId: author.id); },
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
                            this.pushToLoginPage();
                        }
                    },
                    moreCallBack: () => {
                        if (!UserInfoManager.isLoggedIn()) {
                            this.pushToLoginPage();
                        }
                        else {
                            ActionSheetUtils.showModalActionSheet(
                                context: this.context,
                                new ActionSheet(
                                    items: new List<ActionSheetItem> {
                                        new ActionSheetItem(
                                            "举报",
                                            type: ActionType.normal,
                                            () => Navigator.pushNamed(
                                                context: this.context,
                                                routeName: NavigatorRoutes.Report,
                                                new ReportScreenArguments {
                                                    id = comment.id ?? "",
                                                    reportType = ReportType.comment
                                                }
                                            )
                                        ),
                                        new ActionSheetItem("取消", type: ActionType.cancel)
                                    }
                                ));
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
            }

            var replies = comment.childMessages;
            var repliesClip = new List<NewMessage>();
            repliesClip = replies.Count > 3 ? replies.GetRange(0, 3) : replies;
            var replyWidgets = new List<Widget> {
                new QACommentCard(
                    message: comment,
                    author: author,
                    avatarSize: 32,
                    userLicense: userLicense,
                    isPraised: isPraise,
                    separatorWidget: new Padding(
                        padding: EdgeInsets.only(56),
                        child: new CustomDivider(
                            color: CColors.Separator2,
                            height: 1
                        )
                    ),
                    onTap: () => { this._sendComment(parentMessageId: comment.id, replyUserId: author.id); },
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
                            this.pushToLoginPage();
                        }
                    },
                    moreCallBack: () => {
                        if (!UserInfoManager.isLoggedIn()) {
                            this.pushToLoginPage();
                        }
                        else {
                            ActionSheetUtils.showModalActionSheet(
                                context: this.context,
                                new ActionSheet(
                                    items: new List<ActionSheetItem> {
                                        new ActionSheetItem(
                                            "举报",
                                            type: ActionType.normal,
                                            () => Navigator.pushNamed(
                                                context: this.context,
                                                routeName: NavigatorRoutes.Report,
                                                new ReportScreenArguments {
                                                    id = comment.id ?? "",
                                                    reportType = ReportType.comment
                                                }
                                            )
                                        ),
                                        new ActionSheetItem("取消", type: ActionType.cancel)
                                    }
                                ));
                        }
                    },
                    pushToUserDetail: userId => {
                        Navigator.pushNamed(context: this.context, routeName: NavigatorRoutes.UserDetail,
                            new UserDetailScreenArguments {
                                id = userId
                            }
                        );
                    }
                )
            };

            var channelId = this.widget.viewModel.channelId;
            var messageId = comment.id;

            foreach (var reply in repliesClip) {
                var userDict = this.widget.viewModel.userDict;
                var messageDict = this.widget.viewModel.messageDict;
                var replyAuthor = userDict.GetValueOrDefault(key: reply.authorId, new User());
                var byReplyUser = new User();
                if (reply.upperMessageId.isNotEmpty()) {
                    var upperMessage = messageDict.GetValueOrDefault(key: reply.upperMessageId, new NewMessage());
                    byReplyUser = userDict.GetValueOrDefault(key: upperMessage.authorId, new User());
                }
                else if (reply.parentMessageId.isNotEmpty()) {
                    var parentMessage = messageDict.GetValueOrDefault(key: reply.parentMessageId, new NewMessage());
                    byReplyUser = userDict.GetValueOrDefault(key: parentMessage.authorId, new User());
                }

                var replyIsPraise = likeDict.ContainsKey(key: reply.id);
                var replyWidget = new Padding(
                    padding: EdgeInsets.only(40),
                    child: new QACommentCard(
                        message: reply,
                        author: replyAuthor,
                        byReplyUser: byReplyUser,
                        isPraised: replyIsPraise,
                        contentMaxLine: 3,
                        separatorWidget: new Padding(
                            padding: EdgeInsets.only(48),
                            child: new CustomDivider(
                                color: CColors.Separator2,
                                height: 1
                            )
                        ),
                        onTap: () => {
                            if (channelId.isNotEmpty() && messageId.isNotEmpty()) {
                                Navigator.pushNamed(
                                    context: this.context,
                                    routeName: NavigatorRoutes.QACommentDetail,
                                    new QACommentDetailScreenArguments() { channelId = channelId, messageId = messageId }
                                );
                            }
                        },
                        praiseCallBack: () => {
                            if (this.widget.viewModel.isLoggedIn) {
                                if (replyIsPraise) {
                                    this.widget.actionModel.removeLikeMessage(arg1: reply.channelId, arg2: reply.id);
                                }
                                else {
                                    this.widget.actionModel.likeMessage(arg1: reply.channelId, arg2: reply.id);
                                }
                            }
                            else {
                                this.pushToLoginPage();
                            }
                        },
                        pushToUserDetail: userId => {
                            Navigator.pushNamed(context: this.context, routeName: NavigatorRoutes.UserDetail,
                                new UserDetailScreenArguments {
                                    id = userId
                                }
                            );
                        }
                    )
                );
                replyWidgets.Add(item: replyWidget);
            }

            var showMoreWidget = new GestureDetector(
                onTap: () => {
                    if (channelId.isNotEmpty() && messageId.isNotEmpty()) {
                        Navigator.pushNamed(
                            context: this.context,
                            routeName: NavigatorRoutes.QACommentDetail,
                            new QACommentDetailScreenArguments() { channelId = channelId, messageId = messageId }
                        );
                    }
                },
                child: new Container(
                    color: CColors.White,
                    padding: EdgeInsets.only(88, 12, bottom: 12),
                    child: new Text(
                        $"查看全部 {comment.replyCount} 条评论",
                        style: CTextStyle.PRegularBlue.defaultHeight()
                    )
                )
            );
            replyWidgets.Add(item: showMoreWidget);
            var separatorWidget = new CustomDivider(color: CColors.Separator2, height: 1);
            replyWidgets.Add(item: separatorWidget);

            return new Container(
                color: CColors.White,
                child: new Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: replyWidgets
                )
            );
        }
    }
}