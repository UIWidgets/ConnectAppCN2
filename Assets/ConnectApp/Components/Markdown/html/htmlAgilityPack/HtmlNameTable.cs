// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>

using System.Xml;

namespace ConnectApp.Components.Markdown.html.htmlAgilityPack {
    class HtmlNameTable : XmlNameTable {
        #region Fields

        NameTable _nametable = new NameTable();

        #endregion

        #region Public Methods

        public override string Add(string array) {
            return this._nametable.Add(key: array);
        }

        public override string Add(char[] array, int offset, int length) {
            return this._nametable.Add(key: array, start: offset, len: length);
        }

        public override string Get(string array) {
            return this._nametable.Get(value: array);
        }

        public override string Get(char[] array, int offset, int length) {
            return this._nametable.Get(key: array, start: offset, len: length);
        }

        #endregion

        #region Internal Methods

        internal string GetOrAdd(string array) {
            var s = this.Get(array: array);
            if (s == null) {
                return this.Add(array: array);
            }

            return s;
        }

        #endregion
    }
}