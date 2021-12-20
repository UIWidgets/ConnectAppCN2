using System;
using System.Collections.Generic;
using ConnectApp.Api;
using ConnectApp.Components;
using ConnectApp.screens;
using Unity.UIWidgets.foundation;
using UnityEngine;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif

namespace ConnectApp.Common.Util {
    public enum QRState {
        click,
        check,
        confirm,
        cancel
    }

    public enum FavoriteTagType {
        create,
        edit,
        delete,
        collect,
        cancelCollect
    }

    public enum EnterMineType {
        Favorite,
        Event,
        History,
        Setting
    }

    public enum GameType {
        tiny,
        instant,
        tinycloud
    }

    public static class AnalyticsManager {
        public static string foucsTime;

        const string ClickEventSegmentId = "Click_Event_Segment";
        const string ClickEnterSearchId = "Click_Enter_Search";
        const string SearchKeywordId = "Search_Keyword";
        const string ClickEnterArticleDetailId = "Click_Enter_ArticleDetail";
        const string ClickReturnArticleDetailId = "Click_Return_ArticleDetail";
        const string ClickArticleTagId = "Click_Article_Tag";
        const string ClickEnterEventDetailId = "Click_Enter_EventDetail";
        const string ClickShareId = "Click_Event_Share";
        const string ClickLikeId = "Click_Event_Like";
        const string ClickCommentId = "Click_Event_Comment";
        const string ClickPublishCommentId = "Click_Event_PublishComment";
        const string ClickNotificationId = "Click_Notification";
        const string ClickHottestSearchId = "Click_Search_Hottest_Search";
        const string ClickHistorySearchId = "Click_Search_History_Search";
        const string SignUpOnlineEventId = "Sign_Up_Online_Event";
        const string ClickEnterMineId = "Click_Enter_Mine";
        const string ClickSetGradeId = "Click_Set_Grade";
        const string ClickEnterAboutUsId = "Click_Enter_AboutUs";
        const string ClickCheckUpdateId = "Click_Check_Update";
        const string ClickClearCacheId = "Click_Clear_Cache";
        const string EnterOnOpenUrlId = "Enter_On_OpenUrl";
        const string EnterAppId = "Enter_App";
        const string ClickLogoutId = "Click_Logout";
        const string ClickLoginId = "UserLogin";
        const string LoginFormId = "Login_Form";

        const string ActiveTime = "ActiveTime";
        const string QRScan = "QRScan";
        const string OpenGame = "Open_Game";
        const string HandleFavoriteTag = "HandleFavoriteTag";
        const string FavoriteArticle = "FavoriteArticle";
        const string UnFavoriteArticle = "UnFavoriteArticle";

        // splash
        const string ShowSplashPageId = "Show_Splash_Page";
        const string ClickSplashPageId = "Click_Splash_Page";
        const string ClickSkipSplashPageId = "Click_Skip_Splash_Page";

        const string TimeUpDismissSplashPageId = "Time_Up_Dismiss_Splash_Page";

        // home banner
        const string ShowHomePageBannerId = "Show_Home_Page_Banner";
        const string ManualHomePageBannerId = "Manual_Show_Home_Page_Banner";
        const string ClickHomePageBannerId = "Click_Home_Page_Banner";

        // qa
        const string ClickQAHomeTopTagId = "Click_QA_Home_Top_Tag";
        const string ClickQuestionTagId = "Click_Question_Tag";

        // tab点击统计
        public static void ClickHomeTab(int fromIndex, int toIndex) {
            if (Application.isEditor) {
                return;
            }

            var tabs = new List<string> {
                "Article", "Event", "Messenger", "Mine"
            };
            var entries = new List<string> {
                "Article_EnterArticle", "QA_EnterQA", "Event_EnterEvent", "Mine_EnterMine"
            };

            var eventType = $"Click_Tab_{entries[index: toIndex]}";
            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "from" }, { "dataType", "string" }, { "value", tabs[index: fromIndex] }
                },
                new Dictionary<string, string> {
                    { "key", "to" }, { "dataType", "string" }, { "value", tabs[index: toIndex] }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: eventType, data: data);
        }

        // 活动
        public static void ClickEventSegment(string from, string type) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "type" }, { "dataType", "string" }, { "value", type }
                },
                new Dictionary<string, string> {
                    { "key", "from" }, { "dataType", "string" }, { "value", from }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: ClickEventSegmentId, data: data);
        }

        //search点击事件统计
        public static void ClickEnterSearch(string from) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "from" }, { "dataType", "string" }, { "value", from }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: ClickEnterSearchId, data: data);
        }

        public enum SearchFrom {
            suggest,
            typing,
            switchTab,
            history,
            hottest,
            tag
        }

        public static void SearchKeyword(string keyword, SearchFrom from, SearchType type) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "keyword" }, { "dataType", "string" }, { "value", keyword }
                },
                new Dictionary<string, string> {
                    { "key", "from" }, { "dataType", "string" }, { "value", from.ToString() }
                },
                new Dictionary<string, string> {
                    { "key", "type" }, { "dataType", "string" }, { "value", type.ToString() }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: SearchKeywordId, data: data);
        }

        //进入文章详情
        public static void ClickEnterArticleDetail(string from, string articleId, string articleTitle) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "from" }, { "dataType", "string" }, { "value", from }
                },
                new Dictionary<string, string> {
                    { "key", "id" }, { "dataType", "string" }, { "value", articleId }
                },
                new Dictionary<string, string> {
                    { "key", "title" }, { "dataType", "string" }, { "value", articleTitle }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: ClickEnterArticleDetailId, data: data);
        }

        public static void ClickArticleTag(string articleId, string tagId) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "id" }, { "dataType", "string" }, { "value", articleId }
                },
                new Dictionary<string, string> {
                    { "key", "tagId" }, { "dataType", "string" }, { "value", tagId }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: ClickArticleTagId, data: data);
        }

        public static void ClickQAHomeTag(int tagIndex, string tagName) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "tagIndex" }, { "dataType", "int" }, { "value", tagIndex.ToString() }
                },
                new Dictionary<string, string> {
                    { "key", "tagName" }, { "dataType", "string" }, { "value", tagName }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: ClickQAHomeTopTagId, data: data);
        }

        public static void ClickQuestionTag(string questionId, string tagId) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "questionId" }, { "dataType", "string" }, { "value", questionId }
                },
                new Dictionary<string, string> {
                    { "key", "tagId" }, { "dataType", "string" }, { "value", tagId }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: ClickQuestionTagId, data: data);
        }

        public static void ClickReturnArticleDetail(string articleId, string articleTitle) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "id" }, { "dataType", "string" }, { "value", articleId }
                },
                new Dictionary<string, string> {
                    { "key", "title" }, { "dataType", "string" }, { "value", articleTitle }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: ClickReturnArticleDetailId, data: data);
        }

        //进入活动详情
        public static void ClickEnterEventDetail(string from, string eventId, string eventTitle, string type) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "from" }, { "dataType", "string" }, { "value", from }
                },
                new Dictionary<string, string> {
                    { "key", "id" }, { "dataType", "string" }, { "value", eventId }
                },
                new Dictionary<string, string> {
                    { "key", "title" }, { "dataType", "string" }, { "value", eventTitle }
                },
                new Dictionary<string, string> {
                    { "key", "type" }, { "dataType", "string" }, { "value", type }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: ClickEnterEventDetailId, data: data);
        }

        //分享
        public static void ClickShare(ShareSheetItemType shareSheetItemType, string type, string objectId,
            string title) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "shareType" }, { "dataType", "string" }, { "value", shareSheetItemType.ToString() }
                },
                new Dictionary<string, string> {
                    { "key", "id" }, { "dataType", "string" }, { "value", objectId }
                },
                new Dictionary<string, string> {
                    { "key", "title" }, { "dataType", "string" }, { "value", title }
                },
                new Dictionary<string, string> {
                    { "key", "type" }, { "dataType", "string" }, { "value", type }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: ClickShareId, data: data);
        }

        // 点赞文章或者评价
        public static void ClickLike(string type, string articleId, string commentId = null) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "type" }, { "dataType", "string" }, { "value", type }
                },
                new Dictionary<string, string> {
                    { "key", "id" }, { "dataType", "string" }, { "value", articleId }
                }
            };
            if (commentId.isNotEmpty()) {
                data.Add(
                    new Dictionary<string, string> {
                        { "key", "commentId" }, { "dataType", "string" }, { "value", commentId }
                    }
                );
            }

            AnalyticsApi.AnalyticsApp(eventType: ClickLikeId, data: data);
        }

        public static void ClickComment(string type, string channelId, string title, string commentId = null) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "type" }, { "dataType", "string" }, { "value", type }
                },
                new Dictionary<string, string> {
                    { "key", "channelId" }, { "dataType", "string" }, { "value", channelId }
                },
                new Dictionary<string, string> {
                    { "key", "title" }, { "dataType", "string" }, { "value", title }
                }
            };
            if (commentId.isNotEmpty()) {
                data.Add(
                    new Dictionary<string, string> {
                        { "key", "commentId" }, { "dataType", "string" }, { "value", commentId }
                    }
                );
            }

            AnalyticsApi.AnalyticsApp(eventType: ClickCommentId, data: data);
        }

        public static void ClickPublishComment(string type, string channelId, string commentId = null) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "type" }, { "dataType", "string" }, { "value", type }
                },
                new Dictionary<string, string> {
                    { "key", "channelId" }, { "dataType", "string" }, { "value", channelId }
                }
            };
            if (commentId.isNotEmpty()) {
                data.Add(
                    new Dictionary<string, string> {
                        { "key", "commentId" }, { "dataType", "string" }, { "value", commentId }
                    }
                );
            }

            AnalyticsApi.AnalyticsApp(eventType: ClickPublishCommentId, data: data);
        }

        public static void ClickNotification(string type, string subtype, string id) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "type" }, { "dataType", "string" }, { "value", type }
                },
                new Dictionary<string, string> {
                    { "key", "subtype" }, { "dataType", "string" }, { "value", subtype }
                },
                new Dictionary<string, string> {
                    { "key", "id" }, { "dataType", "string" }, { "value", id }
                }
            };

            AnalyticsApi.AnalyticsApp(eventType: ClickNotificationId, data: data);
        }

        public static void ShowSplashPage(string id, string name, string url) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "id" }, { "dataType", "string" }, { "value", id }
                },
                new Dictionary<string, string> {
                    { "key", "name" }, { "dataType", "string" }, { "value", name }
                },
                new Dictionary<string, string> {
                    { "key", "url" }, { "dataType", "string" }, { "value", url }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: ShowSplashPageId, data: data);
        }

        public static void ClickSplashPage(string id, string name, string url) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "id" }, { "dataType", "string" }, { "value", id }
                },
                new Dictionary<string, string> {
                    { "key", "name" }, { "dataType", "string" }, { "value", name }
                },
                new Dictionary<string, string> {
                    { "key", "url" }, { "dataType", "string" }, { "value", url }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: ClickSplashPageId, data: data);
        }

        public static void ClickSkipSplashPage(string id, string name, string url) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "id" }, { "dataType", "string" }, { "value", id }
                },
                new Dictionary<string, string> {
                    { "key", "name" }, { "dataType", "string" }, { "value", name }
                },
                new Dictionary<string, string> {
                    { "key", "url" }, { "dataType", "string" }, { "value", url }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: ClickSkipSplashPageId, data: data);
        }

        public static void TimeUpDismissSplashPage(string id, string name, string url) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "id" }, { "dataType", "string" }, { "value", id }
                },
                new Dictionary<string, string> {
                    { "key", "name" }, { "dataType", "string" }, { "value", name }
                },
                new Dictionary<string, string> {
                    { "key", "url" }, { "dataType", "string" }, { "value", url }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: TimeUpDismissSplashPageId, data: data);
        }

        public static void ManualShowHomePageBanner(string id, string name, string url) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "id" }, { "dataType", "string" }, { "value", id }
                },
                new Dictionary<string, string> {
                    { "key", "name" }, { "dataType", "string" }, { "value", name }
                },
                new Dictionary<string, string> {
                    { "key", "url" }, { "dataType", "string" }, { "value", url }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: ManualHomePageBannerId, data: data);
        }

        public static void ShowHomePageBanner(string id, string name, string url) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "id" }, { "dataType", "string" }, { "value", id }
                },
                new Dictionary<string, string> {
                    { "key", "name" }, { "dataType", "string" }, { "value", name }
                },
                new Dictionary<string, string> {
                    { "key", "url" }, { "dataType", "string" }, { "value", url }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: ShowHomePageBannerId, data: data);
        }

        public static void ClickHomePageBanner(string id, string name, string url) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "id" }, { "dataType", "string" }, { "value", id }
                },
                new Dictionary<string, string> {
                    { "key", "name" }, { "dataType", "string" }, { "value", name }
                },
                new Dictionary<string, string> {
                    { "key", "url" }, { "dataType", "string" }, { "value", url }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: ClickHomePageBannerId, data: data);
        }

        //评分
        public static void ClickSetGrade() {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>>();
            AnalyticsApi.AnalyticsApp(eventType: ClickSetGradeId, data: data);
        }

        public static void ClickEnterAboutUs() {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>>();
            AnalyticsApi.AnalyticsApp(eventType: ClickEnterAboutUsId, data: data);
        }

        public static void ClickCheckUpdate() {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>>();
            AnalyticsApi.AnalyticsApp(eventType: ClickCheckUpdateId, data: data);
        }

        public static void ClickClearCache() {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>>();
            AnalyticsApi.AnalyticsApp(eventType: ClickClearCacheId, data: data);
        }

        //通过openurl方式打开app
        public static void EnterOnOpenUrl(string url) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "url" }, { "dataType", "string" }, { "value", url }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: EnterOnOpenUrlId, data: data);
        }

        public static void EnterApp() {
            if (Application.isEditor) {
                return;
            }

            foucsTime = DateTime.UtcNow.ToString();
            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "app" }, { "dataType", "string" }, { "value", "unity connect" }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: EnterAppId, data: data);
        }

        public static void BrowseArtileDetail(string id, string name, DateTime startTime, DateTime endTime) {
            if (Application.isEditor) {
                return;
            }

            var duration = (endTime - startTime).TotalSeconds.ToString("0.0");
        }

        public static void BrowseEventDetail(string id, string name, DateTime startTime, DateTime endTime) {
            if (Application.isEditor) {
                return;
            }

            var duration = (endTime - startTime).TotalSeconds.ToString("0.0");
        }

        public static void AnalyticsOpenApp() {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "enableNotification" }, { "dataType", "bool" },
                    { "value", enableNotification().ToString() }
                }
            };
            AnalyticsApi.AnalyticsApp("OpenApp", data: data);
        }

        public static void AnalyticsWakeApp(string mode, string id = null, string type = null, string subtype = null) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>>();
            if (id.isNotEmpty()) {
                data.Add(new Dictionary<string, string> {
                    { "key", "id" }, { "dataType", "string" }, { "value", id }
                });
            }

            if (type.isNotEmpty()) {
                data.Add(new Dictionary<string, string> {
                    { "key", "type" }, { "dataType", "string" }, { "value", type }
                });
            }

            if (subtype.isNotEmpty()) {
                data.Add(new Dictionary<string, string> {
                    { "key", "subtype" }, { "dataType", "string" }, { "value", subtype }
                });
            }

            AnalyticsApi.AnalyticsApp(eventType: mode, data: data);
        }

        public static void ClickLogout() {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>>();
            AnalyticsApi.AnalyticsApp(eventType: ClickLogoutId, data: data);
        }

        public static void AnalyticsLogin(string type) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "type" }, { "dataType", "string" }, { "value", type }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: ClickLoginId, data: data);
        }

        public static void AnalyticsActiveTime(int timespan) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "duration" }, { "dataType", "int" }, { "value", timespan.ToString() }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: ActiveTime, data: data);
        }

        public static void AnalyticsQRScan(QRState state, bool success = true) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "state" }, { "dataType", "string" }, { "value", state.ToString() }
                },
                new Dictionary<string, string> {
                    { "key", "success" }, { "dataType", "bool" }, { "value", success.ToString() }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: QRScan, data: data);
        }

        public static void AnalyticsOpenGame(GameType type, string url, string name) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "type" }, { "dataType", "string" }, { "value", type.ToString() }
                },
                new Dictionary<string, string> {
                    { "key", "url" }, { "dataType", "string" }, { "value", url }
                },
                new Dictionary<string, string> {
                    { "key", "name" }, { "dataType", "string" }, { "value", name }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: OpenGame, data: data);
        }

        public static void AnalyticsHandleFavoriteTag(FavoriteTagType type) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "type" }, { "dataType", "string" }, { "value", type.ToString() }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: HandleFavoriteTag, data: data);
        }

        public static void AnalyticsFavoriteArticle(string articleId, IEnumerable<string> favoriteTagIds) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "articleId" }, { "dataType", "string" }, { "value", articleId }
                },
                new Dictionary<string, string> {
                    { "key", "state" }, { "dataType", "string" }, { "value", string.Join(",", values: favoriteTagIds) }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: FavoriteArticle, data: data);
        }

        public static void AnalyticsUnFavoriteArticle(string favoriteId) {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>> {
                new Dictionary<string, string> {
                    { "key", "favoriteId" }, { "dataType", "string" }, { "value", favoriteId }
                }
            };
            AnalyticsApi.AnalyticsApp(eventType: UnFavoriteArticle, data: data);
        }

        public static void AnalyticsClickHomeFocus() {
            if (Application.isEditor) {
                return;
            }

            var data = new List<Dictionary<string, string>>();
            AnalyticsApi.AnalyticsApp("ClickHomeFocus",
                data: data);
        }

        public static string deviceId() {
            return Application.isEditor ? "Editor" : getDeviceID();
        }

        public static bool enableNotification() {
            return isEnableNotification();
        }

#if UNITY_IOS
        [DllImport("__Internal")]
        static extern string getDeviceID();

        [DllImport("__Internal")]
        static extern bool isEnableNotification();

#elif UNITY_ANDROID
        static AndroidJavaClass _plugin;

        static AndroidJavaClass Plugin() {
            if (_plugin == null) {
                _plugin = new AndroidJavaClass("com.unity3d.unityconnect.plugins.CommonPlugin");
            }

            return _plugin;
        }

        static string getDeviceID() {
            return Plugin().CallStatic<string>("getDeviceID");
        }

        static bool isEnableNotification() {
            return Plugin().CallStatic<bool>("isEnableNotification");
        }
#else
        static string getDeviceID() {
            return "Unity Editor";
        }

        static bool isEnableNotification() {
            return false;
        }
#endif
    }
}