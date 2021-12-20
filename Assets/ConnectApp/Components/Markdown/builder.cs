using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ConnectApp.Common.Constant;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Components.Markdown.basic;
using ConnectApp.Models.Model;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.material;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;
using Element = ConnectApp.Components.Markdown.basic.Element;
using Image = Unity.UIWidgets.widgets.Image;
using Path = System.IO.Path;
using Text = ConnectApp.Components.Markdown.basic.Text;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace ConnectApp.Components.Markdown {
    public static class builderUtil {
        static readonly List<string> _kBlockTags = new List<string>() {
            "p",
            "h1",
            "h2",
            "h3",
            "h4",
            "h5",
            "h6",
            "li",
            "blockquote",
            "pre",
            "ol",
            "ul",
            "hr",
            "table",
            "thead",
            "tbody",
            "tr"
        };

        static readonly List<string> _kListTags = new List<string>() {
            "ul",
            "ol"
        };

        public static bool _isBlockTag(string tag) {
            return _kBlockTags.Contains(item: tag);
        }

        public static bool _isListTag(string tag) {
            return _kListTags.Contains(item: tag);
        }
    }

    public class _BlockElement {
        public string tag;
        public List<Widget> children = new List<Widget>();
        public int nextListIndex = 0;

        public _BlockElement(string tag) {
            this.tag = tag;
        }
    }

    public class _TableElement {
        public List<TableRow> rows = new List<TableRow>();
    }

    public class _InlineElement {
        public string tag;
        public TextStyle style;
        public List<Widget> children = new List<Widget>();

        public _InlineElement(string tag, TextStyle style) {
            this.tag = tag;
            this.style = style;
        }
    }

    public interface IMarkdownBuilderDelegate {
        GestureRecognizer createLink(string href);
        TextSpan formatText(MarkdownStyleSheet styleSheet, string code);
    }

    public class MarkdownBuilder : NodeVisitor {
        public BuildContext buildContext;
        public IMarkdownBuilderDelegate builderDelegate;
        public bool selectable;
        public MarkdownStyleSheet styleSheet;
        public string imageDirectory;
        public MarkdownImageBuilder imageBuilder;
        public MarkdownCheckboxBuilder checkboxBuilder;
        public bool fitContent;
        Dictionary<string, VideoSliceMap> videoSlices;
        Dictionary<string, string> videoPosterMap;
        Action<string, string, int> playVideo;
        Action loginAction;
        public Action<string> browserImageInMarkdown;
        public List<string> _listIndents = new List<string>();
        public List<_BlockElement> _blocks = new List<_BlockElement>();
        public List<_TableElement> _tables = new List<_TableElement>();
        public List<_InlineElement> _inlines = new List<_InlineElement>();
        public List<GestureRecognizer> _linkHandlers = new List<GestureRecognizer>();
        public bool enableHTML;

        static readonly Regex textAlignPattern = new Regex(@"text-align: (left|center|right)");

        bool _isInBlockquote = false;

        public MarkdownBuilder(
            BuildContext buildContext,
            IMarkdownBuilderDelegate dele,
            bool selectable,
            MarkdownStyleSheet styleSheet,
            string imageDirectory,
            MarkdownImageBuilder imageBuilder,
            MarkdownCheckboxBuilder checkboxBuilder,
            Dictionary<string, VideoSliceMap> videoSlices,
            Dictionary<string, string> videoPosterMap,
            bool fitContent = false,
            bool enableHTML = false,
            Action<string> browserImageInMarkdown = null,
            Action<string, string, int> playVideo = null,
            Action loginAction = null
        ) {
            this.buildContext = buildContext;
            this.builderDelegate = dele;
            this.selectable = selectable;
            this.styleSheet = styleSheet;
            this.imageDirectory = imageDirectory;
            this.imageBuilder = imageBuilder;
            this.checkboxBuilder = checkboxBuilder;
            this.fitContent = fitContent;
            this.enableHTML = enableHTML;
            this.browserImageInMarkdown = browserImageInMarkdown;
            this.videoSlices = videoSlices;
            this.videoPosterMap = videoPosterMap;
            this.playVideo = playVideo;
            this.loginAction = loginAction;
        }

        public List<Widget> build(List<Node> nodes) {
            this._listIndents.Clear();
            this._blocks.Clear();
            this._tables.Clear();
            this._inlines.Clear();
            this._linkHandlers.Clear();
            MarkdownUtils.markdownImages.Clear();
            this._isInBlockquote = false;
            this._blocks.Add(new _BlockElement(null));

            Debug.Assert(this._tables.isEmpty());
            Debug.Assert(this._inlines.isEmpty());
            Debug.Assert(condition: !this._isInBlockquote);
            
            foreach (var node in nodes) {
                Debug.Assert(this._blocks.Count == 1);
                node.accept(this);
            }
            return this._blocks.Single().children;
        }

        public override bool visitElementBefore(Element element) {
            var tag = element.tag;
            if (builderUtil._isBlockTag(tag: tag)) {
                this._addAnonymousBlockIfNeeded();
                if (builderUtil._isListTag(tag: tag)) {
                    this._listIndents.Add(item: tag);
                }
                else if (tag == "blockquote") {
                    this._isInBlockquote = true;
                }
                else if (tag == "table") {
                    this._tables.Add(new _TableElement());
                }
                else if (tag == "tr") {
                    var length = this._tables.Single().rows.Count;
                    var decoration = this.styleSheet.tableCellsDecoration;
                    if (length == 0 || length % 2 == 1) {
                        decoration = null;
                    }

                    this._tables.Single().rows.Add(new TableRow(
                        decoration: decoration,
                        children: new List<Widget>()
                    ));
                }

                this._blocks.Add(new _BlockElement(tag: tag));
            }
            else {
                this._addParentInlineIfNeeded(tag: this._blocks.Last().tag);

                var parentStyle = this._inlines.Last().style;
                this._inlines.Add(new _InlineElement(
                    tag: tag,
                    parentStyle.merge(this.styleSheet.styles.ContainsKey(key: tag)
                        ? this.styleSheet.styles[key: tag]
                        : null)
                ));
            }

            if (tag == "a") {
                this._linkHandlers.Add(this.builderDelegate.createLink(element.attributes["href"]));
            }

            return true;
        }

        public override void visitText(Text text) {
            if (this.enableHTML && text is HTML html) {
                this.visitHTML(html: html);
                return;
            }

            if (this._blocks.Last().tag == null) {
                return;
            }

            this._addParentInlineIfNeeded(tag: this._blocks.Last().tag);

            Widget child;
            if (this._blocks.last().tag == "pre") {
                child = new NoScrollbar(
                    new CustomScrollbar(
                        child: new SingleChildScrollView(
                            scrollDirection: Axis.horizontal,
                            padding: this.styleSheet.codeBlockPadding,
                            child: this._buildRichText(
                                this.builderDelegate.formatText(
                                    styleSheet: this.styleSheet,
                                    code: text.text
                                )
                            )
                        )
                    )
                );
            }
            else {
                child = this._buildRichText(new TextSpan(
                    style: this._isInBlockquote
                        ? this._inlines.Last().style.merge(other: this.styleSheet.blockquote)
                        : this._inlines.Last().style,
                    text: text.text,
                    recognizer: this._linkHandlers.isNotEmpty() ? this._linkHandlers.Last() : null
                ));
            }

            this._inlines.Last().children.Add(item: child);
        }

        void visitHTML(HTML html) {
            // this._blocks.Last().children.Add(
            //     new HtmlView(data: html.textContent)
            // );
            // TODO: html original sample display
            if (html.text.isNotEmpty()) {
                this._blocks.Last().children.Add(
                    new Unity.UIWidgets.widgets.Text(
                        data: html.text,
                        style: CTextStyle.PRegularBody
                    )
                );
            }
        }

        public override void visitElementAfter(Element element) {
            var tag = element.tag;
            if (builderUtil._isBlockTag(tag: tag)) {
                this._addAnonymousBlockIfNeeded();

                var current = this._blocks.removeLast();
                Widget child;

                if (current.children.isNotEmpty()) {
                    child = new Column(
                        crossAxisAlignment: this.fitContent ? CrossAxisAlignment.start : CrossAxisAlignment.stretch,
                        children: current.children
                    );
                }
                else {
                    child = new SizedBox();
                }

                if (builderUtil._isListTag(tag: tag)) {
                    Debug.Assert(this._listIndents.isNotEmpty());
                    this._listIndents.removeLast();
                }
                else if (tag == "li") {
                    if (this._listIndents.isNotEmpty()) {
                        if (element.children.Count == 0) {
                            element.children.Add(new Text(""));
                        }

                        Widget bullet;
                        var el = element.children[0];
                        string elType;
                        if (el is Element && (el as Element).attributes.TryGetValue("type", value: out elType) &&
                            elType == "checkbox") {
                            var val = (el as Element).attributes["checked"] != "false";
                            bullet = this._buildCheckBox(check: val);
                        }
                        else {
                            bullet = this._buildBullet(this._listIndents.last());
                        }

                        child = new Row(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: new List<Widget> {
                                new SizedBox(
                                    width: this.styleSheet.listIndent,
                                    child: bullet
                                ),
                                new Expanded(child: child)
                            }
                        );
                    }
                }
                else if (tag == "table") {
                    child = new Table(
                        defaultColumnWidth: this.styleSheet.tableColumnWidth,
                        defaultVerticalAlignment: TableCellVerticalAlignment.middle,
                        border: this.styleSheet.tableBorder,
                        children: this._tables.removeLast().rows
                    );
                }
                else if (tag == "blockquote") {
                    this._isInBlockquote = false;
                    child = new DecoratedBox(
                        decoration: this.styleSheet.blockquoteDecoration,
                        child: new Padding(
                            padding: this.styleSheet.blockquotePadding,
                            child: child
                        )
                    );
                }
                else if (tag == "pre") {
                    child = new DecoratedBox(
                        decoration: this.styleSheet.codeBlockDecoration,
                        child: child
                    );
                }
                else if (tag == "hr") {
                    child = new DecoratedBox(
                        decoration: this.styleSheet.horizontalRuleDecoration,
                        child: child
                    );
                }

                this._addBlockChild(child: child);
            }
            else {
                var current = this._inlines.removeLast();
                var parent = this._inlines.last();

                if (tag == "img") {
                    // create an image widget for this image
                    current.children.Add(this._buildImage(element.attributes["src"], element.attributes["alt"]));
                }
                else if (tag == "video") {
                    current.children.Add(this._buildVideo(element.attributes["src"], element.attributes["alt"]));
                }
                else if (tag == "br") {
                    current.children.Add(this._buildRichText(new TextSpan("\n")));
                }
                else if (tag == "th" || tag == "td") {
                    var align = TextAlign.left;
                    string style;
                    element.attributes.TryGetValue("style", value: out style);
                    if (style == null || style.isEmpty()) {
                        align = tag == "th" ? this.styleSheet.tableHeadAlign : TextAlign.left;
                    }
                    else {
                        var match = textAlignPattern.matchAsPrefix(content: style);
                        if (match.Success) {
                            switch (match.Groups[1].Value) {
                                case "left":
                                    align = TextAlign.left;
                                    break;
                                case "center":
                                    align = TextAlign.center;
                                    break;
                                case "right":
                                    align = TextAlign.right;
                                    break;
                            }
                        }
                    }

                    var child = this._buildTableCell(this._mergeInlineChildren(inLineChildren: current.children),
                        textAlign: align
                    );
                    this._tables.Single().rows.last().children.Add(item: child);
                }
                else if (tag == "a") {
                    this._linkHandlers.removeLast();
                }

                if (current.children.isNotEmpty()) {
                    parent.children.AddRange(collection: current.children);
                }
            }
        }

        Widget _buildVideo(string src, string alt) {
            Widget videoWidget = new Container();
            var title = alt;
            Widget titleWidget = new Container();
            if (title.isNotEmpty()) {
                titleWidget = new Container(
                    decoration: new BoxDecoration(
                        border: new Border(
                            bottom: new BorderSide(
                                color: CColors.Separator,
                                2
                            )
                        )
                    ),
                    child: new Container(
                        margin: EdgeInsets.only(4, 8, 4, 4),
                        child: new Unity.UIWidgets.widgets.Text(
                            data: title,
                            style: CTextStyle.PRegularBody4
                        )
                    )
                );
            }

            if (src.StartsWith(value: Config.unity_cn_url)) {
                // unity video
                var videoId = src.Split('?').FirstOrDefault()?.Split('/').LastOrDefault();
                if (videoId.isNotEmpty() && this.videoSlices.isNotNullAndEmpty() &&
                    this.videoSlices.ContainsKey(key: videoId)) {
                    var videoSlice = this.videoSlices[key: videoId];
                    var coverImage = "";
                    if (this.videoPosterMap != null && this.videoPosterMap.isNotEmpty() &&
                        this.videoPosterMap.ContainsKey(key: videoId)) {
                        coverImage = this.videoPosterMap[key: videoId];
                    }

                    if (videoSlice != null) {
                        var playButton = Positioned.fill(
                            new Center(
                                child: videoSlice.status == "completed"
                                    ? UserInfoManager.isLoggedIn()
                                        ? new CustomButton(
                                            onPressed: () => this.playVideo(
                                                $"{Config.apiAddress_cn}/playlist/{videoId}",
                                                arg2: videoSlice.verifyType,
                                                arg3: videoSlice.limitSeconds
                                            ),
                                            child: new Container(
                                                width: 60,
                                                height: 60,
                                                decoration: new BoxDecoration(
                                                    color: CColors.H5White,
                                                    borderRadius: BorderRadius.all(30)
                                                ),
                                                child: new Icon(
                                                    icon: CIcons.play_arrow,
                                                    size: 45,
                                                    color: CColors.Icon
                                                )
                                            )
                                        )
                                        : (Widget) new GestureDetector(
                                            onTap: () => this.loginAction(),
                                            child: new Container(
                                                color: CColors.Black.withOpacity(0.5f),
                                                alignment: Alignment.center,
                                                child: new Unity.UIWidgets.widgets.Text(
                                                    "Login to view this video",
                                                    style: CTextStyle.PXLargeWhite.merge(
                                                        new TextStyle(decoration: TextDecoration.underline)
                                                    )
                                                )
                                            ))
                                    : new Container(
                                        color: CColors.Black.withOpacity(0.5f),
                                        alignment: Alignment.center,
                                        child: new Unity.UIWidgets.widgets.Text(
                                            "Video is processing, try it later",
                                            style: CTextStyle.PXLargeWhite
                                        )
                                    )
                            )
                        );

                        var ResWidth = CCommonUtils.getScreenWidth(buildContext: this.buildContext) - 32;
                        var ResHeight = ResWidth * 9 / 16;

                        return new Container(
                            color: CColors.White,
                            padding: EdgeInsets.only(bottom: 16),
                            alignment: Alignment.center,
                            child: new Container(
                                child: new Column(
                                    children: new List<Widget> {
                                        new Stack(
                                            children: new List<Widget> {
                                                new Container(
                                                    width: ResWidth,
                                                    height: ResHeight,
                                                    color: CColors.Black,
                                                    child: Image.network(
                                                        src: coverImage,
                                                        fit: BoxFit.cover
                                                    )
                                                ),
                                                playButton
                                            }
                                        ),
                                        titleWidget
                                    }
                                )
                            )
                        );
                    }
                }
            }
            else {
                // iframe video
                if (src.isUrl()) {
                    var ResWidth = CCommonUtils.getScreenWidth(buildContext: this.buildContext) - 32;
                    var ResHeight = ResWidth * 9 / 16;
                    return new Container(
                        color: CColors.White,
                        padding: EdgeInsets.only(bottom: 16),
                        alignment: Alignment.center,
                        child: new Container(
                            child: new Column(
                                children: new List<Widget> {
                                    new Stack(
                                        children: new List<Widget> {
                                            new Container(
                                                width: ResWidth,
                                                height: ResHeight,
                                                color: CColors.Black,
                                                child: new Container(
                                                    color: CColors.Black
                                                )
                                            ),
                                            Positioned.fill(
                                                new Center(
                                                    child: new CustomButton(
                                                        onPressed: () => Application.OpenURL(url: src),
                                                        child: new Container(
                                                            width: 60,
                                                            height: 60,
                                                            decoration: new BoxDecoration(
                                                                color: CColors.H5White,
                                                                borderRadius: BorderRadius.all(30)
                                                            ),
                                                            child: new Icon(
                                                                icon: CIcons.play_arrow,
                                                                size: 45,
                                                                color: CColors.Icon
                                                            )
                                                        )
                                                    )
                                                )
                                            )
                                        }
                                    ),
                                    titleWidget
                                }
                            )
                        )
                    );
                }
            }

            return videoWidget;
        }

        // _buildImage, buildCheckBox, build bullet, buildTableCell
        Widget _buildImage(string src, string alt) {
            if (!src.isUrl()) {
                return new Container();
            }

            var parts = alt.Split('!');
            var title = parts.first();
            Widget titleWidget = new Container();
            if (title.isNotEmpty()) {
                titleWidget = new Container(
                    decoration: new BoxDecoration(
                        border: new Border(
                            bottom: new BorderSide(
                                color: CColors.Separator,
                                2
                            )
                        )
                    ),
                    child: new Container(
                        margin: EdgeInsets.only(4, 8, 4, 4),
                        child: new Unity.UIWidgets.widgets.Text(
                            data: title,
                            style: CTextStyle.PRegularBody4
                        )
                    )
                );
            }

            var width = CCommonUtils.getScreenWidth(buildContext: this.buildContext) - 32;
            var height = width * 9 / 16;

            var isCustomSize = false;
            if (parts.Length == 3) {
                var size = parts.last().Split('(', ')')[1];
                var dimensions = size.Split('x');
                if (dimensions.Length == 2) {
                    var imageWidth = float.Parse(dimensions[0]);
                    var imageHeight = float.Parse(dimensions[1]);
                    width = imageWidth < MediaQuery.of(context: this.buildContext).size.width - 32
                        ? imageWidth
                        : MediaQuery.of(context: this.buildContext).size.width - 32;
                    height = width * imageHeight / imageWidth;
                    isCustomSize = true;
                }
            }

            var uri = new Uri(uriString: src);
            MarkdownUtils.markdownImages.Add(uri.ToString());
            Widget child;
            if (uri.Scheme == "http" || uri.Scheme == "https") {
                var imageUrl = uri.ToString();
                if (imageUrl.isNotEmpty()) {
                    imageUrl = !imageUrl.Contains("unity")
                        ? imageUrl
                        : CImageUtils.SuitableSizeImageUrl(imageWidth: width, imageUrl: imageUrl);
                    if (isCustomSize) {
                        child = new PlaceholderImage(
                            color: Color.white,
                            imageUrl: imageUrl,
                            width: width,
                            height: height,
                            fit: BoxFit.cover
                        );
                    }
                    else {
                        child = new PlaceholderImage(
                            imageUrl: imageUrl,
                            fit: BoxFit.cover
                        );
                    }
                }
                else {
                    child = new Container();
                }
            }
            // else if (uri.Scheme == "data") {
            //     // todo
            //     child = this._handleDataSchemeUri(uri: uri, width: width, height: height);
            // }
            // else if (uri.Scheme == "resource") {
            //     //TODO:
            //     child = Image.file(src.Substring(9), width: width, height: height);
            // }
            else {
                var filePath = this.imageDirectory == null
                    ? uri.ToString()
                    : Path.Combine(path1: this.imageDirectory, uri.ToString());
                child = Image.file(file: filePath, null, 1, width: width, height: height);
            }

            if (this._linkHandlers.isNotEmpty()) {
                var recognizer = this._linkHandlers.last() as TapGestureRecognizer;
                return new GestureDetector(null, child: child, null, null, onTap: recognizer.onTap);
            }

            var nodes = new List<Widget> {
                new Stack(
                    alignment: Alignment.center,
                    children: new List<Widget> {
                        new GestureDetector(
                            child: new Hero(
                                tag: uri.ToString(),
                                child: new ClipRRect(
                                    borderRadius: BorderRadius.circular(2),
                                    child: child
                                )
                            ),
                            onTap: () => { this.browserImageInMarkdown?.Invoke(uri.ToString()); }
                        )
                    }
                ),
                titleWidget
            };

            return new Container(
                padding: EdgeInsets.only(bottom: 8),
                alignment: Alignment.center,
                child: new Container(
                    child: new Column(
                        children: nodes
                    )
                )
            );
        }

        Widget _handleDataSchemeUri(Uri uri, float width, float height) {
            //TODO:
            return SizedBox.expand();
        }

        Widget _buildCheckBox(bool check) {
            if (this.checkboxBuilder != null) {
                return this.checkboxBuilder(value: check);
            }

            return new Padding(
                padding: EdgeInsets.only(right: 4),
                child: new Icon(
                    check ? CIcons.check_box : CIcons.check_box_outline_blank,
                    size: this.styleSheet.checkbox.fontSize,
                    color: this.styleSheet.checkbox.color
                )
            );
        }

        Widget _buildBullet(string listTag) {
            if (listTag == "ul") {
                return new Unity.UIWidgets.widgets.Text("•", null, style: this.styleSheet.listBullet);
            }

            var index = this._blocks.last().nextListIndex;
            return new Padding(
                padding: EdgeInsets.only(right: 4),
                child: new Unity.UIWidgets.widgets.Text(
                    (index + 1) + ".",
                    style: this.styleSheet.listBullet,
                    textAlign: TextAlign.right
                )
            );
        }

        Widget _buildTableCell(List<Widget> children, TextAlign textAlign) {
            return new TableCell(
                child: new Padding(
                    padding: this.styleSheet.tableCellsPadding,
                    child: new DefaultTextStyle(
                        style: this.styleSheet.tableBody,
                        textAlign: textAlign,
                        child: new Wrap(children: children)
                    )
                )
            );
        }

        void _addParentInlineIfNeeded(string tag) {
            if (this._inlines.isEmpty()) {
                this._inlines.Add(new _InlineElement(
                    tag: tag,
                    this.styleSheet.styles[key: tag]
                ));
            }
        }

        void _addBlockChild(Widget child) {
            var parent = this._blocks.Last();
            if (parent.children.isNotEmpty()) {
                parent.children.Add(new SizedBox(height: this.styleSheet.blockSpacing));
            }

            parent.children.Add(item: child);
            parent.nextListIndex += 1;
        }

        void _addAnonymousBlockIfNeeded() {
            if (this._inlines.isEmpty()) {
                return;
            }

            var inline = this._inlines.Single();
            if (inline.children.isNotEmpty()) {
                var mergedInlines = this._mergeInlineChildren(inLineChildren: inline.children);
                var wrap = new Wrap(
                    crossAxisAlignment: WrapCrossAlignment.center,
                    children: mergedInlines
                );
                this._addBlockChild(child: wrap);
                this._inlines.Clear();
            }
        }

        List<Widget> _mergeInlineChildren(List<Widget> inLineChildren) {
            var mergedTexts = new List<Widget>();
            foreach (var child in inLineChildren) {
                if (mergedTexts.isNotEmpty() && mergedTexts.Last() is RichText && child is RichText) {
                    var previous = (RichText) mergedTexts.removeLast();
                    var previousTextSpan = previous.text as TextSpan;
                    var children = previousTextSpan.children != null
                        ? previousTextSpan.children
                        : new List<InlineSpan>() {previousTextSpan};
                    children.Add(item: (child as RichText).text);
                    var mergedSpan = new TextSpan(children: children);
                    mergedTexts.Add(this._buildRichText(text: mergedSpan));
                }
                else if (mergedTexts.isNotEmpty() &&
                         mergedTexts.Last() is SelectableText &&
                         child is SelectableText) {
                    var previous = (SelectableText) mergedTexts.removeLast();
                    var previousTextSpan = previous.textSpan;
                    var children = previousTextSpan.children != null
                        ? previousTextSpan.children
                        : new List<InlineSpan>() {previousTextSpan};
                    children.Add((child as SelectableText).textSpan);
                    var mergedSpan = new TextSpan(children: children);
                    mergedTexts.Add(this._buildRichText(mergedSpan));
                }
                else {
                    mergedTexts.Add(item: child);
                }
            }

            return mergedTexts;
        }


        Widget _buildRichText(TextSpan text) {
            if (this.selectable) {
                return SelectableText.rich(
                    textSpan: text,
                    textScaleFactor: this.styleSheet.textScaleFactor
                );
            }
            else {
                return new RichText(
                    text: text,
                    textScaleFactor: this.styleSheet.textScaleFactor
                );
            }
        }
    }
}