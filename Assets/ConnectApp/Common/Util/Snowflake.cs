using System;

namespace ConnectApp.Common.Util {
    public static class Snowflake {
        public static string CreateNonce() {
            var startTime = new DateTime(2016, 1, 1, 0, 0, 0, kind: DateTimeKind.Utc);
            var sinceEpoch = (long) (DateTime.UtcNow - startTime).TotalMilliseconds + 1;
            var shifted = (sinceEpoch << 22) - 1;
            var nonce = Convert.ToString(value: shifted, 16);
            while (nonce.Length < 16) {
                nonce = "0" + nonce;
            }

            return nonce;
        }
    }
}