using ConnectApp.Common.Util;
using UnityEngine;

namespace ConnectApp.Common.Constant {
    public static class Config {
        public const bool enableDebug = false;

        public const string apiAddress_cn = "https://connect.unity.cn";

        public const string apiAddress_com = "https://connect.unity.com";

        public const string apiAddress_learn = "https://learn.u3d.cn";
        
        public const string unity_cn_url = "https://unity.cn";

        public const string developer_unity_cn_url = "https://developer.unity.cn";

        public const string apiPath = "/api/connectapp/v9";

        public const string apiPath_learn = "/api/learn/cn";

        public const string auth_username_learn = "connect_app";

        public const string auth_password_learn = "4f5e6072f83d6385fe5d9ed7de239afd";

        public const string idBaseUrl = "https://id.unity.cn";

        public const string termsOfService = "https://unity.cn/legal/connectApp-terms";

        public const string privacyPolicy = "https://unity.cn/legal/connectApp-privacy";

        public const string legalEmail = "legal_china@unity3d.com";

        public const string wechatAppId = "";

        public const string miniId = "";

        public const int miniProgramType = 0; // 0 -> 正式版  1 -> 开发版  2 -> 体验版

        public const string versionName = "2.3.1";

        public const int versionCode = 151;

        public const string originCodeUrl = "https://github.com/Unity-Technologies/ConnectAppCN2";

        public const string widgetOriginCodeUrl = "https://github.com/Unity-Technologies/com.unity.uiwidgets";

        public const string unityStoreUrl = "https://store.unity.com";

        public const string unityLearnPremiumUrl = "https://unity.com/learn-premium";
        
        public const string unitySmallLogoUrl = "https://unity-cn-cdn-prd.unitychina.cn/images/app_icon_128.jpg";

        const string IOSPlatform = "ios";
        const string AndroidPlatform = "android";
        const string OtherPlatform = "other";

        const string AppStore = "appstore";
        const string OfficialStore = "official";
        const string HuaweiStore = "huawei";
        const string XiaomiStore = "xiaomi";

        const string HuaweiVendorName = "huawei";
        const string HonorVendorName = "honor";
        const string XiaomiVendorName = "xiaomi";

        public static string getPlatform() {
            if (CCommonUtils.isIPhone) {
                return IOSPlatform;
            }

            return CCommonUtils.isAndroid ? AndroidPlatform : OtherPlatform;
        }

        public static string getStore() {
            if (CCommonUtils.isIPhone) {
                return AppStore;
            }

            var deviceModel = SystemInfo.deviceModel.ToLower();
            if (deviceModel.Contains(value: XiaomiVendorName)) {
                return XiaomiStore;
            }

            if (deviceModel.Contains(value: HuaweiVendorName) || deviceModel.Contains(value: HonorVendorName)) {
                return HuaweiStore;
            }

            return OfficialStore;
        }
    }
}