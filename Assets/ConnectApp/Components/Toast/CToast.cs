using System;
using System.Collections.Generic;
using ConnectApp.Common.Visual;
using JetBrains.Annotations;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace ConnectApp.Components.Toast
{
    /// ToastGravity
    /// Used to define the position of the Toast on the screen
    public enum ToastGravity
    {
        TOP,
        BOTTOM,
        CENTER,
        TOP_LEFT,
        TOP_RIGHT,
        BOTTOM_LEFT,
        BOTTOM_RIGHT,
        CENTER_LEFT,
        CENTER_RIGHT,
        SNACKBAR
    }

    public delegate Widget PositionedToastBuilder(BuildContext context, Widget child);
    
    /// Works with all platforms just in two lines of code
    /// CToast.init(context)
    /// CToast.instance.showToast(child)
    /// 
    public class CToast
    {
        static readonly CToast _instance = new CToast();
        public static CToast instance {
            get { return _instance; }
        }

        public static void init(BuildContext context)
        {
            _instance.context = context;
        }

        [CanBeNull] BuildContext context;
        [CanBeNull] OverlayEntry _entry;
        [CanBeNull] Timer _timer;
        List<ToastEntry> _overlayQueue = new List<ToastEntry>();

        void _showOverlay()
        {
            if (this._overlayQueue.Count == 0)
            {
                this._entry = null;
                return;
            }

            var toastEntry = this._overlayQueue.first();
            this._overlayQueue.RemoveAt(0);
            this._entry = toastEntry.entry;
            if (this.context == null)
            {
                throw new UIWidgetsError("Error: Context is null, Please call init(context) before showing toast.");
            }

            Overlay.of(context: this.context).insert(entry: this._entry);
            this._timer = Timer.create(duration: toastEntry.duration,
                () => { Future.delayed(TimeSpan.FromMilliseconds(360)).then(_ => this.removeQueuedCustomToasts()); });
        }

        void removeCustomToast()
        {
            this._timer?.cancel();
            this._timer = null;
            if (this._entry != null)
            {
                this._entry.remove();
            }

            this._showOverlay();
        }

        void removeQueuedCustomToasts()
        {
            this._timer?.cancel();
            this._timer = null;
            this._overlayQueue.Clear();
            if (this._entry != null)
            {
                this._entry.remove();
            }

            this._entry = null;
        }

        public void showToast(
            Widget child,
            [CanBeNull] PositionedToastBuilder positionedToastBuilder = null,
            TimeSpan? toastDuration = null,
            ToastGravity? gravity = null
        )
        {
            var newChild = new ToastWidget(child: child, toastDuration ?? TimeSpan.FromSeconds(1));
            var newEntry = new OverlayEntry(buildContext =>
            {
                if (positionedToastBuilder != null)
                {
                    return positionedToastBuilder(context: this.context, child: newChild);
                }

                return this._getPositionWidgetBasedOnGravity(child: newChild, gravity: gravity);
            });
            this._overlayQueue.Add(new ToastEntry(entry: newEntry, toastDuration ?? TimeSpan.FromSeconds(1)));
            if (this._timer == null)
            {
                this._showOverlay();
            }
        }

        Widget _getPositionWidgetBasedOnGravity(Widget child, ToastGravity? gravity)
        {
            switch (gravity)
            {
                case ToastGravity.TOP:
                    return new Positioned(top: 100, left: 24, right: 24, child: child);
                case ToastGravity.TOP_LEFT:
                    return new Positioned(top: 100, left: 24, child: child);
                case ToastGravity.TOP_RIGHT:
                    return new Positioned(top: 100, right: 24, child: child);
                case ToastGravity.CENTER:
                    return new Positioned(
                        top: 50, bottom: 50, left: 24, right: 24, child: child);
                case ToastGravity.CENTER_LEFT:
                    return new Positioned(top: 50, bottom: 50, left: 24, child: child);
                case ToastGravity.CENTER_RIGHT:
                    return new Positioned(top: 50, bottom: 50, right: 24, child: child);
                case ToastGravity.BOTTOM_LEFT:
                    return new Positioned(bottom: 50, left: 24, child: child);
                case ToastGravity.BOTTOM_RIGHT:
                    return new Positioned(bottom: 50, right: 24, child: child);
                case ToastGravity.SNACKBAR:
                    return new Positioned(
                        bottom: MediaQuery.of(context: this.context).viewInsets.bottom,
                        left: 0,
                        right: 0,
                        child: child);
                default:
                    return new Positioned(bottom: 50, left: 24, right: 24, child: child);
            }
        }
    }

    class ToastEntry
    {
        public ToastEntry(
            OverlayEntry entry,
            TimeSpan? duration
        )
        {
            this.entry = entry;
            this.duration = duration ?? TimeSpan.FromSeconds(1);
        }

        [CanBeNull] public readonly OverlayEntry entry;
        public readonly TimeSpan duration;
    }

    public class ToastWidget : StatefulWidget
    {
        public ToastWidget(
            Widget child,
            TimeSpan? duration = null,
            Key key = null
        ) : base(key: key)
        {
            D.assert(child != null);
            this.child = child;
            this.duration = duration ?? TimeSpan.FromSeconds(1);
        }

        public readonly Widget child;
        public readonly TimeSpan duration;

        public override State createState()
        {
            return new _ToastWidget();
        }
    }

    public class _ToastWidget : SingleTickerProviderStateMixin<ToastWidget>
    {
        AnimationController _animationController;
        CurvedAnimation _fadeAnimation;
        [CanBeNull] Timer _timer;

        void showIt()
        {
            this._animationController.forward();
        }

        void hideIt()
        {
            this._animationController.reverse();
            this._timer?.cancel();
        }

        public override void initState()
        {
            this._animationController = new AnimationController(
                vsync: this,
                duration: TimeSpan.FromMilliseconds(350)
            );
            this._fadeAnimation = new CurvedAnimation(parent: this._animationController, curve: Curves.easeIn);
            base.initState();
            this.showIt();
            this._timer = Timer.create(duration: this.widget.duration, () => { this.hideIt(); });
        }

        public override void deactivate()
        {
            this._timer?.cancel();
            this._animationController?.stop();
            base.deactivate();
        }

        public override void dispose()
        {
            this._timer?.cancel();
            this._animationController?.stop();
            base.dispose();
        }

        public override Widget build(BuildContext context)
        {
            return new FadeTransition(
                opacity: this._fadeAnimation,
                child: new Center(
                    child: new Container(
                        color: CColors.Transparent,
                        child: this.widget.child
                    )
                )
            );
        }
    }
}