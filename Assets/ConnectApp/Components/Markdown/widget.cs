using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ConnectApp.Common.Visual;
using ConnectApp.Components.Markdown.basic;
using ConnectApp.Models.Model;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Element = ConnectApp.Components.Markdown.basic.Element;

namespace ConnectApp.Components.Markdown {
    public delegate void MarkdownTapLinkCallback(string href);

    public delegate Widget MarkdownImageBuilder(Uri uri);

    public delegate Widget MarkdownCheckboxBuilder(bool value);

    public abstract class SyntaxHighlighter {
        // ignore: one_member_abstracts
        /// Returns the formatted [TextSpan] for the given string.
        public abstract TextSpan format(string source);
    }

    public class CSharpSyntaxHighlighter : SyntaxHighlighter {
        // Highlighter highlighter = new Highlighter();

        public override TextSpan format(string source) {
            // TODO: Finn Code Highlight
            // return this.highlighter.Highlight("C#", input: source);
            return new TextSpan(text: source, style: CTextStyle.PCodeStyle);
        }
    }


    public abstract class MarkdownWidget : StatefulWidget {
        protected MarkdownWidget(
            Key key,
            string data,
            MarkdownStyleSheet markdownStyleSheet,
            SyntaxHighlighter syntaxHighlighter1,
            MarkdownTapLinkCallback onTapLink,
            string imageDirectory,
            ExtensionSet extensionSet,
            MarkdownImageBuilder imageBuilder,
            MarkdownCheckboxBuilder checkboxBuilder,
            bool fitContent = false,
            bool selectable = false,
            bool enableHTML = true,
            Dictionary<string, VideoSliceMap> videoSlices = null,
            Dictionary<string, string> videoPosterMap = null,
            Action<string> browserImageInMarkdown = null,
            Action<string, string, int> playVideo = null,
            Action loginAction = null,
            List<Node> nodes = null
        ) : base(key: key) {
            Debug.Assert(data != null);
            this.data = data;
            this.styleSheet = markdownStyleSheet;
            this.syntaxHighlighter = syntaxHighlighter1;
            this.onTapLink = onTapLink;
            this.extensionSet = extensionSet;
            this.imageDirectory = imageDirectory;
            this.imageBuilder = imageBuilder;
            this.checkboxBuilder = checkboxBuilder;
            this.fitContent = fitContent;
            this.selectable = selectable;
            this.enableHTML = enableHTML;
            this.browserImageInMarkdown = browserImageInMarkdown;
            this.videoSlices = videoSlices;
            this.videoPosterMap = videoPosterMap;
            this.playVideo = playVideo;
            this.loginAction = loginAction;
            this.nodes = nodes ?? new List<Node>();
        }

        public string data;
        
        public List<Node> nodes;

        public bool selectable;

        public MarkdownStyleSheet styleSheet;

//        public MarkdownStyleSheetBaseTheme 

        public SyntaxHighlighter syntaxHighlighter;

        public MarkdownTapLinkCallback onTapLink;

        public string imageDirectory;

        public ExtensionSet extensionSet;

        public MarkdownImageBuilder imageBuilder;

        public MarkdownCheckboxBuilder checkboxBuilder;

        public bool fitContent;

        public Action<string> browserImageInMarkdown;

        public Dictionary<string, VideoSliceMap> videoSlices;

        public Dictionary<string, string> videoPosterMap;

        public Action<string, string, int> playVideo;

        public Action loginAction;

        public abstract Widget build(BuildContext context, List<Widget> children);

        public bool enableHTML;

        public override State createState() {
            return new _MarkdownWidgetState();
        }
    }

    class _MarkdownWidgetState : State<MarkdownWidget>, IMarkdownBuilderDelegate {
        List<Widget> _children = new List<Widget>();
        public List<GestureRecognizer> _recognizers = new List<GestureRecognizer> { };

        public override void didChangeDependencies() {
            this._buildMarkdown();
            base.didChangeDependencies();
        }
        
        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget: oldWidget);
            if (oldWidget == null) {
                return;
            }
        
            if (this.widget.data != (oldWidget as MarkdownWidget)?.data) {
                Debug.Log(">>> data change");
            }
        
            // if (this.widget.styleSheet != (oldWidget as MarkdownWidget)?.styleSheet) {
            //     this._buildMarkdown();
            //     Debug.Log(">>> styleSheet change");
            // }
        }

        public override void dispose() {
            this._disposeRecognizers();
            base.dispose();
        }

        void _buildMarkdown() {
            var styleSheet = this.widget.styleSheet ?? MarkdownStyleSheet.fromTheme();
            this._disposeRecognizers();
            var builder = new MarkdownBuilder(
                buildContext: this.context,
                this,
                selectable: this.widget.selectable,
                styleSheet: styleSheet, // todo merge fallback style sheet
                imageDirectory: this.widget.imageDirectory,
                imageBuilder: this.widget.imageBuilder,
                checkboxBuilder: this.widget.checkboxBuilder,
                fitContent: this.widget.fitContent,
                enableHTML: this.widget.enableHTML,
                browserImageInMarkdown: this.widget.browserImageInMarkdown,
                videoSlices: this.widget.videoSlices,
                videoPosterMap: this.widget.videoPosterMap,
                playVideo: this.widget.playVideo,
                loginAction: this.widget.loginAction
            );
            this._children = builder.build(nodes: this.widget.nodes);
        }
        
        void _disposeRecognizers() {
            if (this._recognizers.isEmpty()) {
                return;
            }

            var localRecognizers = this._recognizers.ToArray();
            this._recognizers.Clear();
            foreach (var r in localRecognizers) {
                r.dispose();
            }
        }

        public override Widget build(BuildContext context) {
            return this.widget.build(context: context, children: this._children);
        }

        public GestureRecognizer createLink(string href) {
            var recognizer = new TapGestureRecognizer();
            recognizer.onTap = () => { this.widget.onTapLink?.Invoke(href: href); };
            this._recognizers.Add(item: recognizer);
            return recognizer;
        }

        public TextSpan formatText(MarkdownStyleSheet styleSheet, string code) {
            //TODO: format code!
            if (this.widget.syntaxHighlighter != null) {
                return this.widget.syntaxHighlighter.format(source: code);
            }

            return new TextSpan(text: code, style: styleSheet.code);
        }
    }

    public class Markdown : MarkdownWidget {
        public Markdown(
            Key key = null,
            string data = null,
            List<Node> nodes = null,
            bool selectable = false,
            MarkdownStyleSheet markdownStyleSheet = null,
            SyntaxHighlighter syntaxHighlighter = null,
            MarkdownTapLinkCallback onTapLink = null,
            string imageDirectory = null,
            ExtensionSet extensionSet = null,
            MarkdownImageBuilder imageBuilder = null,
            MarkdownCheckboxBuilder checkboxBuilder = null,
            Action<string> browserImageInMarkdown = null,
            ScrollPhysics physics = null,
            bool shrinkWrap = false,
            bool enableHTML = false
        ) : base(key: key, data: data, markdownStyleSheet: markdownStyleSheet, syntaxHighlighter1: syntaxHighlighter,
            onTapLink: onTapLink, imageDirectory: imageDirectory, extensionSet: extensionSet,
            imageBuilder: imageBuilder,
            checkboxBuilder: checkboxBuilder, fitContent: selectable, selectable: enableHTML,
            browserImageInMarkdown: browserImageInMarkdown, nodes: nodes) {
            this.padding = EdgeInsets.all(16);
            this.physics = physics;
            this.shrinkWrap = shrinkWrap;
        }

        public EdgeInsets padding;

        public ScrollPhysics physics;

        public bool shrinkWrap;

        public override Widget build(BuildContext context, List<Widget> children) {
            return new ListView(
                padding: this.padding,
                physics: this.physics,
                shrinkWrap: this.shrinkWrap,
                children: children
            );
        }
    }

    class TaskListSyntax : InlineSyntax {
        const string _pattern = @"^ *\[([ xX])\] +";

        public TaskListSyntax() : base(pattern: _pattern) {
        }

        internal override bool onMatch(InlineParser parser, Match match) {
            var el = Element.withTag("input");
            el.attributes["type"] = "checkbox";
            el.attributes["disabled"] = "true";
            el.attributes["checked"] = match.Groups[1].Value.ToLower();
            parser.addNode(node: el);
            return true;
        }
    }
}