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
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.widgets;

namespace ConnectApp.screens {
    public class SearchTeamScreenConnector : StatelessWidget {
        public SearchTeamScreenConnector(
            Key key = null
        ) : base(key: key) {
        }

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, SearchScreenViewModel>(
                converter: state => {
                    var searchTeamIds = state.searchState.searchTeamIdDict.ContainsKey(key: state.searchState.keyword)
                        ? state.searchState.searchTeamIdDict[key: state.searchState.keyword]
                        : null;
                    var currentUserId = state.loginState.loginInfo.userId ?? "";
                    var followMap = state.followState.followDict.ContainsKey(key: currentUserId)
                        ? state.followState.followDict[key: currentUserId]
                        : new Dictionary<string, bool>();
                    return new SearchScreenViewModel {
                        searchTeamLoading = state.searchState.searchTeamLoading,
                        searchKeyword = state.searchState.keyword,
                        searchTeamIds = searchTeamIds,
                        searchTeamHasMore = state.searchState.searchTeamHasMore,
                        teamDict = state.teamState.teamDict,
                        followMap = followMap,
                        currentUserId = currentUserId,
                        isLoggedIn = state.loginState.isLoggedIn
                    };
                },
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new SearchScreenActionModel {
                        searchTeam = (keyword, pageNumber) => dispatcher.dispatch<Future>(
                            CActions.searchTeams(keyword: keyword, pageNumber: pageNumber)),
                        startFollowTeam = followTeamId => dispatcher.dispatch(new StartFetchFollowTeamAction {
                            followTeamId = followTeamId
                        }),
                        followTeam = followTeamId =>
                            dispatcher.dispatch<Future>(CActions.fetchFollowTeam(followTeamId: followTeamId)),
                        startUnFollowTeam = unFollowTeamId => dispatcher.dispatch(new StartFetchUnFollowTeamAction {
                            unFollowTeamId = unFollowTeamId
                        }),
                        unFollowTeam = unFollowTeamId =>
                            dispatcher.dispatch<Future>(CActions.fetchUnFollowTeam(unFollowTeamId: unFollowTeamId))
                    };
                    return new SearchTeamScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }

    public class SearchTeamScreen : StatefulWidget {
        public SearchTeamScreen(
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
            return new _SearchTeamScreenState();
        }
    }

    class _SearchTeamScreenState : State<SearchTeamScreen> {
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

            this.widget.actionModel.searchTeam(arg1: this.widget.viewModel.searchKeyword, arg2: this._pageNumber)
                .then(_ => this._refreshController.sendBack(up: up, up ? RefreshStatus.completed : RefreshStatus.idle))
                .catchError(_ => this._refreshController.sendBack(up: up, mode: RefreshStatus.failed));
        }

        void _onFollow(UserType userType, string teamId) {
            if (this.widget.viewModel.isLoggedIn) {
                if (userType == UserType.follow) {
                    ActionSheetUtils.showModalActionSheet(
                        context: this.context,
                        new ActionSheet(
                            title: "确定不再关注？",
                            items: new List<ActionSheetItem> {
                                new ActionSheetItem("确定", type: ActionType.normal,
                                    () => {
                                        this.widget.actionModel.startUnFollowTeam(obj: teamId);
                                        this.widget.actionModel.unFollowTeam(arg: teamId);
                                    }),
                                new ActionSheetItem("取消", type: ActionType.cancel)
                            }
                        )
                    );
                }

                if (userType == UserType.unFollow) {
                    this.widget.actionModel.startFollowTeam(obj: teamId);
                    this.widget.actionModel.followTeam(arg: teamId);
                }
            }
            else {
                Navigator.pushNamed(
                    context: this.context,
                    routeName: NavigatorRoutes.Login
                );
            }
        }

        public override Widget build(BuildContext context) {
            var searchTeamIds = this.widget.viewModel.searchTeamIds;
            var searchKeyword = this.widget.viewModel.searchKeyword ?? "";
            Widget child = new Container();
            if (this.widget.viewModel.searchTeamLoading && searchTeamIds == null) {
                child = new GlobalLoading();
            }
            else if (searchKeyword.Length > 0) {
                child = searchTeamIds.isNotNullAndEmpty()
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
            var searchTeamIds = this.widget.viewModel.searchTeamIds;
            var enablePullUp = this.widget.viewModel.searchTeamHasMore;
            return new Container(
                color: CColors.Background,
                child: new CustomListView(
                    controller: this._refreshController,
                    enablePullDown: false,
                    enablePullUp: enablePullUp,
                    onRefresh: this._onRefresh,
                    itemCount: searchTeamIds.Count,
                    itemBuilder: this._buildTeamCard,
                    headerWidget: CustomListViewConstant.defaultHeaderWidget,
                    footerWidget: enablePullUp ? null : CustomListViewConstant.defaultFooterWidget
                )
            );
        }

        Widget _buildTeamCard(BuildContext context, int index) {
            var searchTeamIds = this.widget.viewModel.searchTeamIds;

            var searchTeamId = searchTeamIds[index: index];
            if (!this.widget.viewModel.teamDict.ContainsKey(key: searchTeamId)) {
                return new Container();
            }

            var searchTeam = this.widget.viewModel.teamDict[key: searchTeamId];
            var userType = UserType.unFollow;
            if (!this.widget.viewModel.isLoggedIn) {
                userType = UserType.unFollow;
            }
            else {
                if (this.widget.viewModel.currentUserId == searchTeam.id) {
                    userType = UserType.me;
                }
                else if (searchTeam.followTeamLoading ?? false) {
                    userType = UserType.loading;
                }
                else if (this.widget.viewModel.followMap.ContainsKey(key: searchTeam.id)) {
                    userType = UserType.follow;
                }
            }

            return new TeamCard(
                team: searchTeam,
                () => {
                    Navigator.pushNamed(context: this.context, routeName: NavigatorRoutes.TeamDetail,
                        new TeamDetailScreenArguments {
                            id = searchTeam.id
                        }
                    );
                },
                userType: userType,
                () => this._onFollow(userType: userType, teamId: searchTeam.id),
                true,
                new ObjectKey(value: searchTeam.id)
            );
        }
    }
}