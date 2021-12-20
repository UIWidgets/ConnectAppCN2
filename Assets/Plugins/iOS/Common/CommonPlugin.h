//
//  CommonPlugin.h
//  Unity-iPhone
//
//  Created by wangshuang on 2021/11/16.
//

#import "UnityAppController.h"

NS_ASSUME_NONNULL_BEGIN

@interface CommonPlugin : NSObject

+ (nonnull instancetype) instance;

@property (nonatomic,copy) NSString *pushJson;

@property (nonatomic,copy) NSString *schemeUrl;

@property (nonatomic,copy) NSString *universalLink;


@end

NS_ASSUME_NONNULL_END
