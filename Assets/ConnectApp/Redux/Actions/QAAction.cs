using System.Collections.Generic;
using ConnectApp.Api;
using ConnectApp.Common.Util;
using ConnectApp.Models.Api;
using ConnectApp.Models.Model;
using ConnectApp.Models.State;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.Redux;

namespace ConnectApp.redux.actions {
    public class BlockQuestionAction : RequestAction {
        public string questionId;
    }

    public class BlockAnswerAction : RequestAction {
        public string answerId;
    }

    public class QuestionAction : BaseAction {
        public List<Question> questions;
    }

    public class AnswerAction : BaseAction {
        public List<Answer> answers;
    }

    public class AnswerDraftAction : BaseAction {
        public List<AnswerDraft> drafts;
    }

    public class QAImageAction : BaseAction {
        public Dictionary<string, ContentData> contentMap;
    }

    public class NewMessageAction : BaseAction {
        public Dictionary<string, NewMessage> messageMap;
    }

    public class QALikeAction : BaseAction {
        public Dictionary<string, QALike> likeMap;
    }

    public class NextAnswerIdAction : BaseAction {
        public string answerId;
        public string nextAnswerId;
    }

    // public class StartFetchQuestionAction : BaseAction {
    // }

    public class FetchQuestionsSuccessAction : BaseAction {
        public List<string> questionIds;
        public int page;
        public bool hasMore;
        public QATab tab;
    }

    public class FetchQuestionsFailureAction : BaseAction {
    }

    public class FetchQuestionDetailSuccessAction : BaseAction {
        public string questionId;
        public Question question;
        public List<string> answerIds;
    }

    public class FetchQuestionDetailFailureAction : BaseAction {
    }

    public class AnswerHasMoreAction : BaseAction {
        public string questionId;
        public bool hasMore;
    }

    public class FetchAnswersSuccessAction : BaseAction {
        public string questionId;
        public List<string> answerIds;
        public bool hasMore;
        public int page;
    }

    public class FetchAnswersFailureAction : BaseAction {
    }

    public class FetchAnswerDetailSuccessAction : BaseAction {
        public string channelId;
        public List<string> messageIds;
    }

    public class FetchAnswerDetailFailureAction : BaseAction {
    }

    public class FetchQATopCommentSuccessAction : BaseAction {
        public string channelId;
        public string after;
        public bool hasMore;
        public List<string> messageIds;
        public Dictionary<string, ChildNewMessage> childMessageMap;
    }

    public class FetchQATopCommentFailureAction : BaseAction {
    }

    public class FetchQACommentDetailSuccessAction : BaseAction {
        // public string channelId;
        public string messageId;
        public string after;
        public bool hasMore;
        public List<string> messageIds;
    }

    public class FetchQACommentDetailFailureAction : BaseAction {
    }

    public class SendQAMessageSuccessAction : BaseAction {
        public QAMessageType type;
        public string itemId;
        public string messageId;
    }

    public class SendQAMessageFailureAction : BaseAction {
    }

    public class LikeSuccessAction : BaseAction {
        public QALikeType likeType;
        public QALike like;
    }

    public class LikeFailureAction : BaseAction {
    }

    public class RemoveLikeSuccessAction : BaseAction {
        public QALikeType likeType;
        public string itemId;
    }

    public class RemoveLikeFailureAction : BaseAction {
    }

    public class StartCreateQuestionDraftAction : BaseAction {
    }

    public class CreateQuestionDraftSuccessAction : BaseAction {
        public QuestionDraft questionDraft;
    }

    public class CreateQuestionDraftFailureAction : BaseAction {
    }
    public class FetchAllPlatesSuccessAction : BaseAction {
        public List<Plate> plates;
    }
    
    public class ChangeQuestionPlateAction : BaseAction {
        public Plate plate;
    }

    public class StartFetchQuestionDraftAction : BaseAction {
    }

    public class FetchQuestionDraftSuccessAction : BaseAction {
        public QuestionDraft questionDraft;
    }

    public class FetchQuestionDraftFailureAction : BaseAction {
    }

    public class UpdateQuestionTagsSuccessAction : BaseAction {
        public List<string> tagList;
    }

    public class RestQuestionDraftAction : BaseAction {
    }

    public class UpdateQuestionSuccessAction : BaseAction {
        public QuestionDraft questionDraft;
    }

    public class PublishQuestionSuccessAction : BaseAction {
        public QuestionDraft questionDraft;
    }

    public class StartCreateAnswerAction : BaseAction {
    }

    public class CreateAnswerSuccessAction : BaseAction {
        public AnswerDraft answerDraft;
    }

    public class CreateAnswerFailureAction : BaseAction {
    }

    public class StartFetchAnswerDraftAction : BaseAction {
    }

    public class FetchAnswerDraftSuccessAction : BaseAction {
        public AnswerDraft answerDraft;
    }

    public class FetchAnswerDraftFailureAction : BaseAction {
    }

    public class UpdateAnswerSuccessAction : BaseAction {
        public AnswerDraft answerDraft;
    }

    public class PublishAnswerSuccessAction : BaseAction {
        public AnswerDraft answerDraft;
    }

    public class RemoveAnswerDraftAction : BaseAction {
        public string questionId;
    }

    public class ResetAnswerDraftAction : BaseAction {
    }

    public class AllowAnswerQuestionAction : BaseAction {
        public string questionId;
        public bool canAnswer;
    }

    public class FetchUserQuestionsSuccessAction : BaseAction {
        public string userId;
        public List<string> questionIds;
        public int page;
        public bool hasMore;
    }

    public class FetchUserAnswersSuccessAction : BaseAction {
        public string userId;
        public List<string> answerIds;
        public int page;
        public bool hasMore;
    }

    public class FetchUserAnswerDraftsSuccessAction : BaseAction {
        // questionIds
        public List<string> answerDraftIds;
        public int page;
        public bool hasMore;
    }

    public class DeleterAnswerDraftSuccessAction : BaseAction {
        public string questionId;
    }

    public class UploadQuestionImageSuccessAction : BaseAction {
        public string questionId;
        public string contentId;
        public string tmpId;
        public QAAttachment attachment;
    }

    public class UploadAnswerImageSuccessAction : BaseAction {
        public string questionId;
        public string answerId;
        public string contentId;
        public string tmpId;
        public QAAttachment attachment;
    }

    public static partial class CActions {
        public static object fetchQuestions(QATab tab, string type, int page) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return QAApi.FetchQuestions(tab: tab, type: type, page: page)
                    .then(data => {
                        if (!(data is FetchQuestionsResponse questionsResponse)) {
                            return;
                        }

                        // tag
                        dispatcher.dispatch(new TagMapAction {tagMap = questionsResponse.tagSimpleMap});
                        // questions
                        var questions = questionsResponse.questionSimpleList;
                        var questionIds = new List<string>();
                        if (questions.isNotNullAndEmpty()) {
                            dispatcher.dispatch(new QuestionAction {
                                questions = questions
                            });
                            questions.ForEach(question => { questionIds.Add(item: question.id); });
                        }

                        dispatcher.dispatch(new FetchQuestionsSuccessAction {
                            questionIds = questionIds,
                            page = questionsResponse.currentPage,
                            hasMore = questionsResponse.hasMore,
                            tab = tab
                        });
                    })
                    .catchError(err => { dispatcher.dispatch(new FetchQuestionsFailureAction()); });
            });
        }

        public static object fetchQuestionDetail(string questionId) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return QAApi.FetchQuestionDetail(questionId: questionId)
                    .then(data => {
                        if (!(data is FetchQuestionDetailResponse questionDetailResponse)) {
                            return;
                        }

                        // user
                        dispatcher.dispatch(new UserMapAction {userMap = questionDetailResponse.userSimpleV2Map});
                        // userLicenseMap
                        dispatcher.dispatch(new UserLicenseMapAction
                            {userLicenseMap = questionDetailResponse.userLicenseMap});
                        // tag
                        dispatcher.dispatch(new TagMapAction {tagMap = questionDetailResponse.tagSimpleMap});
                        // image
                        dispatcher.dispatch(new QAImageAction {contentMap = questionDetailResponse.contentMap});
                        // answer
                        var answers = questionDetailResponse.answerSimpleList;
                        var answerIds = new List<string>();
                        if (answers.isNotNullAndEmpty()) {
                            dispatcher.dispatch(new AnswerAction {
                                answers = answers
                            });
                            answers.ForEach(answer => { answerIds.Add(item: answer.id); });
                        }

                        // like
                        dispatcher.dispatch(new QALikeAction {likeMap = questionDetailResponse.likeItemMap});
                        // canAnswer
                        dispatcher.dispatch(new AllowAnswerQuestionAction {
                            questionId = questionId,
                            canAnswer = questionDetailResponse.canAnswer
                        });
                        // question
                        var question = questionDetailResponse.question;
                        // parse markdown
                        if (question.description.isNotEmpty()) {
                            question.markdownBodyNodes = MarkdownUtils.parseMarkdown(data: question.description);
                        }
                        dispatcher.dispatch(new FetchQuestionDetailSuccessAction {
                            questionId = questionId,
                            question = question,
                            answerIds = answerIds
                        });
                    })
                    .catchError(err => { dispatcher.dispatch(new FetchQuestionDetailFailureAction()); });
            });
        }

        public static object fetchAnswers(string questionId, int page) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return QAApi.FetchAnswers(questionId: questionId, page: page)
                    .then(data => {
                        if (!(data is FetchAnswersResponse answersResponse)) {
                            return;
                        }

                        // user
                        dispatcher.dispatch(new UserMapAction {userMap = answersResponse.userSimpleV2Map});
                        // userLicenseMap
                        dispatcher.dispatch(new UserLicenseMapAction
                            {userLicenseMap = answersResponse.userLicenseMap});
                        // image
                        dispatcher.dispatch(new QAImageAction {contentMap = answersResponse.contentMap});
                        // answer
                        var answers = answersResponse.answerSimpleList;
                        var answerIds = new List<string>();
                        if (answers.isNotNullAndEmpty()) {
                            dispatcher.dispatch(new AnswerAction {
                                answers = answers
                            });
                            answers.ForEach(answer => { answerIds.Add(item: answer.id); });
                        }

                        // question
                        dispatcher.dispatch(new FetchAnswersSuccessAction {
                            questionId = questionId,
                            answerIds = answerIds,
                            hasMore = answersResponse.hasMore,
                            page = page
                        });
                    })
                    .catchError(err => { dispatcher.dispatch(new FetchAnswersFailureAction()); });
            });
        }

        public static object fetchAnswerDetail(string questionId, string answerId) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return QAApi.FetchAnswerDetail(questionId: questionId, answerId: answerId)
                    .then(data => {
                        if (!(data is FetchAnswerDetailResponse answerDetailResponse)) {
                            return;
                        }

                        // user
                        dispatcher.dispatch(new UserMapAction {userMap = answerDetailResponse.userSimpleV2Map});
                        // userLicenseMap
                        dispatcher.dispatch(new UserLicenseMapAction
                            {userLicenseMap = answerDetailResponse.userLicenseMap});
                        // image
                        dispatcher.dispatch(new QAImageAction {contentMap = answerDetailResponse.contentMap});
                        // question
                        var questions = new List<Question> {answerDetailResponse.questionSimple};
                        if (questions.isNotNullAndEmpty()) {
                            dispatcher.dispatch(new QuestionAction {
                                questions = questions
                            });
                        }

                        // canAnswer
                        dispatcher.dispatch(new AllowAnswerQuestionAction {
                            questionId = questionId,
                            canAnswer = answerDetailResponse.canAnswer
                        });
                        // answer
                        var answer = answerDetailResponse.answer;
                        // parse markdown
                        if (answer.description.isNotEmpty()) {
                            answer.markdownBodyNodes = MarkdownUtils.parseMarkdown(data: answer.description);
                        }
                        var answers = new List<Answer> {answer};
                        if (answers.isNotNullAndEmpty()) {
                            dispatcher.dispatch(new AnswerAction {
                                answers = answers
                            });
                        }

                        // nextAnswerId
                        var nextAnswerId = answerDetailResponse.nextAnswerId ?? "";
                        dispatcher.dispatch(new NextAnswerIdAction {
                            answerId = answerId,
                            nextAnswerId = nextAnswerId
                        });

                        // follow
                        dispatcher.dispatch(new FollowMapAction {followMap = answerDetailResponse.followMap});

                        // message
                        var messages = answerDetailResponse.messages;
                        var messageIds = new List<string>();
                        var messageMap = new Dictionary<string, NewMessage>();
                        if (messages.isNotNullAndEmpty()) {
                            messages.ForEach(message => {
                                messageIds.Add(item: message.id);
                                messageMap.Add(key: message.id, value: message);
                            });
                        }

                        dispatcher.dispatch(new NewMessageAction {messageMap = messageMap});

                        // like
                        dispatcher.dispatch(new QALikeAction {likeMap = answerDetailResponse.likeItemMap});

                        dispatcher.dispatch(new FetchAnswerDetailSuccessAction {
                            channelId = answerDetailResponse.answer.channelId,
                            messageIds = messageIds
                        });
                    })
                    .catchError(err => { dispatcher.dispatch(new FetchAnswerDetailFailureAction()); });
            });
        }

        public static object fetchQATopLevelComment(string channelId, string after) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return QAApi.FetchTopLevelMessage(channelId: channelId, after: after)
                    .then(data => {
                        if (!(data is FetchTopLevelMessageResponse commentResponse)) {
                            return;
                        }

                        // user
                        dispatcher.dispatch(new UserMapAction {userMap = commentResponse.userSimpleV2Map});
                        // userLicenseMap
                        dispatcher.dispatch(new UserLicenseMapAction
                            {userLicenseMap = commentResponse.userLicenseMap});
                        // message
                        var messageMap = commentResponse.messageMap;
                        if (messageMap.isNotNullAndEmpty()) {
                            dispatcher.dispatch(new NewMessageAction {messageMap = messageMap});
                        }

                        // like
                        dispatcher.dispatch(new QALikeAction {likeMap = commentResponse.likeItemMap});

                        var messageIds = commentResponse.messageIdList ?? new List<string>();
                        var childMessageMap =
                            commentResponse.childMessageMap ?? new Dictionary<string, ChildNewMessage>();
                        dispatcher.dispatch(new FetchQATopCommentSuccessAction {
                            channelId = channelId,
                            after = after,
                            hasMore = commentResponse.hasMore,
                            messageIds = messageIds,
                            childMessageMap = childMessageMap
                        });
                    })
                    .catchError(err => { dispatcher.dispatch(new FetchQATopCommentFailureAction()); });
            });
        }

        public static object fetchQACommentDetail(string channelId, string messageId, string after) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return QAApi.FetchQACommentDetail(channelId: channelId, messageId: messageId, after: after)
                    .then(data => {
                        if (!(data is FetchSecondLevelMessageResponse commentResponse)) {
                            return;
                        }

                        // user
                        dispatcher.dispatch(new UserMapAction {userMap = commentResponse.userSimpleV2Map});
                        // userLicenseMap
                        dispatcher.dispatch(new UserLicenseMapAction
                            {userLicenseMap = commentResponse.userLicenseMap});
                        // message
                        var messageMap = commentResponse.allMessageMap;
                        if (messageMap.isNotNullAndEmpty()) {
                            dispatcher.dispatch(new NewMessageAction {messageMap = messageMap});
                        }

                        // like
                        dispatcher.dispatch(new QALikeAction {likeMap = commentResponse.likeItemMap});

                        var messageIds = commentResponse.childMessageList ?? new List<string>();
                        dispatcher.dispatch(new FetchQACommentDetailSuccessAction {
                            messageId = messageId,
                            after = after,
                            hasMore = commentResponse.hasMore,
                            messageIds = messageIds
                        });
                    })
                    .catchError(err => { dispatcher.dispatch(new FetchQACommentDetailFailureAction()); });
            });
        }

        public static object sendQAMessage(QAMessageType messageType, string itemId, string channelId, string content,
            string nonce, string parentMessageId,
            string upperMessageId) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return QAApi.SendQAMessage(channelId: channelId, content: content, nonce: nonce,
                        parentMessageId: parentMessageId, upperMessageId: upperMessageId)
                    .then(data => {
                        // CustomDialogUtils.hiddenCustomDialog();
                        if (!(data is NewMessage newMessage)) {
                            return;
                        }

                        var messageMap = new Dictionary<string, NewMessage> {{newMessage.id, newMessage}};
                        dispatcher.dispatch(new NewMessageAction {messageMap = messageMap});
                        // CustomDialogUtils.showToast("发送成功", iconData: CIcons.sentiment_satisfied);
                        dispatcher.dispatch(new SendQAMessageSuccessAction
                            {type = messageType, itemId = itemId, messageId = newMessage.id ?? ""});
                    });
                    // .catchError(error => {
                    //     // CustomDialogUtils.hiddenCustomDialog();
                    //     // CustomDialogUtils.showToast("发送失败", iconData: CIcons.sentiment_dissatisfied);
                    //     dispatcher.dispatch(new SendQAMessageFailureAction());
                    // });
            });
        }

        public static object qaLike(QALikeType likeType, string questionId = "", string answerId = "",
            string channelId = "", string messageId = "") {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return QAApi.QALike(likeType: likeType, questionId: questionId, answerId: answerId,
                        channelId: channelId, messageId: messageId)
                    .then(data => {
                        if (!(data is QALike like)) {
                            return;
                        }

                        dispatcher.dispatch(new LikeSuccessAction {
                            likeType = likeType,
                            like = like
                        });
                    })
                    .catchError(error => {
                        // CustomDialogUtils.showToast("点赞失败", iconData: CIcons.sentiment_dissatisfied);
                        dispatcher.dispatch(new LikeFailureAction());
                    });
            });
        }

        public static object qaRemoveLike(QALikeType likeType, string questionId = "", string answerId = "",
            string channelId = "", string messageId = "") {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return QAApi.QARemoveLike(likeType: likeType, questionId: questionId, answerId: answerId,
                        channelId: channelId, messageId: messageId)
                    .then(data => {
                        if (!(data is string itemId)) {
                            return;
                        }

                        dispatcher.dispatch(new RemoveLikeSuccessAction {
                            likeType = likeType,
                            itemId = itemId
                        });
                    })
                    .catchError(error => {
                        dispatcher.dispatch(new RemoveLikeFailureAction());
                    });
            });
        }

        public static object createQuestion() {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return QAApi.CreateQuestion()
                    .then(data => {
                        if (!(data is QuestionDraftResponse response)) {
                            return;
                        }

                        // tag
                        dispatcher.dispatch(new TagMapAction {tagMap = response.tagSimpleMap});
                        // image
                        dispatcher.dispatch(new QAImageAction {contentMap = response.contentMap});
                        // plate 
                        var plateSimpleMap = response.plateSimpleMap;
                        var plateId = response.questionData.plateId;
                        Plate plate = null;
                        if (plateId.isNotEmpty() && plateSimpleMap.ContainsKey(key: plateId)) {
                            plateSimpleMap.TryGetValue(key: plateId, value: out plate);
                        }
                        // question
                        var question = response.questionData;
                        var questionDraft = new QuestionDraft {
                            questionId = question.id,
                            title = question.title,
                            description = question.description,
                            plate = plate,
                            tagIds = question.tagIds,
                            contentIds = question.contentIds
                        };
                        dispatcher.dispatch(new CreateQuestionDraftSuccessAction {
                            questionDraft = questionDraft
                        });
                    });
            });
        }

        public static object fetchAllPlates() {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return QAApi.FetchAllPlates()
                    .then(data => {
                        if (!(data is List<Plate> response)) {
                            return;
                        }
                        dispatcher.dispatch(new FetchAllPlatesSuccessAction {
                            plates = response
                        });
                    });
            });
        }

        public static object fetchQuestionDraft(string questionId) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return QAApi.FetchQuestionDraft(questionId: questionId)
                    .then(data => {
                        if (!(data is QuestionDraftResponse response)) {
                            return;
                        }

                        // tag
                        dispatcher.dispatch(new TagMapAction {tagMap = response.tagSimpleMap});
                        // image
                        dispatcher.dispatch(new QAImageAction {contentMap = response.contentMap});
                        // question
                        var question = response.questionData;
                        dispatcher.dispatch(new QuestionAction {
                            questions = new List<Question> {question}
                        });
                        // plate 
                        var plateSimpleMap = response.plateSimpleMap;
                        var plateId = response.questionData.plateId;
                        Plate plate = null;
                        if (plateId.isNotEmpty() && plateSimpleMap.ContainsKey(key: plateId)) {
                            plateSimpleMap.TryGetValue(key: plateId, value: out plate);
                        }
                        var questionDraft = new QuestionDraft {
                            questionId = question.id,
                            title = question.title,
                            description = question.description,
                            plate = plate,
                            tagIds = question.tagIds,
                            contentIds = question.contentIds
                        };
                        dispatcher.dispatch(new FetchQuestionDraftSuccessAction {
                            questionDraft = questionDraft
                        });
                    }).catchError(err => { dispatcher.dispatch(new FetchQuestionDraftFailureAction()); });
            });
        }

        public static object updateQuestionTags(string questionId, List<string> tagIds) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return QAApi.UpdateQuestionTags(questionId: questionId, tagIds: tagIds)
                    .then(data => {
                        if (!(data is UpdateQuestionTagsResponse tagsResponse)) {
                            return;
                        }

                        var tagMap = new Dictionary<string, Tag>();
                        if (tagsResponse.skills.isNotNullAndEmpty()) {
                            tagsResponse.skills.ForEach(tag => { tagMap.Add(key: tag.id, value: tag); });
                            dispatcher.dispatch(new TagMapAction {tagMap = tagMap});
                        }

                        dispatcher.dispatch(new UpdateQuestionTagsSuccessAction {tagList = tagsResponse.skillIds});
                    });
            });
        }

        public static object updateQuestion(string questionId, string title, string description,
            List<string> contentIds, string plateId) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return QAApi.UpdateQuestion(questionId: questionId, title: title, description: description,
                        contentIds: contentIds, plateId: plateId)
                    .then(data => {
                        if (!(data is Question question)) {
                            return;
                        }

                        dispatcher.dispatch(new QuestionAction {
                            questions = new List<Question> {question}
                        });
                        var questionDraft = new QuestionDraft {
                            questionId = question.id,
                            title = question.title,
                            description = question.description,
                            tagIds = question.tagIds,
                            contentIds = question.contentIds
                        };
                        dispatcher.dispatch(new UpdateQuestionSuccessAction {
                            questionDraft = questionDraft
                        });
                    });
            });
        }

        public static object publishQuestion(string questionId) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return QAApi.PublishQuestion(questionId: questionId)
                    .then(data => {
                        if (!(data is Question question)) {
                            return;
                        }

                        dispatcher.dispatch(new QuestionAction {
                            questions = new List<Question> {question}
                        });
                        var questionDraft = new QuestionDraft {
                            questionId = question.id,
                            title = question.title,
                            description = question.description,
                            tagIds = question.tagIds,
                            contentIds = question.contentIds
                        };
                        dispatcher.dispatch(new PublishQuestionSuccessAction {
                            questionDraft = questionDraft
                        });
                    });
            });
        }

        public static object createAnswer(string questionId) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return QAApi.CreateAnswer(questionId: questionId)
                    .then(data => {
                        if (!(data is AnswerDraftResponse response)) {
                            return;
                        }

                        // image
                        dispatcher.dispatch(new QAImageAction {contentMap = response.contentMap});
                        var question = getState().qaState.questionDict
                            .GetValueOrDefault(key: questionId, new Question {title = ""});
                        // answer
                        var answer = response.answerData;
                        var answerDraft = new AnswerDraft {
                            answerId = answer.id,
                            questionId = questionId,
                            questionTitle = question.title,
                            description = answer.description,
                            descriptionPlain = answer.descriptionPlain,
                            createdTime = answer.createdTime,
                            contentIds = answer.contentIds
                        };
                        dispatcher.dispatch(new CreateAnswerSuccessAction {
                            answerDraft = answerDraft
                        });
                        dispatcher.dispatch(new AnswerDraftAction {drafts = new List<AnswerDraft> {answerDraft}});
                    }).catchError(err => { dispatcher.dispatch(new CreateAnswerFailureAction()); });
            });
        }

        public static object fetchAnswerDraft(string questionId, string answerId) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return QAApi.FetchAnswerDraft(questionId: questionId, answerId: answerId)
                    .then(data => {
                        if (!(data is AnswerDraftResponse response)) {
                            return;
                        }

                        // image
                        dispatcher.dispatch(new QAImageAction {contentMap = response.contentMap});
                        var question = getState().qaState.questionDict
                            .GetValueOrDefault(key: questionId, new Question {title = ""});
                        // answer
                        var answer = response.answerData;
                        dispatcher.dispatch(new AnswerAction {
                            answers = new List<Answer> {answer}
                        });
                        var answerDraft = new AnswerDraft {
                            answerId = answer.id,
                            questionId = questionId,
                            questionTitle = question.title,
                            description = answer.description,
                            descriptionPlain = answer.descriptionPlain,
                            createdTime = answer.createdTime,
                            contentIds = answer.contentIds
                        };
                        dispatcher.dispatch(new FetchAnswerDraftSuccessAction {
                            answerDraft = answerDraft
                        });
                        dispatcher.dispatch(new AnswerDraftAction {drafts = new List<AnswerDraft> {answerDraft}});
                    }).catchError(err => { dispatcher.dispatch(new FetchAnswerDraftFailureAction()); });
            });
        }

        public static object updateAnswer(string questionId, string answerId, string description,
            List<string> contentIds) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return QAApi.UpdateAnswer(questionId: questionId, answerId: answerId, description: description,
                        contentIds: contentIds)
                    .then(data => {
                        if (!(data is AnswerDraftResponse response)) {
                            return;
                        }

                        // image
                        dispatcher.dispatch(new QAImageAction {contentMap = response.contentMap});
                        var question = getState().qaState.questionDict
                            .GetValueOrDefault(key: questionId, new Question {title = ""});
                        // answer
                        var answer = response.answerData;
                        dispatcher.dispatch(new AnswerAction {
                            answers = new List<Answer> {answer}
                        });
                        var answerDraft = new AnswerDraft {
                            answerId = answer.id,
                            questionId = questionId,
                            questionTitle = question.title,
                            description = answer.description,
                            descriptionPlain = answer.descriptionPlain,
                            createdTime = answer.createdTime,
                            contentIds = answer.contentIds
                        };
                        dispatcher.dispatch(new UpdateAnswerSuccessAction {
                            answerDraft = answerDraft
                        });
                        dispatcher.dispatch(new AnswerDraftAction {drafts = new List<AnswerDraft> {answerDraft}});
                    });
            });
        }

        public static object publishAnswer(string questionId, string answerId) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return QAApi.PublishAnswer(questionId: questionId, answerId: answerId)
                    .then(data => {
                        if (!(data is AnswerDraftResponse response)) {
                            return;
                        }

                        // image
                        dispatcher.dispatch(new QAImageAction {contentMap = response.contentMap});
                        var question = getState().qaState.questionDict
                            .GetValueOrDefault(key: questionId, new Question {title = ""});
                        // answer
                        var answer = response.answerData;
                        dispatcher.dispatch(new AnswerAction {
                            answers = new List<Answer> {answer}
                        });
                        var answerDraft = new AnswerDraft {
                            answerId = answer.id,
                            questionId = questionId,
                            questionTitle = question.title,
                            description = answer.description,
                            descriptionPlain = answer.descriptionPlain,
                            createdTime = answer.createdTime,
                            contentIds = answer.contentIds
                        };
                        dispatcher.dispatch(new PublishAnswerSuccessAction {
                            answerDraft = answerDraft
                        });
                        dispatcher.dispatch(new RemoveAnswerDraftAction {
                            questionId = questionId
                        });
                    });
            });
        }

        public static object fetchUserQuestions(string userId, int page) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return QAApi.FetchUserQuestions(userId: userId, page: page)
                    .then(data => {
                        if (!(data is UserQuestionResponse response)) {
                            return;
                        }

                        // questions
                        var questions = response.questions;
                        var questionIds = new List<string>();
                        if (questions.isNotNullAndEmpty()) {
                            dispatcher.dispatch(new QuestionAction {
                                questions = questions
                            });
                            questions.ForEach(question => { questionIds.Add(item: question.id); });
                        }

                        dispatcher.dispatch(new FetchUserQuestionsSuccessAction {
                            userId = userId,
                            questionIds = questionIds,
                            page = response.currentPage,
                            hasMore = response.hasMore
                        });
                    });
            });
        }

        public static object fetchUserAnswers(string userId, int page) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return QAApi.FetchUserAnswers(userId: userId, page: page)
                    .then(data => {
                        if (!(data is UserAnswerResponse response)) {
                            return;
                        }

                        // answer
                        var answers = response.answers;
                        var answerIds = new List<string>();
                        if (answers.isNotNullAndEmpty()) {
                            dispatcher.dispatch(new AnswerAction {
                                answers = answers
                            });
                            answers.ForEach(answer => { answerIds.Add(item: answer.id); });
                        }

                        // questions
                        var questionMap = response.questionSimpleMap;
                        var questions = new List<Question>();
                        if (questionMap.isNotNullAndEmpty()) {
                            foreach (var keyValue in questionMap) {
                                questions.Add(item: keyValue.Value);
                            }

                            dispatcher.dispatch(new QuestionAction {
                                questions = questions
                            });
                        }

                        dispatcher.dispatch(new FetchUserAnswersSuccessAction {
                            userId = userId,
                            answerIds = answerIds,
                            hasMore = response.hasMore,
                            page = page
                        });
                    });
            });
        }

        public static object fetchUserAnswerDrafts(int page) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return QAApi.FetchUserAnswerDrafts(page: page)
                    .then(data => {
                        if (!(data is UserAnswerDraftResponse response)) {
                            return;
                        }

                        var answers = response.answers;
                        var questionMap = response.questionSimpleMap;
                        // questions
                        var questions = new List<Question>();
                        if (questionMap.isNotNullAndEmpty()) {
                            foreach (var keyValue in questionMap) {
                                questions.Add(item: keyValue.Value);
                            }

                            dispatcher.dispatch(new QuestionAction {
                                questions = questions
                            });
                        }

                        // answerDraft
                        var answerDrafts = new List<AnswerDraft>();
                        var answerDraftIds = new List<string>();
                        if (answers.isNotNullAndEmpty() && questionMap.isNotNullAndEmpty()) {
                            answers.ForEach(answer => {
                                var question = questionMap.GetValueOrDefault(key: answer.questionId,
                                    new Question {title = ""});
                                answerDrafts.Add(new AnswerDraft {
                                    answerId = answer.id,
                                    questionId = answer.questionId,
                                    questionTitle = question.title,
                                    description = answer.description,
                                    descriptionPlain = answer.descriptionPlain,
                                    createdTime = answer.createdTime,
                                    contentIds = answer.contentIds
                                });
                                answerDraftIds.Add(item: answer.questionId);
                            });
                            dispatcher.dispatch(new AnswerDraftAction {drafts = answerDrafts});
                        }

                        dispatcher.dispatch(new FetchUserAnswerDraftsSuccessAction {
                            answerDraftIds = answerDraftIds,
                            hasMore = response.hasMore,
                            page = page
                        });
                    });
            });
        }

        public static object deleteAnswerDraft(string questionId, string answerId) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return QAApi.DeleteAnswerDraft(questionId: questionId, answerId: answerId)
                    .then(response => {
                        dispatcher.dispatch(new DeleterAnswerDraftSuccessAction {
                            questionId = questionId
                        });
                    });
            });
        }

        public static object uploadQuestionImage(string questionId, byte[] data, string tmpId) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return QAApi.UploadQuestionImage(questionId: questionId, data: data)
                    .then(imageData => {
                        if (!(imageData is QAAttachment response)) {
                            return;
                        }

                        dispatcher.dispatch(new UploadQuestionImageSuccessAction {
                            questionId = questionId,
                            contentId = response.id,
                            tmpId = tmpId,
                            attachment = response
                        });
                    });
            });
        }

        public static object uploadAnswerImage(string questionId, string answerId, byte[] data, string tmpId) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return QAApi.UploadAnswerImage(questionId: questionId, answerId: answerId, data: data)
                    .then(imageData => {
                        if (!(imageData is QAAttachment response)) {
                            return;
                        }

                        dispatcher.dispatch(new UploadAnswerImageSuccessAction {
                            questionId = questionId,
                            answerId = answerId,
                            contentId = response.id,
                            tmpId = tmpId,
                            attachment = response
                        });
                    });
            });
        }
    }
}