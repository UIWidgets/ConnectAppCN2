using ConnectApp.Common.Visual;
using ConnectApp.Components;
using Unity.UIWidgets.widgets;

namespace ConnectApp.screens {
    static class LoginNavigatorRoutes {
        public const string Login = "login/login";
        public const string BindUnity = "login/bind-unity";
        public const string WechatBindUnity = "login/wechat-bind-unity";
    }

    public class LoginScreen : StatelessWidget {
        static Route onGenerateRoute(BuildContext context, RouteSettings settings) {
            WidgetBuilder builder = null;
            switch (settings.name) {
                case LoginNavigatorRoutes.Login: {
                    builder = _ => new LoginSwitchScreenConnector(
                        () => Navigator.pop(context: context)
                    );
                    break;
                }
                case LoginNavigatorRoutes.BindUnity: {
                    builder = _ => new BindUnityScreenConnector(
                        fromPage: FromPage.login,
                        () => Navigator.pop(context: context)
                    );
                    break;
                }
                case LoginNavigatorRoutes.WechatBindUnity: {
                    builder = _ => new BindUnityScreenConnector(
                        fromPage: FromPage.wechat,
                        () => Navigator.pop(context: context)
                    );
                    break;
                }
            }

            return new CustomPageRoute(
                builder: builder,
                settings: settings
            );
        }

        static Route onUnknownRoute(RouteSettings settings) {
            return new CustomPageRoute(
                _ => new Container(color: CColors.White, child: new Center(child: new Text("NOT FOUND 404"))),
                settings: settings
            );
        }

        public override Widget build(BuildContext context) {
            return new Navigator(
                initialRoute: LoginNavigatorRoutes.Login,
                onGenerateRoute: settings => onGenerateRoute(context: context, settings: settings),
                onUnknownRoute: onUnknownRoute
            );
        }
    }
}