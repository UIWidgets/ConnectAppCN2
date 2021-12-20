using System;
using System.Collections.Generic;
using ConnectApp.Common.Util;
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
    public class SelectPlateScreenConnector : StatelessWidget {
        public SelectPlateScreenConnector(
            Key key = null
        ) : base(key: key) {
        }
        
        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, SelectPlateScreenViewModel>(
                converter: state => {
                    return new SelectPlateScreenViewModel {
                        plate = state.qaEditorState.questionDraft.plate,
                        plates = state.qaEditorState.plates
                    };
                },
                builder: (context1, viewModel, dispatcher) => {
                    return new SelectPlateScreen(
                        viewModel: viewModel,
                         plate => {
                            dispatcher.dispatch(new ChangeQuestionPlateAction {plate = plate});
                        }
                    );
                }
            );
        }
    }

    public class SelectPlateScreen : StatelessWidget {
        public SelectPlateScreen(
            SelectPlateScreenViewModel viewModel = null,
            Action<Plate> changePlate = null,
            Key key = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.changePlate = changePlate;
        }

        readonly SelectPlateScreenViewModel viewModel;
        readonly Action<Plate> changePlate;

        public override Widget build(BuildContext context) {
            return new Container(
                padding: EdgeInsets.only(top: CCommonUtils.getSafeAreaTopPadding(context: context) + 44),
                child: new Column(
                    children: new List<Widget> {
                        this._buildHeader(context: context),
                        new Expanded(
                            child: this._buildContent(context: context)
                        )
                    }
                )
            );
        }

        Widget _buildHeader(BuildContext context) {
            return new Container(
                decoration: new BoxDecoration(
                    color: CColors.White,
                    borderRadius: BorderRadius.only(12, 12)
                ),
                child: new Column(
                    children: new List<Widget> {
                        new Row(
                            mainAxisAlignment: MainAxisAlignment.spaceBetween,
                            children: new List<Widget> {
                                new SizedBox(width: 72),
                                new Text(
                                    "请选择发布的板块",
                                    style: CTextStyle.PXLargeMedium.defaultHeight()
                                ),
                                new CustomButton(
                                    padding: EdgeInsets.all(20),
                                    onPressed: () => Navigator.pop(context: context),
                                    child: new Text(
                                        "完成",
                                        style: CTextStyle.PLargeBlue.defaultHeight()
                                    )
                                )
                            }
                        ),
                        new CustomDivider(color: CColors.Separator2, height: 1)
                    }
                )
            );
        }

        Widget _buildContent(BuildContext context) {
            var widgets = new List<Widget> {
                new CustomDivider(
                    color: CColors.White
                )
            };
            var values = this.viewModel.plates;
            foreach (var plate in values) {
                var widget = this._buildPlateItem(context: context, plate: plate);
                widgets.Add(item: widget);
            }

            return new Container(
                color: CColors.Background,
                child: new ListView(
                    children: widgets,
                    padding: EdgeInsets.zero
                )
            );
        }

        Widget _buildPlateItem(BuildContext context, Plate plate) {
            var name = plate.name;
            var isCheck = this.viewModel.plate?.id == plate.id;
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
                    this.changePlate(obj: plate);
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
                                style: isCheck ? CTextStyle.PLargeBlue.defaultHeight() : CTextStyle.PLargeBody.defaultHeight()
                            ),
                            checkWidget
                        }
                    )
                )
            );
        }
    }
}