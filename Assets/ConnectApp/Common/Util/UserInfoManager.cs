using System;
using System.Collections.Generic;
using ConnectApp.Models.Model;
using ConnectApp.Plugins;
using ConnectApp.redux;
using ConnectApp.redux.actions;
using Newtonsoft.Json;
using Unity.UIWidgets.foundation;
using UnityEngine;

namespace ConnectApp.Common.Util {
    public static class UserInfoManager {
        const string UserInfoKey = "UserInfo";

        public static void saveUserInfo(LoginInfo loginInfo) {
            if (loginInfo == null) {
                return;
            }

            var infoStr = JsonConvert.SerializeObject(value: loginInfo);
            PlayerPrefs.SetString(key: UserInfoKey, value: infoStr);
            PlayerPrefs.Save();
        }

        public static LoginInfo getUserInfo() {
            var info = PlayerPrefs.GetString(key: UserInfoKey);
            if (info.isNotEmpty()) {
                try {
                    var loginInfo = JsonConvert.DeserializeObject<LoginInfo>(value: info);
                    return loginInfo;
                }
                catch (Exception) {
                    saveUserInfo(new LoginInfo());
                    StoreProvider.store.dispatcher.dispatch(new LogoutAction());
                }
            }

            return new LoginInfo();
        }

        public static bool isLoggedIn() {
            var info = PlayerPrefs.GetString(key: UserInfoKey);
            return info.isNotEmpty();
        }

        public static bool isRealName() {
            var info = PlayerPrefs.GetString(key: UserInfoKey);
            if (info.isEmpty()) {
                return false;
            }

            var loginInfo = JsonConvert.DeserializeObject<LoginInfo>(value: info);
            return loginInfo.isRealName;
        }

        public static void passRealName() {
            var info = PlayerPrefs.GetString(key: UserInfoKey);
            if (info.isEmpty()) {
                return;
            }

            var loginInfo = JsonConvert.DeserializeObject<LoginInfo>(value: info);
            loginInfo.isRealName = true;
            saveUserInfo(loginInfo: loginInfo);
        }

        public static Dictionary<string, User> getUserInfoDict() {
            var info = getUserInfo();
            if (info.userId.isEmpty()) {
                return new Dictionary<string, User>();
            }

            var user = new User {
                fullName = info.userFullName,
                id = info.userId,
                avatar = info.userAvatar,
                title = info.title,
                coverImage = info.coverImageWithCDN
            };
            return new Dictionary<string, User> {{user.id, user}};
        }

        public static void clearUserInfo() {
            // 取消注册 huawei 推送 Token
            CommonPlugin.registerHmsToken(userId: getUserInfo().userId);
            if (PlayerPrefs.HasKey(key: UserInfoKey)) {
                PlayerPrefs.DeleteKey(key: UserInfoKey);
            }
        }

        public static bool isMe(string userId) {
            return userId == getUserInfo().userId;
        }
    }
}