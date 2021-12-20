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
using Unity.UIWidgets.widgets;

namespace ConnectApp.screens {
    public class SearchQuestionScreenConnector : StatelessWidget {
        public SearchQuestionScreenConnector(
            Key key = null
        ) : base(key: key) {
        }

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, SearchScreenViewModel>(
                converter: state => {
                    var searchState = state.searchState;
                    var keyword = searchState.keyword;
                    var searchQuestionIds =
                        searchState.searchQuestionIdsDict.GetValueOrDefault(key: keyword, new List<string>());
                    var blockQuestionIds = state.qaState.blockQuestionList;
                    var searchQuestions = new List<Question>();
                    searchQuestionIds.ForEach(id => {
                        if (id.isNotEmpty() && blockQuestionIds.Contains(item: id)) {
                            return;
                        }

                        searchQuestions.Add(state.qaState.questionDict[key: id]);
                    });
                    return new SearchScreenViewModel {
                        searchQuestionLoading = state.searchState.searchQuestionLoading,
                        searchKeyword = keyword,
                        searchQuestionHasMore = state.searchState.searchQuestionHasMore,
                        searchQuestions = searchQuestions,
                        isLoggedIn = state.loginState.isLoggedIn
                    };
                },
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new SearchScreenActionModel {
                        searchQuestion = (keyword, pageNumber) => dispatcher.dispatch<Future>(
                            CActions.searchQuestions(keyword: keyword, pageNumber: pageNumber))
                    };
                    return new SearchQuestionScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }

    public class SearchQuestionScreen : StatefulWidget {
        public SearchQuestionScreen(
            SearchScreenViewModel viewModel = null,
            SearchScreenActionModel actionModel = null,
            Key key = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly SearchScreenViewModel viewModel;
        public readonly SearchScreenActionModel actionModel;

        public override State createState() {
            return new _SearchQuestionScreenState();
        }
    }

    class _SearchQuestionScreenState : State<SearchQuestionScreen> {
        const int _initPageNumber = 1;
        int _pageNumber = _initPageNumber;
        RefreshController _refreshController;

        public override void initState() {
            base.initState();
            this._refreshController = new RefreshController();
        }

        void _onRefresh(bool up) {
            if (up) {
                this._pageNumber = _initPageNumber;
            }
            else {
                this._pageNumber++;
            }

            this.widget.actionModel.searchQuestion(arg1: this.widget.viewModel.searchKeyword, arg2: this._pageNumber)
                .then(_ => this._refreshController.sendBack(up: up, up ? RefreshStatus.completed : RefreshStatus.idle))
                .catchError(_ => this._refreshController.sendBack(up: up, mode: RefreshStatus.failed));
        }

        public override Widget build(BuildContext context) {
            var searchQuestions = this.widget.viewModel.searchQuestions;
            var searchKeyword = this.widget.viewModel.searchKeyword ?? "";
            Widget child = new Container();
            if (this.widget.viewModel.searchQuestionLoading && searchQuestions.isNullOrEmpty()) {
                child = new GlobalLoading();
            }
            else if (searchKeyword.Length > 0) {
                child = searchQuestions.isNotNullAndEmpty()
                    ? this._buildContent()
                    : new BlankView(
                        "哎呀，换个关键词试试吧",
                        imageName: BlankImage.search
                    );
            }

            return new Container(
                color: CColors.White,
                child: child
            );
        }

        Widget _buildContent() {
            var searchQuestions = this.widget.viewModel.searchQuestions;
            var enablePullUp = this.widget.viewModel.searchQuestionHasMore;
            return new Container(
                color: CColors.Background,
                child: new CustomListView(
                    controller: this._refreshController,
                    enablePullDown: false,
                    enablePullUp: enablePullUp,
                    onRefresh: this._onRefresh,
                    itemCount: searchQuestions.Count,
                    itemBuilder: this._buildQuestionCard,
                    headerWidget: CustomListViewConstant.defaultHeaderWidget,
                    footerWidget: enablePullUp ? null : CustomListViewConstant.defaultFooterWidget
                )
            );
        }

        Widget _buildQuestionCard(BuildContext buildContext, int index) {
            var question = this.widget.viewModel.searchQuestions[index: index];
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
                    padding: EdgeInsets.all(16),
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
                            new Container(
                                margin: EdgeInsets.only(right: 16, bottom: 12),
                                child: new Text(
                                    data: question.title,
                                    style: CTextStyle.PXLargeMedium.copyWith(height: 1.44f)
                                )
                            ),
                            new Row(
                                children: new List<Widget> {
                                    new Container(
                                        margin: EdgeInsets.only(bottom: 12),
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
    }
}