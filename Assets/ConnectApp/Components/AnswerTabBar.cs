using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace ConnectApp.Components {
    public class AnswerTabBar : StatelessWidget {
        public AnswerTabBar(
            bool isVote = false,
            int vote = 0,
            int comment = 0,
            bool canAnswer = false,
            bool canEdit = false,
            GestureTapCallback likeCallback = null,
            GestureTapCallback answerCallback = null,
            GestureTapCallback commentCallback = null,
            Key key = null
        ) : base(key: key) {
            this.isVote = isVote;
            this.vote = vote;
            this.comment = comment;
            this.canAnswer = canAnswer;
            this.canEdit = canEdit;
            this.likeCallback = likeCallback;
            this.commentCallback = commentCallback;
            this.answerCallback = answerCallback;
        }

        readonly GestureTapCallback likeCallback;
        readonly GestureTapCallback commentCallback;
        readonly GestureTapCallback answerCallback;
        readonly bool isVote;
        readonly int vote;
        readonly int comment;
        readonly bool canAnswer;
        readonly bool canEdit;

        public override Widget build(BuildContext context) {
            var buttonWidth = CCommonUtils.getScreenWithoutPadding16Width(buildContext: context) / 3;
            // const int buttonWidth = 114;
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
                        // 赞成
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
                                                    ? $"已赞成 {CStringUtils.CountToString(count: this.vote)}"
                                                    : $"赞成 {CStringUtils.CountToString(count: this.vote)}",
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
                                            this.canEdit ? "编辑回答" : "写回答",
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
                    }
                )
            );
        }
    }
}