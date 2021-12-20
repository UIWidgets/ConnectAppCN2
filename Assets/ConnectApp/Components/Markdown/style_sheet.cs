using System.Collections.Generic;
using ConnectApp.Common.Visual;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace ConnectApp.Components.Markdown {
    public class MarkdownStyleSheet {
        public MarkdownStyleSheet(
            TextStyle a,
            TextStyle p,
            TextStyle code,
            TextStyle h1,
            TextStyle h2,
            TextStyle h3,
            TextStyle h4,
            TextStyle h5,
            TextStyle h6,
            TextStyle em,
            TextStyle strong,
            TextStyle del,
            TextStyle blockquote,
            TextStyle img,
            TextStyle checkbox,
            float blockSpacing,
            float listIndent,
            TextStyle listBullet,
            TextStyle tableHead,
            TextStyle tableBody,
            TextAlign tableHeadAlign,
            TableBorder tableBorder,
            TableColumnWidth tableColumnWidth,
            EdgeInsets tableCellsPadding,
            Decoration tableCellsDecoration,
            EdgeInsets blockquotePadding,
            Decoration blockquoteDecoration,
            EdgeInsets codeBlockPadding,
            Decoration codeBlockDecoration,
            Decoration horizontalRuleDecoration,
            float textScaleFactor = 1.0f
        ) {
            this.a = a;
            this.p = p;
            this.code = code;
            this.h1 = h1;
            this.h2 = h2;
            this.h3 = h3;
            this.h4 = h4;
            this.h5 = h5;
            this.h6 = h6;
            this.em = em;
            this.strong = strong;
            this.del = del;
            this.blockquote = blockquote;
            this.img = img;
            this.checkbox = checkbox;
            this.blockSpacing = blockSpacing;
            this.listIndent = listIndent;
            this.listBullet = listBullet;
            this.tableHead = tableHead;
            this.tableBody = tableBody;
            this.tableHeadAlign = tableHeadAlign;
            this.tableBorder = tableBorder;
            this.tableColumnWidth = tableColumnWidth;
            this.tableCellsPadding = tableCellsPadding;
            this.tableCellsDecoration = tableCellsDecoration;
            this.blockquotePadding = blockquotePadding;
            this.blockquoteDecoration = blockquoteDecoration;
            this.codeBlockPadding = codeBlockPadding;
            this.codeBlockDecoration = codeBlockDecoration;
            this.horizontalRuleDecoration = horizontalRuleDecoration;
            this.textScaleFactor = textScaleFactor;

            this._styles = new Dictionary<string, TextStyle>() {
                {"a", a},
                {"p", p},
                {"li", p},
                {"code", code},
                {"pre", p},
                {"h1", h1},
                {"h2", h2},
                {"h3", h3},
                {"h4", h4},
                {"h5", h5},
                {"h6", h6},
                {"em", em},
                {"strong", strong},
                {"del", del},
                {"blockquote", blockquote},
                {"img", img},
                {"table", p},
                {"th", tableHead},
                {"tr", tableHead},
                {"td", tableHead},
            };
        }

        public TextStyle a,
            p,
            code,
            h1,
            h2,
            h3,
            h4,
            h5,
            h6,
            em,
            strong,
            del,
            blockquote,
            img,
            checkbox,
            listBullet,
            tableHead,
            tableBody;

        public float blockSpacing, listIndent, textScaleFactor;

        public TextAlign tableHeadAlign;

        public TableBorder tableBorder;

        public TableColumnWidth tableColumnWidth;

        public EdgeInsets tableCellsPadding, blockquotePadding, codeBlockPadding;

        public Decoration tableCellsDecoration, blockquoteDecoration, codeBlockDecoration, horizontalRuleDecoration;

        Dictionary<string, TextStyle> _styles;

        public Dictionary<string, TextStyle> styles {
            get { return this._styles; }
        }

        public static MarkdownStyleSheet fromTheme() {
            return new MarkdownStyleSheet(
                new TextStyle(true, color: CColors.Blue),
                p: CTextStyle.PRegularBody,
                code: CTextStyle.PCodeStyle,
                h1: CTextStyle.H1,
                h2: CTextStyle.H2,
                h3: CTextStyle.H3,
                h4: CTextStyle.H4,
                h5: CTextStyle.H5,
                h6: CTextStyle.H5,
                new TextStyle(fontStyle: FontStyle.italic),
                new TextStyle(fontWeight: FontWeight.bold),
                new TextStyle(decoration: TextDecoration.lineThrough),
                blockquote: CTextStyle.PLargeBody,
                img: CTextStyle.PLargeBody,
                CTextStyle.PLargeBody.copyWith(
                    color: CColors.PrimaryBlue
                ),
                8.0f,
                24.0f,
                listBullet: CTextStyle.PLargeBody,
                new TextStyle(fontWeight: FontWeight.w600),
                tableBody: CTextStyle.PLargeBody,
                tableHeadAlign: TextAlign.center,
                TableBorder.all(color: CColors.Grey, 0),
                new FlexColumnWidth(),
                EdgeInsets.fromLTRB(16, 8, 16, 8),
                new BoxDecoration(color: CColors.Grey),
                EdgeInsets.all(8.0f),
                new BoxDecoration(
                    color: CColors.Blue,
                    borderRadius: BorderRadius.circular(2.0f)
                ),
                EdgeInsets.all(8.0f),
                new BoxDecoration(
                    color: CColors.Grey,
                    borderRadius: BorderRadius.circular(2.0f)
                ),
                new BoxDecoration(
                    border: new Border(
                        new BorderSide(width: 5.0f, color: CColors.Grey)
                    )
                )
            );
        }
    }
}