using System;
using System.IO;
using UnityEngine;

namespace Plugins.Editor {
    public class XClass : IDisposable {
        readonly string _filePath;
        public XClass(string fPath) {
            this._filePath = fPath;
            if (!File.Exists(path: this._filePath)) {
                Debug.LogError(this._filePath + "not found in path.");
            }
        }

        public void WriteBelow(string below, string text) {
            var streamReader = new StreamReader(path: this._filePath);
            var textAll = streamReader.ReadToEnd();
            streamReader.Close();

            var beginIndex = textAll.IndexOf(value: below, comparisonType: StringComparison.CurrentCulture);
            if (beginIndex == -1) {
                Debug.LogError(this._filePath + " not found sign in " + below);
                return;
            }

            var endIndex = textAll.LastIndexOf("\n", beginIndex + below.Length, comparisonType: StringComparison.CurrentCulture);

            textAll = textAll.Substring(0, length: endIndex) + "\n" + text + "\n" + textAll.Substring(startIndex: endIndex);

            var streamWriter = new StreamWriter(path: this._filePath);
            streamWriter.Write(value: textAll);
            streamWriter.Close();
        }

        public void Replace(string below, string newText) {
            var streamReader = new StreamReader(path: this._filePath);
            var textAll = streamReader.ReadToEnd();
            streamReader.Close();

            var beginIndex = textAll.IndexOf(value: below, comparisonType: StringComparison.CurrentCulture);
            if (beginIndex == -1) {
                Debug.LogError(this._filePath + " not found sign in " + below);
                return;
            }

            textAll = textAll.Replace(oldValue: below, newValue: newText);
            var streamWriter = new StreamWriter(path: this._filePath);
            streamWriter.Write(value: textAll);
            streamWriter.Close();
        }

        public void Dispose() {
        }
    }
}