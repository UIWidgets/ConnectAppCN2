using System;

namespace ConnectApp.Models.Model {
    public enum QALikeType {
        question,
        answer,
        message
    }

    [Serializable]
    public class QALike {
        public string id;
        public string userId;
        public string likeType; // question, answer,  message
        public string itemId;
        public DateTime createdTime;
    }
}