using System;
using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Models.Model;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace ConnectApp.Components {
    public class AnswerCard : StatelessWidget {
        public AnswerCard(
            Answer answer,
            User user,
            List<string> images,
            Dictionary<string, UserLicense> userLicenseDict = null,
            GestureTapCallback onTap = null,
            GestureTapCallback avatarCallBack = null,
            GestureTapCallback followCallBack = null,
            GestureTapCallback likeCallBack = null,
            GestureTapCallback commentCallBack = null,
            GestureTapCallback moreCallBack = null,
            Key key = null
        ) : base(key: key) {
            this.answer = answer;
            this.user = user;
            this.images = images;
            this.userLicenseDict = userLicenseDict;
            this.onTap = onTap;
            this.avatarCallBack = avatarCallBack;
            this.followCallBack = followCallBack;
            this.likeCallBack = likeCallBack;
            this.commentCallBack = commentCallBack;
            this.moreCallBack = moreCallBack;
        }

        readonly Answer answer;
        readonly User user;
        readonly List<string> images;
        readonly Dictionary<string, UserLicense> userLicenseDict;
        readonly GestureTapCallback onTap;
        readonly GestureTapCallback avatarCallBack;
        readonly GestureTapCallback followCallBack;
        readonly GestureTapCallback likeCallBack;
        readonly GestureTapCallback commentCallBack;
        readonly GestureTapCallback moreCallBack;

        public override Widget build(BuildContext context) {
            if (this.answer == null) {
                return new Container();
            }

            return new Container(
                color: CColors.White,
                child: new Column(
                    children: new List<Widget> {
                        new GestureDetector(
                            onTap: this.onTap,
                            child: new Container(
                                color: CColors.White,
                                padding: EdgeInsets.all(16),
                                child: new Column(
                                    crossAxisAlignment: CrossAxisAlignment.start,
                                    children: new List<Widget> {
                                        this._buildAvatar(),
                                        this._buildAnswerInfo(buildContext: context),
                                        this._buildBottom()
                                    }
                                )
                            )
                        ),
                        new CustomDivider(
                            color: CColors.Background,
                            height: 8
                        )
                    }
                )
            );
        }

        Widget _buildAvatar() {
            var user = this.user;
            return new Container(
                child: new Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: new List<Widget> {
                        new GestureDetector(
                            child: Avatar.User(
                                user: user,
                                24
                            ),
                            onTap: this.avatarCallBack
                        ),
                        new Expanded(
                            child: new GestureDetector(
                                onTap: this.avatarCallBack,
                                child: new Container(
                                    color: CColors.Transparent,
                                    margin: EdgeInsets.only(8, right: 16),
                                    child: new Column(
                                        crossAxisAlignment: CrossAxisAlignment.start,
                                        children: new List<Widget> {
                                            new Row(
                                                children: new List<Widget> {
                                                    new Flexible(
                                                        child: new Text(
                                                            user.fullName ?? user.name ?? "佚名",
                                                            style: CTextStyle.PMediumBody3.defaultHeight(),
                                                            maxLines: 1,
                                                            overflow: TextOverflow.ellipsis
                                                        )
                                                    ),
                                                    CImageUtils.GenBadgeImage(
                                                        badges: user.badges,
                                                        CCommonUtils.GetUserLicense(
                                                            userId: user.id,
                                                            userLicenseMap: this.userLicenseDict
                                                        ),
                                                        EdgeInsets.only(4)
                                                    )
                                                }
                                            )
                                        }
                                    )
                                )
                            )
                        )
                    }
                )
            );
        }

        Widget _buildBestAnswerBanner() {
            if (!this.answer.isAccepted) {
                return new Container();
            }

            return new Container(
                height: 40,
                margin: EdgeInsets.only(bottom: 16),
                decoration: new BoxDecoration(
                    CColors.BestAnswerText.withOpacity(0.1f),
                    borderRadius: BorderRadius.all(2)
                ),
                child: new Padding(
                    padding: EdgeInsets.symmetric(horizontal: 12),
                    child: new Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        crossAxisAlignment: CrossAxisAlignment.center,
                        children: new List<Widget> {
                            new Row(
                                children: new List<Widget> {
                                    new Text(
                                        "该回答被作者标记为 ",
                                        style: new TextStyle(
                                            fontSize: 14,
                                            fontFamily: "Roboto-Regular",
                                            color: CColors.BestAnswerText
                                        )
                                    ),
                                    new Text(
                                        "已解决",
                                        style: new TextStyle(
                                            fontSize: 14,
                                            fontFamily: "Roboto-Medium",
                                            color: CColors.BestAnswerText
                                        )
                                    )
                                }
                            ),
                            new Icon(icon: CIcons.round_check_circle, size: 18, color: CColors.BestAnswerText)
                        }
                    )
                )
            );
        }

        Widget _buildImages(BuildContext buildContext) {
            var imagesWidth = CCommonUtils.getScreenWithoutPadding16Width(buildContext: buildContext);
            const int imagePadding = 2;
            const int borderRadius = 2;
            var images = this.images;
            if (images.isNullOrEmpty()) {
                return new Container();
            }

            Widget imagesWidget = new Container();

            switch (images.Count) {
                case 1: {
                    var imageWidth = imagesWidth;
                    var imageHeight = imagesWidth / 2;
                    var image = images.first();
                    imagesWidget = new PlaceholderImage(
                        imageUrl: image,
                        width: imageWidth,
                        height: imageHeight,
                        borderRadius: borderRadius,
                        fit: BoxFit.cover,
                        color: CColorUtils.GetSpecificDarkColorFromId(id: image)
                    );
                    break;
                }
                case 2: {
                    var imageWidth = (imagesWidth - imagePadding) / 2;
                    var imageHeight = imageWidth * 2 / 3;
                    var firstImage = images.first();
                    var lastImage = images.last();
                    imagesWidget = new ClipRRect(
                        borderRadius: BorderRadius.all(radius: borderRadius),
                        child: new Row(
                            children: new List<Widget> {
                                new PlaceholderImage(
                                    imageUrl: firstImage,
                                    width: imageWidth,
                                    height: imageHeight,
                                    fit: BoxFit.cover,
                                    color: CColorUtils.GetSpecificDarkColorFromId(id: firstImage)
                                ),
                                new SizedBox(width: 2),
                                new PlaceholderImage(
                                    imageUrl: lastImage,
                                    width: imageWidth,
                                    height: imageHeight,
                                    fit: BoxFit.cover,
                                    color: CColorUtils.GetSpecificDarkColorFromId(id: lastImage)
                                )
                            }
                        )
                    );
                    break;
                }
                default: {
                    if (images.Count >= 3) {
                        var overCount = images.Count - 3;
                        var imageWidth = (imagesWidth - imagePadding * 2) / 3;
                        var imageHeight = imageWidth * 2 / 3;
                        var firstImage = images[0];
                        var secondImage = images[1];
                        var thirdImage = images[2];
                        imagesWidget = new ClipRRect(
                            borderRadius: BorderRadius.all(radius: borderRadius),
                            child: new Row(
                                children: new List<Widget> {
                                    new PlaceholderImage(
                                        imageUrl: firstImage,
                                        width: imageWidth,
                                        height: imageHeight,
                                        fit: BoxFit.cover,
                                        color: CColorUtils.GetSpecificDarkColorFromId(id: firstImage)
                                    ),
                                    new SizedBox(width: 2),
                                    new PlaceholderImage(
                                        imageUrl: secondImage,
                                        width: imageWidth,
                                        height: imageHeight,
                                        fit: BoxFit.cover,
                                        color: CColorUtils.GetSpecificDarkColorFromId(id: secondImage)
                                    ),
                                    new SizedBox(width: 2),
                                    new Stack(
                                        children: new List<Widget> {
                                            new PlaceholderImage(
                                                imageUrl: thirdImage,
                                                width: imageWidth,
                                                height: imageHeight,
                                                fit: BoxFit.cover,
                                                color: CColorUtils.GetSpecificDarkColorFromId(id: thirdImage)
                                            ),
                                            Positioned.fill(
                                                overCount > 0
                                                    ? new Container(
                                                        alignment: Alignment.center,
                                                        color: CColors.Black.withOpacity(0.3f),
                                                        child: new Text(
                                                            $"+{overCount}",
                                                            style: CTextStyle.PXLargeMediumWhite.defaultHeight()
                                                        )
                                                    )
                                                    : new Container()
                                            )
                                        }
                                    )
                                }
                            )
                        );
                    }

                    break;
                }
            }

            return new Padding(
                padding: EdgeInsets.only(top: 16),
                child: imagesWidget
            );
        }

        Widget _buildAnswerInfo(BuildContext buildContext) {
            return new Container(
                padding: EdgeInsets.only(top: 16),
                child: new Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: new List<Widget> {
                        new Container(
                            child: new Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: new List<Widget> {
                                    this._buildBestAnswerBanner(),
                                    new Text(
                                        data: this.answer.descriptionPlain,
                                        style: CTextStyle.PLargeBody2,
                                        maxLines: 3,
                                        overflow: TextOverflow.ellipsis
                                    ),
                                    this._buildImages(buildContext: buildContext)
                                }
                            )
                        )
                    }
                )
            );
        }

        Widget _buildBottom() {
            return new Padding(
                padding: EdgeInsets.only(top: 16),
                child: new Text(
                    $"赞成 {CStringUtils.CountToString(count: this.answer.likeCount, "0")}" +
                    $" · 评论 {CStringUtils.CountToString(count: this.answer.commentCount, "0")}" +
                    $" · {DateConvert.DateStringFromNow(this.answer.createdTime ?? DateTime.Now)}",
                    style: CTextStyle.PRegularBody4.defaultHeight()
                )
            );
        }
    }
}