using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Components;
using ConnectApp.Components.PullToRefresh;
using ConnectApp.Models.ActionModel;
using ConnectApp.Models.State;
using ConnectApp.Models.ViewModel;
using ConnectApp.redux.actions;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace ConnectApp.screens {
    public class LearnScreenConnector : StatelessWidget {
        public LearnScreenConnector(
            Key key = null
        ) : base(key: key) {
        }

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, LearnScreenViewModel>(
                converter: state => new LearnScreenViewModel {
                    courses = state.learnState.courses,
                    hasMore = state.learnState.hasMore
                },
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new LearnScreenActionModel {
                        fetchLearnCourses = page => dispatcher.dispatch<Future>(CActions.fetchLearnCourseList(page: page))
                    };
                    return new LearnScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }
    
    public class LearnScreen : StatefulWidget {
        public LearnScreen(
            Key key = null,
            LearnScreenViewModel viewModel = null,
            LearnScreenActionModel actionModel = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly LearnScreenViewModel viewModel;
        public readonly LearnScreenActionModel actionModel;

        public override State createState() {
            return new _LearnScreenState();
        }
    }

    class _LearnScreenState : AutomaticKeepAliveClientMixin<LearnScreen>, RouteAware {
        
        static int firstPageNumber = 0;
        RefreshController _refreshController;
        bool _isLoading;
        int _pageNumber;
        
        protected override bool wantKeepAlive {
            get { return true; }
        }
        
        public override void didChangeDependencies() {
            base.didChangeDependencies();
            Main.ConnectApp.routeObserver.subscribe(this, (PageRoute) ModalRoute.of(context: this.context));
        }

        public override void dispose() {
            Main.ConnectApp.routeObserver.unsubscribe(this);
            base.dispose();
        }
        
        public override void initState() {
            base.initState();
            this._refreshController = new RefreshController();
            this._pageNumber = firstPageNumber;
            this._isLoading = true;
            SchedulerBinding.instance.addPostFrameCallback(_ => {
                this.widget.actionModel.fetchLearnCourses(arg: firstPageNumber).then(v => {
                    this._isLoading = false;
                }).catchError(err => {
                    this._isLoading = false;
                });
            });
        }

        Widget _buildContentView() {
            var courses = this.widget.viewModel.courses;
            if (this._isLoading && courses.isNullOrEmpty()) {
                return new Container(
                    padding: EdgeInsets.only(bottom: CSizes.TabBarHeight +
                                                     CCommonUtils.getSafeAreaBottomPadding(context: this.context)),
                    child: new GlobalLoading()
                );
            }

            if (0 == courses.Count) {
                return new Container(
                    padding: EdgeInsets.only(bottom: CSizes.TabBarHeight +
                                                     CCommonUtils.getSafeAreaBottomPadding(context: this.context)),
                    child: new BlankView(
                        "暂无新课程，稍后刷新看看",
                        imageName: BlankImage.events,
                        true,
                        () => {
                            this._isLoading = true;
                            this.widget.actionModel.fetchLearnCourses(arg: firstPageNumber).then(v => {
                                this._isLoading = false;
                            }).catchError(err => {
                                this._isLoading = false;
                            });
                        }
                    )
                );
            }

            var enablePullUp = this.widget.viewModel.hasMore;
            
            return new Container(
                color: CColors.Background,
                child: new CustomListView(
                    controller: this._refreshController,
                    enablePullDown: true,
                    enablePullUp: enablePullUp,
                    onRefresh: this._refresh,
                    hasBottomMargin: true,
                    itemCount: courses.Count,
                    itemBuilder: this._buildLearnCard,
                    headerWidget: CustomListViewConstant.defaultHeaderWidget,
                    footerWidget: enablePullUp ? null : new EndView(hasBottomMargin: true)
                )
            );
        }

        Widget _buildLearnCard(BuildContext buildContext, int index) {
            var course = this.widget.viewModel.courses[index: index];
            return new LearnCard(
                model: course,
                () => Application.OpenURL(CStringUtils.genLearnCourseUrl(id: course.id))
            );
        }
        
        void _refresh(bool up) {
            if (up) {
                this._pageNumber = firstPageNumber;
            }
            else {
                this._pageNumber++;
            }
            this.widget.actionModel.fetchLearnCourses(arg: this._pageNumber)
                .then(_ => this._refreshController.sendBack(up: up, up ? RefreshStatus.completed : RefreshStatus.idle))
                .catchError(_ => this._refreshController.sendBack(up: up, mode: RefreshStatus.failed));
        }

        public override Widget build(BuildContext context) {
            return new Container(
                color: CColors.White,
                child: new CustomSafeArea(
                    bottom: false,
                    child: new Column(
                        children: new List<Widget> {
                            new CustomNavigationBar(
                                new Text("课堂", style: CTextStyle.H2)
                            ),
                            new Container(color: CColors.Separator2, height: 1),
                            new Expanded(
                                child: this._buildContentView()
                            )
                        }
                    )
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