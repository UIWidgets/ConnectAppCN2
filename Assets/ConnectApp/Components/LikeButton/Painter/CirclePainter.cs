using ConnectApp.Components.LikeButton.Utils;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace ConnectApp.Components.LikeButton.Painter {
    public class CirclePainter : AbstractCustomPainter {
        readonly Paint circlePaint = new Paint();
        readonly Paint maskPaint = new Paint();

        public CirclePainter(
            float outerCircleRadiusProgress,
            float innerCircleRadiusProgress,
            CircleColor circleColor = null
        ) {
            this.outerCircleRadiusProgress = outerCircleRadiusProgress;
            this.innerCircleRadiusProgress = innerCircleRadiusProgress;
            this.circleColor = circleColor ?? new CircleColor(new Color(0xFFFF5722), new Color(0xFFFFC107));
            this.circlePaint.style = PaintingStyle.fill;
            this.maskPaint.blendMode = BlendMode.clear;
        }

        readonly float outerCircleRadiusProgress;
        readonly float innerCircleRadiusProgress;
        readonly CircleColor circleColor;

        public override void paint(Canvas canvas, Size size) {
            var center = size.width * 0.5f;
            this._updateCircleColor();
            canvas.saveLayer(Offset.zero & size, new Paint());
            canvas.drawCircle(new Offset(dx: center, dy: center), this.outerCircleRadiusProgress * center,
                paint: this.circlePaint);
            canvas.drawCircle(new Offset(dx: center, dy: center), this.innerCircleRadiusProgress * center + 1,
                paint: this.maskPaint);
            canvas.restore();
        }

        void _updateCircleColor() {
            var colorProgress = LikeButtonUtil.clamp(value: this.outerCircleRadiusProgress, 0.5f, 1);
            colorProgress = LikeButtonUtil.mapValueFromRangeToRange(value: colorProgress, 0.5f, 1, 0, 1);
            this.circlePaint.color = Color.lerp(a: this.circleColor.start, b: this.circleColor.end, t: colorProgress);
        }

        public override bool shouldRepaint(CustomPainter oldDelegate) {
            return true;
        }
    }
}