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
    public class QACommentDetailScreenConnector : StatelessWidget {
        public QACommentDetailScreenConnector(
            string channelId,
            string messageId,
            Key key = null
        ) : base(key: key) {
            this.channelId = channelId;
            this.messageId = messageId;
        }

        string channelId;
        string messageId;

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, QACommentDetailScreenViewModel>(
                converter: state => {
                    var message = state.qaState.messageDict.GetValueOrDefault(
                        key: this.messageId,
                        new NewMessage()
                    );
                    var messageDict = state.qaState.messageDict;
                    var secondLevelCommentList = state.qaState.messageSecondLevelListDict.GetValueOrDefault(
                        this.messageId ?? "",
                        new NewMessageList()
                    );
                    var secondLevelCommentIds = new List<string>();
                    if (secondLevelCommentList.list.isNotNullAndEmpty()) {
                        secondLevelCommentIds = secondLevelCommentList.list;
                    }

                    var hasMore = secondLevelCommentList.hasMore;
                    var lastId = secondLevelCommentList.lastId;
                    var secondLevelComments = new List<NewMessage>();
                    foreach (var secondLevelCommentId in secondLevelCommentIds) {
                        if (messageDict.ContainsKey(key: secondLevelCommentId)) {
                            var msg = messageDict[key: secondLevelCommentId];
                            secondLevelComments.Add(item: msg);
                        }
                    }

                    var topLevelMessage =
                        state.qaState.messageDict.GetValueOrDefault(key: this.messageId, new NewMessage());
                    return new QACommentDetailScreenViewModel {
                        channelId = this.channelId,
                        messageId = this.messageId,
                        message = message,
                        topLevelMessage = topLevelMessage,
                        messages = secondLevelComments,
                        messageDict = messageDict,
                        userDict = state.userState.userDict,
                        userLicenseDict = state.userState.userLicenseDict,
                        hasMore = hasMore,
                        lastId = lastId,
                        isLoggedIn = state.loginState.isLoggedIn,
                        likeDict = state.qaState.likeDict
                    };
                },
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new QACommentDetailScreenActionModel {
                        fetchQACommentDetail = (channelId, messageId, after) =>
                            dispatcher.dispatch<Future>(CActions.fetchQACommentDetail(channelId: channelId,
                                messageId: messageId, after: after)),
                        sendComment = (channelId, content, nonce, parentMessageId, upperMessageId) => {
                            AnalyticsManager.ClickPublishComment("QA", channelId: channelId, commentId: parentMessageId);
                            // CustomDialogUtils.showCustomDialog(child: new CustomLoadingDialog());
                            return dispatcher.dispatch<Future>(
                                CActions.sendQAMessage(messageType: QAMessageType.other, "",
                                    channelId: channelId, content: content,
                                    nonce: nonce, parentMessageId: parentMessageId, upperMessageId: upperMessageId));
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
                    return new QACommentDetailScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }


    public class QACommentDetailScreen : StatefulWidget {
        public QACommentDetailScreen(
            Key key = null,
            QACommentDetailScreenViewModel viewModel = null,
            QACommentDetailScreenActionModel actionModel = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly QACommentDetailScreenViewModel viewModel;
        public readonly QACommentDetailScreenActionModel actionModel;

        public override State createState() {
            return new _QACommentDetailScreenState();
        }
    }

    class _QACommentDetailScreenState : State<QACommentDetailScreen>, RouteAware {
        RefreshController _refreshController;
        bool _firstLoading;

        public override void initState() {
            base.initState();
            this._refreshController = new RefreshController();
            this._firstLoading = true;
            var channelId = this.widget.viewModel.channelId;
            var messageId = this.widget.viewModel.messageId;
            SchedulerBinding.instance.addPostFrameCallback(_ => {
                this.widget.actionModel.fetchQACommentDetail(arg1: channelId, arg2: messageId, "")
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
            var message = this.widget.viewModel.message;
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
                                onTap: () => {
                                    this._sendComment(parentMessageId: message.id, replyUserId: message.authorId);
                                },
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
                var messageId = this.widget.viewModel.messageId;
                var author = this.widget.viewModel.userDict.GetValueOrDefault(key: replyUserId, new User());
                var authorName = author.fullName ?? "";
                ActionSheetUtils.showModalActionSheet(
                    context: this.context,
                    new CustomInput(
                        replyUserName: authorName,
                        text => {
                            ActionSheetUtils.hiddenModalPopup(context: this.context);
                            this.widget.actionModel.sendComment(
                                arg1: channelId,
                                arg2: text,
                                Snowflake.CreateNonce(),
                                arg4: parentMessageId,
                                arg5: upperMessageId
                            ).then(_ => {
                                CustomDialogUtils.showToast(context: this.context, "评论成功，会在审核通过后展示",
                                    iconData: CIcons.sentiment_satisfied, 2);
                                this.widget.actionModel.fetchQACommentDetail(arg1: channelId, arg2: messageId, "");
                            }).catchError(_ => {
                                CustomDialogUtils.showToast(context: this.context, "评论失败",
                                    iconData: CIcons.sentiment_dissatisfied, 2);
                            });
                        })
                );
            }
        }

        Widget _buildCommentList() {
            if (this._firstLoading) {
                return new GlobalLoading();
            }

            var messages = this.widget.viewModel.messages;
            var enablePullUp = this.widget.viewModel.hasMore;
            return new Container(
                color: CColors.BgGrey,
                child: new CustomListView(
                    enablePullDown: true,
                    enablePullUp: enablePullUp,
                    controller: this._refreshController,
                    onRefresh: this._onRefresh,
                    itemCount: messages.Count,
                    itemBuilder: this._buildCommentCard,
                    headerWidget: this._buildListHeader(),
                    footerWidget: enablePullUp ? null : new EndView()
                )
            );
        }

        void _onRefresh(bool up) {
            var channelId = this.widget.viewModel.channelId;
            var messageId = this.widget.viewModel.messageId;
            var lastId = this.widget.viewModel.lastId;
            this.widget.actionModel.fetchQACommentDetail(arg1: channelId, arg2: messageId, arg3: lastId)
                .then(_ => { this._refreshController.sendBack(up: up, mode: RefreshStatus.completed); })
                .catchError(_ => { this._refreshController.sendBack(up: up, mode: RefreshStatus.failed); });
        }

        Widget _buildListHeader() {
            var message = this.widget.viewModel.topLevelMessage;
            var author = this.widget.viewModel.userDict.GetValueOrDefault(key: message.authorId, new User());
            var userLicense = CCommonUtils.GetUserLicense(userId: author.id,
                userLicenseMap: this.widget.viewModel.userLicenseDict);
            var commentCount = this.widget.viewModel.message.replyCount;
            var likeDict = this.widget.viewModel.likeDict;
            var isPraise = likeDict.ContainsKey(key: message.id);
            return new Column(
                children: new List<Widget> {
                    new QACommentCard(
                        message: message,
                        author: author,
                        avatarSize: 32,
                        userLicense: userLicense,
                        isPraised: isPraise,
                        onTap: () => { this._sendComment(parentMessageId: message.id, replyUserId: author.id); },
                        praiseCallBack: () => {
                            if (this.widget.viewModel.isLoggedIn) {
                                if (isPraise) {
                                    this.widget.actionModel.removeLikeMessage(arg1: message.channelId,
                                        arg2: message.id);
                                }
                                else {
                                    this.widget.actionModel.likeMessage(arg1: message.channelId, arg2: message.id);
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
                                                        id = message.id ?? "",
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
                    ),
                    new CustomDivider(color: CColors.BgGrey, height: 8),
                    new Container(
                        color: CColors.White,
                        height: 44,
                        alignment: Alignment.centerLeft,
                        padding: EdgeInsets.only(16),
                        child: new Text(
                            $"{commentCount} 条回复",
                            style: new TextStyle(
                                fontSize: 16,
                                fontFamily: "Roboto-Medium",
                                color: CColors.TextBody
                            )
                        )
                    ),
                    new CustomDivider(color: CColors.Separator2, height: 1)
                }
            );
        }

        Widget _buildHeader() {
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
                                        icon: CIcons.arrow_back,
                                        size: 24,
                                        color: CColors.Cancel
                                    )
                                ),
                                new Text(
                                    "评论详情",
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
            var replies = this.widget.viewModel.messages;
            if (replies.isNullOrEmpty()) {
                return new Container();
            }

            var messageId = this.widget.viewModel.messageId;

            var reply = replies[index: index];
            var userDict = this.widget.viewModel.userDict;
            var messageDict = this.widget.viewModel.messageDict;
            var likeDict = this.widget.viewModel.likeDict;
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

            var isPraise = likeDict.ContainsKey(key: reply.id);

            return new QACommentCard(
                message: reply,
                author: replyAuthor,
                avatarSize: 32,
                byReplyUser: byReplyUser,
                isPraised: isPraise,
                separatorWidget: new Padding(
                    padding: EdgeInsets.only(56),
                    child: new CustomDivider(
                        color: CColors.Separator2,
                        height: 1
                    )
                ),
                onTap: () => {
                    this._sendComment(parentMessageId: messageId, reply.parentMessageId.isNotEmpty() ? reply.id : "",
                        replyUserId: replyAuthor.id);
                },
                praiseCallBack: () => {
                    if (this.widget.viewModel.isLoggedIn) {
                        if (isPraise) {
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
                                                id = reply.id ?? "",
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