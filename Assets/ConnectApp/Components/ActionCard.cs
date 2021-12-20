using System.Collections.Generic;
using ConnectApp.Common.Visual;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace ConnectApp.Components {
    public class ActionCard : StatelessWidget {
        public ActionCard(
            IconData iconData,
            string title,
            bool done,
            GestureTapCallback onTap = null,
            Key key = null
        ) : base(key: key) {
            this.title = title;
            this.iconData = iconData;
            this.done = done;
            this.onTap = onTap;
        }

        readonly IconData iconData;
        readonly string title;
        readonly bool done;
        readonly GestureTapCallback onTap;

        public override Widget build(BuildContext context) {
            var iconColor = this.done ? CColors.PrimaryBlue : CColors.LightBlueGrey;
            var textColor = this.done ? CColors.PrimaryBlue : CColors.TextBody2;
            var child = new Container(
                decoration: new BoxDecoration(
                    color: CColors.Transparent,
                    borderRadius: BorderRadius.circular(4),
                    border: Border.all(color: CColors.Separator)
                ),
                width: 100,
                height: 40,
                child: new Row(
                    mainAxisAlignment: MainAxisAlignment.center,
                    crossAxisAlignment: CrossAxisAlignment.center,
                    children: new List<Widget> {
                        new Container(
                            margin: EdgeInsets.only(right: 10),
                            child: new Icon(icon: this.iconData, color: iconColor)),
                        new Text(data: this.title,
                            style: new TextStyle(
                                height: 1.09f,
                                fontSize: 16,
                                fontFamily: "Roboto-Regular",
                                color: textColor
                            )
                        )
                    }
                )
            );
            return new GestureDetector(
                onTap: this.onTap,
                child: child
            );
        }
    }
}