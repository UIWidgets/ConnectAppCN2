using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Components;
using ConnectApp.Main;
using ConnectApp.Models.ActionModel;
using ConnectApp.Models.Model;
using ConnectApp.Models.State;
using ConnectApp.Models.ViewModel;
using ConnectApp.redux.actions;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.service;
using Unity.UIWidgets.widgets;

namespace ConnectApp.screens {
    public class HistoryArticleScreenConnector : StatelessWidget {
        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, HistoryScreenViewModel>(
                converter: state => new HistoryScreenViewModel {
                    articleHistory = state.articleState.articleHistory,
                    isLoggedIn = state.loginState.isLoggedIn
                },
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new HistoryScreenActionModel {
                        pushToBlock = articleId => {
                            dispatcher.dispatch(new BlockArticleAction {articleId = articleId});
                            dispatcher.dispatch(new DeleteArticleHistoryAction {articleId = articleId});
                        },
                        shareToWechat = (type, title, description, linkUrl, imageUrl, path) =>
                            dispatcher.dispatch<Future>(
                                CActions.shareToWechat(sheetItemType: type, title: title, description: description,
                                    linkUrl: linkUrl,
                                    imageUrl: imageUrl, path: path)),
                        deleteArticleHistory = id =>
                            dispatcher.dispatch(new DeleteArticleHistoryAction {articleId = id})
                    };
                    return new HistoryArticleScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }

    public class HistoryArticleScreen : StatelessWidget {
        public HistoryArticleScreen(
            HistoryScreenViewModel viewModel = null,
            HistoryScreenActionModel actionModel = null,
            Key key = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        readonly HistoryScreenViewModel viewModel;
        readonly HistoryScreenActionModel actionModel;

        readonly CustomDismissibleController _controller = new CustomDismissibleController();

        public override Widget build(BuildContext context) {
            if (this.viewModel.articleHistory.Count == 0) {
                return new BlankView(
                    "哎呀，还没有任何文章记录",
                    imageName: BlankImage.common
                );
            }

            return new Container(
                color: CColors.Background,
                child: new CustomListView(
                    itemCount: this.viewModel.articleHistory.Count,
                    itemBuilder: this._buildArticleCard,
                    headerWidget: CustomListViewConstant.defaultHeaderWidget,
                    hasRefresh: false
                )
            );
        }

        Widget _buildArticleCard(BuildContext context, int index) {
            var article = this.viewModel.articleHistory[index: index];
            var linkUrl = CStringUtils.JointProjectShareLink(projectId: article.id);
            return CustomDismissible.builder(
                Key.key(value: article.id),
                new ArticleCard(
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
                        true,
                        isLoggedIn: this.viewModel.isLoggedIn,
                        () => {
                            Clipboard.setData(new ClipboardData(text: linkUrl));
                            CustomDialogUtils.showToast(context: context, "复制链接成功",
                                iconData: CIcons.check_circle_outline);
                        },
                        () => Navigator.pushNamed(
                            context: context,
                            routeName: NavigatorRoutes.Login
                        ),
                        () => this.actionModel.pushToBlock(obj: article.id),
                        () => Navigator.pushNamed(
                            context: context,
                            routeName: NavigatorRoutes.Report,
                            new ReportScreenArguments {
                                id = article.id,
                                reportType = ReportType.article
                            }
                        ),
                        type => {
                            CustomDialogUtils.showCustomDialog(
                                context: context,
                                child: new CustomLoadingDialog()
                            );
                            var imageUrl = CImageUtils.SizeTo200ImageUrl(imageUrl: article.thumbnail.url);
                            this.actionModel.shareToWechat(arg1: type, arg2: article.title,
                                    arg3: article.subTitle, arg4: linkUrl, arg5: imageUrl, "")
                                .then(_ => CustomDialogUtils.hiddenCustomDialog(context: context))
                                .catchError(_ => CustomDialogUtils.hiddenCustomDialog(context: context));
                        }
                    ),
                    fullName: article.fullName,
                    new ObjectKey(value: article.id)
                ),
                new CustomDismissibleDrawerDelegate(),
                secondaryActions: new List<Widget> {
                    new DeleteActionButton(
                        80,
                        onTap: () => this.actionModel.deleteArticleHistory(obj: article.id)
                    )
                },
                controller: this._controller
            );
        }
    }
}