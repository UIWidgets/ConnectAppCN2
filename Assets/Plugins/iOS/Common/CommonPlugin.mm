//
//  CommonPlugin.m
//  Unity-iPhone
//
//  Created by wangshuang on 2021/11/16.
//

#import "CommonPlugin.h"
#include "uiwidgets_message_manager.h"

#if defined(__cplusplus)
extern "C" {
#endif
    extern NSString*  CreateNSString (const char* string);
    extern id         APNativeJSONObject(NSData *data);
#if defined(__cplusplus)
}
#endif

@implementation CommonPlugin

+ (nonnull instancetype) instance {
    static id _shared;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        _shared = [[self alloc] init];
    });
    return _shared;
}

@end

#if defined(__cplusplus)
extern "C" {
#endif
    NSString *CreateNSString (const char *string) {
        return [NSString stringWithUTF8String:(string ? string : "")];
    }
    
    id APNativeJSONObject(NSData *data) {
        if (!data) {
            return nil;
        }
        
        NSError *error = nil;
        id retId = [NSJSONSerialization JSONObjectWithData:data options:0 error:&error];
        
        if (error) {
            NSLog(@"%s trans data to obj with error: %@", __func__, error);
            return nil;
        }
        
        return retId;
    }
    
    static char *MakeHeapString(const char *string) {
        if (!string){
            return NULL;
        }
        char *mem = static_cast<char*>(malloc(strlen(string) + 1));
        if (mem) {
            strcpy(mem, string);
        }
        return mem;
    }
    
    void listenCompleted(){
        BOOL needPush = false;
        if ([CommonPlugin instance].pushJson.length > 0||[CommonPlugin instance].schemeUrl.length > 0||[CommonPlugin instance].universalLink.length > 0) {
            needPush = true;
        }
        NSError *error = nil;
        NSData *data = [NSJSONSerialization dataWithJSONObject:@{@"push":@(needPush)} options:0 error:&error];
        NSString *jsonStr = [[NSString alloc]initWithData:data encoding:NSUTF8StringEncoding];
        UIWidgetsMethodMessage(@"common", @"CompletedCallback", @[jsonStr]);
        
        if ([CommonPlugin instance].pushJson.length > 0) {
            UIWidgetsMethodMessage(@"common", @"OnOpenNotification", @[[CommonPlugin instance].pushJson]);
        }
        if ([CommonPlugin instance].schemeUrl.length > 0) {
            UIWidgetsMethodMessage(@"common", @"OnOpenUrl", @[[CommonPlugin instance].schemeUrl]);
        }
        if ([CommonPlugin instance].universalLink.length > 0) {
            UIWidgetsMethodMessage(@"common", @"OnOpenUniversalLinks", @[[CommonPlugin instance].universalLink]);
        }
    }
    
#if defined(__cplusplus)
}
#endif

