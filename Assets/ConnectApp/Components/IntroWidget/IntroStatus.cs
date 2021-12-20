namespace ConnectApp.Components.IntroWidget
{
    public class IntroStatus
    {
        public IntroStatus(
            bool isOpen,
            int currentStepIndex
        )
        {
            this.isOpen = isOpen;
            this.currentStepIndex = currentStepIndex;
        }

        /// Intro is showing on the screen or not
        internal bool isOpen;

        /// Current step page index
        internal int currentStepIndex;

        public override string ToString()
        {
            return $"IntroStatus(isOpen: {this.isOpen}, currentStepIndex: {this.currentStepIndex})";
        }
    }
}