using System;
using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Components;
using ConnectApp.Components.Toast;
using ConnectApp.Models.Model;
using ConnectApp.Models.State;
using ConnectApp.redux;
using ConnectApp.screens;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace ConnectApp.Main {
    public class ConnectApp : StatelessWidget {
        public static RouteObserve<PageRoute> routeObserver;
        static HeroController _heroController;
        bool _exitApp;
        Timer _timer;

        public override Widget build(BuildContext context) {
            routeObserver = new RouteObserve<PageRoute>();
            _heroController = new HeroController();
            return new StoreProvider<AppState>(
                store: StoreProvider.store,
                new WidgetsApp(
                    color: CColors.White,
                    initialRoute: NavigatorRoutes.Root,
                    onGenerateRoute: this.onGenerateRoute,
                    onUnknownRoute: onUnknownRoute,
                    navigatorObservers: new List<NavigatorObserver> {
                        routeObserver,
                        _heroController
                    }
                )
            );
        }

        Future<bool> onWillPop() {
            TipMenu.dismiss();
            if (VersionManager.needForceUpdate()) {
                return Future.value(false).to<bool>();
            }

            if (Screen.orientation != ScreenOrientation.Portrait) {
                // 视频全屏时禁止物理返回按钮
                EventBus.publish(sName: EventBusConstant.fullScreen, new List<object> { true });
            }
            else {
                if (!CCommonUtils.isAndroid) {
                    return Future.value(false).to<bool>();
                }

                if (this._exitApp) {
                    if (this._timer != null) {
                        this._timer.Dispose();
                        this._timer = null;
                    }

                    Application.Quit();
                    return Future.value(true).to<bool>();
                }

                this._exitApp = true;
                this._timer = Timer.create(TimeSpan.FromMilliseconds(2000),
                    () => { this._exitApp = false; }
                );
                CustomToast.showToast(null, "再按一次退出");
            }

            return Future.value(false).to<bool>();
        }

        Route onGenerateRoute(RouteSettings settings) {
            WidgetBuilder builder = null;
            var fullscreenDialog = false;
            switch (settings.name) {
                case NavigatorRoutes.Root: {
                    Widget screen = new RootScreen();
                    
                    if (VersionManager.needForceUpdate()) {
                        screen = new ForceUpdateScreen();
                    }

                    builder = _ => new WillPopScope(
                        onWillPop: this.onWillPop,
                        child: screen
                    );
                    break;
                }
                case NavigatorRoutes.Splash: {
                    return new PageRouteBuilder(
                        pageBuilder: (context, animation, secondaryAnimation) => new WillPopScope(
                            onWillPop: this.onWillPop,
                            child: new SplashScreen()
                        ),
                        transitionDuration: TimeSpan.FromMilliseconds(600),
                        transitionsBuilder: (context1, animation, secondaryAnimation, child) =>
                            new FadeTransition(
                                opacity: animation,
                                child: child
                            )
                    );
                }
                case NavigatorRoutes.Main: {
                    return new PageRouteBuilder(
                        pageBuilder: (context, animation, secondaryAnimation) => new WillPopScope(
                            onWillPop: this.onWillPop,
                            child: new MainScreen()
                        ),
                        transitionDuration: TimeSpan.FromMilliseconds(600),
                        transitionsBuilder: (context1, animation, secondaryAnimation, child) =>
                            new FadeTransition(
                                opacity: animation,
                                child: child
                            )
                    );
                }
                case NavigatorRoutes.Login: {
                    fullscreenDialog = true;
                    builder = _ => new LoginScreen();
                    break;
                }
                case NavigatorRoutes.LeaderBoard: {
                    var arg = settings.arguments as LeaderBoardScreenArguments;
                    builder = _ => new LeaderBoardScreenConnector(initIndex: arg.initIndex);
                    break;
                }
                case NavigatorRoutes.LeaderBoardDetail: {
                    var arg = settings.arguments as LeaderBoardDetailScreenArguments;
                    builder = _ => new LeaderBoardDetailScreenConnector(tagId: arg.id, type: arg.type);
                    break;
                }
                case NavigatorRoutes.Blogger: {
                    builder = _ => new BloggerScreenConnector();
                    break;
                }
                case NavigatorRoutes.UserFollowing: {
                    var arg = settings.arguments as UserFollowingScreenArguments;
                    builder = _ => new UserFollowingScreenConnector(userId: arg.id, initialPage: arg.initialPage);
                    break;
                }
                case NavigatorRoutes.UserLike: {
                    var arg = settings.arguments as ScreenArguments;
                    builder = _ => new UserLikeArticleScreenConnector(userId: arg.id);
                    break;
                }
                case NavigatorRoutes.UserFollower: {
                    var arg = settings.arguments as ScreenArguments;
                    builder = _ => new UserFollowerScreenConnector(userId: arg.id);
                    break;
                }
                case NavigatorRoutes.WritingCenter: {
                    var arg = settings.arguments as ScreenArguments;
                    builder = _ => new WritingCenterScreenConnector(userId: arg.id);
                    break;
                }
                case NavigatorRoutes.Search: {
                    var arg = settings.arguments as SearchScreenArguments;
                    fullscreenDialog = arg.isModal;
                    builder = _ => new SearchScreenConnector(searchType: arg.searchType, searchKeyword: arg.keyword);
                    break;
                }
                case NavigatorRoutes.UserDetail: {
                    var arg = settings.arguments as UserDetailScreenArguments;
                    builder = _ => new UserDetailScreenConnector(userId: arg.id, isSlug: arg.isSlug);
                    break;
                }
                case NavigatorRoutes.EditPersonalInfo: {
                    var arg = settings.arguments as ScreenArguments;
                    builder = _ => new EditPersonalInfoScreenConnector(personalId: arg.id);
                    break;
                }
                case NavigatorRoutes.PersonalRole: {
                    builder = _ => new PersonalJobRoleScreenConnector();
                    break;
                }
                case NavigatorRoutes.FeedbackType: {
                    builder = _ => new FeedbackTypeScreenConnector();
                    break;
                }
                case NavigatorRoutes.FavoriteDetail: {
                    var arg = settings.arguments as FavoriteDetailScreenArguments;
                    builder = _ =>
                        new FavoriteDetailScreenConnector(userId: arg.userId, tagId: arg.tagId, type: arg.type);
                    break;
                }
                case NavigatorRoutes.EditFavorite: {
                    var arg = settings.arguments as ScreenArguments;
                    builder = _ => new EditFavoriteScreenConnector(tagId: arg.id);
                    break;
                }
                case NavigatorRoutes.TeamDetail: {
                    var arg = settings.arguments as TeamDetailScreenArguments;
                    builder = _ => new TeamDetailScreenConnector(teamId: arg.id, isSlug: arg.isSlug);
                    break;
                }
                case NavigatorRoutes.TeamFollower: {
                    var arg = settings.arguments as ScreenArguments;
                    builder = _ => new TeamFollowerScreenConnector(teamId: arg.id);
                    break;
                }
                case NavigatorRoutes.TeamMember: {
                    var arg = settings.arguments as ScreenArguments;
                    builder = _ => new TeamMemberScreenConnector(teamId: arg.id);
                    break;
                }
                case NavigatorRoutes.Report: {
                    var arg = settings.arguments as ReportScreenArguments;
                    builder = _ => new ReportScreenConnector(reportId: arg.id, reportType: arg.reportType);
                    break;
                }
                case NavigatorRoutes.ArticleDetail: {
                    var arg = settings.arguments as ArticleDetailScreenArguments;
                    builder = _ => new ArticleDetailScreenConnector(articleId: arg.id, isPush: arg.isPush);
                    break;
                }
                case NavigatorRoutes.QuestionDetail: {
                    var arg = settings.arguments as ScreenArguments;
                    builder = _ => new QuestionDetailScreenConnector(questionId: arg.id);
                    break;
                }
                case NavigatorRoutes.PostQuestion: {
                    var arg = settings.arguments as PostQuestionScreenArguments;
                    fullscreenDialog = arg.isModal;
                    builder = _ => new PostQuestionScreenConnector(questionId: arg.id);
                    break;
                }
                case NavigatorRoutes.SelectPlate: {
                    builder = _ => new SelectPlateScreenConnector();
                    break;
                }
                case NavigatorRoutes.AnswerDetail: {
                    var arg = settings.arguments as AnswerDetailScreenArguments;
                    fullscreenDialog = arg.isModal;
                    builder = _ => new AnswerDetailScreenConnector(questionId: arg.questionId, answerId: arg.answerId);
                    break;
                }
                case NavigatorRoutes.PostAnswer: {
                    var arg = settings.arguments as PostAnswerScreenArguments;
                    fullscreenDialog = arg.isModal;
                    builder = _ => new PostAnswerScreenConnector(questionId: arg.questionId, answerId: arg.answerId,
                        canSave: arg.canSave);
                    break;
                }
                case NavigatorRoutes.QACommentDetail: {
                    var arg = settings.arguments as QACommentDetailScreenArguments;
                    builder = _ =>
                        new QACommentDetailScreenConnector(channelId: arg.channelId, messageId: arg.messageId);
                    break;
                }
                case NavigatorRoutes.DraftBox: {
                    builder = _ => new DraftBoxScreenConnector();
                    break;
                }
                case NavigatorRoutes.RealName: {
                    fullscreenDialog = true;
                    builder = _ => new RealNameScreenConnector();
                    break;
                }
                case NavigatorRoutes.MyFavorite: {
                    builder = _ => new MyFavoriteScreenConnector();
                    break;
                }
                case NavigatorRoutes.History: {
                    builder = _ => new HistoryScreenConnector();
                    break;
                }
                case NavigatorRoutes.Setting: {
                    builder = _ => new SettingScreenConnector();
                    break;
                }
                case NavigatorRoutes.Feedback: {
                    builder = _ => new FeedbackScreenConnector();
                    break;
                }
                case NavigatorRoutes.BindUnity: {
                    builder = _ => new BindUnityScreenConnector(fromPage: FromPage.setting);
                    break;
                }
                case NavigatorRoutes.AboutUs: {
                    builder = _ => new AboutUsScreenConnector();
                    break;
                }
                case NavigatorRoutes.Notification: {
                    builder = _ => new NotificationContainerScreenConnector();
                    break;
                }
                case NavigatorRoutes.QRScanLogin: {
                    var arg = settings.arguments as QRScanLoginScreenArguments;
                    builder = _ => new QRScanLoginScreenConnector(token: arg.token);
                    break;
                }
                case NavigatorRoutes.ForceUpdate: {
                    builder = _ => new ForceUpdateScreen();
                    break;
                }
                case NavigatorRoutes.PhotoView: {
                    var arg = settings.arguments as PhotoViewScreenArguments;
                    if (arg.url.isNotEmpty() && arg.urls.isNotEmpty() && arg.urls.Contains(item: arg.url)) {
                        var index = arg.urls.IndexOf(item: arg.url);
                        return new PageRouteBuilder(
                            pageBuilder: (context, _, __) => new PhotoView(
                                urls: arg.urls,
                                index: index,
                                useCachedNetworkImage: arg.useCachedNetworkImage,
                                imageData: arg.imageData
                            ),
                            transitionDuration: TimeSpan.FromMilliseconds(200),
                            transitionsBuilder: (context1, animation, secondaryAnimation, child) =>
                                new FadeTransition(
                                    opacity: animation,
                                    child: child
                                )
                        );
                    }

                    break;
                }
                case NavigatorRoutes.VideoPlayer: {
                    var arg = settings.arguments as VideoPlayerScreenArguments;
                    if (arg.url.isNotEmpty()) {
                        return new PageRouteBuilder(
                            pageBuilder: (context, _, __) => new VideoViewScreen(
                                url: arg.url,
                                verifyType: arg.verifyType,
                                limitSeconds: arg.limitSeconds
                            ),
                            transitionDuration: TimeSpan.FromMilliseconds(200),
                            transitionsBuilder: (context1, animation, secondaryAnimation, child) =>
                                new FadeTransition(
                                    opacity: animation,
                                    child: child
                                )
                        );
                    }

                    break;
                }
            }

            return new CustomPageRoute(
                builder: builder,
                settings: settings,
                fullscreenDialog: fullscreenDialog
            );
        }

        static Route onUnknownRoute(RouteSettings settings) {
            return new CustomPageRoute(
                _ => new Container(color: CColors.White, child: new Center(child: new Text("NOT FOUND 404"))),
                settings: settings
            );
        }
    }

    static class NavigatorRoutes {
        public const string Root = "/";
        public const string Splash = "/splash";
        public const string Main = "/main";
        public const string Search = "/search";
        public const string ArticleDetail = "/article-detail";
        public const string Notification = "/notification";
        public const string Setting = "/setting";
        public const string MyFavorite = "/my-favorite";
        public const string History = "/history";
        public const string Login = "/login";
        public const string BindUnity = "/bind-unity";
        public const string Report = "/report";
        public const string AboutUs = "/aboutUs";
        public const string UserDetail = "/user-detail";
        public const string FavoriteDetail = "/favorite-detail";
        public const string EditFavorite = "/edit-favorite";
        public const string UserFollowing = "/user-following";
        public const string UserFollower = "/user-follower";
        public const string UserLike = "/user-like";
        public const string EditPersonalInfo = "/edit-personalInfo";
        public const string PersonalRole = "/personal-role";
        public const string TeamDetail = "/team-detail";
        public const string TeamFollower = "/team-follower";
        public const string TeamMember = "/team-member";
        public const string QRScanLogin = "/qr-login";
        public const string Feedback = "/feedback";
        public const string FeedbackType = "/feedback-type";
        public const string LeaderBoard = "/leader-board";
        public const string LeaderBoardDetail = "/leader-board-detail";
        public const string Blogger = "/blogger";
        public const string ForceUpdate = "/force-update";
        public const string QA = "/question-and-answer";
        public const string QuestionDetail = "/question-detail";
        public const string AnswerDetail = "/answer-detail";
        public const string QATopLevelComment = "/qa-top-level-comment";
        public const string QACommentDetail = "/qa-comment-detail";
        public const string PostQuestion = "/post-question";
        public const string SelectPlate = "/select-plate";
        public const string AddTag = "/add-tag";
        public const string RealName = "/real-name";
        public const string PostAnswer = "/post-answer";
        public const string WritingCenter = "/writing-center";
        public const string DraftBox = "/draft-box";
        public const string PhotoView = "/photo-view";
        public const string VideoPlayer = "/video-player";
    }
}