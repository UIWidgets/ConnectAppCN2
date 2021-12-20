using ConnectApp.Common.Visual;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace ConnectApp.Components {
    public enum CheckboxShape
    {
        Circle,
        Square
    }
    public class CustomCheckbox : StatefulWidget {
        public CustomCheckbox(
            bool value,
            ValueChanged<bool> onChanged,
            Color activeColor = null,
            Color inactiveColor = null,
            float size = 12,
            EdgeInsets padding = null,
            CheckboxShape shape = CheckboxShape.Circle,
            Key key = null
        ) : base(key: key) {
            this.value = value;
            this.onChanged = onChanged;
            this.activeColor = activeColor ?? CColors.PrimaryBlue;
            this.inactiveColor = inactiveColor ?? CColors.White;
            this.size = size;
            this.padding = padding ?? EdgeInsets.all(8);
            this.shape = shape;
        }

        public readonly bool value;
        public readonly ValueChanged<bool> onChanged;
        public readonly Color activeColor;
        public readonly Color inactiveColor;
        public readonly float size;
        public readonly EdgeInsets padding;
        public readonly CheckboxShape shape;

        public override State createState() {
            return new _CustomCheckboxState();
        }
    }

    class _CustomCheckboxState : State<CustomCheckbox> {
        bool _value;

        public override void initState() {
            base.initState();
            this._value = this.widget.value;
        }

        public override void didUpdateWidget(StatefulWidget oldWidget) {
            base.didUpdateWidget(oldWidget: oldWidget);
            if (oldWidget is CustomCheckbox customCheckbox) {
                if (this.widget.value != customCheckbox.value) {
                    this.setState(() => this._value = this.widget.value);
                }
            }
        }

        public override Widget build(BuildContext context) {
            var child = this._value ? this._buildCheckbox() : this._buildUnCheckbox();

            return new CustomButton(
                onPressed: () => {
                    this.widget.onChanged?.Invoke(value: !this._value);
                },
                padding: this.widget.padding,
                child: child
            );
        }

        Widget _buildCheckbox() {
            return new Container(
                width: this.widget.size,
                height: this.widget.size,
                decoration: new BoxDecoration(
                    color: this.widget.activeColor,
                    borderRadius: this.widget.shape == CheckboxShape.Circle 
                        ? BorderRadius.circular(this.widget.size / 2) 
                        : BorderRadius.circular(2)
                ),
                child: new Icon(
                    icon: CIcons.check,
                    size: this.widget.size - 4,
                    color: CColors.White
                )
            );
        }

        Widget _buildUnCheckbox() {
            return new Container(
                width: this.widget.size,
                height: this.widget.size,
                decoration: new BoxDecoration(
                    color: this.widget.inactiveColor,
                    borderRadius: this.widget.shape == CheckboxShape.Circle 
                        ? BorderRadius.circular(this.widget.size / 2) 
                        : BorderRadius.circular(2),
                    border: Border.all(
                        color: CColors.LightBlueGrey
                    )
                )
            );
        }
    }
}