using System;
using Unity.UIWidgets.async;

namespace ConnectApp.Models.ActionModel {
    public class BloggerScreenActionModel : BaseActionModel {
        public Action startFetchBlogger;
        public Func<int, Future> fetchBlogger;
        public Action<string> startFollowUser;
        public Func<string, Future> followUser;
        public Action<string> startUnFollowUser;
        public Func<string, Future> unFollowUser;
    }
}