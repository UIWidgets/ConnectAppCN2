using System;
using Unity.UIWidgets.async;

namespace ConnectApp.Models.ActionModel {
    public class TeamDetailScreenActionModel : BaseActionModel {
        public Action<string> blockArticleAction;
        public Action startFetchTeam;
        public Func<Future> fetchTeam;
        public Action startFetchTeamArticle;
        public Func<int, Future> fetchTeamArticle;
        public Action startFollowTeam;
        public Func<string, Future> followTeam;
        public Action startUnFollowTeam;
        public Func<string, Future> unFollowTeam;
    }
}