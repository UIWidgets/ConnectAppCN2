using ConnectApp.Api;
using ConnectApp.Common.Constant;
using ConnectApp.Models.State;
using Unity.UIWidgets.Redux;

namespace ConnectApp.redux.actions {
    public class FetchReviewUrlSuccessAction : BaseAction {
        public string url;
    }

    public class FetchReviewUrlFailureAction : BaseAction {
    }

    public class SettingClearCacheAction : BaseAction {
    }

    public class SettingVibrateAction : BaseAction {
        public bool vibrate;
    }

    public static partial class CActions {
        public static object fetchReviewUrl() {
            return new ThunkAction<AppState>((dispatcher, getState) => {
                return SettingApi.FetchReviewUrl(Config.getPlatform(), Config.getStore())
                    .then(data => {
                        if (!(data is string url)) {
                            return;
                        }

                        dispatcher.dispatch(new FetchReviewUrlSuccessAction {url = url});
                    })
                    .catchError(error => { dispatcher.dispatch(new FetchReviewUrlFailureAction()); });
            });
        }
    }
}