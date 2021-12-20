using System;
using UnityEngine;

namespace ConnectApp.Common.Util {
    public static class LocalDataManager {
        const string _leaderBoardUpdatedTimeKey = "leaderBoardUpdatedTimeKey";
        const string _fpsLabelStatusKey = "fpsLabelStatusKey";
        const string _hiddenWeixinLoginKey = "hiddenWeixinLoginKey";
        const string _hiddenWeixinLoginIOSKey = "hiddenWeixinLoginIOSKey";
        const string _hiddenWeixinLoginAndroidKey = "hiddenWeixinLoginAndroidKey";
        const string _hiddenRegisterKey = "hiddenRegisterKey";
        const string _agreeTermsTimeKey = "agreeTermsTimeKey";

        public static void setFPSLabelStatus(bool isOpen) {
            PlayerPrefs.SetInt(key: _fpsLabelStatusKey, Convert.ToInt32(value: isOpen));
            PlayerPrefs.Save();
        }

        public static bool getFPSLabelStatus() {
            if (PlayerPrefs.HasKey(key: _fpsLabelStatusKey)) {
                var isOpen = PlayerPrefs.GetInt(key: _fpsLabelStatusKey);
                return Convert.ToBoolean(value: isOpen);
            }

            setFPSLabelStatus(false);
            return false;
        }

        public static bool hiddenWeixinLoginIOS() {
            return PlayerPrefs.GetInt(key: _hiddenWeixinLoginIOSKey) == 1;
        }

        public static void setHiddenWeixinLoginIOS(bool hiddenWexin) {
            PlayerPrefs.SetInt(key: _hiddenWeixinLoginIOSKey, hiddenWexin ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static bool hiddenWeixinLoginAndroid() {
            return PlayerPrefs.GetInt(key: _hiddenWeixinLoginAndroidKey) == 1;
        }

        public static void setHiddenWeixinLoginAndroid(bool hiddenWexin) {
            PlayerPrefs.SetInt(key: _hiddenWeixinLoginAndroidKey, hiddenWexin ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static bool hiddenRegisterButton() {
            return PlayerPrefs.GetInt(key: _hiddenRegisterKey) == 1;
        }

        public static void setHiddenRegisterButton(bool hiddenRegister) {
            PlayerPrefs.SetInt(key: _hiddenRegisterKey, hiddenRegister ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void markLeaderBoardUpdatedTime() {
            PlayerPrefs.SetString(key: _leaderBoardUpdatedTimeKey, DateTime.UtcNow.ToString("yyyy/MM/dd HH:mm:ss"));
            PlayerPrefs.Save();
        }

        public static bool needNoticeNewLeaderBoard(DateTime dateTime) {
            if (!PlayerPrefs.HasKey(key: _leaderBoardUpdatedTimeKey)) {
                // first check need notice
                return true;
            }

            var timeString = PlayerPrefs.GetString(key: _leaderBoardUpdatedTimeKey);
            DateTime.TryParse(s: timeString, out var newTime);
            return DateTime.Compare(t1: newTime, t2: dateTime) <= 0;
        }

        public static string agreeTermsTime() {
            return PlayerPrefs.HasKey(key: _agreeTermsTimeKey) ? PlayerPrefs.GetString(key: _agreeTermsTimeKey) : "";
        }

        public static void setAgreeTermsTime(string agreeTermsTime) {
            PlayerPrefs.SetString(key: _agreeTermsTimeKey, value: agreeTermsTime);
            PlayerPrefs.Save();
        }
    }
}