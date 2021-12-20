using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Models.Model;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.gestures;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace ConnectApp.Components {
    public class LearnCard : StatelessWidget {
        public LearnCard(
            LearnCourse model,
            GestureTapCallback onTap = null,
            Key key = null
        ) : base(key: key) {
            this.model = model;
            this.onTap = onTap;
        }

        readonly LearnCourse model;
        readonly GestureTapCallback onTap;

        public override Widget build(BuildContext context) {
            if (this.model == null) {
                return new Container();
            }

            const float imageWidth = 114;
            const float imageHeight = 76;
            const float borderRadius = 2;
            
            var imageUrl = this.model.thumbnail;

            var level = "";
            var skillLevel = this.model.skillLevel;
            if (skillLevel == "beginner") {
                level = "初级";
            }
            else if (skillLevel == "intermediate") {
                level = "中级";
            }
            else if (skillLevel == "advanced") {
                level = "高级";
            }
            
            var card = new Container(
                height: 108,
                padding: EdgeInsets.all(16),
                color: CColors.White,
                child: new Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: new List<Widget> {
                        new Expanded(
                            child: new Container(
                                margin: EdgeInsets.only(right: 8),
                                child: new Column(
                                    crossAxisAlignment: CrossAxisAlignment.start,
                                    children: new List<Widget> {
                                        new Container(
                                            margin: EdgeInsets.only(bottom: 8),
                                            child: new Text(
                                                data: this.model.title,
                                                style: CTextStyle.PLargeMedium,
                                                maxLines: 2,
                                                overflow: TextOverflow.ellipsis
                                            )
                                        ),
                                        new Container(
                                            child: new Text(
                                                level + " • " + this.model.duration + " 分钟",
                                                style: CTextStyle.PSmall.copyWith(color: CColors.TextBody4)
                                            )
                                        )
                                    }
                                )
                            )
                        ),
                        new PlaceholderImage(
                            CImageUtils.SuitableSizeImageUrl(imageWidth: imageWidth, imageUrl: imageUrl),
                            width: imageWidth,
                            height: imageHeight,
                            borderRadius: borderRadius,
                            fit: BoxFit.cover,
                            color: CColorUtils.GetSpecificDarkColorFromId(id: this.model.id)
                        )
                    }
                )
            );
            return new GestureDetector(
                child: card,
                onTap: this.onTap
            );
        }
    }
}