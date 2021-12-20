using System;
using ConnectApp.Common.Visual;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.widgets;

namespace ConnectApp.Components.Toast
{
    public enum ToastType
    {
        Normal,
        Error
    }
    
    public enum ToastLength
    {
        Short = 2,
        Medium = 4,
        Long = 6
    }
    
    public static class CustomToast
    {
        static BuildContext _buildContext;

        public static void init(BuildContext buildContext)
        {
            _buildContext = buildContext;
        }

        // public static void showToast(string message = "", ToastType type = ToastType.Normal,
        //     ToastLength length = ToastLength.Short)
        // { 
        //     showToast(context: _buildContext, message: message, type: type, length: length);
        // }
        
        public static void showToast(BuildContext context = null, string message = "", ToastType type = ToastType.Normal, ToastLength length = ToastLength.Short, ToastGravity gravity = ToastGravity.BOTTOM)
        {
            var textColor = CColors.PrimaryBlue;
            switch (type)
            {
                case ToastType.Normal:
                    textColor = CColors.PrimaryBlue;
                    break;
                case ToastType.Error:
                    textColor = CColors.Error;
                    break;
            }
            var duration  = TimeSpan.FromSeconds((int)length);
            var widget = new Container(
                padding: EdgeInsets.symmetric(8, 16),
                decoration: new BoxDecoration(
                    color: CColors.White,
                    borderRadius: BorderRadius.circular(4)
                ),
                child: new Text(data: message, style: CTextStyle.PRegularBody.copyWith(color: textColor, height: 1))
            );
            var cxt = context == null ? _buildContext : context;
            CToast.init(context: cxt);
            CToast.instance.showToast(child: widget, toastDuration: duration, gravity: gravity);
        }
    }
}