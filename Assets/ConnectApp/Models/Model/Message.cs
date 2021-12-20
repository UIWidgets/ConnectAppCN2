using System;
using System.Collections.Generic;

namespace ConnectApp.Models.Model {
    public enum QAMessageType {
        question,
        answer,
        message,
        other
    }
    
    [Serializable]
    public class Message {
        public string id;
        public string parentMessageId;
        public string upperMessageId;
        public string channelId;
        public User author;
        public string content;
        public string nonce;
        public bool mentionEveryone;
        public List<string> replyMessageIds;
        public List<string> lowerMessageIds;
        public List<Reaction> reactions;
        public List<User> mentions;
        public string deletedTime;
        public string type;
        public bool deleted;
        public List<Message> replies;
    }

    [Serializable]
    public class NewMessage {
        public string id;
        public string channelId;
        public string authorId;
        public string content;
        public string nonce;
        public DateTime postTime;
        public string parentMessageId;
        public string upperMessageId;
        public int likeCount;
        public int replyCount;
        public List<NewMessage> childMessages;
    }

    [Serializable]
    public class ChildNewMessage {
        public List<string> list;
    }

    [Serializable]
    public class NewMessageList {
        public List<string> list;
        public bool hasMore;
        public string lastId = "";
    }
}