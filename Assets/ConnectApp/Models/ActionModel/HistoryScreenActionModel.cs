using System;

namespace ConnectApp.Models.ActionModel {
    public class HistoryScreenActionModel : BaseActionModel {
        public Action<string> pushToBlock;
        public Action<string> deleteArticleHistory;
        public Action deleteAllArticleHistory;
        public Action deleteAllEventHistory;
    }
}