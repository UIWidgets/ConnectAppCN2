using System;
using System.Collections.Generic;
using ConnectApp.Api;
using ConnectApp.Common.Util;
using ConnectApp.Models.Model;
using ConnectApp.Models.State;
using Unity.UIWidgets.async;
using Unity.UIWidgets.Redux;

namespace ConnectApp.redux.actions {
    public class LoginChangeEmailAction : BaseAction {
        public string changeText;
    }

    public class LoginChangePasswordAction : BaseAction {
        public string changeText;
    }

    public class LoginByEmailSuccessAction : BaseAction {
        public LoginInfo loginInfo;
    }

    public class LoginByWechatSuccessAction : BaseAction {
        public LoginInfo loginInfo;
    }
    
    public class LogoutAction : BaseAction {
    }

    public class CleanEmailAndPasswordAction : BaseAction {
    }

    public static partial class CActions {
        public static object loginByEmail() {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                var email = getState().loginState.email;
                var password = getState().loginState.password;
                return LoginApi.LoginByEmail(email: email, password: password)
                    .then(data => {
                        if (!(data is LoginInfo loginInfo)) {
                            return;
                        }
                        var user = new User {
                            id = loginInfo.userId,
                            fullName = loginInfo.userFullName,
                            avatar = loginInfo.userAvatar,
                            title = loginInfo.title,
                            coverImage = loginInfo.coverImageWithCDN
                        };
                        var dict = new Dictionary<string, User> {
                            {user.id, user}
                        };
                        dispatcher.dispatch(new UserMapAction {userMap = dict});
                        dispatcher.dispatch(new LoginByEmailSuccessAction {
                            loginInfo = loginInfo
                        });
                        dispatcher.dispatch<Future>(fetchUserProfile(userId: loginInfo.userId));
                        dispatcher.dispatch(new CleanEmailAndPasswordAction());
                        UserInfoManager.saveUserInfo(loginInfo: loginInfo);
                        AnalyticsManager.AnalyticsLogin("email");
                        EventBus.publish(sName: EventBusConstant.login_success, new List<object> {loginInfo.userId});
                    });
            });
        }

        public static object loginByWechat(string code, Action<bool> action) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return LoginApi.LoginByWechat(code: code)
                    .then(data => {
                        if (!(data is LoginInfo loginInfo)) {
                            return;
                        }

                        var user = new User {
                            id = loginInfo.userId,
                            fullName = loginInfo.userFullName,
                            avatar = loginInfo.userAvatar,
                            title = loginInfo.title,
                            coverImage = loginInfo.coverImageWithCDN
                        };
                        var dict = new Dictionary<string, User> {
                            {user.id, user}
                        };
                        dispatcher.dispatch(new UserMapAction {userMap = dict});
                        dispatcher.dispatch(new LoginByWechatSuccessAction {
                            loginInfo = loginInfo
                        });
                        dispatcher.dispatch<Future>(fetchUserProfile(userId: loginInfo.userId));
                        UserInfoManager.saveUserInfo(loginInfo: loginInfo);
                        AnalyticsManager.AnalyticsLogin("wechat");
                        EventBus.publish(sName: EventBusConstant.login_success,
                            new List<object> {loginInfo.userId});
                        action?.Invoke(loginInfo.anonymous);
                    });
            });
        }

        public static object loginByQr(string token, string action) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return LoginApi.LoginByQr(token: token, action: action).then(data => {
                    if (action == "cancel") {
                        AnalyticsManager.AnalyticsQRScan(state: QRState.cancel);
                        return;
                    }
                    if (!(data is bool success)) {
                        return;
                    }
                    if (success) {
                        AnalyticsManager.AnalyticsQRScan(state: QRState.confirm);
                    }
                    else {
                        AnalyticsManager.AnalyticsQRScan(state: QRState.confirm, false);
                    }
                });
            });
        }

        public static object openCreateUnityIdUrl() {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return LoginApi.FetchCreateUnityIdUrl()
                    .then(data => {
                        if (!(data is string url)) {
                            return;
                        }
                        dispatcher.dispatch(new UtilsAction.OpenUrlAction {url = url});
                    });
            });
        }
    }
}