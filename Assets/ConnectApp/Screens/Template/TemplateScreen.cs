using ConnectApp.Common.Visual;
using ConnectApp.Models.ActionModel;
using ConnectApp.Models.State;
using ConnectApp.Models.ViewModel;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.widgets;

namespace ConnectApp.Screens.Template {
    public class TemplateScreenConnector : StatelessWidget {
        public TemplateScreenConnector(
            Key key = null
        ) : base(key: key) {
        }

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, BaseViewModel>(
                converter: state => new BaseViewModel(),
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new BaseActionModel();
                    return new TemplateScreen(viewModel: viewModel, actionModel: actionModel);
                }
            );
        }
    }

    public class TemplateScreen : StatefulWidget {
        public TemplateScreen(
            Key key = null,
            BaseViewModel viewModel = null,
            BaseActionModel actionModel = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly BaseViewModel viewModel;
        public readonly BaseActionModel actionModel;

        public override State createState() {
            return new TemplateScreenState();
        }
    }

    class TemplateScreenState : State<TemplateScreen> {
        public override Widget build(BuildContext context) {
            return new Container(
                color: CColors.White
            );
        }
    }
}