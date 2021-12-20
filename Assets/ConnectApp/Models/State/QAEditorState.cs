using System;
using System.Collections.Generic;
using ConnectApp.Models.Model;

namespace ConnectApp.Models.State {
    [Serializable]
    public class QAEditorState {
        public bool createQuestionDraftLoading { get; set; }
        public bool fetchQuestionDraftLoading { get; set; }
        public QuestionDraft questionDraft { get; set; }
        // key: questionId
        public Dictionary<string, AnswerDraft> answerDraftDict { get; set; }
        public AnswerDraft answerDraft { get; set; }
        public bool createAnswerDraftLoading { get; set; }
        public bool fetchAnswerDraftLoading { get; set; }
        public Dictionary<string, bool> canAnswerDict { get; set; }
        public List<string> userAnswerDraftIds { get; set; }
        public bool userAnswerDraftListHasMore { get; set; }
        public Dictionary<string, string> uploadImageDict { get; set; }
        public List<Plate> plates { get; set; }
    }
}