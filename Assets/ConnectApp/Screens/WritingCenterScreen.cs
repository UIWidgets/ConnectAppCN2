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
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.widgets;

namespace ConnectApp.screens {
    public class WritingCenterScreenConnector : StatelessWidget {
        public WritingCenterScreenConnector(
            string userId,
            Key key = null
        ) : base(key: key) {
            this.userId = userId;
        }

        readonly string userId;

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, WritingCenterScreenViewModel>(
                converter: state => {
                    // question
                    var userQuestionListHasMore = state.qaState.userQuestionListHasMore;
                    var questionDict = state.qaState.questionDict;
                    var userQuestionIds =
                        state.qaState.userQuestionsDict.GetValueOrDefault(key: this.userId, new List<string>());
                    var questions = new List<Question>();
                    if (userQuestionIds.isNotNullAndEmpty()) {
                        userQuestionIds.ForEach(questionId => {
                            var question = questionDict.getOrDefault(key: questionId, null);
                            if (question != null) {
                                questions.Add(item: question);
                            }
                        });
                    }

                    // answer
                    var userAnswerListHasMore = state.qaState.userAnswerListHasMore;
                    var answerDict = state.qaState.answerDict;
                    var userAnswerIds =
                        state.qaState.userAnswersDict.GetValueOrDefault(key: this.userId, new List<string>());
                    var answers = new List<Answer>();
                    if (userAnswerIds.isNotNullAndEmpty()) {
                        userAnswerIds.ForEach(answerId => {
                            var answer = answerDict.getOrDefault(key: answerId, null);
                            if (answer != null) {
                                answers.Add(item: answer);
                            }
                        });
                    }

                    var currentUserId = state.loginState.loginInfo.userId ?? "";
                    var isMe = currentUserId.isNotEmpty() && this.userId.isNotEmpty() &&
                               currentUserId.Equals(value: this.userId);

                    var userDict = state.userState.userDict;
                    var currentUser = userDict.getOrDefault(key: currentUserId, null);

                    return new WritingCenterScreenViewModel {
                        isMe = isMe,
                        currentUser = currentUser,
                        questionListHasMore = userQuestionListHasMore,
                        answerListHasMore = userAnswerListHasMore,
                        questions = questions,
                        answers = answers,
                        questionDict = questionDict
                    };
                },
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new WritingCenterScreenActionModel {
                        fetchUserQuestions = page =>
                            dispatcher.dispatch<Future>(CActions.fetchUserQuestions(userId: this.userId, page: page)),
                        fetchUserAnswers =
                            page => dispatcher.dispatch<Future>(CActions.fetchUserAnswers(userId: this.userId,
                                page: page))
                    };
                    return new WritingCenterScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }


    public class WritingCenterScreen : StatefulWidget {
        public WritingCenterScreen(
            Key key = null,
            WritingCenterScreenViewModel viewModel = null,
            WritingCenterScreenActionModel actionModel = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly WritingCenterScreenViewModel viewModel;
        public readonly WritingCenterScreenActionModel actionModel;

        public override State createState() {
            return new _WritingCenterScreenState();
        }
    }

    class _WritingCenterScreenState : AutomaticKeepAliveClientMixin<WritingCenterScreen>, TickerProvider {
        const int firstPageNumber = 1;
        RefreshController _questionsRefreshController;
        RefreshController _answersRefreshController;
        int _questionsPageNumber;
        int _answersPageNumber;
        bool _questionsLoading;
        bool _answersLoading;
        bool animationIsReady;

        protected override bool wantKeepAlive {
            get { return true; }
        }

        public override void initState() {
            base.initState();
            StatusBarManager.statusBarStyle(false);
            this._questionsRefreshController = new RefreshController();
            this._answersRefreshController = new RefreshController();
            this._questionsPageNumber = firstPageNumber;
            this._answersPageNumber = firstPageNumber;
            this._questionsLoading = true;
            this._answersLoading = true;
            SchedulerBinding.instance.addPostFrameCallback(_ => {
                var modalRoute = ModalRoute.of(context: this.context);
                modalRoute.animation.addStatusListener(listener: this._animationStatusListener);
            });
        }
        
        void _animationStatusListener(AnimationStatus status) {
            if(status == AnimationStatus.completed) {
                this.animationIsReady = true;
                this.setState(() => {});
                this.widget.actionModel.fetchUserQuestions(arg: firstPageNumber)
                    .then(
                        v => {
                            this._questionsLoading = false;
                            this.setState(() => { });
                        }
                    )
                    .catchError(
                        err => {
                            this._questionsLoading = false;
                            this.setState(() => { });
                        }
                    );
                this.widget.actionModel.fetchUserAnswers(arg: firstPageNumber)
                    .then(
                        v => {
                            this._answersLoading = false;
                            this.setState(() => { });
                        }
                    )
                    .catchError(
                        err => {
                            this._answersLoading = false;
                            this.setState(() => { });
                        }
                    );
            }
        }

        public override Widget build(BuildContext context) {
            Widget contentWidget = new GlobalLoading();
            if (this.animationIsReady) {
                contentWidget = this._buildContent();
            }
            return new Container(
                color: CColors.White,
                child: new CustomSafeArea(
                    bottom: false,
                    child: new Container(
                        color: CColors.Background,
                        child: new Column(
                            children: new List<Widget> {
                                this._buildNavigationBar(),
                                new Expanded(
                                    child: contentWidget
                                )
                            }
                        )
                    )
                )
            );
        }

        Widget _buildContent() {
            return new CustomSegmentedControl(
                new List<object> {"提问", "回答"},
                new List<Widget> {
                    this._buildQuestionList(),
                    this._buildAnswerList()
                }
            );
        }

        Widget _buildQuestionList() {
            var questions = this.widget.viewModel.questions;
            var enablePullUp = this.widget.viewModel.questionListHasMore;
            if (questions.isNullOrEmpty() && this._questionsLoading) {
                return ListView.builder(
                    physics: new NeverScrollableScrollPhysics(),
                    itemCount: 6,
                    itemBuilder: (cxt, index) => new QuestionLoadingCard()
                );
            }

            if (questions.isNullOrEmpty()) {
                return new BlankView(
                    "还没有任何提问",
                    imageName: BlankImage.common
                );
            }

            return new Container(
                color: CColors.BgGrey,
                child: new CustomListView(
                    controller: this._questionsRefreshController,
                    enablePullDown: true,
                    enablePullUp: enablePullUp,
                    onRefresh: this._questionOnRefresh,
                    itemCount: questions.Count,
                    itemBuilder: this._buildQuestionCard,
                    hasBottomMargin: false,
                    footerWidget: enablePullUp ? null : new EndView(hasBottomMargin: false)
                )
            );
        }

        Widget _buildQuestionCard(BuildContext buildContext, int index) {
            var question = this.widget.viewModel.questions[index: index];
            var statusTag = new Container();
            var rejectReasonWidget = new Container();
            if (question.status.Equals(QuestionStatus.verifying.ToString())) {
                statusTag = new Container(
                    decoration: new BoxDecoration(
                        color: CColors.BgGrey,
                        borderRadius: BorderRadius.circular(4)
                    ),
                    padding: EdgeInsets.all(4),
                    child: new Text(
                        "审核中",
                        style: new TextStyle(
                            fontSize: 12,
                            fontFamily: "Roboto-Medium",
                            color: CColors.VerifyingStatus
                        )
                    )
                );
            }
            else if (question.status.Equals(QuestionStatus.rejected.ToString())) {
                statusTag = new Container(
                    decoration: new BoxDecoration(
                        color: CColors.BgGrey,
                        borderRadius: BorderRadius.circular(4)
                    ),
                    padding: EdgeInsets.all(4),
                    child: new Text(
                        "已拒绝",
                        style: new TextStyle(
                            fontSize: 12,
                            fontFamily: "Roboto-Medium",
                            color: CColors.RejectedStatus
                        )
                    )
                );
                rejectReasonWidget = new Container(
                    padding: EdgeInsets.only(top: 12),
                    child: new Text(
                        $"拒绝理由: {question.verifiedReason}",
                        style: new TextStyle(
                            fontSize: 14,
                            fontFamily: "Roboto-Medium",
                            color: CColors.RejectedStatus
                        )
                    )
                );
            }
            else if (question.status.Equals(QuestionStatus.resolved.ToString())) {
                statusTag = new Container(
                    decoration: new BoxDecoration(
                        color: CColors.BgGrey,
                        borderRadius: BorderRadius.circular(4)
                    ),
                    padding: EdgeInsets.all(4),
                    child: new Text(
                        "已解决",
                        style: new TextStyle(
                            fontSize: 12,
                            fontFamily: "Roboto-Medium",
                            color: CColors.ResolvedStatus
                        )
                    )
                );
            }

            return new GestureDetector(
                onTap: () => {
                    if (question.status.Equals(QuestionStatus.verifying.ToString())) {
                        CustomDialogUtils.showToast(context: this.context, "请耐心等待审核结果",
                            iconData: CIcons.sentiment_satisfied);
                    }
                    else if (question.status.Equals(QuestionStatus.rejected.ToString())) {
                        Navigator.pushNamed(
                            context: this.context,
                            routeName: NavigatorRoutes.PostQuestion,
                            new PostQuestionScreenArguments {id = question.id, isModal = false}
                        );
                    }
                    else if (question.status.Equals(QuestionStatus.published.ToString()) ||
                             question.status.Equals(QuestionStatus.resolved.ToString())) {
                        Navigator.pushNamed(
                            context: this.context,
                            routeName: NavigatorRoutes.QuestionDetail,
                            new ScreenArguments {id = question.id}
                        );
                    }
                },
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
                                child: new Row(
                                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                    children: new List<Widget> {
                                        new Expanded(
                                            child: new Text(
                                                data: question.title,
                                                style: CTextStyle.PXLargeMedium.copyWith(height: 1.44f),
                                                maxLines: 3,
                                                overflow: TextOverflow.ellipsis
                                            )
                                        ),
                                        statusTag
                                    }
                                )
                            ),
                            new Padding(
                                padding: EdgeInsets.only(right: 16, bottom: 12),
                                child: new Column(
                                    crossAxisAlignment: CrossAxisAlignment.start,
                                    children: new List<Widget> {
                                        new Text(
                                            data: question.descriptionPlain,
                                            style: CTextStyle.PLargeBody2,
                                            maxLines: 3,
                                            overflow: TextOverflow.ellipsis
                                        ),
                                        rejectReasonWidget
                                    }
                                )
                            ),
                            new Row(
                                mainAxisAlignment: MainAxisAlignment.start,
                                children: new List<Widget> {
                                    new Padding(
                                        padding: EdgeInsets.only(bottom: 12),
                                        child: new Text(
                                            $"{CStringUtils.CountToString(count: question.likeCount, "0")} 投票" +
                                            $" · {CStringUtils.CountToString(count: question.answerCount, "0")} 回答 " +
                                            $"· {CStringUtils.CountToString(count: question.viewCount, "0")} 浏览",
                                            style: CTextStyle.PSmallBody5
                                        )
                                    )
                                }
                            )
                        }
                    )
                )
            );
        }

        void _questionOnRefresh(bool up) {
            this._questionsPageNumber = up ? firstPageNumber : this._questionsPageNumber + 1;
            this.widget.actionModel.fetchUserQuestions(arg: this._questionsPageNumber)
                .then(_ => this._questionsRefreshController.sendBack(up: up,
                    up ? RefreshStatus.completed : RefreshStatus.idle))
                .catchError(_ => this._questionsRefreshController.sendBack(up: up, mode: RefreshStatus.failed));
        }

        Widget _buildAnswerList() {
            var answers = this.widget.viewModel.answers;
            var enablePullUp = this.widget.viewModel.answerListHasMore;
            if (answers.isNullOrEmpty() && this._answersLoading) {
                return ListView.builder(
                    physics: new NeverScrollableScrollPhysics(),
                    itemCount: 6,
                    itemBuilder: (cxt, index) => new QuestionLoadingCard()
                );
            }

            if (answers.isNullOrEmpty()) {
                return new BlankView(
                    "还没有任何回答",
                    imageName: BlankImage.common
                );
            }

            return new Container(
                color: CColors.BgGrey,
                child: new CustomListView(
                    controller: this._answersRefreshController,
                    enablePullDown: true,
                    enablePullUp: enablePullUp,
                    onRefresh: this._answerOnRefresh,
                    itemCount: answers.Count,
                    itemBuilder: this._buildAnswerCard,
                    hasBottomMargin: false,
                    footerWidget: enablePullUp ? null : new EndView(hasBottomMargin: false)
                )
            );
        }

        Widget _buildAnswerCard(BuildContext buildContext, int index) {
            var answer = this.widget.viewModel.answers[index: index];
            var question =
                this.widget.viewModel.questionDict.GetValueOrDefault(key: answer.questionId, new Question {title = ""});
            var statusTag = new Container();
            var rejectReasonWidget = new Container();
            if (answer.status.Equals(AnswerStatus.verifying.ToString())) {
                statusTag = new Container(
                    decoration: new BoxDecoration(
                        color: CColors.BgGrey,
                        borderRadius: BorderRadius.circular(4)
                    ),
                    padding: EdgeInsets.all(4),
                    child: new Text(
                        "审核中",
                        style: new TextStyle(
                            fontSize: 12,
                            fontFamily: "Roboto-Medium",
                            color: CColors.VerifyingStatus
                        )
                    )
                );
            }
            else if (answer.status.Equals(AnswerStatus.rejected.ToString())) {
                statusTag = new Container(
                    decoration: new BoxDecoration(
                        color: CColors.BgGrey,
                        borderRadius: BorderRadius.circular(4)
                    ),
                    padding: EdgeInsets.all(4),
                    child: new Text(
                        "已拒绝",
                        style: new TextStyle(
                            fontSize: 12,
                            fontFamily: "Roboto-Medium",
                            color: CColors.RejectedStatus
                        )
                    )
                );
                rejectReasonWidget = new Container(
                    padding: EdgeInsets.only(top: 12),
                    child: new Text(
                        $"拒绝理由: {answer.verifiedReason}",
                        style: new TextStyle(
                            fontSize: 14,
                            fontFamily: "Roboto-Medium",
                            color: CColors.RejectedStatus
                        )
                    )
                );
            }

            return new GestureDetector(
                onTap: () => {
                    if (answer.status.Equals(AnswerStatus.verifying.ToString())) {
                        CustomDialogUtils.showToast(context: this.context, "请耐心等待审核结果",
                            iconData: CIcons.sentiment_satisfied);
                    }
                    else if (answer.status.Equals(AnswerStatus.rejected.ToString())) {
                        Navigator.pushNamed(
                            context: this.context,
                            routeName: NavigatorRoutes.PostAnswer,
                            new PostAnswerScreenArguments {
                                questionId = answer.questionId, 
                                answerId = answer.id,
                                canSave = false,
                                isModal = false
                            }
                        );
                    }
                    else if (answer.status.Equals(AnswerStatus.published.ToString())) {
                        Navigator.pushNamed(
                            context: this.context,
                            routeName: NavigatorRoutes.AnswerDetail,
                            new AnswerDetailScreenArguments {questionId = answer.questionId, answerId = answer.id}
                        );
                    }
                },
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
                                child: new Row(
                                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                    children: new List<Widget> {
                                        new Expanded(
                                            child: new Text(
                                                data: question.title,
                                                style: CTextStyle.PXLargeMedium.copyWith(height: 1.44f),
                                                maxLines: 3,
                                                overflow: TextOverflow.ellipsis
                                            )
                                        ),
                                        statusTag
                                    }
                                )
                            ),
                            new Padding(
                                padding: EdgeInsets.only(right: 16, bottom: 12),
                                child: new Column(
                                    crossAxisAlignment: CrossAxisAlignment.start,
                                    children: new List<Widget> {
                                        new Text(
                                            data: answer.descriptionPlain,
                                            style: CTextStyle.PLargeBody2,
                                            maxLines: 3,
                                            overflow: TextOverflow.ellipsis
                                        ),
                                        rejectReasonWidget
                                    }
                                )
                            ),
                            new Row(
                                mainAxisAlignment: MainAxisAlignment.start,
                                children: new List<Widget> {
                                    new Padding(
                                        padding: EdgeInsets.only(bottom: 12),
                                        child: new Text(
                                            $"赞成 {CStringUtils.CountToString(count: answer.likeCount, "0")}" +
                                            $" · 评论 {CStringUtils.CountToString(count: answer.commentCount, "0")} " +
                                            $" · 浏览 {CStringUtils.CountToString(count: answer.viewCount, "0")}" +
                                            $" · {DateConvert.DateStringFromNow(answer.createdTime ?? DateTime.Now)}",
                                            style: CTextStyle.PSmallBody5
                                        )
                                    )
                                }
                            )
                        }
                    )
                )
            );
        }

        void _answerOnRefresh(bool up) {
            this._answersPageNumber = up ? firstPageNumber : this._answersPageNumber + 1;
            this.widget.actionModel.fetchUserAnswers(arg: this._answersPageNumber)
                .then(_ => this._answersRefreshController.sendBack(up: up,
                    up ? RefreshStatus.completed : RefreshStatus.idle))
                .catchError(_ => this._answersRefreshController.sendBack(up: up, mode: RefreshStatus.failed));
        }

        Widget _buildNavigationBar() {
            var user = this.widget.viewModel.currentUser;

            return new CustomNavigationBar(
                new Text(
                    "创作中心",
                    style: CTextStyle.H2.defaultHeight()
                ),
                topRightWidget: this.widget.viewModel.isMe
                    ? (Widget) new CustomButton(
                        padding: EdgeInsets.symmetric(8, 16),
                        onPressed: () => Navigator.pushNamed(
                            context: this.context,
                            routeName: NavigatorRoutes.DraftBox
                        ),
                        child: new Text(
                            "草稿箱",
                            style: CTextStyle.PLargeBlue.defaultHeight()
                        )
                    )
                    : new Container(),
                onBack: () => Navigator.pop(context: this.context)
            );
        }

        public Ticker createTicker(TickerCallback onTick) {
            return new Ticker(onTick: onTick, $"created by {this}");
        }
    }
}