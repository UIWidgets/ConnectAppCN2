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
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.service;
using Unity.UIWidgets.widgets;

namespace ConnectApp.screens {
    public class PostAnswerScreenConnector : StatelessWidget {
        public PostAnswerScreenConnector(
            string questionId,
            string answerId = null,
            bool canSave = false,
            Key key = null
        ) : base(key: key) {
            this.questionId = questionId;
            this.answerId = answerId;
            this.canSave = canSave;
        }

        readonly string questionId;
        readonly string answerId;
        readonly bool canSave;

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, PostAnswerScreenViewModel>(
                converter: state => {
                    var answerDraft = state.qaEditorState.answerDraft;
                    var userDict = state.userState.userDict;
                    var currentUserId = state.loginState.loginInfo.userId;
                    var user = new User();
                    if (currentUserId.isNotEmpty() && userDict.ContainsKey(key: currentUserId)) {
                        user = userDict.GetValueOrDefault(key: currentUserId, new User());
                    }

                    return new PostAnswerScreenViewModel {
                        createLoading = state.qaEditorState.createAnswerDraftLoading,
                        fetchLoading = state.qaEditorState.fetchAnswerDraftLoading,
                        answerDraft = answerDraft,
                        currentUser = user,
                        isExisting = this.answerId.isNotEmpty(),
                        canSave = this.canSave,
                        uploadImageDict = state.qaEditorState.uploadImageDict
                    };
                },
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new PostAnswerScreenActionModel {
                        startFetchAnswerDraft = () => dispatcher.dispatch(new StartFetchAnswerDraftAction()),
                        fetchAnswerDraft = () =>
                            dispatcher.dispatch<Future>(CActions.fetchAnswerDraft(questionId: this.questionId,
                                answerId: this.answerId)),
                        removeAnswerDraft = () =>
                            dispatcher.dispatch(new RemoveAnswerDraftAction {questionId = this.questionId}),
                        resetAnswerDraft = () =>
                            dispatcher.dispatch(new ResetAnswerDraftAction()),
                        startCreateAnswerDraft = () => dispatcher.dispatch(new StartCreateAnswerAction()),
                        createAnswerDraft = () =>
                            dispatcher.dispatch<Future>(CActions.createAnswer(questionId: this.questionId)),
                        saveAnswerDraft = (questionId, answerId, description, contentIds) =>
                            dispatcher.dispatch<Future>(CActions.updateAnswer(questionId: questionId,
                                answerId: answerId,
                                description: description, contentIds: contentIds)),
                        postAnswerDraft = (questionId, answerId) =>
                            dispatcher.dispatch<Future>(CActions.publishAnswer(questionId: questionId,
                                answerId: answerId)),
                        uploadAnswerImage = (questionId, answerId, data, tmpId) => dispatcher.dispatch<Future>(
                            CActions.uploadAnswerImage(questionId: questionId, answerId: answerId, data: data,
                                tmpId: tmpId)
                        )
                    };
                    return new PostAnswerScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }


    public class PostAnswerScreen : StatefulWidget {
        public PostAnswerScreen(
            Key key = null,
            PostAnswerScreenViewModel viewModel = null,
            PostAnswerScreenActionModel actionModel = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly PostAnswerScreenViewModel viewModel;
        public readonly PostAnswerScreenActionModel actionModel;

        public override State createState() {
            return new _PostAnswerScreenState();
        }
    }

    class _PostAnswerScreenState : State<PostAnswerScreen> {
        GlobalKey _contentInputFieldKey;
        FocusNode _contentInputFieldFocusNode;
        TextEditingController _contentInputFieldController;

        bool _isInit;
        string content;
        bool _isShowKeyBoard;
        float? _contentHeight;
        bool _contentInputFieldHasFocus;
        int selectionIndex;

        public override void initState() {
            base.initState();
            this._contentInputFieldKey = GlobalKey.key("_answerContentInputFieldKey");
            this._contentInputFieldFocusNode = new FocusNode();
            this._contentInputFieldFocusNode.addListener(listener: this._focusNodeListener);
            this._contentInputFieldController = new TextEditingController("");
            this._isInit = true;
            this.content = "";
            this._isShowKeyBoard = false;
            this._contentHeight = null;
            this.selectionIndex = 0;
            this._contentInputFieldHasFocus = false;
            var isExisting = this.widget.viewModel.isExisting;
            SchedulerBinding.instance.addPostFrameCallback(_ => {
                if (isExisting) {
                    this.widget.actionModel.startFetchAnswerDraft();
                    this.widget.actionModel.fetchAnswerDraft();
                }
                else {
                    this.widget.actionModel.startCreateAnswerDraft();
                    this.widget.actionModel.createAnswerDraft();
                }
            });
        }

        void _focusNodeListener() {
            if (this._contentInputFieldFocusNode.hasFocus) {
                this._isShowKeyBoard = true;
                this._contentInputFieldHasFocus = true;
                this._contentHeight = this.calculateContentInputHeight();
                this.setState(() => { });
            }
            else {
                this._isShowKeyBoard = false;
                this._contentInputFieldHasFocus = false;
                this._contentHeight = this.calculateContentInputHeight();
                this.setState(() => { });
            }
        }

        float calculateContentInputHeight() {
            var text = this.widget.viewModel.answerDraft?.questionTitle ?? " ";
            var textStyle = new TextStyle(
                height: 1.33f,
                fontSize: 16,
                fontFamily: "Roboto-Medium",
                color: CColors.TextBody
            );
            var titleHeight = 24f;
            if (text.isNotEmpty()) {
                titleHeight = CTextUtils.CalculateTextHeight(
                    text: text,
                    textStyle: textStyle,
                    CCommonUtils.getScreenWithoutPadding16Width(buildContext: this.context),
                    3
                );
            }

            var bottomPadding =
                this._isShowKeyBoard ? MediaQuery.of(context: this.context).viewInsets.bottom : 0;

            var textBarHeight = this._contentInputFieldHasFocus ? 49 : 0;
            var contentHeight = CCommonUtils.getScreenHeight(buildContext: this.context)
                                - CCommonUtils.getSafeAreaTopPadding(context: this.context) - 44 - 16
                                - titleHeight - 16 - 16 - 16 - textBarHeight - bottomPadding;
            return contentHeight;
        }

        public override void dispose() {
            this._contentInputFieldFocusNode.removeListener(listener: this._focusNodeListener);
            base.dispose();
        }

        public override Widget build(BuildContext context) {
            Widget contentView;
            var createLoading = this.widget.viewModel.createLoading;
            var fetchLoading = this.widget.viewModel.fetchLoading;
            var isExisting = this.widget.viewModel.isExisting;
            var loading = isExisting ? fetchLoading : createLoading;
            var draft = this.widget.viewModel.answerDraft;
            if (loading || draft == null) {
                contentView = new GlobalLoading();
            }
            else {
                if (this._isInit) {
                    if (draft.description.isNotEmpty()) {
                        this._contentInputFieldController.text = draft.description;
                        this.content = draft.description;
                    }

                    this._isInit = false;
                    this._contentHeight = this.calculateContentInputHeight();
                    this.setState(() => { });
                }

                contentView = this._buildPostAnswerView();
            }

            return new Container(
                color: CColors.White,
                child: new CustomSafeArea(
                    child: new Column(
                        children: new List<Widget> {
                            this._buildNavigationBar(),
                            new Expanded(
                                child: contentView
                            )
                        }
                    )
                )
            );
        }

        Widget _buildPostAnswerView() {
            var bottomPadding = this._isShowKeyBoard ? MediaQuery.of(context: this.context).viewInsets.bottom : 0;
            if (this._isShowKeyBoard) {
                this._contentHeight = this.calculateContentInputHeight();
            }

            return new Container(
                padding: EdgeInsets.only(bottom: bottomPadding),
                child: new GestureDetector(
                    onTap: this._dismissKeyboard,
                    child: new Column(
                        children: new List<Widget> {
                            this._buildTitleWidget(),
                            new Expanded(
                                child: new Padding(
                                    padding: EdgeInsets.all(16),
                                    child: this._buildContentInput()
                                )
                            ),
                            this._buildTextBar()
                        }
                    )
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

            var draft = this.widget.viewModel.answerDraft;
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
            this.widget.actionModel
                .uploadAnswerImage(arg1: draft.questionId, arg2: draft.answerId, arg3: pickImage, arg4: nonce)
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


        Widget _buildNavigationBar() {
            var actionList = new List<ActionSheetItem>();
            if (this.widget.viewModel.canSave) {
                actionList = new List<ActionSheetItem> {
                    new ActionSheetItem("保存草稿并退出", type: ActionType.normal,
                        onTap: this.saveAnswerDraft),
                    new ActionSheetItem("不保存", type: ActionType.destructive,
                        () => {
                            Navigator.pop(context: this.context);
                            this.widget.actionModel.resetAnswerDraft();
                        }),
                    new ActionSheetItem("取消", type: ActionType.cancel)
                };
            }
            else {
                actionList = new List<ActionSheetItem> {
                    new ActionSheetItem("不保存并退出", type: ActionType.destructive,
                        () => {
                            Navigator.pop(context: this.context);
                            this.widget.actionModel.resetAnswerDraft();
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
                                    "正在回答",
                                    style: CTextStyle.PXLargeMedium
                                )
                            }
                        ),
                        new CustomButton(
                            padding: EdgeInsets.symmetric(10, 16),
                            onPressed: () => {
                                this.postAnswer();
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

        Widget _buildTitleWidget() {
            var text = this.widget.viewModel.answerDraft.questionTitle;
            var textStyle = new TextStyle(
                height: 1.33f,
                fontSize: 16,
                fontFamily: "Roboto-Medium",
                color: CColors.TextBody
            );
            var textWidth = CCommonUtils.getScreenWithoutPadding16Width(buildContext: this.context);
            const int maxLines = 3;
            var textHeight = CTextUtils.CalculateTextHeight(text: text, textStyle: textStyle, textWidth: textWidth,
                maxLines: maxLines);

            return new Container(
                padding: EdgeInsets.all(16),
                color: CColors.BgGrey,
                height: textHeight + 32,
                child: new CustomScrollbar(
                    child: new ListView(
                        children: new List<Widget> {
                            new Text(
                                data: text,
                                style: new TextStyle(
                                    height: 1.33f,
                                    fontSize: 16,
                                    fontFamily: "Roboto-Medium",
                                    color: CColors.TextBody
                                )
                            )
                        }
                    )
                )
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
                autofocus: true,
                alignment: Alignment.topLeft,
                style: CTextStyle.PLargeBody,
                hintText: "输入你的回答",
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

        void postAnswer() {
            var draft = this.widget.viewModel.answerDraft;
            var content = this.content.Trim();
            if (this.content.Trim().isEmpty()) {
                CustomDialogUtils.showToast(context: this.context, "请填写回答后发布", iconData: CIcons.sentiment_dissatisfied);
                return;
            }

            CustomDialogUtils.showCustomDialog(context: this.context, child: new CustomLoadingDialog(message: "正在保存中"));
            this.widget.actionModel
                .saveAnswerDraft(arg1: draft.questionId, arg2: draft.answerId, arg3: content,
                    content.parseImageContentId())
                .then(_ => {
                    CustomDialogUtils.hiddenCustomDialog(context: this.context);
                    CustomDialogUtils.showCustomDialog(context: this.context,
                        child: new CustomLoadingDialog(message: "正在发布中"));
                    this.widget.actionModel.postAnswerDraft(arg1: draft.questionId, arg2: draft.answerId)
                        .then(v => {
                            CustomDialogUtils.hiddenCustomDialog(context: this.context);
                            Navigator.pop(context: this.context);
                            this.widget.actionModel.removeAnswerDraft();
                            CustomDialogUtils.showToast(context: this.context, "审核中，可在“我的-创作中心”查看进度",
                                iconData: CIcons.sentiment_satisfied, 2);
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

        void saveAnswerDraft() {
            var draft = this.widget.viewModel.answerDraft;
            var content = this.content.Trim();
            CustomDialogUtils.showCustomDialog(context: this.context, child: new CustomLoadingDialog(message: "正在保存中"));
            this.widget.actionModel
                .saveAnswerDraft(arg1: draft.questionId, arg2: draft.answerId, arg3: content,
                    content.parseImageContentId())
                .then(_ => {
                    CustomDialogUtils.hiddenCustomDialog(context: this.context);
                    Navigator.pop(context: this.context);
                    this.widget.actionModel.resetAnswerDraft();
                    CustomDialogUtils.showToast(context: this.context, "保存成功", iconData: CIcons.sentiment_satisfied);
                })
                .catchError(err => {
                    CustomDialogUtils.hiddenCustomDialog(context: this.context);
                    CustomDialogUtils.showToast(context: this.context, "草稿保存失败",
                        iconData: CIcons.sentiment_dissatisfied);
                });
        }

        void _dismissKeyboard() {
            this._contentInputFieldFocusNode.unfocus();
            this._contentInputFieldHasFocus = false;
            this._isShowKeyBoard = false;
            TextInputPlugin.TextInputHide();
        }
    }
}