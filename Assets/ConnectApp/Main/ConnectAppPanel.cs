using ConnectApp.Common.Constant;
using ConnectApp.Common.Util;
using ConnectApp.Plugins;
using Unity.UIWidgets.engine;
using UnityEngine;
using ui_ = Unity.UIWidgets.widgets.ui_;

namespace ConnectApp.Main {
    public sealed class ConnectAppPanel : UIWidgetsPanel {
        protected override void main() {
            ui_.runApp(new ConnectApp());
        }

        protected override void OnEnable() {
#if UNITY_ANDROID
            UIWidgetsGlobalConfiguration.EnableIncrementalGC = true;
            UIWidgetsGlobalConfiguration.EnableAutoAdjustFramerate = true;
#endif
            Application.targetFrameRate = 300;
            base.OnEnable();
            Debuger.EnableLog = Config.enableDebug;
            Screen.fullScreen = false;
            Screen.orientation = ScreenOrientation.Portrait;
            if (!CCommonUtils.isIPhone && LocalDataManager.getFPSLabelStatus()) {
                FPSLabelPlugin.SwitchFPSLabelShowStatus(true);
            }
        }
    }
}