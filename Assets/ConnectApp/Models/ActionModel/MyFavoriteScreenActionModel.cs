using System;
using System.Collections.Generic;
using Unity.UIWidgets.async;

namespace ConnectApp.Models.ActionModel {
    public class MyFavoriteScreenActionModel : BaseActionModel {
        public Action startFetchMyFavorite;
        public Func<int, Future> fetchMyFavorite;
        public Action startFetchFollowFavorite;
        public Func<int, Future> fetchFollowFavorite;
        public Func<List<string>, Future> favoriteArticle;
        public Func<string, Future> deleteFavoriteTag;
    }
}