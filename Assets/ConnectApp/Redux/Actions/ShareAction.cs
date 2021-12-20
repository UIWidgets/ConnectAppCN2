using System;
using ConnectApp.Api;
using ConnectApp.Common.Constant;
using ConnectApp.Components;
using ConnectApp.Models.State;
using ConnectApp.Plugins;
using Unity.UIWidgets.Redux;

namespace ConnectApp.redux.actions {
    public static partial class CActions {
        public static object shareToWechat(ShareSheetItemType sheetItemType, string title, string description,
            string linkUrl,
            string imageUrl = Config.unitySmallLogoUrl, string path = "", bool isMiniProgram = false) {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return ShareApi.FetchImageBytes(url: imageUrl)
                    .then(data => {
                        if (!(data is byte[] imageBytes)) {
                            return;
                        }

                        var encodeBytes = Convert.ToBase64String(inArray: imageBytes);
                        if (sheetItemType == ShareSheetItemType.friends) {
                            if (isMiniProgram) {
                                WechatPlugin.instance()
                                    .shareToMiniProgram(title: title, description: description, url: linkUrl,
                                        imageBytes: encodeBytes, path: path);
                            }
                            else {
                                WechatPlugin.instance().shareToFriend(title: title, description: description,
                                    url: linkUrl, imageBytes: encodeBytes);
                            }
                        }
                        else if (sheetItemType == ShareSheetItemType.moments) {
                            WechatPlugin.instance().shareToTimeline(title: title, description: description,
                                url: linkUrl, imageBytes: encodeBytes);
                        }
                    })
                    .catchError(error => {
                        if (sheetItemType == ShareSheetItemType.friends) {
                            if (isMiniProgram) {
                                WechatPlugin.instance().shareToMiniProgram(title: title, description: description,
                                    url: linkUrl, null, path: path);
                            }
                            else {
                                WechatPlugin.instance().shareToFriend(title: title, description: description,
                                    url: linkUrl, null);
                            }
                        }
                        else if (sheetItemType == ShareSheetItemType.moments) {
                            WechatPlugin.instance().shareToTimeline(title: title, description: description,
                                url: linkUrl, null);
                        }
                    });
            });
        }
    }
}