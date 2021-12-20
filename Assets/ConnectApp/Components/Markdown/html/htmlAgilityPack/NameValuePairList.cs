// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>

using System;
using System.Collections.Generic;

namespace ConnectApp.Components.Markdown.html.htmlAgilityPack {
    class NameValuePairList {
        #region Fields

        internal readonly string Text;
        List<KeyValuePair<string, string>> _allPairs;
        Dictionary<string, List<KeyValuePair<string, string>>> _pairsWithName;

        #endregion

        #region Constructors

        internal NameValuePairList() :
            this(null) {
        }

        internal NameValuePairList(string text) {
            this.Text = text;
            this._allPairs = new List<KeyValuePair<string, string>>();
            this._pairsWithName = new Dictionary<string, List<KeyValuePair<string, string>>>();

            this.Parse(text: text);
        }

        #endregion

        #region Internal Methods

        internal static string GetNameValuePairsValue(string text, string name) {
            var l = new NameValuePairList(text: text);
            return l.GetNameValuePairValue(name: name);
        }

        internal List<KeyValuePair<string, string>> GetNameValuePairs(string name) {
            if (name == null) {
                return this._allPairs;
            }

            return this._pairsWithName.ContainsKey(key: name)
                ? this._pairsWithName[key: name]
                : new List<KeyValuePair<string, string>>();
        }

        internal string GetNameValuePairValue(string name) {
            if (name == null) {
                throw new ArgumentNullException();
            }

            var al = this.GetNameValuePairs(name: name);
            if (al.Count == 0) {
                return string.Empty;
            }

            // return first item
            return al[0].Value.Trim();
        }

        #endregion

        #region Private Methods

        void Parse(string text) {
            this._allPairs.Clear();
            this._pairsWithName.Clear();
            if (text == null) {
                return;
            }

            var p = text.Split(';');
            foreach (var pv in p) {
                if (pv.Length == 0) {
                    continue;
                }

                var onep = pv.Split(new[] {'='}, 2);
                if (onep.Length == 0) {
                    continue;
                }

                var nvp = new KeyValuePair<string, string>(onep[0].Trim().ToLower(),
                    onep.Length < 2 ? "" : onep[1]);

                this._allPairs.Add(item: nvp);

                // index by name
                List<KeyValuePair<string, string>> al;
                if (!this._pairsWithName.ContainsKey(key: nvp.Key)) {
                    al = new List<KeyValuePair<string, string>>();
                    this._pairsWithName[key: nvp.Key] = al;
                }
                else {
                    al = this._pairsWithName[key: nvp.Key];
                }

                al.Add(item: nvp);
            }
        }

        #endregion
    }
}