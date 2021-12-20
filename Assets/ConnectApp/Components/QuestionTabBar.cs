using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace ConnectApp.Components {
    public class QuestionTabBar : StatelessWidget {
        public QuestionTabBar(
            bool isVote = false,
            int vote = 0,
            int comment = 0,
            GestureTapCallback likeCallback = null,
            GestureTapCallback followCallback = null,
            GestureTapCallback answerCallback = null,
            GestureTapCallback commentCallback = null,
            Key key = null
        ) : base(key: key) {
            this.vote = vote;
            this.isVote = isVote;
            this.comment = comment;
            this.likeCallback = likeCallback;
            this.followCallback = followCallback;
            this.answerCallback = answerCallback;
            this.commentCallback = commentCallback;
        }

        readonly int vote;
        readonly bool isVote;
        readonly int comment;
        readonly GestureTapCallback likeCallback;
        readonly GestureTapCallback followCallback;
        readonly GestureTapCallback answerCallback;
        readonly GestureTapCallback commentCallback;

        public override Widget build(BuildContext context) {
            var buttonWidth = CCommonUtils.getScreenWithoutPadding16Width(buildContext: context) / 3;
            return new Container(
                height: 49,
                padding: EdgeInsets.symmetric(horizontal: 16),
                decoration: new BoxDecoration(
                    border: new Border(new BorderSide(color: CColors.Separator)),
                    color: CColors.White
                ),
                child: new Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    crossAxisAlignment: CrossAxisAlignment.center,
                    children: new List<Widget> {
                        // 投票
                        new Container(
                            margin: EdgeInsets.only(right: 10),
                            child: new CustomButton(
                                padding: EdgeInsets.zero,
                                height: 32,
                                width: buttonWidth - 10,
                                decoration: new BoxDecoration(
                                    this.isVote ? CColors.White : CColors.TextBody,
                                    border: this.isVote
                                        ? new Border(
                                            new BorderSide(color: CColors.LoadingGrey, 2),
                                            new BorderSide(color: CColors.LoadingGrey, 2),
                                            new BorderSide(color: CColors.LoadingGrey, 2),
                                            new BorderSide(color: CColors.LoadingGrey, 2)
                                        )
                                        : null,
                                    borderRadius: BorderRadius.circular(2)
                                ),
                                onPressed: this.likeCallback,
                                child: new Row(
                                    mainAxisAlignment: MainAxisAlignment.center,
                                    crossAxisAlignment: CrossAxisAlignment.center,
                                    children: new List<Widget> {
                                        this.isVote
                                            ? new Container()
                                            : (Widget) new Icon(icon: CIcons.baseline_arrow_up_no_padding, size: 12,
                                                color: CColors.White),
                                        new Padding(
                                            padding: EdgeInsets.only(this.isVote ? 0 : 8),
                                            child: new Text(
                                                this.isVote
                                                    ? $"已投票 {CStringUtils.CountToString(count: this.vote)}"
                                                    : $"投票 {CStringUtils.CountToString(count: this.vote)}",
                                                style: this.isVote
                                                    ? CTextStyle.PMediumBody2.defaultHeight()
                                                    : CTextStyle.PMediumWhite
                                            )
                                        )
                                    }
                                )
                            )
                        ),
                        // 写回答
                        new CustomButton(
                            padding: EdgeInsets.zero,
                            height: 32,
                            width: buttonWidth,
                            decoration: new BoxDecoration(color: CColors.White),
                            onPressed: this.answerCallback,
                            child: new Row(
                                mainAxisAlignment: MainAxisAlignment.center,
                                crossAxisAlignment: CrossAxisAlignment.center,
                                children: new List<Widget> {
                                    new Icon(icon: CIcons.outline_create_answer, size: 24, color: CColors.TextBody2),
                                    new Padding(
                                        padding: EdgeInsets.only(8),
                                        child: new Text(
                                            "写回答",
                                            style: CTextStyle.PMediumBody2.defaultHeight()
                                        )
                                    )
                                }
                            )
                        ),
                        // 评论
                        new CustomButton(
                            padding: EdgeInsets.zero,
                            height: 32,
                            width: buttonWidth,
                            decoration: new BoxDecoration(color: CColors.White),
                            onPressed: this.commentCallback,
                            child: new Row(
                                mainAxisAlignment: MainAxisAlignment.center,
                                crossAxisAlignment: CrossAxisAlignment.center,
                                children: new List<Widget> {
                                    new Icon(icon: CIcons.outline_comment, size: 24, color: CColors.TextBody2),
                                    new Padding(
                                        padding: EdgeInsets.only(8),
                                        child: new Text(
                                            $"评论 {CStringUtils.CountToString(count: this.comment, "0")}",
                                            style: CTextStyle.PMediumBody2.defaultHeight()
                                        )
                                    )
                                }
                            )
                        )
                        // // 关注问题
                        // new CustomButton(
                        //     padding: EdgeInsets.zero,
                        //     height: 32,
                        //     width: buttonWidth,
                        //     decoration: new BoxDecoration(color: CColors.White),
                        //     onPressed: this.followCallback,
                        //     child: new Row(
                        //         mainAxisAlignment: MainAxisAlignment.center,
                        //         crossAxisAlignment: CrossAxisAlignment.center,
                        //         children: new List<Widget> {
                        //             new Icon(icon: CIcons.outline_follow_question, size: 24, color: CColors.TextBody2),
                        //             new Padding(
                        //                 padding: EdgeInsets.only(8),
                        //                 child: new Text(
                        //                     "关注问题",
                        //                     style: CTextStyle.PMediumBody2.defaultHeight()
                        //                 )
                        //             )
                        //         }
                        //     )
                        // )
                    }
                )
            );
        }
    }
}