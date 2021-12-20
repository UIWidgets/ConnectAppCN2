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
    public class DraftBoxScreenConnector : StatelessWidget {
        public DraftBoxScreenConnector(
            Key key = null
        ) : base(key: key) {
        }

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, DraftBoxScreenViewModel>(
                converter: state => {
                    // answer
                    var answerDraftsHasMore = state.qaEditorState.userAnswerDraftListHasMore;
                    var answerDraftDict = state.qaEditorState.answerDraftDict;
                    var userAnswerDraftIds = state.qaEditorState.userAnswerDraftIds;
                    var answerDrafts = new List<AnswerDraft>();
                    if (userAnswerDraftIds.isNotNullAndEmpty()) {
                        userAnswerDraftIds.ForEach(questionId => {
                            var answer = answerDraftDict.getOrDefault(key: questionId, null);
                            if (answer != null) {
                                answerDrafts.Add(item: answer);
                            }
                        });
                    }

                    return new DraftBoxScreenViewModel {
                        answerDraftsHasMore = answerDraftsHasMore,
                        answerDrafts = answerDrafts
                    };
                },
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new DraftBoxScreenActionModel {
                        fetchAnswerDrafts =
                            page => dispatcher.dispatch<Future>(CActions.fetchUserAnswerDrafts(page: page)),
                        deleteAnswerDraft = (questionId, answerId) =>
                            dispatcher.dispatch<Future>(CActions.deleteAnswerDraft(questionId: questionId,
                                answerId: answerId))
                    };
                    return new DraftBoxScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }


    public class DraftBoxScreen : StatefulWidget {
        public DraftBoxScreen(
            Key key = null,
            DraftBoxScreenViewModel viewModel = null,
            DraftBoxScreenActionModel actionModel = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly DraftBoxScreenViewModel viewModel;
        public readonly DraftBoxScreenActionModel actionModel;

        public override State createState() {
            return new _DraftBoxScreenState();
        }
    }

    class _DraftBoxScreenState : State<DraftBoxScreen> {
        const int firstPageNumber = 1;
        RefreshController _answerDraftsRefreshController;
        int _answersPageNumber;
        bool _answersLoading;

        public override void initState() {
            base.initState();
            StatusBarManager.statusBarStyle(false);
            this._answerDraftsRefreshController = new RefreshController();
            this._answersPageNumber = firstPageNumber;
            this._answersLoading = true;
            SchedulerBinding.instance.addPostFrameCallback(_ => {
                this.widget.actionModel.fetchAnswerDrafts(arg: firstPageNumber)
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
            });
        }

        public override Widget build(BuildContext context) {
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
                                    child: this._buildContent()
                                )
                            }
                        )
                    )
                )
            );
        }

        Widget _buildContent() {
            return new CustomSegmentedControl(
                new List<object> {"回答"},
                new List<Widget> {
                    this._buildAnswerList()
                },
                unselectedColor: CColors.TextTitle
            );
        }

        Widget _buildAnswerList() {
            var answerDrafts = this.widget.viewModel.answerDrafts;
            var enablePullUp = this.widget.viewModel.answerDraftsHasMore;
            if (answerDrafts.isNullOrEmpty() && this._answersLoading) {
                return ListView.builder(
                    physics: new NeverScrollableScrollPhysics(),
                    itemCount: 6,
                    itemBuilder: (cxt, index) => new QuestionLoadingCard()
                );
            }

            if (answerDrafts.isNullOrEmpty()) {
                return new BlankView(
                    "回答草稿箱是空的",
                    imageName: BlankImage.common
                );
            }

            return new Container(
                color: CColors.BgGrey,
                child: new CustomListView(
                    controller: this._answerDraftsRefreshController,
                    enablePullDown: true,
                    enablePullUp: enablePullUp,
                    onRefresh: this._answerOnRefresh,
                    itemCount: answerDrafts.Count,
                    itemBuilder: this._buildAnswerDraftCard,
                    hasBottomMargin: false,
                    footerWidget: enablePullUp ? null : new EndView(hasBottomMargin: false)
                )
            );
        }

        Widget _buildAnswerDraftCard(BuildContext buildContext, int index) {
            var answerDraft = this.widget.viewModel.answerDrafts[index: index];
            return new GestureDetector(
                onTap: () => Navigator.pushNamed(
                    context: this.context,
                    routeName: NavigatorRoutes.PostAnswer,
                    new PostAnswerScreenArguments {
                        questionId = answerDraft.questionId,
                        answerId = answerDraft.answerId,
                        canSave = true,
                        isModal = false
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
                                    data: answerDraft.questionTitle,
                                    style: CTextStyle.PXLargeMedium.copyWith(height: 1.44f),
                                    maxLines: 3,
                                    overflow: TextOverflow.ellipsis
                                )
                            ),
                            new Padding(
                                padding: EdgeInsets.only(right: 16, bottom: 12),
                                child: new Text(
                                    data: answerDraft.descriptionPlain,
                                    style: CTextStyle.PLargeBody2,
                                    maxLines: 3,
                                    overflow: TextOverflow.ellipsis
                                )
                            ),
                            new Row(
                                mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                children: new List<Widget> {
                                    new Padding(
                                        padding: EdgeInsets.only(bottom: 12),
                                        child: new Text(
                                            $"编辑于{DateConvert.DateStringFromNow(answerDraft.createdTime ?? DateTime.Now)}",
                                            style: CTextStyle.PSmallBody5
                                        )
                                    ),
                                    new CustomButton(
                                        padding: EdgeInsets.only(16, right: 16, bottom: 12),
                                        child: new Row(
                                            children: new List<Widget> {
                                                new Icon(
                                                    icon: CIcons.delete,
                                                    size: 18,
                                                    color: CColors.BrownGrey
                                                ),
                                                new SizedBox(width: 4),
                                                new Text(
                                                    "删除",
                                                    style: CTextStyle.PSmallBody5.defaultHeight()
                                                )
                                            }
                                        ),
                                        onPressed: () => {
                                            ActionSheetUtils.showModalActionSheet(
                                                context: this.context,
                                                new ActionSheet(
                                                    title: "确定删除该草稿吗？",
                                                    items: new List<ActionSheetItem> {
                                                        new ActionSheetItem("确定", type: ActionType.destructive,
                                                            () => {
                                                                if (answerDraft.questionId.isNotEmpty() &&
                                                                    answerDraft.answerId.isNotEmpty()) {
                                                                    CustomDialogUtils.showCustomDialog(
                                                                        context: this.context,
                                                                        child: new CustomLoadingDialog(
                                                                            message: "正在删除"));
                                                                    this.widget.actionModel
                                                                        .deleteAnswerDraft(arg1: answerDraft.questionId,
                                                                            arg2: answerDraft.answerId)
                                                                        .then(_ => {
                                                                            CustomDialogUtils.hiddenCustomDialog(
                                                                                context: this.context);
                                                                            CustomDialogUtils.showToast(
                                                                                context: this.context,
                                                                                "删除成功",
                                                                                iconData: CIcons.sentiment_satisfied);
                                                                        })
                                                                        .catchError(err => {
                                                                            CustomDialogUtils.hiddenCustomDialog(
                                                                                context: this.context);
                                                                            CustomDialogUtils.showToast(
                                                                                context: this.context,
                                                                                "删除失败",
                                                                                iconData: CIcons.sentiment_dissatisfied);
                                                                        });
                                                                }
                                                            }),
                                                        new ActionSheetItem("取消", type: ActionType.cancel)
                                                    }
                                                ));
                                        }
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
            this.widget.actionModel.fetchAnswerDrafts(arg: this._answersPageNumber)
                .then(_ => this._answerDraftsRefreshController.sendBack(up: up,
                    up ? RefreshStatus.completed : RefreshStatus.idle))
                .catchError(_ => this._answerDraftsRefreshController.sendBack(up: up, mode: RefreshStatus.failed));
        }

        Widget _buildNavigationBar() {
            return new CustomNavigationBar(
                new Text(
                    $"草稿箱",
                    style: CTextStyle.H2.defaultHeight()
                ),
                onBack: () => Navigator.pop(context: this.context)
            );
        }
    }
}