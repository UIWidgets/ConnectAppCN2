using System;

namespace ConnectApp.Models.Model {
    [Serializable]
    public class NotificationAttr {
        public string type;
        public int parentType;
        public string parentTypeName;
        public string notifOperator;
        public bool showOperator;
        public string operationName;
        public string target;
        public string operationResult;
        public string childContent;
    }
}