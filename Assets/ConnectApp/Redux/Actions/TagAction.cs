using System.Collections.Generic;
using ConnectApp.Models.Model;

namespace ConnectApp.redux.actions {
    public class TagMapAction : BaseAction {
        public Dictionary<string, Tag> tagMap;
    }
}