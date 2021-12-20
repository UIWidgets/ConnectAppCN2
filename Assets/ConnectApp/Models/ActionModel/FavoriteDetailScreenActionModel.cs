using System;
using ConnectApp.Components;
using Unity.UIWidgets.async;

namespace ConnectApp.Models.ActionModel {
    public class FavoriteDetailScreenActionModel : BaseActionModel {
        public Action startFetchFavoriteDetail;
        public Func<string, int, Future> fetchFavoriteDetail;
        public Func<string, string, Future> unFavoriteArticle;
        public Action<string> blockArticleAction;
        public Func<string, string, Future> collectFavoriteTag;
        public Func<string, string, Future> cancelCollectFavoriteTag;
    }
}