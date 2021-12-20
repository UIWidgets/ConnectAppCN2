using System.Diagnostics;

namespace ConnectApp.Components.Markdown.html.htmlAgilityPack {
    partial class Trace {
        partial void WriteLineIntern(string message, string category) {
            Debug.WriteLine(message: message, category: category);
        }
    }
}