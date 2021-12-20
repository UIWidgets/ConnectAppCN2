using System.Collections.Generic;
using ConnectApp.Common.Visual;
using ConnectApp.Components;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace ConnectApp.screens {
    public class RootScreen : StatelessWidget {
        public override Widget build(BuildContext context) {
            return new VersionUpdater(
                new Container(
                    color: CColors.White,
                    child: new CustomSafeArea(
                        top: false,
                        child: new Column(
                            mainAxisAlignment: MainAxisAlignment.end,
                            children: new List<Widget> {
                                new Container(
                                    width: 176,
                                    height: 32,
                                    child: Image.file("image/unityConnectBlack.png")
                                ),
                                new Container(
                                    width: 101,
                                    height: 22,
                                    margin: EdgeInsets.only(top: 6, bottom: 20),
                                    child: Image.file("image/madeWithUnity.png")
                                )
                            })
                    )
                )
            );
        }
    }
}