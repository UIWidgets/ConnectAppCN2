using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Components;
using ConnectApp.Components.Toast;
using ConnectApp.Plugins;
using ConnectApp.redux;
using ConnectApp.redux.actions;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace ConnectApp.screens {
    public class MainScreen : StatelessWidget {
        public override Widget build(BuildContext context) {
            StatusBarManager.hideStatusBar(false);
            CommonPlugin.init(buildContext: context);
            CustomToast.init(buildContext: context);
            var child = new Container(
                color: CColors.White,
                child: new CustomSafeArea(
                    top: false,
                    bottom: false,
                    child: new CustomTabBarConnector(
                        new List<Widget> {
                            new ArticlesScreenConnector(),
                            new QAScreenConnector(),
                            new LearnScreenConnector(),
                            new PersonalScreenConnector()
                        },
                        new List<CustomTabBarItem> {
                            new CustomTabBarItem(
                                0,
                                normalIcon: CIcons.tab_home_line,
                                selectedIcon: CIcons.tab_home_fill,
                                _getSelectedImages("home"),
                                "首页"
                            ),
                            new CustomTabBarItem(
                                1,
                                normalIcon: CIcons.tab_messenger_line,
                                selectedIcon: CIcons.tab_messenger_fill,
                                _getSelectedImages("messenger"),
                                "问答"
                            ),
                            new CustomTabBarItem(
                                2,
                                normalIcon: CIcons.tab_events_line,
                                selectedIcon: CIcons.tab_events_fill,
                                _getSelectedImages("event"),
                                "课堂"
                            ),
                            new CustomTabBarItem(
                                3,
                                normalIcon: CIcons.tab_mine_line,
                                selectedIcon: CIcons.tab_mine_fill,
                                _getSelectedImages("mine"),
                                "我的"
                            )
                        },
                        backgroundColor: CColors.TabBarBg,
                        (fromIndex, toIndex) => {
                            AnalyticsManager.ClickHomeTab(fromIndex: fromIndex, toIndex: toIndex);
                            var myUserId = UserInfoManager.getUserInfo().userId;
                            if (toIndex == 3 && myUserId.isNotEmpty()) {
                                // mine page
                                StoreProvider.store.dispatcher.dispatch(CActions.fetchUserProfile(userId: myUserId));
                            }
                            StatusBarManager.statusBarStyle(toIndex == 3 && UserInfoManager.isLoggedIn());
                            StoreProvider.store.dispatcher.dispatch(new SwitchTabBarIndexAction { index = toIndex });
                            return true;
                        }
                    )
                )
            );
            return child;
        }

        static List<string> _getSelectedImages(string name) {
            var loadingImages = new List<string>();
            for (var index = 0; index <= 60; index++) {
                loadingImages.Add($"image/tab-loading/{name}-tab-loading/{name}-tab-loading{index}.png");
            }
            return loadingImages;
        }
    }
}