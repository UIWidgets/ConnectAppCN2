using System.Collections.Generic;
using ConnectApp.Api;
using ConnectApp.Models.Api;
using ConnectApp.Models.Model;
using ConnectApp.Models.State;
using ConnectApp.screens;
using Unity.UIWidgets.Redux;

namespace ConnectApp.redux.actions {
    public class RankListAction : BaseAction {
        public List<RankData> rankList;
    }

    public class StartFetchLeaderBoardCollectionAction : RequestAction {
    }

    public class FetchLeaderBoardCollectionSuccessAction : BaseAction {
        public List<string> collectionIds;
        public bool hasMore;
        public int pageNumber;
    }

    public class FetchLeaderBoardCollectionFailureAction : BaseAction {
    }

    public class StartFetchLeaderBoardColumnAction : RequestAction {
    }

    public class FetchLeaderBoardColumnSuccessAction : BaseAction {
        public List<string> columnIds;
        public Dictionary<string, UserArticle> userArticleMap;
        public bool hasMore;
        public int pageNumber;
    }

    public class FetchLeaderBoardColumnFailureAction : BaseAction {
    }

    public class StartFetchLeaderBoardBloggerAction : RequestAction {
    }

    public class FetchLeaderBoardBloggerSuccessAction : BaseAction {
        public List<string> bloggerIds;
        public bool hasMore;
        public int pageNumber;
    }

    public class FetchLeaderBoardBloggerFailureAction : BaseAction {
    }

    public class StartFetchHomeBloggerAction : RequestAction {
    }

    public class FetchHomeBloggerSuccessAction : BaseAction {
        public List<string> bloggerIds;
        public bool hasMore;
        public int pageNumber;
    }

    public class FetchHomeBloggerFailureAction : BaseAction {
    }
    
    public class StartFetchLeaderBoardDetailAction : RequestAction {
    }

    public class FetchLeaderBoardDetailSuccessAction : BaseAction {
        public LeaderBoardType type;
        public string albumId;
        public List<string> articleList;
        public bool hasMore;
        public int pageNumber;
    }

    public class FetchLeaderBoardDetailFailureAction : BaseAction {
    }

    public static partial class CActions {
        public static object fetchLeaderBoardCollection(int page) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return LeaderBoardApi.FetchLeaderBoardCollection(page: page)
                    .then(data => {
                        if (!(data is FetchLeaderBoardCollectionResponse collectionResponse)) {
                            return;
                        }
                        dispatcher.dispatch(new RankListAction {rankList = collectionResponse.rankList});
                        var collectionIds = new List<string>();
                        collectionResponse.rankList.ForEach(rankData => { collectionIds.Add(item: rankData.id); });
                        dispatcher.dispatch(new FetchLeaderBoardCollectionSuccessAction {
                            collectionIds = collectionIds,
                            hasMore = collectionResponse.hasMore,
                            pageNumber = page
                        });

                        dispatcher.dispatch(new FavoriteTagMapAction {
                            favoriteTagMap = collectionResponse.favoriteTagMap
                        });
                        dispatcher.dispatch(new FavoriteTagArticleMapAction {
                            favoriteTagArticleMap = collectionResponse.favoriteTagArticleMap
                        });
                        dispatcher.dispatch(new CollectedTagMapAction {
                            collectedTagMap = collectionResponse.collectedTagMap
                        });
                    })
                    .catchError(error => {
                        dispatcher.dispatch(new FetchLeaderBoardCollectionFailureAction());
                        Debuger.LogError(message: error);
                    });
            });
        }

        public static object fetchLeaderBoardColumn(int page) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return LeaderBoardApi.FetchLeaderBoardColumn(page: page)
                    .then(data => {
                        if (!(data is FetchLeaderBoardColumnResponse columnResponse)) {
                            return;
                        }
                        dispatcher.dispatch(new RankListAction {rankList = columnResponse.rankList});
                        dispatcher.dispatch(new UserMapAction {userMap = columnResponse.userSimpleV2Map});
                        var columnIds = new List<string>();
                        columnResponse.rankList.ForEach(rankData => { columnIds.Add(item: rankData.id); });
                        dispatcher.dispatch(new FetchLeaderBoardColumnSuccessAction {
                            columnIds = columnIds,
                            userArticleMap = columnResponse.userArticleMap,
                            hasMore = columnResponse.hasMore,
                            pageNumber = page
                        });
                    })
                    .catchError(error => {
                        dispatcher.dispatch(new FetchLeaderBoardColumnFailureAction());
                        Debuger.LogError(message: error);
                    });
            });
        }

        public static object fetchLeaderBoardBlogger(int page) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return LeaderBoardApi.FetchLeaderBoardBlogger(page: page)
                    .then(data => {
                        if (!(data is FetchBloggerResponse bloggerResponse)) {
                            return;
                        }
                        dispatcher.dispatch(new RankListAction {rankList = bloggerResponse.rankList});
                        dispatcher.dispatch(new UserMapAction {userMap = bloggerResponse.userFullMap});
                        dispatcher.dispatch(new FollowMapAction {followMap = bloggerResponse.followMap});
                        dispatcher.dispatch(new UserLicenseMapAction {userLicenseMap = bloggerResponse.userLicenseMap});
                        var bloggerIds = new List<string>();
                        bloggerResponse.rankList.ForEach(rankData => { bloggerIds.Add(item: rankData.itemId); });
                        dispatcher.dispatch(new FetchLeaderBoardBloggerSuccessAction {
                            bloggerIds = bloggerIds,
                            hasMore = bloggerResponse.hasMore,
                            pageNumber = page
                        });
                    })
                    .catchError(error => {
                        dispatcher.dispatch(new FetchLeaderBoardBloggerFailureAction());
                        Debuger.LogError(message: error);
                    });
            });
        }

        public static object fetchHomeBlogger(int page) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return LeaderBoardApi.FetchHomeBlogger(page: page)
                    .then(data => {
                        if (!(data is FetchBloggerResponse bloggerResponse)) {
                            return;
                        }
                        dispatcher.dispatch(new UserMapAction {userMap = bloggerResponse.userFullMap});
                        dispatcher.dispatch(new FollowMapAction {followMap = bloggerResponse.followMap});
                        dispatcher.dispatch(new UserLicenseMapAction {userLicenseMap = bloggerResponse.userLicenseMap});
                        var bloggerIds = new List<string>();
                        bloggerResponse.rankList.ForEach(rankData => { bloggerIds.Add(item: rankData.itemId); });
                        dispatcher.dispatch(new FetchHomeBloggerSuccessAction {
                            bloggerIds = bloggerIds,
                            hasMore = bloggerResponse.hasMore,
                            pageNumber = page
                        });
                    })
                    .catchError(error => {
                        dispatcher.dispatch(new FetchHomeBloggerFailureAction());
                        Debuger.LogError(message: error);
                    });
            });
        }

        public static object fetchLeaderBoardDetail(string tagId, int page, LeaderBoardType type) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return LeaderBoardApi.FetchLeaderBoardDetail(tagId: tagId, page: page, leaderBoardType: type)
                    .then(data => {
                        if (!(data is FetchLeaderBoardDetailResponse detailResponse)) {
                            return;
                        }
                        
                        dispatcher.dispatch(new UserMapAction {userMap = detailResponse.userSimpleV2Map});
                        dispatcher.dispatch(new TeamMapAction {teamMap = detailResponse.teamSimpleMap});

                        var articleIds = new List<string>();
                        var articleDict = new Dictionary<string, Article>();
                        detailResponse.projectSimples.ForEach(project => {
                            articleIds.Add(item: project.id);
                            articleDict.Add(key: project.id, value: project);
                        });
                        dispatcher.dispatch(new ArticleMapAction {articleMap = articleDict});

                        dispatcher.dispatch(new FetchLeaderBoardDetailSuccessAction {
                            type = type,
                            albumId = tagId,
                            articleList = articleIds,
                            hasMore = detailResponse.hasMore,
                            pageNumber = page
                        });

                        dispatcher.dispatch(new FavoriteTagMapAction {
                            favoriteTagMap = detailResponse.favoriteTagMap
                        });
                        dispatcher.dispatch(new FavoriteTagArticleMapAction {
                            favoriteTagArticleMap = detailResponse.favoriteTagArticleMap
                        });
                        dispatcher.dispatch(new CollectedTagMapAction {
                            collectedTagMap = detailResponse.collectedTagMap
                        });
                        if (type == LeaderBoardType.collection) {
                            if (detailResponse.myFavoriteTag != null) {
                                detailResponse.rankData.myFavoriteTagId = detailResponse.myFavoriteTag.id;
                            }

                            dispatcher.dispatch(new RankListAction {
                                rankList = new List<RankData> {detailResponse.rankData}
                            });
                        }
                    })
                    .catchError(error => {
                        dispatcher.dispatch(new FetchLeaderBoardDetailFailureAction());
                        Debuger.LogError(message: error);
                    });
            });
        }
    }
}