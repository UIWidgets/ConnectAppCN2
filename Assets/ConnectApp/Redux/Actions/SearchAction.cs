using System.Collections.Generic;
using ConnectApp.Api;
using ConnectApp.Common.Util;
using ConnectApp.Models.Api;
using ConnectApp.Models.Model;
using ConnectApp.Models.State;
using Unity.UIWidgets.Redux;

namespace ConnectApp.redux.actions {
    public class PopularSearchArticleSuccessAction : RequestAction {
        public List<PopularSearch> popularSearchArticles;
    }

    public class PopularSearchUserSuccessAction : RequestAction {
        public List<PopularSearch> popularSearchUsers;
    }

    public class StartSearchArticleAction : RequestAction {
        public string keyword;
    }

    public class SearchArticleSuccessAction : BaseAction {
        public string keyword;
        public int pageNumber;
        public List<Article> searchArticles;
        public bool hasMore;
    }

    public class SearchArticleFailureAction : BaseAction {
    }

    public class ClearSearchResultAction : BaseAction {
    }

    public class ClearSearchFollowingResultAction : BaseAction {
    }

    public class SaveSearchArticleHistoryAction : BaseAction {
        public string keyword;
    }

    public class DeleteSearchArticleHistoryAction : BaseAction {
        public string keyword;
    }

    public class DeleteAllSearchArticleHistoryAction : BaseAction {
    }

    public class StartSearchUserAction : RequestAction {
        public string keyword;
    }

    public class SearchUserSuccessAction : BaseAction {
        public string keyword;
        public int pageNumber;
        public List<string> searchUserIds;
        public bool hasMore;
    }

    public class SearchUserFailureAction : BaseAction {
    }

    public class StartSearchFollowingAction : RequestAction {
    }

    public class SearchFollowingSuccessAction : BaseAction {
        public string keyword;
        public int pageNumber;
        public List<User> users;
        public bool hasMore;
    }

    public class SearchFollowingFailureAction : BaseAction {
        public string keyword;
    }

    public class StartSearchTeamAction : RequestAction {
        public string keyword;
    }

    public class SearchTeamSuccessAction : BaseAction {
        public string keyword;
        public int pageNumber;
        public List<string> searchTeamIds;
        public bool hasMore;
    }

    public class SearchTeamFailureAction : BaseAction {
    }

    public class StartSearchQuestionAction : RequestAction {
        public string keyword;
    }

    public class SearchQuestionsSuccessAction : BaseAction {
        public string keyword;
        public int page;
        public List<string> questionIds;
        public bool hasMore;
    }

    public class SearchQuestionsFailureAction : BaseAction {
    }

    public class SearchTagsSuccessAction : BaseAction {
        public string keyword;
        public List<string> tagIds;
    }

    public class SearchTagsFailureAction : BaseAction {
    }


    public static partial class CActions {
        public static object searchArticles(string keyword, int pageNumber) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return SearchApi.SearchArticle(keyword: keyword, pageNumber: pageNumber)
                    .then(data => {
                        if (!(data is FetchSearchArticleResponse searchArticleResponse)) {
                            return;
                        }
                        dispatcher.dispatch(new UserMapAction {userMap = searchArticleResponse.userMap});
                        dispatcher.dispatch(new TeamMapAction {teamMap = searchArticleResponse.teamMap});
                        // dispatcher.dispatch(new PlaceMapAction {placeMap = searchArticleResponse.placeMap});
                        dispatcher.dispatch(new LikeMapAction {likeMap = searchArticleResponse.likeMap});
                        dispatcher.dispatch(new SearchArticleSuccessAction {
                            keyword = keyword,
                            pageNumber = pageNumber,
                            searchArticles = searchArticleResponse.projects,
                            hasMore = searchArticleResponse.hasMore
                        });
                    })
                    .catchError(error => {
                        dispatcher.dispatch(new SearchArticleFailureAction());
                        Debuger.LogError(message: error);
                    });
            });
        }

        public static object popularSearchArticle() {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return SearchApi.PopularSearchArticle()
                    .then(data => {
                        if (!(data is List<PopularSearch> popularSearchArticles)) {
                            return;
                        }
                        dispatcher.dispatch(new PopularSearchArticleSuccessAction {
                            popularSearchArticles = popularSearchArticles
                        });
                    })
                    .catchError(onError: Debuger.LogError);
            });
        }

        public static object searchUsers(string keyword, int pageNumber) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return SearchApi.SearchUser(keyword: keyword, pageNumber: pageNumber)
                    .then(data => {
                        if (!(data is FetchSearchUserResponse searchUserResponse)) {
                            return;
                        }
                        dispatcher.dispatch(new FollowMapAction {followMap = searchUserResponse.followingMap});
                        var userMap = new Dictionary<string, User>();
                        var searchUserIds = new List<string>();
                        (searchUserResponse.users ?? new List<User>()).ForEach(searchUser => {
                            searchUserIds.Add(item: searchUser.id);
                            if (userMap.ContainsKey(key: searchUser.id)) {
                                userMap[key: searchUser.id] = searchUser;
                            }
                            else {
                                userMap.Add(key: searchUser.id, value: searchUser);
                            }
                        });
                        dispatcher.dispatch(new UserMapAction {userMap = userMap});
                        dispatcher.dispatch(new UserLicenseMapAction
                            {userLicenseMap = searchUserResponse.userLicenseMap});
                        dispatcher.dispatch(new SearchUserSuccessAction {
                            keyword = keyword,
                            pageNumber = pageNumber,
                            searchUserIds = searchUserIds,
                            hasMore = searchUserResponse.hasMore
                        });
                    })
                    .catchError(error => {
                        dispatcher.dispatch(new SearchUserFailureAction());
                        Debuger.LogError(message: error);
                    });
            });
        }

        public static object searchFollowings(string keyword, int pageNumber) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return SearchApi.SearchUser(keyword: keyword, pageNumber: pageNumber)
                    .then(data => {
                        if (!(data is FetchSearchUserResponse searchFollowingResponse)) {
                            return;
                        }
                        dispatcher.dispatch(new SearchFollowingSuccessAction {
                            keyword = keyword,
                            pageNumber = pageNumber,
                            users = searchFollowingResponse.users,
                            hasMore = searchFollowingResponse.hasMore
                        });
                    })
                    .catchError(error => {
                        dispatcher.dispatch(new SearchFollowingFailureAction {keyword = keyword});
                        Debuger.LogError(message: error);
                    });
            });
        }

        public static object popularSearchUser() {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return SearchApi.PopularSearchUser()
                    .then(data => {
                        if (!(data is List<PopularSearch> popularSearchUsers)) {
                            return;
                        }
                        dispatcher.dispatch(new PopularSearchUserSuccessAction {
                            popularSearchUsers = popularSearchUsers
                        });
                    })
                    .catchError(onError: Debuger.LogError);
            });
        }

        public static object searchTeams(string keyword, int pageNumber) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return SearchApi.SearchTeam(keyword: keyword, pageNumber: pageNumber)
                    .then(data => {
                        if (!(data is FetchSearchTeamResponse searchTeamResponse)) {
                            return;
                        }
                        dispatcher.dispatch(new FollowMapAction {followMap = searchTeamResponse.followingMap});
                        var teamMap = new Dictionary<string, Team>();
                        var searchTeamIds = new List<string>();
                        (searchTeamResponse.teams ?? new List<Team>()).ForEach(searchTeam => {
                            searchTeamIds.Add(item: searchTeam.id);
                            if (teamMap.ContainsKey(key: searchTeam.id)) {
                                teamMap[key: searchTeam.id] = searchTeam;
                            }
                            else {
                                teamMap.Add(key: searchTeam.id, value: searchTeam);
                            }
                        });
                        dispatcher.dispatch(new TeamMapAction {teamMap = teamMap});
                        dispatcher.dispatch(new SearchTeamSuccessAction {
                            keyword = keyword,
                            pageNumber = pageNumber,
                            searchTeamIds = searchTeamIds,
                            hasMore = searchTeamResponse.hasMore
                        });
                    })
                    .catchError(error => {
                        dispatcher.dispatch(new SearchTeamFailureAction());
                        Debuger.LogError(message: error);
                    });
            });
        }

        public static object searchQuestions(string keyword, int pageNumber) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return SearchApi.SearchQuestions(keyword: keyword, pageNumber: pageNumber)
                    .then(data => {
                        if (!(data is SearchQuestionsResponse searchResponse)) {
                            return;
                        }
                        // tag
                        dispatcher.dispatch(new TagMapAction {tagMap = searchResponse.tagSimpleMap});
                        // questions
                        var questions = searchResponse.simpleQuestions;
                        var questionIds = new List<string>();
                        if (questions.isNotNullAndEmpty()) {
                            dispatcher.dispatch(new QuestionAction {
                                questions = questions
                            });
                            questions.ForEach(question => { questionIds.Add(item: question.id); });
                        }

                        dispatcher.dispatch(new SearchQuestionsSuccessAction {
                            keyword = keyword,
                            questionIds = questionIds,
                            page = searchResponse.currentPage,
                            hasMore = searchResponse.hasMore
                        });
                    })
                    .catchError(error => { dispatcher.dispatch(new SearchQuestionsFailureAction()); });
            });
        }

        public static object searchTags(string keyword) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return SearchApi.SearchTags(keyword: keyword)
                    .then(data => {
                        if (!(data is SearchTagsResponse searchResponse)) {
                            return;
                        }
                        // tag
                        var tagIds = new List<string>();
                        var tagMap = new Dictionary<string, Tag>();
                        if (searchResponse.candidates.isNotNullAndEmpty()) {
                            var tags = searchResponse.candidates;
                            tags.ForEach(tag => {
                                tagIds.Add(item: tag.id);
                                tagMap.Add(key: tag.id, value: tag);
                            });
                            dispatcher.dispatch(new TagMapAction {tagMap = tagMap});
                        }
                        dispatcher.dispatch(new SearchTagsSuccessAction {
                            keyword = keyword,
                            tagIds = tagIds
                        });
                    });
            });
        }
    }
}