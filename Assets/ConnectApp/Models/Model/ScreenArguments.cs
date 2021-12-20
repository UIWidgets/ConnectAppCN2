using System;
using System.Collections.Generic;
using ConnectApp.Models.State;
using ConnectApp.screens;

namespace ConnectApp.Models.Model {
    [Serializable]
    public class ScreenArguments {
        public string id;
        public bool isModal;
    }

    [Serializable]
    public class GameDetailScreenArguments : ScreenArguments {
        public bool isCloud;
    }

    [Serializable]
    public class ArticleDetailScreenArguments : ScreenArguments {
        public bool isPush;
    }
    
    [Serializable]
    public class LeaderBoardScreenArguments : ScreenArguments {
        public int initIndex;
    }
    
    [Serializable]
    public class LeaderBoardDetailScreenArguments : ScreenArguments {
        public LeaderBoardType type = LeaderBoardType.collection;
    }

    [Serializable]
    public class PostQuestionScreenArguments : ScreenArguments {
        public new bool isModal;
    }
    
    [Serializable]
    public class PostAnswerScreenArguments : ScreenArguments {
        public string questionId;
        public string answerId;
        public bool canSave;
    }
    
    [Serializable]
    public class QACommentDetailScreenArguments : ScreenArguments {
        public string channelId;
        public string messageId;
    }
    
    [Serializable]
    public class AnswerDetailScreenArguments : ScreenArguments {
        public string questionId;
        public string answerId;
    }

    [Serializable]
    public class UserFollowingScreenArguments : ScreenArguments {
        public int initialPage;
    }

    [Serializable]
    public class UserDetailScreenArguments : ScreenArguments {
        public bool isSlug;
    }
    
    [Serializable]
    public class FavoriteDetailScreenArguments : ScreenArguments {
        public string tagId = "";
        public string userId = "";
        public FavoriteType type;
    }

    [Serializable]
    public class TeamDetailScreenArguments : ScreenArguments {
        public bool isSlug;
    }
    
    [Serializable]
    public class ReportScreenArguments : ScreenArguments {
        public ReportType reportType;
    }
    
    [Serializable]
    public class SearchScreenArguments : ScreenArguments {
        public SearchType searchType = SearchType.article;
        public string keyword = "";
    }
    
    [Serializable]
    public class QRScanLoginScreenArguments : ScreenArguments {
        public string token = "";
    }
    
    [Serializable]
    public class PhotoViewScreenArguments : ScreenArguments {
        public string url;
        public List<string> urls;
        public bool useCachedNetworkImage = true;
        public Dictionary<string, byte[]> imageData;
    }

    [Serializable]
    public class VideoPlayerScreenArguments : ScreenArguments {
        public string url;
        public string verifyType = "empty";
        public int limitSeconds;
    }
}