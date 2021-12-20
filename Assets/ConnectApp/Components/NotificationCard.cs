using System;
using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Models.Model;
using ConnectApp.screens;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;
using Notification = ConnectApp.Models.Model.Notification;

namespace ConnectApp.Components {
    public class NotificationCard : StatelessWidget {
        public NotificationCard(
            Notification notification,
            User user = null,
            Team team = null,
            List<User> mentions = null,
            Action onTap = null,
            Action<string> pushToUserDetail = null,
            Action<string> pushToTeamDetail = null,
            bool isLast = false,
            Key key = null,
            bool showCategory = false
        ) : base(key: key) {
            this.notification = notification;
            this.user = user;
            this.team = team;
            this.mentions = mentions;
            this.onTap = onTap;
            this.pushToUserDetail = pushToUserDetail;
            this.pushToTeamDetail = pushToTeamDetail;
            this.isLast = isLast;
            this.showCategory = showCategory;
        }

        readonly Notification notification;
        readonly User user;
        readonly Team team;
        readonly List<User> mentions;
        readonly Action onTap;
        readonly Action<string> pushToTeamDetail;
        readonly Action<string> pushToUserDetail;
        readonly bool isLast;
        const string TextSpacing = " ";
        readonly bool showCategory;

        public override Widget build(BuildContext context) {
            if (this.notification == null) {
                return new Container();
            }

            var type = this.notification.type;
            if (!NotificationDict.Instance.GetDict().ContainsKey(type)) {
                return new Container();
            }

            return new GestureDetector(
                onTap: () => this.onTap(),
                child: new Container(
                    color: CColors.White,
                    child: new Row(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: new List<Widget> {
                            this._buildNotificationAvatar(),
                            new Expanded(
                                child: new Container(
                                    padding: EdgeInsets.only(0, 16, 16, 16),
                                    decoration: new BoxDecoration(
                                        border: this.isLast
                                            ? null
                                            : new Border(bottom: new BorderSide(color: CColors.Separator2))
                                    ),
                                    child: new Column(
                                        mainAxisAlignment: MainAxisAlignment.start,
                                        crossAxisAlignment: CrossAxisAlignment.start,
                                        children: new List<Widget> {
                                            this._buildNotificationType(),
                                            this._buildNotificationTitle(),
                                            this._buildNotificationTime()
                                        }
                                    )
                                )
                            )
                        }
                    )
                )
            );
        }

        Widget _buildNotificationAvatar() {
            Widget avatar;
            GestureTapCallback onTap;
            if (this.user == null) {
                avatar = Avatar.Team(team: this.team, 48);
                onTap = () => this.pushToTeamDetail(obj: this.team.id);
            }
            else {
                if (this.user.id.Equals("unity-hub")) {
                    avatar = new ClipRRect(
                        borderRadius: BorderRadius.all(4),
                        child: Image.file(
                            "image/unity-icon-black.png",
                            width: 48,
                            height: 48,
                            fit: BoxFit.cover
                        )
                    );
                    onTap = null;
                }
                else {
                    avatar = Avatar.User(user: this.user, 48);
                    onTap = () => this.pushToUserDetail(obj: this.user.id);
                }
            }

            return new Container(
                padding: EdgeInsets.only(16, 16, 16),
                child: new GestureDetector(
                    onTap: onTap,
                    child: avatar
                )
            );
        }

        Widget _buildNotificationType() {
            if (!this.showCategory) {
                return new Container();
            }
            var notifAttr = NotificationDict.Instance.GetAttr(this.notification.type);
            var type = notifAttr != null ? notifAttr.parentTypeName : "";

            return new Container(
                child: new Text(
                    type,
                    style: CTextStyle.PSmallBody4
                )
            );
        }

        Widget _buildNotificationTitle() {
            var childrenWidgets = this._transformNotification(this.notification);
            return new Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: childrenWidgets
            );
        }

        Widget _buildNotificationTime() {
            var createdTime = this.notification.createdTime;
            return new Container(
                child: new Text(
                    DateConvert.DateStringFromNow(dt: createdTime),
                    style: CTextStyle.PSmallBody4
                )
            );
        }

        string _analyzeComment(string comment) {
            return comment.isNotEmpty()
                ? $" “{MessageUtils.AnalyzeMessage(content: comment, mentions: this.mentions, false)}”"
                : "";
        }

        List<Widget> _transformNotification(Notification notif) {
            var type = notif.type;
            var data = notif.data;
            NotificationAttr attr = NotificationDict.Instance.GetAttr(type);
            if (attr == null) {
                return new List<Widget>();
            }

            List<InlineSpan> textSpans = new List<InlineSpan>();
            if (attr.showOperator) {
                textSpans.Add(new TextSpan(
                    text: data.fullname,
                    style: CTextStyle.PLargeMedium,
                    recognizer: new TapGestureRecognizer {
                        onTap = () => this.pushToUserDetail(obj: data.userId)
                    }
                ));
                textSpans.Add(new TextSpan(text: TextSpacing));
            }
            else {
                if (type == "project_article_publish") {
                    string name;
                    GestureTapCallback onTap;
                    if (this.notification.data.role == "team") {
                        name = data.teamName;
                        onTap = () => this.pushToTeamDetail(obj: data.teamId);
                    }
                    else {
                        name = data.fullname;
                        onTap = () => this.pushToUserDetail(obj: data.userId);
                    }

                    textSpans.Add(new TextSpan(
                        text: name,
                        recognizer: new TapGestureRecognizer {
                            onTap = onTap
                        },
                        style: CTextStyle.PLargeMedium
                    ));
                    textSpans.Add(new TextSpan(text: TextSpacing));
                }
            }

            if (attr.operationName != null) {
                textSpans.Add(new TextSpan(
                    text: attr.operationName,
                    style: CTextStyle.PLargeBody2
                ));
            }

            Widget subTitleWidget = new Container();
            switch (type) {
                case "project_liked":
                case "like_article":
                    attr.target = $"《{data.projectTitle}》";
                    break;


                case "project_commented":
                    attr.target = $"《{data.projectTitle}》";
                    subTitleWidget = new Text(
                        this._analyzeComment(comment: data.comment),
                        style: CTextStyle.PLargeBody2,
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis
                    );
                    break;


                case "project_message_commented":
                    if (data.upperMessageId.isNotEmpty()) {
                        attr.target = this._analyzeComment(comment: data.upperComment);
                    }
                    else if (data.parentMessageId.isNotEmpty()) {
                        attr.target = this._analyzeComment(comment: data.parentComment);
                    }

                    subTitleWidget = new Text(
                        this._analyzeComment(comment: data.comment),
                        style: CTextStyle.PLargeBody2,
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis
                    );
                    break;


                case "project_participate_comment":
                    attr.target = $"《{data.projectTitle}》";
                    subTitleWidget = new Text(
                        this._analyzeComment(comment: data.comment),
                        style: CTextStyle.PLargeBody2,
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis
                    );
                    break;


                case "project_message_participate_liked":
                case "project_message_liked":
                    attr.target = this._analyzeComment(comment: data.comment);
                    break;


                case "team_followed":
                    textSpans.Add(new TextSpan(
                        text: data.teamName,
                        recognizer: new TapGestureRecognizer {
                            onTap = () => this.pushToTeamDetail(obj: data.teamId)
                        },
                        style: CTextStyle.PLargeBlue
                    ));
                    break;


                case "project_article_publish":
                    attr.target = $"《{data.projectTitle}》";
                    break;

                case "chatwoot_message_unity_hub":
                    textSpans.Add(new TextSpan(
                        " Unity Hub 客服 ",
                        style: CTextStyle.PLargeMedium
                    ));
                    textSpans.Add(new TextSpan(
                        "发来新消息: ",
                        style: CTextStyle.PLargeBody2
                    ));
                    textSpans.Add(new TextSpan(
                        text: data.content,
                        style: CTextStyle.PLargeMedium
                    ));
                    break;

                case "comment_reply_comment":
                    if (data.messageToReply.isNotEmpty()) {
                        attr.target = "\"" + data.messageToReply + "\"";
                    }

                    if (data.message.isNotEmpty()) {
                        subTitleWidget = new Text(
                            this._analyzeComment(comment: data.message),
                            style: CTextStyle.PLargeBody2,
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis
                        );
                    }

                    break;

                case "followed_publish_article":
                case "answer_followed_question":
                case "resolved_followed_question":
                case "answer_question":
                case "answer_adopted":
                case "answer_cancel_adopted":
                case "like_question":
                case "like_answer":
                case "answer_commented_question":
                case "answer_answered_question":
                    if (data.message.isNotEmpty()) {
                        attr.target = $"《{data.message}》";
                    }

                    break;

                case "comment_question":
                case "comment_answer":
                case "comment_article":
                case "comment_commented_question":
                case "comment_commented_answer":
                    if (data.messageToReply.isNotEmpty()) {
                        attr.target = $"《{data.messageToReply}》";
                    }

                    if (data.message.isNotEmpty()) {
                        subTitleWidget = new Text(
                            this._analyzeComment(comment: data.message),
                            style: CTextStyle.PLargeBody2,
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis
                        );
                    }

                    break;

                case "like_comment":
                    if (data.message.isNotEmpty()) {
                        attr.target = "\"" + data.message + "\"";
                    }

                    break;

                case "system_published_question_deleted_admin":
                case "system_answered_question_deleted":
                case "system_commented_question_deleted":
                    if (data.message.isNotEmpty()) {
                        attr.target = $"《{data.message}》";
                    }

                    if (attr.operationResult.isNotEmpty()) {
                        subTitleWidget = new Text(
                            attr.operationResult,
                            style: CTextStyle.PRegularBody4,
                            overflow: TextOverflow.ellipsis
                        );
                    }

                    break;
            }

            if (attr.target != null) {
                textSpans.Add(new TextSpan(
                    text: attr.target,
                    style: CTextStyle.PLargeMedium
                ));
            }

            List<Widget> children  = new List<Widget> {
                new RichText(
                    maxLines: 3,
                    text: new TextSpan(
                        children: textSpans
                    ),
                    overflow: TextOverflow.ellipsis
                ),
                subTitleWidget
            };
            return children;
        }
        
    }
    
}