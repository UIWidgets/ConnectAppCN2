//
//  QRScanViewController.m
//  Unity-iPhone
//
//  Created by unity on 2019/7/24.
//

#import "QRScanViewController.h"
#import <AVFoundation/AVFoundation.h>
#import "QRScanView.h"
#import "QRScanUtil.h"
#import "NSTimer+Addition.h"
#import "Masonry.h"

@interface QRScanViewController ()<AVCaptureMetadataOutputObjectsDelegate>

@property (strong, nonatomic) AVCaptureDevice *device;
@property (strong, nonatomic) AVCaptureDeviceInput *input;
@property (strong, nonatomic) AVCaptureMetadataOutput *output;
@property (strong, nonatomic) AVCaptureSession *session;
@property (strong, nonatomic) AVCaptureVideoPreviewLayer *preview;
@property (assign, nonatomic) CGPoint startPoint;
@property (assign, nonatomic) CGPoint movePoint;
@property (assign, nonatomic) CGPoint endPoint;
@property (strong, nonatomic) QRScanView *qrScanView;

@end

@implementation QRScanViewController

- (UIStatusBarStyle)preferredStatusBarStyle
{
    return UIStatusBarStyleLightContent;
}

- (void)viewDidLoad
{
    [super viewDidLoad];
    // Do any additional setup after loading the view.
    self.view.backgroundColor = [UIColor whiteColor];
    
    AVAuthorizationStatus authStatus = [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeVideo];
    if (authStatus == AVAuthorizationStatusNotDetermined) {
        [AVCaptureDevice requestAccessForMediaType:AVMediaTypeVideo completionHandler:^(BOOL granted) {
            if (!granted) {
                dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)(1 * NSEC_PER_SEC)), dispatch_get_main_queue(), ^{
                    [self buildAlertController];
                });
                return;
            }
        }];
    }
    if (authStatus == AVAuthorizationStatusRestricted || authStatus == AVAuthorizationStatusDenied) {
        dispatch_after(dispatch_time(DISPATCH_TIME_NOW, (int64_t)(1 * NSEC_PER_SEC)), dispatch_get_main_queue(), ^{
            [self buildAlertController];
        });
        return;
    }
        
    [self setQRScanView];
}

- (void)setQRScanView
{
    _device = [AVCaptureDevice defaultDeviceWithMediaType:AVMediaTypeVideo];
    
    _input = [AVCaptureDeviceInput deviceInputWithDevice:self.device error:nil];
    
    _output = [[AVCaptureMetadataOutput alloc] init];
    [_output setMetadataObjectsDelegate:self queue:dispatch_get_main_queue()];
    
    _session = [[AVCaptureSession alloc] init];
    [_session setSessionPreset:AVCaptureSessionPresetHigh];
    if ([_session canAddInput:self.input]) {
        [_session addInput:self.input];
    }
    if ([_session canAddOutput:self.output]) {
        [_session addOutput:self.output];
    }
    
    AVCaptureConnection *outputConnection = [_output connectionWithMediaType:AVMediaTypeVideo];
    outputConnection.videoOrientation = [QRScanUtil videoOrientationFromCurrentDeviceOrientation];
    
    //增加条形码扫描
    _output.metadataObjectTypes = @[AVMetadataObjectTypeEAN13Code,
                                    AVMetadataObjectTypeEAN8Code,
                                    AVMetadataObjectTypeCode128Code,
                                    AVMetadataObjectTypeQRCode];
    
    _preview = [AVCaptureVideoPreviewLayer layerWithSession:_session];
    _preview.videoGravity = AVLayerVideoGravityResize;
    _preview.frame = [QRScanUtil screenBounds];
    [self.view.layer insertSublayer:_preview atIndex:0];
    
    _preview.connection.videoOrientation = [QRScanUtil videoOrientationFromCurrentDeviceOrientation];
    
    [_session startRunning];
    
    CGRect screenRect = [QRScanUtil screenBounds];
    QRScanView *qrRectView = [[QRScanView alloc] initWithFrame:screenRect];
    qrRectView.transparentArea = CGSizeMake(screenRect.size.width - 110.0, screenRect.size.width - 110.0);
    qrRectView.backgroundColor = [UIColor clearColor];
    qrRectView.center = CGPointMake(screenRect.size.width / 2.0, screenRect.size.height / 2.0);
    [self.view addSubview:self.qrScanView = qrRectView];
    //修正扫描区域
    CGFloat screenHeight = self.view.frame.size.height;
    CGFloat screenWidth = self.view.frame.size.width;
    CGRect cropRect = CGRectMake((screenWidth - qrRectView.transparentArea.width) / 2,
                                 (screenHeight - qrRectView.transparentArea.height) / 2,
                                 qrRectView.transparentArea.width,
                                 qrRectView.transparentArea.height);
    
    [_output setRectOfInterest:CGRectMake(cropRect.origin.y / screenHeight,
                                          cropRect.origin.x / screenWidth,
                                          cropRect.size.height / screenHeight,
                                          cropRect.size.width / screenWidth)];
    
    UIView *navView = [[UIView alloc] init];
    navView.backgroundColor = [UIColor clearColor];
    [self.view addSubview:navView];
    
    [navView mas_makeConstraints:^(MASConstraintMaker *make) {
        if (@available(iOS 11.0, *)) {
            make.top.mas_equalTo(self.view.mas_safeAreaLayoutGuideTop);
        } else {
            make.top.mas_equalTo(20);
        }
        make.height.mas_equalTo(44);
        make.width.mas_equalTo(self.view);
    }];
    
    // 返回按钮
    UIButton *pop = [UIButton buttonWithType:UIButtonTypeCustom];
    [pop setImage:[UIImage imageNamed:@"arrowBack"] forState:UIControlStateNormal];
    [pop addTarget:self action:@selector(pop:) forControlEvents:UIControlEventTouchUpInside];
    [navView addSubview:pop];
    
    [pop mas_makeConstraints:^(MASConstraintMaker *make) {
        make.size.mas_equalTo(CGSizeMake(70, 44));
        make.top.left.mas_equalTo(0);
    }];
    
    // 标题
    UILabel *titleLabel = [[UILabel alloc] init];
    titleLabel.backgroundColor = [UIColor clearColor];
    titleLabel.text = @"二维码";
    titleLabel.font = [UIFont systemFontOfSize:18.0];
    titleLabel.textColor = [UIColor whiteColor];
    titleLabel.textAlignment = NSTextAlignmentCenter;
    [navView addSubview:titleLabel];
    
    [titleLabel mas_makeConstraints:^(MASConstraintMaker *make) {
        make.size.mas_equalTo(CGSizeMake(140, 44));
        make.top.mas_equalTo(0);
        make.center.mas_equalTo(navView);
    }];
    
}

- (void)buildAlertController
{
    NSString *appName = [[[NSBundle mainBundle] infoDictionary] objectForKey:@"CFBundleDisplayName"];
    NSString *message = [NSString stringWithFormat:@"%@没有获得照相机的使用权限，请在设置中开启", appName];
    UIAlertController *alert = [UIAlertController alertControllerWithTitle:nil message:message preferredStyle:UIAlertControllerStyleAlert];
    [alert addAction:[UIAlertAction actionWithTitle:@"取消" style:UIAlertActionStyleCancel handler:^(UIAlertAction *action) {
        if (self.qrCodeBlock) {
            self.qrCodeBlock(@"pop");
        }
        [self dismissViewControllerAnimated:YES completion:nil];
    }]];
    [alert addAction:[UIAlertAction actionWithTitle:@"开启" style:UIAlertActionStyleDefault handler:^(UIAlertAction *action) {
        [self dismissViewControllerAnimated:YES completion:nil];
        NSURL *url = [NSURL URLWithString:UIApplicationOpenSettingsURLString];
        if ([[UIApplication sharedApplication] canOpenURL:url]) {
            [[UIApplication sharedApplication] openURL:url];
        }
    }]];
    [self presentViewController:alert animated:YES completion:nil];
}

- (void)pop:(UIButton *)sender
{
    if (self.qrCodeBlock) {
        self.qrCodeBlock(@"pop");
    }
    [self dismissViewControllerAnimated:YES completion:^{
        [self.qrScanView.timer invalidate];
        [_session stopRunning];
        _session = nil;
    }];
}

- (void)playBeep
{
    NSURL *url = [[NSBundle mainBundle] URLForResource:@"Data/Raw/files/noticeMusic.wav" withExtension:nil];
    if (url) {
        SystemSoundID soundID;
        AudioServicesCreateSystemSoundID((__bridge CFURLRef)url, &soundID);
        AudioServicesPlaySystemSound(soundID);
    }
    AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
}

- (void)captureOutput:(AVCaptureOutput *)captureOutput didOutputMetadataObjects:(NSArray *)metadataObjects fromConnection:(AVCaptureConnection *)connection
{
    [_session stopRunning];
    [self.qrScanView.timer pauseTimer];
    NSString *stringValue = @"";
    if (metadataObjects.count > 0) {
        AVMetadataMachineReadableCodeObject *metadataObject = metadataObjects[0];
        stringValue = metadataObject.stringValue;
    }
//     播放 beep 声
    [self playBeep];
    [self dismissViewControllerAnimated:YES completion:^{
        [self.qrScanView.timer invalidate];
        [_session stopRunning];
        _session = nil;
    }];
    if (self.qrCodeBlock) {
        self.qrCodeBlock(stringValue);
    }
}

@end
