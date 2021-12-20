using System;
using Unity.UIWidgets.async;

namespace ConnectApp.Components.IntroWidget
{
    public class Throttling
    {
        TimeSpan _duration;
        Timer _timer;

        public Throttling(
            TimeSpan duration
        ) {
            this._duration = duration;
        }

        public void throttle(Action func)
        {
            if (this._timer == null)
            {
                this._timer = Timer.create(duration: this._duration, () =>
                {
                    func.Invoke();
                    this._timer?.cancel();
                    this._timer = null;
                });
            }
        }
    }
}