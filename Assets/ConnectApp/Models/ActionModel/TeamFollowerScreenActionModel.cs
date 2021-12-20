using System;
using Unity.UIWidgets.async;

namespace ConnectApp.Models.ActionModel {
    public class TeamFollowerScreenActionModel : BaseActionModel {
        public Action startFetchFollower;
        public Func<int, Future> fetchFollower;
        public Action<string> startFollowUser;
        public Func<string, Future> followUser;
        public Action<string> startUnFollowUser;
        public Func<string, Future> unFollowUser;
    }
}