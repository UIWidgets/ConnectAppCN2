using System;
using Unity.UIWidgets.async;

namespace ConnectApp.Models.ActionModel {
    public class TeamMemberScreenActionModel : BaseActionModel {
        public Action startFetchMember;
        public Func<int, Future> fetchMember;
        public Action<string> startFollowUser;
        public Func<string, Future> followUser;
        public Action<string> startUnFollowUser;
        public Func<string, Future> unFollowUser;
    }
}