using System;
using Unity.UIWidgets.async;

namespace ConnectApp.Models.ActionModel {
    public class LeaderBoardScreenActionModel : BaseActionModel {
        public Action startFetchCollection;
        public Func<int, Future> fetchCollection;
        public Action startFetchColumn;
        public Func<int, Future> fetchColumn;
        public Action startFetchBlogger;
        public Func<int, Future> fetchBlogger;
    }
}