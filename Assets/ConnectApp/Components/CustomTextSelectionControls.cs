using System.Collections.Generic;
using ConnectApp.Common.Visual;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.service;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using UnityEngine;
using Canvas = Unity.UIWidgets.ui.Canvas;
using Color = Unity.UIWidgets.ui.Color;
using Rect = Unity.UIWidgets.ui.Rect;
using TextStyle = Unity.UIWidgets.painting.TextStyle;
using Transform = Unity.UIWidgets.widgets.Transform;

namespace ConnectApp.Components {
    public static class CustomTextSelectionControlsUtils {
        public const float _kSelectionHandleOverlap = 1.5f;
        
        internal const float _kHandlesPadding = 18.0f;

        internal const float _kToolbarScreenPadding = 8.0f;

        internal const float _kToolbarHeight = 36.0f;
        
        public const float _kSelectionHandleRadius = 6;
        
        internal static readonly Color _kToolbarBackgroundColor = new Color(0xFF2E2E2E);

        internal static readonly Color _kToolbarDividerColor = new Color(0xFFB9B9B9);

        internal static readonly Color _kHandlesColor = new Color(0xFF136FE0);

        internal static readonly Size _kSelectionOffset = new Size(20.0f, 40.0f);

        internal static readonly Size _kToolbarTriangleSize = new Size(18.0f, 9.0f);

        internal static readonly EdgeInsets _kToolbarButtonPadding = EdgeInsets.symmetric(10.0f, 18.0f);

        internal static readonly BorderRadius _kToolbarBorderRadius = BorderRadius.all(Radius.circular(7.5f));

        internal static readonly TextStyle _kToolbarButtonFontStyle = new TextStyle(
            false,
            fontSize: 14.0f,
            letterSpacing: -0.11f,
            fontWeight: FontWeight.w300,
            fontFamily: "Roboto-Regular",
            color: CColors.White
        );
    }

    public enum ArrowDirection {
        up,
        down
    }

    public class TrianglePainter : AbstractCustomPainter {
        public TrianglePainter(
            ArrowDirection arrowDirection,
            Color color = null
        ) {
            this.arrowDirection = arrowDirection;
            this.color = color ?? CustomTextSelectionControlsUtils._kToolbarBackgroundColor;
        }

        readonly ArrowDirection arrowDirection;
        readonly Color color;

        public override void paint(Canvas canvas, Size size) {
            var paint = new Paint {
                color = this.color,
                style = PaintingStyle.fill
            };
            var triangleBottomY = this.arrowDirection == ArrowDirection.down
                ? 0.0f
                : CustomTextSelectionControlsUtils._kToolbarTriangleSize.height;
            var triangle = new Path();
            triangle.lineTo(CustomTextSelectionControlsUtils._kToolbarTriangleSize.width / 2, y: triangleBottomY);
            triangle.lineTo(0.0f, y: CustomTextSelectionControlsUtils._kToolbarTriangleSize.height);
            triangle.lineTo(-(CustomTextSelectionControlsUtils._kToolbarTriangleSize.width / 2), y: triangleBottomY);
            triangle.close();
            canvas.drawPath(path: triangle, paint: paint);
        }

        public override bool shouldRepaint(CustomPainter oldPainter) {
            return false;
        }
    }

    class _TextSelectionToolbar : StatelessWidget {
        public _TextSelectionToolbar(
            Key key = null,
            VoidCallback handleCut = null,
            VoidCallback handleCopy = null,
            VoidCallback handlePaste = null,
            VoidCallback handleSelectAll = null,
            ArrowDirection? arrowDirection = null
        ) : base(key: key) {
            this.handleCut = handleCut;
            this.handleCopy = handleCopy;
            this.handlePaste = handlePaste;
            this.handleSelectAll = handleSelectAll;
            this.arrowDirection = arrowDirection;
        }

        readonly VoidCallback handleCut;
        readonly VoidCallback handleCopy;
        readonly VoidCallback handlePaste;
        readonly VoidCallback handleSelectAll;
        readonly ArrowDirection? arrowDirection;

        public override Widget build(BuildContext context) {
            var items = new List<Widget>();
            Widget onePhysicalPixelVerticalDivider =
                new SizedBox(width: 1.0f / MediaQuery.of(context: context).devicePixelRatio);

            if (this.handleCut != null) {
                items.Add(_buildToolbarButton("剪切", onPressed: this.handleCut));
            }

            if (this.handleCopy != null) {
                if (items.isNotEmpty()) {
                    items.Add(item: onePhysicalPixelVerticalDivider);
                }

                items.Add(_buildToolbarButton("拷贝", onPressed: this.handleCopy));
            }

            if (this.handlePaste != null) {
                if (items.isNotEmpty()) {
                    items.Add(item: onePhysicalPixelVerticalDivider);
                }

                items.Add(_buildToolbarButton("粘贴", onPressed: this.handlePaste));
            }

            if (this.handleSelectAll != null) {
                if (items.isNotEmpty()) {
                    items.Add(item: onePhysicalPixelVerticalDivider);
                }

                items.Add(_buildToolbarButton("全选", onPressed: this.handleSelectAll));
            }

            Widget padding = new Padding(padding: EdgeInsets.only(bottom: 10.0f));

            Widget triangle = SizedBox.fromSize(
                size: CustomTextSelectionControlsUtils._kToolbarTriangleSize,
                child: new CustomPaint(
                    painter: new TrianglePainter((ArrowDirection) this.arrowDirection)
                )
            );

            Widget toolbar = new ClipRRect(
                borderRadius: CustomTextSelectionControlsUtils._kToolbarBorderRadius,
                child: new DecoratedBox(
                    decoration: new BoxDecoration(
                        color: CustomTextSelectionControlsUtils._kToolbarDividerColor,
                        borderRadius: CustomTextSelectionControlsUtils._kToolbarBorderRadius,
                        border: Border.all(color: CustomTextSelectionControlsUtils._kToolbarBackgroundColor, 0)
                    ),
                    child: new Row(mainAxisSize: MainAxisSize.min, children: items)
                )
            );

            var menus = this.arrowDirection == ArrowDirection.down
                ? new List<Widget> {toolbar, triangle, padding}
                : new List<Widget> {padding, triangle, toolbar};

            return new Column(
                mainAxisSize: MainAxisSize.min,
                children: menus
            );
        }

        static CustomButton _buildToolbarButton(string text, VoidCallback onPressed) {
            return new CustomButton(
                child: new Text(data: text, style: CustomTextSelectionControlsUtils._kToolbarButtonFontStyle),
                decoration: new BoxDecoration(color: CustomTextSelectionControlsUtils._kToolbarBackgroundColor),
                padding: CustomTextSelectionControlsUtils._kToolbarButtonPadding,
                onPressed: () => onPressed()
            );
        }
    }

    class _TextSelectionToolbarLayout : SingleChildLayoutDelegate {
        internal _TextSelectionToolbarLayout(
            Size screenSize = null,
            Rect globalEditableRegion = null,
            Offset position = null
        ) {
            this.screenSize = screenSize;
            this.globalEditableRegion = globalEditableRegion;
            this.position = position;
        }

        readonly Size screenSize;
        readonly Rect globalEditableRegion;
        readonly Offset position;

        public override BoxConstraints getConstraintsForChild(BoxConstraints constraints) {
            return constraints.loosen();
        }

        public override Offset getPositionForChild(Size size, Size childSize) {
            var globalPosition = this.globalEditableRegion.topLeft + this.position;

            var x = globalPosition.dx - childSize.width / 2.0f;
            var y = globalPosition.dy - childSize.height;

            if (x < CustomTextSelectionControlsUtils._kToolbarScreenPadding) {
                x = CustomTextSelectionControlsUtils._kToolbarScreenPadding;
            }
            else if (x + childSize.width >
                     this.screenSize.width - CustomTextSelectionControlsUtils._kToolbarScreenPadding) {
                x = this.screenSize.width - childSize.width - CustomTextSelectionControlsUtils._kToolbarScreenPadding;
            }

            if (y < CustomTextSelectionControlsUtils._kToolbarScreenPadding) {
                y = CustomTextSelectionControlsUtils._kToolbarScreenPadding;
            }
            else if (y + childSize.height >
                     this.screenSize.height - CustomTextSelectionControlsUtils._kToolbarScreenPadding) {
                y = this.screenSize.height - childSize.height - CustomTextSelectionControlsUtils._kToolbarScreenPadding;
            }

            return new Offset(dx: x, dy: y);
        }

        public override bool shouldRelayout(SingleChildLayoutDelegate oldDelegate) {
            var layout = (_TextSelectionToolbarLayout) oldDelegate;
            return this.screenSize != layout.screenSize
                   || this.globalEditableRegion != layout.globalEditableRegion
                   || this.position != layout.position;
        }
    }

    class _TextSelectionHandlePainter : AbstractCustomPainter {
        internal _TextSelectionHandlePainter(Offset origin) {
            this.origin = origin;
        }

        readonly Offset origin;

        public override void paint(Canvas canvas, Size size) {
            var paint = new Paint {
                color = CustomTextSelectionControlsUtils._kHandlesColor,
                strokeWidth = 2.0f
            };
            canvas.drawCircle(this.origin.translate(0.0f, 6.0f), 5.5f, paint: paint);
            canvas.drawLine(
                p1: this.origin,
                this.origin.translate(
                    0.0f,
                    -(size.height - 2.0f * CustomTextSelectionControlsUtils._kHandlesPadding)
                ),
                paint: paint
            );
        }

        public override bool shouldRepaint(CustomPainter oldPainter) {
            return this.origin != ((_TextSelectionHandlePainter) oldPainter).origin;
        }
    }

    public class CustomTextSelectionControls : TextSelectionControls {
        public override Size getHandleSize(float textLineHeight) {
            return CustomTextSelectionControlsUtils._kSelectionOffset;
        }

        public override Widget buildToolbar(BuildContext context, Rect globalEditableRegion, float textLineHeight,
            Offset position,
            List<TextSelectionPoint> endpoints, TextSelectionDelegate selectionDelegate) {
            var availableHeight
                = globalEditableRegion.top - MediaQuery.of(context: context).padding.top -
                  CustomTextSelectionControlsUtils._kToolbarScreenPadding;
            var direction = availableHeight > CustomTextSelectionControlsUtils._kToolbarHeight
                ? ArrowDirection.down
                : ArrowDirection.up;

            var y = direction == ArrowDirection.up
                ? globalEditableRegion.height + CustomTextSelectionControlsUtils._kToolbarHeight + 6.0f
                : 0.0f;
            return new ConstrainedBox(
                constraints: BoxConstraints.tight(size: globalEditableRegion.size),
                child: new CustomSingleChildLayout(
                    layoutDelegate: new _TextSelectionToolbarLayout(
                        screenSize: MediaQuery.of(context: context).size,
                        globalEditableRegion: globalEditableRegion,
                        new Offset(dx: position.dx, position.dy + y)
                    ),
                    child: new _TextSelectionToolbar(
                        handleCut: this.canCut(_delegate: selectionDelegate)
                            ? () => this.handleCut(selectionDelegate: selectionDelegate)
                            : (VoidCallback) null,
                        handleCopy: this.canCopy(_delegate: selectionDelegate)
                            ? () => this.handleCopy(selectionDelegate: selectionDelegate)
                            : (VoidCallback) null,
                        handlePaste: this.canPaste(_delegate: selectionDelegate)
                            ? () => this.handlePaste(selectionDelegate: selectionDelegate)
                            : (VoidCallback) null,
                        handleSelectAll: this.canSelectAll(_delegate: selectionDelegate)
                            ? () => this.handleSelectAll(selectionDelegate: selectionDelegate)
                            : (VoidCallback) null,
                        arrowDirection: direction
                    )
                )
            );
        }

        public override Widget buildHandle(BuildContext context, TextSelectionHandleType type, float textLineHeight) {
            var desiredSize = new Size(
                2.0f * CustomTextSelectionControlsUtils._kHandlesPadding,
                textLineHeight + 2.0f * CustomTextSelectionControlsUtils._kHandlesPadding
            );

            Widget handle = SizedBox.fromSize(
                size: desiredSize,
                child: new CustomPaint(
                    painter: new _TextSelectionHandlePainter(
                        new Offset(
                            dx: CustomTextSelectionControlsUtils._kHandlesPadding,
                            textLineHeight + CustomTextSelectionControlsUtils._kHandlesPadding
                        )
                    )
                )
            );

            switch (type) {
                case TextSelectionHandleType.left: {
                    var matrix4 = Matrix4.rotationZ(radians: Mathf.PI);
                    matrix4.multiply(
                        Matrix4.translationValues(
                            x: -CustomTextSelectionControlsUtils._kHandlesPadding,
                            y: -CustomTextSelectionControlsUtils._kHandlesPadding,
                            0
                        )
                    );
                    return new Transform(
                        transform: matrix4,
                        child: handle
                    );
                }
                case TextSelectionHandleType.right: {
                    return new Transform(
                        transform: Matrix4.translationValues(
                            x: -CustomTextSelectionControlsUtils._kHandlesPadding,
                            -(textLineHeight + CustomTextSelectionControlsUtils._kHandlesPadding),
                            0
                        ),
                        child: handle
                    );
                }
                case TextSelectionHandleType.collapsed:
                    return new Container();
                default:
                    return null;
            }
        }

        public override Offset getHandleAnchor(TextSelectionHandleType type, float textLineHeight) {
            var handleSize = this.getHandleSize(textLineHeight: textLineHeight);
            switch (type) {
            
                case TextSelectionHandleType.left:
                    return new Offset(
                        handleSize.width / 2,
                        handleSize.height
                    );
             
                case TextSelectionHandleType.right:
                    return new Offset(
                        handleSize.width / 2,
                        handleSize.height - 2 * CustomTextSelectionControlsUtils._kSelectionHandleRadius + CustomTextSelectionControlsUtils._kSelectionHandleOverlap
                    );
              
                default:
                    return new Offset(
                        handleSize.width / 2,
                        textLineHeight + (handleSize.height - textLineHeight) / 2
                    );
            }
        }
    }
}