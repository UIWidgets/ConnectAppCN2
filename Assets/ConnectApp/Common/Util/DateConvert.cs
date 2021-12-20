using System;

namespace ConnectApp.Common.Util {

    public static class DateConvert {
        static readonly DateTime startTime =
            new DateTime(2016, 1, 1, 0, 0, 0, kind: DateTimeKind.Utc);

        public static string DateStringFromNow(DateTime dt) {
            var span = DateTime.UtcNow - dt;
            if (span.TotalDays > 3) {
                return dt.ToString("yyyy-MM-dd");
            }

            if (span.TotalDays > 1) {
                return $"{(int) Math.Floor(d: span.TotalDays)}天前";
            }

            if (span.TotalHours > 1) {
                return $"{(int) Math.Floor(d: span.TotalHours)}小时前";
            }

            if (span.TotalMinutes > 1) {
                return $"{(int) Math.Floor(d: span.TotalMinutes)}分钟前";
            }

            return "刚刚";
        }

        public static string DateStringFromNonce(string nonce) {
            return DateStringFromNow(DateTimeFromNonce(nonce: nonce));
        }

        public static DateTime DateTimeFromNonce(string nonce) {
            if (string.IsNullOrEmpty(value: nonce)) {
                return startTime;
            }

            return DateTimeFromNonce(Convert.ToInt64(value: nonce, 16));
        }

        public static DateTime DateTimeFromNonce(long span) {
            var shifted = (span + 1) >> 22;
            var timespan = (shifted - 1);
            return startTime.AddMilliseconds(value: timespan);
        }
    }
}