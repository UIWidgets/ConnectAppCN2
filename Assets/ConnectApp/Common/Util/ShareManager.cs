using ConnectApp.Components;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace ConnectApp.Common.Util {
    public static class ShareManager {
        public static void showDoubleDeckShareView(
            BuildContext context,
            bool showReportAndBlock,
            bool isLoggedIn,
            VoidCallback pushToCopy,
            VoidCallback pushToLogin,
            VoidCallback pushToBlock,
            VoidCallback pushToReport,
            OnShareType shareToWechat,
            bool canEdit = false,
            VoidCallback pushToEdit = null,
            VoidCallback mainRouterPop = null
        ) {
            ActionSheetUtils.showModalActionSheet(
                context: context,
                new ShareView(
                    buildContext: context,
                    shareSheetStyle: ShareSheetStyle.doubleDeck,
                    showReportAndBlock: showReportAndBlock,
                    canEdit: canEdit,
                    onPressed: type => {
                        if (type == ShareSheetItemType.clipBoard) {
                            pushToCopy?.Invoke();
                        }
                        else if (type == ShareSheetItemType.edit) {
                            pushToEdit?.Invoke();
                        }
                        else if (type == ShareSheetItemType.block) {
                            ReportManager.blockObject(
                                context: context,
                                isLoggedIn: isLoggedIn,
                                pushToLogin: pushToLogin,
                                pushToBlock: pushToBlock,
                                mainRouterPop: mainRouterPop
                            );
                        }
                        else if (type == ShareSheetItemType.report) {
                            ReportManager.report(
                                isLoggedIn: isLoggedIn,
                                pushToLogin: pushToLogin,
                                pushToReport: pushToReport
                            );
                        }
                        else {
                            shareToWechat?.Invoke(sheetItemType: type);
                        }
                    }
                )
            );
        }
    }
}