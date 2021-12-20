using System;
using System.Collections.Generic;
using ConnectApp.Models.Model;

namespace ConnectApp.Models.State {
    [Serializable]
    public class QAState {
        public List<string> latestQuestionIds { get; set; }
        public bool latestQuestionListHasMore { get; set; }
        public List<string> hotQuestionIds { get; set; }
        public bool hotQuestionListHasMore { get; set; }
        public List<string> pendingQuestionIds { get; set; }
        public bool pendingQuestionListHasMore { get; set; }
        public Dictionary<string, List<string>> userQuestionsDict { get; set; }
        public bool userQuestionListHasMore { get; set; }
        public Dictionary<string, List<string>> userAnswersDict { get; set; }
        public bool userAnswerListHasMore { get; set; }
        public Dictionary<string, Question> questionDict { get; set; }
        public Dictionary<string, bool> answerListHasMoreDict { get; set; }
        public Dictionary<string, List<string>> answerIdsDict { get; set; }
        public Dictionary<string, Answer> answerDict { get; set; }
        public Dictionary<string, string> imageDict { get; set; }
        // key = channelId
        public Dictionary<string, List<string>> messageToplevelSimpleListDict { get; set; }
        // key = channelId
        public Dictionary<string, NewMessageList> messageToplevelListDict { get; set; }
        // key = messageId
        public Dictionary<string, NewMessageList> messageSecondLevelSimpleListDict { get; set; }
        // key = messageId
        public Dictionary<string, NewMessageList> messageSecondLevelListDict { get; set; }
        public Dictionary<string, NewMessage> messageDict { get; set; }
        // key = itemId (questionId or answerId or messageId)
        public Dictionary<string, QALike> likeDict { get; set; }
        public Dictionary<string, string> nextAnswerIdDict { get; set; }
        public List<string> blockQuestionList { get; set; }
        public List<string> blockAnswerList { get; set; }
    }
}