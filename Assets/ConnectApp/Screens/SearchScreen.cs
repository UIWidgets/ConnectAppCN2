using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Components;
using ConnectApp.Models.ActionModel;
using ConnectApp.Models.State;
using ConnectApp.Models.ViewModel;
using ConnectApp.redux.actions;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace ConnectApp.screens {
    public enum SearchType {
        article = 0,
        question,
        user,
        team
    }

    public class SearchScreenConnector : StatelessWidget {
        public SearchScreenConnector(
            SearchType searchType,
            string searchKeyword,
            Key key = null
        ) : base(key: key) {
            this.searchType = searchType;
            this.searchKeyword = searchKeyword;
        }

        readonly SearchType searchType;
        readonly string searchKeyword;

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, SearchScreenViewModel>(
                converter: state => {
                    var keyword = state.searchState.keyword ?? "";
                    var searchState = state.searchState;
                    return new SearchScreenViewModel {
                        searchType = this.searchType,
                        preSearchKeyword = this.searchKeyword,
                        searchKeyword = keyword,
                        searchArticleHistoryList = searchState.searchArticleHistoryList,
                        searchSuggest = state.articleState.searchSuggest,
                        popularSearchArticleList = state.popularSearchState.popularSearchArticles,
                        searchArticleIds =
                            searchState.searchArticleIdDict.GetValueOrDefault(key: keyword, new List<string>()),
                        searchUserIds =
                            searchState.searchUserIdDict.GetValueOrDefault(key: keyword, new List<string>()),
                        searchTeamIds =
                            searchState.searchTeamIdDict.GetValueOrDefault(key: keyword, new List<string>()),
                        searchQuestionIds =
                            searchState.searchQuestionIdsDict.GetValueOrDefault(key: keyword, new List<string>())
                    };
                },
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new SearchScreenActionModel {
                        fetchPopularSearch = () => dispatcher.dispatch<Future>(CActions.popularSearchArticle()),
                        startSearchArticle = keyword => dispatcher.dispatch(
                            new StartSearchArticleAction {
                                keyword = keyword
                            }
                        ),
                        searchArticle = (keyword, pageNumber) => dispatcher.dispatch<Future>(
                            CActions.searchArticles(keyword: keyword, pageNumber: pageNumber)),
                        startSearchUser = keyword => dispatcher.dispatch(new StartSearchUserAction {keyword = keyword}),
                        searchUser = (keyword, pageNumber) => dispatcher.dispatch<Future>(
                            CActions.searchUsers(keyword: keyword, pageNumber: pageNumber)),
                        startSearchTeam = keyword => dispatcher.dispatch(new StartSearchTeamAction {keyword = keyword}),
                        searchTeam = (keyword, pageNumber) => dispatcher.dispatch<Future>(
                            CActions.searchTeams(keyword: keyword, pageNumber: pageNumber)),
                        startSearchQuestion = keyword =>
                            dispatcher.dispatch(new StartSearchQuestionAction {keyword = keyword}),
                        searchQuestion = (keyword, pageNumber) => dispatcher.dispatch<Future>(
                            CActions.searchQuestions(keyword: keyword, pageNumber: pageNumber)),
                        clearSearchResult = () => dispatcher.dispatch(new ClearSearchResultAction()),
                        saveSearchArticleHistory = keyword =>
                            dispatcher.dispatch(new SaveSearchArticleHistoryAction {keyword = keyword}),
                        deleteSearchArticleHistory = keyword =>
                            dispatcher.dispatch(new DeleteSearchArticleHistoryAction {keyword = keyword}),
                        deleteAllSearchArticleHistory = () =>
                            dispatcher.dispatch(new DeleteAllSearchArticleHistoryAction())
                    };
                    return new SearchScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }

    public class SearchScreen : StatefulWidget {
        public SearchScreen(
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
            return new _SearchScreenState();
        }
    }

    class _SearchScreenState : State<SearchScreen>, RouteAware {
        readonly TextEditingController _controller = new TextEditingController("");
        FocusNode _focusNode;
        SearchType _searchType;
        const int defaultPageNumber = 1;

        public override void initState() {
            base.initState();
            StatusBarManager.statusBarStyle(false);
            this._focusNode = new FocusNode();
            this._searchType = this.widget.viewModel.searchType;
            SchedulerBinding.instance.addPostFrameCallback(_ => {
                if (this.widget.viewModel.searchKeyword.Length > 0
                    || this.widget.viewModel.searchArticleIds.Count > 0
                    || this.widget.viewModel.searchUserIds.Count > 0
                    || this.widget.viewModel.searchTeamIds.Count > 0
                    || this.widget.viewModel.searchQuestionIds.Count > 0) {
                    this.widget.actionModel.clearSearchResult();
                }

                if (this.widget.viewModel.preSearchKeyword.isNotEmpty()) {
                    this._searchResult(text: this.widget.viewModel.preSearchKeyword);
                    AnalyticsManager.SearchKeyword(keyword: this.widget.viewModel.preSearchKeyword,
                        from: AnalyticsManager.SearchFrom.tag, type: this._searchType);
                }

                this.widget.actionModel.fetchPopularSearch();
            });
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            Main.ConnectApp.routeObserver.subscribe(this, (PageRoute) ModalRoute.of(context: this.context));
        }

        public override void dispose() {
            this._controller.dispose();
            Main.ConnectApp.routeObserver.unsubscribe(this);
            base.dispose();
        }

        void _searchResult(string text) {
            if (text.isEmpty() && this.widget.viewModel.searchSuggest.isEmpty()) {
                return;
            }

            var searchKey = "";
            if (this.widget.viewModel.searchSuggest.isNotEmpty()) {
                searchKey = this.widget.viewModel.searchSuggest;
            }

            if (text.isNotEmpty()) {
                searchKey = text;
            }

            if (this._focusNode.hasFocus) {
                this._focusNode.unfocus();
            }

            this._controller.text = searchKey;

            this._search(text: searchKey, searchType: this._searchType);
        }

        void _search(string text, SearchType searchType) {
            if (searchType == SearchType.article) {
                this.widget.actionModel.saveSearchArticleHistory(obj: text);
                this.widget.actionModel.startSearchArticle(obj: text);
                this.widget.actionModel.searchArticle(arg1: text, arg2: defaultPageNumber);
            }
            else if (searchType == SearchType.question) {
                this.widget.actionModel.startSearchQuestion(obj: text);
                this.widget.actionModel.searchQuestion(arg1: text, arg2: defaultPageNumber);
            }
            else if (searchType == SearchType.user) {
                this.widget.actionModel.startSearchUser(obj: text);
                this.widget.actionModel.searchUser(arg1: text, arg2: defaultPageNumber);
            }
            else if (searchType == SearchType.team) {
                this.widget.actionModel.startSearchTeam(obj: text);
                this.widget.actionModel.searchTeam(arg1: text, arg2: defaultPageNumber);
            }
        }

        public override Widget build(BuildContext context) {
            Widget child;
            if (this.widget.viewModel.searchKeyword.isNotEmpty()) {
                child = this._buildSearchResult();
            }
            else {
                child = new ListView(
                    children: new List<Widget> {
                        this._buildSearchHistory(),
                        this._buildPopularSearch()
                    }
                );
            }

            return new Container(
                color: CColors.White,
                child: new CustomSafeArea(
                    bottom: false,
                    child: new Container(
                        child: new Column(
                            children: new List<Widget> {
                                this._buildSearchBar(),
                                new Flexible(
                                    child: new NotificationListener<ScrollNotification>(
                                        onNotification: notification => {
                                            if (this._focusNode.hasFocus) {
                                                this._focusNode.unfocus();
                                            }

                                            return true;
                                        },
                                        child: child
                                    )
                                )
                            }
                        )
                    )
                )
            );
        }

        Widget _buildSearchBar() {
            return new Container(
                height: 94,
                padding: EdgeInsets.only(16, 0, 16, 12),
                color: CColors.White,
                child: new Column(
                    mainAxisAlignment: MainAxisAlignment.end,
                    crossAxisAlignment: CrossAxisAlignment.end,
                    children: new List<Widget> {
                        new CustomButton(
                            padding: EdgeInsets.only(8, 8, 0, 8),
                            onPressed: () => Navigator.pop(context: this.context),
                            child: new Text(
                                "取消",
                                style: CTextStyle.PLargeBlue
                            )
                        ),
                        new InputField(
                            height: 40,
                            controller: this._controller,
                            focusNode: this._focusNode,
                            style: CTextStyle.H2,
                            autofocus: this.widget.viewModel.preSearchKeyword.isEmpty(),
                            hintText: this.widget.viewModel.searchSuggest ?? "搜索",
                            hintStyle: CTextStyle.H2Body4,
                            cursorColor: CColors.PrimaryBlue,
                            textInputAction: TextInputAction.search,
                            clearButtonMode: InputFieldClearButtonMode.whileEditing,
                            onChanged: text => {
                                if (text.isNotEmpty()) {
                                    return;
                                }

                                this.widget.actionModel.clearSearchResult();
                            },
                            onSubmitted: keyword => {
                                this._searchResult(text: keyword);
                                if (keyword.isEmpty() && this.widget.viewModel.searchSuggest.isNotEmpty()) {
                                    AnalyticsManager.SearchKeyword(keyword: this.widget.viewModel.searchSuggest,
                                        from: AnalyticsManager.SearchFrom.suggest, type: this._searchType);
                                }
                                else {
                                    AnalyticsManager.SearchKeyword(keyword: keyword,
                                        from: AnalyticsManager.SearchFrom.typing, type: this._searchType);
                                }
                            }
                        )
                    }
                )
            );
        }

        Widget _buildSearchResult() {
            return new CustomSegmentedControl(
                new List<object> {"文章", "问题", "用户", "公司"},
                new List<Widget> {
                    new SearchArticleScreenConnector(),
                    new SearchQuestionScreenConnector(),
                    new SearchUserScreenConnector(),
                    new SearchTeamScreenConnector()
                },
                newValue => {
                    this._searchType = (SearchType) newValue;
                    this.setState(() => { });
                    this._searchResult(text: this.widget.viewModel.searchKeyword);
                    AnalyticsManager.SearchKeyword(keyword: this.widget.viewModel.searchKeyword,
                        from: AnalyticsManager.SearchFrom.switchTab, type: this._searchType);
                },
                (int) this._searchType
            );
        }

        Widget _buildPopularSearch() {
            if (this.widget.viewModel.popularSearchArticleList.Count <= 0) {
                return new Container();
            }

            return new Container(
                padding: EdgeInsets.only(16, 24, 16),
                color: CColors.White,
                child: new Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: new List<Widget> {
                        new Container(
                            margin: EdgeInsets.only(bottom: 16),
                            child: new Text(
                                "热门搜索",
                                style: CTextStyle.PXLargeBody4
                            )
                        ),
                        new Wrap(
                            spacing: 8,
                            runSpacing: 20,
                            children: this._buildPopularSearchItem()
                        )
                    }
                )
            );
        }

        List<Widget> _buildPopularSearchItem() {
            var popularSearch = this.widget.viewModel.popularSearchArticleList;
            var widgets = new List<Widget>();
            popularSearch.ForEach(item => {
                Widget widget = new GestureDetector(
                    onTap: () => {
                        this._searchResult(text: item.keyword);
                        AnalyticsManager.SearchKeyword(keyword: item.keyword, from: AnalyticsManager.SearchFrom.hottest,
                            type: this._searchType);
                    },
                    child: new Container(
                        decoration: new BoxDecoration(
                            color: CColors.Separator2,
                            borderRadius: BorderRadius.circular(16)
                        ),
                        height: 32,
                        padding: EdgeInsets.symmetric(0, 16),
                        child: new Row(
                            mainAxisSize: MainAxisSize.min,
                            children: new List<Widget> {
                                new Text(
                                    data: item.keyword,
                                    maxLines: 1,
                                    style: new TextStyle(
                                        fontSize: 16,
                                        fontFamily: "Roboto-Regular",
                                        color: CColors.TextBody
                                    ),
                                    overflow: TextOverflow.ellipsis
                                )
                            }
                        )
                    )
                );
                widgets.Add(item: widget);
            });
            return widgets;
        }

        Widget _buildSearchHistory() {
            var searchHistoryList = this.widget.viewModel.searchArticleHistoryList;
            if (searchHistoryList == null || searchHistoryList.isNullOrEmpty()) {
                return new Container();
            }

            var widgets = new List<Widget> {
                new Container(
                    margin: EdgeInsets.only(top: 24, bottom: 10),
                    child: new Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: new List<Widget> {
                            new Text(
                                "搜索历史",
                                style: CTextStyle.PXLargeBody4
                            ),
                            new CustomButton(
                                padding: EdgeInsets.only(8, 8, 0, 8),
                                onPressed: () => {
                                    ActionSheetUtils.showModalActionSheet(
                                        context: this.context,
                                        new ActionSheet(
                                            title: "确定清除搜索历史记录？",
                                            items: new List<ActionSheetItem> {
                                                new ActionSheetItem("确定", type: ActionType.destructive,
                                                    () => this.widget.actionModel.deleteAllSearchArticleHistory()),
                                                new ActionSheetItem("取消", type: ActionType.cancel)
                                            }
                                        )
                                    );
                                },
                                child: new Text(
                                    "清空",
                                    style: CTextStyle.PRegularBody4
                                )
                            )
                        }
                    )
                )
            };
            searchHistoryList.ForEach(item => {
                var child = new GestureDetector(
                    onTap: () => {
                        this._searchResult(text: item);
                        AnalyticsManager.SearchKeyword(keyword: item, from: AnalyticsManager.SearchFrom.history,
                            type: this._searchType);
                    },
                    child: new Container(
                        height: 44,
                        color: CColors.White,
                        child: new Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: new List<Widget> {
                                new Expanded(
                                    child: new Text(
                                        data: item,
                                        maxLines: 1,
                                        overflow: TextOverflow.ellipsis,
                                        style: CTextStyle.PLargeBody
                                    )
                                ),
                                new CustomButton(
                                    padding: EdgeInsets.only(8, 8, 0, 8),
                                    onPressed: () => this.widget.actionModel.deleteSearchArticleHistory(obj: item),
                                    child: new Icon(
                                        icon: CIcons.close,
                                        size: 16,
                                        color: Color.fromRGBO(199, 203, 207, 1)
                                    )
                                )
                            }
                        )
                    )
                );
                widgets.Add(item: child);
            });

            return new Container(
                padding: EdgeInsets.symmetric(horizontal: 16),
                color: CColors.White,
                child: new Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: widgets
                )
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