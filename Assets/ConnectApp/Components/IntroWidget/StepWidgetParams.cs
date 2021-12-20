using Unity.UIWidgets.ui;

namespace ConnectApp.Components.IntroWidget
{
    public class StepWidgetParams
    {
        public StepWidgetParams(
            VoidCallback onFinish,
            int currentStepIndex,
            int stepCount,
            Size screenSize,
            Size size,
            Offset offset,
            VoidCallback onPrev,
            VoidCallback onNext
        )
        {
            this.onFinish = onFinish;
            this.currentStepIndex = currentStepIndex;
            this.stepCount = stepCount;
            this.screenSize = screenSize;
            this.size = size;
            this.offset = offset;
            this.onPrev = onPrev;
            this.onNext = onNext;
        }

        /// Return to the previous guide page method, or null if there is none
        internal VoidCallback onPrev;

        /// Enter the next guide page method, or null if there is none
        internal VoidCallback onNext;

        /// End all guide page methods
        internal VoidCallback onFinish;

        /// Which guide page is currently displayed, starting from 0
        internal int currentStepIndex;

        /// Total number of guide pages
        internal int stepCount;

        /// The width and height of the screen
        internal Size screenSize;

        /// The width and height of the highlighted component
        internal Size size;

        /// The coordinates of the upper left corner of the highlighted component
        internal Offset offset;

        public override string ToString()
        {
            return
                $"StepWidgetParams(currentStepIndex: {this.currentStepIndex}, stepCount: {this.stepCount}, size: {this.size}, screenSize: {this.screenSize}, offset: {this.offset})";
        }
    }
}