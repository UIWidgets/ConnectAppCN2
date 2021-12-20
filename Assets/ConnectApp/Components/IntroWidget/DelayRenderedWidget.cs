using System;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace ConnectApp.Components.IntroWidget
{
    /// Delayed rendering class, used to achieve [Intro] animation effect
    /// For internal use of classes, developers don’t need to care
    ///
    public class DelayRenderedWidget : StatefulWidget
    {

        public DelayRenderedWidget(
            TimeSpan duration,
            Widget child,
            bool removed = false,
            bool childPersist = false,
            Key key = null
        ) : base(key: key)
        {
            this.duration = duration;
            this.child = child;
            this.removed = removed;
            this.childPersist = childPersist;
        }
        
        /// Sub-elements that need to fade in and out
        internal Widget child;

        /// [child] Whether to continue rendering, that is, the animation will only be once
        internal bool childPersist;

        /// Animation duration
        internal TimeSpan duration;

        /// [child] need to be removed (hidden)
        internal bool removed;
        
        public override State createState()
        {
            return new _DelayRenderedWidgetState();
        }
    }

    class _DelayRenderedWidgetState : State<DelayRenderedWidget>
    {
        float opacity = 0;
        Widget child;
        Timer timer;
        static readonly TimeSpan durationInterval = TimeSpan.FromMilliseconds(100);

        public override void initState()
        {
            base.initState();
            this.child = this.widget.child;
            this.timer = Timer.create(duration: durationInterval, () =>
            {
                if (this.mounted)
                {
                    this.opacity = 1;
                    this.setState();
                }
            });
        }

        public override void dispose()
        {
            this.timer?.cancel();
            this.timer = null;
            base.dispose();
        }

        public override void didUpdateWidget(StatefulWidget oldWidget)
        {
            base.didUpdateWidget(oldWidget);
            if (oldWidget is DelayRenderedWidget renderedWidget)
            {
                var duration = this.widget.duration;
                if (this.widget.removed)
                {
                    this.opacity = 0;
                    this.setState(() => {});
                    return;
                }

                if (!Equals(renderedWidget.child, this.widget.child))
                {
                    if (this.widget.childPersist)
                    {
                        this.child = this.widget.child;
                        this.setState(() => {});
                    }
                    else
                    {
                        this.opacity = 0;
                        this.setState(() => {});
                        Timer.create(TimeSpan.FromMilliseconds(duration.TotalMilliseconds + durationInterval.TotalMilliseconds),
                            () =>
                            {
                                this.child = this.widget.child;
                                this.opacity = 1;
                                this.setState(() => {});
                            }
                        );
                    }
                }
            }
        }

        public override Widget build(BuildContext context)
        {
            return new AnimatedOpacity(
                opacity: this.opacity,
                duration: this.widget.duration,
                child: this.child
            );
        }
    }
}