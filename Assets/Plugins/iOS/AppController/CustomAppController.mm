//
//  CustomAppController.m
//  Unity-iPhone
//
//  Created by luo on 2019/4/25.
//

#import "UnityAppController.h"
#import "WechatPlugin.h"
#include "WXApi.h"
#import <UserNotifications/UserNotifications.h>
#include "uiwidgets_message_manager.h"
#import "CommonPlugin.h"
#import <AVFoundation/AVFoundation.h>
#import "UUIDUtils.h"
#import "PickImageController.h"
#import "RealReachability.h"
#import "custom_app_controller.h"

@interface CustomAppController : CustomUIWidgetsAppController<WXApiDelegate>
@property (nonatomic,assign) NSInteger tabIndex;
@end
IMPL_APP_CONTROLLER_SUBCLASS (CustomAppController)

@implementation CustomAppController

- (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions
{
    
    [super application:application didFinishLaunchingWithOptions:launchOptions];
    // 在 register 之前打开 log, 后续可以根据 log 排查问题
    [WXApi startLogByLevel:WXLogLevelDetail logBlock:^(NSString *log) {
        NSLog(@"WeChatSDK: %@", log);
    }];
    
    // [WXApi registerApp:@"" universalLink:@""];
    
    // 调用自检函数
    // [WXApi checkUniversalLinkReady:^(WXULCheckStep step, WXCheckULStepResult* result) {
    //     NSLog(@"%@, %u, %@, %@", @(step), result.success, result.errorInfo, result.suggestion);
    // }];
    
    // Listen for network status
    [GLobalRealReachability startNotifier];
    
    return YES;
}

#pragma mark- JPUSHRegisterDelegate

- (void)application:(UIApplication *)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken {
    // Required.
}

- (void)application:(UIApplication *)application didReceiveRemoteNotification:(NSDictionary *)userInfo {
    [super application:application didReceiveRemoteNotification:userInfo];
    // Required.
}

- (void)application:(UIApplication *)application didReceiveRemoteNotification:(NSDictionary *)userInfo fetchCompletionHandler:(void (^)(UIBackgroundFetchResult result))handler {
    [super application:application didReceiveRemoteNotification:userInfo fetchCompletionHandler:handler];
    handler(UIBackgroundFetchResultNewData);
}

- (void)networkDidReceiveMessage:(NSNotification *)notification {
}

- (void)networkDidReceivePushNotification:(NSNotification *)notification {
}

- (void)networkOpenPushNotification:(NSNotification *)notification {
}

NSData *APNativeJSONData(id obj) {
    NSError *error = nil;
    NSData *data = [NSJSONSerialization dataWithJSONObject:obj options:0 error:&error];
    if (error) {
        NSLog(@"%s trans obj to data with error: %@", __func__, error);
        return nil;
    }
    return data;
}
#pragma mark wechat

- (void)scene:(UIScene *)scene continueUserActivity:(NSUserActivity *)userActivity API_AVAILABLE(ios(13.0)){
    [WXApi handleOpenUniversalLink:userActivity delegate:self];
}

- (BOOL)application:(UIApplication *)application continueUserActivity:(NSUserActivity *)userActivity restorationHandler:(void (^)(NSArray<id<UIUserActivityRestoring>> * _Nullable))restorationHandler{
    
    if ([userActivity.activityType isEqualToString:NSUserActivityTypeBrowsingWeb]) {
        NSURL *webpageURL = userActivity.webpageURL;
        NSString *host = webpageURL.host;
        if ([host isEqualToString:@"applink.unity.cn"]) {
            // 判断域名是自己的网站，进行我们需要的处理
            [CommonPlugin instance].universalLink = [webpageURL absoluteString];
            UIWidgetsMethodMessage(@"common", @"OnOpenUniversalLinks", @[[webpageURL absoluteString]]);
        }
    }
    return [WXApi handleOpenUniversalLink:userActivity delegate:self];
}

- (BOOL)application:(UIApplication*)app openURL:(NSURL*)url options:(NSDictionary<NSString*, id>*)options
{
    if ([[url scheme] isEqualToString:@"unityconnect"]) {
        [CommonPlugin instance].schemeUrl = [url absoluteString];
        UIWidgetsMethodMessage(@"common", @"OnOpenUrl", @[[url absoluteString]]);
    }
    return [WXApi handleOpenURL:url delegate:self];
}

- (void)onResp:(BaseResp *)resp {
    if ([resp isKindOfClass:[SendAuthResp class]]) {
        SendAuthResp *sendAuthResp = (SendAuthResp *) resp;
        [[WechatPlugin instance]sendCodeEvent:sendAuthResp.code stateId:sendAuthResp.state];
    }
    if ([resp isKindOfClass:[WXLaunchMiniProgramResp class]]) {
        WXLaunchMiniProgramResp *miniResp = (WXLaunchMiniProgramResp *) resp;
        if (miniResp.extMsg.length!=0) {
            UIWidgetsMethodMessage(@"wechat", @"openUrl", @[miniResp.extMsg]);
        }
    }
}

+(void)saveImage:(NSString*)imagePath{
    UIImage *img = [UIImage imageWithContentsOfFile:imagePath];
    UIImageWriteToSavedPhotosAlbum(img, self,
                                   @selector(image:didFinishSavingWithError:contextInfo:), nil);
    
}
+(void)image:(UIImage *)image didFinishSavingWithError:(NSError *)error contextInfo:(void *)contextInfo{
    if (!error) {
        UIWidgetsMethodMessage(@"common", @"SaveImageSuccess", @[]);
    } else {
        UIWidgetsMethodMessage(@"common", @"SaveImageError",@[]);
    }
}
-(void)updateTabIndex:(NSInteger)index{
    self.tabIndex = index;
}
extern "C"  {
    void pauseAudioSession();
    void setStatusBarStyle(bool isLight);
    void hiddenStatusBar(bool hidden);
    bool isOpenSensor();
    const char *getDeviceID();
    void pickImage(const char *source, bool cropped, int maxSize);
    void pickVideo(const char *source);
    void saveImage(const char *path);
    bool isPhotoLibraryAuthorization ();
    bool isCameraAuthorization ();
    bool isEnableNotification();
    void playSystemSound();
    void updateShowAlert(bool isShow);
    void clearAllAlert();
    void clearBadge();
}

void pauseAudioSession(){
    AVAudioSession *session = [AVAudioSession sharedInstance];
    [session setCategory:AVAudioSessionCategoryPlayback error:nil];
    [session setActive:YES error:nil];
}
void setStatusBarStyle(bool isLight){
    AppController_SendNotificationWithArg(@"UpdateStatusBarStyle",
                                          @{@"key":@"style",@"value":@(isLight)});
}
void hiddenStatusBar(bool hidden){
    AppController_SendNotificationWithArg(@"UpdateStatusBarStyle",
                                          @{@"key":@"hidden",@"value":@(hidden)});
}
bool isOpenSensor() {
    return true;
}

const char *getDeviceID()
{
    NSString *result = [UUIDUtils getUUID];
    if (!result) {
        return NULL;
    }
    const char *s = [result UTF8String];
    char *r = (char *)malloc(strlen(s) + 1);
    strcpy(r, s);
    return r;
}

void pickImage(const char *source, bool cropped, int maxSize) {
    NSString *sourceString = [NSString stringWithUTF8String:source];
    [[PickImageController sharedInstance] pickImageWithSource:sourceString cropped:cropped maxSize:maxSize];
    
}
void pickVideo(const char *source) {
    NSString *sourceString = [NSString stringWithUTF8String:source];
    [[PickImageController sharedInstance] pickVideoWithSource:sourceString];
}

void saveImage(const char *path)//相册
{
    NSString *imageStr = [NSString stringWithUTF8String:path];
    [CustomAppController saveImage:imageStr];
}

bool isPhotoLibraryAuthorization (){
    return [[PickImageController sharedInstance] isPhotoLibraryAuthorization];
}
bool isCameraAuthorization (){
    return [[PickImageController sharedInstance] isCameraAuthorization];
    
}
bool isEnableNotification(){
    BOOL isEnable = NO;
    if ([[UIDevice currentDevice].systemVersion floatValue] >= 8.0f) { // iOS版本 >= 8.0 处理逻辑
        UIUserNotificationSettings *setting = [[UIApplication sharedApplication] currentUserNotificationSettings];
        isEnable = (UIUserNotificationTypeNone == setting.types) ? NO : YES;
    }
    return isEnable;
}

void playSystemSound(){
    AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
}

void updateShowAlert(bool isShow){
}

void clearAllAlert(){
    [UIApplication sharedApplication].applicationIconBadgeNumber = 1;
    [UIApplication sharedApplication].applicationIconBadgeNumber = 0;
}
void clearBadge(){
    [[UIApplication sharedApplication] setApplicationIconBadgeNumber:0];
}

@end
