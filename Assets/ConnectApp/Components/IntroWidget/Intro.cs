using System;
using System.Collections.Generic;
using ConnectApp.Common.Visual;
using Unity.UIWidgets.async;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Color = Unity.UIWidgets.ui.Color;

namespace ConnectApp.Components.IntroWidget
{
    public class IntroConfig
    {
        public EdgeInsets padding;
        public BorderRadius borderRadius;
    }
    public class Intro
    {
        public Intro(
            Func<StepWidgetParams, Widget> widgetBuilder,
            int stepCount,
            Action<IntroStatus> onHighlightWidgetTap = null,
            Color maskColor = null,
            bool noAnimation = false,
            bool maskClosable = false,
            BorderRadius borderRadius = null,
            EdgeInsets padding = null
        )
        {
            Debug.Assert(stepCount > 0, "stepCount must more than 0");
            this._animationDuration = noAnimation ? TimeSpan.Zero : TimeSpan.FromMilliseconds(300);
            for (var i = 0; i < stepCount; i++)
            {
                this._globalKeys.Add(GlobalKey.key());
                this._configMap.Add(new IntroConfig());
            }

            this.widgetBuilder = widgetBuilder;
            this.stepCount = stepCount;
            this.onHighlightWidgetTap = onHighlightWidgetTap;
            this.maskColor = maskColor ?? Color.fromRGBO(0, 0, 0, .6f);
            this.noAnimation = noAnimation;
            this.maskClosable = maskClosable;
            this.borderRadius = borderRadius ?? BorderRadius.all(Radius.circular(4));
            this.padding = padding ?? EdgeInsets.all(8);
        }
        
        /// The mask color of step page
        internal Color maskColor;

        /// Current step page index
        /// 2021-03-31 @caden
        /// I don’t remember why this parameter was exposed at the time,
        /// it seems to be useless, and there is a bug in this one, so let’s block it temporarily.
        // int get currentStepIndex => _currentStepIndex;

        /// No animation
        internal bool noAnimation;

        // Click on whether the mask is allowed to be closed.
        internal bool maskClosable;

        /// The method of generating the content of the guide page,
        /// which will be called internally by [Intro] when the guide page appears.
        /// And will pass in some parameters on the current page through [StepWidgetParams]
        internal Func<StepWidgetParams, Widget> widgetBuilder;

        /// [Widget] [padding] of the selected area, the default is [EdgeInsets.all(8)]
        internal EdgeInsets padding;

        /// [Widget] [borderRadius] of the selected area, the default is [BorderRadius.all(Radius.circular(4))]
        internal BorderRadius borderRadius;

        /// How many steps are there in total
        internal int stepCount;

        /// The highlight widget tapped callback
        internal Action<IntroStatus> onHighlightWidgetTap;

        bool _removed = false;
        float? _widgetWidth;
        float? _widgetHeight;
        Offset _widgetOffset;
        OverlayEntry _overlayEntry;
        int _currentStepIndex = 0;
        Widget _stepWidget;
        List<IntroConfig> _configMap = new List<IntroConfig>();
        List<GlobalKey> _globalKeys = new List<GlobalKey>();
        TimeSpan _animationDuration;
        Size _lastScreenSize;
        internal Throttling _th = new Throttling(TimeSpan.FromMilliseconds(500));

        public List<GlobalKey> keys {
            get { return this._globalKeys; }
        }

        /// Set the configuration of the specified number of steps
        ///
        /// [stepIndex] Which step of configuration needs to be modified
        /// [padding] Padding setting
        /// [borderRadius] BorderRadius setting
        public void setStepConfig(
            int stepIndex,
            EdgeInsets padding = null,
            BorderRadius borderRadius = null
        ) {
            Debug.Assert(stepIndex >= 0 && stepIndex < this.stepCount, "stepIndex error");
            this._configMap[index: stepIndex] = new IntroConfig
            {
                padding = padding,
                borderRadius = borderRadius
            };
        }

        /// Set the configuration of multiple steps
        ///
        /// [stepsIndex] Which steps of configuration needs to be modified
        /// [padding] Padding setting
        /// [borderRadius] BorderRadius setting
        public void setStepsConfig(
            List<int> stepsIndex,
            EdgeInsets padding = null,
            BorderRadius borderRadius = null
        )
        {
            // dart code
            // assert(stepsIndex
            //     .every((stepIndex) => stepIndex >= 0 && stepIndex < stepCount));
            foreach (var index in stepsIndex)
            {
                this.setStepConfig(stepIndex: index, padding: padding, borderRadius: borderRadius);
            }
        }
        
        void _getWidgetInfo(GlobalKey globalKey) {
            var currentConfigPadding = this._configMap[index: this._currentStepIndex].padding;
            var renderBox = globalKey.currentContext.findRenderObject() as RenderBox;
            this._widgetWidth = renderBox.size.width +
                                (currentConfigPadding?.horizontal ?? this.padding.horizontal);
            this._widgetHeight =
                renderBox.size.height + (currentConfigPadding?.vertical ?? this.padding.vertical);
            this._widgetOffset = new Offset(
                renderBox.localToGlobal(point: Offset.zero).dx -
                (currentConfigPadding?.left ?? this.padding.left),
                renderBox.localToGlobal(point: Offset.zero).dy -
                (currentConfigPadding?.top ?? this.padding.top)
            );
        }
        
        Widget _widgetBuilder(
            float left,
            float top,
            float? width = null,
            float? height = null,
            float? bottom = null,
            float? right = null,
            BlendMode? backgroundBlendMode = null,
            BorderRadius borderRadius = null,
            Widget child = null,
            VoidCallback onTap = null
        ) {
            var decoration = new BoxDecoration(
                color: CColors.White,
                backgroundBlendMode: backgroundBlendMode,
                borderRadius: borderRadius
            );
            return new AnimatedPositioned(
                duration: this._animationDuration,
                child: new GestureDetector(
                    onTap: () => onTap?.Invoke(),
                    child: new AnimatedContainer(
                        padding: this.padding,
                        decoration: decoration,
                        width: width,
                        height: height,
                        child: child,
                        duration: this._animationDuration
                    )
                ),
                left: left,
                top: top,
                bottom: bottom,
                right: right
            );
        }

        void _showOverlay(BuildContext context, GlobalKey globalKey)
        {
            this._overlayEntry = new OverlayEntry(
                buildContext =>
                {
                    var screenSize = MediaQuery.of(context: context).size;

                    if (screenSize.width != this._lastScreenSize.width ||
                        screenSize.height != this._lastScreenSize.height) {
                        this._lastScreenSize = screenSize;
                        this._th.throttle(() => {
                            this._createStepWidget(context: context);
                            this._overlayEntry.markNeedsBuild();
                        });
                    }

                    VoidCallback maskOnTap = null;
                    if (this.maskClosable)
                    {
                        maskOnTap = () =>
                        {
                            if (this.stepCount - 1 == this._currentStepIndex)
                            {
                                this._onFinish();
                            }
                            else
                            {
                                this._onNext(context: context);
                            }
                        };
                    }
                    
                    VoidCallback onHighlightTap = null;
                    if (this.onHighlightWidgetTap != null)
                    {
                        onHighlightTap = () =>
                        {
                            var introStatus = this.getStatus();
                            this.onHighlightWidgetTap(obj: introStatus);
                        };
                    }
                    
                    return new DelayRenderedWidget(
                        removed: this._removed,
                        childPersist: true,
                        duration: this._animationDuration,
                        child: new Container(
                            color: CColors.Transparent,
                            child: new Stack(
                                children: new List<Widget>
                                {
                                    new ColorFiltered(
                                        ColorFilter.mode(
                                            color: this.maskColor,
                                            blendMode: BlendMode.srcOut
                                        ),
                                        new Stack(
                                            children: new List<Widget>
                                            {
                                                this._widgetBuilder(
                                                    backgroundBlendMode: BlendMode.dstOut,
                                                    left: 0,
                                                    top: 0,
                                                    right: 0,
                                                    bottom: 0,
                                                    onTap: maskOnTap
                                                ),
                                                this._widgetBuilder(
                                                    width: this._widgetWidth,
                                                    height: this._widgetHeight,
                                                    left: this._widgetOffset.dx,
                                                    top: this._widgetOffset.dy,
                                                    // Skipping through the intro very fast may cause currentStepIndex to out of bounds
                                                    // I have tried to fix it, here is just to make the code safer
                                                    // https://github.com/tal-tech/flutter_intro/issues/22
                                                    borderRadius: this._currentStepIndex < this.stepCount 
                                                        ? this._configMap[index: this._currentStepIndex].borderRadius ?? this.borderRadius
                                                        : this.borderRadius,
                                                    onTap: onHighlightTap
                                                )
                                            }
                                        )
                                    ),
                                    new DelayRenderedWidget(
                                        duration: this._animationDuration,
                                        child: this._stepWidget
                                    )
                                }
                            )
                        )
                    );
                } 
            );
            Overlay.of(context: context).insert(entry: this._overlayEntry);
        }
        
        void _onNext(BuildContext context) {
            if (this._currentStepIndex + 1 < this.stepCount) {
                this._currentStepIndex++;
                this._renderStep(context: context);
            }
        }

        void _onPrev(BuildContext context) {
            if (this._currentStepIndex - 1 >= 0) {
                this._currentStepIndex--;
                this._renderStep(context: context);
            }
        }
        
        void _onFinish() {
            if (this._overlayEntry == null) return;
            this._removed = true;
            this._overlayEntry.markNeedsBuild();
            Timer.create(duration: this._animationDuration, () => {
                if (this._overlayEntry == null) return;
                this._overlayEntry.remove();
                this._overlayEntry = null;
            });
        }

        void _createStepWidget(BuildContext context)
        {
            this._getWidgetInfo(this._globalKeys[index: this._currentStepIndex]);
            var screenSize = MediaQuery.of(context: context).size;
            var widgetSize = new Size(this._widgetWidth != null ? this._widgetWidth.Value : 0, this._widgetHeight != null ? this._widgetHeight.Value : 0);
            VoidCallback onNext = null;
            if (this._currentStepIndex != this.stepCount - 1)
            {
                onNext = () => this._onNext(context: context);
            }

            VoidCallback onPrev = null;
            if (this._currentStepIndex != 0)
            {
                onPrev = () => this._onPrev(context: context);
            }

            this._stepWidget = this.widgetBuilder?.Invoke(
                new StepWidgetParams(
                    screenSize: screenSize,
                    size: widgetSize,
                    onNext: onNext,
                    onPrev: onPrev,
                    offset: this._widgetOffset,
                    currentStepIndex: this._currentStepIndex,
                    stepCount: this.stepCount,
                    onFinish: this._onFinish
                )
            );
        }
        
        void _renderStep(BuildContext context) {
            this._createStepWidget(context: context);
            this._overlayEntry.markNeedsBuild();
        }

        /// Trigger the start method of the guided operation
        ///
        /// [context] Current environment [BuildContext]
        public void start(BuildContext context) {
            this._lastScreenSize = MediaQuery.of(context: context).size;
            this._removed = false;
            this._currentStepIndex = 0;
            this._createStepWidget(context: context);
            this._showOverlay(
                context: context,
                this._globalKeys[index: this._currentStepIndex]
            );
        }
        
        /// Destroy the guide page and release all resources
        void dispose() {
            this._onFinish();
        }
        
        /// Get intro instance current status
        IntroStatus getStatus() {
            var isOpen = this._overlayEntry != null;
            var introStatus = new IntroStatus(
                isOpen: isOpen,
                currentStepIndex: this._currentStepIndex
            );
            return introStatus;
        }
    }
}