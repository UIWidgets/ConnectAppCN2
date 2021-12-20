using System.Collections.Generic;
using ConnectApp.Common.Visual;
using ConnectApp.Plugins;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace ConnectApp.Components {
    public enum ShareSheetItemType {
        friends,
        moments,
        miniProgram,
        clipBoard,
        block,
        report,
        edit
    }

    public enum ShareSheetStyle {
        doubleDeck,
        singleDeck
    }

    public delegate void OnShareType(ShareSheetItemType sheetItemType);

    public class ShareView : StatelessWidget {
        public ShareView(
            Key key = null,
            BuildContext buildContext = null,
            OnShareType onPressed = null,
            ShareSheetStyle shareSheetStyle = ShareSheetStyle.doubleDeck,
            bool showReportAndBlock = true,
            bool canEdit = false
        ) : base(key: key) {
            this.buildContext = buildContext;
            this.onPressed = onPressed;
            this.shareSheetStyle = shareSheetStyle;
            this.showReportAndBlock = showReportAndBlock;
            this.canEdit = canEdit;
        }

        readonly BuildContext buildContext;
        readonly OnShareType onPressed;
        readonly ShareSheetStyle shareSheetStyle;
        readonly bool showReportAndBlock;
        readonly bool canEdit;

        public override Widget build(BuildContext context) {
            var mediaQueryData = MediaQuery.of(context: context);
            return new Container(
                child: new Column(
                    mainAxisAlignment: MainAxisAlignment.end,
                    children: new List<Widget> {
                        new Container(
                            decoration: new BoxDecoration(
                                color: CColors.White,
                                borderRadius: BorderRadius.only(12, 12)
                            ),
                            child: new Column(
                                mainAxisAlignment: MainAxisAlignment.start,
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: new List<Widget> {
                                    new Container(
                                        padding: EdgeInsets.all(16),
                                        child: new Text("分享至", style: CTextStyle.PRegularBody4)
                                    ),
                                    new Container(color: CColors.Separator2, height: 1),
                                    new Container(
                                        padding: EdgeInsets.symmetric(16),
                                        child: new Row(
                                            mainAxisAlignment: MainAxisAlignment.start,
                                            children: this._buildShareItems(context: context)
                                        )
                                    ),
                                    new Container(color: CColors.Separator2, height: 1),
                                    this.shareSheetStyle == ShareSheetStyle.doubleDeck && this.showReportAndBlock
                                        ? new Container(
                                            child: new Column(
                                                children: new List<Widget> {
                                                    new Container(
                                                        padding: EdgeInsets.symmetric(16),
                                                        child: new Row(
                                                            mainAxisAlignment: MainAxisAlignment.start,
                                                            children: this._buildOtherItems(context: context)
                                                        )
                                                    ),
                                                    new Container(color: CColors.Separator2, height: 1)
                                                }
                                            )
                                        )
                                        : new Container(),
                                    new GestureDetector(
                                        onTap: () => {
                                            Navigator.pop(context: this.buildContext);
                                        },
                                        child: new Container(
                                            height: 49,
                                            color: CColors.Transparent,
                                            alignment: Alignment.center,
                                            child: new Text("取消",
                                                style: CTextStyle.PLargeBody
                                            )
                                        )
                                    ),
                                    new Container(color: CColors.White, height: mediaQueryData.padding.bottom)
                                }
                            )
                        )
                    }
                )
            );
        }

        List<Widget> _buildShareItems(BuildContext context) {
            var shareItems = new List<Widget>();
            if (WechatPlugin.instance().isInstalled()) {
                shareItems.Add(
                    _buildShareItem(
                        context: context,
                        icon: CIcons.WechatIcon,
                        "微信好友",
                        color: CColors.White,
                        background: CColors.WeChatGreen,
                        () => this.onPressed(sheetItemType: ShareSheetItemType.friends)
                    )
                );
                shareItems.Add(
                    _buildShareItem(
                        context: context,
                        icon: CIcons.WechatMoment,
                        "朋友圈",
                        color: CColors.White,
                        background: CColors.WeChatGreen,
                        () => this.onPressed(sheetItemType: ShareSheetItemType.moments)
                    )
                );
            }

            shareItems.Add(
                _buildShareItem(
                    context: context,
                    icon: CIcons.insert_link,
                    "复制链接",
                    color: CColors.White,
                    background: CColors.PrimaryBlue,
                    () => this.onPressed(sheetItemType: ShareSheetItemType.clipBoard)
                )
            );
            return shareItems;
        }

        List<Widget> _buildOtherItems(BuildContext context) {
            if (this.canEdit) {
                return new List<Widget> {
                    _buildShareItem(
                        context: context,
                        icon: CIcons.outline_create_answer,
                        "编辑",
                        color: CColors.BrownGrey,
                        background: CColors.White,
                        () => this.onPressed(sheetItemType: ShareSheetItemType.edit),
                        true
                    )
                };
            }

            return new List<Widget> {
                _buildShareItem(
                    context: context,
                    icon: CIcons.block,
                    "屏蔽",
                    color: CColors.BrownGrey,
                    background: CColors.White,
                    () => this.onPressed(sheetItemType: ShareSheetItemType.block),
                    true
                ),
                _buildShareItem(
                    context: context,
                    icon: CIcons.report,
                    "举报",
                    color: CColors.BrownGrey,
                    background: CColors.White,
                    () => this.onPressed(sheetItemType: ShareSheetItemType.report),
                    true
                )
            };
        }

        static Widget _buildShareItem(BuildContext context, IconData icon, string title, Color color, Color background,
            GestureTapCallback onTap, bool isBorder = false) {
            var border = isBorder
                ? Border.all(
                    Color.fromRGBO(216, 216, 216, 1)
                )
                : null;
            return new GestureDetector(
                onTap: () => {
                    if (Navigator.canPop(context: context)) {
                        Navigator.pop<object>(context: context);
                    }

                    onTap();
                },
                child: new Container(
                    padding: EdgeInsets.symmetric(horizontal: 16),
                    color: CColors.White,
                    child: new Column(children: new List<Widget> {
                        new Container(
                            width: 48,
                            height: 48,
                            decoration: new BoxDecoration(
                                color: background,
                                borderRadius: BorderRadius.all(24),
                                border: border
                            ),
                            child: new Icon(
                                icon: icon,
                                size: 30,
                                color: color
                            )
                        ),
                        new Container(
                            margin: EdgeInsets.only(top: 8),
                            height: 20,
                            child: new Text(
                                data: title,
                                style: CTextStyle.PSmallBody4
                            )
                        )
                    })
                )
            );
        }
    }
}