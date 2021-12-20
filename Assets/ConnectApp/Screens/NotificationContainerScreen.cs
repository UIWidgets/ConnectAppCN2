using System.Collections.Generic;
using ConnectApp.Common.Other;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Components;
using ConnectApp.Models.ActionModel;
using ConnectApp.Models.Model;
using ConnectApp.Models.State;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.widgets;

namespace ConnectApp.screens {
    public class NotificationContainerScreenConnector : StatelessWidget {
        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, object>(
                converter: state => null,
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new NotificationContainerScreenActionModel();
                    return new NotificationContainerScreen(actionModel: actionModel);
                }
            );
        }
    }

    public class NotificationContainerScreen : StatefulWidget {
        public NotificationContainerScreen(
            NotificationContainerScreenActionModel actionModel = null,
            Key key = null
        ) : base(key: key) {
            this.actionModel = actionModel;
        }

        public readonly NotificationContainerScreenActionModel actionModel;

        public override State createState() {
            return new _NotificationContainerScreenState();
        }
    }

    class _NotificationContainerScreenState : State<NotificationContainerScreen> {

        public override void initState() {
            base.initState();
            StatusBarManager.statusBarStyle(false);
        }

        public override Widget build(BuildContext context) {
            return new Container(
                color: CColors.White,
                child: new CustomSafeArea(
                    bottom: false,
                    child: new Container(
                        color: CColors.White,
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
                    "通知",
                    style: CTextStyle.H2
                ),
                onBack: () => Navigator.pop(context: this.context)
            );
        }

        static readonly List<object> tabsTitles = new List<object> {
            "全部通知", "我的关注", "与我相关", "我参与的", "系统通知"
        };

        Widget _buildContentView() {
            var childWidgets = new List<Widget>();
            childWidgets.Add(new NotificationScreenConnector(category: NotificationCategory.All));
            childWidgets.Add(new NotificationScreenConnector(category: NotificationCategory.Follow));
            childWidgets.Add(new NotificationScreenConnector(category: NotificationCategory.Involve));
            childWidgets.Add(new NotificationScreenConnector(category: NotificationCategory.Participate));
            childWidgets.Add(new NotificationScreenConnector(category: NotificationCategory.System));

            return new CustomSegmentedControl(
                items: tabsTitles,
                children: childWidgets
            );
        }
    }

    public sealed class NotificationDict {
        static readonly NotificationDict instance = new NotificationDict();
        Dictionary<string, NotificationAttr> dict;

        static NotificationDict() {
        }

        NotificationDict() {
        }

        public static NotificationDict Instance {
            get { return instance; }
        }

        public Dictionary<string, NotificationAttr> GetDict() {
            return this.dict ?? (this.dict = this.BuildNotificationDict());
        }

        public NotificationAttr GetAttr(string key) {
            this.GetDict().TryGetValue(key: key, out var attr);
            return attr;
        }

        Dictionary<string,NotificationAttr> BuildNotificationDict() {
            var dict = new Dictionary<string, NotificationAttr>();
            dict.Add("project_liked",new NotificationAttr {type = "project_liked", operationName = "赞了你的文章", parentType = 2, parentTypeName = "与我相关", showOperator = true});
            dict.Add("project_commented",new NotificationAttr {type = "project_commented", operationName = "评价了你的文章", parentType = 2, parentTypeName = "与我相关", showOperator = true});
            dict.Add("project_message_commented",new NotificationAttr {type = "project_message_commented", operationName = "回复了你", parentType = 2, parentTypeName = "与我相关", showOperator = true});
            dict.Add("project_participate_comment",new NotificationAttr {type = "project_participate_comment", operationName = "评价了你关注的文章", parentType = 3, parentTypeName = "我参与的", showOperator = true});
            dict.Add("project_message_liked",new NotificationAttr {type = "project_message_liked", operationName = "赞了你的评论", parentType = 2, parentTypeName = "与我相关", showOperator = true});
            dict.Add("project_message_participate_liked",new NotificationAttr {type = "project_message_participate_liked", operationName = "赞了你关注的评论", parentType = 3, parentTypeName = "我参与的", showOperator = true});
            dict.Add("project_article_publish",new NotificationAttr {type = "project_article_publish", operationName = "发布了新文章", parentType = 1, parentTypeName = "我的关注"});
            dict.Add("team_followed",new NotificationAttr {type = "team_followed", operationName = "关注了", parentType = 1, parentTypeName = "我的关注"});
            dict.Add("chatwoot_message_unity_hub",new NotificationAttr {type = "chatwoot_message_unity_hub", operationName = "Unity Hub 客服", parentType = 2, parentTypeName = "与我相关"});
            // follow notification
            dict.Add("followed_publish_article",new NotificationAttr {type = "followed_publish_article", operationName = "你关注的人发布了新的文章", parentType = 1, parentTypeName = "我的关注"});
            dict.Add("answer_followed_question",new NotificationAttr {type = "answer_followed_question", operationName = "你关注的问题有了新的回答", parentType = 1, parentTypeName = "我的关注"});
            dict.Add("resolved_followed_question",new NotificationAttr {type = "resolved_followed_question", operationName = "你关注的问题已被解决", parentType = 1, parentTypeName = "我的关注"});
            // related to me notification
            dict.Add("followed",new NotificationAttr {type = "followed", operationName = "关注了你", parentType = 2, parentTypeName = "与我相关", showOperator = true});
            dict.Add("answer_question",new NotificationAttr {type = "answer_question", operationName = "回答了你的问题", parentType = 2, parentTypeName = "与我相关", showOperator = true});
            dict.Add("answer_adopted",new NotificationAttr {type = "answer_adopted", operationName = "采纳了你的回答", parentType = 2, parentTypeName = "与我相关", showOperator = true});
            dict.Add("answer_cancel_adopted",new NotificationAttr {type = "answer_cancel_adopted", operationName = "取消采纳了你的回答", parentType = 2, parentTypeName = "与我相关", showOperator = true});
            dict.Add("comment_reply_comment",new NotificationAttr {type = "comment_reply_comment", operationName = "回复了你的评论", parentType = 2, parentTypeName = "与我相关", showOperator = true});
            dict.Add("comment_question",new NotificationAttr {type = "comment_question", operationName = "评论了你的问题", parentType = 2, parentTypeName = "与我相关", showOperator = true});
            dict.Add("comment_answer",new NotificationAttr {type = "comment_answer", operationName = "评论了你的回答", parentType = 2, parentTypeName = "与我相关", showOperator = true});
            dict.Add("comment_article",new NotificationAttr {type = "comment_article", operationName = "评论了你的文章", parentType = 2, parentTypeName = "与我相关", showOperator = true});
            dict.Add("like_article",new NotificationAttr {type = "like_article", operationName = "赞了你的文章", parentType = 2, parentTypeName = "与我相关", showOperator = true});
            dict.Add("like_question",new NotificationAttr {type = "like_question", operationName = "赞了你的问题", parentType = 2, parentTypeName = "与我相关", showOperator = true});
            dict.Add("like_answer",new NotificationAttr {type = "like_answer", operationName = "赞了你的回答", parentType = 2, parentTypeName = "与我相关", showOperator = true});
            dict.Add("like_comment",new NotificationAttr {type = "like_comment", operationName = "赞了你的评论", parentType = 2, parentTypeName = "与我相关", showOperator = true});
            // i participated in notification
            dict.Add("answer_commented_question",new NotificationAttr {type = "answer_commented_question", operationName = "回答了你评论过的问题", parentType = 3, parentTypeName = "我参与的", showOperator = true});
            dict.Add("answer_answered_question",new NotificationAttr {type = "answer_answered_question", operationName = "回答了你回答过的问题", parentType = 3, parentTypeName = "我参与的", showOperator = true});
            dict.Add("comment_commented_question",new NotificationAttr {type = "comment_commented_question", operationName = "评论了你评论过的问题", parentType = 3, parentTypeName = "我参与的", showOperator = true});
            dict.Add("comment_commented_answer",new NotificationAttr {type = "comment_commented_answer", operationName = "评论了你评论过的回答", parentType = 3, parentTypeName = "我参与的", showOperator = true});
            // system notification
            dict.Add("system_published_question_deleted_admin",new NotificationAttr {type = "system_published_question_deleted_admin", operationName = "你发布的问题", operationResult = "已被管理员删除", parentType = 4, parentTypeName = "系统通知"});
            dict.Add("system_answered_question_deleted",new NotificationAttr {type = "system_answered_question_deleted", operationName = "你回答过的问题",operationResult = "已被删除", parentType = 4, parentTypeName = "系统通知"});
            dict.Add("system_commented_question_deleted",new NotificationAttr {type = "system_commented_question_deleted", operationName = "你参与评论过的问题", operationResult = "已被删除", parentType = 4, parentTypeName = "系统通知"});
            return dict;
        }
    }
}