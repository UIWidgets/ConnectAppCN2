using System;
using Unity.UIWidgets.async;

namespace ConnectApp.Models.ActionModel {
    public class LearnScreenActionModel : BaseActionModel {
        public Func<int, Future> fetchLearnCourses;
    }
}