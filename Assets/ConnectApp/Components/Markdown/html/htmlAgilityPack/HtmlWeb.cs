using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ConnectApp.Components.Markdown.html.htmlAgilityPack {
    /// <summary>
    /// Used for downloading and parsing html from the internet
    /// </summary>
    public class HtmlWeb {
        /// <summary>
        /// Allows for setting document defaults before loading
        /// </summary>
        public Action<HtmlDocument> PreHandleDocument { get; set; }

        #region Instance Methods

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        public async Task<HtmlDocument> LoadFromWebAsync(string url) {
            return await this.LoadFromWebAsync(new Uri(uriString: url), null, null);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="encoding">The encoding to use while downloading the document</param>
        public async Task<HtmlDocument> LoadFromWebAsync(string url, Encoding encoding) {
            return await this.LoadFromWebAsync(new Uri(uriString: url), encoding: encoding, null);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="encoding">The encoding to use while downloading the document</param>
        /// <param name="userName">Username to use for credentials in the web request</param>
        /// <param name="password">Password to use for credentials in the web request</param>
        public async Task<HtmlDocument> LoadFromWebAsync(string url, Encoding encoding, string userName,
            string password) {
            return await this.LoadFromWebAsync(new Uri(uriString: url), encoding: encoding,
                new NetworkCredential(userName: userName, password: password));
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="encoding">The encoding to use while downloading the document</param>
        /// <param name="userName">Username to use for credentials in the web request</param>
        /// <param name="password">Password to use for credentials in the web request</param>
        /// <param name="domain">Domain to use for credentials in the web request</param>
        public async Task<HtmlDocument> LoadFromWebAsync(string url, Encoding encoding, string userName,
            string password, string domain) {
            return await this.LoadFromWebAsync(new Uri(uriString: url), encoding: encoding,
                new NetworkCredential(userName: userName, password: password, domain: domain));
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="userName">Username to use for credentials in the web request</param>
        /// <param name="password">Password to use for credentials in the web request</param>
        /// <param name="domain">Domain to use for credentials in the web request</param>
        public async Task<HtmlDocument> LoadFromWebAsync(string url, string userName, string password, string domain) {
            return await this.LoadFromWebAsync(new Uri(uriString: url), null,
                new NetworkCredential(userName: userName, password: password, domain: domain));
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="userName">Username to use for credentials in the web request</param>
        /// <param name="password">Password to use for credentials in the web request</param>
        public async Task<HtmlDocument> LoadFromWebAsync(string url, string userName, string password) {
            return await this.LoadFromWebAsync(new Uri(uriString: url), null,
                new NetworkCredential(userName: userName, password: password));
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="url">Url to the html document</param>
        /// <param name="credentials">The credentials to use for authenticating the web request</param>
        public async Task<HtmlDocument> LoadFromWebAsync(string url, NetworkCredential credentials) {
            return await this.LoadFromWebAsync(new Uri(uriString: url), null, credentials: credentials);
        }

        /// <summary>
        /// Begins the process of downloading an internet resource
        /// </summary>
        /// <param name="uri">Url to the html document</param>
        /// <param name="encoding">The encoding to use while downloading the document</param>
        /// <param name="credentials">The credentials to use for authenticating the web request</param>
        public async Task<HtmlDocument> LoadFromWebAsync(Uri uri, Encoding encoding, NetworkCredential credentials) {
            var clientHandler = new HttpClientHandler();
            if (credentials == null) {
                clientHandler.UseDefaultCredentials = true;
            }
            else {
                clientHandler.Credentials = credentials;
            }

            var client = new HttpClient(handler: clientHandler);

            var e = await client.GetAsync(requestUri: uri);
            if (e.StatusCode == HttpStatusCode.OK) {
                var html = string.Empty;
                if (encoding != null) {
                    using (var sr = new StreamReader(await e.Content.ReadAsStreamAsync(), encoding: encoding)) {
                        html = sr.ReadToEnd();
                    }
                }
                else {
                    html = await e.Content.ReadAsStringAsync();
                }

                var doc = new HtmlDocument();
                if (this.PreHandleDocument != null) {
                    this.PreHandleDocument(obj: doc);
                }

                doc.LoadHtml(html: html);
                return doc;
            }

            throw new Exception("Error downloading html");
        }

        #endregion
    }
}