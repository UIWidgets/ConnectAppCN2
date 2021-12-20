using System.Collections.Generic;
using ConnectApp.Models.Model;

namespace ConnectApp.Models.ViewModel {
    public class PersonalScreenViewModel {
        public bool isLoggedIn;
        public LoginInfo user;
        public Dictionary<string, User> userDict;
        public Dictionary<string, UserLicense> userLicenseDict;
        public int currentTabBarIndex;
        public bool hasUnreadNotifications;
    }

    public class WritingCenterScreenViewModel {
        public bool isMe;
        public User currentUser;
        public bool questionListHasMore;
        public bool answerListHasMore;
        public List<Question> questions;
        public List<Answer> answers;
        public Dictionary<string, Question> questionDict;
    }

    public class DraftBoxScreenViewModel {
        public bool answerDraftsHasMore;
        public List<AnswerDraft> answerDrafts;
    }
}