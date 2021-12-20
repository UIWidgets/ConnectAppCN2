using System;
using System.Collections.Generic;
using Unity.UIWidgets.animation;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace ConnectApp.Components {
    public class TextSwitch : StatefulWidget {
        public TextSwitch(
            bool value,
            ValueChanged<bool> onChanged,
            string activeText = "On",
            string inactiveText = "Off",
            Color activeColor = null,
            Color inactiveColor = null,
            Color activeTextColor = null,
            Color inactiveTextColor = null,
            Key key = null
        ) : base(key: key) {
            this.value = value;
            this.onChanged = onChanged;
            this.activeText = activeText;
            this.inactiveText = inactiveText;
            this.activeColor = activeColor;
            this.inactiveColor = inactiveColor;
            this.activeTextColor = activeTextColor;
            this.inactiveTextColor = inactiveTextColor;
        }

        public bool value;
        public ValueChanged<bool> onChanged;
        public readonly string activeText;
        public readonly string inactiveText;
        public readonly Color activeColor;
        public readonly Color inactiveColor;
        public readonly Color activeTextColor;
        public readonly Color inactiveTextColor;

        public override State createState() {
            return new _TextSwitch();
        }
    }

    class _TextSwitch : SingleTickerProviderStateMixin<TextSwitch> {
        AlignmentTween _circleAnimation;
        AnimationController _animationController;

        public override void initState() {
            base.initState();
            this._animationController =
                new AnimationController(vsync: this, duration: TimeSpan.FromMilliseconds(60));
            this._circleAnimation = new AlignmentTween(
                this.widget.value ? Alignment.centerRight : Alignment.centerLeft,
                this.widget.value ? Alignment.centerLeft : Alignment.centerRight
            );
        }

        public override Widget build(BuildContext context) {
            return new GestureDetector(
                onTap: () => {
                    // if (this._animationController.isCompleted) {
                    //     this._animationController.reverse();
                    // } else {
                    //     this._animationController.forward();
                    // }
                    this.widget.onChanged(this.widget.value == false);
                },
                child: new Container(
                    height: 28,
                    width: 96,
                    decoration: new BoxDecoration(
                        color: this.widget.inactiveColor,
                        borderRadius: BorderRadius.circular(14)
                    ),
                    child: new Stack(
                        children: new List<Widget> {
                            Positioned.fill(
                                new Row(
                                    children: new List<Widget> {
                                        new Container(
                                            width: 48,
                                            height: 28,
                                            alignment: Alignment.center,
                                            decoration: new BoxDecoration(
                                                color: this.widget.inactiveColor,
                                                borderRadius: BorderRadius.circular(14)
                                            ),
                                            child: new Text(
                                                data: this.widget.inactiveText,
                                                style: new TextStyle(
                                                    fontSize: 12,
                                                    fontFamily: "Roboto-Medium",
                                                    color: this.widget.inactiveTextColor
                                                )
                                            )
                                        ),
                                        new Container(
                                            width: 48,
                                            height: 28,
                                            alignment: Alignment.center,
                                            decoration: new BoxDecoration(
                                                color: this.widget.inactiveColor,
                                                borderRadius: BorderRadius.circular(14)
                                            ),
                                            child: new Text(
                                                data: this.widget.activeText,
                                                style: new TextStyle(
                                                    fontSize: 12,
                                                    fontFamily: "Roboto-Medium",
                                                    color: this.widget.inactiveTextColor
                                                )
                                            )
                                        )
                                    }
                                )
                            ),
                            new Positioned(
                                left: this.widget.value ? (float?) null : 0,
                                right: this.widget.value ? 0 : (float?) null,
                                child: new Container(
                                    width: 48,
                                    height: 28,
                                    alignment: Alignment.center,
                                    decoration: new BoxDecoration(
                                        color: this.widget.activeColor,
                                        border: new Border(
                                            new BorderSide(width: 2, color: this.widget.inactiveColor),
                                            new BorderSide(width: 2, color: this.widget.inactiveColor),
                                            new BorderSide(width: 2, color: this.widget.inactiveColor),
                                            new BorderSide(width: 2, color: this.widget.inactiveColor)
                                        ),
                                        borderRadius: BorderRadius.circular(14)
                                    ),
                                    child: new Text(
                                        this.widget.value ? this.widget.activeText : this.widget.inactiveText,
                                        style: new TextStyle(
                                            fontSize: 12,
                                            fontFamily: "Roboto-Medium",
                                            color: this.widget.activeTextColor
                                        )
                                    )
                                )
                            )
                        }
                    )
                )
            );
        }
    }
}