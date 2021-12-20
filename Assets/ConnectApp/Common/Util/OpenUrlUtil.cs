using System;
using ConnectApp.Common.Constant;
using ConnectApp.Main;
using ConnectApp.Models.Model;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;
using UnityEngine;

namespace ConnectApp.Common.Util {
    public static class OpenUrlUtil {
        public static void OpenUrl(BuildContext buildContext, string url) {

            if (!url.isUrl()) {
                return;
            }

            var uri = new Uri(uriString: url);
            var hostCn = new Uri(uriString: Config.apiAddress_cn).Host;
            var hostCom = new Uri(uriString: Config.apiAddress_com).Host;
            var hostUnityCn = new Uri(uriString: Config.unity_cn_url).Host;
            var hostDeveloper = new Uri(uriString: Config.developer_unity_cn_url).Host;

            if (uri.Host.Equals(value: hostCn)
                || uri.Host.Equals(value: hostCom)
                || uri.Host.Equals(value: hostUnityCn)
                || uri.Host.Equals(value: hostDeveloper)) {
                if (uri.AbsolutePath.StartsWith("/p/")) {
                    var articleId = uri.AbsolutePath.Remove(0, count: "/p/".Length);
                    if (CTemporaryValue.currentPageModelId.isNotEmpty() &&
                        CTemporaryValue.currentPageModelId.Equals(value: articleId)) {
                        return;
                    }
                    Navigator.pushNamed(
                        context: buildContext, 
                        routeName: NavigatorRoutes.ArticleDetail,
                        new ArticleDetailScreenArguments {
                            id = articleId
                        }
                    );
                }
                else if (uri.AbsolutePath.StartsWith("/projects/")) {
                    var articleId = uri.AbsolutePath.Remove(0, count: "/projects/".Length);
                    if (CTemporaryValue.currentPageModelId.isNotEmpty() &&
                        CTemporaryValue.currentPageModelId.Equals(value: articleId)) {
                        return;
                    }
                    Navigator.pushNamed(
                        context: buildContext, 
                        routeName: NavigatorRoutes.ArticleDetail,
                        new ArticleDetailScreenArguments {
                            id = articleId
                        }
                    );
                }
                else if (uri.AbsolutePath.StartsWith("/u/")) {
                    var userId = uri.AbsolutePath.Remove(0, count: "/u/".Length);
                    if (CTemporaryValue.currentPageModelId.isNotEmpty() &&
                        CTemporaryValue.currentPageModelId.Equals(value: userId)) {
                        return;
                    }
                    Navigator.pushNamed(
                        context: buildContext, 
                        routeName: NavigatorRoutes.UserDetail,
                        new UserDetailScreenArguments {
                            id = userId,
                            isSlug = true
                        }
                    );
                }
                else if (uri.AbsolutePath.StartsWith("/t/")) {
                    var teamId = uri.AbsolutePath.Remove(0, count: "/t/".Length);
                    if (CTemporaryValue.currentPageModelId.isNotEmpty() &&
                        CTemporaryValue.currentPageModelId.Equals(value: teamId)) {
                        return;
                    }
                    Navigator.pushNamed(
                        context: buildContext, 
                        routeName: NavigatorRoutes.TeamDetail,
                        new TeamDetailScreenArguments {
                            id = teamId,
                            isSlug = true
                        }
                    );
                }
                else if (uri.AbsolutePath.StartsWith("/ask/question/")) {
                    if (!uri.AbsolutePath.Contains("/answer/")) {
                        // question
                        var questionId = uri.AbsolutePath.Remove(0, count: "/ask/question/".Length);
                        if (CTemporaryValue.currentPageModelId.isNotEmpty() &&
                            CTemporaryValue.currentPageModelId.Equals(value: questionId)) {
                            return;
                        }
                        Navigator.pushNamed(
                            context: buildContext, 
                            routeName: NavigatorRoutes.QuestionDetail,
                            new ScreenArguments {
                                id = questionId
                            }
                        );
                    }
                    else {
                        // answer
                        var urlPath = uri.AbsolutePath.Remove(0, count: "/ask/question/".Length);
                        var questionIdAnswerId = urlPath.Replace("/answer/", "_");
                        var idStrings = questionIdAnswerId.Split('_');
                        if (idStrings.isNotNullAndEmpty() && idStrings.Length == 2) {
                            var questionId = idStrings.first();
                            var answerId = idStrings.last();
                            if (CTemporaryValue.currentPageModelId.isNotEmpty() &&
                                CTemporaryValue.currentPageModelId.Equals(value: answerId)) {
                                return;
                            }
                            Navigator.pushNamed(
                                context: buildContext, 
                                routeName: NavigatorRoutes.AnswerDetail,
                                new AnswerDetailScreenArguments {
                                    questionId = questionId, 
                                    answerId = answerId
                                }
                            );
                        }
                    }
                }
                else {
                    // open by default browser
                    Application.OpenURL(url: url);
                }
            }
            else {
                // open by default browser
                Application.OpenURL(url: url);
            }
        }
    }
}