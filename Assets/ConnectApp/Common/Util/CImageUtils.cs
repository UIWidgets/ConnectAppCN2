using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ConnectApp.Common.Visual;
using Unity.UIWidgets.async;
using Unity.UIWidgets.engine;
using Unity.UIWidgets.foundation;
using Unity.UIWidgets.painting;
using Unity.UIWidgets.rendering;
using Unity.UIWidgets.ui;
using Unity.UIWidgets.widgets;
using Image = Unity.UIWidgets.widgets.Image;

namespace ConnectApp.Common.Util {
    public static class CImageUtils {
        static readonly List<int> ImageWidthList = new List<int>
            {200, 400, 600, 800, 1000, 1200, 1400, 1600, 1800, 2000};

        static readonly int ImageWidthMin = ImageWidthList.first();
        static readonly int ImageWidthMax = ImageWidthList.last();
        
        static readonly int gifWidth = 600;

        static string oldCDNUrl = "https://connect-cdn-public-prd.unitychina.cn/";
        static string newCDNUrl = "https://udp-cdn-public-prd.unitychina.cn/";

        static readonly List<string> ImageCDNList = new List<string> {
            "https://connect-cdn-public-prd.unitychina.cn/",
            "https://connect-cn-cdn-public-prd.unitychina.cn/"
        };

        public static string SuitableSizeImageUrl(float imageWidth, string imageUrl) {
            if (imageUrl.isEmpty()) {
                return "";
            }

            var devicePixelRatio = CSizes.DevicePixelRatio;
            if (imageWidth <= 0) {
                DebuggerUtils.DebugAssert(imageWidth <= 0, $"Image width error, width: {imageWidth}");
            }

            var networkImageWidth = (int) Math.Ceiling(imageWidth * devicePixelRatio);
            if (networkImageWidth <= ImageWidthMin) {
                networkImageWidth = ImageWidthMin;
            }
            else if (networkImageWidth >= ImageWidthMax) {
                networkImageWidth = ImageWidthMax;
            }
            else {
                networkImageWidth = ImageWidthList[networkImageWidth / 200];
            }

            var url = imageUrl;
            if (imageUrl.EndsWith(".png") || imageUrl.EndsWith(".PNG")) {
                url = $"{imageUrl}.{networkImageWidth}x0x1.png";
            }

            if (imageUrl.EndsWith(".jpg") || imageUrl.EndsWith(".JPG") || imageUrl.EndsWith(".jpeg") ||
                imageUrl.EndsWith(".JPEG")) {
                url = $"{imageUrl}.{networkImageWidth}x0x1.jpg";
            }

            if (imageUrl.EndsWith(".gif") || imageUrl.EndsWith(".GIF")) {
                if (imageUrl.Contains(value: oldCDNUrl)) {
                    imageUrl = imageUrl.Replace(oldValue: oldCDNUrl, newValue: newCDNUrl);
                }
                url = $"{imageUrl}.{gifWidth}x0x1.gif";
            }

            return url;
        }

        public static string SizeTo200ImageUrl(string imageUrl) {
            return SuitableSizeImageUrl(ImageWidthMin / CSizes.DevicePixelRatio, imageUrl: imageUrl);
        }

        public static string SizeTo400ImageUrl(string imageUrl) {
            return imageUrl + ".400x0x1.jpg";
        }

        public static string SizeToScreenImageUrl(BuildContext buildContext, string imageUrl) {
            return SuitableSizeImageUrl(imageWidth: MediaQuery.of(context: buildContext).size.width, imageUrl: imageUrl);
        }

        static bool isNationalDay = false;
        const string LicenseBadgePro = "UnityPro";
        const string LicenseBadgePlus = "UnityPersonalPlus";
        const string LicenseBadgePrem = "UnityLearnPremium";
        const string AwardBadgeOfficial = "official";
        const string AwardBadgeUvp2020 = "uvp_2020";
        const string AwardBadgeUvb2020 = "uvb_2020";

        public static Widget GenBadgeImage(List<string> badges, string license, EdgeInsets padding,
            bool showFlag = true, bool showLicenseBadge = true) {
            var badgeList = new List<Widget>();

            Widget badgeWidget = null;
            if (license.isNotEmpty()) {
                if (license.Equals(value: LicenseBadgePro)) {
                    badgeWidget = Image.file(
                        "image/badge-pro.png",
                        height: 16,
                        width: 24
                    );
                }
                else if (license.Equals(value: LicenseBadgePlus)) {
                    badgeWidget = Image.file(
                        "image/badge-plus.png",
                        height: 16,
                        width: 28
                    );
                }

                // else if (license.Equals(value: LicenseBadgePrem)) {
                //     badgeWidget = Image.file(
                //         "image/badge-prem.png",
                //         height: 16,
                //         width: 33
                //     );
                // }
            }

            if (badges != null && badges.isNotEmpty()) {
                if (badges.Any(badge => badge.isNotEmpty() && badge.Equals(value: AwardBadgeOfficial))) {
                    badgeWidget = Image.file(
                        "image/badge-official.png",
                        height: 18,
                        width: 18
                    );
                }
            }

            if (badgeWidget != null && showLicenseBadge) {
                if (badgeList.Count >= 1) {
                    badgeList.Add(new SizedBox(width: 4));
                }

                badgeList.Add(item: badgeWidget);
            }

            Widget awardBadgeWidget = null;
            if (badges != null && badges.isNotEmpty()) {
                if (badges.Any(badge => badge.isNotEmpty() && badge.Equals(value: AwardBadgeUvb2020))) {
                    awardBadgeWidget = Image.file(
                        "image/badge-uvb-2020.png",
                        height: 18,
                        width: 56
                    );
                }

                if (badges.Any(badge => badge.isNotEmpty() && badge.Equals(value: AwardBadgeUvp2020))) {
                    awardBadgeWidget = Image.file(
                        "image/badge-uvp-2020.png",
                        height: 18,
                        width: 55
                    );
                }
            }

            if (awardBadgeWidget != null) {
                if (badgeList.Count >= 1) {
                    badgeList.Add(new SizedBox(width: 4));
                }

                badgeList.Add(item: awardBadgeWidget);
            }

            if (isNationalDay && showFlag) {
                if (badgeList.Count >= 1) {
                    badgeList.Add(new SizedBox(width: 4));
                }

                badgeList.Add(Image.file(
                    "image/china-flag-badge.png",
                    height: 14,
                    width: 16
                ));
            }

            if (badgeList.Count > 0) {
                return new Container(
                    padding: padding,
                    child: new Row(
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: badgeList
                    )
                );
            }

            return new Container();
        }

        static readonly List<string> PatternImages = new List<string> {
            "image/pattern1.png",
            "image/pattern2.png",
            "image/pattern3.png",
            "image/pattern4.png"
        };

        public static string GetSpecificPatternImageNameFromId(string id) {
            return PatternImages[CCommonUtils.GetStableHash(s: id) % PatternImages.Count];
        }

        public const string FavoriteCoverImagePath = "image/favorites";

        public static string addSuffix(this string imageName, string suffix) {
            if (imageName.isNotEmpty()) {
                return imageName + suffix;
            }
            return "";
        }
        
        public static string removeSuffix(this string imageName) {
            if (imageName.isNotEmpty()) {
                return imageName.Split('.').first();
            }
            return "";
        }

        public static readonly List<string> FavoriteCoverImages = new List<string> {
            "favor-gamepad.png",
            "favor-programming.png",
            "favor-trophy.png",
            "favor-document.png",
            "favor-gamer.png",
            "favor-360_rotate.png",
            "favor-musical_note.png",
            "favor-book.png",
            "favor-smartphone.png",
            "favor-headphones.png",
            "favor-keyboard.png",
            "favor-game_console.png",
            "favor-hamburger.png",
            "favor-pokemon_go.png",
            "favor-security.png",
            "favor-beer.png",
            "favor-magazine.png",
            "favor-vr.png",
            "favor-french_fries.png",
            "favor-balloons.png",
            "favor-smartwatch.png",
            "favor-analytics.png",
            "favor-coffee_cup.png",
            "favor-computer.png",
            "favor-pencil.png",
            "favor-gamepad2.png",
            "favor-smartphone2.png",
            "favor-blog.png",
            "favor-muffin.png",
            "favor-camera.png",
            "favor-layers.png",
            "favor-cmyk.png",
            "favor-hot_air_balloon.png",
            "favor-video_camera.png",
            "favor-idea.png",
            "favor-map.png"
        };


        public static byte[] readImage(string path) {
            var fs = new FileStream(path: path, mode: FileMode.Open, access: FileAccess.Read);
            fs.Seek(0, origin: SeekOrigin.Begin);
            var bytes = new byte[fs.Length];
            fs.Read(array: bytes, 0, (int) fs.Length);
            fs.Close();
            fs.Dispose();
            return bytes;
        }

        public static Future<byte[]> asyncLoadImageFile(string path) {
            var completer = Completer.create();
            var isolate = Isolate.current;
            var panel = UIWidgetsPanelWrapper.current.window;
            if (panel.isActive()) {
                panel.startCoroutine(_read(completer: completer, path: path, isolate: isolate));
            }
            return completer.future.to<byte[]>();
        }

        static IEnumerator _read(Completer completer, string path, Isolate isolate) {
            var bytes = readImage(path: path);
            using (Isolate.getScope(isolate: isolate)) {
                if (bytes == null) {
                    completer.completeError(new Exception("Failed to read file"));
                    yield break;
                }
                completer.complete(value: bytes);
            }
        }
    }
}