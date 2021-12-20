using System;
using System.Collections.Generic;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace ConnectApp.Components.IntroWidget
{
    class IntroPosition
    {
        public float? left;
        public float? top;
        public float? right;
        public float? bottom;
        public float width;
        public CrossAxisAlignment crossAxisAlignment;
    }
    
    /// The [WidgetBuilder] class that comes with Flutter Intro
    ///
    /// You can use the defaultTheme provided by Flutter Intro
    ///
    /// {@tool snippet}
    /// ```dart
    /// final Intro intro = Intro(
    ///   stepCount: 2,
    ///   widgetBuilder: StepWidgetBuilder.useDefaultTheme([
    ///     "say something",
    ///     "say other something",
    ///   ]),
    /// );
    /// ```
    /// {@end-tool}
    ///
    public class StepWidgetBuilder
    {
        static IntroPosition smartGetPosition(Size size, Size screenSize, Offset offset) =>
            _smartGetPosition(size: size, screenSize: screenSize, offset: offset);

        static IntroPosition _smartGetPosition(Size size, Size screenSize, Offset offset)
        {
            var height = size.height;
            var width = size.width;
            var screenWidth = screenSize.width;
            var screenHeight = screenSize.height;
            var bottomArea = screenHeight - offset.dy - height;
            var topArea = screenHeight - height - bottomArea;
            var rightArea = screenWidth - offset.dx - width;
            var leftArea = screenWidth - width - rightArea;
            var position = new IntroPosition();

            position.crossAxisAlignment = CrossAxisAlignment.start;

            if (topArea > bottomArea)
            {
                position.bottom = bottomArea + height + 16;
            }
            else
            {
                position.top = offset.dy + height + 12;
            }

            if (leftArea > rightArea)
            {
                position.right = rightArea <= 0 ? 16f : rightArea;
                position.crossAxisAlignment = CrossAxisAlignment.end;
                position.width = Math.Min(leftArea + width - 16, screenWidth * 0.618f);
            }
            else
            {
                position.left = offset.dx <= 0 ? 16f : offset.dx;
                position.width = Math.Min(rightArea + width - 16, screenWidth * 0.618f);
            }

            // The distance on the right side is very large, it is more beautiful on the right side
            if (rightArea > 0.8f * topArea && rightArea > 0.8f * bottomArea)
            {
                position.left = offset.dx + width + 16;
                position.top = offset.dy - 4;
                position.bottom = null;
                position.right = null;
                position.width = Math.Min(val1: position.width, rightArea * 0.8f);
            }

            // The distance on the left is large, it is more beautiful on the left side
            if (leftArea > 0.8 * topArea && leftArea > 0.8 * bottomArea)
            {
                position.right = rightArea + width + 16;
                position.top = offset.dy - 4;
                position.bottom = null;
                position.left = null;
                position.crossAxisAlignment = CrossAxisAlignment.end;
                position.width = Math.Min(val1: position.width, leftArea * 0.8f);
            }

            return position;
        }

        public static Func<StepWidgetParams, Widget> useAdvancedTheme(Func<StepWidgetParams, Widget> widgetBuilder)
        {
            return stepWidgetParams =>
            {
                var offset = stepWidgetParams.offset;

                var position = smartGetPosition(
                    screenSize: stepWidgetParams.screenSize,
                    size: stepWidgetParams.size,
                    offset: offset
                );
                
                return new Stack(
                    children: new List<Widget>
                    {
                        new Positioned(
                            left: position.left,
                            top: position.top,
                            bottom: position.bottom,
                            right: position.right,
                            child: new SizedBox(
                                width: position.width,
                                child: widgetBuilder(arg: stepWidgetParams)
                            )
                        )
                    }
                );
            };
        }
    }
}