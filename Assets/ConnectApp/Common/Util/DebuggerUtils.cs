namespace ConnectApp.Common.Util {
    public static class DebuggerUtils {
        public static void DebugAssert(bool condition, string logMsg) {
            if (!condition) {
                Debuger.Log(message: logMsg);
            }
        }
    }
}