using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace ConnectApp.Components {
    public static class BlankImage {
        public const string blocked = "image/default/default-blocked.png";
        public const string comment = "image/default/default-comment.png";
        public const string common = "image/default/default-common.png";
        public const string events = "image/default/default-event.png";
        public const string follow = "image/default/default-follow.png";
        public const string network = "image/default/default-network.png";
        public const string notification = "image/default/default-notification.png";
        public const string search = "image/default/default-search.png";
    }

    public class BlankView : StatelessWidget {
        public BlankView(
            string title,
            string imageName = null,
            bool showButton = false,
            GestureTapCallback tapCallback = null,
            Decoration decoration = null,  
            string buttonName = null,
            Key key = null
        ) : base(key: key) {
            this.title = title;
            this.imageName = imageName;
            this.showButton = showButton;
            this.tapCallback = tapCallback;
            this.decoration = decoration ?? new BoxDecoration(color: CColors.White);
            this.buttonName = buttonName ?? "刷新";
        }

        readonly string title;
        readonly string imageName;
        readonly bool showButton;
        readonly GestureTapCallback tapCallback;
        readonly Decoration decoration;
        readonly string buttonName;

        public override Widget build(BuildContext context) {
            var imageName = HttpManager.isNetWorkError() ? BlankImage.network : this.imageName;
            var message = HttpManager.isNetWorkError() ? "数据不见了，快检查下网络吧" : $"{this.title}";
            return new Container(
                decoration: this.decoration,
                width: MediaQuery.of(context: context).size.width,
                child: new Column(
                    mainAxisAlignment: MainAxisAlignment.center,
                    crossAxisAlignment: CrossAxisAlignment.center,
                    children: new List<Widget> {
                        imageName != null
                            ? new Container(
                                margin: EdgeInsets.only(bottom: 8),
                                child: Image.file(
                                    file: imageName,
                                    width: 160,
                                    height: 160
                                )
                            )
                            : new Container(),
                        new Container(
                            margin: EdgeInsets.only(bottom: 24),
                            child: new Text(
                                data: message,
                                style: CTextStyle.PLargeBody5.defaultHeight()
                            )
                        ),
                        this._buildButton()
                    }
                )
            );
        }

        Widget _buildButton() {
            if (!this.showButton) {
                return new Container();
            }

            return new CustomButton(
                onPressed: this.tapCallback,
                padding: EdgeInsets.zero,
                child: new Container(
                    width: 128,
                    height: 48,
                    alignment: Alignment.center,
                    decoration: new BoxDecoration(
                        color: CColors.White,
                        borderRadius: BorderRadius.circular(24),
                        border: Border.all(color: CColors.PrimaryBlue)
                    ),
                    child: new Text(
                        data: this.buttonName,
                        style: CTextStyle.PLargeMediumBlue.defaultHeight()
                    )
                )
            );
        }
    }
}