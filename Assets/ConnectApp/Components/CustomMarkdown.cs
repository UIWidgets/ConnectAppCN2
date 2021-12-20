using System;
using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Components.Markdown;
using ConnectApp.Components.Markdown.basic;
using ConnectApp.Components.PullToRefresh;
using ConnectApp.Models.Model;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;

namespace ConnectApp.Components {
    public class CustomMarkdown : MarkdownWidget {
        public CustomMarkdown(
            Key key = null,
            string data = null,
            bool selectable = false,
            MarkdownStyleSheet markdownStyleSheet = null,
            SyntaxHighlighter syntaxHighlighter = null,
            MarkdownTapLinkCallback onTapLink = null,
            string imageDirectory = null,
            ExtensionSet extensionSet = null,
            MarkdownImageBuilder imageBuilder = null,
            MarkdownCheckboxBuilder checkboxBuilder = null,
            ScrollPhysics physics = null,
            bool shrinkWrap = false,
            Widget contentHead = null,
            Widget tagsWidget = null,
            Widget relatedArticles = null,
            List<Widget> commentList = null,
            RefreshController refreshController = null,
            bool enablePullDown = false,
            bool enablePullUp = false,
            OnRefresh onRefresh = null,
            NotificationListenerCallback<ScrollNotification> onNotification = null,
            float initialOffset = 0f,
            bool needRebuildWithCachedCommentPosition = false,
            bool isArticleJumpToCommentStateActive = false,
            Action<string> browserImageInMarkdown = null,
            Dictionary<string, VideoSliceMap> videoSlices = null,
            Dictionary<string, string> videoPosterMap = null,
            Action<string, string, int> playVideo = null,
            Action loginAction = null,
            List<Node> nodes = null
        ) : base(
            key: key,
            data: data,
            markdownStyleSheet: markdownStyleSheet,
            syntaxHighlighter1: syntaxHighlighter,
            onTapLink: onTapLink,
            imageDirectory: imageDirectory,
            extensionSet: extensionSet,
            imageBuilder: imageBuilder,
            checkboxBuilder: checkboxBuilder,
            fitContent: selectable,
            browserImageInMarkdown: browserImageInMarkdown,
            videoSlices: videoSlices,
            videoPosterMap: videoPosterMap,
            playVideo: playVideo,
            loginAction: loginAction,
            nodes: nodes
        ) {
            this.padding = EdgeInsets.all(16);
            this.physics = physics;
            this.shrinkWrap = shrinkWrap;
            this.contentHead = contentHead;
            this.tagsWidget = tagsWidget;
            this.relatedArticles = relatedArticles;
            this.commentList = commentList;
            this.refreshController = refreshController;
            this.enablePullDown = enablePullDown;
            this.enablePullUp = enablePullUp;
            this.onRefresh = onRefresh;
            this.onNotification = onNotification;
            this.initialOffset = initialOffset;
            this.needRebuildWithCachedCommentPosition = needRebuildWithCachedCommentPosition;
            this.isArticleJumpToCommentStateActive = isArticleJumpToCommentStateActive;
        }

        public EdgeInsets padding;
        public ScrollPhysics physics;
        public bool shrinkWrap;
        public Widget contentHead;
        public Widget tagsWidget;
        public Widget relatedArticles;
        public List<Widget> commentList;
        public RefreshController refreshController;
        public bool enablePullDown;
        public bool enablePullUp;
        public OnRefresh onRefresh;
        public NotificationListenerCallback<ScrollNotification> onNotification;
        public float initialOffset;
        public bool needRebuildWithCachedCommentPosition;
        public bool isArticleJumpToCommentStateActive;

        public override Widget build(BuildContext context, List<Widget> children) {
            var commentIndex = 0;
            var originItems = new List<Widget>();
            if (this.contentHead != null) {
                originItems.Add(item: this.contentHead);
            }

            var paddingWidgets = new List<Widget>();

            children.ForEach(widget => {
                paddingWidgets.Add(new Container(color: CColors.White, padding: EdgeInsets.symmetric(horizontal: 16),
                    child: widget));
            });

            if (this.tagsWidget != null) {
                paddingWidgets.Add(new Container(color: CColors.White, padding: EdgeInsets.only(top: 24),
                    child: this.tagsWidget));
            }

            originItems.AddRange(collection: paddingWidgets);

            if (this.relatedArticles != null) {
                originItems.Add(item: this.relatedArticles);
            }

            commentIndex = originItems.Count;
            if (this.commentList.isNotNullAndEmpty()) {
                originItems.AddRange(collection: this.commentList);
            }

            commentIndex = this.isArticleJumpToCommentStateActive ? commentIndex : 0;

            if (this.needRebuildWithCachedCommentPosition == false && commentIndex != 0) {
                return new CenteredRefresher(
                    controller: this.refreshController,
                    enablePullDown: this.enablePullDown,
                    enablePullUp: this.enablePullUp,
                    onRefresh: this.onRefresh,
                    onNotification: this.onNotification,
                    children: originItems,
                    centerIndex: commentIndex
                );
            }

            return new SmartRefresher(
                initialOffset: this.initialOffset,
                controller: this.refreshController,
                enablePullDown: this.enablePullDown,
                enablePullUp: this.enablePullUp,
                onRefresh: this.onRefresh,
                onNotification: this.onNotification,
                child: ListView.builder(
                    physics: new AlwaysScrollableScrollPhysics(),
                    itemCount: originItems.Count,
                    itemBuilder: (cxt, index) => originItems[index: index]
                ));
        }
    }
}