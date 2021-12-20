using System;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Models.Model;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Image = Unity.UIWidgets.widgets.Image;
using TextStyle = Unity.UIWidgets.painting.TextStyle;

namespace ConnectApp.Components {
    public enum OwnerType {
        user,
        team
    }

    public enum AvatarShape {
        circle,
        rect
    }

    public class Avatar : StatelessWidget {
        Avatar(
            string id,
            string avatarUrl,
            string fullName,
            float size = 36,
            OwnerType type = OwnerType.user,
            bool hasWhiteBorder = false,
            float whiteBorderWidth = DefaultWhiteBorderWidth,
            AvatarShape avatarShape = AvatarShape.circle,
            Key key = null
        ) : base(key: key) {
            this.id = id ?? "";
            this.avatarUrl = avatarUrl ?? "";
            this.fullName = fullName ?? "";
            this.size = size;
            this.type = type;
            this.hasWhiteBorder = hasWhiteBorder;
            this.whiteBorderWidth = whiteBorderWidth;
            this.avatarShape = avatarShape;
        }

        readonly string id;
        readonly string avatarUrl;
        readonly string fullName;
        readonly float size;
        readonly OwnerType type;
        readonly bool hasWhiteBorder;
        readonly float whiteBorderWidth;
        readonly AvatarShape avatarShape;

        const int DefaultWhiteBorderWidth = 2;
        const int DefaultRectCorner = 4;

        public static Avatar User(
            User user,
            float size,
            bool hasWhiteBorder = false,
            float whiteBorderWidth = DefaultWhiteBorderWidth,
            AvatarShape avatarShape = AvatarShape.circle,
            Key key = null
        ) {
            return new Avatar(
                id: user.id,
                avatarUrl: user.avatar,
                user.fullName ?? user.name,
                size: size,
                type: OwnerType.user,
                hasWhiteBorder: hasWhiteBorder,
                whiteBorderWidth: whiteBorderWidth,
                avatarShape: avatarShape,
                key: key
            );
        }

        public static Avatar Team(
            Team team,
            float size,
            bool hasWhiteBorder = false,
            float whiteBorderWidth = DefaultWhiteBorderWidth,
            AvatarShape avatarShape = AvatarShape.rect,
            Key key = null
        ) {
            return new Avatar(
                id: team.id,
                avatarUrl: team.avatar,
                fullName: team.name,
                size: size,
                type: OwnerType.team,
                hasWhiteBorder: hasWhiteBorder,
                whiteBorderWidth: whiteBorderWidth,
                avatarShape: avatarShape,
                key: key
            );
        }

        public override Widget build(BuildContext context) {
            var avatarSize = this.hasWhiteBorder ? this.size : this.size - this.whiteBorderWidth * 2;
            var border = this.hasWhiteBorder
                ? Border.all(
                    color: CColors.White,
                    width: this.whiteBorderWidth
                )
                : null;

            // fix Android 9 http request error 
            var httpsUrl = this.avatarUrl.httpToHttps();
            return new Container(
                width: this.size,
                height: this.size,
                decoration: new BoxDecoration(
                    borderRadius: BorderRadius.circular(this.avatarShape == AvatarShape.circle
                        ? this.size / 2
                        : DefaultRectCorner),
                    border: border
                ),
                child: new ClipRRect(
                    borderRadius: BorderRadius.circular(this.avatarShape == AvatarShape.circle
                        ? avatarSize
                        : this.hasWhiteBorder
                            ? DefaultRectCorner / 2
                            : DefaultRectCorner),
                    child: this.avatarUrl.isEmpty()
                        ? new Container(
                            child: new AvatarPlaceholder(
                                this.id ?? "",
                                this.fullName ?? "",
                                size: avatarSize
                            )
                        )
                        : new Container(
                            width: avatarSize,
                            height: avatarSize,
                            color: CColors.LoadingGrey,
                            child: Image.network(src: httpsUrl)
                        )
                )
            );
        }
    }

    public class AvatarPlaceholder : StatelessWidget {
        public AvatarPlaceholder(
            string id,
            string title,
            float size = 36,
            Key key = null
        ) : base(key: key) {
            D.assert(id != null);
            D.assert(title != null);
            this.id = id;
            this.title = title;
            this.size = size;
        }

        readonly string id;
        readonly string title;
        readonly float size;

        public override Widget build(BuildContext context) {
            var fontSize = (int) Math.Ceiling(this.size * 0.5f);
            var name = CStringUtils.genAvatarName(name: this.title);
            if (name.IsLetterOrNumber()) {
                fontSize = (int) Math.Ceiling(this.size * 0.4f);
            }

            return new Container(
                width: this.size,
                height: this.size,
                alignment: Alignment.center,
                color: CColorUtils.GetSpecificColorFromId(id: this.id),
                child: new Container(
                    alignment: Alignment.center,
                    child: new Text(
                        CStringUtils.genAvatarName(name: this.title),
                        textAlign: TextAlign.center,
                        style: new TextStyle(
                            color: CColors.White,
                            height: 1.15f,
                            fontFamily: "Roboto-Medium",
                            fontSize: fontSize
                        )
                    )
                )
            );
        }
    }
}