using System;
using ConnectApp.Common.Other;
using Unity.UIWidgets.async;

namespace ConnectApp.Models.ActionModel {
    public class FeedbackScreenActionModel : BaseActionModel {
        public Action<FeedbackType> changeFeedbackType;
        public Func<string, string, string, Future> sendFeedback;
    }

    public class FeedbackTypeScreenActionModel : BaseActionModel {
        public Action<FeedbackType> changeFeedbackType;
    }
}