using System;
using System.Collections.Generic;
using ConnectApp.Common.Visual;
using ConnectApp.Components.Markdown.basic;
using ConnectApp.Components.Markdown.html.htmlAgilityPack;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using FontStyle = Unity.UIWidgets.ui.FontStyle;
using Image = Unity.UIWidgets.widgets.Image;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace ConnectApp.Components.Markdown.html {
    static class RichTextParserUtils {
        public static readonly float OFFSET_TAGS_FONT_SIZE_FACTOR = 0.7f;
    }


    public delegate Widget CustomRender(HtmlNode node, List<Widget> children);

    public delegate TextStyle CustomTextStyle(
        HtmlNode node,
        TextStyle baseStyle
    );

    public delegate TextAlign? CustomTextAlign(HtmlNode elem);

    public delegate EdgeInsets CustomEdgeInsets(HtmlNode node);

    public delegate void OnLinkTap(string url);

    public delegate void OnImageTap(string source);

//The ratio of the parent font for each of the offset tags: sup or sub

    public class LinkTextSpan : TextSpan {
        // Beware!
        //
        // This class is only safe because the TapGestureRecognizer is not
        // given a deadline and therefore never allocates any resources.
        //
        // In any other situation -- setting a deadline, using any of the less trivial
        // recognizers, etc -- you would have to manage the gesture recognizer's
        // lifetime and call dispose() when the TextSpan was no longer being rendered.
        //
        // Since TextSpan itself is @immutable, this means that you would have to
        // manage the recognizer from outside the TextSpan, e.g. in the State of a
        // stateful widget that then hands the recognizer to the TextSpan.
        public readonly string url;

        public LinkTextSpan(TextStyle style = null,
            string url = null,
            string text = null,
            OnLinkTap onLinkTap = null,
            List<InlineSpan> children = null) : base(
            style: style,
            text: text,
            children: children ?? new List<InlineSpan>(),
            recognizer: new TapGestureRecognizer()
        ) {
            this.url = url;
            (this.recognizer as TapGestureRecognizer).onTap = () => {
                if (onLinkTap != null) {
                    onLinkTap(url: url);
                }
            };
        }
    }

    public class LinkBlock : Container {
        // final string url;
        // final EdgeInsets padding;
        // final EdgeInsets margin;
        // final OnLinkTap onLinkTap;
        public readonly List<Widget> children;

        public LinkBlock(
            string url = null,
            EdgeInsets padding = null,
            EdgeInsets margin = null,
            OnLinkTap onLinkTap = null,
            List<Widget> children = null
        ) : base(
            padding: padding,
            margin: margin,
            child: new GestureDetector(
                onTap: () => { onLinkTap(url: url); },
                child: new Column(
                    children: children
                )
            )
        ) {
            this.children = children;
        }
    }

    public class BlockText : StatelessWidget {
        public readonly RichText child;
        public readonly EdgeInsets padding;
        public readonly EdgeInsets margin;
        public readonly Decoration decoration;
        public readonly bool shrinkToFit;

        public BlockText(
            /* @required */ RichText child = null,
            /* @required */ bool shrinkToFit = false,
            EdgeInsets padding = null,
            EdgeInsets margin = null,
            Decoration decoration = null
        ) {
            this.child = child;
            this.shrinkToFit = shrinkToFit;
            this.padding = padding;
            this.margin = margin;
            this.decoration = decoration;
        }


        public override Widget build(BuildContext context) {
            return new Container(
                width: this.shrinkToFit ? (float?) null : float.PositiveInfinity,
                padding: this.padding,
                margin: this.margin,
                decoration: this.decoration,
                child: this.child
            );
        }
    }

    public class ParseContext {
        public List<Widget> rootWidgetList; // the widgetList accumulator
        public object parentElement; // the parent spans accumulator
        public int indentLevel = 0;
        public int listCount = 0;
        public string listChar = "•";
        public string blockType; // blockType can be 'p', 'div', 'ul', 'ol', 'blockquote'
        public bool condenseWhitespace = true;
        public bool spansOnly = false;
        public bool inBlock = false;
        public TextStyle childStyle;

        public ParseContext(
            List<Widget> rootWidgetList = null,
            object parentElement = null,
            int indentLevel = 0,
            int listCount = 0,
            string listChar = "•",
            string blockType = null,
            bool condenseWhitespace = true,
            bool spansOnly = false,
            bool inBlock = false,
            TextStyle childStyle = null
        ) {
            this.rootWidgetList = rootWidgetList;
            this.parentElement = parentElement;
            this.indentLevel = indentLevel;
            this.listCount = listCount;
            this.listChar = listChar;
            this.blockType = blockType;
            this.condenseWhitespace = condenseWhitespace;
            this.spansOnly = spansOnly;
            this.inBlock = inBlock;
            this.childStyle = childStyle ?? new TextStyle();
        }

        public static ParseContext fromContext(ParseContext parseContext) {
            return new ParseContext(
                rootWidgetList: parseContext.rootWidgetList,
                parentElement: parseContext.parentElement,
                indentLevel: parseContext.indentLevel,
                listCount: parseContext.listCount,
                listChar: parseContext.listChar,
                blockType: parseContext.blockType,
                condenseWhitespace: parseContext.condenseWhitespace,
                spansOnly: parseContext.spansOnly,
                inBlock: parseContext.inBlock,
                parseContext.childStyle ?? new TextStyle()
            );
        }
    }

    public class HtmlRichTextParser : StatelessWidget {
        public HtmlRichTextParser(
            bool shrinkToFit = false,
            OnLinkTap onLinkTap = null,
            bool renderNewlines = false,
            string html = null,
            CustomEdgeInsets customEdgeInsets = null,
            CustomTextStyle customTextStyle = null,
            CustomTextAlign customTextAlign = null,
            ImageErrorListener onImageError = null,
            TextStyle linkStyle = null,
            ImageProperties imageProperties = null,
            OnImageTap onImageTap = null,
            bool showImages = true
        ) {
            this.shrinkToFit = shrinkToFit;
            this.onLinkTap = onLinkTap;
            this.renderNewlines = renderNewlines;
            this.html = html;
            this.customEdgeInsets = customEdgeInsets;
            this.customTextStyle = customTextStyle;
            this.customTextAlign = customTextAlign;
            this.onImageError = onImageError;
            this.linkStyle = linkStyle ?? new TextStyle(
                decoration: TextDecoration.underline,
                color: CColors.Blue,
                decorationColor: CColors.Blue
            );
            this.imageProperties = imageProperties;
            this.onImageTap = onImageTap;
            this.showImages = showImages;
        }

        public readonly float indentSize = 10.0f;

        public readonly bool shrinkToFit;
        public readonly OnLinkTap onLinkTap;
        public readonly bool renderNewlines;
        public readonly string html;
        public readonly CustomEdgeInsets customEdgeInsets;
        public readonly CustomTextStyle customTextStyle;
        public readonly CustomTextAlign customTextAlign;
        public readonly ImageErrorListener onImageError;
        public readonly TextStyle linkStyle;
        public readonly ImageProperties imageProperties;
        public readonly OnImageTap onImageTap;
        public readonly bool showImages;

        // style elements set a default style
        // for all child nodes
        // treat ol, ul, and blockquote like style elements also
        static List<string> _supportedStyleElements = new List<string> {
            "b",
            "i",
            "address",
            "cite",
            "var",
            "em",
            "strong",
            "kbd",
            "samp",
            "tt",
            "code",
            "ins",
            "u",
            "small",
            "abbr",
            "acronym",
            "mark",
            "ol",
            "ul",
            "blockquote",
            "del",
            "s",
            "strike",
            "ruby",
            "rp",
            "rt",
            "bdi",
            "data",
            "time",
            "span",
            "big",
            "sub"
        };

        // specialty elements require unique handling
        // eg. the "a" tag can contain a block of text or an image
        // sometimes "a" will be rendered with a textspan and recognizer
        // sometimes "a" will be rendered with a clickable Block
        static List<string> _supportedSpecialtyElements = new List<string> {
            "a",
            "br",
            "table",
            "tbody",
            "caption",
            "td",
            "tfoot",
            "th",
            "thead",
            "tr",
            "q",
        };

        // block elements are always rendered with a new
        // block-level widget, if a block level element
        // is found inside another block level element,
        // we simply treat it as a new block level element
        static List<string> _supportedBlockElements = new List<string> {
            "article",
            "aside",
            "body",
            "html",
            "center",
            "dd",
            "dfn",
            "div",
            "dl",
            "dt",
            "figcaption",
            "figure",
            "footer",
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "h6",
            "header",
            "hr",
            "img",
            "li",
            "main",
            "nav",
            "noscript",
            "p",
            "pre",
            "section"
        };

        static List<string> _supportedElements {
            get {
                var tmp = new List<string>();
                tmp.AddRange(collection: _supportedStyleElements);
                tmp.AddRange(collection: _supportedSpecialtyElements);
                tmp.AddRange(collection: _supportedBlockElements);
                return tmp;
            }
        }

        // this function is called recursively for each child
        // however, the first time it is called, we make sure
        // to ignore the node itself, so we only pay attention
        // to the children
        bool _hasBlockChild(HtmlNode node, bool ignoreSelf = true) {
            var retval = false;
            if (node.NodeType == HtmlNodeType.Element) {
                if (_supportedBlockElements.Contains(item: node.Name) && !ignoreSelf) {
                    return true;
                }

                foreach (var _node in node.ChildNodes) {
                    if (this._hasBlockChild(node: _node, false)) {
                        retval = true;
                    }
                }
            }

            return retval;
        }

        // Parses an html string and returns a list of RichText widgets that represent the body of your html document.


        public override Widget build(BuildContext context) {
            try {
                var data = this.html;

                if (this.renderNewlines) {
                    data = data.replaceAll("\n", "<br />");
                }

                var document = new HtmlDocument();
                document.LoadHtml(html: data);
                var body = document.DocumentNode;

                var widgetList = new List<Widget>();
                var parseContext = new ParseContext(
                    rootWidgetList: widgetList,
                    childStyle: DefaultTextStyle.of(context: context).style
                );

                // don't ignore the top level "body"
                this._parseNode(node: body, parseContext: parseContext, buildContext: context);

                // filter out empty widgets
                var children = new List<Widget>();
                foreach (var w in widgetList) {
                    if (w is BlockText blockText) {
                        if (blockText.child.text == null) {
                            continue;
                        }

                        var childTextSpan = blockText.child.text as TextSpan;
                        if ((childTextSpan.text == null || childTextSpan.text.isEmpty()) &&
                            (childTextSpan.children == null || childTextSpan.children.isEmpty())) {
                            continue;
                        }
                    }
                    else if (w is LinkBlock linkBlock) {
                        if (linkBlock.children.isEmpty()) {
                            continue;
                        }
                    }
                    // else if (w is LinkTextSpan linkTextSpan) {
                    //     if (linkTextSpan.text == "" && linkTextSpan.children.isEmpty()) {
                    //         continue;
                    //     }
                    // }

                    children.Add(item: w);
                }

                return new Column(
                    children: children
                );
            }
            catch (Exception e) {
                Debug.LogError(message: e);
                return new Container();
            }
        }

        // THE WORKHORSE FUNCTION!!
        // call the function with the current node and a ParseContext
        // the ParseContext is used to do a number of things
        // first, since we call this function recursively, the parseContext holds references to
        // all the data that is relevant to a particular iteration and its child iterations
        // it holds information about whether to indent the text, whether we are in a list, etc.
        //
        // secondly, it holds the 'global' widgetList that accumulates all the block-level widgets
        //
        // thirdly, it holds a reference to the most recent "parent" so that this iteration of the
        // function can add child nodes to the parent if it should
        //
        // each iteration creates a new parseContext as a copy of the previous one if it needs to
        void _parseNode(HtmlNode node, ParseContext parseContext, BuildContext buildContext) {
            // TEXT ONLY NODES
            // a text only node is a child of a tag with no inner html
            if (node.NodeType == HtmlNodeType.Text) {
                // WHITESPACE CONSIDERATIONS ---
                // truly empty nodes should just be ignored
                if (node.InnerText.Trim() == "" && node.InnerText.IndexOf(" ") == -1) {
                    return;
                }

                // we might want to preserve internal whitespace
                // empty strings of whitespace might be significant or not, condense it by default
                var finalText = node.InnerText;
                if (parseContext.condenseWhitespace) {
                    finalText = this.condenseHtmlWhitespace(stringToTrim: node.InnerText);

                    // if this is part of a string of spans, we will preserve leading
                    // and trailing whitespace unless the previous character is whitespace
                    if (parseContext.parentElement == null) {
                        finalText = finalText.TrimStart();
                    }
                    else if (parseContext.parentElement is LinkTextSpan textSpan) {
                        var lastString = textSpan.text ?? "";
                        if (!textSpan.children.isEmpty()) {
                            lastString = textSpan.children.last().toPlainText() ?? "";
                        }

                        if (lastString.endsWith(' ') || lastString.endsWith('\n')) {
                            finalText = finalText.TrimStart();
                        }
                    }
                }

                // if the finalText is actually empty, just return (unless it's just a space)
                if (finalText.Trim().isEmpty() && finalText != " ") {
                    return;
                }

                // NOW WE HAVE OUR TRULY FINAL TEXT
                // debugPrint("Plain Text Node: '$finalText'");

                // create a span by default
                var span = new TextSpan(
                    text: finalText,
                    children: new List<InlineSpan>(),
                    style: parseContext.childStyle);

                // in this class, a ParentElement must be a BlockText, LinkTextSpan, Row, Column, TextSpan

                // the parseContext might actually be a block level style element, so we
                // need to honor the indent and styling specified by that block style.
                // e.g. ol, ul, blockquote
                var treatLikeBlock = new List<string> {
                    "blockquote", "ul", "ol"
                }.IndexOf(item: parseContext.blockType) != -1;

                // if there is no parentElement, contain the span in a BlockText
                if (parseContext.parentElement == null) {
                    // if this is inside a context that should be treated like a block
                    // but the context is not actually a block, create a block
                    // and append it to the root widget tree
                    if (treatLikeBlock) {
                        Decoration decoration = null;
                        if (parseContext.blockType == "blockquote") {
                            decoration = new BoxDecoration(
                                border:
                                new Border(left: new BorderSide(color: CColors.Black, 2.0f))
                            );
                            parseContext.childStyle = parseContext.childStyle.merge(new TextStyle(
                                fontStyle: FontStyle.italic
                            ));
                        }

                        var blockText = new BlockText(
                            shrinkToFit: this.shrinkToFit,
                            margin: EdgeInsets.only(
                                top: 8.0f,
                                bottom: 8.0f,
                                left: parseContext.indentLevel * this.indentSize),
                            padding: EdgeInsets.all(2.0f),
                            decoration: decoration,
                            child: new RichText(
                                textAlign: TextAlign.left,
                                text: span
                            )
                        );
                        parseContext.rootWidgetList.Add(item: blockText);
                    }
                    else {
                        parseContext.rootWidgetList.Add(new BlockText(
                            new RichText(text: span),
                            shrinkToFit: this.shrinkToFit
                        ));
                    }

                    // this allows future items to be added as children of this item
                    parseContext.parentElement = span;

                    // if the parent is a LinkTextSpan, keep the main attributes of that span going.
                }
                else if (parseContext.parentElement is LinkTextSpan textSpan) {
                    // add this node to the parent as another LinkTextSpan
                    textSpan.children.Add(new LinkTextSpan(
                        textSpan.style.merge(other: parseContext.childStyle),
                        url: textSpan.url,
                        text: finalText,
                        onLinkTap: this.onLinkTap
                    ));

                    // if the parent is a normal span, just add this to that list
                }
                else if (parseContext.parentElement is TextSpan _textSpan) {
                    _textSpan.children.Add(item: span);
                }
                else {
                    // Doing nothing... we shouldn't ever get here
                }
            }

            // OTHER ELEMENT NODES
            else if (node.NodeType == HtmlNodeType.Element || node.NodeType == HtmlNodeType.Document) {
                if (!_supportedElements.Contains(item: node.Name) && node.NodeType != HtmlNodeType.Document) {
                    return;
                }

                // make a copy of the current context so that we can modify
                // pieces of it for the next iteration of this function
                var nextContext = ParseContext.fromContext(parseContext: parseContext);

                // handle style elements
                if (_supportedStyleElements.Contains(item: node.Name)) {
                    var childStyle = parseContext.childStyle ?? new TextStyle();

                    switch (node.Name) {
                        //"b","i","em","strong","code","u","small","abbr","acronym"
                        case "b":
                        case "strong":
                            childStyle =
                                childStyle.merge(new TextStyle(fontWeight: FontWeight.bold));
                            break;
                        case "i":
                        case "address":
                        case "cite":
                        case "var":
                        case "em":
                            childStyle =
                                childStyle.merge(new TextStyle(fontStyle: FontStyle.italic));
                            break;
                        case "kbd":
                        case "samp":
                        case "tt":
                        case "code":
                            childStyle = childStyle.merge(new TextStyle(fontFamily: "monospace"));
                            break;
                        case "ins":
                        case "u":
                            childStyle = childStyle
                                .merge(new TextStyle(decoration: TextDecoration.underline));
                            break;
                        case "abbr":
                        case "acronym":
                            childStyle = childStyle.merge(new TextStyle(
                                decoration: TextDecoration.underline,
                                decorationStyle: TextDecorationStyle.solid
                            ));
                            break;
                        case "big":
                            childStyle = childStyle.merge(new TextStyle(fontSize: 20.0f));
                            break;
                        case "small":
                            childStyle = childStyle.merge(new TextStyle(fontSize: 10.0f));
                            break;
                        case "mark":
                            childStyle = childStyle.merge(
                                new TextStyle(backgroundColor: CColors.Yellow, color: CColors.Black));
                            break;
                        case "sub":
                            childStyle = childStyle.merge(
                                new TextStyle(
                                    fontSize: childStyle.fontSize * RichTextParserUtils.OFFSET_TAGS_FONT_SIZE_FACTOR
                                )
                            );
                            break;
                        case "del":
                        case "s":
                        case "strike":
                            childStyle = childStyle
                                .merge(new TextStyle(decoration: TextDecoration.lineThrough));
                            break;
                        case "ol":
                            nextContext.indentLevel += 1;
                            nextContext.listChar = "#";
                            nextContext.listCount = 0;
                            nextContext.blockType = "ol";
                            break;
                        case "ul":
                            nextContext.indentLevel += 1;
                            nextContext.listChar = "•";
                            nextContext.listCount = 0;
                            nextContext.blockType = "ul";
                            break;
                        case "blockquote":
                            nextContext.indentLevel += 1;
                            nextContext.blockType = "blockquote";
                            break;
                        case "ruby":
                        case "rt":
                        case "rp":
                        case "bdi":
                        case "data":
                        case "time":
                        case "span":
                            //No additional styles
                            break;
                    }

                    if (this.customTextStyle != null) {
                        var customStyle = this.customTextStyle(node: node, baseStyle: childStyle);
                        if (customStyle != null) {
                            childStyle = customStyle;
                        }
                    }

                    nextContext.childStyle = childStyle;
                }

                // handle specialty elements
                else if (_supportedSpecialtyElements.Contains(item: node.Name)) {
                    // should support "a","br","table","tbody","thead","tfoot","th","tr","td"

                    switch (node.Name) {
                        case "a":
                            // if this item has block children, we create
                            // a container and gesture recognizer for the entire
                            // element, otherwise, we create a LinkTextSpan
                            var url = node.Attributes["href"].Value ?? null;

                            if (this._hasBlockChild(node: node)) {
                                var linkContainer = new LinkBlock(
                                    url: url,
                                    margin: EdgeInsets.only(
                                        parseContext.indentLevel * this.indentSize),
                                    onLinkTap: this.onLinkTap,
                                    children: new List<Widget>()
                                );
                                nextContext.parentElement = linkContainer;
                                nextContext.rootWidgetList.Add(item: linkContainer);
                            }
                            else {
                                var _linkStyle = parseContext.childStyle.merge(other: this.linkStyle);
                                var span = new LinkTextSpan(
                                    style: _linkStyle,
                                    url: url,
                                    onLinkTap: this.onLinkTap,
                                    children: new List<InlineSpan>()
                                );
                                if (parseContext.parentElement is TextSpan textSpan) {
                                    textSpan.children.Add(item: span);
                                }
                                else {
                                    // start a new block element for this link and its text
                                    var blockElement = new BlockText(
                                        shrinkToFit: this.shrinkToFit,
                                        margin: EdgeInsets.only(
                                            parseContext.indentLevel * this.indentSize, 10.0f),
                                        child: new RichText(text: span)
                                    );
                                    parseContext.rootWidgetList.Add(item: blockElement);
                                    nextContext.inBlock = true;
                                }

                                nextContext.childStyle = this.linkStyle;
                                nextContext.parentElement = span;
                            }

                            break;

                        case "br":
                            if (parseContext.parentElement != null &&
                                parseContext.parentElement is TextSpan _textSpan) {
                                _textSpan.children.Add(new TextSpan("\n", children: new List<InlineSpan>()));
                            }

                            break;

                        case "table":
                            // new block, so clear out the parent element
                            parseContext.parentElement = null;
                            nextContext.parentElement = new Column(
                                crossAxisAlignment: CrossAxisAlignment.start,
                                children: new List<Widget>()
                            );
                            nextContext.rootWidgetList.Add(new Container(
                                margin: EdgeInsets.symmetric(12.0f),
                                child: nextContext.parentElement as Column));
                            break;

                        // we don't handle tbody, thead, or tfoot elements separately for now
                        case "tbody":
                        case "thead":
                        case "tfoot":
                            break;

                        case "td":
                        case "th":
                            var colspan = 1;
                            if (node.Attributes["colspan"] != null) {
                                int.TryParse(s: node.Attributes["colspan"].Value, result: out colspan);
                            }

                            nextContext.childStyle = nextContext.childStyle.merge(new TextStyle(
                                fontWeight: (node.Name == "th")
                                    ? FontWeight.bold
                                    : FontWeight.normal));
                            var span1 = new TextSpan("", children: new List<InlineSpan>());
                            var _text = new RichText(text: span1);
                            var _cell = new Expanded(
                                flex: colspan,
                                child: new Container(padding: EdgeInsets.all(1.0f), child: _text)
                            );
                            if (nextContext.parentElement is Column column) {
                                column.children.Add(item: _cell);
                            }
                            else if (nextContext.parentElement is Row row1) {
                                row1.children.Add(item: _cell);
                            }
                            else if (nextContext.parentElement is TextSpan _span) {
                                _span.children.Add(item: span1);
                            }
                            else {
                                Debug.LogError("nextContext.parentElement is " + nextContext.parentElement);
                            }

                            nextContext.parentElement = _text.text;
                            break;

                        case "tr":
                            var _row = new Row(
                                crossAxisAlignment: CrossAxisAlignment.center,
                                children: new List<Widget>()
                            );
                            (nextContext.parentElement as Column).children.Add(item: _row);
                            nextContext.parentElement = _row;
                            break;

                        // treat captions like a row with one expanded cell
                        case "caption":
                            // create the row
                            var row = new Row(
                                crossAxisAlignment: CrossAxisAlignment.center,
                                children: new List<Widget>()
                            );

                            // create an expanded cell
                            var text = new RichText(
                                textAlign: TextAlign.center,
                                textScaleFactor: 1.2f,
                                text: new TextSpan("", children: new List<InlineSpan>()));
                            var cell = new Expanded(
                                child: new Container(padding: EdgeInsets.all(2.0f), child: text)
                            );
                            row.children.Add(item: cell);
                            (nextContext.parentElement as Column).children.Add(item: row);
                            nextContext.parentElement = text.text;
                            break;
                        case "q":
                            if (parseContext.parentElement != null &&
                                parseContext.parentElement is TextSpan __textSpan) {
                                __textSpan.children
                                    .Add(new TextSpan("\"", children: new List<InlineSpan>()));
                                var content = new TextSpan("", children: new List<InlineSpan>());
                                __textSpan.children.Add(item: content);
                                __textSpan.children
                                    .Add(new TextSpan("\"", children: new List<InlineSpan>()));
                                nextContext.parentElement = content;
                            }

                            break;
                    }

                    if (this.customTextStyle != null) {
                        var customStyle = this.customTextStyle(node: node, baseStyle: nextContext.childStyle);
                        if (customStyle != null) {
                            nextContext.childStyle = customStyle;
                        }
                    }
                }

                // handle block elements
                else if (_supportedBlockElements.Contains(item: node.Name) || node.NodeType == HtmlNodeType.Document) {
                    // block elements only show up at the "root" widget level
                    // so if we have a block element, reset the parentElement to null
                    parseContext.parentElement = null;
                    var textAlign = TextAlign.left;
                    if (this.customTextAlign != null) {
                        textAlign = this.customTextAlign(elem: node) ?? textAlign;
                    }

                    EdgeInsets _customEdgeInsets = null;
                    if (this.customEdgeInsets != null) {
                        _customEdgeInsets = this.customEdgeInsets(node: node);
                    }

                    switch (node.Name) {
                        case "hr":
                            parseContext.rootWidgetList
                                .Add(new CustomDivider(height: 1.0f, color: CColors.Black));
                            break;
                        case "img":
                            if (this.showImages) {
                                if (node.Attributes["src"] != null) {
                                    var width = this.imageProperties?.width ??
                                                ((node.Attributes["width"] != null)
                                                    ? float.Parse(node.Attributes["width"].Value.Substring(0,
                                                        node.Attributes["width"].Value
                                                            .LastIndexOfAny("01234567890".ToCharArray()) + 1))
                                                    : (float?) null);
                                    var height = this.imageProperties?.height ??
                                                 ((node.Attributes["height"] != null)
                                                     ? float.Parse(node.Attributes["height"].Value.Substring(0,
                                                         node.Attributes["height"].Value
                                                             .LastIndexOfAny("01234567890".ToCharArray()) + 1))
                                                     : (float?) null);

                                    if (node.Attributes["src"].Value.StartsWith("data:image") &&
                                        node.Attributes["src"].Value.Contains("base64,")) {
                                        parseContext.rootWidgetList.Add(new GestureDetector(
                                            child: Image.memory(
                                                Convert.FromBase64String(
                                                    node.Attributes["src"].Value.Split(new string[] {"base64,"},
                                                        options: StringSplitOptions.None)[1].Trim()),
                                                width: (width ?? -1) > 0 ? width : null,
                                                height: (height ?? -1) > 0 ? width : null,
                                                scale: this.imageProperties?.scale ?? 1.0f,
                                                centerSlice: this.imageProperties?.centerSlice,
                                                filterQuality: this.imageProperties?.filterQuality ??
                                                               FilterQuality.low,
                                                alignment: this.imageProperties?.alignment ?? Alignment.center,
                                                colorBlendMode: this.imageProperties?.colorBlendMode ??
                                                                BlendMode.srcOver,
                                                fit: this.imageProperties?.fit,
                                                color: this.imageProperties?.color,
                                                repeat: this.imageProperties?.repeat ?? ImageRepeat.noRepeat
                                            ),
                                            onTap: () => {
                                                if (this.onImageTap != null) {
                                                    this.onImageTap(source: node.Attributes["src"].Value);
                                                }
                                            }
                                        ));
                                    }
                                    else if (node.Attributes["src"].Value.StartsWith("asset:")) {
                                        var assetPath = node.Attributes["src"].Value.Substring(6);
                                        parseContext.rootWidgetList.Add(new GestureDetector(
                                            child: Image.file(
                                                file: assetPath,
                                                width: (width ?? -1) > 0 ? width : null,
                                                height: (height ?? -1) > 0 ? height : null,
                                                scale: this.imageProperties?.scale ?? 1.0f,
                                                centerSlice: this.imageProperties?.centerSlice,
                                                filterQuality: this.imageProperties?.filterQuality ?? FilterQuality.low,
                                                alignment: this.imageProperties?.alignment ?? Alignment.center,
                                                colorBlendMode: this.imageProperties?.colorBlendMode ??
                                                                BlendMode.srcOver,
                                                fit: this.imageProperties?.fit,
                                                color: this.imageProperties?.color,
                                                repeat: this.imageProperties?.repeat ?? ImageRepeat.noRepeat
                                            ),
                                            onTap: () => {
                                                if (this.onImageTap != null) {
                                                    this.onImageTap(source: node.Attributes["src"].Value);
                                                }
                                            }
                                        ));
                                    }
                                    else {
                                        parseContext.rootWidgetList.Add(new GestureDetector(
                                            child: Image.network(
                                                src: node.Attributes["src"].Value,
                                                width: (width ?? -1) > 0 ? width : null,
                                                height: (height ?? -1) > 0 ? height : null,
                                                scale: this.imageProperties?.scale ?? 1.0f,
                                                centerSlice: this.imageProperties?.centerSlice,
                                                filterQuality: this.imageProperties?.filterQuality ?? FilterQuality.low,
                                                alignment: this.imageProperties?.alignment ?? Alignment.center,
                                                colorBlendMode: this.imageProperties?.colorBlendMode ??
                                                                BlendMode.srcOver,
                                                fit: this.imageProperties?.fit,
                                                color: this.imageProperties?.color,
                                                repeat: this.imageProperties?.repeat ?? ImageRepeat.noRepeat
                                            ),
                                            onTap: () => {
                                                if (this.onImageTap != null) {
                                                    this.onImageTap(source: node.Attributes["src"].Value);
                                                }
                                            }
                                        ));
                                    }
                                }
                            }

                            break;
                        case "li":
                            var leadingChar = parseContext.listChar;
                            if (parseContext.blockType == "ol") {
                                // nextContext will handle nodes under this 'li'
                                // but we want to increment the count at this level
                                parseContext.listCount += 1;
                                leadingChar = parseContext.listCount.ToString() + '.';
                            }

                            var _blockText = new BlockText(
                                shrinkToFit: this.shrinkToFit,
                                margin: EdgeInsets.only(
                                    parseContext.indentLevel * this.indentSize, 3.0f),
                                child: new RichText(
                                    text: new TextSpan(
                                        $"{leadingChar}  ",
                                        style: DefaultTextStyle.of(context: buildContext).style,
                                        new List<InlineSpan> {
                                            new TextSpan("", style: nextContext.childStyle)
                                        }
                                    )
                                )
                            );
                            parseContext.rootWidgetList.Add(item: _blockText);
                            nextContext.parentElement = _blockText.child.text;
                            nextContext.spansOnly = true;
                            nextContext.inBlock = true;
                            break;

                        case "h1":
                            nextContext.childStyle = nextContext.childStyle.merge(
                                new TextStyle(fontSize: 26.0f, fontWeight: FontWeight.bold)
                            );
                            goto myDefault;
                        case "h2":
                            nextContext.childStyle = nextContext.childStyle.merge(
                                new TextStyle(fontSize: 24.0f, fontWeight: FontWeight.bold)
                            );
                            goto myDefault;
                        case "h3":
                            nextContext.childStyle = nextContext.childStyle.merge(
                                new TextStyle(fontSize: 22.0f, fontWeight: FontWeight.bold)
                            );
                            goto myDefault;
                        case "h4":
                            nextContext.childStyle = nextContext.childStyle.merge(
                                new TextStyle(fontSize: 20.0f, fontWeight: FontWeight.w100)
                            );
                            goto myDefault;
                        case "h5":
                            nextContext.childStyle = nextContext.childStyle.merge(
                                new TextStyle(fontSize: 18.0f, fontWeight: FontWeight.bold)
                            );
                            goto myDefault;
                        case "h6":
                            nextContext.childStyle = nextContext.childStyle.merge(
                                new TextStyle(fontSize: 18.0f, fontWeight: FontWeight.w100)
                            );
                            goto myDefault;

                        case "pre":
                            nextContext.condenseWhitespace = false;
                            goto myDefault;

                        case "center":
                            textAlign = TextAlign.center;
                            // no break here
                            goto myDefault;

                        default:
                            myDefault:
                            Decoration decoration = null;
                            if (parseContext.blockType == "blockquote") {
                                decoration = new BoxDecoration(
                                    border: new Border(left: new BorderSide(color: CColors.Black, 2.0f))
                                );
                                nextContext.childStyle = nextContext.childStyle.merge(new TextStyle(
                                    fontStyle: FontStyle.italic
                                ));
                            }

                            var blockText = new BlockText(
                                shrinkToFit: this.shrinkToFit,
                                margin: node.Name != "body"
                                    ? _customEdgeInsets ??
                                      EdgeInsets.only(
                                          top: 8.0f,
                                          bottom: 8.0f,
                                          left: parseContext.indentLevel * this.indentSize)
                                    : EdgeInsets.zero,
                                padding: EdgeInsets.all(2.0f),
                                decoration: decoration,
                                child: new RichText(
                                    textAlign: textAlign,
                                    text: new TextSpan(
                                        "",
                                        style: nextContext.childStyle,
                                        new List<InlineSpan>()
                                    )
                                )
                            );
                            parseContext.rootWidgetList.Add(item: blockText);
                            nextContext.parentElement = blockText.child.text;
                            nextContext.spansOnly = true;
                            nextContext.inBlock = true;
                            break;
                    }

                    if (this.customTextStyle != null) {
                        var customStyle = this.customTextStyle(node: node, baseStyle: nextContext.childStyle);
                        if (customStyle != null) {
                            nextContext.childStyle = customStyle;
                        }
                    }
                }

                foreach (var childNode in node.ChildNodes) {
                    this._parseNode(node: childNode, parseContext: nextContext, buildContext: buildContext);
                }
            }
        }

        public string condenseHtmlWhitespace(string stringToTrim) {
            stringToTrim = stringToTrim.replaceAll("\n", " ");
            while (stringToTrim.IndexOf("  ") != -1) {
                stringToTrim = stringToTrim.replaceAll("  ", " ");
            }

            return stringToTrim;
        }
    }
}