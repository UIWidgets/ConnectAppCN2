using System;
using System.Collections.Generic;
using ConnectApp.Common.Other;
using ConnectApp.Common.Visual;
using ConnectApp.Components;
using ConnectApp.Models.Model;
using ConnectApp.Models.State;
using ConnectApp.Models.ViewModel;
using ConnectApp.redux.actions;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.widgets;

namespace ConnectApp.screens {
    public class PersonalJobRoleScreenConnector : StatelessWidget {
        public PersonalJobRoleScreenConnector(
            Key key = null
        ) : base(key: key) {
        }

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, EditPersonalInfoScreenViewModel>(
                converter: state => {
                    var currentUserId = state.loginState.loginInfo.userId ?? "";
                    var user = state.userState.userDict.ContainsKey(key: currentUserId)
                        ? state.userState.userDict[key: currentUserId]
                        : new User();
                    return new EditPersonalInfoScreenViewModel {
                        user = user,
                        jobRole = state.userState.jobRole
                    };
                },
                builder: (context1, viewModel, dispatcher) => {
                    return new PersonalJobRoleScreen(
                        viewModel: viewModel,
                         jobRole => {
                            dispatcher.dispatch(new ChangePersonalRoleAction {jobRole = jobRole});
                        }
                    );
                }
            );
        }
    }

    public class PersonalJobRoleScreen : StatelessWidget {
        public PersonalJobRoleScreen(
            EditPersonalInfoScreenViewModel viewModel = null,
            Action<JobRole> changeJobRole = null,
            Key key = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.changeJobRole = changeJobRole;
        }

        readonly EditPersonalInfoScreenViewModel viewModel;
        readonly Action<JobRole> changeJobRole;

        public override Widget build(BuildContext context) {
            return new Container(
                color: CColors.White,
                child: new CustomSafeArea(
                    bottom: false,
                    child: new Column(
                        children: new List<Widget> {
                            this._buildNavigationBar(context: context),
                            new Flexible(
                                child: this._buildContent(context: context)
                            )
                        }
                    )
                )
            );
        }

        Widget _buildNavigationBar(BuildContext context) {
            return new CustomAppBar(
                () => Navigator.pop(context: context),
                new Text(
                    "请选择对应的身份",
                    style: CTextStyle.PXLargeMedium
                )
            );
        }

        Widget _buildContent(BuildContext context) {
            var widgets = new List<Widget> {
                new CustomDivider(
                    color: CColors.White
                )
            };
            var values = this.viewModel.user.jobRoleMap.Values;
            foreach (var jobRole in values) {
                var widget = this._buildRoleItem(context: context, jobRole: jobRole);
                widgets.Add(item: widget);
            }

            return new Container(
                color: CColors.Background,
                child: new ListView(
                    children: widgets
                )
            );
        }

        Widget _buildRoleItem(BuildContext context, JobRole jobRole) {
            var name = DictData.jobRoleDict.ContainsKey(key: jobRole.name)
                ? DictData.jobRoleDict[key: jobRole.name]
                : jobRole.name;
            var isCheck = this.viewModel.jobRole.id == jobRole.id;
            Widget checkWidget;
            if (isCheck) {
                checkWidget = new Icon(
                    icon: CIcons.check,
                    size: 24,
                    color: CColors.PrimaryBlue
                );
            }
            else {
                checkWidget = new Container();
            }

            return new GestureDetector(
                onTap: () => {
                    this.changeJobRole(obj: jobRole);
                    Navigator.pop(context: context);
                },
                child: new Container(
                    color: CColors.White,
                    height: 44,
                    padding: EdgeInsets.symmetric(horizontal: 16),
                    child: new Row(
                        mainAxisAlignment: MainAxisAlignment.spaceBetween,
                        children: new List<Widget> {
                            new Text(
                                data: name,
                                style: isCheck ? CTextStyle.PLargeBlue : CTextStyle.PLargeBody
                            ),
                            checkWidget
                        }
                    )
                )
            );
        }
    }
}