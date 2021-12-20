using System;

namespace ConnectApp.Models.Model {
    [Serializable]
    public class OriginalImage {
        public string url;
        public float width;
        public float height;
        public bool gif;
    }

    [Serializable]
    public class ThumbnailImage {
        public string url;
        public float width;
        public float height;
        public bool gif;
    }
}