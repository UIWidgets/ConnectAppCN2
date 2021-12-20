using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using ConnectApp.Common.Constant;
using Unity.UIWidgets.foundation;

namespace ConnectApp.Common.Util {
    public static class CStringUtils {
        public static string genApiUrl(string path) {
            return $"{Config.apiAddress_cn}{Config.apiPath}{path}";
        }
        
        public static string genLearnApiUrl(string path) {
            return $"{Config.apiAddress_learn}{Config.apiPath_learn}{path}";
        }
        
        public static string genLearnCourseUrl(string id) {
            return $"{Config.apiAddress_learn}/tutorial/{id}";
        }

        public static string JointProjectShareLink(string projectId) {
            return $"{Config.developer_unity_cn_url}/projects/{projectId}?app=true";
        }
        
        public static string JointQuestionShareLink(string questionId) {
            return $"{Config.developer_unity_cn_url}/ask/question/{questionId}";
        }

        public static string JointAnswerShareLink(string questionId, string answerId) {
            return $"{Config.developer_unity_cn_url}/ask/question/{questionId}/answer/{answerId}";
        }

        public static string CountToString(int count, string placeholder = "") {
            if (count > 0 && count < 1000) {
                return count.ToString();
            }

            if (count >= 1000 && count <= 10000) {
                return $"{count / 1000f:f1}k";
            }

            if (count > 10000) {
                return "10k+";
            }

            return placeholder;
        }

        public static string genAvatarName(string name) {
            var avatarName = "";
            var tmpName = name.Trim();
            if (tmpName.isEmpty()) {
                return avatarName;
            }

            var nameList = Regex.Split(input: tmpName, @"\s{1,}");
            if (nameList.Length > 0) {
                for (var i = 0; i < 2 && i < nameList.Length; i++) {
                    var str = nameList[i].ToCharArray();
                    if (i == 0) {
                        avatarName += str.first();
                        if (char.IsHighSurrogate(str.first())) {
                            if (str.Length > 1 && char.IsLowSurrogate(str[1])) {
                                avatarName += str[1];
                                break;
                            }

                            // There is a single high surrogate char, which will cause crash.
                            // This should never happen.
                            avatarName = $"{(char) EmojiUtils.emptyEmojiCode}";
                            break;
                        }

                        if (!str.first().ToString().IsLetterOrNumber()) {
                            break;
                        }
                    }

                    if (i == 1) {
                        if (str.first().ToString().IsLetterOrNumber()) {
                            avatarName += str.first();
                        }
                    }
                }
            }

            avatarName = avatarName.ToUpper();
            return avatarName;
        }

        public static string CreateMiniPath(string id, string title = "") {
            if (id.isNotEmpty() && title.isNotEmpty()) {
                return $"pages/Home/Home?type=toDetail&app=true&id={id}&title={title}";
            }

            return "";
        }

        public static string FileSize(long bytes) {
            if (bytes < 1024) {
                return $"{bytes}B";
            }

            if (bytes < 1024 * 1024) {
                return $"{bytes / 1024.0f:F}K";
            }

            if (bytes < 1024 * 1024 * 1024) {
                return $"{bytes / (1024.0f * 1024):F}M";
            }

            return $"{bytes / (1024.0f * 1024 * 1024):F}G";
        }

        public static long hexToLong(this string number, long defaultValue = -1) {
            if (number.isEmpty()) {
                return defaultValue;
            }

            try {
                return Convert.ToInt64(value: number, 16);
            }
            catch (Exception e) {
                Debuger.LogWarning($"Error in converting {number}: {e}");
                return defaultValue;
            }
        }

        public static string httpToHttps(this string url) {
            if (url.isEmpty()) {
                return "";
            }

            return url.Contains("http://")
                ? url.Replace("http://", "https://")
                : url;
        }

        public static bool isUrl(this string url) {
            if (url.isEmpty()) {
                return false;
            }

            return url.StartsWith("http://") || url.StartsWith("https://");
        }

        static readonly Regex LetterOrNumberRegex = new Regex(@"^[A-Za-z0-9]+$");

        public static bool IsLetterOrNumber(this string str) {
            return LetterOrNumberRegex.IsMatch(input: str);
        }

        static readonly Regex LowercaseLetterOrNumberRegex = new Regex(@"^[a-z0-9]+$");

        static bool IsLowercaseLetterOrNumber(this string str) {
            return LowercaseLetterOrNumberRegex.IsMatch(input: str);
        }

        public static bool isSlug(this string str) {
            if (str.isEmpty()) {
                return false;
            }

            return str.Length != 24 || !str.IsLowercaseLetterOrNumber();
        }

        public static string replaceRange(this string str, int start, int end, string replacement = "") {
            var theString = str;
            var sb = new StringBuilder(value: theString);
            sb.Remove(startIndex: start, end - start);
            sb.Insert(index: start, value: replacement);
            theString = sb.ToString();
            return theString;
        }

        static readonly Regex finalRegex = new Regex(@"\(.*?\)");

        public static List<string> parseImageContentId(this string content) {
            var contentIds = new List<string>();
            var matchedList = new List<string>();
            var matchedList2 = new List<string>();
            const string pattern = @"!\[.*?\]\(.*?\)";
            foreach (Match match in Regex.Matches(input: content, pattern: pattern)) {
                Console.WriteLine(value: match.Value);
                matchedList.Add(item: match.Value);
            }

            foreach (var item in matchedList) {
                var match = finalRegex.Match(input: item);
                matchedList2.Add(match.Value.Trim('(', ')'));
            }

            foreach (var matched in matchedList2) {
                var itemArr = matched.Split('/');
                if (itemArr.Length != 4 || !(itemArr[1].Equals("markdown") && itemArr[2].Equals("images"))) {
                    continue;
                }

                Console.WriteLine(itemArr.last());
                contentIds.Add(itemArr.last());
            }

            return contentIds;
        }

        public static string UnicodeToString(this string unicode) {
            var reg = new Regex(@"(?!)\\[uU]([0-9a-f]{4})");
            return reg.Replace(
                input: unicode,
                m => (
                    (char) Convert.ToInt32(value: m.Groups[1].Value, 16)).ToString()
            );
        }
    }
}