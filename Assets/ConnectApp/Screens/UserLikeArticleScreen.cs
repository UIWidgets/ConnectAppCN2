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
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.service;
using Unity.UIWidgets.widgets;

namespace ConnectApp.screens {
    public class UserLikeArticleScreenConnector : StatelessWidget {
        public UserLikeArticleScreenConnector(
            string userId,
            Key key = null
        ) : base(key: key) {
            this.userId = userId;
        }

        readonly string userId;

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, UserLikeArticleScreenViewModel>(
                converter: state => {
                    var user = state.userState.userDict.ContainsKey(key: this.userId)
                        ? state.userState.userDict[key: this.userId]
                        : new User();
                    var likeArticleIds = user.likeArticleIds ?? new List<string>();
                    return new UserLikeArticleScreenViewModel {
                        likeArticleLoading = state.userState.userLikeArticleLoading,
                        likeArticleIds = likeArticleIds,
                        likeArticlePage = user.likeArticlesPage ?? 1,
                        likeArticleHasMore = user.likeArticlesHasMore ?? false,
                        isLoggedIn = state.loginState.isLoggedIn,
                        articleDict = state.articleState.articleDict,
                        userDict = state.userState.userDict,
                        teamDict = state.teamState.teamDict
                    };
                },
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new UserLikeArticleScreenActionModel {
                        startFetchUserLikeArticle = () => dispatcher.dispatch(new StartFetchUserLikeArticleAction()),
                        fetchUserLikeArticle = pageNumber =>
                            dispatcher.dispatch<Future>(CActions.fetchUserLikeArticle(userId: this.userId,
                                pageNumber: pageNumber)),
                        blockArticleAction = articleId => {
                            dispatcher.dispatch(new BlockArticleAction {articleId = articleId});
                            dispatcher.dispatch(new DeleteArticleHistoryAction {articleId = articleId});
                        },
                        shareToWechat = (type, title, description, linkUrl, imageUrl, path) => dispatcher.dispatch<Future>(
                            CActions.shareToWechat(sheetItemType: type, title: title, description: description,
                                linkUrl: linkUrl,
                                imageUrl: imageUrl))
                    };
                    return new UserLikeArticleScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }

    public class UserLikeArticleScreen : StatefulWidget {
        public UserLikeArticleScreen(
            UserLikeArticleScreenViewModel viewModel,
            UserLikeArticleScreenActionModel actionModel,
            Key key = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly UserLikeArticleScreenViewModel viewModel;
        public readonly UserLikeArticleScreenActionModel actionModel;

        public override State createState() {
            return new _UserLikeArticleScreenState();
        }
    }

    class _UserLikeArticleScreenState : State<UserLikeArticleScreen>, RouteAware {
        const int firstPageNumber = 1;
        int likeArticlePageNumber = firstPageNumber;
        RefreshController _refreshController;

        public override void initState() {
            base.initState();
            StatusBarManager.statusBarStyle(false);
            this._refreshController = new RefreshController();
            SchedulerBinding.instance.addPostFrameCallback(_ => {
                this.widget.actionModel.startFetchUserLikeArticle();
                this.widget.actionModel.fetchUserLikeArticle(arg: firstPageNumber);
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

        void _onRefresh(bool up) {
            this.likeArticlePageNumber = up ? firstPageNumber : this.likeArticlePageNumber + 1;
            this.widget.actionModel.fetchUserLikeArticle(arg: this.likeArticlePageNumber)
                .then(_ => this._refreshController.sendBack(up: up, up ? RefreshStatus.completed : RefreshStatus.idle))
                .catchError(_ => this._refreshController.sendBack(up: up, mode: RefreshStatus.failed));
        }

        public override Widget build(BuildContext context) {
            var likeArticleIds = this.widget.viewModel.likeArticleIds;
            Widget content;
            if (this.widget.viewModel.likeArticleLoading && likeArticleIds.isEmpty()) {
                content = new GlobalLoading();
            }
            else if (likeArticleIds.Count <= 0) {
                content = new BlankView(
                    "暂无点赞的文章",
                    imageName: BlankImage.common,
                    true,
                    () => {
                        this.widget.actionModel.startFetchUserLikeArticle();
                        this.widget.actionModel.fetchUserLikeArticle(arg: firstPageNumber);
                    }
                );
            }
            else {
                var enablePullUp = this.widget.viewModel.likeArticleHasMore;
                content = new CustomListView(
                    controller: this._refreshController,
                    enablePullDown: true,
                    enablePullUp: enablePullUp,
                    onRefresh: this._onRefresh,
                    itemCount: likeArticleIds.Count,
                    itemBuilder: this._buildUserCard,
                    footerWidget: enablePullUp ? null : CustomListViewConstant.defaultFooterWidget
                );
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
                                    child: content
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
                    "点赞的文章",
                    style: CTextStyle.H2
                ),
                onBack: () => Navigator.pop(context: this.context)
            );
        }

        Widget _buildUserCard(BuildContext context, int index) {
            var articleId = this.widget.viewModel.likeArticleIds[index: index];
            var articleDict = this.widget.viewModel.articleDict;
            if (!articleDict.ContainsKey(key: articleId)) {
                return new Container();
            }

            var article = articleDict[key: articleId];
            var linkUrl = CStringUtils.JointProjectShareLink(projectId: article.id);
            var fullName = "";
            if (article.ownerType == OwnerType.user.ToString()) {
                if (this.widget.viewModel.userDict.ContainsKey(key: article.userId)) {
                    fullName = this.widget.viewModel.userDict[key: article.userId].fullName
                               ?? this.widget.viewModel.userDict[key: article.userId].name;
                }
            }

            if (article.ownerType == OwnerType.team.ToString()) {
                if (this.widget.viewModel.teamDict.ContainsKey(key: article.teamId)) {
                    fullName = this.widget.viewModel.teamDict[key: article.teamId].name;
                }
            }

            return new ArticleCard(
                article: article,
                () => Navigator.pushNamed(
                    context: context, 
                    routeName: NavigatorRoutes.ArticleDetail,
                    new ArticleDetailScreenArguments {
                        id = article.id
                    }
                ),
                () => ShareManager.showDoubleDeckShareView(
                    context: context,
                    false,
                    isLoggedIn: this.widget.viewModel.isLoggedIn,
                    () => {
                        Clipboard.setData(new ClipboardData(text: linkUrl));
                        CustomDialogUtils.showToast(context: this.context, "复制链接成功",
                            iconData: CIcons.check_circle_outline);
                    },
                    () => Navigator.pushNamed(
                        context: this.context,
                        routeName: NavigatorRoutes.Login
                    ),
                    () => this.widget.actionModel.blockArticleAction(obj: article.id),
                    () => Navigator.pushNamed(
                        context: this.context,
                        routeName: NavigatorRoutes.Report,
                        new ReportScreenArguments {
                            id = article.id,
                            reportType = ReportType.article
                        }
                    ),
                    type => {
                        CustomDialogUtils.showCustomDialog(
                            context: this.context,
                            child: new CustomLoadingDialog()
                        );
                        var imageUrl = CImageUtils.SizeTo200ImageUrl(imageUrl: article.thumbnail.url);
                        this.widget.actionModel.shareToWechat(arg1: type, arg2: article.title,
                                arg3: article.subTitle, arg4: linkUrl, arg5: imageUrl, "")
                            .then(_ => CustomDialogUtils.hiddenCustomDialog(context: this.context))
                            .catchError(_ => CustomDialogUtils.hiddenCustomDialog(context: this.context));
                    }
                ),
                fullName: fullName,
                new ObjectKey(value: article.id)
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