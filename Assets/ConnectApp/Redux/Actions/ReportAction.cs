using ConnectApp.Api;
using ConnectApp.Common.Other;
using ConnectApp.Models.State;
using Unity.UIWidgets.Redux;

namespace ConnectApp.redux.actions {
    
    public class ChangeFeedbackTypeAction : BaseAction {
        public FeedbackType type;
    }

    public static partial class CActions {
        public static object reportItem(string itemId, string itemType, string reportContext) {
            return new ThunkAction<AppState>((dispatcher, getState) => 
                ReportApi.ReportItem(itemId: itemId, itemType: itemType, reportContext: reportContext));
        }

        public static object feedback(FeedbackType type, string content, string name = "", string contact = "") {
            return new ThunkAction<AppState>((dispatcher, getState) => 
                ReportApi.Feedback(type: type, content: content, name: name, contact: contact));
        }
    }
}