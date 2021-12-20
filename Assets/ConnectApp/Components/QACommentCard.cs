using System;
using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Models.Model;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Image = Unity.UIWidgets.widgets.Image;

namespace ConnectApp.Components {
    public class QACommentCard : StatelessWidget {
        public QACommentCard(
            NewMessage message,
            User author,
            User byReplyUser = null,
            float? avatarSize = null,
            int? contentMaxLine = null,
            bool isPraised = false,
            string userLicense = null,
            Color color = null,
            Widget separatorWidget = null,
            GestureTapCallback onTap = null,
            GestureTapCallback moreCallBack = null,
            GestureTapCallback praiseCallBack = null,
            GestureTapCallback replyCallBack = null,
            Action<string> pushToUserDetail = null,
            Key key = null
        ) : base(key: key) {
            this.message = message;
            this.author = author;
            this.byReplyUser = byReplyUser;
            this.avatarSize = avatarSize ?? 24;
            this.contentMaxLine = contentMaxLine;
            this.isPraised = isPraised;
            this.userLicense = userLicense ?? "";
            this.color = color ?? CColors.White;
            this.separatorWidget = separatorWidget ?? new Container();
            this.onTap = onTap;
            this.moreCallBack = moreCallBack;
            this.praiseCallBack = praiseCallBack;
            this.replyCallBack = replyCallBack;
            this.pushToUserDetail = pushToUserDetail;
        }

        readonly NewMessage message;
        readonly User author;
        readonly User byReplyUser;
        readonly float avatarSize;
        readonly int? contentMaxLine;
        readonly string userLicense;
        readonly bool isPraised;
        readonly Color color;
        readonly Widget separatorWidget;
        readonly GestureTapCallback onTap;
        readonly GestureTapCallback moreCallBack;
        readonly GestureTapCallback praiseCallBack;
        readonly GestureTapCallback replyCallBack;
        readonly Action<string> pushToUserDetail;

        public override Widget build(BuildContext context) {
            if (this.message == null) {
                return new Container();
            }

            return new GestureDetector(
                onTap: this.onTap,
                child: new Column(
                    children: new List<Widget> {
                        new Container(
                            color: this.color,
                            padding: EdgeInsets.only(16),
                            child: new Row(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: new List<Widget> {
                                    new GestureDetector(
                                        onTap: () => this.pushToUserDetail(obj: this.author.id),
                                        child: new Container(
                                            margin: EdgeInsets.only(right: 8, top: 12),
                                            child: Avatar.User(user: this.author, size: this.avatarSize)
                                        )
                                    ),
                                    new Expanded(
                                        child: new Container(
                                            child: new Column(
                                                crossAxisAlignment: CrossAxisAlignment.start,
                                                children: new List<Widget> {
                                                    this._buildCommentAvatarName(),
                                                    new Padding(
                                                        padding: EdgeInsets.only(right: 16),
                                                        child: this._buildCommentContent()
                                                    ),
                                                    this._buildBottomBar()
                                                }
                                            )
                                        )
                                    )
                                }
                            )
                        ),
                        this.separatorWidget
                    }
                )
            );
        }

        Widget _buildCommentAvatarName() {
            var isMe = UserInfoManager.isMe(userId: this.author.id);
            var textStyle = CTextStyle.PMediumBody3.defaultHeight();

            var firstWidget = new GestureDetector(
                onTap: () => { this.pushToUserDetail(obj: this.author.id); },
                child: new Text(
                    data: this.author.fullName,
                    style: textStyle,
                    overflow: TextOverflow.ellipsis
                )
            );

            Widget middleWidget, lastWidget;
            if (this.byReplyUser != null && this.byReplyUser.id.isNotEmpty()) {
                middleWidget = new Icon(icon: CIcons.round_arrow_right, size: 22, color: CColors.Icon);
                lastWidget = new GestureDetector(
                    onTap: () => this.pushToUserDetail(obj: this.byReplyUser.id),
                    child: new Text(
                        this.byReplyUser.fullName ?? "",
                        style: textStyle,
                        maxLines: 1,
                        overflow: TextOverflow.ellipsis
                    )
                );
            }
            else {
                middleWidget = CImageUtils.GenBadgeImage(
                    badges: this.author.badges,
                    license: this.userLicense,
                    EdgeInsets.only(4)
                );
                lastWidget = new Container();
            }


            return new Container(
                child: new Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: new List<Widget> {
                        new Container(
                            padding: EdgeInsets.only(top: 14, bottom: 2),
                            constraints: new BoxConstraints(
                                maxWidth: 150
                            ),
                            child: firstWidget
                        ),
                        new Container(
                            padding: EdgeInsets.only(top: 12, bottom: 2),
                            child: middleWidget
                        ),
                        new Expanded(
                            child: new Container(
                                padding: EdgeInsets.only(top: 14, bottom: 2),
                                child: lastWidget
                            )
                        ),
                        !isMe
                            ? new CustomButton(
                                padding: EdgeInsets.only(16, 13, 16),
                                onPressed: this.moreCallBack,
                                child: new Icon(
                                    icon: CIcons.ellipsis,
                                    size: 20,
                                    color: CColors.BrownGrey
                                )
                            )
                            : (Widget) new Container(width: 16)
                    }
                )
            );
        }

        Widget _buildCommentContent() {
            return new TipMenu(
                new List<TipMenuItem> {
                    new TipMenuItem("复制", () => Clipboard.setData(new ClipboardData(text: this.message.content)))
                },
                new Container(
                    child: new Text(
                        data: this.message.content,
                        style: CTextStyle.PRegularBody.copyWith(height: 1.57f),
                        maxLines: this.contentMaxLine,
                        overflow: this.contentMaxLine != null ? TextOverflow.ellipsis : (TextOverflow?) null
                    )
                )
            );
        }

        Widget _buildBottomBar() {
            return new Container(
                child: new Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: new List<Widget> {
                        new Padding(
                            padding: EdgeInsets.only(top: 10),
                            child: new Text(
                                $"{DateConvert.DateStringFromNonce(nonce: this.message.nonce)}",
                                style: CTextStyle.PSmallBody4.defaultHeight()
                            )
                        ),
                        new Container(
                            child: new Row(
                                children: new List<Widget> {
                                    new CustomButton(
                                        onPressed: this.praiseCallBack,
                                        padding: EdgeInsets.only(8, 7, 16, 13),
                                        child: new Row(
                                            children: new List<Widget> {
                                                new Text(
                                                    CStringUtils.CountToString(count: this.message.likeCount),
                                                    style: CTextStyle.PSmallBody4.defaultHeight()
                                                ),
                                                new SizedBox(width: 4),
                                                Image.file(
                                                    this.isPraised ? "image/like-colorful.png" : "image/like-grey.png",
                                                    height: 18,
                                                    width: 18
                                                )
                                            }
                                        )
                                    ),
                                    new CustomButton(
                                        onPressed: this.replyCallBack,
                                        padding: EdgeInsets.only(8, 7, 16, 13),
                                        child: new Row(
                                            children: new List<Widget> {
                                                new Text(
                                                    CStringUtils.CountToString(0),
                                                    style: CTextStyle.PSmallBody4.defaultHeight()
                                                ),
                                                new SizedBox(width: 4),
                                                new Icon(icon: CIcons.outline_comment, size: 18, color: CColors.Icon)
                                            }
                                        )
                                    )
                                }
                            )
                        )
                    }
                )
            );
        }
    }
}