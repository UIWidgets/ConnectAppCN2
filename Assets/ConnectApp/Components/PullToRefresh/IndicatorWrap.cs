using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.scheduler;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace ConnectApp.Components.PullToRefresh {
    public abstract class Wrapper : StatefulWidget {
        protected Wrapper(
            ValueNotifier<int> modeListener,
            bool up,
            IndicatorBuilder builder = null,
            float triggerDistance = 0,
            Key key = null
        ) : base(key: key) {
            this.modeListener = modeListener;
            this.up = up;
            this.builder = builder;
            this.triggerDistance = triggerDistance;
        }

        public readonly ValueNotifier<int> modeListener;

        public readonly IndicatorBuilder builder;

        public readonly bool up;

        public readonly float triggerDistance;

        public bool _isRefreshing {
            get { return this.mode == RefreshStatus.refreshing; }
        }

        public bool _isComplete {
            get {
                return this.mode != RefreshStatus.idle && this.mode != RefreshStatus.refreshing &&
                       this.mode != RefreshStatus.canRefresh;
            }
        }

        public int mode {
            get { return this.modeListener.value; }

            set { this.modeListener.value = value; }
        }

        public bool _isScrollToOutSide(ScrollNotification notification) {
            if (this.up) {
                if (notification.metrics.minScrollExtent - notification.metrics.pixels >
                    0) {
                    return true;
                }
            }
            else {
                if (notification.metrics.pixels - notification.metrics.maxScrollExtent >
                    0) {
                    return true;
                }
            }

            return false;
        }
    }

    public class RefreshWrapper : Wrapper {
        public RefreshWrapper(
            bool up,
            ValueNotifier<int> modeListener = null,
            int completeDuration = DefaultConstants.default_completeDuration,
            OnOffsetChange onOffsetChange = null,
            float visibleRange = DefaultConstants.default_VisibleRange,
            IndicatorBuilder builder = null,
            float triggerDistance = 0,
            Key key = null
        ) : base(modeListener: modeListener, up: up, builder: builder, triggerDistance: triggerDistance, key: key) {
            this.completeDuration = completeDuration;
            this.onOffsetChange = onOffsetChange;
            this.visibleRange = visibleRange;
        }

        public readonly int completeDuration;

        public readonly OnOffsetChange onOffsetChange;

        public readonly float visibleRange;

        public override State createState() {
            return new _RefreshWrapperState();
        }
    }

    public class _RefreshWrapperState : State<RefreshWrapper>, TickerProvider, GestureProcessor {
        AnimationController _sizeController;

        public override void initState() {
            base.initState();
            this._sizeController = new AnimationController(
                vsync: this,
                lowerBound: DefaultConstants.minSpace,
                duration: TimeSpan.FromMilliseconds(value: DefaultConstants.spaceAnimateMill));
            this._sizeController.addListener(listener: this._handleOffsetCallBack);
            this.widget.modeListener.addListener(listener: this._handleModeChange);
        }

        public override Widget build(BuildContext context) {
            if (this.widget.up) {
                return new Column(
                    children: new List<Widget> {
                        new SizeTransition(
                            sizeFactor: this._sizeController,
                            child: new Container(height: this.widget.visibleRange)
                        ),
                        this.widget.builder(context: context, mode: this.widget.mode)
                    }
                );
            }

            return new Column(
                children: new List<Widget> {
                    this.widget.builder(context: context, mode: this.widget.mode),
                    new SizeTransition(
                        sizeFactor: this._sizeController,
                        child: new Container(height: this.widget.visibleRange)
                    )
                }
            );
        }

        void _dismiss() {
            this._sizeController.animateTo(target: DefaultConstants.minSpace)
                .then(_ => { this.widget.mode = RefreshStatus.idle; });
        }

        public int mode {
            get { return this.widget.modeListener.value; }
        }

        float _measure(ScrollNotification notification) {
            if (this.widget.up) {
                return (notification.metrics.minScrollExtent -
                        notification.metrics.pixels) / this.widget.triggerDistance;
            }

            return (notification.metrics.pixels -
                    notification.metrics.maxScrollExtent) / this.widget.triggerDistance;
        }

        public Ticker createTicker(TickerCallback onTick) {
            return new Ticker(onTick: onTick, $"created by {this}");
        }

        public void onDragStart(ScrollStartNotification notification) {
//            throw new NotImplementedException();
        }

        public void onDragMove(ScrollUpdateNotification notification) {
            if (!this.widget._isScrollToOutSide(notification: notification)) {
                return;
            }

            if (this.widget._isComplete || this.widget._isRefreshing) {
                return;
            }

            var offset = this._measure(notification: notification);
            if (offset >= 1.0) {
                this.widget.mode = RefreshStatus.canRefresh;
            }
            else {
                this.widget.mode = RefreshStatus.idle;
            }
        }

        public void onDragEnd(ScrollNotification notification) {
            if (!this.widget._isScrollToOutSide(notification: notification)) {
                return;
            }

            if (this.widget._isComplete || this.widget._isRefreshing) {
                return;
            }

            var reachMax = this._measure(notification: notification) >= 1.0;
            if (!reachMax) {
                this._sizeController.animateTo(0.0f);
            }
            else {
                this.widget.mode = RefreshStatus.refreshing;
            }
        }

        void _handleOffsetCallBack() {
            this.widget.onOffsetChange?.Invoke(
                up: this.widget.up, this._sizeController.value * this.widget.visibleRange);
        }

        void _handleModeChange() {
            switch (this.mode) {
                case RefreshStatus.refreshing:
                    this._sizeController.setValue(1.0f);
                    break;
                case RefreshStatus.completed:
                    Future.delayed(
                        TimeSpan.FromMilliseconds(value: this.widget.completeDuration),
                        () => {
                            this._dismiss();
                            return default;
                        }
                    );
                    break;
                case RefreshStatus.failed:
                    Future.delayed(
                        TimeSpan.FromMilliseconds(value: this.widget.completeDuration),
                        () => {
                            this._dismiss();
                            return default;
                        }
                    ).then(_ => { this.widget.mode = RefreshStatus.idle; });
                    break;
            }

            this.setState(() => { });
        }

        public override void dispose() {
            this.widget.modeListener.removeListener(listener: this._handleModeChange);
            this._sizeController.removeListener(listener: this._handleOffsetCallBack);
            base.dispose();
        }
    }


    public class LoadWrapper : Wrapper {
        public LoadWrapper(
            bool up,
            ValueNotifier<int> modeListener = null,
            bool autoLoad = true,
            IndicatorBuilder builder = null,
            float triggerDistance = 0,
            Key key = null
        ) : base(modeListener: modeListener, up: up, builder: builder, triggerDistance: triggerDistance, key: key) {
            this.autoLoad = autoLoad;
        }

        public readonly bool autoLoad;

        public override State createState() {
            return new _LoadWrapperState();
        }
    }

    public class _LoadWrapperState : State<LoadWrapper>, GestureProcessor {
        VoidCallback _updateListener;


        public override void initState() {
            base.initState();
            this._updateListener = () => { this.setState(() => { }); };
            this.widget.modeListener.addListener(listener: this._updateListener);
        }

        public override Widget build(BuildContext context) {
            return this.widget.builder(context: context, mode: this.widget.mode);
        }


        public void onDragStart(ScrollStartNotification notification) {
//            throw new NotImplementedException();
        }

        public void onDragMove(ScrollUpdateNotification notification) {
            if (this.widget._isRefreshing || this.widget._isComplete) {
                return;
            }

            if (this.widget.autoLoad) {
                if (this.widget.up &&
                    notification.metrics.extentBefore() <= this.widget.triggerDistance) {
                    this.widget.mode = RefreshStatus.refreshing;
                }

                if (!this.widget.up &&
                    notification.metrics.extentAfter() <= this.widget.triggerDistance) {
                    this.widget.mode = RefreshStatus.refreshing;
                }
            }
        }

        public void onDragEnd(ScrollNotification notification) {
            if (this.widget._isRefreshing || this.widget._isComplete) {
                return;
            }

            if (this.widget.autoLoad) {
                if (this.widget.up &&
                    notification.metrics.extentBefore() <= this.widget.triggerDistance) {
                    this.widget.mode = RefreshStatus.refreshing;
                }

                if (!this.widget.up &&
                    notification.metrics.extentAfter() <= this.widget.triggerDistance) {
                    this.widget.mode = RefreshStatus.refreshing;
                }
            }
        }


        public override void dispose() {
            this.widget.modeListener.removeListener(listener: this._updateListener);
            base.dispose();
        }
    }


    public interface GestureProcessor {
        void onDragStart(ScrollStartNotification notification);

        void onDragMove(ScrollUpdateNotification notification);

        void onDragEnd(ScrollNotification notification);
    }
}