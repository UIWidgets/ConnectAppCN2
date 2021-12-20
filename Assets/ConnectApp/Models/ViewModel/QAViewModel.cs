using System.Collections.Generic;
using ConnectApp.Models.Model;

namespace ConnectApp.Models.ViewModel {
    public class QAScreenViewModel {
        public List<Question> latestQuestions;
        public List<Question> hotQuestions;
        public List<Question> pendingQuestions;
        public Dictionary<string, Tag> tagDict;
        public bool latestQuestionListHasMore;
        public bool hotQuestionListHasMore;
        public bool pendingQuestionListHasMore;
    }

    public class QuestionDetailScreenViewModel {
        public string questionId;
        public User author;
        public Question question;
        public bool answerHasMore;
        public bool isLoggedIn;
        public bool canAnswer;
        public bool canEdit;
        public List<Answer> answers;
        public Dictionary<string, Tag> tagDict;
        public Dictionary<string, User> userDict;
        public Dictionary<string, string> imageDict;
        public Dictionary<string, QALike> likeDict;
        public Dictionary<string, UserLicense> userLicenseDict;
    }

    public class AnswerDetailScreenViewModel {
        public string questionId;
        public string answerId;
        public string nextAnswerId;
        public User author;
        public Question question;
        public Answer answer;
        public Dictionary<string, User> userDict;
        public Dictionary<string, string> imageDict;
        public List<NewMessage> messages;
        public bool isLoggedIn;
        public bool canAnswer;
        public bool canEdit;
        public Dictionary<string, QALike> likeDict;
        public Dictionary<string, bool> followMap;
        public string currentUserId;
        public Dictionary<string, UserLicense> userLicenseDict;
    }

    public class QATopLevelCommentScreenViewModel {
        public string channelId;
        public string itemId;
        public User author;
        public List<NewMessage> messages;
        public Dictionary<string, NewMessage> messageDict;
        public Dictionary<string, User> userDict;
        public Dictionary<string, UserLicense> userLicenseDict;
        public int commentCount;
        public string lastId;
        public bool hasMore;
        public bool isLoggedIn;
        public QAMessageType messageType;
        public Dictionary<string, QALike> likeDict;
    }

    public class QACommentDetailScreenViewModel {
        public string channelId;
        public string messageId;
        public NewMessage message;
        public NewMessage topLevelMessage;
        public List<NewMessage> messages;
        public Dictionary<string, NewMessage> messageDict;
        public Dictionary<string, User> userDict;
        public Dictionary<string, UserLicense> userLicenseDict;
        public string lastId;
        public bool hasMore;
        public bool isLoggedIn;
        public Dictionary<string, QALike> likeDict;
    }

    public class PostQuestionScreenViewModel {
        public QuestionDraft questionDraft;
        public bool isExisting;
        public bool fetchQuestionLoading;
        public bool createLoading;
        public Dictionary<string, Tag> tagDict;
        public User currentUser;
        public Dictionary<string, string> uploadImageDict;
    }

    public class SelectPlateScreenViewModel {
        public Plate plate;
        public List<Plate> plates;
    }

    public class AddTagScreenViewModel {
        public QuestionDraft questionDraft;
        public List<Tag> tags;
        public List<Tag> searchTags;
    }

    public class PostAnswerScreenViewModel {
        public string questionId;
        public bool isExisting;
        public bool canSave;
        public AnswerDraft answerDraft;
        public bool createLoading;
        public bool fetchLoading;
        public Dictionary<string, Tag> tagDict;
        public User currentUser;
        public Dictionary<string, string> uploadImageDict;
    }
}