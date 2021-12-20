//
//  JPImageresizerFrameView.m
//  DesignSpaceRestructure
//
//  Created by 周健平 on 2017/12/11.
//  Copyright © 2017年 周健平. All rights reserved.
//

#import "JPImageresizerFrameView.h"
#import "UIImage+JPExtension.h"

/** keypath */
#define aKeyPath(objc, keyPath) @(((void)objc.keyPath, #keyPath))

struct JPRGBAColor {
    CGFloat jp_r;
    CGFloat jp_g;
    CGFloat jp_b;
    CGFloat jp_a;
};

typedef NS_ENUM(NSUInteger, JPRectHorn) {
    JPCenter,
    JPLeftTop,
    JPLeftMid,
    JPLeftBottom,
    
    JPRightTop,
    JPRightMid,
    JPRightBottom,
    
    JPTopMid,
    JPBottomMid
};

typedef NS_ENUM(NSUInteger, JPLinePosition) {
    JPHorizontalTop,
    JPHorizontalBottom,
    JPVerticalLeft,
    JPVerticalRight
};

@interface JPImageresizerFrameView ()

@property (nonatomic, strong) NSTimer *timer;

@property (nonatomic, weak) UIScrollView *scrollView;
@property (nonatomic, weak) UIImageView *imageView;

@property (nonatomic, weak) CAShapeLayer *bgLayer;
@property (nonatomic, weak) CAShapeLayer *frameLayer;

@property (nonatomic, weak) CAShapeLayer *leftTopDot;
@property (nonatomic, weak) CAShapeLayer *leftMidDot;
@property (nonatomic, weak) CAShapeLayer *leftBottomDot;
@property (nonatomic, weak) CAShapeLayer *rightTopDot;
@property (nonatomic, weak) CAShapeLayer *rightMidDot;
@property (nonatomic, weak) CAShapeLayer *rightBottomDot;
@property (nonatomic, weak) CAShapeLayer *topMidDot;
@property (nonatomic, weak) CAShapeLayer *bottomMidDot;

@property (nonatomic, weak) CAShapeLayer *horTopLine;
@property (nonatomic, weak) CAShapeLayer *horBottomLine;
@property (nonatomic, weak) CAShapeLayer *verLeftLine;
@property (nonatomic, weak) CAShapeLayer *verRightLine;

@property (nonatomic, assign) JPRectHorn currHorn;
@property (nonatomic, assign) CGPoint diagonal;

@property (nonatomic, assign) CGRect originImageFrame;

@property (nonatomic, assign) CGRect maxResizeFrame;
- (CGFloat)maxResizeX;
- (CGFloat)maxResizeY;
- (CGFloat)maxResizeW;
- (CGFloat)maxResizeH;

@property (nonatomic) CGFloat imageresizeX;
@property (nonatomic) CGFloat imageresizeY;
@property (nonatomic) CGFloat imageresizeW;
@property (nonatomic) CGFloat imageresizeH;

- (CGSize)imageresizerSize;
- (CGSize)imageViewSzie;

- (BOOL)isShowMidDot;

@property (nonatomic, weak) UIView *blurContentView;
@property (nonatomic, weak) UIVisualEffectView *blurEffectView;
@end

@implementation JPImageresizerFrameView
{
    NSTimeInterval _defaultDuration;
    NSTimeInterval _updateDuration;
    NSString *_kCAMediaTimingFunction;
    UIViewAnimationOptions _animationOption;
    
    CGFloat _dotWH;
    CGFloat _arrLineW;
    CGFloat _arrLength;
    CGFloat _scopeWH;
    CGFloat _minImageWH;
    
    CGFloat _baseImageW;
    CGFloat _baseImageH;
    
    CGFloat _startResizeW;
    CGFloat _startResizeH;
    
    CGFloat _originWHScale;
    
    CGFloat _verBaseMargin;
    CGFloat _horBaseMargin;
    
    CGFloat _diffHalfW;
    
    BOOL _isArbitrarily;
    
    struct JPRGBAColor _fillRgba;
    UIColor *_clearColor;
    
    BOOL _isHideBlurEffect;
    BOOL _isHideFrameLine;
    
    CGFloat _diffRotLength;
    CGRect _bgFrame; // 扩大旋转时的区域（防止旋转时有空白区域）
    
    CGSize _contentSize;
}

#pragma mark - setter

- (void)setOriginImageFrame:(CGRect)originImageFrame {
    _originImageFrame = originImageFrame;
    _originWHScale = originImageFrame.size.width / originImageFrame.size.height;
}

- (void)setFillColor:(UIColor *)fillColor {
    if (self.maskType == JPLightBlurMaskType) {
        fillColor = [UIColor whiteColor];
    } else if (self.maskType == JPDarkBlurMaskType) {
        fillColor = [UIColor blackColor];
    }
    _fillRgba = [self createRgbaWithColor:fillColor];
    _fillColor = [UIColor colorWithRed:_fillRgba.jp_r green:_fillRgba.jp_g blue:_fillRgba.jp_b alpha:_fillRgba.jp_a * _maskAlpha];
    _clearColor = [UIColor colorWithRed:_fillRgba.jp_r green:_fillRgba.jp_g blue:_fillRgba.jp_b alpha:0];
    if (self.blurContentView) {
        self.blurContentView.backgroundColor = _fillColor;
    } else {
        [CATransaction begin];
        [CATransaction setDisableActions:YES];
        self.bgLayer.fillColor = _fillColor.CGColor;
        [CATransaction commit];
    }
}

- (void)setMaskAlpha:(CGFloat)maskAlpha {
    _maskAlpha = maskAlpha;
    if (maskAlpha == 0) {
        _fillColor = _clearColor;
    } else {
        _fillColor = [UIColor colorWithRed:_fillRgba.jp_r green:_fillRgba.jp_g blue:_fillRgba.jp_b alpha:_fillRgba.jp_a * _maskAlpha];
    }
    
    if (self.blurContentView) {
        self.blurContentView.backgroundColor = _fillColor;
    } else {
        [CATransaction begin];
        [CATransaction setDisableActions:YES];
        self.bgLayer.fillColor = _fillColor.CGColor;
        [CATransaction commit];
    }
}

- (void)setStrokeColor:(UIColor *)strokeColor {
    _strokeColor = strokeColor;
    CGColorRef strokeCGColor = strokeColor.CGColor;
    CGColorRef clearCGColor = [UIColor clearColor].CGColor;
    [CATransaction begin];
    [CATransaction setDisableActions:YES];
    _frameLayer.strokeColor = strokeCGColor;
    if (_frameType == JPConciseFrameType ||
        _frameType == JPConciseWithoutOtherDotFrameType) {
        
        _leftTopDot.fillColor = strokeCGColor;
        _leftBottomDot.fillColor = strokeCGColor;
        _rightTopDot.fillColor = strokeCGColor;
        _rightBottomDot.fillColor = strokeCGColor;
        
        _leftTopDot.strokeColor = clearCGColor;
        _leftBottomDot.strokeColor = clearCGColor;
        _rightTopDot.strokeColor = clearCGColor;
        _rightBottomDot.strokeColor = clearCGColor;
        
        _leftMidDot.fillColor = strokeCGColor;
        _rightMidDot.fillColor = strokeCGColor;
        _topMidDot.fillColor = strokeCGColor;
        _bottomMidDot.fillColor = strokeCGColor;
        
    } else {
        _leftTopDot.strokeColor = strokeCGColor;
        _leftBottomDot.strokeColor = strokeCGColor;
        _rightTopDot.strokeColor = strokeCGColor;
        _rightBottomDot.strokeColor = strokeCGColor;
        
        _leftTopDot.fillColor = clearCGColor;
        _leftBottomDot.fillColor = clearCGColor;
        _rightTopDot.fillColor = clearCGColor;
        _rightBottomDot.fillColor = clearCGColor;
        
        _horTopLine.strokeColor = strokeCGColor;
        _horBottomLine.strokeColor = strokeCGColor;
        _verLeftLine.strokeColor = strokeCGColor;
        _verRightLine.strokeColor = strokeCGColor;
    }
    [CATransaction commit];
}

- (void)setImageresizerFrame:(CGRect)imageresizerFrame {
    [self updateImageresizerFrame:imageresizerFrame animateDuration:-1.0];
}

- (void)setImageresizeX:(CGFloat)imageresizeX {
    _imageresizerFrame.origin.x = imageresizeX;
}

- (void)setImageresizeY:(CGFloat)imageresizeY {
    _imageresizerFrame.origin.y = imageresizeY;
}

- (void)setImageresizeW:(CGFloat)imageresizeW {
    _imageresizerFrame.size.width = imageresizeW;
}

- (void)setImageresizeH:(CGFloat)imageresizeH {
    _imageresizerFrame.size.height = imageresizeH;
}

- (void)setResizeWHScale:(CGFloat)resizeWHScale {
    [self setResizeWHScale:resizeWHScale animated:NO];
}

- (void)setResizeWHScale:(CGFloat)resizeWHScale animated:(BOOL)isAnimated {
    if (resizeWHScale > 0 && [self isHorizontalDirection:_rotationDirection]) resizeWHScale = 1.0 / resizeWHScale;
    if (_resizeWHScale == resizeWHScale) return;
    _resizeWHScale = resizeWHScale;
    
    _updateDuration = isAnimated ? _defaultDuration : -1.0;
    
    _isArbitrarily = resizeWHScale <= 0;
    
    if (_frameType == JPConciseFrameType) {
        CGFloat midDotOpacity = 1;
        if (!_isArbitrarily) midDotOpacity = 0;
        _leftMidDot.opacity = midDotOpacity;
        _rightMidDot.opacity = midDotOpacity;
        _topMidDot.opacity = midDotOpacity;
        _bottomMidDot.opacity = midDotOpacity;
    }
    
    if (self.superview) [self updateImageOriginFrameWithDirection:_rotationDirection];
}

- (void)setFrameType:(JPImageresizerFrameType)frameType {
    _frameType = frameType;
    CGFloat lineW = 0;
    if (frameType == JPConciseFrameType ||
        frameType == JPConciseWithoutOtherDotFrameType) {
        if (frameType == JPConciseFrameType) {
            [self leftMidDot];
            [self rightMidDot];
            [self topMidDot];
            [self bottomMidDot];
        } else {
            [_leftMidDot removeFromSuperlayer];
            [_rightMidDot removeFromSuperlayer];
            [_topMidDot removeFromSuperlayer];
            [_bottomMidDot removeFromSuperlayer];
        }
        [_horTopLine removeFromSuperlayer];
        [_horBottomLine removeFromSuperlayer];
        [_verLeftLine removeFromSuperlayer];
        [_verRightLine removeFromSuperlayer];
        _isHideFrameLine = NO;
    } else {
        [self horTopLine];
        [self horBottomLine];
        [self verLeftLine];
        [self verRightLine];
        [_leftMidDot removeFromSuperlayer];
        [_rightMidDot removeFromSuperlayer];
        [_topMidDot removeFromSuperlayer];
        [_bottomMidDot removeFromSuperlayer];
        lineW = _arrLineW;
    }
    self.leftTopDot.lineWidth = lineW;
    self.leftBottomDot.lineWidth = lineW;
    self.rightTopDot.lineWidth = lineW;
    self.rightBottomDot.lineWidth = lineW;
}

- (void)setAnimationCurve:(JPAnimationCurve)animationCurve {
    _animationCurve = animationCurve;
    switch (animationCurve) {
        case JPAnimationCurveEaseInOut:
            _kCAMediaTimingFunction = kCAMediaTimingFunctionEaseInEaseOut;
            _animationOption = UIViewAnimationOptionCurveEaseInOut;
            break;
        case JPAnimationCurveEaseIn:
            _kCAMediaTimingFunction = kCAMediaTimingFunctionEaseIn;
            _animationOption = UIViewAnimationOptionCurveEaseIn;
            break;
        case JPAnimationCurveEaseOut:
            _kCAMediaTimingFunction = kCAMediaTimingFunctionEaseOut;
            _animationOption = UIViewAnimationOptionCurveEaseOut;
            break;
        case JPAnimationCurveLinear:
            _kCAMediaTimingFunction = kCAMediaTimingFunctionLinear;
            _animationOption = UIViewAnimationOptionCurveLinear;
            break;
    }
}

- (void)setIsCanRecovery:(BOOL)isCanRecovery {
    _isCanRecovery = isCanRecovery;
    !self.imageresizerIsCanRecovery ? : self.imageresizerIsCanRecovery(isCanRecovery);
}

- (void)setIsPrepareToScale:(BOOL)isPrepareToScale {
    _isPrepareToScale = isPrepareToScale;
    !self.imageresizerIsPrepareToScale ? : self.imageresizerIsPrepareToScale(isPrepareToScale);
}

#pragma mark - getter

- (CGFloat)maxResizeX {
    return self.maxResizeFrame.origin.x;
}

- (CGFloat)maxResizeY {
    return self.maxResizeFrame.origin.y;
}

- (CGFloat)maxResizeW {
    return self.maxResizeFrame.size.width;
}

- (CGFloat)maxResizeH {
    return self.maxResizeFrame.size.height;
}

- (CGFloat)imageresizeX {
    return _imageresizerFrame.origin.x;
}

- (CGFloat)imageresizeY {
    return _imageresizerFrame.origin.y;
}

- (CGFloat)imageresizeW {
    return _imageresizerFrame.size.width;
}

- (CGFloat)imageresizeH {
    return _imageresizerFrame.size.height;
}

- (CGSize)imageresizerSize {
    CGFloat w = ((NSInteger)(self.imageresizerFrame.size.width)) * 1.0;
    CGFloat h = ((NSInteger)(self.imageresizerFrame.size.height)) * 1.0;
    return CGSizeMake(w, h);
}

- (CGSize)imageViewSzie {
    CGFloat w = ((NSInteger)(self.imageView.frame.size.width)) * 1.0;
    CGFloat h = ((NSInteger)(self.imageView.frame.size.height)) * 1.0;
    return [self isHorizontalDirection:_rotationDirection] ? CGSizeMake(h, w) : CGSizeMake(w, h);
}

- (CAShapeLayer *)leftTopDot {
    if (!_leftTopDot) _leftTopDot = [self createShapeLayer:0];
    return _leftTopDot;
}

- (CAShapeLayer *)leftMidDot {
    if (!_leftMidDot) _leftMidDot = [self createShapeLayer:0];
    return _leftMidDot;
}

- (CAShapeLayer *)leftBottomDot {
    if (!_leftBottomDot) _leftBottomDot = [self createShapeLayer:0];
    return _leftBottomDot;
}

- (CAShapeLayer *)rightTopDot {
    if (!_rightTopDot) _rightTopDot = [self createShapeLayer:0];
    return _rightTopDot;
}

- (CAShapeLayer *)rightMidDot {
    if (!_rightMidDot) _rightMidDot = [self createShapeLayer:0];
    return _rightMidDot;
}

- (CAShapeLayer *)rightBottomDot {
    if (!_rightBottomDot) _rightBottomDot = [self createShapeLayer:0];
    return _rightBottomDot;
}

- (CAShapeLayer *)topMidDot {
    if (!_topMidDot) _topMidDot = [self createShapeLayer:0];
    return _topMidDot;
}

- (CAShapeLayer *)bottomMidDot {
    if (!_bottomMidDot) _bottomMidDot = [self createShapeLayer:0];
    return _bottomMidDot;
}

- (CAShapeLayer *)horTopLine {
    if (!_horTopLine) _horTopLine = [self createShapeLayer:0.5];
    return _horTopLine;
}

- (CAShapeLayer *)horBottomLine {
    if (!_horBottomLine) _horBottomLine = [self createShapeLayer:0.5];
    return _horBottomLine;
}

- (CAShapeLayer *)verLeftLine {
    if (!_verLeftLine) _verLeftLine = [self createShapeLayer:0.5];
    return _verLeftLine;
}

- (CAShapeLayer *)verRightLine {
    if (!_verRightLine) _verRightLine = [self createShapeLayer:0.5];
    return _verRightLine;
}

- (BOOL)edgeLineIsEnabled {
    if (_isArbitrarily) {
        return _edgeLineIsEnabled;
    } else {
        return NO;
    }
}

#pragma mark - init

- (instancetype)initWithFrame:(CGRect)frame
                  contentSize:(CGSize)contentSize
                     maskType:(JPImageresizerMaskType)maskType
                    frameType:(JPImageresizerFrameType)frameType
               animationCurve:(JPAnimationCurve)animationCurve
                  strokeColor:(UIColor *)strokeColor
                    fillColor:(UIColor *)fillColor
                    maskAlpha:(CGFloat)maskAlpha
                verBaseMargin:(CGFloat)verBaseMargin
                horBaseMargin:(CGFloat)horBaseMargin
                resizeWHScale:(CGFloat)resizeWHScale
                   scrollView:(UIScrollView *)scrollView
                    imageView:(UIImageView *)imageView
    imageresizerIsCanRecovery:(JPImageresizerIsCanRecoveryBlock)imageresizerIsCanRecovery
 imageresizerIsPrepareToScale:(JPImageresizerIsPrepareToScaleBlock)imageresizerIsPrepareToScale {
    
    if (self = [super initWithFrame:frame]) {
        self.clipsToBounds = NO;
        
        _edgeLineIsEnabled = YES;
        
        _defaultDuration = 0.27;
        _updateDuration = -1.0;
        _dotWH = 10.0;
        _arrLineW = 2.5;
        _arrLength = 20.0;
        _scopeWH = 50.0;
        _minImageWH = 70.0;
        _rotationDirection = JPImageresizerVerticalUpDirection;
        
        _contentSize = contentSize;
        _maskType = maskType;
        _horBaseMargin = horBaseMargin;
        _verBaseMargin = verBaseMargin;
        _imageresizerIsCanRecovery = [imageresizerIsCanRecovery copy];
        _imageresizerIsPrepareToScale = [imageresizerIsPrepareToScale copy];
        
        _diffRotLength = 1000;
        _bgFrame = CGRectMake(self.bounds.origin.x - _diffRotLength,
                              self.bounds.origin.y - _diffRotLength,
                              self.bounds.size.width + _diffRotLength * 2,
                              self.bounds.size.height + _diffRotLength * 2);
        if (maskType != JPNormalMaskType) {
            UIView *blurContentView = [[UIView alloc] initWithFrame:_bgFrame];
            [self addSubview:blurContentView];
            self.blurContentView = blurContentView;
            
            UIBlurEffect *blurEffect;
            if (maskType == JPLightBlurMaskType) {
                blurEffect = [UIBlurEffect effectWithStyle:UIBlurEffectStyleLight];
            } else {
                blurEffect = [UIBlurEffect effectWithStyle:UIBlurEffectStyleDark];
            }
            UIVisualEffectView *blurEffectView = [[UIVisualEffectView alloc] initWithEffect:blurEffect];
            blurEffectView.frame = blurContentView.bounds;
            [blurContentView addSubview:blurEffectView];
            self.blurEffectView = blurEffectView;
            
            CAShapeLayer *bgLayer = [CAShapeLayer layer];
            bgLayer.frame = blurContentView.bounds;
            bgLayer.fillColor = [UIColor blackColor].CGColor;
            blurContentView.layer.mask = bgLayer;
            self.bgLayer = bgLayer;
        } else {
            self.bgLayer = [self createShapeLayer:0];
            
        }
        self.bgLayer.fillRule = kCAFillRuleEvenOdd;
        
        CAShapeLayer *frameLayer = [self createShapeLayer:1.0];
        frameLayer.fillColor = [UIColor clearColor].CGColor;
        self.frameLayer = frameLayer;
        
        self.frameType = frameType;
        self.animationCurve = animationCurve;
        self.scrollView = scrollView;
        self.imageView = imageView;
        
        _maskAlpha = maskAlpha;
        self.fillColor = fillColor;
        self.strokeColor = strokeColor;
        
        if (resizeWHScale == _resizeWHScale) _resizeWHScale = resizeWHScale - 1.0;
        self.resizeWHScale = resizeWHScale;
        
        UIPanGestureRecognizer *panGR = [[UIPanGestureRecognizer alloc] initWithTarget:self action:@selector(panHandle:)];
        [self addGestureRecognizer:panGR];
        _panGR = panGR;
    }
    return self;
}

#pragma mark - life cycle

- (void)didMoveToSuperview {
    [super didMoveToSuperview];
    if (self.superview) [self updateImageOriginFrameWithDirection:_rotationDirection];
}

- (void)dealloc {
    [self willDie];
}

#pragma mark - timer

- (BOOL)addTimer {
    BOOL isHasTimer = [self removeTimer];
    self.timer = [NSTimer scheduledTimerWithTimeInterval:0.65 target:self selector:@selector(timerHandle) userInfo:nil repeats:NO]; // default 0.65
    [[NSRunLoop mainRunLoop] addTimer:self.timer forMode:NSRunLoopCommonModes];
    return isHasTimer;
}

- (BOOL)removeTimer {
    if (self.timer) {
        [self.timer invalidate];
        self.timer = nil;
        return YES;
    }
    return NO;
}

- (void)timerHandle {
    [self removeTimer];
    [self updateImageresizerFrameWithAnimateDuration:_defaultDuration];
}

#pragma mark - assist method

- (void)willDie {
    self.window.userInteractionEnabled = YES;
    [self removeTimer];
}

- (CAShapeLayer *)createShapeLayer:(CGFloat)lineWidth {
    CAShapeLayer *shapeLayer = [CAShapeLayer layer];
    shapeLayer.frame = self.bounds;
    shapeLayer.lineWidth = lineWidth;
    [self.layer addSublayer:shapeLayer];
    return shapeLayer;
}

- (BOOL)isShowMidDot {
    return _isArbitrarily && _frameType == JPConciseFrameType;
}

- (BOOL)isHorizontalDirection:(JPImageresizerRotationDirection)direction {
    return (direction == JPImageresizerHorizontalLeftDirection || direction == JPImageresizerHorizontalRightDirection);
}

- (UIBezierPath *)dotPathWithPosition:(CGPoint)position {
    CGFloat dotWH = _dotWH;
    UIBezierPath *dotPath = [UIBezierPath bezierPathWithOvalInRect:CGRectMake(position.x - dotWH * 0.5, position.y - dotWH * 0.5, dotWH, dotWH)];
    return dotPath;
}

- (UIBezierPath *)arrPathWithPosition:(CGPoint)position rectHorn:(JPRectHorn)horn {
    CGFloat arrLineW = _arrLineW;
    CGFloat arrLength = _arrLength;
    UIBezierPath *path = [UIBezierPath bezierPath];
    CGFloat halfArrLineW = arrLineW * 0.5;
    CGPoint firstPoint = CGPointZero;
    CGPoint secondPoint = CGPointZero;
    CGPoint thirdPoint = CGPointZero;
    switch (horn) {
        case JPLeftTop:
        {
            position.x -= halfArrLineW;
            position.y -= halfArrLineW;
            firstPoint = CGPointMake(position.x, position.y + arrLength);
            thirdPoint = CGPointMake(position.x + arrLength, position.y);
            break;
        }
            
        case JPLeftBottom:
        {
            position.x -= halfArrLineW;
            position.y += halfArrLineW;
            firstPoint = CGPointMake(position.x, position.y - arrLength);
            thirdPoint = CGPointMake(position.x + arrLength, position.y);
            break;
        }
            
        case JPRightTop:
        {
            position.x += halfArrLineW;
            position.y -= halfArrLineW;
            firstPoint = CGPointMake(position.x - arrLength, position.y);
            thirdPoint = CGPointMake(position.x, position.y + arrLength);
            break;
        }
            
        case JPRightBottom:
        {
            position.x += halfArrLineW;
            position.y += halfArrLineW;
            firstPoint = CGPointMake(position.x - arrLength, position.y);
            thirdPoint = CGPointMake(position.x, position.y - arrLength);
            break;
        }
            
        default:
        {
            firstPoint = position;
            thirdPoint = position;
            break;
        }
    }
    secondPoint = position;
    [path moveToPoint:firstPoint];
    [path addLineToPoint:secondPoint];
    [path addLineToPoint:thirdPoint];
    return path;
}

- (UIBezierPath *)linePathWithLinePosition:(JPLinePosition)linePosition location:(CGPoint)location length:(CGFloat)length {
    UIBezierPath *path = [UIBezierPath bezierPath];
    CGPoint point = CGPointZero;
    switch (linePosition) {
        case JPHorizontalTop:
        case JPHorizontalBottom:
        {
            point = CGPointMake(location.x + length, location.y);
            break;
        }
        case JPVerticalLeft:
        case JPVerticalRight:
        {
            point = CGPointMake(location.x, location.y + length);
            break;
        }
    }
    [path moveToPoint:location];
    [path addLineToPoint:point];
    return path;
}

- (BOOL)imageresizerFrameIsFullImageViewFrame {
    CGSize imageresizerSize = self.imageresizerSize;
    CGSize imageViewSzie = self.imageViewSzie;
    return (fabs(imageresizerSize.width - imageViewSzie.width) <= 1 &&
            fabs(imageresizerSize.height - imageViewSzie.height) <= 1);
}

- (BOOL)imageresizerFrameIsEqualImageViewFrame {
    CGSize imageresizerSize = self.imageresizerSize;
    CGSize imageViewSzie = self.imageViewSzie;
    CGFloat resizeWHScale = [self isHorizontalDirection:_rotationDirection] ? (1.0 / _resizeWHScale) : _resizeWHScale;
    if (_isArbitrarily || (resizeWHScale == _originWHScale)) {
        return (fabs(imageresizerSize.width - imageViewSzie.width) <= 1 &&
                fabs(imageresizerSize.height - imageViewSzie.height) <= 1);
    } else {
        return (fabs(imageresizerSize.width - imageViewSzie.width) <= 1 ||
                fabs(imageresizerSize.height - imageViewSzie.height) <= 1);
    }
}

#pragma mark - private method

- (struct JPRGBAColor)createRgbaWithColor:(UIColor *)color {
    CGFloat r, g, b, a;
    [color getRed:&r green:&g blue:&b alpha:&a];
    struct JPRGBAColor rgba;
    rgba.jp_r = r;
    rgba.jp_g = g;
    rgba.jp_b = b;
    rgba.jp_a = a;
    return rgba;
}

- (void)hideOrShowBlurEffect:(BOOL)isHide animateDuration:(NSTimeInterval)duration {
    if (self.maskType == JPNormalMaskType) return;
    if (_isHideBlurEffect == isHide) return;
    _isHideBlurEffect = isHide;
    CGFloat toOpacity = isHide ? 0 : 1;
    if (duration > 0 && ((_blurContentView != nil) || (_blurContentView == nil && ![self imageresizerFrameIsFullImageViewFrame]))) {
        CGFloat fromOpacity = isHide ? 1 : 0;
        CABasicAnimation *anim = [CABasicAnimation animationWithKeyPath:aKeyPath(self.blurEffectView.layer, opacity)];
        anim.fillMode = kCAFillModeBackwards;
        anim.fromValue = @(fromOpacity);
        anim.toValue = @(toOpacity);
        anim.duration = duration;
        anim.timingFunction = [CAMediaTimingFunction functionWithName:_kCAMediaTimingFunction];
        [self.blurEffectView.layer addAnimation:anim forKey:@"opacity"];
    }
    [CATransaction begin];
    [CATransaction setDisableActions:YES];
    self.blurEffectView.layer.opacity = toOpacity;
    [CATransaction commit];
}

- (void)hideOrShowFrameLine:(BOOL)isHide animateDuration:(NSTimeInterval)duration {
    if (self.frameType != JPClassicFrameType) return;
    if (_isHideFrameLine == isHide) return;
    _isHideFrameLine = isHide;
    CGFloat toOpacity = isHide ? 0 : 1;
    if (duration > 0) {
        CGFloat fromOpacity = isHide ? 1 : 0;
        CABasicAnimation *anim = [CABasicAnimation animationWithKeyPath:@"opacity"];
        anim.fillMode = kCAFillModeBackwards;
        anim.fromValue = @(fromOpacity);
        anim.toValue = @(toOpacity);
        anim.duration = duration;
        anim.timingFunction = [CAMediaTimingFunction functionWithName:_kCAMediaTimingFunction];
        [_horTopLine addAnimation:anim forKey:@"opacity"];
        [_horBottomLine addAnimation:anim forKey:@"opacity"];
        [_verLeftLine addAnimation:anim forKey:@"opacity"];
        [_verRightLine addAnimation:anim forKey:@"opacity"];
    }
    [CATransaction begin];
    [CATransaction setDisableActions:YES];
    _horTopLine.opacity = toOpacity;
    _horBottomLine.opacity = toOpacity;
    _verLeftLine.opacity = toOpacity;
    _verRightLine.opacity = toOpacity;
    [CATransaction commit];
}

- (CGRect)baseImageresizerFrame {
    if (_isArbitrarily) {
        return self.originImageFrame;
    } else {
        CGFloat w = 0;
        CGFloat h = 0;
        if ([self isHorizontalDirection:_rotationDirection]) {
            h = _baseImageW;
            w = h * _resizeWHScale;
            if (w > self.maxResizeW) {
                w = self.maxResizeW;
                h = w / _resizeWHScale;
            }
        } else {
            w = _baseImageW;
            h = w / _resizeWHScale;
            if (h > self.maxResizeH) {
                h = self.maxResizeH;
                w = h * _resizeWHScale;
            }
        }
        CGFloat x = self.maxResizeX + (self.maxResizeW - w) * 0.5;
        CGFloat y = self.maxResizeY + (self.maxResizeH - h) * 0.5;
        return CGRectMake(x, y, w, h);
    }
}

- (void)updateImageresizerFrame:(CGRect)imageresizerFrame animateDuration:(NSTimeInterval)duration {
    _imageresizerFrame = imageresizerFrame;
    
    CGFloat imageresizerX = imageresizerFrame.origin.x;
    CGFloat imageresizerY = imageresizerFrame.origin.y;
    CGFloat imageresizerMidX = CGRectGetMidX(imageresizerFrame);
    CGFloat imageresizerMidY = CGRectGetMidY(imageresizerFrame);
    CGFloat imageresizerMaxX = CGRectGetMaxX(imageresizerFrame);
    CGFloat imageresizerMaxY = CGRectGetMaxY(imageresizerFrame);
    
    UIBezierPath *leftTopDotPath;
    UIBezierPath *leftBottomDotPath;
    UIBezierPath *rightTopDotPath;
    UIBezierPath *rightBottomDotPath;
    
    UIBezierPath *leftMidDotPath;
    UIBezierPath *rightMidDotPath;
    UIBezierPath *topMidDotPath;
    UIBezierPath *bottomMidDotPath;
    
    UIBezierPath *horTopLinePath;
    UIBezierPath *horBottomLinePath;
    UIBezierPath *verLeftLinePath;
    UIBezierPath *verRightLinePath;
    
    if (_frameType == JPConciseFrameType || _frameType == JPConciseWithoutOtherDotFrameType) {
        leftTopDotPath = [self dotPathWithPosition:CGPointMake(imageresizerX, imageresizerY)];
        leftBottomDotPath = [self dotPathWithPosition:CGPointMake(imageresizerX, imageresizerMaxY)];
        rightTopDotPath = [self dotPathWithPosition:CGPointMake(imageresizerMaxX, imageresizerY)];
        rightBottomDotPath = [self dotPathWithPosition:CGPointMake(imageresizerMaxX, imageresizerMaxY)];
        
        if (_frameType == JPConciseFrameType) {
            leftMidDotPath = [self dotPathWithPosition:CGPointMake(imageresizerX, imageresizerMidY)];
            rightMidDotPath = [self dotPathWithPosition:CGPointMake(imageresizerMaxX, imageresizerMidY)];
            topMidDotPath = [self dotPathWithPosition:CGPointMake(imageresizerMidX, imageresizerY)];
            bottomMidDotPath = [self dotPathWithPosition:CGPointMake(imageresizerMidX, imageresizerMaxY)];
        }
        
    } else {
        leftTopDotPath = [self arrPathWithPosition:CGPointMake(imageresizerX, imageresizerY) rectHorn:JPLeftTop];
        leftBottomDotPath = [self arrPathWithPosition:CGPointMake(imageresizerX, imageresizerMaxY) rectHorn:JPLeftBottom];
        rightTopDotPath = [self arrPathWithPosition:CGPointMake(imageresizerMaxX, imageresizerY) rectHorn:JPRightTop];
        rightBottomDotPath = [self arrPathWithPosition:CGPointMake(imageresizerMaxX, imageresizerMaxY) rectHorn:JPRightBottom];
        
        CGFloat imageresizerW = imageresizerFrame.size.width;
        CGFloat imageresizerH = imageresizerFrame.size.height;
        CGFloat oneThirdW = imageresizerW / 3.0;
        CGFloat oneThirdH = imageresizerH / 3.0;
        
        horTopLinePath = [self linePathWithLinePosition:JPHorizontalTop location:CGPointMake(imageresizerX, imageresizerY + oneThirdH) length:imageresizerW];
        horBottomLinePath = [self linePathWithLinePosition:JPHorizontalBottom location:CGPointMake(imageresizerX, imageresizerY + oneThirdH * 2) length:imageresizerW];
        verLeftLinePath = [self linePathWithLinePosition:JPVerticalLeft location:CGPointMake(imageresizerX + oneThirdW, imageresizerY) length:imageresizerH];
        verRightLinePath = [self linePathWithLinePosition:JPVerticalRight location:CGPointMake(imageresizerX + oneThirdW * 2, imageresizerY) length:imageresizerH];
    }
    
    UIBezierPath *bgPath;
    UIBezierPath *framePath = [UIBezierPath bezierPathWithRect:imageresizerFrame];
    if (self.blurContentView) {
        bgPath = [UIBezierPath bezierPathWithRect:self.blurContentView.bounds];
        CGRect frame = imageresizerFrame;
        frame.origin.x += _diffRotLength;
        frame.origin.y += _diffRotLength;
        [bgPath appendPath:[UIBezierPath bezierPathWithRect:frame]];
    } else {
        bgPath = [UIBezierPath bezierPathWithRect:_bgFrame];
        [bgPath appendPath:framePath];
    }
    
    if (duration > 0) {
        __weak __typeof(self) wSelf = self;
        void (^layerPathAnimate)(CAShapeLayer *layer, UIBezierPath *path) = ^(CAShapeLayer *layer, UIBezierPath *path) {
            __strong __typeof(wSelf) sSelf = wSelf;
            if (!sSelf) return;
            CABasicAnimation *anim = [CABasicAnimation animationWithKeyPath:aKeyPath(layer, path)];
            anim.fillMode = kCAFillModeBackwards;
            anim.fromValue = [UIBezierPath bezierPathWithCGPath:layer.path];
            anim.toValue = path;
            anim.duration = duration;
            anim.timingFunction = [CAMediaTimingFunction functionWithName:sSelf->_kCAMediaTimingFunction];
            [layer addAnimation:anim forKey:@"path"];
        };
        
        layerPathAnimate(_leftTopDot, leftTopDotPath);
        layerPathAnimate(_leftBottomDot, leftBottomDotPath);
        layerPathAnimate(_rightTopDot, rightTopDotPath);
        layerPathAnimate(_rightBottomDot, rightBottomDotPath);
        if (_frameType == JPConciseFrameType) {
            layerPathAnimate(_leftMidDot, leftMidDotPath);
            layerPathAnimate(_rightMidDot, rightMidDotPath);
            layerPathAnimate(_topMidDot, topMidDotPath);
            layerPathAnimate(_bottomMidDot, bottomMidDotPath);
        } else if (_frameType == JPClassicFrameType) {
            layerPathAnimate(_horTopLine, horTopLinePath);
            layerPathAnimate(_horBottomLine, horBottomLinePath);
            layerPathAnimate(_verLeftLine, verLeftLinePath);
            layerPathAnimate(_verRightLine, verRightLinePath);
        }
        layerPathAnimate(_bgLayer, bgPath);
        layerPathAnimate(_frameLayer, framePath);
    }
    
    [CATransaction begin];
    [CATransaction setDisableActions:YES];
    _leftTopDot.path = leftTopDotPath.CGPath;
    _leftBottomDot.path = leftBottomDotPath.CGPath;
    _rightTopDot.path = rightTopDotPath.CGPath;
    _rightBottomDot.path = rightBottomDotPath.CGPath;
    if (_frameType == JPConciseFrameType) {
        _leftMidDot.path = leftMidDotPath.CGPath;
        _rightMidDot.path = rightMidDotPath.CGPath;
        _topMidDot.path = topMidDotPath.CGPath;
        _bottomMidDot.path = bottomMidDotPath.CGPath;
    } else if (_frameType == JPClassicFrameType) {
        _horTopLine.path = horTopLinePath.CGPath;
        _horBottomLine.path = horBottomLinePath.CGPath;
        _verLeftLine.path = verLeftLinePath.CGPath;
        _verRightLine.path = verRightLinePath.CGPath;
    }
    _bgLayer.path = bgPath.CGPath;
    _frameLayer.path = framePath.CGPath;
    [CATransaction commit];
}

- (void)updateImageOriginFrameWithDirection:(JPImageresizerRotationDirection)rotationDirection {
    [self removeTimer];
    _baseImageW = self.imageView.bounds.size.width;
    _baseImageH = self.imageView.bounds.size.height;
    _diffHalfW = (self.bounds.size.width - _contentSize.width) * 0.5;
    CGFloat x = (self.bounds.size.width - _baseImageW) * 0.5;
    CGFloat y = (self.bounds.size.height - _baseImageH) * 0.5;
    self.originImageFrame = CGRectMake(x, y, _baseImageW, _baseImageH);
    [self updateRotationDirection:rotationDirection];
    [self updateImageresizerFrame:[self baseImageresizerFrame] animateDuration:_updateDuration];
    [self updateImageresizerFrameWithAnimateDuration:_updateDuration];
    _updateDuration = -1.0;
}

- (void)updateRotationDirection:(JPImageresizerRotationDirection)rotationDirection {
    [self updateMaxResizeFrameWithDirection:rotationDirection];
    if (!_isArbitrarily) {
        BOOL isSwitchVerHor = [self isHorizontalDirection:_rotationDirection] != [self isHorizontalDirection:rotationDirection];
        if (isSwitchVerHor) _resizeWHScale = 1.0 / _resizeWHScale;
    }
    _rotationDirection = rotationDirection;
}

- (void)updateMaxResizeFrameWithDirection:(JPImageresizerRotationDirection)direction {
    CGFloat x = 0;
    CGFloat y = 0;
    CGFloat w = self.bounds.size.width;
    CGFloat h = self.bounds.size.height;
    if ([self isHorizontalDirection:direction]) {
        x = (w - _contentSize.height) * 0.5 +  _verBaseMargin;
        y = (h - _contentSize.width) * 0.5 + _horBaseMargin;
    } else {
        x = _diffHalfW + _horBaseMargin;
        y = _verBaseMargin;
    }
    w -= 2 * x;
    h -= 2 * y;
    self.maxResizeFrame = CGRectMake(x, y, w, h);
    
    _frameLayer.lineWidth = 1.0;
    CGFloat lineW = 0;
    if (_frameType == JPClassicFrameType) lineW = _arrLineW;
    _leftTopDot.lineWidth = lineW;
    _leftBottomDot.lineWidth = lineW;
    _rightTopDot.lineWidth = lineW;
    _rightBottomDot.lineWidth = lineW;
    
    lineW = 0.5;
    _horTopLine.lineWidth = lineW;
    _horBottomLine.lineWidth = lineW;
    _verLeftLine.lineWidth = lineW;
    _verRightLine.lineWidth = lineW;
}

- (void)updateImageresizerFrameWithAnimateDuration:(NSTimeInterval)duration {
    CGRect adjustResizeFrame = [self adjustResizeFrame];
    
    // contentInset
    UIEdgeInsets contentInset = [self scrollViewContentInsetWithAdjustResizeFrame:adjustResizeFrame];

    // contentOffset
    CGPoint contentOffset = CGPointZero;
    CGPoint origin = self.imageresizerFrame.origin;
    CGPoint convertPoint = [self convertPoint:origin toView:self.imageView];
    // 这个convertPoint是相对self.imageView.bounds上的点，所以要✖️zoomScale拿到相对frame实际显示的大小
    contentOffset.x = -contentInset.left + convertPoint.x * self.scrollView.zoomScale;
    contentOffset.y = -contentInset.top + convertPoint.y * self.scrollView.zoomScale;
    
    // minimumZoomScale
    self.scrollView.minimumZoomScale = [self scrollViewMinZoomScaleWithResizeSize:adjustResizeFrame.size];
    
    // zoomFrame
    // 根据裁剪的区域，因为需要有间距，所以拼接成self的尺寸获取缩放的区域zoomFrame
    // 宽高比不变，所以宽度高度的比例是一样，这里就用宽度比例吧
    CGFloat convertScale = self.imageresizeW / adjustResizeFrame.size.width;
    CGFloat diffXSpace = adjustResizeFrame.origin.x * convertScale;
    CGFloat diffYSpace = adjustResizeFrame.origin.y * convertScale;
    CGFloat convertW = self.imageresizeW + 2 * diffXSpace;
    CGFloat convertH = self.imageresizeH + 2 * diffYSpace;
    CGFloat convertX = self.imageresizeX - diffXSpace;
    CGFloat convertY = self.imageresizeY - diffYSpace;
    // 边沿检测，到顶就往外取值，防止有空隙
    CGRect convertImageresizerFrame = [self convertRect:self.imageresizerFrame toView:self.scrollView];
    BOOL isTheTop = fabs(convertImageresizerFrame.origin.y - self.imageView.frame.origin.y) < 1.0;
    BOOL isTheLead = fabs(convertImageresizerFrame.origin.x - self.imageView.frame.origin.x) < 1.0;
    BOOL isTheBottom = fabs(CGRectGetMaxY(convertImageresizerFrame) - CGRectGetMaxY(self.imageView.frame)) < 1.0;
    BOOL isTheTrail = fabs(CGRectGetMaxX(convertImageresizerFrame) - CGRectGetMaxX(self.imageView.frame)) < 1.0;
    if (isTheTop) convertY -= 1.0;
    if (isTheLead) convertX -= 1.0;
    if (isTheBottom) convertH += 1.0;
    if (isTheTrail) convertW += 1.0;
    CGRect zoomFrame = [self convertRect:CGRectMake(convertX, convertY, convertW, convertH) toView:self.imageView];
    
    __weak __typeof(self) wSelf = self;
    
    void (^zoomBlock)(void) = ^{
        __strong __typeof(wSelf) sSelf = wSelf;
        if (!sSelf) return;
        [sSelf.scrollView setContentInset:contentInset];
        [sSelf.scrollView setContentOffset:contentOffset animated:NO];
        [sSelf.scrollView zoomToRect:zoomFrame animated:NO];
    };
    
    void (^completeBlock)(void) = ^{
        __strong __typeof(wSelf) sSelf = wSelf;
        if (!sSelf) return;
        sSelf.superview.userInteractionEnabled = YES;
        [sSelf checkIsCanRecovery];
        sSelf.isPrepareToScale = NO;
    };
    
    self.superview.userInteractionEnabled = NO;
    [self hideOrShowBlurEffect:NO animateDuration:duration];
    [self hideOrShowFrameLine:NO animateDuration:duration];
    [self updateImageresizerFrame:adjustResizeFrame animateDuration:duration];
    if (duration > 0) {
        [UIView animateWithDuration:duration delay:0 options:_animationOption animations:^{
            zoomBlock();
        } completion:^(BOOL finished) {
            completeBlock();
        }];
    } else {
        zoomBlock();
        completeBlock();
    }
}

- (CGRect)adjustResizeFrame {
    CGFloat resizeWHScale = _isArbitrarily ? (self.imageresizeW / self.imageresizeH) : _resizeWHScale;
    CGFloat adjustResizeW = 0;
    CGFloat adjustResizeH = 0;
    if (resizeWHScale >= 1) {
        adjustResizeW = self.maxResizeW;
        adjustResizeH = adjustResizeW / resizeWHScale;
        if (adjustResizeH > self.maxResizeH) {
            adjustResizeH = self.maxResizeH;
            adjustResizeW = self.maxResizeH * resizeWHScale;
        }
    } else {
        adjustResizeH = self.maxResizeH;
        adjustResizeW = adjustResizeH * resizeWHScale;
        if (adjustResizeW > self.maxResizeW) {
            adjustResizeW = self.maxResizeW;
            adjustResizeH = adjustResizeW / resizeWHScale;
        }
    }
    CGFloat adjustResizeX = self.maxResizeX + (self.maxResizeW - adjustResizeW) * 0.5;
    CGFloat adjustResizeY = self.maxResizeY + (self.maxResizeH - adjustResizeH) * 0.5;
    return CGRectMake(adjustResizeX, adjustResizeY, adjustResizeW, adjustResizeH);
}

- (UIEdgeInsets)scrollViewContentInsetWithAdjustResizeFrame:(CGRect)adjustResizeFrame {
    // scrollView宽高跟self一样，上下左右不需要额外添加Space
    CGFloat top = adjustResizeFrame.origin.y; // + veSpace?
    CGFloat bottom = self.bounds.size.height - CGRectGetMaxY(adjustResizeFrame); // + veSpace?
    CGFloat left = adjustResizeFrame.origin.x; // + hoSpace?
    CGFloat right = self.bounds.size.width - CGRectGetMaxX(adjustResizeFrame); // + hoSpace?
    return UIEdgeInsetsMake(top, left, bottom, right);
}

- (CGFloat)scrollViewMinZoomScaleWithResizeSize:(CGSize)size {
    CGFloat length;
    CGFloat baseLength;
    CGFloat width;
    CGFloat baseWidth;
    if (size.width >= size.height) {
        length = size.width;
        baseLength = _baseImageW;
        width = size.height;
        baseWidth = _baseImageH;
    } else {
        length = size.height;
        baseLength = _baseImageH;
        width = size.width;
        baseWidth = _baseImageW;
    }
    CGFloat minZoomScale = length / baseLength;
    CGFloat scaleWidth = baseWidth * minZoomScale;
    if (scaleWidth < width) {
        minZoomScale *= (width / scaleWidth);
    }
    return minZoomScale;
}

- (void)checkIsCanRecovery {
    BOOL isVerticalityMirror = self.isVerticalityMirror ? self.isVerticalityMirror() : NO;
    BOOL isHorizontalMirror = self.isHorizontalMirror ? self.isHorizontalMirror() : NO;
    if (isVerticalityMirror || isHorizontalMirror) {
        self.isCanRecovery = YES;
        return;
    }
    
    CGPoint convertCenter = [self convertPoint:CGPointMake(CGRectGetMidX(self.bounds), CGRectGetMidY(self.bounds)) toView:self.imageView];
    CGPoint imageViewCenter = CGPointMake(CGRectGetMidX(self.imageView.bounds), CGRectGetMidY(self.imageView.bounds));
    BOOL isSameCenter = labs((NSInteger)convertCenter.x - (NSInteger)imageViewCenter.x) <= 1 && labs((NSInteger)convertCenter.y - (NSInteger)imageViewCenter.y) <= 1;
    BOOL isOriginFrame = self.rotationDirection == JPImageresizerVerticalUpDirection && [self imageresizerFrameIsEqualImageViewFrame];
    self.isCanRecovery = !isOriginFrame || !isSameCenter;
}

#pragma mark - puild method

- (void)updateFrameType:(JPImageresizerFrameType)frameType {
    if (self.frameType == frameType) return;
    self.frameType = frameType;
    self.strokeColor = _strokeColor;
    [self updateImageresizerFrame:_imageresizerFrame animateDuration:-1.0];
}

- (void)updateImageresizerFrameWithVerBaseMargin:(CGFloat)verBaseMargin horBaseMargin:(CGFloat)horBaseMargin {
    _verBaseMargin = verBaseMargin;
    _horBaseMargin = horBaseMargin;
    self.layer.transform = CATransform3DIdentity;
    [self updateImageOriginFrameWithDirection:JPImageresizerVerticalUpDirection];
}

- (void)startImageresizer {
    self.isPrepareToScale = YES;
    [self removeTimer];
    [self hideOrShowBlurEffect:YES animateDuration:0.2];
    [self hideOrShowFrameLine:YES animateDuration:0.2];
}

- (void)endedImageresizer {
    UIEdgeInsets contentInset = [self scrollViewContentInsetWithAdjustResizeFrame:self.imageresizerFrame];
    self.scrollView.contentInset = contentInset;
    [self addTimer];
}

- (void)rotationWithDirection:(JPImageresizerRotationDirection)direction rotationDuration:(NSTimeInterval)rotationDuration {
    [self removeTimer];
    [self updateRotationDirection:direction];
    [self updateImageresizerFrameWithAnimateDuration:rotationDuration];
}

- (void)willMirror:(BOOL)animated {
    self.window.userInteractionEnabled = NO;
    if (animated) [self hideOrShowBlurEffect:YES animateDuration:-1.0];
}

- (void)verticalityMirrorWithDiffX:(CGFloat)diffX {
    CGFloat w = [self isHorizontalDirection:_rotationDirection] ? self.bounds.size.height : self.bounds.size.width;
    CGFloat x = (_contentSize.width - w) * 0.5 + diffX;
    CGRect frame = self.frame;
    frame.origin.x = x;
    self.scrollView.frame = frame;
    self.frame = frame;
}

- (void)horizontalMirrorWithDiffY:(CGFloat)diffY {
    CGFloat h = [self isHorizontalDirection:_rotationDirection] ? self.bounds.size.width : self.bounds.size.height;
    CGFloat y = (_contentSize.height - h) * 0.5 + diffY;
    CGRect frame = self.frame;
    frame.origin.y = y;
    self.scrollView.frame = frame;
    self.frame = frame;
}

- (void)mirrorDone {
    [self hideOrShowBlurEffect:NO animateDuration:_defaultDuration];
    [self checkIsCanRecovery];
    self.window.userInteractionEnabled = YES;
}

- (void)willRecovery {
    self.window.userInteractionEnabled = NO;
    [self removeTimer];
}

- (void)recoveryWithDuration:(NSTimeInterval)duration {
    [self updateRotationDirection:JPImageresizerVerticalUpDirection];
    
    CGRect adjustResizeFrame = _isArbitrarily ? [self baseImageresizerFrame] : [self adjustResizeFrame];
    
    UIEdgeInsets contentInset = [self scrollViewContentInsetWithAdjustResizeFrame:adjustResizeFrame];
    
    CGFloat minZoomScale = [self scrollViewMinZoomScaleWithResizeSize:adjustResizeFrame.size];
    
    CGFloat contentOffsetX = -contentInset.left + (_baseImageW * minZoomScale - adjustResizeFrame.size.width) * 0.5;
    CGFloat contentOffsetY = -contentInset.top + (_baseImageH * minZoomScale - adjustResizeFrame.size.height) * 0.5;
    
    [self updateImageresizerFrame:adjustResizeFrame animateDuration:duration];
    
    self.scrollView.minimumZoomScale = minZoomScale;
    self.scrollView.zoomScale = minZoomScale;
    self.scrollView.contentInset = contentInset;
    self.scrollView.contentOffset = CGPointMake(contentOffsetX, contentOffsetY);
}

- (void)recoveryDone {
    [self updateImageresizerFrameWithAnimateDuration:-1.0];
    self.window.userInteractionEnabled = YES;
}

- (void)imageresizerWithComplete:(void (^)(UIImage *))complete isOriginImageSize:(BOOL)isOriginImageSize referenceWidth:(CGFloat)referenceWidth {
    if (!complete) return;
    
    /**
     * UIImageOrientationUp,            // default orientation
     * UIImageOrientationDown,          // 180 deg rotation
     * UIImageOrientationLeft,          // 90 deg CCW
     * UIImageOrientationRight,         // 90 deg CW
     */
    UIImageOrientation orientation;
    switch (self.rotationDirection) {
        case JPImageresizerHorizontalLeftDirection:
            orientation = UIImageOrientationLeft;
            break;
        case JPImageresizerVerticalDownDirection:
            orientation = UIImageOrientationDown;
            break;
        case JPImageresizerHorizontalRightDirection:
            orientation = UIImageOrientationRight;
            break;
        default:
            orientation = UIImageOrientationUp;
            break;
    }
    
    BOOL isVerticalityMirror = self.isVerticalityMirror ? self.isVerticalityMirror() : NO;
    BOOL isHorizontalMirror = self.isHorizontalMirror ? self.isHorizontalMirror() : NO;
    if ([self isHorizontalDirection:_rotationDirection]) {
        BOOL temp = isVerticalityMirror;
        isVerticalityMirror = isHorizontalMirror;
        isHorizontalMirror = temp;
    }
    
    __block UIImage *image = self.imageView.image;
    
    CGFloat deviceScale = [UIScreen mainScreen].scale;
    
    CGRect imageViewBounds = self.imageView.bounds;
    CGFloat imageViewWidth = imageViewBounds.size.width;
    
    CGRect cropFrame = (self.isCanRecovery || self.resizeWHScale > 0) ? [self convertRect:self.imageresizerFrame toView:self.imageView] : imageViewBounds;
    
    __weak __typeof(self) wSelf = self;
    dispatch_async(dispatch_get_global_queue(0, 0), ^{
        __strong __typeof(wSelf) sSelf = wSelf;
        if (!sSelf) return;
        
        // 修正图片方向
        image = [image jp_fixOrientation];
        
        // 镜像处理
        if (isVerticalityMirror) image = [image jp_verticalityMirror];
        if (isHorizontalMirror) image = [image jp_horizontalMirror];
        
        // 获取裁剪区域
        CGFloat imageScale = image.scale;
        CGFloat imageWidth = image.size.width * imageScale;
        CGFloat imageHeight = image.size.height * imageScale;
        // 宽高比不变，所以宽度高度的比例是一样
        CGFloat scale = imageWidth / imageViewWidth;
        CGFloat cropX = cropFrame.origin.x * scale;
        CGFloat cropY = cropFrame.origin.y * scale;
        CGFloat cropW = cropFrame.size.width * scale;
        CGFloat cropH = cropFrame.size.height * scale;
        if (cropX < 0) {
            cropW += -cropX;
            cropX = 0;
        }
        if (cropY < 0) {
            cropH += -cropY;
            cropY = 0;
        }
        CGFloat cropMaxX = cropX + cropW;
        if (cropMaxX > imageWidth) {
            cropW -= (cropMaxX - imageWidth);
            cropMaxX = cropX + cropW;
        }
        CGFloat cropMaxY = cropY + cropH;
        if (cropMaxY > imageHeight) {
            cropH -= (cropMaxY - imageHeight);
            cropMaxY = cropY + cropH;
        }
        if (isVerticalityMirror) cropX = imageWidth - cropMaxX;
        if (isHorizontalMirror) cropY = imageHeight - cropMaxY;
        CGRect cropRect = CGRectMake(cropX, cropY, cropW, cropH);
        
        // 裁剪并旋转图片
        CGImageRef imgRef = CGImageCreateWithImageInRect(image.CGImage, cropRect);
        image = [[UIImage imageWithCGImage:imgRef] jp_rotate:orientation];
        CGImageRelease(imgRef);
        
        // 若原图尺寸，则原图输出
        if (isOriginImageSize) {
            dispatch_async(dispatch_get_main_queue(), ^{
                complete(image);
            });
            return;
        }
        
        // 按照参照宽度获取压缩尺寸
        CGFloat referenceW = referenceWidth;
        if (referenceW > 0) {
            CGFloat maxWidth = MAX(imageWidth, imageViewWidth);
            CGFloat minWidth = MIN(imageWidth, imageViewWidth);
            if (referenceW > maxWidth) referenceW = maxWidth;
            if (referenceW < minWidth) referenceW = minWidth;
        } else {
            referenceW = imageViewWidth;
        }
        CGFloat cropScale = imageWidth / referenceW;
        // 有小数的情况下，边界会多出白线，需要把小数点去掉
        CGSize cropSize = CGSizeMake(floor(image.size.width / cropScale), floor(image.size.height / cropScale));
        if (cropSize.width < 1) cropSize.width = 1;
        if (cropSize.height < 1) cropSize.height = 1;
        
        // 压缩图片
        /**
         * 参考：http://www.jb51.net/article/81318.htm
         * 这里要注意一点CGContextDrawImage这个函数的坐标系和UIKIt的坐标系上下颠倒，需对坐标系处理如下：
            - 1.CGContextTranslateCTM(context, 0, cropSize.height);
            - 2.CGContextScaleCTM(context, 1, -1);
         */
        UIGraphicsBeginImageContextWithOptions(cropSize, 0, deviceScale);
        CGContextRef context = UIGraphicsGetCurrentContext();
        CGContextTranslateCTM(context, 0, cropSize.height);
        CGContextScaleCTM(context, 1, -1);
        CGContextDrawImage(context, CGRectMake(0, 0, cropSize.width, cropSize.height), image.CGImage);
        image = UIGraphicsGetImageFromCurrentImageContext();
        UIGraphicsEndImageContext();
        
        // 输出压缩图片
        dispatch_async(dispatch_get_main_queue(), ^{
            complete(image);
        });
    });
}

#pragma mark - UIPanGestureRecognizer

- (void)panHandle:(UIPanGestureRecognizer *)panGR {
    
    CGPoint translation = [panGR translationInView:self];
    
    [panGR setTranslation:CGPointZero inView:self];
    
    if (panGR.state == UIGestureRecognizerStateBegan) [self panBeganHandleWithLocation:[panGR locationInView:self]];
    
    if (panGR.state == UIGestureRecognizerStateChanged) [self panChangedHandleWithTranslation:translation];
    
    if (panGR.state == UIGestureRecognizerStateEnded ||
        panGR.state == UIGestureRecognizerStateCancelled ||
        panGR.state == UIGestureRecognizerStateFailed) [self endedImageresizer];
}

- (void)panBeganHandleWithLocation:(CGPoint)location {
    
    [self startImageresizer];
    
    CGFloat x = self.imageresizeX;
    CGFloat y = self.imageresizeY;
    CGFloat width = self.imageresizeW;
    CGFloat height = self.imageresizeH;
    CGFloat midX = CGRectGetMidX(self.imageresizerFrame);
    CGFloat midY = CGRectGetMidY(self.imageresizerFrame);
    CGFloat maxX = CGRectGetMaxX(self.imageresizerFrame);
    CGFloat maxY = CGRectGetMaxY(self.imageresizerFrame);
    
    CGFloat scopeWH = _scopeWH;
    CGFloat halfScopeWH = scopeWH * 0.5;
    
    CGRect leftTopRect = CGRectMake(x - halfScopeWH, y - halfScopeWH, scopeWH, scopeWH);
    CGRect leftBotRect = CGRectMake(x - halfScopeWH, maxY - halfScopeWH, scopeWH, scopeWH);
    CGRect rightTopRect = CGRectMake(maxX - halfScopeWH, y - halfScopeWH, scopeWH, scopeWH);
    CGRect rightBotRect = CGRectMake(maxX - halfScopeWH, maxY - halfScopeWH, scopeWH, scopeWH);
    
    if (CGRectContainsPoint(leftTopRect, location)) {
        self.currHorn = JPLeftTop;
        self.diagonal = CGPointMake(x + width, y + height);
    } else if (CGRectContainsPoint(leftBotRect, location)) {
        self.currHorn = JPLeftBottom;
        self.diagonal = CGPointMake(x + width, y);
    } else if (CGRectContainsPoint(rightTopRect, location)) {
        self.currHorn = JPRightTop;
        self.diagonal = CGPointMake(x, y + height);
    } else if (CGRectContainsPoint(rightBotRect, location)) {
        self.currHorn = JPRightBottom;
        self.diagonal = CGPointMake(x, y);
    } else if (_isArbitrarily) {
        CGRect leftMidRect = CGRectNull;
        CGRect rightMidRect = CGRectNull;
        CGRect topMidRect = CGRectNull;
        CGRect botMidRect = CGRectNull;
        if (_edgeLineIsEnabled) {
            leftMidRect = CGRectMake(x - halfScopeWH, y + halfScopeWH, scopeWH, height - scopeWH);
            rightMidRect = CGRectMake(maxX - halfScopeWH, y + halfScopeWH, scopeWH, height - scopeWH);
            topMidRect = CGRectMake(x + halfScopeWH, y - halfScopeWH, width - scopeWH, scopeWH);
            botMidRect = CGRectMake(x + halfScopeWH, maxY - halfScopeWH, width - scopeWH, scopeWH);
        } else if (self.isShowMidDot) {
            leftMidRect = CGRectMake(x - halfScopeWH, midY - halfScopeWH, scopeWH, scopeWH);
            rightMidRect = CGRectMake(maxX - halfScopeWH, midY - halfScopeWH, scopeWH, scopeWH);
            topMidRect = CGRectMake(midX - halfScopeWH, y - halfScopeWH, scopeWH, scopeWH);
            botMidRect = CGRectMake(midX - halfScopeWH, maxY - halfScopeWH, scopeWH, scopeWH);
        }
        if (CGRectContainsPoint(leftMidRect, location)) {
            self.currHorn = JPLeftMid;
            self.diagonal = CGPointMake(maxX, midY);
        } else if (CGRectContainsPoint(rightMidRect, location)) {
            self.currHorn = JPRightMid;
            self.diagonal = CGPointMake(x, midY);
        } else if (CGRectContainsPoint(topMidRect, location)) {
            self.currHorn = JPTopMid;
            self.diagonal = CGPointMake(midX, maxY);
        } else if (CGRectContainsPoint(botMidRect, location)) {
            self.currHorn = JPBottomMid;
            self.diagonal = CGPointMake(midX, y);
        } else {
            self.currHorn = JPCenter;
        }
    } else {
        self.currHorn = JPCenter;
    }
    
    _startResizeW = width;
    _startResizeH = height;
}

- (void)panChangedHandleWithTranslation:(CGPoint)translation {
    
    CGFloat x = self.imageresizeX;
    CGFloat y = self.imageresizeY;
    CGFloat width = self.imageresizeW;
    CGFloat height = self.imageresizeH;
    
    switch (self.currHorn) {
            
        case JPLeftTop: {
            if (_isArbitrarily) {
                x += translation.x;
                y += translation.y;
                
                if (x < self.maxResizeX) {
                    x = self.maxResizeX;
                }
                
                if (y < self.maxResizeY) {
                    y = self.maxResizeY;
                }
                
                width = self.diagonal.x - x;
                height = self.diagonal.y - y;
                
                if (width < _minImageWH) {
                    width = _minImageWH;
                    x = self.diagonal.x - width;
                }
                
                if (height < _minImageWH) {
                    height = _minImageWH;
                    y = self.diagonal.y - height;
                }
            } else {
                x += translation.x;
                width = self.diagonal.x - x;
                
                if (translation.x != 0) {
                    CGFloat diff = translation.x / _resizeWHScale;
                    y += diff;
                    height = self.diagonal.y - y;
                }
                
                if (x < self.maxResizeX) {
                    x = self.maxResizeX;
                    width = self.diagonal.x - x;
                    height = width / _resizeWHScale;
                    y = self.diagonal.y - height;
                }
                
                if (y < self.maxResizeY) {
                    y = self.maxResizeY;
                    height = self.diagonal.y - y;
                    width = height * _resizeWHScale;
                    x = self.diagonal.x - width;
                }
                
                if (width < _minImageWH && height < _minImageWH) {
                    if (_resizeWHScale >= 1) {
                        width = _minImageWH;
                        height = width / _resizeWHScale;
                    } else {
                        height = _minImageWH;
                        width = height * _resizeWHScale;
                    }
                    x = self.diagonal.x - width;
                    y = self.diagonal.y - height;
                }
            }
            
            break;
        }
            
        case JPLeftBottom: {
            if (_isArbitrarily) {
                x += translation.x;
                height = height + translation.y;
                
                if (x < self.maxResizeX) {
                    x = self.maxResizeX;
                }
                
                CGFloat maxResizeMaxY = CGRectGetMaxY(self.maxResizeFrame);
                if ((y + height) > maxResizeMaxY) {
                    height = maxResizeMaxY - self.diagonal.y;
                }
                
                width = self.diagonal.x - x;
                
                if (width < _minImageWH) {
                    width = _minImageWH;
                    x = self.diagonal.x - width;
                }
                
                if (height < _minImageWH) {
                    height = _minImageWH;
                }
            } else {
                x += translation.x;
                width = self.diagonal.x - x;
                
                if (translation.x != 0) {
                    height = width / _resizeWHScale;
                }
                
                if (x < self.maxResizeX) {
                    x = self.maxResizeX;
                    width = self.diagonal.x - x;
                    height = width / _resizeWHScale;
                }
                
                CGFloat maxResizeMaxY = CGRectGetMaxY(self.maxResizeFrame);
                if ((y + height) > maxResizeMaxY) {
                    height = maxResizeMaxY - self.diagonal.y;
                    width = height * _resizeWHScale;
                    x = self.diagonal.x - width;
                }
                
                if (width < _minImageWH && height < _minImageWH) {
                    if (_resizeWHScale >= 1) {
                        width = _minImageWH;
                        height = width / _resizeWHScale;
                    } else {
                        height = _minImageWH;
                        width = height * _resizeWHScale;
                    }
                    x = self.diagonal.x - width;
                    y = self.diagonal.y;
                }
            }
            
            break;
        }
            
        case JPRightTop: {
            if (_isArbitrarily) {
                y += translation.y;
                width = width + translation.x;
                
                if (y < self.maxResizeY) {
                    y = self.maxResizeY;
                }
                
                CGFloat maxResizeMaxX = CGRectGetMaxX(self.maxResizeFrame);
                if ((x + width) > maxResizeMaxX) {
                    width = maxResizeMaxX - self.diagonal.x;
                }
                
                height = self.diagonal.y - y;
                
                if (width < _minImageWH) {
                    width = _minImageWH;
                }
                
                if (height < _minImageWH) {
                    height = _minImageWH;
                    y = self.diagonal.y - height;
                }
            } else {
                width = width + translation.x;
                
                if (translation.x != 0) {
                    CGFloat diff = translation.x / _resizeWHScale;
                    y -= diff;
                    height = self.diagonal.y - y;
                }
                
                if (y < self.maxResizeY) {
                    y = self.maxResizeY;
                    height = self.diagonal.y - y;
                    width = height * _resizeWHScale;
                }
                
                CGFloat maxResizeMaxX = CGRectGetMaxX(self.maxResizeFrame);
                if ((x + width) > maxResizeMaxX) {
                    width = maxResizeMaxX - self.diagonal.x;
                    height = width / _resizeWHScale;
                    y = self.diagonal.y - height;
                }
                
                if (width < _minImageWH && height < _minImageWH) {
                    if (_resizeWHScale >= 1) {
                        width = _minImageWH;
                        height = width / _resizeWHScale;
                    } else {
                        height = _minImageWH;
                        width = height * _resizeWHScale;
                    }
                    x = self.diagonal.x;
                    y = self.diagonal.y - height;
                }
            }
            
            break;
        }
            
        case JPRightBottom: {
            if (_isArbitrarily) {
                width = width + translation.x;
                height = height + translation.y;
                
                CGFloat maxResizeMaxX = CGRectGetMaxX(self.maxResizeFrame);
                if ((x + width) > maxResizeMaxX) {
                    width = maxResizeMaxX - self.diagonal.x;
                }
                
                CGFloat maxResizeMaxY = CGRectGetMaxY(self.maxResizeFrame);
                if ((y + height) > maxResizeMaxY) {
                    height = maxResizeMaxY - self.diagonal.y;
                }
                
                if (width < _minImageWH) {
                    width = _minImageWH;
                }
                
                if (height < _minImageWH) {
                    height = _minImageWH;
                }
            } else {
                width = width + translation.x;
                
                if (translation.x != 0) {
                    height = width / _resizeWHScale;
                }
                
                CGFloat maxResizeMaxX = CGRectGetMaxX(self.maxResizeFrame);
                if ((x + width) > maxResizeMaxX) {
                    width = maxResizeMaxX - self.diagonal.x;
                    height = width / _resizeWHScale;
                }
                
                CGFloat maxResizeMaxY = CGRectGetMaxY(self.maxResizeFrame);
                if ((y + height) > maxResizeMaxY) {
                    height = maxResizeMaxY - self.diagonal.y;
                    width = height * _resizeWHScale;
                }
                
                if (width < _minImageWH && height < _minImageWH) {
                    if (_resizeWHScale >= 1) {
                        width = _minImageWH;
                        height = width / _resizeWHScale;
                    } else {
                        height = _minImageWH;
                        width = height * _resizeWHScale;
                    }
                    x = self.diagonal.x;
                    y = self.diagonal.y;
                }
            }
            
            break;
        }
            
        case JPLeftMid: {
            x += translation.x;
            
            if (x < self.maxResizeX) {
                x = self.maxResizeX;
            }
            
            width = self.diagonal.x - x;
            
            if (width < _minImageWH) {
                width = _minImageWH;
                x = self.diagonal.x - width;
            }
            break;
        }
            
        case JPRightMid: {
            width = width + translation.x;
            
            CGFloat maxResizeMaxX = CGRectGetMaxX(self.maxResizeFrame);
            if ((x + width) > maxResizeMaxX) {
                width = maxResizeMaxX - self.diagonal.x;
            }
            
            if (width < _minImageWH) {
                width = _minImageWH;
            }
            break;
        }
            
        case JPTopMid: {
            y += translation.y;
            
            if (y < self.maxResizeY) {
                y = self.maxResizeY;
            }
            
            height = self.diagonal.y - y;
            
            if (height < _minImageWH) {
                height = _minImageWH;
                y = self.diagonal.y - height;
            }
            break;
        }
            
        case JPBottomMid: {
            height = height + translation.y;
            
            CGFloat maxResizeMaxY = CGRectGetMaxY(self.maxResizeFrame);
            if ((y + height) > maxResizeMaxY) {
                height = maxResizeMaxY - self.diagonal.y;
            }
            
            if (height < _minImageWH) {
                height = _minImageWH;
            }
            break;
        }
            
        default:
        {
            break;
        }
            
    }
    self.imageresizerFrame = CGRectMake(x, y, width, height);
    
    CGRect zoomFrame = [self convertRect:self.imageresizerFrame toView:self.imageView];
    CGPoint contentOffset = self.scrollView.contentOffset;
    if (zoomFrame.origin.x < 0) {
        contentOffset.x -= zoomFrame.origin.x;
    } else if (CGRectGetMaxX(zoomFrame) > _baseImageW) {
        contentOffset.x -= CGRectGetMaxX(zoomFrame) - _baseImageW;
    }
    if (zoomFrame.origin.y < 0) {
        contentOffset.y -= zoomFrame.origin.y;
    } else if (CGRectGetMaxY(zoomFrame) > _baseImageH) {
        contentOffset.y -= CGRectGetMaxY(zoomFrame) - _baseImageH;
    }
    [self.scrollView setContentOffset:contentOffset animated:NO];
    
    CGFloat wZoomScale = 0;
    CGFloat hZoomScale = 0;
    if (width > _startResizeW) {
        wZoomScale = width / _baseImageW;
    }
    if (height > _startResizeH) {
        hZoomScale = height / _baseImageH;
    }
    CGFloat zoomScale = MAX(wZoomScale, hZoomScale);
    if (zoomScale > self.scrollView.zoomScale) {
        [self.scrollView setZoomScale:zoomScale animated:NO];
    }
}

#pragma mark - super method

- (BOOL)pointInside:(CGPoint)point withEvent:(UIEvent *)event {
    if (!self.panGR.enabled) return NO;
    
    CGFloat scopeWH = _scopeWH;
    CGFloat halfScopeWH = scopeWH * 0.5;
    
    if (self.edgeLineIsEnabled) {
        CGRect maxFrame = CGRectInset(self.imageresizerFrame, -halfScopeWH, -halfScopeWH);
        CGRect minFrame = CGRectInset(self.imageresizerFrame, halfScopeWH, halfScopeWH);
        if (CGRectContainsPoint(maxFrame, point) && !CGRectContainsPoint(minFrame, point)) {
            return YES;
        }
    } else {
        CGFloat x = self.imageresizeX;
        CGFloat y = self.imageresizeY;
        CGFloat midX = CGRectGetMidX(self.imageresizerFrame);
        CGFloat midY = CGRectGetMidY(self.imageresizerFrame);
        CGFloat maxX = CGRectGetMaxX(self.imageresizerFrame);
        CGFloat maxY = CGRectGetMaxY(self.imageresizerFrame);
        
        CGRect leftTopRect = CGRectMake(x - halfScopeWH, y - halfScopeWH, scopeWH, scopeWH);
        CGRect leftBotRect = CGRectMake(x - halfScopeWH, maxY - halfScopeWH, scopeWH, scopeWH);
        CGRect rightTopRect = CGRectMake(maxX - halfScopeWH, y - halfScopeWH, scopeWH, scopeWH);
        CGRect rightBotRect = CGRectMake(maxX - halfScopeWH, maxY - halfScopeWH, scopeWH, scopeWH);
        
        if (CGRectContainsPoint(leftTopRect, point) ||
            CGRectContainsPoint(leftBotRect, point) ||
            CGRectContainsPoint(rightTopRect, point) ||
            CGRectContainsPoint(rightBotRect, point)) {
            return YES;
        } else if (self.isShowMidDot) {
            CGRect leftMidRect = CGRectMake(x - halfScopeWH, midY - halfScopeWH, scopeWH, scopeWH);
            CGRect rightMidRect = CGRectMake(maxX - halfScopeWH, midY - halfScopeWH, scopeWH, scopeWH);
            CGRect topMidRect = CGRectMake(midX - halfScopeWH, y - halfScopeWH, scopeWH, scopeWH);
            CGRect botMidRect = CGRectMake(midX - halfScopeWH, maxY - halfScopeWH, scopeWH, scopeWH);
            if (CGRectContainsPoint(leftMidRect, point) ||
                CGRectContainsPoint(rightMidRect, point) ||
                CGRectContainsPoint(topMidRect, point) ||
                CGRectContainsPoint(botMidRect, point)) {
                return YES;
            }
        }
    }
    return NO;
}

@end
