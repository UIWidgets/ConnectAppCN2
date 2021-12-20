using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ConnectApp.Common.Constant;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Models.Model;
using Newtonsoft.Json;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Color = Unity.UIWidgets.ui.Color;
using FontStyle = Unity.UIWidgets.ui.FontStyle;
using Image = Unity.UIWidgets.widgets.Image;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace ConnectApp.Components {
    public static class ContentDescription {
        const int codeBlockNumber = 10;
        static readonly Color codeBlockBackgroundColor = CColors.CodeBackground;
        public static readonly List<string> imageUrls = new List<string>();
        // static readonly Highlighter highlighter = new Highlighter();

        public static List<Widget> map(BuildContext context, string cont, Dictionary<string, ContentData> contentMap,
            Dictionary<string, VideoSliceMap> videoSliceMap, Dictionary<string, string> videoPosterMap,
            Action<string> openUrl, Action<string, string, int> playVideo, Action loginAction, string licence,
            Action<string> browserImage = null, DetailContent detailContent = null) {
            if (cont == null || contentMap == null) {
                return new List<Widget>();
            }

            imageUrls.Clear();

            DetailContent content; 
            if (detailContent != null) {
                content = detailContent;
            }
            else {
                content = JsonConvert.DeserializeObject<DetailContent>(value: cont);
            }
            
            var widgets = new List<Widget>();

            var orderedIndex = 1;
            var blocks = content.blocks;
            for (var i = 0; i < blocks.Count; i++) {
                var block = blocks[index: i];
                var type = block.type;
                var text = block.text;
                if (text.isEmpty()) {
                    continue;
                }

                if (text.Contains("\u0000")) {
                    text = text.Replace("\u0000", "");
                }

                switch (type) {
                    case "header-one": {
                        var inlineSpans = _RichStyle(text: text, entityMap: content.entityMap,
                            entityRanges: block.entityRanges,
                            inlineStyleRanges: block.inlineStyleRanges, openUrl: openUrl, style: CTextStyle.H4);
                        widgets.Add(_H1(text: text, inlineSpans: inlineSpans));
                    }
                        break;
                    case "header-two": {
                        var inlineSpans = _RichStyle(text: text, entityMap: content.entityMap,
                            entityRanges: block.entityRanges,
                            inlineStyleRanges: block.inlineStyleRanges, openUrl: openUrl, style: CTextStyle.H5);
                        widgets.Add(_H2(text: text, inlineSpans: inlineSpans));
                    }
                        break;
                    case "blockquote": {
                        var inlineSpans = _RichStyle(text: text, entityMap: content.entityMap,
                            entityRanges: block.entityRanges,
                            inlineStyleRanges: block.inlineStyleRanges, openUrl: openUrl,
                            style: CTextStyle.PXLargeBody4);
                        widgets.Add(_QuoteBlock(text: text, inlineSpans: inlineSpans));
                    }
                        break;
                    case "code-block": {
                        var codeBlockList = _CodeBlock(text: text);
                        if (codeBlockList.Count > 0) {
                            foreach (var widget in codeBlockList) {
                                widgets.Add(item: widget);
                            }
                        }
                    }
                        break;
                    case "unstyled": {
                        if (text.isNotEmpty()) {
                            var inlineSpans = _RichStyle(text: text, entityMap: content.entityMap,
                                entityRanges: block.entityRanges,
                                inlineStyleRanges: block.inlineStyleRanges, openUrl: openUrl);
                            widgets.Add(_Unstyled(text: text, inlineSpans: inlineSpans));
                        }
                        else if (text == "") {
                            var child = new Container(
                                color: CColors.White,
                                child: new Text("" + Environment.NewLine, style: CTextStyle.PXLarge));
                            widgets.Add(item: child);
                        }
                    }
                        break;
                    case "unordered-list-item": {
                        var isFirst = true;
                        if (i > 0) {
                            var beforeBlock = blocks[i - 1];
                            isFirst = beforeBlock.type != "unordered-list-item";
                        }

                        var isLast = true;
                        if (i < blocks.Count - 1) {
                            var afterBlock = blocks[i + 1];
                            isLast = afterBlock.type != "unordered-list-item";
                        }

                        var inlineSpans = _RichStyle(text: text, entityMap: content.entityMap,
                            entityRanges: block.entityRanges,
                            inlineStyleRanges: block.inlineStyleRanges, openUrl: openUrl);
                        widgets.Add(
                            _UnorderedList(
                                text: block.text,
                                isFirst: isFirst,
                                isLast: isLast,
                                inlineSpans: inlineSpans
                            )
                        );
                    }
                        break;
                    case "ordered-list-item": {
                        var isFirst = true;
                        if (i > 0) {
                            var beforeBlock = blocks[i - 1];
                            isFirst = beforeBlock.type != "ordered-list-item";
                        }

                        if (isFirst) {
                            orderedIndex = 1;
                        }
                        else {
                            orderedIndex++;
                        }

                        var isLast = true;
                        if (i < blocks.Count - 1) {
                            var afterBlock = blocks[i + 1];
                            isLast = afterBlock.type != "ordered-list-item";
                        }

                        var inlineSpans = _RichStyle(text: text, entityMap: content.entityMap,
                            entityRanges: block.entityRanges,
                            inlineStyleRanges: block.inlineStyleRanges, openUrl: openUrl);
                        widgets.Add(_OrderedList(text: text, index: orderedIndex, isLast: isLast,
                            inlineSpans: inlineSpans));
                    }
                        break;
                    case "atomic": {
                        var key = block.entityRanges.first().key.ToString();
                        if (content.entityMap.ContainsKey(key: key)) {
                            var dataMap = content.entityMap[key: key];
                            var data = dataMap.data;
                            if (data.contentId.isNotEmpty()) {
                                if (contentMap.ContainsKey(key: data.contentId)) {
                                    var map = contentMap[key: data.contentId];
                                    var url = map.url;
                                    var attachmentId = map.attachmentId ?? "";
                                    var downloadUrl = map.downloadUrl ?? "";
                                    var contentType = map.contentType ?? "";
                                    var originalImage = map.originalImage ?? map.thumbnail;
                                    var verifyType = "empty";
                                    var limitSeconds = 0;
                                    var videoStatus = "completed";
                                    if (videoSliceMap != null && videoSliceMap.isNotEmpty() &&
                                        videoSliceMap.ContainsKey(key: map.attachmentId)) {
                                        var videoSlice = videoSliceMap[key: map.attachmentId];
                                        videoStatus = videoSlice.status;
                                        if (videoSlice.canWatch == false) {
                                            verifyType = videoSlice.verifyType;
                                            limitSeconds = videoSlice.limitSeconds;
                                        }
                                    }

                                    var videoPoster = "";
                                    if (videoPosterMap != null && videoPosterMap.isNotEmpty() &&
                                        videoPosterMap.ContainsKey(key: map.attachmentId)) {
                                        videoPoster = videoPosterMap[key: map.attachmentId];
                                    }

                                    widgets.Add(_Atomic(context: context, type: dataMap.type, contentType: contentType,
                                        title: data.title, dataUrl: data.url,
                                        originalImage: originalImage, videoStatus: videoStatus,
                                        videoPoster: videoPoster,
                                        url: url, downloadUrl: downloadUrl, attachmentId: attachmentId
                                        , openUrl: openUrl, playVideo: playVideo, loginAction: loginAction,
                                        verifyType: verifyType, limitSeconds: limitSeconds,
                                        browserImage: browserImage));
                                }
                            }
                        }
                    }
                        break;
                }
            }

            return widgets;
        }

        static Widget _H1(string text, List<InlineSpan> inlineSpans) {
            if (text == null) {
                return new Container();
            }

            Widget child;
            if (inlineSpans != null) {
                child = new RichText(
                    text: new TextSpan(
                        children: inlineSpans
                    )
                );
            }
            else {
                child = new Text(
                    data: text,
                    style: CTextStyle.H4
                );
            }

            return new Container(
                color: CColors.White,
                padding: EdgeInsets.only(16, 16, 16, 24),
                child: new TipMenu(
                    new List<TipMenuItem> {
                        new TipMenuItem(
                            "复制",
                            () => Clipboard.setData(new ClipboardData(text: text))
                        )
                    },
                    child: child
                )
            );
        }

        static Widget _H2(string text, List<InlineSpan> inlineSpans) {
            if (text == null) {
                return new Container();
            }

            Widget child;
            if (inlineSpans != null) {
                child = new RichText(
                    text: new TextSpan(
                        children: inlineSpans
                    )
                );
            }
            else {
                child = new Text(
                    data: text,
                    style: CTextStyle.H5
                );
            }

            return new Container(
                color: CColors.White,
                padding: EdgeInsets.only(16, 16, 16, 24),
                child: new TipMenu(
                    new List<TipMenuItem> {
                        new TipMenuItem(
                            "复制",
                            () => Clipboard.setData(new ClipboardData(text: text))
                        )
                    },
                    child: child
                )
            );
        }

        static Widget _Unstyled(string text, List<InlineSpan> inlineSpans) {
            if (text == null) {
                return new Container();
            }

            Widget child;
            if (inlineSpans != null) {
                child = new RichText(
                    text: new TextSpan(
                        children: inlineSpans
                    )
                );
            }
            else {
                child = new Text(
                    data: text,
                    style: CTextStyle.PXLarge
                );
            }

            return new TipMenu(
                new List<TipMenuItem> {
                    new TipMenuItem(
                        "复制",
                        () => Clipboard.setData(new ClipboardData(text: text))
                    )
                },
                new Container(
                    color: CColors.White,
                    padding: EdgeInsets.only(16, right: 16, bottom: 24),
                    child: child
                )
            );
        }

        static List<Widget> _CodeBlock(string text) {
            var codeBlockList = new List<Widget>();
            if (text.isEmpty()) {
                codeBlockList.Add(new Container());
            }
            else {
                var codeStringList = text.Split(Environment.NewLine.ToCharArray());
                codeBlockList.Add(new Container(color: codeBlockBackgroundColor, height: 16));
                for (var i = 0; i < codeStringList.Length; i++) {
                    var codeBlockGroup = "";
                    for (var j = 0; j < codeBlockNumber && i < codeStringList.Length; j++) {
                        codeBlockGroup += codeStringList[i];
                        if (i == codeStringList.Length - 1 && codeStringList.Length % codeBlockNumber != 0) {
                            break;
                        }

                        if (j < codeBlockNumber - 1) {
                            codeBlockGroup += Environment.NewLine;
                            i++;
                        }
                    }

                    var codeWidget = new TipMenu(
                        new List<TipMenuItem> {
                            new TipMenuItem(
                                "复制",
                                () => Clipboard.setData(new ClipboardData(text: codeBlockGroup))
                            )
                        },
                        new Container(
                            color: codeBlockBackgroundColor,
                            padding: EdgeInsets.symmetric(horizontal: 16),
                            child: new RichText(
                                // TODO: Finn Code Highlight
                                // text: highlighter.Highlight("C#", input: codeBlockGroup)
                                text: new TextSpan(text: codeBlockGroup, style: CTextStyle.PCodeStyle)
                            )
                        )
                    );
                    codeBlockList.Add(item: codeWidget);
                }

                codeBlockList.Add(new Container(color: codeBlockBackgroundColor, height: 16));
                codeBlockList.Add(new Container(color: CColors.White, height: 24));
            }

            return codeBlockList;
        }

        static Widget _QuoteBlock(string text, List<InlineSpan> inlineSpans) {
            Widget child;
            if (inlineSpans != null) {
                child = new RichText(
                    text: new TextSpan(
                        children: inlineSpans
                    )
                );
            }
            else {
                child = new Text(
                    data: text,
                    style: CTextStyle.PXLargeBody4
                );
            }

            return new TipMenu(
                new List<TipMenuItem> {
                    new TipMenuItem("复制", () => Clipboard.setData(new ClipboardData(text: text)))
                },
                new Container(
                    color: CColors.White,
                    padding: EdgeInsets.only(16, right: 16, bottom: 24),
                    child: new Container(
                        decoration: new BoxDecoration(
                            color: CColors.White,
                            border: new Border(
                                left: new BorderSide(
                                    color: CColors.Separator,
                                    8
                                )
                            )
                        ),
                        padding: EdgeInsets.only(16),
                        child: child
                    )
                )
            );
        }

        static Widget _Atomic(BuildContext context, string type, string contentType, string title, string dataUrl,
            OriginalImage originalImage, string videoStatus, string videoPoster,
            string url, string downloadUrl, string attachmentId, Action<string> openUrl,
            Action<string, string, int> playVideo, Action loginAction, string verifyType, int limitSeconds,
            Action<string> browserImage = null) {
            if (type == "ATTACHMENT" && contentType != "video/mp4") {
                return new Container();
            }

            var playButton = Positioned.fill(
                new Container()
            );

            if (type == "VIDEO" || type == "ATTACHMENT") {
                playButton = Positioned.fill(
                    new Center(
                        child: videoStatus == "completed"
                            ? UserInfoManager.isLoggedIn()
                                ? new CustomButton(
                                    onPressed: () => {
                                        if (type == "ATTACHMENT") {
                                            if (url.isEmpty()) {
                                                playVideo(arg1: downloadUrl, "empty", 0);
                                            }
                                            else {
                                                playVideo($"{Config.apiAddress_cn}/playlist/{attachmentId}",
                                                    arg2: verifyType,
                                                    arg3: limitSeconds);
                                            }
                                        }
                                        else {
                                            if (url.isEmpty()) {
                                                return;
                                            }

                                            openUrl(obj: url);
                                        }
                                    },
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
                                    onTap: () => loginAction(),
                                    child: new Container(
                                        color: CColors.Black.withOpacity(0.5f),
                                        alignment: Alignment.center,
                                        child: new Text("Login to view this video",
                                            style: CTextStyle.PXLargeWhite.merge(
                                                new TextStyle(decoration: TextDecoration.underline)))
                                    ))
                            : new Container(
                                color: CColors.Black.withOpacity(0.5f),
                                alignment: Alignment.center,
                                child: new Text("Video is processing, try it later", style: CTextStyle.PXLargeWhite)
                            )
                    )
                );
            }

            var attachWidth = MediaQuery.of(context: context).size.width - 32;
            var attachHeight = attachWidth * 9 / 16;
            if (type == "ATTACHMENT") {
                Widget imageWidget = new Container(color: CColors.Black);
                if (videoPoster.isNotEmpty()) {
                    imageWidget = Image.network(
                        src: videoPoster,
                        fit: BoxFit.cover
                    );
                }
                return new Container(
                    color: CColors.White,
                    padding: EdgeInsets.only(bottom: 32),
                    alignment: Alignment.center,
                    child: new Container(
                        padding: EdgeInsets.only(16, right: 16),
                        child: new Column(
                            children: new List<Widget> {
                                new Stack(
                                    children: new List<Widget> {
                                        new Container(
                                            width: attachWidth,
                                            height: attachHeight,
                                            color: CColors.Black,
                                            child: imageWidget
                                        ),
                                        playButton
                                    }
                                )
                            }
                        )
                    )
                );
            }

            var width = originalImage.width < MediaQuery.of(context: context).size.width - 32
                ? originalImage.width
                : MediaQuery.of(context: context).size.width - 32;
            var height = width * originalImage.height / originalImage.width;
            var imageUrl = originalImage.url;
            if (imageUrl.isNotEmpty()) {
                imageUrl = CImageUtils.SizeToScreenImageUrl(imageUrl: imageUrl, buildContext: context);
                imageUrls.Add(item: imageUrl);
            }

            var nodes = new List<Widget> {
                new Stack(
                    children: new List<Widget> {
                        new GestureDetector(
                            child: new Hero(
                                tag: imageUrl,
                                child: new PlaceholderImage(
                                    imageUrl: imageUrl,
                                    width: width,
                                    height: height,
                                    fit: BoxFit.cover
                                )
                            ), onTap: () => {
                                if (dataUrl.isNotEmpty()) {
                                    openUrl(obj: dataUrl);
                                }
                                else {
                                    browserImage?.Invoke(obj: imageUrl);
                                }
                            }
                        ),
                        playButton
                    }
                )
            };
            if (title.isNotEmpty()) {
                var imageTitle = new Container(
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
                        child: new Text(
                            data: title,
                            style: CTextStyle.PRegularBody4
                        )
                    )
                );
                nodes.Add(item: imageTitle);
            }

            return new Container(
                color: CColors.White,
                padding: EdgeInsets.only(bottom: 32),
                alignment: Alignment.center,
                child: new Container(
                    padding: EdgeInsets.only(16, right: 16),
                    child: new Column(
                        children: nodes
                    )
                )
            );
        }

        static Widget _OrderedList(
            string text,
            int index,
            bool isLast,
            List<InlineSpan> inlineSpans
        ) {
            var spans = new List<InlineSpan> {
                new TextSpan(
                    $"{index}. ",
                    style: CTextStyle.PXLarge
                )
            };
            if (inlineSpans != null) {
                spans.AddRange(collection: inlineSpans);
            }
            else {
                spans.Add(new TextSpan(text: text, style: CTextStyle.PXLarge));
            }

            return new TipMenu(
                new List<TipMenuItem> {
                    new TipMenuItem("复制", () => Clipboard.setData(new ClipboardData(text: text)))
                },
                new Container(
                    color: CColors.White,
                    padding: EdgeInsets.only(16, right: 16, top: index == 1 ? 0 : 4, bottom: isLast ? 24 : 0),
                    child: new RichText(
                        text: new TextSpan(
                            style: CTextStyle.PXLarge,
                            children: spans
                        )
                    )
                )
            );
        }

        static Widget _UnorderedList(
            string text,
            bool isFirst,
            bool isLast,
            List<InlineSpan> inlineSpans
        ) {
            Widget child;
            if (inlineSpans != null) {
                child = new RichText(
                    text: new TextSpan(
                        children: inlineSpans
                    )
                );
            }
            else {
                child = new Text(
                    data: text,
                    style: CTextStyle.PXLarge
                );
            }

            var spans = new List<Widget> {
                new Container(
                    width: 8,
                    height: 8,
                    margin: EdgeInsets.only(top: 14, right: 8),
                    decoration: new BoxDecoration(
                        color: CColors.Black,
                        borderRadius: BorderRadius.all(4)
                    )
                ),
                new Expanded(
                    child: child
                )
            };

            return new TipMenu(
                new List<TipMenuItem> {
                    new TipMenuItem("复制", () => Clipboard.setData(new ClipboardData(text: text)))
                },
                new Container(
                    color: CColors.White,
                    padding: EdgeInsets.only(16, isFirst ? 0 : 4, 16, isLast ? 24 : 0),
                    child: new Row(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: spans
                    )
                )
            );
        }

        static List<InlineSpan> _RichStyle(
            string text,
            Dictionary<string, DetailContentEntity> entityMap,
            List<EntityRange> entityRanges,
            List<InlineStyleRange> inlineStyleRanges,
            Action<string> openUrl,
            TextStyle style = null
        ) {
            if (text == null) {
                return null;
            }

            // 过滤 emoji
            text = Regex.Replace(input: text, @"\p{Cs}", $"{(char) EmojiUtils.emptyEmojiCode}");

            if (entityRanges.isNullOrEmpty()
                && inlineStyleRanges.isNullOrEmpty()) {
                return null;
            }

            var newStyle = style != null ? style : CTextStyle.PXLarge;
            if (inlineStyleRanges.Count <= 0) {
                return _LinkSpans(text: text, entityMap: entityMap, entityRanges: entityRanges, openUrl: openUrl,
                    style: newStyle);
            }

            if (entityRanges.Count <= 0) {
                return _InlineStyle(text: text, inlineStyleRanges: inlineStyleRanges, style: newStyle);
            }

            var inlineStyleRange = inlineStyleRanges.first();
            var inlineOffset = inlineStyleRange.offset;
            var inlineLength = inlineStyleRange.length;
            var inlineStyle = inlineStyleRange.style;

            var entityRange = entityRanges.first();
            var key = entityRange.key.ToString();
            var data = entityMap[key: key];
            var entityOffset = entityRange.offset;
            var entityLength = entityRange.length;
            var textStyle = newStyle.merge(
                new TextStyle(
                    fontWeight: inlineStyle == "BOLD" ? FontWeight.bold : FontWeight.normal,
                    fontStyle: inlineStyle == "ITALIC" ? FontStyle.italic : FontStyle.normal,
                    decoration: inlineStyle == "UNDERLINE" ? TextDecoration.underline : TextDecoration.none
                )
            );
            var textBlueStyle = newStyle.merge(
                new TextStyle(
                    fontWeight: inlineStyle == "BOLD" ? FontWeight.bold : FontWeight.normal,
                    fontStyle: inlineStyle == "ITALIC" ? FontStyle.italic : FontStyle.normal,
                    decoration: inlineStyle == "UNDERLINE" ? TextDecoration.underline : TextDecoration.none,
                    color: CColors.PrimaryBlue
                )
            );
            var recognizer = new TapGestureRecognizer {
                onTap = () => openUrl(obj: data.data.url)
            };
            if (entityOffset >= inlineOffset + inlineLength) {
                var spans = new List<InlineSpan> {
                    new TextSpan(text.Substring(0, length: inlineOffset), style: newStyle),
                    new TextSpan(text.Substring(startIndex: inlineOffset, length: inlineLength), style: textStyle),
                    new TextSpan(
                        text.Substring(inlineOffset + inlineLength, entityOffset - inlineOffset - inlineLength),
                        style: newStyle),
                    new TextSpan(text.Substring(startIndex: entityOffset, length: entityLength),
                        newStyle.copyWith(color: CColors.PrimaryBlue),
                        recognizer: recognizer),
                    new TextSpan(text.Substring(entityOffset + entityLength, text.Length - entityOffset - entityLength),
                        style: newStyle)
                };
                return spans;
            }

            if (inlineOffset >= entityOffset + entityLength) {
                var spans = new List<InlineSpan> {
                    new TextSpan(text.Substring(0, length: entityOffset), style: newStyle),
                    new TextSpan(text.Substring(startIndex: entityOffset, length: entityLength),
                        newStyle.copyWith(color: CColors.PrimaryBlue),
                        recognizer: recognizer),
                    new TextSpan(
                        text.Substring(entityOffset + entityLength, inlineOffset - entityOffset - entityLength),
                        style: newStyle),
                    new TextSpan(text.Substring(startIndex: inlineOffset, length: inlineLength), style: textStyle),
                    new TextSpan(text.Substring(inlineOffset + inlineLength, text.Length - inlineOffset - inlineLength),
                        style: newStyle)
                };
                return spans;
            }

            if (entityOffset >= inlineOffset) {
                if (entityOffset + entityLength <= inlineOffset + inlineLength) {
                    var lastLength = inlineOffset + inlineLength - entityOffset - entityLength;
                    var spans = new List<InlineSpan> {
                        new TextSpan(text.Substring(0, length: inlineOffset), style: newStyle),
                        new TextSpan(text.Substring(startIndex: inlineOffset, entityOffset - inlineOffset),
                            style: textStyle),
                        new TextSpan(text.Substring(startIndex: entityOffset, length: entityLength),
                            style: textBlueStyle, recognizer: recognizer),
                        new TextSpan(text.Substring(entityOffset + entityLength, length: lastLength), style: textStyle),
                        new TextSpan(
                            text.Substring(inlineOffset + inlineLength, text.Length - inlineOffset - inlineLength),
                            style: newStyle)
                    };
                    return spans;
                }
            }

            return null;
        }

        static List<InlineSpan> _InlineStyle(string text, List<InlineStyleRange> inlineStyleRanges, TextStyle style) {
            if (inlineStyleRanges == null && inlineStyleRanges.Count <= 0) {
                return null;
            }

            var inlineStyleRange = inlineStyleRanges.first();
            var offset = inlineStyleRange.offset;
            var length = inlineStyleRange.length;
            var inlineStyle = inlineStyleRange.style;
            var leftText = text.Substring(0, length: offset);
            var currentText = text.Substring(startIndex: offset, length: length);
            var rightText = text.Substring(length + offset, text.Length - length - offset);
            var textStyle = style.merge(
                new TextStyle(
                    fontWeight: inlineStyle == "BOLD" ? FontWeight.bold : FontWeight.normal,
                    fontStyle: inlineStyle == "ITALIC" ? FontStyle.italic : FontStyle.normal,
                    decoration: inlineStyle == "UNDERLINE" ? TextDecoration.underline : TextDecoration.none
                )
            );
            return new List<InlineSpan> {
                new TextSpan(
                    text: leftText,
                    style: style
                ),
                new TextSpan(
                    text: currentText,
                    style: textStyle
                ),
                new TextSpan(
                    text: rightText,
                    style: style
                )
            };
        }

        static List<InlineSpan> _LinkSpans(
            string text,
            Dictionary<string, DetailContentEntity> entityMap,
            List<EntityRange> entityRanges,
            Action<string> openUrl,
            TextStyle style
        ) {
            if (entityRanges != null && entityRanges.Count > 0) {
                var linkSpans = new List<InlineSpan>();
                var startIndex = 0;
                entityRanges.ForEach(entityRange => {
                    var key = entityRange.key.ToString();
                    if (entityMap.ContainsKey(key: key)) {
                        var data = entityMap[key: key];
                        if (data.type == "LINK") {
                            var offset = entityRange.offset;
                            var length = entityRange.length;
                            var leftText = offset - startIndex <= text.Length
                                ? text.Substring(startIndex: startIndex, offset - startIndex)
                                : text;

                            var currentText = length - offset < text.Length
                                ? text.Substring(startIndex: offset, length: length)
                                : text;

                            length = currentText.Length;
                            var recognizer = new TapGestureRecognizer {
                                onTap = () => openUrl(obj: data.data.url)
                            };
                            var linkSpan = new List<TextSpan> {
                                new TextSpan(
                                    text: leftText,
                                    style: style
                                ),
                                new TextSpan(
                                    text: currentText,
                                    style.copyWith(color: CColors.PrimaryBlue),
                                    recognizer: recognizer
                                )
                            };
                            linkSpans.AddRange(collection: linkSpan);
                            startIndex = length + offset;
                        }
                    }
                });
                var rightText = text.Substring(startIndex: startIndex, text.Length - startIndex);
                linkSpans.Add(new TextSpan(text: rightText, style: style));
                return linkSpans;
            }

            return null;
        }
    }
}