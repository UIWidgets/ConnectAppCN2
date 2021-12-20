using System;
using Unity.UIWidgets.async;

namespace ConnectApp.Models.ActionModel {
    public class UserFollowingScreenActionModel : BaseActionModel  {
        public Action startFetchFollowingUser;
        public Action startFetchFollowingTeam;
        public Func<int, Future> fetchFollowingUser;
        public Func<int, Future> fetchFollowingTeam;
        public Action<string> startFollowUser;
        public Func<string, Future> followUser;
        public Action<string> startUnFollowUser;
        public Func<string, Future> unFollowUser;
        public Action<string> startFollowTeam;
        public Func<string, Future> followTeam;
        public Action<string> startUnFollowTeam;
        public Func<string, Future> unFollowTeam;
        public Action startSearchFollowingUser;
        public Func<string, int, Future> searchFollowingUser;
        public Action clearSearchFollowingResult;
    }
}