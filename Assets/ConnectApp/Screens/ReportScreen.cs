using System;
using System.Collections.Generic;
using ConnectApp.Common.Util;
using ConnectApp.Common.Visual;
using ConnectApp.Components;
using ConnectApp.Components.Toast;
using ConnectApp.Models.ActionModel;
using ConnectApp.Models.State;
using ConnectApp.Models.ViewModel;
using ConnectApp.redux.actions;
using Unity.UIWidgets.async;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.Redux;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;

namespace ConnectApp.screens {
    public enum ReportType {
        article,
        comment,
        question,
        answer,
        mock
    }

    public class ReportScreenConnector : StatelessWidget {
        public ReportScreenConnector(
            string reportId,
            ReportType reportType,
            Key key = null
        ) : base(key: key) {
            this.reportId = reportId;
            this.reportType = reportType;
        }

        readonly string reportId;
        readonly ReportType reportType;

        public override Widget build(BuildContext context) {
            return new StoreConnector<AppState, ReportScreenViewModel>(
                converter: state => new ReportScreenViewModel {
                    reportId = this.reportId,
                    reportType = this.reportType
                },
                builder: (context1, viewModel, dispatcher) => {
                    var actionModel = new ReportScreenActionModel {
                        reportItem = reportContext => {
                            string itemType;
                            switch (this.reportType) {
                                case ReportType.article:
                                    itemType = "project";
                                    break;
                                case ReportType.comment:
                                    itemType = "comment";
                                    break;
                                case ReportType.mock:
                                    itemType = "mock";
                                    break;
                                default:
                                    itemType = "";
                                    break;
                            }

                            return dispatcher.dispatch<Future>(CActions.reportItem(itemId: this.reportId,
                                itemType: itemType, reportContext: reportContext)
                            );
                        }
                    };
                    return new ReportScreen(viewModel: viewModel, actionModel: actionModel, key: this.key);
                }
            );
        }
    }

    public class ReportScreen : StatefulWidget {
        public ReportScreen(
            ReportScreenViewModel viewModel = null,
            ReportScreenActionModel actionModel = null,
            Key key = null
        ) : base(key: key) {
            this.viewModel = viewModel;
            this.actionModel = actionModel;
        }

        public readonly ReportScreenViewModel viewModel;
        public readonly ReportScreenActionModel actionModel;

        public override State createState() {
            return new _ReportScreenState();
        }
    }

    class _ReportScreenState : State<ReportScreen> {
        readonly List<string> _reportItems = new List<string> {
            "垃圾信息",
            "涉嫌侵权",
            "不友善行为",
            "有害信息"
        };

        int _selectedIndex;
        bool _isLoading;

        public override void initState() {
            base.initState();
            StatusBarManager.statusBarStyle(false);
            this._selectedIndex = 0;
            this._isLoading = false;
        }

        public override Widget build(BuildContext context) {
            return new Container(
                color: CColors.White,
                child: new CustomSafeArea(
                    child: new Container(
                        child: new Column(
                            children: new List<Widget> {
                                this._buildNavigationBar(),
                                this._buildContent()
                            }
                        )
                    )
                )
            );
        }

        Widget _buildNavigationBar() {
            return new CustomAppBar(
                () => Navigator.pop(context: this.context),
                new Text(
                    "举报",
                    style: CTextStyle.PXLargeMedium
                )
            );
        }

        Widget _buildContent() {
            var widgets = new List<Widget>();
            this._reportItems.ForEach(item => {
                var index = this._reportItems.IndexOf(item: item);
                var widget = this._buildReportItem(title: item, index: index);
                widgets.Add(item: widget);
            });
            widgets.Add(this._buildReportButton());
            return new Container(
                margin: EdgeInsets.only(top: 15),
                child: new Column(
                    children: widgets
                )
            );
        }

        Widget _buildReportItem(string title, int index) {
            return new GestureDetector(
                onTap: () => {
                    if (this._selectedIndex != index) {
                        this.setState(() => this._selectedIndex = index);
                    }
                },
                child: new Container(
                    height: 44,
                    color: CColors.White,
                    padding: EdgeInsets.symmetric(horizontal: 16),
                    child: new Row(
                        children: new List<Widget> {
                            index == this._selectedIndex ? _buildCheckBox() : _buildUnCheckBox(),
                            new Container(
                                margin: EdgeInsets.only(12),
                                child: new Text(
                                    data: title,
                                    style: CTextStyle.PLargeBody
                                )
                            )
                        }
                    )
                )
            );
        }

        static Widget _buildUnCheckBox() {
            return new Container(
                width: 20,
                height: 20,
                decoration: new BoxDecoration(
                    borderRadius: BorderRadius.circular(10),
                    border: Border.all(Color.fromRGBO(216, 216, 216, 1))
                )
            );
        }

        static Widget _buildCheckBox() {
            return new Container(
                width: 20,
                height: 20,
                decoration: new BoxDecoration(
                    color: CColors.PrimaryBlue,
                    borderRadius: BorderRadius.circular(10)
                ),
                alignment: Alignment.center,
                child: new Container(
                    width: 10,
                    height: 10,
                    decoration: new BoxDecoration(
                        color: CColors.White,
                        borderRadius: BorderRadius.circular(5)
                    )
                )
            );
        }

        Widget _buildReportButton() {
            Widget right;
            if (this._isLoading) {
                right = new CustomActivityIndicator(
                    loadingColor: LoadingColor.white,
                    size: LoadingSize.small
                );
            }
            else {
                right = new Container();
            }

            return new Container(
                padding: EdgeInsets.symmetric(horizontal: 16),
                margin: EdgeInsets.only(top: 16),
                child: new CustomButton(
                    onPressed: () => {
                        if (this._isLoading) {
                            return;
                        }
                        this._isLoading = true;
                        this.setState(() =>{});
                        if (this.widget.viewModel.reportType == ReportType.mock) {
                            if (HttpManager.isNetWorkError()) {
                                this._isLoading = false;
                                this.setState(() =>{});
                                CustomToast.showToast(context: this.context, "请检查网络连接", type: ToastType.Error);
                            }
                            else {
                                Future.delayed(TimeSpan.FromSeconds(0.5f)).then(_ => {
                                    this._isLoading = false;
                                    this.setState(() =>{});
                                    Navigator.pop(context: this.context);
                                    CustomToast.showToast(context: this.context, "举报成功");
                                });
                            }
                        }
                        else {
                            this.widget.actionModel.reportItem(this._reportItems[index: this._selectedIndex])
                                .then(_ => {
                                    this._isLoading = false;
                                    this.setState(() =>{});
                                    Navigator.pop(context: this.context);
                                    CustomToast.showToast(context: this.context, "举报成功");
                                }).catchError(err => {
                                    this._isLoading = false;
                                    this.setState(() =>{});
                                    CustomToast.showToast(context: this.context, "举报失败", type: ToastType.Error);
                                });
                        }
                    },
                    padding: EdgeInsets.zero,
                    child: new Container(
                        height: 40,
                        decoration: new BoxDecoration(
                            this._isLoading
                                ? CColors.ButtonActive
                                : CColors.PrimaryBlue,
                            borderRadius: BorderRadius.all(4)
                        ),
                        child: new Stack(
                            children: new List<Widget> {
                                new Align(
                                    alignment: Alignment.center,
                                    child: new Text(
                                        "举报",
                                        style: CTextStyle.PLargeMediumWhite
                                    )
                                ),
                                new Positioned(
                                    right: 24,
                                    height: 40,
                                    child: right
                                )
                            }
                        )
                    )
                )
            );
        }
    }
}