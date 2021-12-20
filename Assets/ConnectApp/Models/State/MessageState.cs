using System;
using System.Collections.Generic;
using ConnectApp.Models.Model;

namespace ConnectApp.Models.State {
    [Serializable]
    public class MessageState {
        public Dictionary<string, List<string>> channelMessageList { get; set; }
        public Dictionary<string, Dictionary<string, Message>> channelMessageDict { get; set; }
    }
}