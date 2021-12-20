using System;

namespace ConnectApp.Models.Model {
    [Serializable]
    public class LearnCourse {
        public string id;
        public string skillLevel;
        public string thumbnail;
        public string language;
        public string duration; // minutes
        public string title; // minutes
    }
}