using System.Collections.Generic;

namespace ConnectApp.Common.Other {
    public static class NotificationCategory {
        public const string All = "all";
        public const string Follow = "follow";
        public const string Involve = "involve";
        public const string Participate = "participate";
        public const string System = "system";
    }

    public class FeedbackType {
        FeedbackType(string value, string description) {
            this.value = value;
            this.description = description;
        }

        public string value { get; }
        public string description { get; }

        public static List<FeedbackType> typesList {
            get {
                return new List<FeedbackType> {
                    Advice,
                    Bug,
                    Other
                };
            }
        }

        public static FeedbackType Advice { get { return new FeedbackType("advice", "意见建议"); } }
        public static FeedbackType Bug { get { return new FeedbackType("bug", "BUG 相关"); } }
        public static FeedbackType Other { get { return new FeedbackType("other", "其他"); } }
    }
}