using System.Collections.Generic;
using System.Linq;
using ConnectApp.Api;
using ConnectApp.Models.Api;
using ConnectApp.Models.Model;
using ConnectApp.Models.State;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.Redux;

namespace ConnectApp.redux.actions {
    public class StartFetchNotificationsAction : BaseAction {
        public string category;
    }

    public class FetchNotificationsSuccessAction : BaseAction {
        public string category;
        public int page;
        public int pageNumber;
        public int pageTotal;
        public List<Notification> notifications;
        public List<User> mentions;
    }

    public class FetchNotificationsFailureAction : BaseAction {
        public string category;
    }

    public static partial class CActions {
        public static object fetchNotifications(int pageNumber, string category) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return NotificationApi.FetchNotificationsByCategory(pageNumber: pageNumber, category: category)
                    .then(data => {
                        if (!(data is FetchNotificationResponse notificationResponse)) {
                            return;
                        }

                        var results = notificationResponse.results;
                        if (results != null && results.Count > 0) {
                            var userMap = notificationResponse.userMap;
                            var teamMap = new Dictionary<string, Team>();
                            results.ForEach(item => {
                                var itemData = item.data;
                                if (itemData.userId.isNotEmpty()) {
                                    var user = new User {
                                        id = itemData.userId,
                                        fullName = itemData.fullname,
                                        avatar = itemData.avatarWithCDN
                                    };
                                    if (userMap.ContainsKey(key: itemData.userId)) {
                                        userMap[key: itemData.userId] = user;
                                    }
                                    else {
                                        userMap.Add(key: itemData.userId, value: user);
                                    }
                                }

                                if (itemData.teamId.isNotEmpty()) {
                                    var team = new Team {
                                        id = itemData.teamId,
                                        name = itemData.teamName,
                                        avatar = itemData.teamAvatarWithCDN ?? ""
                                    };
                                    if (teamMap.ContainsKey(key: itemData.teamId)) {
                                        teamMap[key: itemData.teamId] = team;
                                    }
                                    else {
                                        teamMap.Add(key: itemData.teamId, value: team);
                                    }
                                }
                            });
                            dispatcher.dispatch(new UserMapAction {userMap = userMap});
                            dispatcher.dispatch(new TeamMapAction {teamMap = teamMap});
                        }

                        dispatcher.dispatch(new FetchNotificationsSuccessAction {
                            category = category,
                            page = notificationResponse.page,
                            pageNumber = pageNumber,
                            pageTotal = notificationResponse.pageTotal,
                            notifications = results,
                            mentions = notificationResponse.userMap.Values.ToList()
                        });
                    })
                    .catchError(err => {
                        dispatcher.dispatch(new FetchNotificationsFailureAction { category = category });
                    });
            });
        }

        public static object fetchMakeAllSeen() {
            return new ThunkAction<AppState>((dispatcher, getState) => NotificationApi.FetchMakeAllSeen());
        }
    }
}