using System;
using System.Xml.XPath;

namespace ConnectApp.Components.Markdown.html.htmlAgilityPack {
    public partial class HtmlNode : IXPathNavigable {
        /// <summary>
        /// Creates a new XPathNavigator object for navigating this HTML node.
        /// </summary>
        /// <returns>An XPathNavigator object. The XPathNavigator is positioned on the node from which the method was called. It is not positioned on the root of the document.</returns>
        public XPathNavigator CreateNavigator() {
            return new HtmlNodeNavigator(doc: this.OwnerDocument, this);
        }

        /// <summary>
        /// Creates an XPathNavigator using the root of this document.
        /// </summary>
        /// <returns></returns>
        public XPathNavigator CreateRootNavigator() {
            return new HtmlNodeNavigator(doc: this.OwnerDocument, currentNode: this.OwnerDocument.DocumentNode);
        }

        /// <summary>
        /// Selects a list of nodes matching the <see cref="XPath"/> expression.
        /// </summary>
        /// <param name="xpath">The XPath expression.</param>
        /// <returns>An <see cref="HtmlNodeCollection"/> containing a collection of nodes matching the <see cref="XPath"/> query, or <c>null</c> if no node matched the XPath expression.</returns>
        public HtmlNodeCollection SelectNodes(string xpath) {
            var list = new HtmlNodeCollection(null);

            var nav = new HtmlNodeNavigator(doc: this.OwnerDocument, this);
            var it = nav.Select(xpath: xpath);
            while (it.MoveNext()) {
                var n = (HtmlNodeNavigator) it.Current;
                list.Add(node: n.CurrentNode);
            }

            if (list.Count == 0) {
                return null;
            }

            return list;
        }

        /// <summary>
        /// Selects the first XmlNode that matches the XPath expression.
        /// </summary>
        /// <param name="xpath">The XPath expression. May not be null.</param>
        /// <returns>The first <see cref="HtmlNode"/> that matches the XPath query or a null reference if no matching node was found.</returns>
        public HtmlNode SelectSingleNode(string xpath) {
            if (xpath == null) {
                throw new ArgumentNullException("xpath");
            }

            var nav = new HtmlNodeNavigator(doc: this.OwnerDocument, this);
            var it = nav.Select(xpath: xpath);
            if (!it.MoveNext()) {
                return null;
            }

            var node = (HtmlNodeNavigator) it.Current;
            return node.CurrentNode;
        }
    }
}