//
//  ImageCropController.m
//  Unity-iPhone
//
//  Created by luo on 2019/7/10.
//

#import "ImageCropController.h"
#import "JPImageresizeView/JPImageresizerConfigure.h"
#import "JPImageresizeView/JPImageresizerView.h"
#import "Masonry.h"

@interface ImageCropController ()
@property (weak, nonatomic) UIImageView *imageView;
@property (weak, nonatomic) UIButton *cancelButton;
@property (weak, nonatomic) UIButton *recoveryButton;
@property (weak, nonatomic) UIButton *doneButton;
@property (weak, nonatomic) UIButton *rotateButton;
@property (nonatomic, weak) JPImageresizerView *imageresizerView;
@end

@implementation ImageCropController

- (void)viewDidLoad {
    [super viewDidLoad];
    UIEdgeInsets contentInsets = UIEdgeInsetsMake(50, 0, (40 + 30 + 30 + 10), 0);
    BOOL isX = [UIScreen mainScreen].bounds.size.height > 736.0;
    if (isX) {
        contentInsets.top += 24;
        contentInsets.bottom += 34;
    }
    JPImageresizerConfigure *configure = [JPImageresizerConfigure defaultConfigureWithResizeImage:self.image make:^(JPImageresizerConfigure *configure) {
        configure
        .jp_resizeImage(self.image)
        .jp_maskAlpha(0.5)
        .jp_strokeColor([UIColor whiteColor])
        .jp_frameType(JPClassicFrameType)
        .jp_contentInsets(contentInsets)
        .jp_bgColor([UIColor blackColor])
        .jp_isClockwiseRotation(YES)
        .jp_resizeWHScale(1.0)
        .jp_animationCurve(JPAnimationCurveEaseOut);
    }];
    self.view.backgroundColor = configure.bgColor;
    
    self.recoveryButton.enabled = NO;
    
    __weak __typeof(self) wSelf = self;
    JPImageresizerView *imageresizerView = [JPImageresizerView imageresizerViewWithConfigure:configure imageresizerIsCanRecovery:^(BOOL isCanRecovery) {
        __strong __typeof(wSelf) sSelf = wSelf;
        if (!sSelf) return;
        // 当不需要重置设置按钮不可点
        sSelf.recoveryButton.enabled = isCanRecovery;
    } imageresizerIsPrepareToScale:^(BOOL isPrepareToScale) {
        __strong __typeof(wSelf) sSelf = wSelf;
        if (!sSelf) return;
        // 当预备缩放设置按钮不可点，结束后可点击
        BOOL enabled = !isPrepareToScale;
        sSelf.rotateButton.enabled = enabled;
    }];
    [self.view insertSubview:imageresizerView atIndex:0];
    self.imageresizerView = imageresizerView;
    // 注意：iOS11以下的系统，所在的controller最好设置automaticallyAdjustsScrollViewInsets为NO，不然就会随导航栏或状态栏的变化产生偏移
    if (@available(iOS 11.0, *)) {
        
    } else {
        self.automaticallyAdjustsScrollViewInsets = NO;
    }
    [self loadToolBarView];
}

- (void)loadToolBarView{
    UIView *toolBarView = [[UIView alloc] init];
    toolBarView.backgroundColor = [UIColor clearColor];
    [self.view addSubview:toolBarView];
    [toolBarView mas_makeConstraints:^(MASConstraintMaker *make) {
        make.height.mas_equalTo(49);
        make.width.mas_equalTo(self.view);
        if (@available(iOS 11.0, *)) {
            make.bottom.mas_equalTo(self.view.mas_safeAreaLayoutGuideBottom);
        } else {
            make.bottom.mas_equalTo(0);
        }
    }];
    
    UIButton *rotateButton = [UIButton buttonWithType:UIButtonTypeSystem];
    [rotateButton setTitle:@"旋转" forState:UIControlStateNormal];
    [rotateButton setTitleColor:[UIColor whiteColor] forState:UIControlStateNormal];
    [rotateButton addTarget:self action:@selector(rotate) forControlEvents:UIControlEventTouchUpInside];
    [rotateButton setContentHuggingPriority:UILayoutPriorityRequired
                                    forAxis:UILayoutConstraintAxisHorizontal];
    [self.view addSubview:rotateButton];
    [rotateButton mas_makeConstraints:^(MASConstraintMaker *make) {
        make.height.mas_equalTo(49);
        make.left.mas_equalTo(20);
        make.bottom.mas_equalTo(toolBarView.mas_top);
    }];
    
    UIButton *cancelButton = [UIButton buttonWithType:UIButtonTypeSystem];
    [cancelButton setTitle:@"取消" forState:UIControlStateNormal];
    [cancelButton setTitleColor:[UIColor whiteColor] forState:UIControlStateNormal];
    [cancelButton addTarget:self action:@selector(cancel) forControlEvents:UIControlEventTouchUpInside];
    [cancelButton setContentHuggingPriority:UILayoutPriorityRequired
                                    forAxis:UILayoutConstraintAxisHorizontal];
    [toolBarView addSubview:cancelButton];
    [cancelButton mas_makeConstraints:^(MASConstraintMaker *make) {
        make.left.mas_equalTo(20);
        make.centerY.mas_equalTo(toolBarView.mas_centerY);
    }];
    
    UIButton *recoveryButton = [UIButton buttonWithType:UIButtonTypeSystem];
    [recoveryButton setTitle:@"重置" forState:UIControlStateNormal];
    [recoveryButton setTitleColor:[UIColor whiteColor] forState:UIControlStateNormal];
    [recoveryButton addTarget:self action:@selector(recovery) forControlEvents:UIControlEventTouchUpInside];
    [recoveryButton setContentHuggingPriority:UILayoutPriorityRequired
                                    forAxis:UILayoutConstraintAxisHorizontal];
    [toolBarView addSubview:recoveryButton];
    [recoveryButton mas_makeConstraints:^(MASConstraintMaker *make) {
        make.center.mas_equalTo(toolBarView);
    }];
    
    UIButton *doneButton = [UIButton buttonWithType:UIButtonTypeSystem];
    [doneButton setTitle:@"完成" forState:UIControlStateNormal];
    [doneButton setTitleColor:[UIColor whiteColor] forState:UIControlStateNormal];
    [doneButton addTarget:self action:@selector(done) forControlEvents:UIControlEventTouchUpInside];
    [doneButton setContentHuggingPriority:UILayoutPriorityRequired
                                    forAxis:UILayoutConstraintAxisHorizontal];
    [toolBarView addSubview:doneButton];
    [doneButton mas_makeConstraints:^(MASConstraintMaker *make) {
        make.right.mas_equalTo(toolBarView.mas_right).mas_offset(-20);
        make.centerY.mas_equalTo(toolBarView.mas_centerY);
    }];
}

- (void)viewWillAppear:(BOOL)animated{
    [super viewWillAppear:animated];
    [self.navigationController setNavigationBarHidden:YES animated:true];

}

- (void)viewDidDisappear:(BOOL)animated{
    [super viewDidDisappear:animated];
    [self.navigationController setNavigationBarHidden:NO animated:true];
}

- (void)cancel {
    if (self.cancelBlock) {
        self.cancelBlock();
    }
}

- (void)recovery {
    [self.imageresizerView recovery];
}

- (void)done {
    self.recoveryButton.enabled = NO;
    
    __weak __typeof(self) wSelf = self;
    
    [self.imageresizerView imageresizerWithComplete:^(UIImage *resizeImage) {
        __strong __typeof(wSelf) sSelf = wSelf;
        if (!sSelf) return;
        
        if (!resizeImage) {
            return;
        }
        sSelf.recoveryButton.enabled = YES;
        if (sSelf.cropBlock) {
            sSelf.cropBlock(resizeImage);
        }
    }];
}

- (void)rotate {
    [self.imageresizerView rotation];
}

@end
