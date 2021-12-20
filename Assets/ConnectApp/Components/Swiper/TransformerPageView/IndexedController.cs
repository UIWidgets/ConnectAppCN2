using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;

namespace ConnectApp.Components.Swiper {
    public class IndexController : ChangeNotifier {
        public const int NEXT = 1;
        public const int PREVIOUS = -1;
        public const int MOVE = 0;

        Completer _completer;

        public int index;
        public bool? animation;
        public int evt;

        public Future move(int index, bool animation = true) {
            this.animation = animation;
            this.index = index;
            this.evt = MOVE;
            this._completer = Completer.create();
            this.notifyListeners();
            return this._completer.future;
        }

        public Future next(bool animation = true) {
            this.evt = NEXT;
            this.animation = animation;
            this._completer = Completer.create();
            this.notifyListeners();
            return this._completer.future;
        }

        public Future previous(bool animation = true) {
            this.evt = PREVIOUS;
            this.animation = animation;
            this._completer = Completer.create();
            this.notifyListeners();
            return this._completer.future;
        }

        public void complete() {
            if (!this._completer.isCompleted) {
                this._completer.complete();
            }
        }
    }
}