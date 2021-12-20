using System;
using System.Collections.Generic;

namespace ConnectApp.Models.Model {
    [Serializable]
    public class RankData {
        public string id;
        public string rankType;
        public string itemId;
        public int rank;
        public DateTime? createdTime;
        public DateTime? updatedTime;
        public string resetTitle;
        public string resetLabel;
        public string resetSubLabel;
        public string resetDesc;
        public string redirectURL;
        public string image;
        public string myFavoriteTagId;
        public List<string> attachmentURLs;
    }

    [Serializable]
    public class GameData {
        public string id;
        public string rankType;
        public string itemId;
        public string gameType;
        public string fileSize;
        public string fileMD5;
        public bool share;
        public bool canPlay;
        public int rank;
        public DateTime? createdTime;
        public DateTime? updatedTime;
        public string resetTitle;
        public string resetLabel;
        public string resetSubLabel;
        public string resetDesc;
        public string redirectURL;
        public string image;
        public string myFavoriteTagId;

        public List<string> attachmentURLs;

        // for tiny cloud game
        public int buildNumber;
        public string tinyVersion;
        public string author;
    }
}