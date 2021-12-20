using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ConnectApp.Common.Util.Sqlite;
using Unity.UIWidgets.async;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine.Networking;

namespace ConnectApp.Components.CachedNetworkImage {
    enum CachedImagePhase {
        start,
        waiting,
        fadeOut,
        fadeIn,
        completed
    }

    delegate void _CachedImageProviderResolverListener();

    class _CachedImageProviderResolver {
        public _CachedImageProviderResolver(
            _CachedNetworkImageState state,
            _CachedImageProviderResolverListener listener
        ) {
            this.state = state;
            this.listener = listener;
        }

        readonly _CachedNetworkImageState state;
        readonly _CachedImageProviderResolverListener listener;

        CachedNetworkImage widget {
            get { return this.state.widget; }
        }

        public ImageStream _imageStream;
        public ImageInfo _imageInfo;

        public void resolve(ImageProvider provider) {
            var oldImageStream = this._imageStream;
            Size size;
            if (this.widget.width != null && this.widget.height != null) {
                size = new Size((float) this.widget.width, (float) this.widget.height);
            }
            else {
                size = null;
            }

            this._imageStream =
                provider.resolve(ImageUtils.createLocalImageConfiguration(context: this.state.context, size: size));
            D.assert(this._imageStream != null);

            if (this._imageStream.key != oldImageStream?.key) {
                oldImageStream?.removeListener(new ImageStreamListener(onImage: this._handleImageChanged));
                this._imageStream.addListener(new ImageStreamListener(onImage: this._handleImageChanged));
            }
        }

        void _handleImageChanged(ImageInfo imageInfo, bool synchronousCall) {
            this._imageInfo = imageInfo;
            this.listener();
        }

        public void stopListening() {
            this._imageStream?.removeListener(new ImageStreamListener(onImage: this._handleImageChanged));
        }
    }

    public class CachedNetworkImageProvider : ImageProvider<CachedNetworkImageProvider>,
        IEquatable<CachedNetworkImageProvider> {
        public CachedNetworkImageProvider(
            string url,
            float scale = 1.0f,
            IDictionary<string, string> headers = null
        ) {
            this.url = url;
            this.scale = scale;
            this.headers = headers;
        }

        readonly string url;
        readonly float scale;
        readonly IDictionary<string, string> headers;

        public override Future<CachedNetworkImageProvider> obtainKey(ImageConfiguration configuration) {
            return new SynchronousFuture<CachedNetworkImageProvider>(this);
        }

        public override ImageStreamCompleter load(CachedNetworkImageProvider key, DecoderCallback decode) {
            IEnumerable<DiagnosticsNode> infoCollector() {
                yield return new DiagnosticsProperty<ImageProvider>("Image provider", this);
                yield return new DiagnosticsProperty<CachedNetworkImageProvider>("Image key", value: key);
            }

            return new MultiFrameImageStreamCompleter(
                this._loadAsync(key: key, decode: decode),
                scale: key.scale,
                informationCollector: infoCollector
            );
        }

        Future<Codec> _loadAsync(CachedNetworkImageProvider key, DecoderCallback decode) {
            var localPath = SQLiteDBManager.instance.GetCachedFilePath(url: key.url);
            //the cached file might be deleted by the OS
            if (!File.Exists(path: localPath)) {
                localPath = null;
            }

            var completer = Completer.create();
            var isolate = Isolate.current;
            var panel = UIWidgetsPanelWrapper.current.window;
            if (panel.isActive()) {
                if (localPath != null) {
                    panel.startCoroutine(_loadFromFile(file: localPath, completer: completer, isolate: isolate));
                }
                else {
                    panel.startCoroutine(this._loadFromNetwork(url: key.url, completer: completer, isolate: isolate));
                }

                return completer.future.to<byte[]>().then_<byte[]>(data => {
                    if (data != null && data.Length > 0) {
                        return decode(bytes: data);
                    }

                    throw new Exception("not loaded");
                }).to<Codec>();
            }
            else {
                return new Future<Codec>(Future.create(() => FutureOr.value(null)));
            }
        }

        static IEnumerator _loadFromFile(string file, Completer completer, Isolate isolate) {
#if UNITY_EDITOR_WIN
            var uri = "file:///" + file;
#else
            var uri = "file://" + file;
#endif
            using (var www = UnityWebRequest.Get(uri: uri)) {
                yield return www.SendWebRequest();
                using (Isolate.getScope(isolate: isolate)) {
                    if (www.isNetworkError || www.isHttpError) {
                        completer.completeError(new Exception($"Failed to get file \"{uri}\": {www.error}"));
                        yield break;
                    }

                    var data = www.downloadHandler.data;
                    completer.complete(value: data);
                }
            }
        }

        IEnumerator _loadFromNetwork(string url, Completer completer, Isolate isolate) {
            var uri = new Uri(uriString: url);

            var suffix = "png";
            if (uri.LocalPath.EndsWith(".gif") || uri.LocalPath.EndsWith(".GIF")) {
                suffix = "gif";
            }

            using (var www = UnityWebRequest.Get(uri: uri)) {
                if (this.headers != null) {
                    foreach (var header in this.headers) {
                        www.SetRequestHeader(name: header.Key, value: header.Value);
                    }
                }

                yield return www.SendWebRequest();

                using (Isolate.getScope(isolate: isolate)) {
                    if (www.isNetworkError || www.isHttpError) {
                        completer.completeError(new Exception($"Failed to load from url \"{uri}\": {www.error}"));
                        yield break;
                    }

                    var data = www.downloadHandler.data;
                    CacheFileHelper.SyncSaveCacheFile(url: url, data: data, suffix: suffix);
                    completer.complete(value: data);
                }
            }
        }

        public bool Equals(CachedNetworkImageProvider other) {
            if (ReferenceEquals(null, objB: other)) {
                return false;
            }

            if (ReferenceEquals(this, objB: other)) {
                return true;
            }

            return string.Equals(a: this.url, b: other.url) && this.scale.Equals(obj: other.scale);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, objB: obj)) {
                return false;
            }

            if (ReferenceEquals(this, objB: obj)) {
                return true;
            }

            if (obj.GetType() != this.GetType()) {
                return false;
            }

            return this.Equals((CachedNetworkImageProvider) obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((this.url != null ? this.url.GetHashCode() : 0) * 397) ^ this.scale.GetHashCode();
            }
        }

        public static bool operator ==(CachedNetworkImageProvider left, CachedNetworkImageProvider right) {
            return Equals(objA: left, objB: right);
        }

        public static bool operator !=(CachedNetworkImageProvider left, CachedNetworkImageProvider right) {
            return !Equals(objA: left, objB: right);
        }

        public override string ToString() {
            return $"runtimeType(\"{this.url}\", scale: {this.scale})";
        }
    }
}