#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace ConnectApp.Common.Util {
    public static class StatusBarManager {
        public static void hideStatusBar(bool hidden) {
#if UNITY_IPHONE && !UNITY_EDITOR
            _ShowStatusBar(!hidden);
#endif
        }

        // style:
        // 1:UIStatusBarStyleLightContent
        // 2:UIStatusBarStyleBlackTranslucent
        // 3:UIStatusBarStyleBlackOpaque
        // else:UIStatusBarStyleDefault
        public static void statusBarStyle(bool isLight) {
#if UNITY_IPHONE && !UNITY_EDITOR
            _SetStatusBarStyle(isLight ? 1 : 0);
#endif
        }

#if UNITY_IPHONE
        [DllImport("__Internal")]
        static extern void _ShowStatusBar(bool isShow);

        [DllImport("__Internal")]
        static extern void _SetStatusBarStyle(int style);

        [DllImport("__Internal")]
        static extern void _SetStatusBarAnimation(int anim);
#endif

// #if UNITY_IOS
//         [DllImport("__Internal")]
//         static extern void setStatusBarStyle(bool isLight);
//
//         [DllImport("__Internal")]
//         static extern void hiddenStatusBar(bool hidden);
//
// #elif UNITY_ANDROID
//         static void setStatusBarStyle(bool isLight) {
//         }
//         static void hiddenStatusBar(bool hidden) {
//         }
// #else
//         static void setStatusBarStyle(bool isLight) {
//         }
//         static void hiddenStatusBar(bool hidden) {
//         }
// #endif
    }
}