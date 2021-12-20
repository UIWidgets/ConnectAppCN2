using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ConnectApp.Common.Visual;
using ConnectApp.Components.Markdown;
using ConnectApp.Components.Markdown.basic;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace ConnectApp.Common.Util {
    public static class MarkdownUtils {
        public static List<Node> parseMarkdown(string data) {
            var lines = Regex.Split(input: data, "\r?\n");
            var document = new Document(
                extensionSet: ExtensionSet.githubFlavored,
                inlineSyntaxes: new List<InlineSyntax> {new TaskListSyntax()},
                encodeHtml: false
            );
            var nodes = document.parseLines(lines.ToList());
            return nodes;
        }
        
        public static MarkdownStyleSheet defaultStyle() {
            return new MarkdownStyleSheet(
                a: CTextStyle.PXLargeBlue,
                p: CTextStyle.PXLarge,
                code: CTextStyle.PCodeStyle,
                h1: CTextStyle.H4,
                h2: CTextStyle.H5,
                h3: CTextStyle.PXLarge,
                h4: CTextStyle.PXLarge,
                h5: CTextStyle.PXLarge,
                h6: CTextStyle.PXLarge,
                CTextStyle.PXLarge.copyWith(fontStyle: FontStyle.italic),
                CTextStyle.PXLarge.copyWith(fontWeight: FontWeight.bold),
                new TextStyle(decoration: TextDecoration.lineThrough),
                blockquote: CTextStyle.PXLargeBody4,
                img: CTextStyle.PXLarge,
                checkbox: CTextStyle.PXLarge,
                8.0f,
                24.0f,
                listBullet: CTextStyle.PXLarge,
                new TextStyle(fontWeight: FontWeight.w600),
                tableBody: CTextStyle.PXLarge,
                tableHeadAlign: TextAlign.center,
                TableBorder.all(color: CColors.Grey, 0),
                new FlexColumnWidth(),
                EdgeInsets.fromLTRB(16, 8, 16, 8),
                new BoxDecoration(color: CColors.Grey),
                EdgeInsets.all(16.0f),
                new BoxDecoration(
                    border: new Border(
                        left: new BorderSide(
                            color: CColors.Separator,
                            8
                        ))
                ),
                EdgeInsets.all(30.0f),
                new BoxDecoration(
                    Color.fromRGBO(110, 198, 255, 0.12f)
                ),
                new BoxDecoration(
                    border: new Border(
                        new BorderSide(width: 5.0f, color: CColors.Grey)
                    )
                ));
        }

        public static MarkdownStyleSheet qaStyleSheet() {
            return new MarkdownStyleSheet(
                a: CTextStyle.PLargeBlue,
                p: CTextStyle.PLargeBody2,
                code: CTextStyle.PCodeStyle,
                h1: CTextStyle.H4,
                h2: CTextStyle.H5,
                h3: CTextStyle.PLargeBody2,
                h4: CTextStyle.PLargeBody2,
                h5: CTextStyle.PLargeBody2,
                h6: CTextStyle.PLargeBody2,
                CTextStyle.PLargeBody2.copyWith(fontStyle: FontStyle.italic),
                CTextStyle.PLargeBody2.copyWith(fontWeight: FontWeight.bold),
                new TextStyle(decoration: TextDecoration.lineThrough),
                blockquote: CTextStyle.PLargeBody4,
                img: CTextStyle.PLargeBody2,
                checkbox: CTextStyle.PLargeBody2,
                8.0f,
                24.0f,
                listBullet: CTextStyle.PLargeBody2,
                new TextStyle(fontWeight: FontWeight.w600),
                tableBody: CTextStyle.PLargeBody2,
                tableHeadAlign: TextAlign.center,
                TableBorder.all(color: CColors.Grey, 0),
                new FlexColumnWidth(),
                EdgeInsets.fromLTRB(16, 8, 16, 8),
                new BoxDecoration(color: CColors.Grey),
                EdgeInsets.all(16.0f),
                new BoxDecoration(
                    border: new Border(
                        left: new BorderSide(
                            color: CColors.Separator,
                            8
                        ))
                ),
                EdgeInsets.all(30.0f),
                new BoxDecoration(
                    Color.fromRGBO(110, 198, 255, 0.12f)
                ),
                new BoxDecoration(
                    border: new Border(
                        new BorderSide(width: 5.0f, color: CColors.Grey)
                    )
                ));
        }

        public static List<string> markdownImages = new List<string>();
    }
}