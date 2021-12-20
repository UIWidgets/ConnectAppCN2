using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Models.Api;
using ConnectApp.Models.Model;
using Newtonsoft.Json;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;

namespace ConnectApp.Api {
    public static class QAApi {
        public static Future<FetchQuestionsResponse> FetchQuestions(QATab tab, string type, int page) {
            var url = CStringUtils.genApiUrl("/ask/home/list");
            var para = new Dictionary<string, object> {
                // tab = latest / hot / pending
                {"tab", tab.ToString()},
                // {"type", type},
                {"page", page}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchQuestionsResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var questionsResponse = JsonConvert.DeserializeObject<FetchQuestionsResponse>(value: responseText);
                return FutureOr.value(value: questionsResponse);
            });
        }

        public static Future<FetchQuestionDetailResponse> FetchQuestionDetail(string questionId) {
            var url = CStringUtils.genApiUrl($"/ask/question/{questionId}");
            var request = HttpManager.GET(url: url);
            return HttpManager.resume(request: request).then_<FetchQuestionDetailResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var questionDetailResponse =
                    JsonConvert.DeserializeObject<FetchQuestionDetailResponse>(value: responseText);
                return FutureOr.value(value: questionDetailResponse);
            });
        }

        public static Future<FetchAnswersResponse> FetchAnswers(string questionId, int page) {
            var url = CStringUtils.genApiUrl($"/ask/question/{questionId}/answer");
            var para = new Dictionary<string, object> {
                {"page", page}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchAnswersResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var answersResponse =
                    JsonConvert.DeserializeObject<FetchAnswersResponse>(value: responseText);
                return FutureOr.value(value: answersResponse);
            });
        }

        public static Future<FetchAnswerDetailResponse> FetchAnswerDetail(string questionId, string answerId) {
            var url = CStringUtils.genApiUrl($"/ask/question/{questionId}/answer/{answerId}");
            var request = HttpManager.GET(url: url);
            return HttpManager.resume(request: request).then_<FetchAnswerDetailResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var answerDetailResponse =
                    JsonConvert.DeserializeObject<FetchAnswerDetailResponse>(value: responseText);
                return FutureOr.value(value: answerDetailResponse);
            });
        }

        public static Future<FetchTopLevelMessageResponse> FetchTopLevelMessage(string channelId, string after = "") {
            var url = CStringUtils.genApiUrl($"/ask/channel/{channelId}/floorMessage");
            var para = new Dictionary<string, object> {
                {"after", after}
            };
            var request =
                HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchTopLevelMessageResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var messageResponse =
                    JsonConvert.DeserializeObject<FetchTopLevelMessageResponse>(value: responseText);
                return FutureOr.value(value: messageResponse);
            });
        }

        public static Future<FetchSecondLevelMessageResponse> FetchQACommentDetail(string channelId, string messageId,
            string after = "") {
            var url = CStringUtils.genApiUrl($"/ask/channel/{channelId}/message/{messageId}/child");
            var para = new Dictionary<string, object> {
                {"after", after}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<FetchSecondLevelMessageResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var messageResponse =
                    JsonConvert.DeserializeObject<FetchSecondLevelMessageResponse>(value: responseText);
                return FutureOr.value(value: messageResponse);
            });
        }

        public static Future<NewMessage> SendQAMessage(string channelId, string content, string nonce,
            string parentMessageId = "", string upperMessageId = "") {
            var url = CStringUtils.genApiUrl($"/ask/channel/{channelId}/message");
            var para = new SendCommentParameter {
                content = content,
                nonce = nonce,
                parentMessageId = parentMessageId,
                upperMessageId = upperMessageId
            };
            var request = HttpManager.POST(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<NewMessage>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var newMessage = JsonConvert.DeserializeObject<NewMessage>(value: responseText);
                return FutureOr.value(value: newMessage);
            });
        }

        public static Future<QALike> QALike(QALikeType likeType, string questionId = "", string answerId = "",
            string channelId = "", string messageId = "") {
            var path = "";
            if (likeType == QALikeType.question) {
                path = $"/ask/question/{questionId}/like";
            }
            else if (likeType == QALikeType.answer) {
                path = $"/ask/question/{questionId}/answer/{answerId}/like";
            }
            else if (likeType == QALikeType.message) {
                path = $"/ask/channel/{channelId}/message/{messageId}/like";
            }

            var url = CStringUtils.genApiUrl(path: path);
            var request = HttpManager.POST(url: url);
            return HttpManager.resume(request: request).then_<QALike>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var like = JsonConvert.DeserializeObject<QALike>(value: responseText);
                return FutureOr.value(value: like);
            });
        }

        public static Future<string> QARemoveLike(QALikeType likeType, string questionId = "", string answerId = "",
            string channelId = "", string messageId = "") {
            var path = "";
            var itemId = "";
            if (likeType == QALikeType.question) {
                path = $"/ask/question/{questionId}/dislike";
                itemId = questionId;
            }
            else if (likeType == QALikeType.answer) {
                path = $"/ask/question/{questionId}/answer/{answerId}/dislike";
                itemId = answerId;
            }
            else if (likeType == QALikeType.message) {
                path = $"/ask/channel/{channelId}/message/{messageId}/dislike";
                itemId = messageId;
            }

            var url = CStringUtils.genApiUrl(path: path);
            var request = HttpManager.POST(url: url);
            return HttpManager.resume(request: request).then_<string>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                return FutureOr.value(value: itemId);
            });
        }

        public static Future<QuestionDraftResponse> CreateQuestion() {
            var url = CStringUtils.genApiUrl("/ask/question/0/new");
            var request = HttpManager.GET(url: url);
            return HttpManager.resume(request: request).then_<QuestionDraftResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var questionDraftResponse = JsonConvert.DeserializeObject<QuestionDraftResponse>(value: responseText);
                return FutureOr.value(value: questionDraftResponse);
            });
        }
        
        public static Future<List<Plate>> FetchAllPlates() {
            var url = CStringUtils.genApiUrl("/allPlates");
            var request = HttpManager.GET(url: url);
            return HttpManager.resume(request: request).then_<List<Plate>>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var platesResponse = JsonConvert.DeserializeObject<List<Plate>>(value: responseText);
                return FutureOr.value(value: platesResponse);
            });
        }

        public static Future<QuestionDraftResponse> FetchQuestionDraft(string questionId) {
            var url = CStringUtils.genApiUrl($"/ask/question/{questionId}/draft");
            var request = HttpManager.GET(url: url);
            return HttpManager.resume(request: request).then_<QuestionDraftResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var questionDraftResponse = JsonConvert.DeserializeObject<QuestionDraftResponse>(value: responseText);
                return FutureOr.value(value: questionDraftResponse);
            });
        }

        public static Future<UpdateQuestionTagsResponse> UpdateQuestionTags(string questionId, List<string> tagIds) {
            var url = CStringUtils.genApiUrl($"/ask/question/{questionId}/tags");
            var para = new Dictionary<string, object> {
                {"questionId", questionId},
                {"skillIds", tagIds}
            };
            var request = HttpManager.POST(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<UpdateQuestionTagsResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var tagsResponse = JsonConvert.DeserializeObject<UpdateQuestionTagsResponse>(value: responseText);
                return FutureOr.value(value: tagsResponse);
            });
        }

        public static Future<Question> UpdateQuestion(string questionId, string title, string description,
            List<string> contentIds, string plateId) {
            var url = CStringUtils.genApiUrl($"/ask/question/{questionId}/update");
            var para = new Dictionary<string, object> {
                {"title", title},
                {"description", description},
                {"contentIds", contentIds ?? new List<string>()},
                {"plateId", plateId}
            };
            var request = HttpManager.POST(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<Question>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var questionDraft = JsonConvert.DeserializeObject<Question>(value: responseText);
                return FutureOr.value(value: questionDraft);
            });
        }

        public static Future<Question> PublishQuestion(string questionId) {
            var url = CStringUtils.genApiUrl($"/ask/question/{questionId}/publish");
            var request = HttpManager.POST(url: url);
            return HttpManager.resume(request: request).then_<Question>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var questionDraft = JsonConvert.DeserializeObject<Question>(value: responseText);
                return FutureOr.value(value: questionDraft);
            });
        }

        public static Future<AnswerDraftResponse> CreateAnswer(string questionId) {
            var url = CStringUtils.genApiUrl($"/ask/question/{questionId}/answer/0/new");
            var request =
                HttpManager.GET(url: url);
            return HttpManager.resume(request: request).then_<AnswerDraftResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var answerDraftResponse = JsonConvert.DeserializeObject<AnswerDraftResponse>(value: responseText);
                return FutureOr.value(value: answerDraftResponse);
            });
        }

        public static Future<AnswerDraftResponse> FetchAnswerDraft(string questionId, string answerId) {
            var url = CStringUtils.genApiUrl($"/ask/question/{questionId}/answer/{answerId}/draft");
            var request = HttpManager.GET(url: url);
            return HttpManager.resume(request: request).then_<AnswerDraftResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var answerDraftResponse = JsonConvert.DeserializeObject<AnswerDraftResponse>(value: responseText);
                return FutureOr.value(value: answerDraftResponse);
            });
        }

        public static Future<AnswerDraftResponse> UpdateAnswer(string questionId, string answerId, string description,
            List<string> contentIds) {
            var url = CStringUtils.genApiUrl($"/ask/question/{questionId}/answer/{answerId}/update");
            var para = new Dictionary<string, object> {
                {"description", description},
                {"contentIds", contentIds}
            };
            var request = HttpManager.POST(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<AnswerDraftResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var answerDraftResponse = JsonConvert.DeserializeObject<AnswerDraftResponse>(value: responseText);
                return FutureOr.value(value: answerDraftResponse);
            });
        }

        public static Future<AnswerDraftResponse> PublishAnswer(string questionId, string answerId) {
            var url = CStringUtils.genApiUrl($"/ask/question/{questionId}/answer/{answerId}/publish");
            var request = HttpManager.POST(url: url);
            return HttpManager.resume(request: request).then_<AnswerDraftResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var answerDraftResponse = JsonConvert.DeserializeObject<AnswerDraftResponse>(value: responseText);
                return FutureOr.value(value: answerDraftResponse);
            });
        }

        public static Future<UserQuestionResponse> FetchUserQuestions(string userId, int page) {
            var url = CStringUtils.genApiUrl($"/ask/user/{userId}/questions");
            var para = new Dictionary<string, object> {
                {"page", page}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<UserQuestionResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var userQuestionResponse = JsonConvert.DeserializeObject<UserQuestionResponse>(value: responseText);
                return FutureOr.value(value: userQuestionResponse);
            });
        }

        public static Future<UserAnswerResponse> FetchUserAnswers(string userId, int page) {
            var url = CStringUtils.genApiUrl($"/ask/user/{userId}/answers");
            var para = new Dictionary<string, object> {
                {"page", page}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<UserAnswerResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var userAnswerResponse = JsonConvert.DeserializeObject<UserAnswerResponse>(value: responseText);
                return FutureOr.value(value: userAnswerResponse);
            });
        }

        public static Future<UserAnswerDraftResponse> FetchUserAnswerDrafts(int page) {
            var url = CStringUtils.genApiUrl("/ask/answerDrafts");
            var para = new Dictionary<string, object> {
                {"page", page}
            };
            var request = HttpManager.GET(url: url, parameter: para);
            return HttpManager.resume(request: request).then_<UserAnswerDraftResponse>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var userAnswerDraftResponse =
                    JsonConvert.DeserializeObject<UserAnswerDraftResponse>(value: responseText);
                return FutureOr.value(value: userAnswerDraftResponse);
            });
        }

        public static Future<bool> DeleteAnswerDraft(string questionId, string answerId) {
            var url = CStringUtils.genApiUrl($"/ask/question/{questionId}/answer/{answerId}/draft/rollback");
            var request = HttpManager.POST(url: url);
            return HttpManager.resume(request: request).then_<bool>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                return FutureOr.value(true);
            });
        }

        public static Future<QAAttachment> UploadQuestionImage(string questionId, byte[] data) {
            var url = CStringUtils.genApiUrl($"/ask/question/{questionId}/image");
            var para = new List<List<object>> {
                new List<object> {"askQuestionId", questionId},
                new List<object> {"file", data}
            };
            var request = HttpManager.POST(url: url, parameter: para, true, "image.png", "image/png");
            return HttpManager.resume(request: request).then_<QAAttachment>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var qaAttachment = JsonConvert.DeserializeObject<QAAttachment>(value: responseText);
                return FutureOr.value(value: qaAttachment);
            });
        }

        public static Future<QAAttachment> UploadAnswerImage(string questionId, string answerId, byte[] data) {
            var url = CStringUtils.genApiUrl($"/ask/question/{questionId}/image");
            var para = new List<List<object>> {
                new List<object> {"askQuestionId", questionId},
                new List<object> {"askAnswerId", answerId},
                new List<object> {"file", data}
            };
            var request = HttpManager.POST(url: url, parameter: para, true, "image.png", "image/png");
            return HttpManager.resume(request: request).then_<QAAttachment>(responseText => {
                if (responseText == null) {
                    throw new UIWidgetsError($"Unable to load: {url}");
                }

                var qaAttachment = JsonConvert.DeserializeObject<QAAttachment>(value: responseText);
                return FutureOr.value(value: qaAttachment);
            });
        }
    }
}