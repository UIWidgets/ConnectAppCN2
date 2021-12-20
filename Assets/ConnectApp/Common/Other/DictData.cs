using System.Collections.Generic;

namespace ConnectApp.Common.Other {
    public static class DictData {
        public static readonly Dictionary<string, string> jobRoleDict = new Dictionary<string, string> {
            {"Artist", "艺术家"},
            {"Designer", "设计师"},
            {"Educator", "教育工作者"},
            {"Executive", "行政人员"},
            {"Manager", "经理"},
            {"Marketer", "市场"},
            {"Owner", "董事长"},
            {"Producer", "制作人"},
            {"Programmer", "程序员"},
            {"QA", "测试"},
            {"Student", "学生"},
            {"Writer", "作家"},
            {"Other", "其他"}
        };
    }
}