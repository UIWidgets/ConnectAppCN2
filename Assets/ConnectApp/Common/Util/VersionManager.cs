using System;
using System.Collections.Generic;
using ConnectApp.Api;
using ConnectApp.Common.Constant;
using ConnectApp.Common.Visual;
using ConnectApp.Components;
using ConnectApp.Components.Toast;
using ConnectApp.Models.Api;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Image = Unity.UIWidgets.widgets.Image;

namespace ConnectApp.Common.Util {
    public enum CheckVersionType {
        initialize,
        setting
    }

    public static class VersionManager {
        const string _noticeNewVersionTimeKey = "noticeNewVersionTimeKey";
        const string _needForceUpdateMinVersionCodeKey = "needForceUpdateMinVersionCodeKey";

        public static void checkForUpdates(BuildContext context, CheckVersionType type) {
            if (type == CheckVersionType.setting) {
                CustomDialogUtils.showCustomDialog<object>(
                    context: context,
                    child: new CustomLoadingDialog(message: "正在检查更新")
                );
            }
            
            SettingApi.CheckNewVersion(Config.getPlatform(), Config.getStore(), $"{Config.versionCode}")
                .then(data => {
                    if (!(data is CheckNewVersionResponse versionResponse)) {
                        return;
                    }

                    if (type == CheckVersionType.setting) {
                        CustomDialogUtils.hiddenCustomDialog(context: context);
                    }

                    var status = versionResponse.status;
                    if (status == "NEED_UPDATE" && versionResponse.url.isNotEmpty()) {
                        if (type == CheckVersionType.initialize && !needNoticeNewVersion() || needForceUpdate()) {
                            return;
                        }
                        markUpdateNoticeTime();
                        CustomDialogUtils.showCustomDialog(
                            context: context,
                            true,
                            barrierColor: CColors.Shadow,
                            new CustomAlertDialog(
                                null,
                                message: versionResponse.changeLog,
                                new List<Widget> {
                                    new CustomButton(
                                        child: new Center(
                                            child: new Text(
                                                "稍后再说",
                                                style: CTextStyle.PLargeBody5.defaultHeight(),
                                                textAlign: TextAlign.center
                                            )
                                        ),
                                        onPressed: () => CustomDialogUtils.hiddenCustomDialog(context: context)
                                    ),
                                    new CustomButton(
                                        child: new Center(
                                            child: new Text(
                                                "立即更新",
                                                style: CTextStyle.PLargeBlue.defaultHeight(),
                                                textAlign: TextAlign.center
                                            )
                                        ),
                                        onPressed: () => {
                                            CustomDialogUtils.hiddenCustomDialog(context: context);
                                            Application.OpenURL(url: versionResponse.url);
                                        }
                                    )
                                },
                                Image.file("image/updaterBg.png")
                            )
                        );
                    }
                    else {
                        if (type == CheckVersionType.setting) {
                            CustomToast.showToast(context: context, "当前是最新版本");
                        }
                    }
                })
                .catchError(error => {
                    if (type == CheckVersionType.setting) {
                        CustomDialogUtils.hiddenCustomDialog(context: context);
                    }
                });
        }

        public static void saveMinVersionCode(int versionCode = 0) {
            if (versionCode == 0) {
                return;
            }

            PlayerPrefs.SetInt(key: _needForceUpdateMinVersionCodeKey, value: versionCode);
            PlayerPrefs.Save();
        }

        public static bool needForceUpdate() {
            if (!PlayerPrefs.HasKey(key: _needForceUpdateMinVersionCodeKey)) {
                return false;
            }

            var minVersionCode = PlayerPrefs.GetInt(key: _needForceUpdateMinVersionCodeKey, 0);
            return minVersionCode > Config.versionCode;
        }

        static bool needNoticeNewVersion() {
            if (!PlayerPrefs.HasKey(key: _noticeNewVersionTimeKey)) {
                // when need update first check
                return true;
            }

            var timeString = PlayerPrefs.GetString(key: _noticeNewVersionTimeKey);
            var endTime = DateTime.Parse(s: timeString);
            return DateTime.Compare(t1: endTime, t2: DateTime.Now) <= 0;
        }

        static void markUpdateNoticeTime() {
            var noticeTimeString = DateTime.Now.AddDays(1).ToString("yyyy/MM/dd HH:mm:ss");
            PlayerPrefs.SetString(key: _noticeNewVersionTimeKey, value: noticeTimeString);
            PlayerPrefs.Save();
        }
    }
}