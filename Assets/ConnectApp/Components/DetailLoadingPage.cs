using System.Collections.Generic;
using ConnectApp.Common.Visual;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace ConnectApp.Components {
    public class ArticleDetailLoading : StatelessWidget {
        public override Widget build(BuildContext context) {
            return new Container(
                color: CColors.White,
                padding: EdgeInsets.only(16, right: 16, top: 16),
                child: new Column(
                    children: new List<Widget> {
                        new Container(
                            height: 12,
                            margin: EdgeInsets.only(bottom: 24),
                            color: new Color(0xFFF8F8F8)
                        ),
                        new Container(
                            height: 12,
                            margin: EdgeInsets.only(bottom: 40, right: 100),
                            color: new Color(0xFFF8F8F8)
                        ),
                        new Container(
                            height: 6,
                            margin: EdgeInsets.only(bottom: 24),
                            color: new Color(0xFFF8F8F8)
                        ),
                        new Container(
                            height: 6,
                            margin: EdgeInsets.only(bottom: 24),
                            color: new Color(0xFFF8F8F8)
                        ),
                        new Container(
                            height: 6,
                            margin: EdgeInsets.only(bottom: 24),
                            color: new Color(0xFFF8F8F8)
                        ),
                        new Container(
                            height: 6,
                            margin: EdgeInsets.only(right: 100),
                            color: new Color(0xFFF8F8F8)
                        )
                    }
                )
            );
        }
    }

    public class QuestionDetailLoading : StatelessWidget {
        public override Widget build(BuildContext context) {
            return new Container(
                color: CColors.White,
                child: new Column(
                    children: new List<Widget> {
                        new Container(
                            color: CColors.White,
                            height: 188,
                            padding: EdgeInsets.all(16),
                            child: new Column(
                                children: new List<Widget> {
                                    new Row(
                                        mainAxisAlignment: MainAxisAlignment.start,
                                        children: new List<Widget> {
                                            new Container(color: CColors.F4Bg, height: 28, width: 64),
                                            new SizedBox(width: 8),
                                            new Container(color: CColors.F4Bg, height: 28, width: 64),
                                            new SizedBox(width: 8),
                                            new Container(color: CColors.F4Bg, height: 28, width: 64)
                                        }
                                    ),
                                    new SizedBox(height: 24),
                                    new Container(color: CColors.LoadingBar, height: 12),
                                    new SizedBox(height: 16),
                                    new Container(color: CColors.LoadingBar, height: 12,
                                        margin: EdgeInsets.only(right: 110))
                                }
                            )
                        ),
                        new Container(color: CColors.BgGrey, height: 8),
                        new Container(
                            padding: EdgeInsets.symmetric(0, 16),
                            child: new Column(
                                mainAxisAlignment: MainAxisAlignment.start,
                                children: new List<Widget> {
                                    new Container(
                                        padding: EdgeInsets.symmetric(24),
                                        child: new Row(
                                            mainAxisAlignment: MainAxisAlignment.start,
                                            children: new List<Widget> {
                                                new Container(
                                                    decoration: new BoxDecoration(
                                                        color: CColors.LoadingBar,
                                                        borderRadius: BorderRadius.circular(12)
                                                    ),
                                                    width: 24,
                                                    height: 24
                                                ),
                                                new SizedBox(width: 16),
                                                new Container(
                                                    color: CColors.LoadingBar,
                                                    height: 8,
                                                    width: 120
                                                )
                                            }
                                        )
                                    ),
                                    new Container(
                                        color: CColors.LoadingBar,
                                        height: 8
                                    ),
                                    new SizedBox(height: 16),
                                    new Container(
                                        color: CColors.LoadingBar,
                                        height: 8
                                    ),
                                    new SizedBox(height: 16),
                                    new Container(
                                        color: CColors.LoadingBar,
                                        margin: EdgeInsets.only(right: 180),
                                        height: 8
                                    )
                                }
                            )
                        )
                    }
                )
            );
        }
    }

    public class AnswerDetailLoading : StatelessWidget {
        public override Widget build(BuildContext context) {
            return new Container(
                color: CColors.White,
                child: new Column(
                    children: new List<Widget> {
                        new Container(
                            color: CColors.White,
                            height: 136,
                            padding: EdgeInsets.all(16),
                            child: new Column(
                                children: new List<Widget> {
                                    new Container(
                                        color: CColors.LoadingBar,
                                        height: 12
                                    ),
                                    new SizedBox(height: 16),
                                    new Container(
                                        color: CColors.LoadingBar, 
                                        height: 12,
                                        margin: EdgeInsets.only(right: 110)
                                    )
                                }
                            )
                        ),
                        new Container(color: CColors.BgGrey, height: 8),
                        new Container(
                            padding: EdgeInsets.symmetric(0, 16),
                            child: new Column(
                                mainAxisAlignment: MainAxisAlignment.start,
                                children: new List<Widget> {
                                    new Container(
                                        padding: EdgeInsets.symmetric(24),
                                        child: new Row(
                                            mainAxisAlignment: MainAxisAlignment.start,
                                            children: new List<Widget> {
                                                new Container(
                                                    decoration: new BoxDecoration(
                                                        color: CColors.LoadingBar,
                                                        borderRadius: BorderRadius.circular(19)
                                                    ),
                                                    width: 38,
                                                    height: 38
                                                ),
                                                new SizedBox(width: 16),
                                                new Column(
                                                    mainAxisAlignment: MainAxisAlignment.center,
                                                    crossAxisAlignment: CrossAxisAlignment.start,
                                                    children: new List<Widget> {
                                                        new Container(
                                                            color: CColors.LoadingBar,
                                                            height: 8,
                                                            width: 120
                                                        ),
                                                        new SizedBox(height: 8),
                                                        new Container(
                                                            color: CColors.LoadingBar,
                                                            height: 8,
                                                            width: 60
                                                        )
                                                    }
                                                )
                                            }
                                        )
                                    ),
                                    new Container(
                                        color: CColors.LoadingBar,
                                        height: 8
                                    ),
                                    new SizedBox(height: 16),
                                    new Container(
                                        color: CColors.LoadingBar,
                                        height: 8
                                    ),
                                    new SizedBox(height: 16),
                                    new Container(
                                        color: CColors.LoadingBar,
                                        margin: EdgeInsets.only(right: 180),
                                        height: 8
                                    )
                                }
                            )
                        )
                    }
                )
            );
        }
    }
}