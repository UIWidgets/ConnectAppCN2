using System;
using System.Collections.Generic;
using ConnectApp.Models.Model;

namespace ConnectApp.Models.State {
    [Serializable]
    public class LearnState {
        public List<LearnCourse> courses;
        public bool hasMore;
    }
}