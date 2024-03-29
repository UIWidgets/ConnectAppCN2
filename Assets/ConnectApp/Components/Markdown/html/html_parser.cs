using System;
using System.Collections.Generic;
using System.Linq;
using ConnectApp.Common.Visual;
using ConnectApp.Components.Markdown.html.htmlAgilityPack;
using uiwidgets;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Image = Unity.UIWidgets.widgets.Image;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace ConnectApp.Components.Markdown.html {
    public class HtmlOldParser : StatelessWidget {
        public HtmlOldParser(
            float? width = null,
            OnLinkTap onLinkTap = null,
            bool renderNewlines = false,
            CustomRender customRender = null,
            float blockSpacing = 0.0f,
            string html = null,
            ImageErrorListener onImageError = null,
            TextStyle linkStyle = null,
            bool showImages = true
        ) {
            D.assert(width != null);
            this.width = width.Value;
            this.onLinkTap = onLinkTap;
            this.renderNewlines = renderNewlines;
            this.customRender = customRender;
            this.blockSpacing = blockSpacing;
            this.html = html;
            this.onImageError = onImageError;
            this.linkStyle = linkStyle;
            this.showImages = showImages;
            this.linkStyle = linkStyle ?? new TextStyle(
                decoration: TextDecoration.underline,
                color: CColors.Blue,
                decorationColor: CColors.Blue);
        }

        public readonly float width;
        public readonly OnLinkTap onLinkTap;
        public readonly bool renderNewlines;
        public readonly CustomRender customRender;
        public readonly float blockSpacing;
        public readonly string html;
        public readonly ImageErrorListener onImageError;
        public readonly TextStyle linkStyle;
        public readonly bool showImages;

        static List<string> _supportedElements = new List<string> {
            "a",
            "abbr",
            "acronym",
            "address",
            "article",
            "aside",
            "b",
            "bdi",
            "bdo",
            "big",
            "blockquote",
            "body",
            "br",
            "caption",
            "cite",
            "center",
            "code",
            "data",
            "dd",
            "del",
            "dfn",
            "div",
            "dl",
            "dt",
            "em",
            "figcaption",
            "figure",
            "font",
            "footer",
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "h6",
            "header",
            "hr",
            "i",
            "img",
            "ins",
            "kbd",
            "li",
            "main",
            "mark",
            "nav",
            "noscript",
            "ol", //partial
            "p",
            "pre",
            "q",
            "rp",
            "rt",
            "ruby",
            "s",
            "samp",
            "section",
            "small",
            "span",
            "strike",
            "strong",
            "sub",
            "sup",
            "table",
            "tbody",
            "td",
            "template",
            "tfoot",
            "th",
            "thead",
            "time",
            "tr",
            "tt",
            "u",
            "ul", //partial
            "var",
        };


        public override Widget build(BuildContext context) {
            return new Wrap(
                alignment: WrapAlignment.start,
                children: this.parse(data: this.html)
            );
        }

        ///Parses an html string and returns a list of widgets that represent the body of your html document.
        public List<Widget> parse(string data) {
            var widgetList = new List<Widget>();

            if (this.renderNewlines) {
                data = data.Replace("\n", "<br />");
            }

            var document = new HtmlDocument();
            document.LoadHtml(html: data);
            widgetList.Add(this._parseNode(node: document.DocumentNode));
            return widgetList;
        }

        Widget _parseNode(HtmlNode node) {
            if (this.customRender != null) {
                var customWidget = this.customRender(node: node, this._parseNodeList(nodeList: node.ChildNodes));
                if (customWidget != null) {
                    return customWidget;
                }
            }

            if (node.NodeType is HtmlNodeType.Element) {
                if (!_supportedElements.Contains(item: node.Name)) {
                    return new Container();
                }

                switch (node.Name) {
                    case "a":
                        return new GestureDetector(
                            child: DefaultTextStyle.merge(
                                new Wrap(
                                    children: this._parseNodeList(nodeList: node.ChildNodes)
                                ),
                                style: this.linkStyle
                            ),
                            onTap: () => {
                                if (node.Attributes.Contains("href") && this.onLinkTap != null) {
                                    var url = node.Attributes["href"].Value;
                                    this.onLinkTap(url: url);
                                }
                            });
                    case "abbr":
                        return DefaultTextStyle.merge(
                            new Wrap(
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            ),
                            style: new TextStyle(
                                decoration: TextDecoration.underline,
                                decorationStyle: TextDecorationStyle.solid
                            )
                        );
                    case "acronym":
                        return DefaultTextStyle.merge(
                            new Wrap(
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            ),
                            style: new TextStyle(
                                decoration: TextDecoration.underline,
                                decorationStyle: TextDecorationStyle.solid
                            )
                        );
                    case "address":
                        return DefaultTextStyle.merge(
                            new Wrap(
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            ),
                            style: new TextStyle(
                                fontStyle: FontStyle.italic
                            )
                        );
                    case "article":
                        return new Container(
                            width: this.width,
                            child: new Wrap(
                                crossAxisAlignment: WrapCrossAlignment.center,
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            )
                        );
                    case "aside":
                        return new Container(
                            width: this.width,
                            child: new Wrap(
                                crossAxisAlignment: WrapCrossAlignment.center,
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            )
                        );
                    case "b":
                        return DefaultTextStyle.merge(
                            new Wrap(
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            ),
                            style: new TextStyle(
                                fontWeight: FontWeight.bold
                            )
                        );
                    case "bdi":
                        return new Wrap(
                            children: this._parseNodeList(nodeList: node.ChildNodes)
                        );
                    case "bdo":
                        if (node.Attributes["dir"] != null) {
                            return new Directionality(
                                new Wrap(
                                    children: this._parseNodeList(nodeList: node.ChildNodes)
                                ),
                                node.Attributes["dir"].Value == "rtl"
                                    ? TextDirection.rtl
                                    : TextDirection.ltr
                            );
                        }

                        //Direction attribute is required, just render the text normally now.
                        return new Wrap(
                            children: this._parseNodeList(nodeList: node.ChildNodes)
                        );
                    case "big":
                        return DefaultTextStyle.merge(
                            new Wrap(
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            ),
                            style: new TextStyle(
                                fontSize: 20.0f
                            )
                        );
                    case "blockquote":
                        return new Padding(
                            padding:
                            EdgeInsets.fromLTRB(40.0f, top: this.blockSpacing, 40.0f, bottom: this.blockSpacing),
                            child: new Container(
                                width: this.width,
                                child: new Wrap(
                                    crossAxisAlignment: WrapCrossAlignment.center,
                                    children: this._parseNodeList(nodeList: node.ChildNodes)
                                )
                            )
                        );
                    case "body":
                        return new Container(
                            width: this.width,
                            child: new Wrap(
                                crossAxisAlignment: WrapCrossAlignment.center,
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            )
                        );
                    case "br":
                        if (this._isNotFirstBreakTag(node: node)) {
                            return new Container(width: this.width, height: this.blockSpacing);
                        }

                        return new Container(width: this.width);
                    case "caption":
                        return new Container(
                            width: this.width,
                            child: new Wrap(
                                crossAxisAlignment: WrapCrossAlignment.center,
                                alignment: WrapAlignment.center,
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            )
                        );
                    case "center":
                        return new Container(
                            width: this.width,
                            child: new Wrap(
                                crossAxisAlignment: WrapCrossAlignment.center,
                                children: this._parseNodeList(nodeList: node.ChildNodes),
                                alignment: WrapAlignment.center
                            ));
                    case "cite":
                        return DefaultTextStyle.merge(
                            new Wrap(
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            ),
                            style: new TextStyle(
                                fontStyle: FontStyle.italic
                            )
                        );
                    case "code":
                        return DefaultTextStyle.merge(
                            new Wrap(
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            ),
                            style: new TextStyle(
                                fontFamily: "monospace"
                            )
                        );
                    case "data":
                        return new Wrap(
                            children: this._parseNodeList(nodeList: node.ChildNodes)
                        );
                    case "dd":
                        return new Padding(
                            padding: EdgeInsets.only(40.0f),
                            child: new Container(
                                width: this.width,
                                child: new Wrap(
                                    crossAxisAlignment: WrapCrossAlignment.center,
                                    children: this._parseNodeList(nodeList: node.ChildNodes)
                                )
                            ));
                    case "del":
                        return DefaultTextStyle.merge(
                            new Wrap(
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            ),
                            style: new TextStyle(
                                decoration: TextDecoration.lineThrough
                            )
                        );
                    case "dfn":
                        return DefaultTextStyle.merge(
                            new Wrap(
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            ),
                            style: new TextStyle(
                                fontStyle: FontStyle.italic
                            )
                        );
                    case "div":
                        return new Container(
                            width: this.width,
                            child: new Wrap(
                                crossAxisAlignment: WrapCrossAlignment.center,
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            )
                        );
                    case "dl":
                        return new Padding(
                            padding: EdgeInsets.only(top: this.blockSpacing, bottom: this.blockSpacing),
                            child: new Column(
                                children: this._parseNodeList(nodeList: node.ChildNodes),
                                crossAxisAlignment: CrossAxisAlignment.start
                            ));
                    case "dt":
                        return new Wrap(
                            children: this._parseNodeList(nodeList: node.ChildNodes)
                        );
                    case "em":
                        return DefaultTextStyle.merge(
                            new Wrap(
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            ),
                            style: new TextStyle(
                                fontStyle: FontStyle.italic
                            )
                        );
                    case "figcaption":
                        return new Wrap(
                            children: this._parseNodeList(nodeList: node.ChildNodes)
                        );
                    case "figure":
                        return new Padding(
                            padding:
                            EdgeInsets.fromLTRB(40.0f, top: this.blockSpacing, 40.0f, bottom: this.blockSpacing),
                            child: new Column(
                                children: this._parseNodeList(nodeList: node.ChildNodes),
                                crossAxisAlignment: CrossAxisAlignment.center
                            ));
                    case "font":
                        return new Wrap(
                            children: this._parseNodeList(nodeList: node.ChildNodes)
                        );
                    case "footer":
                        return new Container(
                            width: this.width,
                            child: new Wrap(
                                crossAxisAlignment: WrapCrossAlignment.center,
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            )
                        );
                    case "h1":
                        return DefaultTextStyle.merge(
                            new Container(
                                width: this.width,
                                child: new Wrap(
                                    crossAxisAlignment: WrapCrossAlignment.center,
                                    children: this._parseNodeList(nodeList: node.ChildNodes)
                                )
                            ),
                            style: new TextStyle(
                                fontSize: 28.0f,
                                fontWeight: FontWeight.bold
                            )
                        );
                    case "h2":
                        return DefaultTextStyle.merge(
                            new Container(
                                width: this.width,
                                child: new Wrap(
                                    crossAxisAlignment: WrapCrossAlignment.center,
                                    children: this._parseNodeList(nodeList: node.ChildNodes)
                                )
                            ),
                            style: new TextStyle(
                                fontSize: 21.0f,
                                fontWeight: FontWeight.bold
                            )
                        );
                    case "h3":
                        return DefaultTextStyle.merge(
                            new Container(
                                width: this.width,
                                child: new Wrap(
                                    crossAxisAlignment: WrapCrossAlignment.center,
                                    children: this._parseNodeList(nodeList: node.ChildNodes)
                                )
                            ),
                            style: new TextStyle(
                                fontSize: 16.0f,
                                fontWeight: FontWeight.bold
                            )
                        );
                    case "h4":
                        return DefaultTextStyle.merge(
                            new Container(
                                width: this.width,
                                child: new Wrap(
                                    crossAxisAlignment: WrapCrossAlignment.center,
                                    children: this._parseNodeList(nodeList: node.ChildNodes)
                                )
                            ),
                            style: new TextStyle(
                                fontSize: 14.0f,
                                fontWeight: FontWeight.bold
                            )
                        );
                    case "h5":
                        return DefaultTextStyle.merge(
                            new Container(
                                width: this.width,
                                child: new Wrap(
                                    crossAxisAlignment: WrapCrossAlignment.center,
                                    children: this._parseNodeList(nodeList: node.ChildNodes)
                                )
                            ),
                            style: new TextStyle(
                                fontSize: 12.0f,
                                fontWeight: FontWeight.bold
                            )
                        );
                    case "h6":
                        return DefaultTextStyle.merge(
                            new Container(
                                width: this.width,
                                child: new Wrap(
                                    crossAxisAlignment: WrapCrossAlignment.center,
                                    children: this._parseNodeList(nodeList: node.ChildNodes)
                                )
                            ),
                            style: new TextStyle(
                                fontSize: 10.0f,
                                fontWeight: FontWeight.bold
                            )
                        );
                    case "header":
                        return new Container(
                            width: this.width,
                            child: new Wrap(
                                crossAxisAlignment: WrapCrossAlignment.center,
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            )
                        );
                    case "hr":
                        return new Padding(
                            padding: EdgeInsets.only(top: 7.0f, bottom: 7.0f),
                            child: new CustomDivider(height: 1.0f, color: CColors.Black38)
                        );
                    case "i":
                        return DefaultTextStyle.merge(
                            new Wrap(
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            ),
                            style: new TextStyle(
                                fontStyle: FontStyle.italic
                            )
                        );
                    case "img":
                        return new Builder(
                            builder: (BuildContext context) => {
                                if (this.showImages) {
                                    if (node.Attributes["src"] != null) {
                                        if (node.Attributes["src"].Value.StartsWith("data:image") &&
                                            node.Attributes["src"].Value.Contains("base64,")) {
                                            return Image.memory(Convert.FromBase64String(
                                                node.Attributes["src"].Value.Split(new string[] {"base64,"},
                                                    options: StringSplitOptions.None)[1].Trim()));
                                        }

                                        return Image.network(src: node.Attributes["src"].Value);
                                    }
                                    else if (node.Attributes["alt"] != null) {
                                        //Temp fix for https://github.com/flutter/flutter/issues/736
                                        if (node.Attributes["alt"].Value.EndsWith(" ")) {
                                            return new Container(
                                                padding: EdgeInsets.only(right: 2.0f),
                                                child: new Text(data: node.Attributes["alt"].Value));
                                        }
                                        else {
                                            return new Text(data: node.Attributes["alt"].Value);
                                        }
                                    }
                                }

                                return new Container();
                            }
                        );
                    case "ins":
                        return DefaultTextStyle.merge(
                            new Wrap(
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            ),
                            style: new TextStyle(
                                decoration: TextDecoration.underline
                            )
                        );
                    case "kbd":
                        return DefaultTextStyle.merge(
                            new Wrap(
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            ),
                            style: new TextStyle(
                                fontFamily: "monospace"
                            )
                        );
                    case "li":
                        var type = node.ParentNode.Name; // Parent type; usually ol or ul
                        var markPadding = EdgeInsets.symmetric(horizontal: 4.0f);
                        Widget mark;
                        switch (type) {
                            case "ul":
                                mark = new Container(child: new Text("•"), padding: markPadding);
                                break;
                            case "ol":
                                var index = node.ParentNode.ChildNodes.IndexOf(item: node) + 1;
                                mark = new Container(child: new Text($"{index}."), padding: markPadding);
                                break;
                            default: //Fallback to middle dot
                                mark = new Container(width: 0.0f, height: 0.0f);
                                break;
                        }

                        return new Container(
                            width: this.width,
                            child: new Wrap(
                                crossAxisAlignment: WrapCrossAlignment.center,
                                children: new List<Widget> {
                                    mark,
                                    new Wrap(
                                        crossAxisAlignment: WrapCrossAlignment.center,
                                        children: this._parseNodeList(nodeList: node.ChildNodes))
                                }
                            )
                        );
                    case "main":
                        return new Container(
                            width: this.width,
                            child: new Wrap(
                                crossAxisAlignment: WrapCrossAlignment.center,
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            )
                        );
                    case "mark":
                        return DefaultTextStyle.merge(
                            new Wrap(
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            ),
                            style: new TextStyle(
                                color: Colors.black,
                                background: this._getPaint(color: Colors.yellow)
                            )
                        );
                    case "nav":
                        return new Container(
                            width: this.width,
                            child: new Wrap(
                                crossAxisAlignment: WrapCrossAlignment.center,
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            )
                        );
                    case "noscript":
                        return new Container(
                            width: this.width,
                            child: new Wrap(
                                crossAxisAlignment: WrapCrossAlignment.center,
                                alignment: WrapAlignment.start,
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            )
                        );
                    case "ol":
                        return new Column(
                            children: this._parseNodeList(nodeList: node.ChildNodes),
                            crossAxisAlignment: CrossAxisAlignment.start
                        );
                    case "p":
                        return new Padding(
                            padding: EdgeInsets.only(top: this.blockSpacing, bottom: this.blockSpacing),
                            child: new Container(
                                width: this.width,
                                child: new Wrap(
                                    crossAxisAlignment: WrapCrossAlignment.center,
                                    alignment: WrapAlignment.start,
                                    children: this._parseNodeList(nodeList: node.ChildNodes)
                                )
                            )
                        );
                    case "pre":
                        return new Padding(
                            padding: EdgeInsets.all(value: this.blockSpacing),
                            child: DefaultTextStyle.merge(
                                new Text(data: node.InnerHtml),
                                style: new TextStyle(
                                    fontFamily: "monospace"
                                )
                            )
                        );
                    case "q":
                        var children = new List<Widget>();
                        children.Add(new Text("\""));
                        children.AddRange(this._parseNodeList(nodeList: node.ChildNodes));
                        children.Add(new Text("\""));
                        return DefaultTextStyle.merge(
                            new Wrap(
                                children: children
                            ),
                            style: new TextStyle(
                                fontStyle: FontStyle.italic
                            )
                        );
                    case "rp":
                        return new Wrap(
                            children: this._parseNodeList(nodeList: node.ChildNodes)
                        );
                    case "rt":
                        return new Wrap(
                            children: this._parseNodeList(nodeList: node.ChildNodes)
                        );
                    case "ruby":
                        return new Wrap(
                            children: this._parseNodeList(nodeList: node.ChildNodes)
                        );
                    case "s":
                        return DefaultTextStyle.merge(
                            new Wrap(
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            ),
                            style: new TextStyle(
                                decoration: TextDecoration.lineThrough
                            )
                        );
                    case "samp":
                        return DefaultTextStyle.merge(
                            new Wrap(
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            ),
                            style: new TextStyle(
                                fontFamily: "monospace"
                            )
                        );
                    case "section":
                        return new Container(
                            width: this.width,
                            child: new Wrap(
                                crossAxisAlignment: WrapCrossAlignment.center,
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            )
                        );
                    case "small":
                        return DefaultTextStyle.merge(
                            new Wrap(
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            ),
                            style: new TextStyle(
                                fontSize: 10.0f
                            )
                        );
                    case "span":
                        return new Wrap(
                            children: this._parseNodeList(nodeList: node.ChildNodes)
                        );
                    case "strike":
                        return DefaultTextStyle.merge(
                            new Wrap(
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            ),
                            style: new TextStyle(
                                decoration: TextDecoration.lineThrough
                            )
                        );
                    case "strong":
                        return DefaultTextStyle.merge(
                            new Wrap(
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            ),
                            style: new TextStyle(
                                fontWeight: FontWeight.bold
                            )
                        );
                    case "sub":
                    case "sup":
                        //Use builder to capture the parent font to inherit the font styles
                        return new Builder(builder: (BuildContext context) => {
                            var parent = DefaultTextStyle.of(context: context);
                            var parentStyle = parent.style;

                            var painter = new TextPainter(
                                new TextSpan(
                                    text: node.InnerText,
                                    style: parentStyle
                                ),
                                textDirection: TextDirection.ltr);
                            painter.layout();
                            //print(painter.size);

                            //Get the height from the default text
                            var height = painter.size.height *
                                         1.35f; //compute a higher height for the text to increase the offset of the Positioned text

                            painter = new TextPainter(
                                new TextSpan(
                                    text: node.InnerText,
                                    parentStyle.merge(new TextStyle(
                                        fontSize:
                                        parentStyle.fontSize * RichTextParserUtils.OFFSET_TAGS_FONT_SIZE_FACTOR))
                                ),
                                textDirection: TextDirection.ltr);
                            painter.layout();
                            //print(painter.size);

                            //Get the width from the reduced/positioned text
                            var width = painter.size.width;

                            //print("Width: $width, Height: $height");

                            return DefaultTextStyle.merge(
                                new Wrap(
                                    crossAxisAlignment: WrapCrossAlignment.center,
                                    children: new List<Widget> {
                                        new Stack(
                                            fit: StackFit.loose,
                                            children: new List<Widget> {
                                                //The Stack needs a non-positioned object for the next widget to respect the space so we create
                                                //a sized box to fill the required space
                                                new SizedBox(
                                                    width: width,
                                                    height: height
                                                ),
                                                DefaultTextStyle.merge(
                                                    new Positioned(
                                                        child: new Wrap(
                                                            children: this._parseNodeList(nodeList: node.ChildNodes)),
                                                        bottom: node.Name == "sub" ? 0 : (int?) null,
                                                        top: node.Name == "sub" ? (int?) null : 0
                                                    ),
                                                    style: new TextStyle(
                                                        fontSize: parentStyle.fontSize *
                                                                  RichTextParserUtils.OFFSET_TAGS_FONT_SIZE_FACTOR)
                                                )
                                            }
                                        )
                                    }
                                )
                            );
                        });
                    case "table":
                        return new Column(
                            children: this._parseNodeList(nodeList: node.ChildNodes),
                            crossAxisAlignment: CrossAxisAlignment.start
                        );
                    case "tbody":
                        return new Column(
                            children: this._parseNodeList(nodeList: node.ChildNodes),
                            crossAxisAlignment: CrossAxisAlignment.start
                        );
                    case "td":
                        var colspan = 1;
                        if (node.Attributes["colspan"] != null) {
                            int.TryParse(s: node.Attributes["colspan"].Value, result: out colspan);
                        }

                        return new Expanded(
                            flex: colspan,
                            child: new Wrap(
                                crossAxisAlignment: WrapCrossAlignment.center,
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            )
                        );
                    case "template":
                        //Not usually displayed in HTML
                        return new Container();
                    case "tfoot":
                        return new Column(
                            children: this._parseNodeList(nodeList: node.ChildNodes),
                            crossAxisAlignment: CrossAxisAlignment.start
                        );
                    case "th":
                        var _colspan = 1;
                        if (node.Attributes["colspan"] != null) {
                            int.TryParse(s: node.Attributes["colspan"].Value, result: out _colspan);
                        }

                        return DefaultTextStyle.merge(
                            new Expanded(
                                flex: _colspan,
                                child: new Wrap(
                                    crossAxisAlignment: WrapCrossAlignment.center,
                                    alignment: WrapAlignment.center,
                                    children: this._parseNodeList(nodeList: node.ChildNodes)
                                )
                            ),
                            style: new TextStyle(
                                fontWeight: FontWeight.bold
                            )
                        );
                    case "thead":
                        return new Column(
                            children: this._parseNodeList(nodeList: node.ChildNodes),
                            crossAxisAlignment: CrossAxisAlignment.start
                        );
                    case "time":
                        return new Wrap(
                            children: this._parseNodeList(nodeList: node.ChildNodes)
                        );
                    case "tr":
                        return new Row(
                            children: this._parseNodeList(nodeList: node.ChildNodes),
                            crossAxisAlignment: CrossAxisAlignment.center
                        );
                    case "tt":
                        return DefaultTextStyle.merge(
                            new Wrap(
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            ),
                            style: new TextStyle(
                                fontFamily: "monospace"
                            )
                        );
                    case "u":
                        return DefaultTextStyle.merge(
                            new Wrap(
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            ),
                            style: new TextStyle(
                                decoration: TextDecoration.underline
                            )
                        );
                    case "ul":
                        return new Column(
                            children: this._parseNodeList(nodeList: node.ChildNodes),
                            crossAxisAlignment: CrossAxisAlignment.start
                        );
                    case "var":
                        return DefaultTextStyle.merge(
                            new Wrap(
                                children: this._parseNodeList(nodeList: node.ChildNodes)
                            ),
                            style: new TextStyle(
                                fontStyle: FontStyle.italic
                            )
                        );
                }
            }
            else if (node.NodeType is HtmlNodeType.Text) {
                //We don't need to worry about rendering extra whitespace
                if (node.InnerText.Trim() == "" && node.InnerText.IndexOf(" ") == -1) {
                    return new Wrap();
                }

                if (node.InnerText.Trim() == "" && node.InnerText.IndexOf(" ") != -1) {
                    (node as HtmlTextNode).Text = " ";
                }

                var finalText = this.trimStringHtml(stringToTrim: node.InnerText);
                //Temp fix for https://github.com/flutter/flutter/issues/736
                if (finalText.EndsWith(" ")) {
                    return new Container(
                        padding: EdgeInsets.only(right: 2.0f), child: new Text(data: finalText));
                }
                else {
                    return new Text(data: finalText);
                }
            }

            return new Wrap();
        }

        List<Widget> _parseNodeList(HtmlNodeCollection nodeList) {
            return nodeList.Select(selector: this._parseNode).ToList();
        }

        Paint _getPaint(Color color) {
            var paint = new Paint();
            paint.color = color;
            return paint;
        }

        public string trimStringHtml(string stringToTrim) {
            stringToTrim = stringToTrim.Replace("\n", "");
            while (stringToTrim.IndexOf("  ") != -1) {
                stringToTrim = stringToTrim.Replace("  ", " ");
            }

            return stringToTrim;
        }

        bool _isNotFirstBreakTag(HtmlNode node) {
            var index = node.ParentNode.ChildNodes.IndexOf(item: node);
            if (index == 0) {
                if (node.ParentNode == null) {
                    return false;
                }

                return this._isNotFirstBreakTag(node: node.ParentNode);
            }
            else if (node.ParentNode.ChildNodes[index - 1] is HtmlNode) {
                if ((node.ParentNode.ChildNodes[index - 1]).Name == "br") {
                    return true;
                }

                return false;
            }
            else if (node.ParentNode.ChildNodes[index - 1].NodeType == HtmlNodeType.Text) {
                if ((node.ParentNode.ChildNodes[index - 1]).InnerText.Trim() == "") {
                    return this._isNotFirstBreakTag(node.ParentNode.ChildNodes[index - 1]);
                }
                else {
                    return false;
                }
            }

            return false;
        }
    }
}