namespace ConnectApp.redux.actions {
    public class UtilsAction {
        public class OpenUrlAction : BaseAction {
            public string url = "";
        }

        public class CopyTextAction : BaseAction {
            public string text = "";
        }
    }
}