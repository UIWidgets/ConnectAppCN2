using System.Collections.Generic;
using ConnectApp.Common.Other;
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
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.widgets;
using Notification = ConnectApp.Models.Model.Notification;

namespace ConnectApp.screens {
    public class NotificationScreenConnector : StatelessWidget {
        
        public NotificationScreenConnector(
            string category,
            Key key = null
        ) : base(key: key) {
            this.category = category;
        }
        
        string category;
        
        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, NotificationScreenViewModel>(
                converter: state => {
                    bool notificationLoading;
                    int page;
                    int pageTotal;
                    List<Notification> notifications;
                    List<User> mentions;

                    switch (this.category) {
                        case NotificationCategory.Follow: {
                            notificationLoading = state.notificationState.followNotificationState.loading;
                            page = state.notificationState.followNotificationState.page;
                            pageTotal = state.notificationState.followNotificationState.pageTotal;
                            notifications = state.notificationState.followNotificationState.notifications;
                            mentions = state.notificationState.followNotificationState.mentions;
                            break;
                        }
                        case NotificationCategory.Involve: {
                            notificationLoading = state.notificationState.involveNotificationState.loading;
                            page = state.notificationState.involveNotificationState.page;
                            pageTotal = state.notificationState.involveNotificationState.pageTotal;
                            notifications = state.notificationState.involveNotificationState.notifications;
                            mentions = state.notificationState.involveNotificationState.mentions;
                            break;
                        }
                        case NotificationCategory.Participate: {
                            notificationLoading = state.notificationState.participateNotificationState.loading;
                            page = state.notificationState.participateNotificationState.page;
                            pageTotal = state.notificationState.participateNotificationState.pageTotal;
                            notifications = state.notificationState.participateNotificationState.notifications;
                            mentions = state.notificationState.participateNotificationState.mentions;
                            break;
                        }
                        case NotificationCategory.System: {
                            notificationLoading = state.notificationState.systemNotificationState.loading;
                            page = state.notificationState.systemNotificationState.page;
                            pageTotal = state.notificationState.systemNotificationState.pageTotal;
                            notifications = state.notificationState.systemNotificationState.notifications;
                            mentions = state.notificationState.systemNotificationState.mentions;
                            break;
                        }
                        default: {
                            notificationLoading = state.notificationState.allNotificationState.loading;
                            page = state.notificationState.allNotificationState.page;
                            pageTotal = state.notificationState.allNotificationState.pageTotal;
                            notifications = state.notificationState.allNotificationState.notifications;
                            mentions = state.notificationState.allNotificationState.mentions;
                            break;
                        }
                    }
                    return new NotificationScreenViewModel {
                        notificationLoading = notificationLoading,
                        page = page,
                        pageTotal = pageTotal,
                        notifications = notifications,
                        mentions = mentions,
                        userDict = state.userState.userDict,
                        teamDict = state.teamState.teamDict,
                        showCategory = this.ShowCategory()
                    };
                },
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new NotificationScreenActionModel {
                        startFetchNotifications = () => dispatcher.dispatch(new StartFetchNotificationsAction {category = this.category}),
                        fetchNotifications = pageNumber =>
                            dispatcher.dispatch<Future>(CActions.fetchNotifications(pageNumber: pageNumber,
                                category: this.category))
                    };
                    return new NotificationScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }

        bool ShowCategory() {
            return NotificationCategory.All.Equals(value: this.category);
        }
    }

    public class NotificationScreen : StatefulWidget {
        public NotificationScreen(
            NotificationScreenViewModel viewModel = null,
            NotificationScreenActionModel actionModel = null,
            Key key = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly NotificationScreenViewModel viewModel;
        public readonly NotificationScreenActionModel actionModel;

        public override State createState() {
            return new _NotificationScreenState();
        }
    }

    public class _NotificationScreenState : State<NotificationScreen>, RouteAware {
        const int firstPageNumber = 1;
        int notificationPageNumber = firstPageNumber;
        RefreshController _refreshController;

        public override void initState() {
            base.initState();
            StatusBarManager.statusBarStyle(false);
            this._refreshController = new RefreshController();
            SchedulerBinding.instance.addPostFrameCallback(_ => {
                this.widget.actionModel.startFetchNotifications();
                this.widget.actionModel.fetchNotifications(arg: firstPageNumber);
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
            Widget content;
            var notifications = this.widget.viewModel.notifications;
            if (this.widget.viewModel.notificationLoading && 0 == notifications.Count) {
                content = new GlobalLoading();
            }
            else if (0 == notifications.Count) {
                content = new BlankView(
                    "好冷清，多和小伙伴们互动呀",
                    imageName: BlankImage.notification,
                    true,
                    () => {
                        this.widget.actionModel.startFetchNotifications();
                        this.widget.actionModel.fetchNotifications(arg: firstPageNumber);
                    }
                );
            }
            else {
                var enablePullUp = this.widget.viewModel.page < this.widget.viewModel.pageTotal;
                content = new Container(
                    color: CColors.Background,
                    child: new CustomListView(
                        controller: this._refreshController,
                        enablePullDown: true,
                        enablePullUp: enablePullUp,
                        onRefresh: this._onRefresh,
                        itemCount: notifications.Count,
                        itemBuilder: this._buildNotificationCard,
                        footerWidget: enablePullUp ? null : CustomListViewConstant.defaultFooterWidget,
                        hasScrollBar: false
                    )
                );
            }

            return new Container(
                color: CColors.White,
                child: new Column(
                    children: new List<Widget> {
                        new CustomDivider(
                            color: CColors.Separator2,
                            height: 1
                        ),
                        new Flexible(
                            child: new CustomScrollbar(child: content)
                        )
                    }
                )
            );
        }

        Widget _buildNotificationCard(BuildContext context, int index) {
            var notifications = this.widget.viewModel.notifications;

            var notification = notifications[index: index];
            if (notification.data.userId.isEmpty() && notification.data.role.Equals("user") &&
                !notification.type.Equals("chatwoot_message_unity_hub")) {
                return new Container();
            }

            User user;
            Team team;
            if (notification.type.Equals("project_article_publish") && notification.data.role.Equals("team")) {
                user = null;
                team = this.widget.viewModel.teamDict[key: notification.data.teamId];
            }
            else if (notification.type.Equals("chatwoot_message_unity_hub") && notification.data.role.Equals("user")) {
                user = new User {id = "unity-hub"};
                team = null;
            }
            else {
                user = this.widget.viewModel.userDict[key: notification.data.userId];
                team = null;
            }

            return new NotificationCard(
                notification: notification,
                user: user,
                team: team,
                mentions: this.widget.viewModel.mentions,
                () => {
                    if (notification.type == "followed" || notification.type == "team_followed") {
                        Navigator.pushNamed(context: this.context, routeName: NavigatorRoutes.UserDetail,
                            new UserDetailScreenArguments {
                                id = notification.data.userId
                            }
                        );
                    }
                    else if (notification.type.Equals("chatwoot_message_unity_hub")) {
                        CustomDialogUtils.showToast(context: this.context, "请至 Unity Hub 查看完整消息",
                            iconData: CIcons.UnityLogo);
                    }
                    else if (notification.data != null && notification.data.questionId.isNotEmpty()) {
                        Navigator.pushNamed(
                            context: this.context,
                            routeName: NavigatorRoutes.QuestionDetail,
                            new ScreenArguments {
                                id = notification.data.questionId
                            }
                        ); 
                    }
                    else {
                        Navigator.pushNamed(
                            context: context,
                            routeName: NavigatorRoutes.ArticleDetail,
                            new ArticleDetailScreenArguments {id = notification.data.projectId}
                        );
                        AnalyticsManager.ClickEnterArticleDetail(
                            "Notification_Article",
                            articleId: notification.data.projectId,
                            articleTitle: notification.data.projectTitle
                        );
                    }
                },
                userId => {
                    Navigator.pushNamed(context: this.context, routeName: NavigatorRoutes.UserDetail,
                        new UserDetailScreenArguments {
                            id = userId
                        }
                    );
                },
                teamId => {
                    Navigator.pushNamed(context: this.context, routeName: NavigatorRoutes.TeamDetail,
                        new TeamDetailScreenArguments {
                            id = teamId
                        }
                    );
                },
                index == notifications.Count - 1,
                new ObjectKey(value: notification.id),
                showCategory:this.widget.viewModel.showCategory
            );
        }

        void _onRefresh(bool up) {
            this.notificationPageNumber = up ? firstPageNumber : this.notificationPageNumber + 1;
            this.widget.actionModel.fetchNotifications(arg: this.notificationPageNumber)
                .then(_ => this._refreshController.sendBack(up: up, up ? RefreshStatus.completed : RefreshStatus.idle))
                .catchError(_ => this._refreshController.sendBack(up: up, mode: RefreshStatus.failed));
        }

        public void didPopNext() {
            StatusBarManager.statusBarStyle(false);
            CTemporaryValue.currentPageModelId = NavigatorRoutes.Notification;
        }

        public void didPush() {
            CTemporaryValue.currentPageModelId = NavigatorRoutes.Notification;
        }

        public void didPop() {
            CTemporaryValue.currentPageModelId = null;
        }

        public void didPushNext() {
            CTemporaryValue.currentPageModelId = null;
        }
    }
}