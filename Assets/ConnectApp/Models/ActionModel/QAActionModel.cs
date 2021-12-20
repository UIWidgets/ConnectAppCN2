using System;
using System.Collections.Generic;
using ConnectApp.Models.Model;
using Unity.UIWidgets.async;

namespace ConnectApp.Models.ActionModel {
    public class QAScreenActionModel : BaseActionModel {
        public Func<QATab, string, int, Future> fetchQuestions;
        public Action<string> blockQuestionAction;
    }

    public class QuestionDetailScreenActionModel : BaseActionModel {
        public Func<string, Future> fetchQuestionDetail;
        public Func<string, int, Future> fetchAnswers;
        public Action<string> openUrl;
        public Action<string> likeQuestion;
        public Action<string> removeLikeQuestion;
        public Action<string> blockQuestionAction;
    }

    public class AnswerDetailScreenActionModel : BaseActionModel {
        public Func<string, string, Future> fetchAnswerDetail;
        public Action<string> openUrl;
        public Action<string, string> likeAnswer;
        public Action<string, string> removeLikeAnswer;
        public Action<string, string> likeMessage;
        public Action<string, string> removeLikeMessage;
        public Action<string> startFollowUser;
        public Func<string, Future> followUser;
        public Action<string> startUnFollowUser;
        public Func<string, Future> unFollowUser;
        public Action<string> blockAnswerAction;
    }

    public class QATopLevelCommentScreenActionModel : BaseActionModel {
        public Func<string, string, Future> fetchQATopLevelComment;
        public Action<string, string> likeMessage;
        public Action<string, string> removeLikeMessage;
        public Func<QAMessageType, string, string, string, string, string, string, Future> sendComment;
    }

    public class QACommentDetailScreenActionModel : BaseActionModel {
        public Action<string, string> likeMessage;
        public Action<string, string> removeLikeMessage;
        public Func<string, string, string, Future> fetchQACommentDetail;
        public Func<string, string, string, string, string, Future> sendComment;
    }

    public class PostQuestionScreenActionModel : BaseActionModel {
        public Action restQuestionDraft;
        public Action startCreateQuestionDraft;
        public Action startFetchQuestionDraft;
        public Func<Future> fetchAllPlates;
        public Func<Future> createQuestionDraft;
        public Func<Future> fetchQuestionDraft;
        public Func<string, string, string, List<string>, string, Future> saveQuestionDraft;
        public Func<string, List<string>, Future> updateQuestionTags;
        public Func<string, Future> postQuestionDraft;
        public Func<string, byte[], string, Future> uploadQuestionImage;
    }

    public class AddTagScreenActionModel : BaseActionModel {
        public Func<string, List<string>, Future> updateQuestionTags;
        public Func<string, Future> searchTag;
    }

    public class PostAnswerScreenActionModel : BaseActionModel {
        public Action removeAnswerDraft;
        public Action resetAnswerDraft;
        public Action startCreateAnswerDraft;
        public Func<Future> createAnswerDraft;
        public Action startFetchAnswerDraft;
        public Func<Future> fetchAnswerDraft;
        public Func<string, string, string, List<string>, Future> saveAnswerDraft;
        public Func<string, string, Future> postAnswerDraft;
        public Func<string, string, byte[], string, Future> uploadAnswerImage;
    }
}