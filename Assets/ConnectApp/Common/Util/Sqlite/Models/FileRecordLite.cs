using SQLite4Unity3d;
using UnityEngine.Scripting;

namespace ConnectApp.Common.Util.Sqlite.Models {
    public class FileRecordLite {  
        [Preserve][Indexed][PrimaryKey]
        public string url { get; set; }
        [Preserve]
        public string filepath { get; set; }
    }
}