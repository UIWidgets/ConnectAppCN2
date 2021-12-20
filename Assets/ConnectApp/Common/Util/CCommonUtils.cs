using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ConnectApp.Models.Model;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Rect = Unity.UIWidgets.ui.Rect;

namespace ConnectApp.Common.Util {
    public static class CCommonUtils {
        public static float getSafeAreaTopPadding(BuildContext context) {
            return Application.platform == RuntimePlatform.IPhonePlayer
                ? MediaQuery.of(context: context).padding.top
                : 0;
        }

        public static float getSafeAreaBottomPadding(BuildContext context) {
            return Application.platform == RuntimePlatform.IPhonePlayer
                ? MediaQuery.of(context: context).padding.bottom
                : 0;
        }

        public static Rect getContextRect(this BuildContext context) {
            var renderBox = (RenderBox) context.findRenderObject();
            return renderBox.localToGlobal(point: Offset.zero) & renderBox.size;
        }

        public static float getTagTextMaxWidth(BuildContext buildContext) {
            return MediaQuery.of(context: buildContext).size.width - 16 * 2 - 28 - 10;
        }

        public static float getQuestionCardTagsWidth(BuildContext buildContext) {
            return MediaQuery.of(context: buildContext).size.width - 16 * 2;
        }

        public static float getScreenHeight(BuildContext buildContext) {
            return MediaQuery.of(context: buildContext).size.height;
        }

        public static float getScreenWidth(BuildContext buildContext) {
            return MediaQuery.of(context: buildContext).size.width;
        }

        public static float getScreenWithoutPadding16Width(BuildContext buildContext) {
            return MediaQuery.of(context: buildContext).size.width - 16 * 2;
        }

        public static bool isIPhone {
            get { return Application.platform == RuntimePlatform.IPhonePlayer; }
        }

        public static bool isAndroid {
            get { return Application.platform == RuntimePlatform.Android; }
        }

        public static string GetUserLicense(string userId, Dictionary<string, UserLicense> userLicenseMap = null) {
            if (userLicenseMap == null || !userLicenseMap.ContainsKey(key: userId)) {
                return "";
            }

            var userLicense = userLicenseMap[key: userId];
            if (userLicense != null && userId == userLicense.userId && userLicense.license.isNotEmpty()) {
                return userLicense.license;
            }

            return "";
        }

        const int MUST_BE_LESS_THAN = 10000; // 4 decimal digits

        public static int GetStableHash(string s) {
            uint hash = 0;
            // if you care this can be done much faster with unsafe 
            // using fixed char* reinterpreted as a byte*
            foreach (var b in Encoding.Unicode.GetBytes(s: s)) {
                hash += b;
                hash += (hash << 10);
                hash ^= (hash >> 6);
            }

            // final avalanche
            hash += (hash << 3);
            hash ^= (hash >> 11);
            hash += (hash << 15);
            // helpfully we only want positive integer < MUST_BE_LESS_THAN
            // so simple truncate cast is ok if not perfect
            return (int) (hash % MUST_BE_LESS_THAN);
        }

        static readonly Regex _phoneNumberPattern = new Regex(@"^[0-9]{11}$");

        // static readonly Regex _phoneNumberPattern = new Regex(@"^1(3[0-9]|5[0-9]|7[6-8]|8[0-9])[0-9]{8}$");
        public static bool isPhoneNumber(string phoneNumber) {
            return phoneNumber.isNotEmpty() && _phoneNumberPattern.Match(input: phoneNumber).Success;
        }

        static readonly Regex _IDCardPattern =
            new Regex(@"^[1-9]\d{5}(19|20|21)\d{2}((0[1-9])|(1[0-2]))(([0-2][1-9])|10|20|30|31)\d{3}[0-9Xx]$");

        public static bool isIDCard(string idCard) {
            return idCard.isNotEmpty() && _IDCardPattern.Match(input: idCard).Success;
        }

        static readonly Regex _realNamePattern = new Regex(@"^([\u4E00-\u9FA5]{2,})$");

        public static bool isRealName(string realName) {
            return realName.isNotEmpty();
        }
    }
}