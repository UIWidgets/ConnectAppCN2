using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace ConnectApp.Common.Util {
    public static class NewNotificationManager {
        const string _newNotifications = "NewNotifications";

        public static void saveNewNotification(string userId, string newNotification) {
            var dict = getNewNotifications();
            if (dict.ContainsKey(key: userId) && newNotification == null) {
                dict.Remove(key: userId);
            }
            else {
                dict[key: userId] = newNotification;
            }

            var infoStr = JsonConvert.SerializeObject(value: dict);
            PlayerPrefs.SetString(key: _newNotifications, value: infoStr);
            PlayerPrefs.Save();
        }

        public static string getNewNotification(string userId) {
            var dict = getNewNotifications();
            return dict.ContainsKey(key: userId) ? dict[key: userId] : null;
        }

        public static Dictionary<string, string> getNewNotifications() {
            if (!PlayerPrefs.HasKey(key: _newNotifications)) {
                return new Dictionary<string, string>();
            }

            return JsonConvert.DeserializeObject<Dictionary<string, string>>(
                PlayerPrefs.GetString(key: _newNotifications));
        }

        public static void clearNewNotifications() {
            if (PlayerPrefs.HasKey(key: _newNotifications)) {
                PlayerPrefs.DeleteKey(key: _newNotifications);
            }
        }
    }
}