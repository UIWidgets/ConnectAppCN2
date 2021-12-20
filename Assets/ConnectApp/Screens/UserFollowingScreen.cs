using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Components;
using ConnectApp.Models.ActionModel;
using ConnectApp.Models.State;
using ConnectApp.Models.ViewModel;
using ConnectApp.redux.actions;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.widgets;

namespace ConnectApp.screens {
    public class UserFollowingScreenConnector : StatelessWidget {
        public UserFollowingScreenConnector(
            string userId,
            int initialPage,
            Key key = null
        ) : base(key: key) {
            this.userId = userId;
            this.initialPage = initialPage;
        }

        readonly string userId;
        readonly int initialPage;

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, UserFollowingScreenViewModel>(
                converter: state => new UserFollowingScreenViewModel {
                    userId = this.userId,
                    initialPage = this.initialPage,
                    searchFollowingKeyword = state.searchState.searchFollowingKeyword,
                    searchFollowingUsers = state.searchState.searchFollowings,
                    currentUserId = state.loginState.loginInfo.userId ?? ""
                },
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new UserFollowingScreenActionModel {
                        clearSearchFollowingResult = () => dispatcher.dispatch(new ClearSearchFollowingResultAction())
                    };
                    return new UserFollowingScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }

    public class UserFollowingScreen : StatefulWidget {
        public UserFollowingScreen(
            UserFollowingScreenViewModel viewModel = null,
            UserFollowingScreenActionModel actionModel = null,
            Key key = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly UserFollowingScreenViewModel viewModel;
        public readonly UserFollowingScreenActionModel actionModel;

        public override State createState() {
            return new _UserFollowingScreenState();
        }
    }

    class _UserFollowingScreenState : State<UserFollowingScreen>, RouteAware {
        string _title;

        public override void initState() {
            base.initState();
            StatusBarManager.statusBarStyle(false);
            this._title = this.widget.viewModel.currentUserId == this.widget.viewModel.userId
                ? "我关注的"
                : "全部关注";
            SchedulerBinding.instance.addPostFrameCallback(_ => {
                if (this.widget.viewModel.searchFollowingKeyword.Length > 0
                    || this.widget.viewModel.searchFollowingUsers.Count > 0) {
                    this.widget.actionModel.clearSearchFollowingResult();
                }
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
                                    child: this._buildContentView()
                                )
                            }
                        )
                    )
                )
            );
        }

        Widget _buildNavigationBar() {
            return new CustomNavigationBar(
                new Text(
                    data: this._title,
                    style: CTextStyle.H2
                ),
                onBack: () => Navigator.pop(context: this.context)
            );
        }

        Widget _buildContentView() {
            return new CustomSegmentedControl(
                new List<object> {"用户", "公司"},
                new List<Widget> {
                    new UserFollowingUserScreenConnector(userId: this.widget.viewModel.userId),
                    new UserFollowingTeamScreenConnector(userId: this.widget.viewModel.userId)
                },
                currentIndex: this.widget.viewModel.initialPage
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