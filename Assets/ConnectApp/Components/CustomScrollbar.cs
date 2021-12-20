using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace ConnectApp.Components {
    static class CustomScrollbarUtils {
        public const float _kScrollbarMinLength = 36.0f;
        public const float _kScrollbarMinOverscrollLength = 8.0f;

        public static readonly TimeSpan _kScrollbarTimeToFade = TimeSpan.FromMilliseconds(1200);
        public static readonly TimeSpan _kScrollbarFadeDuration = TimeSpan.FromMilliseconds(250);
        public static readonly TimeSpan _kScrollbarResizeDuration = TimeSpan.FromMilliseconds(100);

        public static readonly Color _kScrollbarColor = new Color(0x99777777);

        public const float _kScrollbarThickness = 2.5f;
        public const float _kScrollbarThicknessDragging = 8.0f;
        public static readonly Radius _kScrollbarRadius = Radius.circular(1.5f);
        public static readonly Radius _kScrollbarRadiusDragging = Radius.circular(4.0f);
        public const float _kScrollbarMainAxisMargin = 3.0f;
        public const float _kScrollbarCrossAxisMargin = 3.0f;

        public static bool _hitTestInteractive(GlobalKey customPaintKey, Offset offset) {
            if (customPaintKey.currentContext == null) {
                return false;
            }

            var customPaint = customPaintKey.currentContext.widget as CustomPaint;
            var painter = customPaint.foregroundPainter as ScrollbarPainter;
            var renderBox = customPaintKey.currentContext.findRenderObject() as RenderBox;
            var localOffset = renderBox.globalToLocal(point: offset);
            return painter.hitTestInteractive(position: localOffset);
        }
    }

    public class CustomScrollbar : StatefulWidget {
        public CustomScrollbar(
            Key key = null,
            ScrollController controller = null,
            bool isAlwaysShown = false,
            Widget child = null
        ) : base(key: key) {
            this.child = child;
            this.controller = controller;
            this.isAlwaysShown = isAlwaysShown;
        }

        public readonly Widget child;
        public readonly ScrollController controller;
        public readonly bool isAlwaysShown;

        public override State createState() {
            return new _CustomScrollbarState();
        }
    }

    class _CustomScrollbarState : TickerProviderStateMixin<CustomScrollbar> {
        readonly GlobalKey _customPaintKey = GlobalKey.key();
        ScrollbarPainter _painter;
        TextDirection _textDirection;
        AnimationController _fadeoutAnimationController;
        Animation<float> _fadeoutOpacityAnimation;
        AnimationController _thicknessAnimationController;

        float _dragScrollbarPositionY;
        Timer _fadeoutTimer;
        Drag _drag;

        float _thickness {
            get {
                return CustomScrollbarUtils._kScrollbarThickness + this._thicknessAnimationController.value *
                    (CustomScrollbarUtils._kScrollbarThicknessDragging - CustomScrollbarUtils._kScrollbarThickness);
            }
        }

        Radius _radius {
            get {
                return Radius.lerp(a: CustomScrollbarUtils._kScrollbarRadius,
                    b: CustomScrollbarUtils._kScrollbarRadiusDragging, t: this._thicknessAnimationController.value);
            }
        }

        ScrollController _currentController;

        ScrollController _controller {
            get { return this.widget.controller ?? PrimaryScrollController.of(context: this.context); }
        }

        public override void initState() {
            base.initState();
            this._fadeoutAnimationController = new AnimationController(
                vsync: this,
                duration: CustomScrollbarUtils._kScrollbarFadeDuration
            );
            this._fadeoutOpacityAnimation = new CurvedAnimation(
                parent: this._fadeoutAnimationController,
                curve: Curves.fastOutSlowIn
            );
            this._thicknessAnimationController = new AnimationController(
                vsync: this,
                duration: CustomScrollbarUtils._kScrollbarResizeDuration
            );
            this._thicknessAnimationController.addListener(() => {
                this._painter.updateThickness(nextThickness: this._thickness, nextRadius: this._radius);
            });
        }

        public override void didChangeDependencies() {
            base.didChangeDependencies();
            if (this._painter == null) {
                this._painter = this._buildCustomScrollbarPainter(context: this.context);
            }
            else {
                this._painter.textDirection = Directionality.of(context: this.context);
                this._painter.color = CustomScrollbarUtils._kScrollbarColor;
                this._painter.padding = MediaQuery.of(context: this.context).padding;
            }

            WidgetsBinding.instance.addPostFrameCallback((TimeSpan duration) => {
                if (this.widget.isAlwaysShown) {
                    D.assert(this.widget.controller != null);
                    this.widget.controller.position.didUpdateScrollPositionBy(0);
                }
            });
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            oldWidget = (CustomScrollbar) oldWidget;
            base.didUpdateWidget(oldWidget: oldWidget);
            if (this.widget.isAlwaysShown != ((CustomScrollbar) oldWidget).isAlwaysShown) {
                if (this.widget.isAlwaysShown == true) {
                    D.assert(this.widget.controller != null);
                    this._fadeoutAnimationController.animateTo(1.0f);
                }
                else {
                    this._fadeoutAnimationController.reverse();
                }
            }
        }

        public ScrollbarPainter _buildCustomScrollbarPainter(BuildContext context) {
            return new ScrollbarPainter(
                color: CustomScrollbarUtils._kScrollbarColor,
                Directionality.of(context: context),
                thickness: this._thickness,
                fadeoutOpacityAnimation: this._fadeoutOpacityAnimation,
                mainAxisMargin: CustomScrollbarUtils._kScrollbarMainAxisMargin,
                crossAxisMargin: CustomScrollbarUtils._kScrollbarCrossAxisMargin,
                radius: this._radius,
                padding: MediaQuery.of(context: context).padding,
                minLength: CustomScrollbarUtils._kScrollbarMinLength,
                minOverscrollLength: CustomScrollbarUtils._kScrollbarMinOverscrollLength
            );
        }

        void _dragScrollbar(float primaryDelta) {
            D.assert(this._currentController != null);

            var scrollOffsetLocal = this._painter.getTrackToScroll(thumbOffsetLocal: primaryDelta);
            var scrollOffsetGlobal = scrollOffsetLocal + this._currentController.position.pixels;

            if (this._drag == null) {
                this._drag = this._currentController.position.drag(
                    new DragStartDetails(
                        globalPosition: new Offset(0.0f, dy: scrollOffsetGlobal)
                    ),
                    () => { }
                );
            }
            else {
                this._drag.update(
                    new DragUpdateDetails(
                        delta: new Offset(0.0f, dy: -scrollOffsetLocal),
                        primaryDelta: (float?) -1f * scrollOffsetLocal,
                        globalPosition: new Offset(0.0f, dy: scrollOffsetGlobal)
                    )
                );
            }
        }

        void _startFadeoutTimer() {
            if (!this.widget.isAlwaysShown) {
                this._fadeoutTimer?.cancel();
                this._fadeoutTimer = Timer.create(duration: CustomScrollbarUtils._kScrollbarTimeToFade, () => {
                    this._fadeoutAnimationController.reverse();
                    this._fadeoutTimer = null;
                });
            }
        }

        bool _checkVertical() {
            return this._currentController?.position?.axis() == Axis.vertical;
        }

        float _pressStartY = 0.0f;

        void _handleLongPressStart(LongPressStartDetails details) {
            this._currentController = this._controller;
            if (!this._checkVertical()) {
                return;
            }

            this._pressStartY = details.localPosition.dy;
            this._fadeoutTimer?.cancel();
            this._fadeoutAnimationController.forward();
            this._dragScrollbar(primaryDelta: details.localPosition.dy);
            this._dragScrollbarPositionY = details.localPosition.dy;
        }

        void _handleLongPress() {
            if (!this._checkVertical()) {
                return;
            }

            this._fadeoutTimer?.cancel();
            this._thicknessAnimationController.forward().then(
                (_) => { return; }
            );
        }

        void _handleLongPressMoveUpdate(LongPressMoveUpdateDetails details) {
            if (!this._checkVertical()) {
                return;
            }

            this._dragScrollbar(details.localPosition.dy - this._dragScrollbarPositionY);
            this._dragScrollbarPositionY = details.localPosition.dy;
        }

        void _handleLongPressEnd(LongPressEndDetails details) {
            if (!this._checkVertical()) {
                return;
            }

            this._handleDragScrollEnd(trackVelocityY: details.velocity.pixelsPerSecond.dy);
            if (details.velocity.pixelsPerSecond.dy.abs() < 10 &&
                (details.localPosition.dy - this._pressStartY).abs() > 0) {
                //HapticFeedback.mediumImpact();
            }

            this._currentController = null;
        }

        void _handleDragScrollEnd(float trackVelocityY) {
            this._startFadeoutTimer();
            this._thicknessAnimationController.reverse();
            this._dragScrollbarPositionY = 0.0f;
            var scrollVelocityY = this._painter.getTrackToScroll(thumbOffsetLocal: trackVelocityY);
            this._drag?.end(new DragEndDetails(
                primaryVelocity: -scrollVelocityY,
                velocity: new Velocity(
                    new Offset(
                        0.0f,
                        dy: -scrollVelocityY
                    )
                )
            ));
            this._drag = null;
        }

        bool _handleScrollNotification(ScrollNotification notification) {
            var metrics = notification.metrics;
            if (metrics.maxScrollExtent <= metrics.minScrollExtent) {
                return false;
            }

            if (notification is ScrollUpdateNotification ||
                notification is OverscrollNotification) {
                if (this._fadeoutAnimationController.status != AnimationStatus.forward) {
                    this._fadeoutAnimationController.forward();
                }

                this._fadeoutTimer?.cancel();
                this._painter.update(metrics: notification.metrics, axisDirection: notification.metrics.axisDirection);
            }

            return false;
        }

        Dictionary<Type, GestureRecognizerFactory> _gestures {
            get {
                var gestures =
                    new Dictionary<Type, GestureRecognizerFactory>();

                gestures[typeof(_ThumbPressGestureRecognizer)] =
                    new GestureRecognizerFactoryWithHandlers<_ThumbPressGestureRecognizer>(
                        () => new _ThumbPressGestureRecognizer(
                            debugOwner: this,
                            customPaintKey: this._customPaintKey
                        ),
                        (_ThumbPressGestureRecognizer instance) => {
                            instance.onLongPressStart = this._handleLongPressStart;
                            instance.onLongPress = this._handleLongPress;
                            instance.onLongPressMoveUpdate = this._handleLongPressMoveUpdate;
                            instance.onLongPressEnd = this._handleLongPressEnd;
                        }
                    );

                return gestures;
            }
        }

        public override void dispose() {
            this._fadeoutAnimationController.dispose();
            this._thicknessAnimationController.dispose();
            this._fadeoutTimer?.cancel();
            this._painter.dispose();
            base.dispose();
        }

        ScrollbarPainter _buildCustomScrollbarPainter() {
            return new ScrollbarPainter(
                color: CustomScrollbarUtils._kScrollbarColor,
                textDirection: this._textDirection,
                thickness: CustomScrollbarUtils._kScrollbarThickness,
                fadeoutOpacityAnimation: this._fadeoutOpacityAnimation,
                mainAxisMargin: CustomScrollbarUtils._kScrollbarMainAxisMargin,
                crossAxisMargin: CustomScrollbarUtils._kScrollbarCrossAxisMargin,
                radius: CustomScrollbarUtils._kScrollbarRadius,
                minLength: CustomScrollbarUtils._kScrollbarMinLength,
                minOverscrollLength: CustomScrollbarUtils._kScrollbarMinOverscrollLength
            );
        }

        public override Widget build(BuildContext context) {
            return new NotificationListener<ScrollNotification>(
                onNotification: this._handleScrollNotification,
                child: new RepaintBoundary(
                    child: new RawGestureDetector(
                        gestures: this._gestures,
                        child: new CustomPaint(
                            key: this._customPaintKey,
                            foregroundPainter: this._painter,
                            child: new RepaintBoundary(child: this.widget.child)
                        )
                    )
                )
            );
        }
    }

    public class _ThumbPressGestureRecognizer : LongPressGestureRecognizer {
        public _ThumbPressGestureRecognizer(
            float? postAcceptSlopTolerance = null,
            PointerDeviceKind kind = default,
            object debugOwner = null,
            GlobalKey customPaintKey = null
        ) : base(
            postAcceptSlopTolerance: postAcceptSlopTolerance,
            kind: kind,
            debugOwner: debugOwner,
            duration: TimeSpan.FromMilliseconds(100)
        ) {
            this._customPaintKey = customPaintKey;
        }


        public readonly GlobalKey _customPaintKey;

        protected override bool isPointerAllowed(PointerDownEvent _event) {
            if (!CustomScrollbarUtils._hitTestInteractive(customPaintKey: this._customPaintKey, offset: _event.position)
            ) {
                return false;
            }

            return base.isPointerAllowed(evt: _event);
        }
    }
}