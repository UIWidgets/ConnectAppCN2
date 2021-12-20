#if UNITY_IOS
using System.IO;
using System.Text;
using ConnectApp.Common.Constant;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace Plugins.Editor {
    public static class XcodePostprocessBuild {
        [PostProcessBuild(999)]
        public static void OnPostprocessBuild(BuildTarget buildTarget, string path) {
            if (BuildTarget.iOS != buildTarget) {
                return;
            }
            ModifyPbxProject(path: path);
            ModifyPlist(path: path);
            ModifyBuildSettings(path: path);
        }
        static void ModifyBuildSettings(string path) {
            var xcodeProjectPath = Path.Combine(path1: path, "Unity-iPhone.xcodeproj");
            var pbxPath = Path.Combine(path1: xcodeProjectPath, "project.pbxproj");
            var xcodeProjectLines = File.ReadAllLines(path: pbxPath);
            var sb = new StringBuilder();
            foreach (var line in xcodeProjectLines) {
                if (line.Contains("GCC_ENABLE_OBJC_EXCEPTIONS") ||
                    line.Contains("GCC_ENABLE_CPP_EXCEPTIONS") ||
                    line.Contains("CLANG_ENABLE_MODULES")) {
                    var newLine = line.Replace("NO", "YES");
                    sb.AppendLine(value: newLine);
                }
                else {
                    sb.AppendLine(value: line);
                }
            }
            File.WriteAllText(path: pbxPath, sb.ToString());
        }
        static void ModifyPbxProject(string path) {
            var projPath = PBXProject.GetPBXProjectPath(buildPath: path);
            var proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(path: projPath));
            var mainTargetGuid = proj.GetUnityMainTargetGuid();
            var unityFrameworkTargetGuid = proj.GetUnityFrameworkTargetGuid();
            proj.SetBuildProperty(targetGuid: unityFrameworkTargetGuid, "LIBRARY_SEARCH_PATHS", "$(inherited)");
            proj.AddBuildProperty(targetGuid: unityFrameworkTargetGuid, "LIBRARY_SEARCH_PATHS", "$(SRCROOT)");
            proj.AddBuildProperty(targetGuid: unityFrameworkTargetGuid, "LIBRARY_SEARCH_PATHS",
                "$(PROJECT_DIR)/Libraries");
            proj.AddBuildProperty(targetGuid: unityFrameworkTargetGuid, "LIBRARY_SEARCH_PATHS",
                "$(PROJECT_DIR)/Libraries/Plugins/iOS");
            proj.AddBuildProperty(targetGuid: unityFrameworkTargetGuid, "LIBRARY_SEARCH_PATHS",
                "$(PROJECT_DIR)/Libraries/Plugins/iOS/WeChatSDK1.8.7.1");
            proj.AddBuildProperty(targetGuid: unityFrameworkTargetGuid, "LIBRARY_SEARCH_PATHS",
                "$(PROJECT_DIR)/Libraries/Plugins/iOS/Common");
            proj.AddBuildProperty(targetGuid: unityFrameworkTargetGuid, "LIBRARY_SEARCH_PATHS",
                "$(PROJECT_DIR)/Libraries/Plugins/iOS/FPSLabel");
            proj.AddBuildProperty(targetGuid: unityFrameworkTargetGuid, "LIBRARY_SEARCH_PATHS",
                "$(PROJECT_DIR)/Libraries/Plugins/iOS/ZipZap");
            proj.AddBuildProperty(targetGuid: unityFrameworkTargetGuid, "LIBRARY_SEARCH_PATHS",
                "$(PROJECT_DIR)/Libraries/Plugins/iOS/RealReachability");
            proj.AddBuildProperty(targetGuid: unityFrameworkTargetGuid, "LIBRARY_SEARCH_PATHS",
                "$(PROJECT_DIR)/Libraries/com.unity.uiwidgets/Runtime/Plugins/ios");
            // Add Framework
            proj.AddFrameworkToProject(targetGuid: unityFrameworkTargetGuid, "libz.tbd", true);
            proj.AddFrameworkToProject(targetGuid: unityFrameworkTargetGuid, "libc++.tbd", true);
            proj.AddFrameworkToProject(targetGuid: unityFrameworkTargetGuid, "libsqlite3.0.tbd", true);
            proj.AddFrameworkToProject(targetGuid: unityFrameworkTargetGuid, "CoreFoundation.framework", false);
            proj.AddFrameworkToProject(targetGuid: unityFrameworkTargetGuid, "libresolv.tbd", false);
            proj.AddFrameworkToProject(targetGuid: unityFrameworkTargetGuid, "UserNotifications.framework", false);
            proj.AddFrameworkToProject(targetGuid: unityFrameworkTargetGuid, "CoreTelephony.framework", true);
            proj.AddFrameworkToProject(targetGuid: unityFrameworkTargetGuid, "CoreServices.framework", true);
            proj.AddFrameworkToProject(targetGuid: unityFrameworkTargetGuid, "MediaPlayer.framework", true);
            proj.AddFrameworkToProject(targetGuid: unityFrameworkTargetGuid, "Photos.framework", false);
            proj.AddFrameworkToProject(targetGuid: unityFrameworkTargetGuid, "SafariServices.framework", false);
            proj.AddFrameworkToProject(targetGuid: unityFrameworkTargetGuid, "WebKit.framework", false);
            
            // Update Build Setting
            proj.SetBuildProperty(targetGuid: unityFrameworkTargetGuid, "ENABLE_BITCODE", "NO");
            proj.AddBuildProperty(targetGuid: unityFrameworkTargetGuid, "OTHER_LDFLAGS", "-all_load");
            proj.AddBuildProperty(targetGuid: unityFrameworkTargetGuid, "OTHER_LDFLAGS", "-ObjC");
            
            // Save changed
            File.WriteAllText(path: projPath, proj.WriteToString());
            // Preprocessor.h
            var preprocessor = new XClass(path + "/Classes/Preprocessor.h");
            preprocessor.Replace("#define UNITY_USES_REMOTE_NOTIFICATIONS 0",
                "#define UNITY_USES_REMOTE_NOTIFICATIONS 1");
            
            var blackDestDict = path + "/Unity-iPhone/Images.xcassets/unityConnectBlack.imageset";
            const string blackSourceFile = "image/iOS/unityConnectBlack.imageset";
            writeFile(sourceFile: blackSourceFile, destDict: blackDestDict);
            var madeDestDict = path + "/Unity-iPhone/Images.xcassets/madeWithUnity.imageset";
            const string madeSourceFile = "image/iOS/madeWithUnity.imageset";
            writeFile(sourceFile: madeSourceFile, destDict: madeDestDict);
            var arrowBackDestDict = path + "/Unity-iPhone/Images.xcassets/arrowBack.imageset";
            const string arrowBackSourceFile = "image/iOS/arrowBack.imageset";
            writeFile(sourceFile: arrowBackSourceFile, destDict: arrowBackDestDict);
            var qrScanLineDestDict = path + "/Unity-iPhone/Images.xcassets/qrScanLine.imageset";
            const string qrScanLineSourceFile = "image/iOS/qrScanLine.imageset";
            writeFile(sourceFile: qrScanLineSourceFile, destDict: qrScanLineDestDict);
            var unityIconBlackDestDict = path + "/Unity-iPhone/Images.xcassets/unityIconBlack.imageset";
            const string unityIconBlackSourceFile = "image/iOS/unityIconBlack.imageset";
            writeFile(sourceFile: unityIconBlackSourceFile, destDict: unityIconBlackDestDict);
            var launchImgDestDict = path + "/Unity-iPhone/Images.xcassets/launch_screen_img.imageset";
            const string launchImgSourceFile = "image/iOS/launch_screen_img.imageset";
            writeFile(sourceFile: launchImgSourceFile, destDict: launchImgDestDict);
            // Add WMPlayerBundle in Build Phases -> Copy Bundle Resources
            const string wmPlayerBundlePath = "Frameworks/Plugins/iOS/AVPlayer/WMPlayer/WMPlayer.bundle";
            proj.AddFileToBuild(targetGuid: mainTargetGuid, proj.AddFile(path: wmPlayerBundlePath, projectPath: wmPlayerBundlePath));
            // Save changed
            File.WriteAllText(path: projPath, proj.WriteToString());
        }
        static void writeFile(string sourceFile, string destDict) {
            FileUtil.DeleteFileOrDirectory(path: destDict);
            FileUtil.CopyFileOrDirectory(Application.dataPath + "/StreamingAssets/" + sourceFile, dest: destDict);
        }
        static void ModifyPlist(string path) {
            // Info.plist
            var plistPath = path + "/Info.plist";
            var plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(path: plistPath));
            // ROOT
            var rootDict = plist.root;
            var urlTypes = rootDict.CreateArray("CFBundleURLTypes");
            // Add URLScheme For Wechat
            var wxUrl = urlTypes.AddDict();
            wxUrl.SetString("CFBundleTypeRole", "Editor");
            wxUrl.SetString("CFBundleURLName", "weixin");
            wxUrl.SetString("CFBundleURLSchemes", val: Config.wechatAppId);
            var wxUrlScheme = wxUrl.CreateArray("CFBundleURLSchemes");
            wxUrlScheme.AddString(val: Config.wechatAppId);
            // Add URLScheme For unityconnect
            var appUrl = urlTypes.AddDict();
            appUrl.SetString("CFBundleTypeRole", "Editor");
            appUrl.SetString("CFBundleURLName", "");
            appUrl.SetString("CFBundleURLSchemes", "unityconnect");
            var appUrlScheme = appUrl.CreateArray("CFBundleURLSchemes");
            appUrlScheme.AddString("unityconnect");
            // 白名单 for wechat
            var queriesSchemes = rootDict.CreateArray("LSApplicationQueriesSchemes");
            queriesSchemes.AddString("weixin");
            queriesSchemes.AddString("weixinULAPI");
            // HTTP 设置
            const string atsKey = "NSAppTransportSecurity";
            var dictTmp = rootDict.CreateDict(key: atsKey);
            dictTmp.SetBoolean("NSAllowsArbitraryLoads", true);
            var backModes = rootDict.CreateArray("UIBackgroundModes");
            backModes.AddString("remote-notification");
            // 出口合规信息
            rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);
            rootDict.SetString("NSCameraUsageDescription", "我们需要访问您的相机，以便您正常使用修改头像、发送图片、扫一扫等功能");
            rootDict.SetString("NSPhotoLibraryUsageDescription", "我们需要访问您的相册，以便您正常使用修改头像、发送图片或者视频等功能");
            rootDict.SetString("NSPhotoLibraryAddUsageDescription", "我们需要访问您的相册，以便您正常使用保存图片功能");
            // Remove exit on suspend if it exists.
            const string exitsOnSuspendKey = "UIApplicationExitsOnSuspend";
            if (rootDict.values.ContainsKey(key: exitsOnSuspendKey)) {
                rootDict.values.Remove(key: exitsOnSuspendKey);
            }
            // 写入
            File.WriteAllText(path: plistPath, plist.WriteToString());
        }
    }
}
#endif