using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Components;
using ConnectApp.Components.Toast;
using ConnectApp.Models.ActionModel;
using ConnectApp.Models.Model;
using ConnectApp.Models.State;
using ConnectApp.Models.ViewModel;
using ConnectApp.Plugins;
using ConnectApp.redux.actions;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using ToastGravity = ConnectApp.Components.Toast.ToastGravity;

namespace ConnectApp.screens {
    public class AddTagScreenConnector : StatelessWidget {
        public AddTagScreenConnector(
            Key key = null
        ) : base(key: key) {
        }

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, AddTagScreenViewModel>(
                converter: state => {
                    var tags = new List<Tag>();
                    var searchTags = new List<Tag>();
                    var tagIds = state.qaEditorState.questionDraft.tagIds;
                    var searchTagIds = state.searchState.searchTagList;
                    var tagDict = state.tagState.tagDict;
                    tagIds.ForEach(tagId => {
                        if (tagDict.ContainsKey(key: tagId)) {
                            tags.Add(tagDict[key: tagId]);
                        }
                    });
                    searchTagIds.ForEach(tagId => {
                        if (tagDict.ContainsKey(key: tagId)) {
                            searchTags.Add(tagDict[key: tagId]);
                        }
                    });

                    return new AddTagScreenViewModel {
                        questionDraft = state.qaEditorState.questionDraft,
                        tags = tags,
                        searchTags = searchTags
                    };
                },
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new AddTagScreenActionModel {
                        updateQuestionTags = (questionId, tagIds) =>
                            dispatcher.dispatch<Future>(CActions.updateQuestionTags(questionId: questionId,
                                tagIds: tagIds)),
                        searchTag = keyword => dispatcher.dispatch<Future>(CActions.searchTags(keyword: keyword))
                    };
                    return new AddTagScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }


    public class AddTagScreen : StatefulWidget {
        public AddTagScreen(
            Key key = null,
            AddTagScreenViewModel viewModel = null,
            AddTagScreenActionModel actionModel = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly AddTagScreenViewModel viewModel;
        public readonly AddTagScreenActionModel actionModel;

        public override State createState() {
            return new _AddTagScreenState();
        }
    }

    class _AddTagScreenState : State<AddTagScreen> {
        const int maxTagCount = 5;
        string keyword;
        GlobalKey _searchInputFieldKey;
        FocusNode _searchInputFieldFocusNode;
        TextEditingController _searchInputFieldController;

        public override void initState() {
            base.initState();
            this._searchInputFieldKey = GlobalKey.key("_searchInputFieldKey");
            this._searchInputFieldFocusNode = new FocusNode();
            this._searchInputFieldController = new TextEditingController();
            this.keyword = "";
        }

        public override Widget build(BuildContext context) {
            return new Container(
                padding: EdgeInsets.only(top: CCommonUtils.getSafeAreaTopPadding(context: context) + 44),
                child: new Column(
                    children: new List<Widget> {
                        this._buildHeader(),
                        new Expanded(
                            child: new Container(
                                color: CColors.White,
                                child: new Column(
                                    children: new List<Widget> {
                                        this._buildTextField(),
                                        this._buildTags(),
                                        new Expanded(
                                            child: this._buildContent(context: context)
                                        )
                                    }
                                )
                            )
                        )
                    }
                )
            );
        }

        Widget _buildContent(BuildContext context) {
            var newKeyword = this.keyword.Trim();
            if (newKeyword.isEmpty()) {
                return new Container(color: CColors.White);
            }

            var bottomPadding = this._searchInputFieldFocusNode.hasFocus
                ? MediaQuery.of(context: context).padding.bottom
                : 0;
            return new GestureDetector(
                onTap: this._dismissKeyboard,
                child: new Container(
                    color: CColors.White,
                    padding: EdgeInsets.only(bottom: bottomPadding),
                    child: new NotificationListener<ScrollNotification>(
                        onNotification: this._onNotification,
                        child: ListView.builder(
                            padding: EdgeInsets.zero,
                            itemCount: this.widget.viewModel.searchTags.Count + 1,
                            itemBuilder: this._buildTagCard
                        )
                    )
                )
            );
        }

        bool _onNotification(ScrollNotification notification) {
            if (this._searchInputFieldFocusNode.hasFocus) {
                this._dismissKeyboard();
            }

            return true;
        }

        Widget _buildTagCard(BuildContext buildContext, int index) {
            var newKeyword = this.keyword.Trim();
            if (newKeyword.isNotEmpty() && index == 0) {
                return new GestureDetector(
                    onTap: () => this.addTag(tagId: newKeyword),
                    child: new Container(
                        height: 44,
                        padding: EdgeInsets.symmetric(horizontal: 16),
                        color: CColors.White,
                        child: new Row(
                            children: new List<Widget> {
                                new Icon(icon: CIcons.round_add_circle, size: 18, color: CColors.LightBlueGrey),
                                new SizedBox(width: 8),
                                new Expanded(
                                    child: new Row(
                                        children: new List<Widget> {
                                            new Text(
                                                "点击添加 ",
                                                style: CTextStyle.PLargeBody5,
                                                overflow: TextOverflow.fade,
                                                maxLines: 1
                                            ),
                                            new Text(
                                                data: newKeyword,
                                                style: CTextStyle.PLargeBody,
                                                overflow: TextOverflow.fade,
                                                maxLines: 1
                                            )
                                        }
                                    )
                                )
                            }
                        )
                    )
                );
            }

            var tag = this.widget.viewModel.searchTags[index - 1];
            return new GestureDetector(
                onTap: () => this.addTag(tagId: tag.id),
                child: new Container(
                    height: 44,
                    padding: EdgeInsets.symmetric(horizontal: 16),
                    color: CColors.White,
                    child: new Row(
                        children: new List<Widget> {
                            new Icon(icon: CIcons.outline_tag, size: 16, color: CColors.TextBody2),
                            new SizedBox(width: 8),
                            new Expanded(
                                child: new Text(
                                    data: tag.name,
                                    style: CTextStyle.PLargeBody,
                                    overflow: TextOverflow.fade,
                                    maxLines: 1
                                )
                            )
                        }
                    )
                )
            );
        }

        Widget _buildHeader() {
            return new Container(
                decoration: new BoxDecoration(
                    color: CColors.White,
                    borderRadius: BorderRadius.only(12, 12)
                ),
                child: new Column(
                    children: new List<Widget> {
                        new Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: new List<Widget> {
                                new SizedBox(width: 72),
                                new Text(
                                    "添加标签",
                                    style: CTextStyle.PXLargeMedium
                                ),
                                new CustomButton(
                                    padding: EdgeInsets.all(20),
                                    onPressed: () => Navigator.pop(context: this.context),
                                    child: new Text(
                                        "完成",
                                        style: CTextStyle.PLargeBlue.defaultHeight()
                                    )
                                )
                            }
                        ),
                        new CustomDivider(color: CColors.Separator2, height: 1)
                    }
                )
            );
        }

        Widget _buildTags() {
            var tags = this.widget.viewModel.tags;
            var tips = "至少添加 1 个标签";
            if (tags.Count > 0 && tags.Count < 5) {
                tips = $"还可以添加 {maxTagCount - tags.Count} 个标签";
            }
            else if (tags.Count == 5) {
                tips = "添加标签数已达到上限";
            }

            return new Container(
                color: CColors.White,
                width: CCommonUtils.getScreenWidth(buildContext: this.context),
                padding: EdgeInsets.symmetric(0, 16),
                child: new Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: new List<Widget> {
                        new Text(data: tips, style: CTextStyle.PRegularBody5),
                        this.widget.viewModel.tags.isNotNullAndEmpty()
                            ? (Widget) new Padding(
                                padding: EdgeInsets.only(top: 12, bottom: 12),
                                child: new Wrap(
                                    spacing: 8,
                                    runSpacing: 8,
                                    children: this._buildTagItems()
                                )
                            )
                            : new Container()
                    }
                )
            );
        }

        List<Widget> _buildTagItems() {
            var widgets = new List<Widget>();
            this.widget.viewModel.tags.ForEach(item => {
                Widget tag = new GestureDetector(
                    onTap: () => this.deleteTag(tagId: item.id),
                    child: new Container(
                        decoration: new BoxDecoration(
                            color: CColors.TagBackground,
                            borderRadius: BorderRadius.all(2)
                        ),
                        height: 24,
                        padding: EdgeInsets.only(8, 0, 4),
                        child: new Row(
                            mainAxisSize: MainAxisSize.min,
                            children: new List<Widget> {
                                new ConstrainedBox(
                                    constraints: new BoxConstraints(
                                        maxWidth: CCommonUtils.getTagTextMaxWidth(buildContext: this.context)
                                    ),
                                    child: new Text(
                                        data: item.name,
                                        maxLines: 1,
                                        style: CTextStyle.PRegularBlue.defaultHeight(),
                                        overflow: TextOverflow.ellipsis,
                                        textAlign: TextAlign.center
                                    )
                                ),
                                new SizedBox(width: 3),
                                new Icon(icon: CIcons.close, size: 14, color: CColors.PrimaryBlue)
                            }
                        )
                    )
                );
                widgets.Add(item: tag);
            });
            return widgets;
        }

        void addTag(string tagId) {
            var draft = this.widget.viewModel.questionDraft;
            var tagIds = draft.tagIds;
            if (!tagIds.Contains(item: tagId)) {
                tagIds.Add(item: tagId);
                if (tagIds.Count > 5) {
                    tagIds = tagIds.GetRange(0, count: maxTagCount);
                    CustomToast.showToast(context: this.context, $"最多添加 {maxTagCount} 个标签", gravity: ToastGravity.CENTER);
                }

                this.widget.actionModel.updateQuestionTags(arg1: draft.questionId, arg2: tagIds).catchError(err => {
                    CustomToast.showToast(context: this.context, "添加失败", type: ToastType.Error, gravity: ToastGravity.CENTER);
                });
            }
        }

        void deleteTag(string tagId) {
            var draft = this.widget.viewModel.questionDraft;
            var tagIds = draft.tagIds;
            if (tagIds.Contains(item: tagId)) {
                tagIds.Remove(item: tagId);
                this.widget.actionModel.updateQuestionTags(arg1: draft.questionId, arg2: tagIds);
            }
        }

        Widget _buildTextField() {
            return new Container(
                padding: EdgeInsets.only(16, 16, 16, 12),
                color: CColors.White,
                child: new Container(
                    padding: EdgeInsets.only(16, 5, 0, 5),
                    decoration: new BoxDecoration(
                        color: CColors.Separator2,
                        borderRadius: BorderRadius.all(16)
                    ),
                    child: new InputField(
                        key: this._searchInputFieldKey,
                        controller: this._searchInputFieldController,
                        focusNode: this._searchInputFieldFocusNode,
                        style: CTextStyle.PRegularBody.defaultHeight(),
                        hintText: "搜索标签",
                        hintStyle: CTextStyle.PRegularBody5.defaultHeight(),
                        autofocus: true,
                        height: 22,
                        maxLines: 1,
                        minLines: 1,
                        maxLength: 100,
                        cursorColor: CColors.PrimaryBlue,
                        clearButtonMode: InputFieldClearButtonMode.hasText,
                        textInputAction: TextInputAction.search,
                        onSubmitted: _ => this._dismissKeyboard(),
                        onChanged: text => {
                            this.widget.actionModel.searchTag(arg: text);
                            this.keyword = text;
                            this.setState(() => { });
                        }
                    )
                )
            );
        }

        void _dismissKeyboard() {
            this._searchInputFieldFocusNode.unfocus();
            TextInputPlugin.TextInputHide();
        }
    }
}