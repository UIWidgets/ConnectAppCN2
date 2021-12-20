using System;
using System.Collections.Generic;

namespace ConnectApp.Models.Model {
    [Serializable]
    public class DetailContent {
        public List<DetailContentBlock> blocks;
        public Dictionary<string, DetailContentEntity> entityMap;
    }

    [Serializable]
    public class DetailContentBlock {
        public string key;
        public string text;
        public string type;
        public List<InlineStyleRange> inlineStyleRanges;
        public List<EntityRange> entityRanges;
    }

    [Serializable]
    public class DetailContentEntity {
        public DetailContentEntityData data;
        public string type;
        public string mutability;
    }

    [Serializable]
    public class DetailContentEntityData {
        public string uploadId;
        public string contentId;
        public string title;
        public string url;
    }

    [Serializable]
    public class EntityRange {
        public int key;
        public int length;
        public int offset;
    }

    [Serializable]
    public class InlineStyleRange {
        public int key;
        public int offset;
        public int length;
        public string style;
    }

    /* contentMap 解析 */

    [Serializable]
    public class ContentData {
        public OriginalImage originalImage;
        public OriginalImage thumbnail;
        public string url;
        public string downloadUrl;
        public string contentType;
        public string attachmentId;
    }

    [Serializable]
    public class VideoSliceMap {
        public string id;
        public string origin;
        public string verifyType;
        public string verifyArg;
        public string status;
        public int trialSlicesCount;
        public int limitSeconds;
        public bool canWatch;
    }
}