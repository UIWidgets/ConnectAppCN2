using System.Collections.Generic;

namespace ConnectApp.Components.Markdown.html.htmlAgilityPack {
    static class Utilities {
        public static TValue GetDictionaryValueOrNull<TKey, TValue>(Dictionary<TKey, TValue> dict, TKey key)
            where TKey : class {
            return dict.ContainsKey(key: key) ? dict[key: key] : default(TValue);
        }
    }
}