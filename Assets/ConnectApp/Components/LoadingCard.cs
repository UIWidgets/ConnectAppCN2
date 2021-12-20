using System.Collections.Generic;
using ConnectApp.Common.Visual;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;

namespace ConnectApp.Components {
    public class ArticleLoadingCard : StatelessWidget {
        public override Widget build(BuildContext context) {
            return new Container(
                color: CColors.White,
                height: 162,
                padding: EdgeInsets.only(16, 16, 16),
                child: new Column(
                    children: new List<Widget> {
                        new Container(
                            height: 16,
                            margin: EdgeInsets.only(bottom: 16),
                            color: CColors.LoadingBar
                        ),
                        new Row(
                            children: new List<Widget> {
                                new Expanded(
                                    child: new Column(
                                        children: new List<Widget> {
                                            new Container(
                                                height: 6,
                                                color: CColors.LoadingBar
                                            ),
                                            new Container(
                                                height: 6,
                                                margin: EdgeInsets.only(top: 16),
                                                color: CColors.LoadingBar
                                            )
                                        }
                                    )
                                ),
                                new Container(
                                    width: 100,
                                    height: 66,
                                    margin: EdgeInsets.only(16),
                                    color: CColors.LoadingBar
                                )
                            }
                        )
                    }
                )
            );
        }
    }

    public class QuestionLoadingCard : StatelessWidget {
        public override Widget build(BuildContext context) {
            return new Container(
                color: CColors.White,
                height: 160,
                child: new Column(
                    children: new List<Widget> {
                        new Container(
                            height: 12,
                            margin: EdgeInsets.symmetric(horizontal: 16),
                            color: CColors.LoadingBar
                        ),
                        new Container(
                            height: 12,
                            margin: EdgeInsets.only(16, 16, 126),
                            color: CColors.LoadingBar
                        ),
                        new Container(
                            height: 8,
                            margin: EdgeInsets.only(16, 32, 76),
                            color: CColors.LoadingBar
                        ),
                        new Container(
                            height: 8,
                            margin: EdgeInsets.only(16, 16, 250, 24),
                            color: CColors.LoadingBar
                        ),
                        new Container(
                            height: 8,
                            color: CColors.BgGrey
                        )
                    }
                )
            );
        }
    }
}