using System;

namespace ConnectApp.Models.Model {
    [Serializable]
    public class Tag {
        public string id;
        public string name;
    }

    [Serializable]
    public class TagCard {
        public int index;
        public string name;
        public string icon;
        public string bg;
    }
}