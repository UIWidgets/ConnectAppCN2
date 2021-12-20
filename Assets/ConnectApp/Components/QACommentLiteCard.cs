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
    public class QACommentLiteCard : StatelessWidget {
        public QACommentLiteCard(
            NewMessage message,
            User author,
            float? avatarSize = null,
            int? contentMaxLine = null,
            bool isPraised = false,
            string userLicense = null,
            Color color = null,
            Widget separatorWidget = null,
            GestureTapCallback onTap = null,
            GestureTapCallback praiseCallBack = null,
            GestureTapCallback replyCallBack = null,
            Action<string> pushToUserDetail = null,
            Key key = null
        ) : base(key: key) {
            this.message = message;
            this.author = author;
            this.avatarSize = avatarSize ?? 24;
            this.contentMaxLine = contentMaxLine;
            this.isPraised = isPraised;
            this.userLicense = userLicense ?? "";
            this.color = color ?? CColors.White;
            this.separatorWidget = separatorWidget ?? new Container();
            this.onTap = onTap;
            this.praiseCallBack = praiseCallBack;
            this.replyCallBack = replyCallBack;
            this.pushToUserDetail = pushToUserDetail;
        }

        readonly NewMessage message;
        readonly User author;
        readonly float avatarSize;
        readonly int? contentMaxLine;
        readonly string userLicense;
        readonly bool isPraised;
        readonly Color color;
        readonly Widget separatorWidget;
        readonly GestureTapCallback onTap;
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
                                            margin: EdgeInsets.only(right: 8),
                                            child: Avatar.User(user: this.author, size: this.avatarSize)
                                        )
                                    ),
                                    new Expanded(
                                        child: new Container(
                                            child: new Column(
                                                crossAxisAlignment: CrossAxisAlignment.start,
                                                children: new List<Widget> {
                                                    new Padding(
                                                        padding: EdgeInsets.only(right: 16),
                                                        child: this._buildCommentAvatarName()
                                                    ),
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
            return new Container(
                height: 24,
                child: new Row(
                    children: new List<Widget> {
                        new Expanded(
                            child: new GestureDetector(
                                onTap: () => this.pushToUserDetail(obj: this.author.id),
                                child: new Row(
                                    children: new List<Widget> {
                                        new Flexible(
                                            child: new Text(
                                                data: this.author.fullName,
                                                style: textStyle,
                                                maxLines: 1,
                                                overflow: TextOverflow.ellipsis
                                            )
                                        ),
                                        CImageUtils.GenBadgeImage(
                                            badges: this.author.badges,
                                            license: this.userLicense,
                                            EdgeInsets.only(4)
                                        )
                                    }
                                )
                            )
                        )
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
                                        padding: EdgeInsets.only(8, 7, 16, 17),
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
                                        padding: EdgeInsets.only(8, 7, 16, 17),
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