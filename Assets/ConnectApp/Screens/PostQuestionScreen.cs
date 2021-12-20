using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Components;
using ConnectApp.Models.ActionModel;
using ConnectApp.Models.Model;
using ConnectApp.Models.State;
using ConnectApp.Models.ViewModel;
using ConnectApp.Plugins;
using ConnectApp.redux.actions;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Avatar = ConnectApp.Components.Avatar;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace ConnectApp.screens {
    public class PostQuestionScreenConnector : StatelessWidget {
        public PostQuestionScreenConnector(
            string questionId = null,
            Key key = null
        ) : base(key: key) {
            this.questionId = questionId;
        }

        readonly string questionId;

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, PostQuestionScreenViewModel>(
                converter: state => {
                    var userDict = state.userState.userDict;
                    var currentUserId = state.loginState.loginInfo.userId;
                    var user = new User();
                    if (currentUserId.isNotEmpty() && userDict.ContainsKey(key: currentUserId)) {
                        user = userDict.GetValueOrDefault(key: currentUserId, new User());
                    }

                    return new PostQuestionScreenViewModel {
                        createLoading = state.qaEditorState.createQuestionDraftLoading,
                        fetchQuestionLoading = state.qaEditorState.fetchQuestionDraftLoading,
                        questionDraft = state.qaEditorState.questionDraft,
                        tagDict = state.tagState.tagDict,
                        currentUser = user,
                        isExisting = this.questionId.isNotEmpty(),
                        uploadImageDict = state.qaEditorState.uploadImageDict
                    };
                },
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new PostQuestionScreenActionModel {
                        restQuestionDraft = () => dispatcher.dispatch(new RestQuestionDraftAction()),
                        startCreateQuestionDraft = () => dispatcher.dispatch(new StartCreateQuestionDraftAction()),
                        createQuestionDraft = () => dispatcher.dispatch<Future>(CActions.createQuestion()),
                        startFetchQuestionDraft = () => dispatcher.dispatch(new StartFetchQuestionDraftAction()),
                        fetchAllPlates = () => dispatcher.dispatch<Future>(CActions.fetchAllPlates()),
                        fetchQuestionDraft = () =>
                            dispatcher.dispatch<Future>(CActions.fetchQuestionDraft(questionId: this.questionId)),
                        saveQuestionDraft = (questionId, title, description, contentIds, plateId) =>
                            dispatcher.dispatch<Future>(CActions.updateQuestion(questionId: questionId, title: title,
                                description: description, contentIds: contentIds, plateId)),
                        postQuestionDraft = questionId =>
                            dispatcher.dispatch<Future>(CActions.publishQuestion(questionId: questionId)),
                        updateQuestionTags = (questionId, tagIds) =>
                            dispatcher.dispatch<Future>(CActions.updateQuestionTags(questionId: questionId,
                                tagIds: tagIds)),
                        uploadQuestionImage = (questionId, data, tmpId) => dispatcher.dispatch<Future>(
                            CActions.uploadQuestionImage(questionId: questionId, data: data, tmpId: tmpId)
                        )
                    };
                    return new PostQuestionScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }


    public class PostQuestionScreen : StatefulWidget {
        public PostQuestionScreen(
            Key key = null,
            PostQuestionScreenViewModel viewModel = null,
            PostQuestionScreenActionModel actionModel = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly PostQuestionScreenViewModel viewModel;
        public readonly PostQuestionScreenActionModel actionModel;

        public override State createState() {
            return new _PostQuestionScreenState();
        }
    }

    class _PostQuestionScreenState : State<PostQuestionScreen> {
        const int maxTagCount = 5;

        GlobalKey _titleInputFieldKey;
        GlobalKey _contentInputFieldKey;

        FocusNode _titleInputFieldFocusNode;
        FocusNode _contentInputFieldFocusNode;

        TextEditingController _titleInputFieldController;
        TextEditingController _contentInputFieldController;

        bool _isInit;
        string title;
        string content;
        bool _isShowKeyBoard;
        bool _contentInputFieldHasFocus;
        float? _contentHeight;
        int selectionIndex;
        bool animationIsReady;

        public override void initState() {
            base.initState();
            this._titleInputFieldKey = GlobalKey.key("_questionTitleInputFieldKey");
            this._contentInputFieldKey = GlobalKey.key("_questionContentInputFieldKey");
            this._titleInputFieldFocusNode = new FocusNode();
            this._contentInputFieldFocusNode = new FocusNode();
            this._titleInputFieldFocusNode.addListener(listener: this._focusNodeListener);
            this._contentInputFieldFocusNode.addListener(listener: this._focusNodeListener);
            this._titleInputFieldController = new TextEditingController("");
            this._contentInputFieldController = new TextEditingController("");
            this._isInit = true;
            this.title = "";
            this.content = "";
            this._isShowKeyBoard = false;
            this._contentHeight = null;
            this._contentInputFieldHasFocus = false;
            this.selectionIndex = 0;
            SchedulerBinding.instance.addPostFrameCallback(_ => {
                var modalRoute = ModalRoute.of(context: this.context);
                modalRoute.animation.addStatusListener(listener: this._animationStatusListener);
            });
        }
        
        void _animationStatusListener(AnimationStatus status) {
            if(status == AnimationStatus.completed) {
                this.animationIsReady = true;
                this.setState(() => {});
                this.widget.actionModel.fetchAllPlates().then(_ => {
                        var isExisting = this.widget.viewModel.isExisting;
                        if (isExisting) {
                            this.widget.actionModel.startFetchQuestionDraft();
                            this.widget.actionModel.fetchQuestionDraft();
                        }
                        else {
                            this.widget.actionModel.startCreateQuestionDraft();
                            this.widget.actionModel.createQuestionDraft();
                        }
                    }
                );
            }
        }

        void _focusNodeListener() {
            if (this._titleInputFieldFocusNode.hasFocus || this._contentInputFieldFocusNode.hasFocus) {
                this._isShowKeyBoard = true;
                this._contentInputFieldHasFocus = this._contentInputFieldFocusNode.hasFocus;
                this._contentHeight = this.calculateContentInputHeight();
                this.setState(() => { });
            }
            else if (!this._titleInputFieldFocusNode.hasFocus && !this._contentInputFieldFocusNode.hasFocus) {
                this._isShowKeyBoard = false;
                this._contentInputFieldHasFocus = false;
                this._contentHeight = this.calculateContentInputHeight();
                this.setState(() => { });
            }
        }

        public override void dispose() {
            this._titleInputFieldFocusNode.removeListener(listener: this._focusNodeListener);
            this._contentInputFieldFocusNode.removeListener(listener: this._focusNodeListener);
            base.dispose();
        }

        public override Widget build(BuildContext context) {
            Widget contentView;
            var draft = this.widget.viewModel.questionDraft;
            if (!this.animationIsReady || this.widget.viewModel.createLoading || this.widget.viewModel.fetchQuestionLoading || draft == null) {
                contentView = new GlobalLoading();
            }
            else {
                if (this._isInit) {
                    if (draft.title.isNotEmpty()) {
                        this._titleInputFieldController.text = draft.title;
                        this.title = draft.title;
                    }

                    if (draft.description.isNotEmpty()) {
                        this._contentInputFieldController.text = draft.description;
                        this.content = draft.description;
                    }

                    this._isInit = false;
                    this._contentHeight = this.calculateContentInputHeight();
                    this.setState(() => { });
                }

                contentView = this._buildPostQuestionView();
            }

            return new Container(
                color: CColors.White,
                child: new CustomSafeArea(
                    bottom: false,
                    child: new Column(
                        children: new List<Widget> {
                            this._buildNavigationBar(),
                            new Expanded(
                                child: contentView
                            ),
                            new SizedBox(height: CCommonUtils.getSafeAreaBottomPadding(context: context))
                        }
                    )
                )
            );
        }

        float calculateContentInputHeight() {
            var titleHeight = 27f;
            if (this.title.isNotEmpty()) {
                titleHeight = CTextUtils.CalculateTextHeight(
                    text: this.title,
                    textStyle: CTextStyle.PXLargeMediumBody,
                    CCommonUtils.getScreenWithoutPadding16Width(buildContext: this.context),
                    3
                );
            }

            var bottomPadding =
                this._isShowKeyBoard ? MediaQuery.of(context: this.context).viewInsets.bottom : 0;

            var textBarHeight = this._contentInputFieldHasFocus ? 49 : 0;

            var selectPlateHeight = 40;

            var contentHeight = CCommonUtils.getScreenHeight(buildContext: this.context)
                                - CCommonUtils.getSafeAreaTopPadding(context: this.context) - 44 - 16
                                - titleHeight - 16 - 54 - selectPlateHeight - textBarHeight - bottomPadding;
            return contentHeight;
        }

        Widget _buildPostQuestionView() {
            var tagIds = this.widget.viewModel.questionDraft.tagIds;
            var tagDict = this.widget.viewModel.tagDict;
            var tags = new List<Tag>();
            if (tagIds.isNotNullAndEmpty()) {
                tagIds.ForEach(id => {
                    if (tagDict.ContainsKey(key: id)) {
                        tags.Add(tagDict[key: id]);
                    }
                });
            }

            var bottomPadding =
                this._isShowKeyBoard ? MediaQuery.of(context: this.context).viewInsets.bottom : 0;

            if (this._isShowKeyBoard) {
                this._contentHeight = this.calculateContentInputHeight();
            }

            return new GestureDetector(
                onTap: this._dismissKeyboard,
                child: new Column(
                    children: new List<Widget> {
                        new Padding(
                            padding: EdgeInsets.all(16),
                            child: this._buildTitleInput()
                        ),
                        new Expanded(
                            child: new Padding(
                                padding: EdgeInsets.only(16, 0, 16),
                                child: this._buildContentInput()
                            )
                        ),
                        this._buildSelectPlate(),
                        this._buildTags(tags: tags),
                        this._buildTextBar(),
                        new SizedBox(height: bottomPadding)
                    }
                )
            );
        }

        Widget _buildSelectPlate() {
            var plate = this.widget.viewModel.questionDraft.plate;
            var name = plate?.name ?? "";
            return new Container(
                color: CColors.White,
                height: 40,
                padding: EdgeInsets.symmetric(horizontal: 16),
                child: new Row(
                    children: new List<Widget> {
                        new Container(
                            child: new Text("发布于", style: CTextStyle.PLargeBody)
                        ),
                        new SizedBox(width: 30),
                        new Expanded(
                            child: new GestureDetector(
                                onTap: () =>
                                    ActionSheetUtils.showModalActionSheet(
                                        context: this.context,
                                        new SelectPlateScreenConnector()
                                    ),
                                child: new Container(
                                    color: CColors.White,
                                    child: new Row(
                                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                                        children: new List<Widget> {
                                            name.isEmpty()
                                                ? new Container(
                                                    alignment: Alignment.centerLeft,
                                                    child: new Text("请选择发布的版块", style: CTextStyle.PLargeBody4)
                                                )
                                                : new Container(
                                                    alignment: Alignment.centerLeft,
                                                    child: new Text(data: name, style: CTextStyle.PLargeBody)
                                                ),
                                            new Icon(
                                                icon: CIcons.chevron_right,
                                                size: 24,
                                                color: CColors.LightBlueGrey
                                            )
                                        }
                                    )
                                )
                            )
                        )
                    }
                )
            );
        }

        Widget _buildTextBar() {
            if (!this._contentInputFieldHasFocus) {
                return new Container();
            }

            return new Container(
                height: 49,
                padding: EdgeInsets.only(6),
                decoration: new BoxDecoration(
                    border: new Border(new BorderSide(color: CColors.Separator, 0.5f))
                ),
                child: new Row(
                    mainAxisAlignment: MainAxisAlignment.start,
                    children: new List<Widget> {
                        this._pickImageButton()
                    }
                )
            );
        }

        Widget _pickImageButton() {
            return new CustomButton(
                padding: EdgeInsets.zero,
                onPressed: this._pickImage,
                child: new Container(
                    width: 44,
                    height: 49,
                    child: new Center(
                        child: new Icon(
                            icon: CIcons.outline_photo_size_select_actual,
                            size: 28,
                            color: CColors.Icon
                        )
                    )
                )
            );
        }

        void _pickImage() {
            if (this._isShowKeyBoard) {
                this.selectionIndex = this._contentInputFieldController.selection.startPos.offset;
                this.setState(fn: this._dismissKeyboard);
            }

            var items = new List<ActionSheetItem> {
                new ActionSheetItem(
                    "拍照",
                    onTap: () => PickImagePlugin.PickImage(
                        source: ImageSource.Camera,
                        imageCallBack: this._pickImageCallback,
                        false
                    )
                ),
                new ActionSheetItem(
                    "从相册选择照片",
                    onTap: () => PickImagePlugin.PickImage(
                        source: ImageSource.Gallery,
                        imageCallBack: this._pickImageCallback,
                        false
                    )
                ),
                new ActionSheetItem("取消", type: ActionType.cancel)
            };

            ActionSheetUtils.showModalActionSheet(
                context: this.context,
                new ActionSheet(
                    title: "添加图片",
                    items: items
                )
            );
        }

        void _pickImageCallback(byte[] pickImage) {
            FocusScope.of(context: this.context).requestFocus(node: this._contentInputFieldFocusNode);
            this._contentInputFieldController.selection = TextSelection.collapsed(offset: this.selectionIndex);
            TextInputPlugin.TextInputHide();

            var draft = this.widget.viewModel.questionDraft;
            var nonce = Snowflake.CreateNonce();
            var content = this._contentInputFieldController.text;
            var insertText = $"\n![description](/markdown/pending/images/{nonce})\n";
            var textSelection = this._contentInputFieldController.selection;
            var tempContent = content.Insert(startIndex: textSelection.start, value: insertText);
            this._contentInputFieldController.text = tempContent;
            this.content = tempContent;
            this.setState(() => { });
            this._contentInputFieldController.selection =
                TextSelection.collapsed(textSelection.start + insertText.Length);
            this.selectionIndex = textSelection.start;
            CustomDialogUtils.showCustomDialog(context: this.context, child: new CustomLoadingDialog());
            this.widget.actionModel.uploadQuestionImage(arg1: draft.questionId, arg2: pickImage, arg3: nonce)
                .then(_ => {
                    var contentId = this.widget.viewModel.uploadImageDict.GetValueOrDefault(key: nonce, "");
                    var newInsertText = $"\n![description](/markdown/images/{contentId})\n";
                    var finalContent = tempContent.Replace(oldValue: insertText, newValue: newInsertText);
                    this._contentInputFieldController.text = finalContent;
                    this.content = finalContent;
                    this.setState(() => { });
                    this._contentInputFieldController.selection =
                        TextSelection.collapsed(textSelection.start + newInsertText.Length);
                    CustomDialogUtils.hiddenCustomDialog(context: this.context);
                    TextInputPlugin.TextInputShow();
                })
                .catchError(err => {
                    var finalContent = tempContent.Replace(oldValue: insertText, "");
                    this._contentInputFieldController.text = finalContent;
                    this.content = finalContent;
                    this.setState(() => { });
                    this._contentInputFieldController.selection = TextSelection.collapsed(offset: textSelection.start);
                    CustomDialogUtils.hiddenCustomDialog(context: this.context);
                    CustomDialogUtils.showToast(context: this.context, "图片上传失败，请稍后重试！",
                        iconData: CIcons.sentiment_dissatisfied);
                    TextInputPlugin.TextInputShow();
                });
        }

        void deleteTag(string tagId) {
            var draft = this.widget.viewModel.questionDraft;
            var tagIds = draft.tagIds;
            if (tagIds.Contains(item: tagId)) {
                tagIds.Remove(item: tagId);
                this.widget.actionModel.updateQuestionTags(arg1: draft.questionId, arg2: tagIds);
            }
        }


        Widget _buildTags(List<Tag> tags) {
            return new GestureDetector(
                onTap: () => {
                    this._dismissKeyboard();
                    this._contentHeight = this.calculateContentInputHeight();
                    ActionSheetUtils.showModalActionSheet(
                        context: this.context,
                        new AddTagScreenConnector()
                    );
                },
                child: new Container(
                    color: CColors.White,
                    height: 54,
                    padding: EdgeInsets.symmetric(13),
                    child: new Container(
                        constraints: new BoxConstraints(
                            maxHeight: 28,
                            maxWidth: CCommonUtils.getScreenWidth(buildContext: this.context),
                            minWidth: CCommonUtils.getScreenWidth(buildContext: this.context)
                        ),
                        child: new ListView(
                            scrollDirection: Axis.horizontal,
                            padding: EdgeInsets.symmetric(horizontal: 16),
                            children: this._buildTagItems(tags: tags)
                        )
                    )
                )
            );
        }

        List<Widget> _buildTagItems(List<Tag> tags) {
            var widgets = new List<Widget> {
                new GestureDetector(
                    onTap: () => {
                        this._dismissKeyboard();
                        this._contentHeight = this.calculateContentInputHeight();
                        ActionSheetUtils.showModalActionSheet(
                            context: this.context,
                            new AddTagScreenConnector()
                        );
                    },
                    child: new Row(
                        children: new List<Widget> {
                            new Container(
                                height: 22,
                                width: 22,
                                decoration: new BoxDecoration(
                                    color: CColors.TagBackground,
                                    borderRadius: BorderRadius.all(2)
                                ),
                                child: new Icon(icon: CIcons.outline_tag, size: 16, color: CColors.PrimaryBlue)
                            ),
                            new SizedBox(width: 4),
                            new Text(
                                $"添加标签 ({tags.Count}/{maxTagCount})",
                                style: new TextStyle(
                                    fontSize: 14,
                                    fontFamily: "Roboto-Regular",
                                    color: CColors.PrimaryBlue
                                )
                            ),
                            new SizedBox(width: 8)
                        }
                    )
                )
            };

            for (var i = 0; i < tags.Count; i++) {
                var tag = tags[index: i];
                Widget tagWidget = new GestureDetector(
                    onTap: () => this.deleteTag(tagId: tag.id),
                    child: new Container(
                        margin: EdgeInsets.only(right: i == tags.Count - 1 ? 0 : 8),
                        decoration: new BoxDecoration(
                            color: CColors.TagBackground,
                            borderRadius: BorderRadius.all(2)
                        ),
                        height: 24,
                        padding: EdgeInsets.only(8, 0, 4),
                        child: new Row(
                            mainAxisSize: MainAxisSize.min,
                            children: new List<Widget> {
                                new Text(
                                    data: tag.name,
                                    maxLines: 1,
                                    style: CTextStyle.PRegularBlue.defaultHeight(),
                                    overflow: TextOverflow.ellipsis,
                                    textAlign: TextAlign.center
                                ),
                                new SizedBox(width: 3),
                                new Icon(icon: CIcons.close, size: 14, color: CColors.PrimaryBlue)
                            }
                        )
                    )
                );
                widgets.Add(item: tagWidget);
            }

            return widgets;
        }

        Widget _buildNavigationBar() {
            var actionList = new List<ActionSheetItem>();
            if (this.widget.viewModel.isExisting) {
                actionList = new List<ActionSheetItem> {
                    new ActionSheetItem("不保存并退出", type: ActionType.destructive,
                        () => {
                            Navigator.pop(context: this.context);
                            this.widget.actionModel.restQuestionDraft();
                        }),
                    new ActionSheetItem("取消", type: ActionType.cancel)
                };
            }
            else {
                actionList = new List<ActionSheetItem> {
                    new ActionSheetItem("保存草稿并退出", type: ActionType.normal,
                        onTap: this.saveQuestionDraft),
                    new ActionSheetItem("不保存", type: ActionType.destructive,
                        () => {
                            Navigator.pop(context: this.context);
                            this.widget.actionModel.restQuestionDraft();
                        }),
                    new ActionSheetItem("取消", type: ActionType.cancel)
                };
            }

            return new Container(
                decoration: new BoxDecoration(
                    color: CColors.White
                ),
                height: 44,
                child: new Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    crossAxisAlignment: CrossAxisAlignment.center,
                    children: new List<Widget> {
                        new CustomButton(
                            padding: EdgeInsets.symmetric(10, 16),
                            onPressed: () => {
                                ActionSheetUtils.showModalActionSheet(
                                    context: this.context,
                                    new ActionSheet(
                                        items: actionList
                                    )
                                );
                                this._contentHeight = this.calculateContentInputHeight();
                            },
                            child: new Text(
                                "取消",
                                style: CTextStyle.PLargeBody5.defaultHeight()
                            )
                        ),
                        new Row(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: new List<Widget> {
                                Avatar.User(user: this.widget.viewModel.currentUser, 20),
                                new SizedBox(width: 4),
                                new Text(
                                    "提问",
                                    style: CTextStyle.PXLargeMedium
                                )
                            }
                        ),
                        new CustomButton(
                            padding: EdgeInsets.symmetric(10, 16),
                            onPressed: () => {
                                this.postQuestion();
                                this._contentHeight = this.calculateContentInputHeight();
                            },
                            child: new Text(
                                "发布",
                                style: CTextStyle.PLargeBlue.defaultHeight()
                            )
                        )
                    }
                )
            );
        }

        Widget _buildTitleInput() {
            return new InputField(
                key: this._titleInputFieldKey,
                height: null,
                controller: this._titleInputFieldController,
                focusNode: this._titleInputFieldFocusNode,
                maxLength: null,
                minLines: 1,
                maxLines: 3,
                autofocus: true,
                alignment: Alignment.topLeft,
                style: CTextStyle.PXLargeMediumBody,
                hintText: "写下你的标题内容",
                hintTextWidth: MediaQuery.of(context: this.context).size.width - 16 * 2,
                hintStyle: CTextStyle.PXLargeMediumBody4,
                cursorColor: CColors.PrimaryBlue,
                clearButtonMode: InputFieldClearButtonMode.never,
                onChanged: title => {
                    if (title == null) {
                        return;
                    }

                    this.title = title;
                    this._contentHeight = this.calculateContentInputHeight();
                    this.setState(() => { });
                }
            );
        }

        Widget _buildContentInput() {
            return new InputField(
                key: this._contentInputFieldKey,
                height: this._contentHeight,
                controller: this._contentInputFieldController,
                focusNode: this._contentInputFieldFocusNode,
                maxLength: 3000,
                maxLines: null,
                alignment: Alignment.topLeft,
                style: CTextStyle.PLargeBody,
                hintText: "描述问题背景和详情…",
                hintTextWidth: MediaQuery.of(context: this.context).size.width - 16 * 2,
                hintStyle: CTextStyle.PLargeBody4,
                cursorColor: CColors.PrimaryBlue,
                clearButtonMode: InputFieldClearButtonMode.never,
                keyboardType: TextInputType.multiline,
                onChanged: content => {
                    if (content == null) {
                        return;
                    }

                    this.content = content;
                    this.setState(() => { });
                }
            );
        }

        void postQuestion() {
            var draft = this.widget.viewModel.questionDraft;
            var title = this.title.Trim();
            var content = this.content.Trim();
            if (this.title.Trim().isEmpty()) {
                CustomDialogUtils.showToast(context: this.context, "请填写问题标题后发布",
                    iconData: CIcons.sentiment_dissatisfied);
                return;
            }

            if (draft.plate == null) {
                CustomDialogUtils.showToast(context: this.context, "请选择板块后发布", iconData: CIcons.sentiment_dissatisfied);
                return;
            }
            
            if (draft.tagIds.isNullOrEmpty()) {
                CustomDialogUtils.showToast(context: this.context, "请添加标签后发布", iconData: CIcons.sentiment_dissatisfied);
                return;
            }

            CustomDialogUtils.showCustomDialog(context: this.context, child: new CustomLoadingDialog(message: "正在保存中"));
            this.widget.actionModel
                .saveQuestionDraft(arg1: draft.questionId, arg2: title, arg3: content, content.parseImageContentId(), arg5: draft.plate?.id)
                .then(_ => {
                    CustomDialogUtils.hiddenCustomDialog(context: this.context);
                    CustomDialogUtils.showCustomDialog(context: this.context,
                        child: new CustomLoadingDialog(message: "正在发布中"));
                    this.widget.actionModel.postQuestionDraft(arg: draft.questionId)
                        .then(v => {
                            CustomDialogUtils.hiddenCustomDialog(context: this.context);
                            Navigator.pop(context: this.context);
                            CustomDialogUtils.showToast(context: this.context, "审核中，可在“我的-创作中心”查看进度",
                                iconData: CIcons.sentiment_satisfied,
                                2);
                            this.widget.actionModel.restQuestionDraft();
                        })
                        .catchError(err => {
                            CustomDialogUtils.hiddenCustomDialog(context: this.context);
                            CustomDialogUtils.showToast(context: this.context, "发布失败",
                                iconData: CIcons.sentiment_dissatisfied);
                        });
                })
                .catchError(err => {
                    CustomDialogUtils.hiddenCustomDialog(context: this.context);
                    CustomDialogUtils.showToast(context: this.context, "草稿保存失败",
                        iconData: CIcons.sentiment_dissatisfied);
                });
        }

        void saveQuestionDraft() {
            var draft = this.widget.viewModel.questionDraft;
            var title = this.title.Trim();
            var content = this.content.Trim();
            CustomDialogUtils.showCustomDialog(context: this.context, child: new CustomLoadingDialog(message: "正在保存中"));
            this.widget.actionModel
                .saveQuestionDraft(arg1: draft.questionId, arg2: title, arg3: content, content.parseImageContentId(), arg5: draft.plate?.id)
                .then(_ => {
                    CustomDialogUtils.hiddenCustomDialog(context: this.context);
                    Navigator.pop(context: this.context);
                    CustomDialogUtils.showToast(context: this.context, "保存成功", iconData: CIcons.sentiment_satisfied);
                    this.widget.actionModel.restQuestionDraft();
                })
                .catchError(err => {
                    CustomDialogUtils.hiddenCustomDialog(context: this.context);
                    CustomDialogUtils.showToast(context: this.context, "草稿保存失败",
                        iconData: CIcons.sentiment_dissatisfied);
                });
        }

        void _dismissKeyboard() {
            this._titleInputFieldFocusNode.unfocus();
            this._contentInputFieldFocusNode.unfocus();
            this._contentInputFieldHasFocus = false;
            this._isShowKeyBoard = false;
            TextInputPlugin.TextInputHide();
        }
    }
}