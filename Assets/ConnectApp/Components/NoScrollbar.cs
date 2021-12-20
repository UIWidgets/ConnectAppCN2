using Unity.UIWidgets.foundation;
using Unity.UIWidgets.widgets;

namespace ConnectApp.Components {
    public class NoScrollbar : StatelessWidget {
        public NoScrollbar(
            Widget child,
            Key key = null
        ) : base(key: key) {
            this.child = child;
        }

        readonly Widget child;

        public override Widget build(BuildContext context) {
            return new NotificationListener<ScrollNotification>(
                onNotification: _ => true,
                child: this.child
            );
        }
    }
}