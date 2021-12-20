using System;
using System.Collections.Generic;
using ConnectApp.Models.Model;

namespace ConnectApp.Models.State {
    
    [Serializable]
    public class NotificationCategoryState {
        public bool loading { get; set; }
        public int page { get; set; }
        public int pageTotal { get; set; }
        public List<Notification> notifications { get; set; }
        public List<User> mentions { get; set; }
    }
    
    [Serializable]
    public class NotificationState {
        public NotificationCategoryState allNotificationState;
        public NotificationCategoryState followNotificationState;
        public NotificationCategoryState involveNotificationState;
        public NotificationCategoryState participateNotificationState;
        public NotificationCategoryState systemNotificationState;
    }
}