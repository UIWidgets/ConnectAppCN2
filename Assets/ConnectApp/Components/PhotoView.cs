using System;
using System.Collections.Generic;
using ConnectApp.Common.Constant;
using ConnectApp.Common.Util;
using ConnectApp.Common.Util.Sqlite;
using ConnectApp.Common.Visual;
using ConnectApp.Plugins;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using ZXing;
using Color = Unity.UIWidgets.ui.Color;
using Image = Unity.UIWidgets.widgets.Image;

namespace ConnectApp.Components {
    public delegate void OnScaleChangedCallback(float scale, float horizontalScale, float verticalScale,
        Offset position, bool scaling);

    public class PhotoView : StatefulWidget {
        public readonly PageController controller;
        public readonly Dictionary<string, string> headers;
        public readonly Dictionary<string, byte[]> imageData;
        public readonly int index;
        public readonly float maxScale;
        public readonly float minScale;
        public readonly List<string> urls;
        public readonly bool useCachedNetworkImage;

        public PhotoView(
            List<string> urls = null,
            int index = 0,
            PageController controller = null,
            Dictionary<string, string> headers = null,
            bool useCachedNetworkImage = false,
            Dictionary<string, byte[]> imageData = null,
            float maxScale = 2.0f,
            float minScale = 1.0f,
            Key key = null) : base(key: key) {
            D.assert(urls != null);
            D.assert(urls.isNotEmpty());
            D.assert(minScale >= 0.0f && minScale <= 1.0f);
            D.assert(maxScale >= 1.0f);
            this.urls = urls;
            this.imageData = imageData;
            this.index = index;
            this.controller = controller ?? new PageController(initialPage: index);
            this.headers = headers;
            this.useCachedNetworkImage = useCachedNetworkImage;
            this.maxScale = maxScale;
            this.minScale = minScale;
        }

        public override State createState() {
            return new _PhotoViewState();
        }
    }

    class _PhotoViewState : TickerProviderStateMixin<PhotoView>, RouteAware {
        readonly Dictionary<string, string> _defaultHeaders = new Dictionary<string, string> {
            { HttpManager.COOKIE, HttpManager.getCookie() },
            { "ConnectAppVersion", Config.versionName },
            { "X-Requested-With", "XmlHttpRequest" }
        };

        int currentIndex;

        bool locked;

        public void didPopNext() {
        }

        public void didPush() {
            StatusBarManager.hideStatusBar(true);
        }

        public void didPop() {
            StatusBarManager.hideStatusBar(false);
        }

        public void didPushNext() {
        }

        public override void initState() {
            base.initState();
            this.currentIndex = this.widget.index;
            this.locked = false;
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            Main.ConnectApp.routeObserver.subscribe(this, (PageRoute)ModalRoute.of(context: this.context));
        }

        public override void dispose() {
            Main.ConnectApp.routeObserver.unsubscribe(this);
            base.dispose();
        }

        Widget _buildItem(string url) {
            return new ImageWrapper(url: url,
                headers: this.widget.headers ?? this._defaultHeaders,
                useCachedNetworkImage: this.widget.useCachedNetworkImage,
                imageData: this.widget.imageData?.getOrDefault(key: url, null),
                maxScale: this.widget.maxScale,
                minScale: this.widget.minScale,
                onScaleChanged: (scale, horizontalScale, verticalScale, position, scaling) => {
                    this.setState(() => { this.locked = horizontalScale > 1.0f; });
                },
                placeholder: new CustomActivityIndicator(loadingColor: LoadingColor.white));
        }

        public override Widget build(BuildContext context) {
            var children = new List<Widget>();
            foreach (var widgetURL in this.widget.urls) {
                children.Add(this._buildItem(url: widgetURL));
            }

            var pageView = new PageView(
                controller: this.widget.controller,
                physics: this.locked ? (ScrollPhysics)new NeverScrollableScrollPhysics() : new PageScrollPhysics(),
                onPageChanged: index => this.setState(() => this.currentIndex = index),
                children: children
            );
            return new GestureDetector(
                onTap: () => Navigator.pop(context: this.context),
                onLongPress: this._pickImage,
                child: new Container(
                    color: CColors.Black,
                    child: new Stack(
                        alignment: Alignment.center,
                        children: new List<Widget> {
                            pageView,
                            new Positioned(
                                bottom: 30,
                                child: new Container(
                                    height: 40,
                                    padding: EdgeInsets.symmetric(0, 24),
                                    alignment: Alignment.center,
                                    decoration: new BoxDecoration(
                                        Color.fromRGBO(0, 0, 0, 0.5f),
                                        borderRadius: BorderRadius.all(20)
                                    ),
                                    child: new Text(
                                        $"{this.currentIndex + 1}/{this.widget.urls.Count}",
                                        style: CTextStyle.PLargeWhite.defaultHeight()
                                    )
                                )
                            )
                        }
                    )
                )
            );
        }

        void _pickImage() {
            var imageUrl = this.widget.urls[index: this.currentIndex];
            var imagePath = SQLiteDBManager.instance.GetCachedFilePath(url: imageUrl);
            if (imagePath.isEmpty()) {
                Debug.Log("imagePath not found");
                return;
            }

            var imageBytes = CImageUtils.readImage(path: imagePath);

            var items = new List<ActionSheetItem> {
                new ActionSheetItem(
                    "保存图片",
                    onTap: () => {
                        //var imageStr = CImageUtils.readImage(path: imagePath);
                        PickImagePlugin.SaveImage(imagePath: imagePath, image: imageBytes);
                    }
                ),
                new ActionSheetItem("取消", type: ActionType.cancel)
            };

            //Load Image as Texture2D
            var texture2DForRecognition = new Texture2D(1, 1);
            texture2DForRecognition.LoadImage(data: imageBytes);
            //Recognition the QR code
            var result = new BarcodeReader { AutoRotate = false }.Decode(texture2DForRecognition.GetPixels32(),
                width: texture2DForRecognition.width, height: texture2DForRecognition.height);
            if (result != null) {
                //Insert QR Option to the Popup Bottom Widget
                items.Insert(0,
                    new ActionSheetItem(
                        "识别二维码",
                        onTap: () => {
                            //Jump to the Url recognized from the QR code .
                            try {
                                OpenUrlUtil.OpenUrl(buildContext: this.context, url: result.Text);
                            }
                            catch (Exception exception) {
                                CustomDialogUtils.showToast(context: this.context, message: result.Text,
                                    iconData: CIcons.sentiment_satisfied);
                                Debug.Log("QR Code Recognition:" + result.Text);
                                Debug.LogError(message: exception.Message);
                            }
                        }
                    )
                );
            }

            ActionSheetUtils.showModalActionSheet(context: this.context,
                new ActionSheet(
                    items: items
                ));
        }
    }

    public class ImageWrapper : StatefulWidget {
        public readonly float deceleration;
        public readonly Dictionary<string, string> headers;
        public readonly byte[] imageData;
        public readonly float maxScale;
        public readonly float maxVelocity;
        public readonly float minScale;
        public readonly OnScaleChangedCallback onScaleChanged;
        public readonly Widget placeholder;

        public readonly string url;
        public readonly bool useCachedNetworkImage;

        public ImageWrapper(
            Key key = null,
            string url = null,
            byte[] imageData = null,
            float maxScale = 2.0f,
            float minScale = 1.0f,
            bool useCachedNetworkImage = false,
            float deceleration = 1000,
            float maxVelocity = 1000,
            Dictionary<string, string> headers = null,
            OnScaleChangedCallback onScaleChanged = null,
            Widget placeholder = null) : base(key: key) {
            D.assert(minScale >= 0.0f && minScale <= 1.0f);
            D.assert(maxScale >= 1.0f);
            this.url = url;
            this.useCachedNetworkImage = useCachedNetworkImage;
            this.imageData = imageData;
            this.headers = headers;
            this.maxScale = maxScale;
            this.minScale = minScale;
            this.deceleration = deceleration;
            this.maxVelocity = maxVelocity;
            this.onScaleChanged = onScaleChanged;
            this.placeholder = placeholder;
        }

        public override State createState() {
            return new _ImageWrapperState();
        }
    }

    class _ImageWrapperState : TickerProviderStateMixin<ImageWrapper> {
        Size _cachedSize;
        ImageInfo _imageInfo;
        ImageStream _imageStream;
        Animation<Offset> _inertiaAnimation;
        AnimationController _inertiaAnimationController;
        Offset _initialPosition = Offset.zero;
        float _initialScale = 1.0f;
        bool _panning;
        Animation<Offset> _positionAnimation;
        AnimationController _positionAnimationController;
        Animation<float> _scaleAnimation;
        AnimationController _scaleAnimationController;
        bool _scaling;

        float _scale {
            get { return this._scaleAnimation.value; }
        }

        Offset _position {
            get { return this._clampPosition(position: this._positionAnimation.value, scale: this._scale); }
        }

        float _effectiveMaxScale {
            get {
                return this.widget.maxScale * this._originalImageScale
                    .clamp(1, max: float.PositiveInfinity);
            }
        }

        float _effectiveMinScale {
            get { return this.widget.minScale; }
        }

        float _originalImageScale {
            get {
                return this._imageInfo != null
                    ? this._size.width * this._imageInfo.image.height >
                      this._imageInfo.image.width * this._size.height
                        ? this._imageInfo.image.height / this._size.height
                        : this._imageInfo.image.width / this._size.width
                    : 1.0f;
            }
        }

        Size _size {
            get {
                if (this._cachedSize == null) {
                    this._cachedSize = MediaQuery.of(context: this.context).size;
                }

                return this._cachedSize;
            }
        }

        float _initialHorizontalScale {
            get {
                return this._imageInfo != null &&
                       this._size.width * this._imageInfo.image.height >
                       this._imageInfo.image.width * this._size.height
                    ? this._imageInfo.image.width * this._size.height /
                      (this._size.width * this._imageInfo.image.height)
                    : 1.0f;
            }
        }

        float _initialVerticalScale {
            get {
                return this._imageInfo != null &&
                       this._size.width * this._imageInfo.image.height <
                       this._imageInfo.image.width * this._size.height
                    ? this._size.width * this._imageInfo.image.height /
                      (this._imageInfo.image.width * this._size.height)
                    : 1.0f;
            }
        }

        public override void initState() {
            base.initState();
            this._scaleAnimationController = new AnimationController(
                duration: TimeSpan.FromMilliseconds(100),
                vsync: this);
            this._scaleAnimationController.addListener(listener: this._onScaleAndPositionChanged);
            this._scaleAnimation = new FloatTween(1.0f, 1.0f).animate(parent: this._scaleAnimationController);
            this._positionAnimationController = new AnimationController(
                duration: TimeSpan.FromMilliseconds(100),
                vsync: this);
            this._positionAnimation =
                new OffsetTween(begin: Offset.zero, end: Offset.zero).animate(parent: this._scaleAnimationController);
            this._positionAnimationController.addListener(
                listener: this._onScaleAndPositionChanged
            );
            this._inertiaAnimationController = new AnimationController(
                duration: TimeSpan.FromMilliseconds(1000),
                vsync: this);
            this._inertiaAnimationController.addListener(listener: this._onScaleAndPositionChanged);
            this._inertiaAnimationController.addStatusListener(status => {
                if (status == AnimationStatus.completed) {
                    var clampedPosition = this._clampPosition(position: this._position, scale: this._scale);
                    this._positionAnimation =
                        new OffsetTween(begin: clampedPosition, end: clampedPosition).animate(
                            parent: this._positionAnimationController);
                    this._positionAnimationController.reset();
                }
            });
            if (!this.widget.useCachedNetworkImage) {
                SchedulerBinding.instance.addPostFrameCallback(_ => {
                    this._imageStream = new NetworkImage(url: this.widget.url,
                        headers: this.widget.headers).resolve(ImageUtils.createLocalImageConfiguration(
                        context: this.context
                    ));
                    this._imageStream.addListener(new ImageStreamListener((imageInfo, __) => {
                        this.setState(() => { this._onImageResolved(imageInfo: imageInfo); });
                    }));
                });
            }
        }

        public override void dispose() {
            this._scaleAnimationController.dispose();
            this._positionAnimationController.dispose();
            this._inertiaAnimationController.dispose();
            base.dispose();
        }

        public override Widget build(BuildContext context) {
            var result = this.widget.imageData == null
                ? this.widget.useCachedNetworkImage
                    ? new CachedNetworkImage.CachedNetworkImage(
                        src: this.widget.url,
                        placeholder: this.widget.placeholder,
                        fit: BoxFit.contain,
                        headers: this.widget.headers,
                        onImageResolved: this._onImageResolved)
                    : this._imageInfo != null
                        ? new RawImage(image: this._imageInfo.image, fit: BoxFit.contain)
                        : this.widget.placeholder
                : Image.memory(bytes: this.widget.imageData, fit: BoxFit.contain);

            result = new ScaleTransition(
                scale: this._scaleAnimation, child: result);

            result = new FractionalTranslation(
                translation: this._position,
                child: result);

            result = new GestureDetector(
                onScaleStart: this._onScaleStart,
                onScaleUpdate: this._onScaleUpdate,
                onScaleEnd: this._onScaleEnd,
                onDoubleTap: this._onDoubleTap,
                child: result);

            result = new Hero(
                tag: this.widget.url,
                flightShuttleBuilder: (_1, _2, _3, _4, _5) => { return result; },
                child: result);

            return result;
        }

        void _onScaleAndPositionChanged() {
            this.setState(() => { });
            if (this.widget.onScaleChanged != null) {
                this.widget.onScaleChanged(
                    scale: this._scale,
                    this._scale * this._initialHorizontalScale,
                    this._scale * this._initialVerticalScale,
                    position: this._position,
                    scaling: this._scaling);
            }
        }

        void _onImageResolved(ImageInfo imageInfo) {
            this._imageInfo = imageInfo;
        }

        Offset _toFractional(Offset offset) {
            return this._size != null
                ? new Offset(offset.dx / this._size.width, offset.dy / this._size.height)
                : Offset.zero;
        }

        Offset _screenToFractional(Offset offset) {
            return this._toFractional(offset: offset) - new Offset(0.5f, 0.5f);
        }

        Offset _clampPosition(Offset position, float scale) {
            if (scale <= 1.0f) {
                return Offset.zero;
            }

            var horizontalMax = scale * this._initialHorizontalScale > 1.0f
                ? (scale * this._initialHorizontalScale - 1.0f) / 2
                : 0;
            var verticalMax = scale * this._initialVerticalScale > 1.0f
                ? (scale * this._initialVerticalScale - 1.0f) / 2
                : 0;
            return new Offset(position.dx.clamp(min: -horizontalMax, max: horizontalMax),
                position.dy.clamp(min: -verticalMax, max: verticalMax));
        }

        void _scaleAndMoveTo(float targetScale, Offset targetPosition) {
            this._scaleAnimation = new FloatTween(
                    begin: this._scale,
                    end: targetScale)
                .animate(parent: this._scaleAnimationController);

            this._positionAnimation = new OffsetTween(
                    begin: this._position,
                    end: targetPosition)
                .animate(parent: this._positionAnimationController);
        }

        void _scaleAndMoveToClamped(float targetScale, Offset targetPosition) {
            this._scaleAndMoveTo(targetScale: targetScale,
                this._clampPosition(position: targetPosition, scale: targetScale));
        }

        void _setScaleAndPosition(float scale, Offset position) {
            this._scaleAnimation = new FloatTween(begin: scale, end: scale)
                .animate(parent: this._scaleAnimationController);
            this._positionAnimation = new OffsetTween(begin: position, end: position)
                .animate(parent: this._positionAnimationController);
            this._scaleAnimationController.reset();
            this._positionAnimationController.reset();
        }

        Offset _computeNewPositionAfterScaleTo(float targetScale, Offset focalPoint) {
            return (this._position - focalPoint) * targetScale / this._scale + focalPoint;
        }

        void _onDoubleTap() {
            // TODO: Finn DoubleTapDetails
            // if (this._scale < this._originalImageScale ||
            //     this._originalImageScale < 1.0f && this._scale > this._originalImageScale) {
            //     this._scaleAndMoveToClamped(
            //         targetScale: this._originalImageScale,
            //         this._computeNewPositionAfterScaleTo(targetScale: this._originalImageScale,
            //             this._screenToFractional(offset: doubleTapDetails.firstGlobalPosition)));
            // }
            // else {
            //     this._scaleAndMoveTo(1, targetPosition: Offset.zero);
            // }
            this._scaleAndMoveTo(1, targetPosition: Offset.zero);

            this._scaleAnimationController.setValue(0);
            this._scaleAnimationController.animateTo(1);
            this._positionAnimationController.setValue(0);
            this._positionAnimationController.animateTo(1);
        }

        void _onScaleStart(ScaleStartDetails scaleStartDetails) {
            this._initialScale = this._scale;
            this._initialPosition = this._screenToFractional(offset: scaleStartDetails.focalPoint) - this._position;
            this._scaling = true;
        }

        void _onScaleUpdate(ScaleUpdateDetails scaleUpdateDetails) {
            this._setScaleAndPosition(
                this._initialScale * scaleUpdateDetails.scale,
                this._clampPosition(
                    this._screenToFractional(offset: scaleUpdateDetails.focalPoint) -
                    this._initialPosition * scaleUpdateDetails.scale,
                    this._initialScale * scaleUpdateDetails.scale)
            );
            this._panning = scaleUpdateDetails.scale == 1;
        }

        void _onScaleEnd(ScaleEndDetails scaleEndDetails) {
            if (this._scale > this._effectiveMaxScale) {
                this._scaleAndMoveToClamped(
                    targetScale: this._effectiveMaxScale,
                    this._position + this._initialPosition *
                    (this._scale - this._effectiveMaxScale) / this._initialScale);
                this._positionAnimationController.setValue(0);
                this._positionAnimationController.animateTo(1);
            }
            else if (this._scale < this._effectiveMinScale) {
                this._scaleAndMoveTo(targetScale: this._effectiveMinScale, targetPosition: Offset.zero);
                this._positionAnimationController.setValue(0);
                this._positionAnimationController.animateTo(1);
            }
            else {
                if (this._panning && scaleEndDetails.velocity != null) {
                    var velocity = scaleEndDetails.velocity.clampMagnitude(0, maxValue: this.widget.maxVelocity)
                        .pixelsPerSecond;
                    var duration = velocity.distance / this.widget.deceleration;
                    velocity = this._toFractional(offset: velocity);
                    this._inertiaAnimationController.duration = TimeSpan.FromSeconds(value: duration);
                    this._positionAnimation = new OffsetTween(
                            begin: this._position,
                            this._position + velocity * duration / 2)
                        .animate(parent: this._inertiaAnimationController);
                    this._inertiaAnimationController.setValue(0);
                    this._inertiaAnimationController.animateTo(1, curve: Curves.decelerate);
                }
            }

            this._scaleAnimationController.setValue(0);
            this._scaleAnimationController.animateTo(1);
            this._panning = false;
            this._scaling = false;
        }
    }
}