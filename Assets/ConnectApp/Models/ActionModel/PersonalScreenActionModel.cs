using System;
using Unity.UIWidgets.async;

namespace ConnectApp.Models.ActionModel {
    public class PersonalScreenActionModel : BaseActionModel {
        public Action updateNotifications;
    }

    public class WritingCenterScreenActionModel : BaseActionModel {
        // public Action<string, string> pushToAnswerDraft;
        public Func<int, Future> fetchUserQuestions;
        public Func<int, Future> fetchUserAnswers;
    }

    public class DraftBoxScreenActionModel : BaseActionModel {
        // public Action<string, string> pushToAnswerDraft;
        public Func<int, Future> fetchAnswerDrafts;
        public Func<string, string, Future> deleteAnswerDraft;
    }
}