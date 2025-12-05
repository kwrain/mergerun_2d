#import <AppTrackingTransparency/AppTrackingTransparency.h>
#import <AdSupport/AdSupport.h>
#import "UnityInterface.h"

extern "C" {
    
    // ATT 권한 상태 확인 (0: NotDetermined, 1: Restricted, 2: Denied, 3: Authorized)
    int GetTrackingAuthorizationStatus() {
        if (@available(iOS 14, *)) {
            return (int)[ATTrackingManager trackingAuthorizationStatus];
        }
        return 3; // iOS 14 미만은 항상 허용된 것으로 간주
    }
    
    // ATT 권한 요청
    void RequestTrackingAuthorization() {
        if (@available(iOS 14, *)) {
            [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
                // Unity에 결과 전달
                UnitySendMessage("AppTrackingTransparencyBridge", "OnTrackingAuthorizationResult", [[NSString stringWithFormat:@"%d", (int)status] UTF8String]);
            }];
        } else {
            // iOS 14 미만은 항상 허용된 것으로 간주
            UnitySendMessage("AppTrackingTransparencyBridge", "OnTrackingAuthorizationResult", "3");
        }
    }
    
    // IDFA 가져오기 (권한이 허용된 경우에만)
    const char* GetIDFANative() {
        if (@available(iOS 14, *)) {
            if ([ATTrackingManager trackingAuthorizationStatus] == ATTrackingManagerAuthorizationStatusAuthorized) {
                NSUUID *idfa = [[ASIdentifierManager sharedManager] advertisingIdentifier];
                NSString *idfaString = [idfa UUIDString];
                return strdup([idfaString UTF8String]);
            }
        } else {
            // iOS 14 미만
            NSUUID *idfa = [[ASIdentifierManager sharedManager] advertisingIdentifier];
            NSString *idfaString = [idfa UUIDString];
            return strdup([idfaString UTF8String]);
        }
        return strdup("");
    }
    
    // 메모리 해제 (strdup로 할당된 메모리)
    void FreeIDFANative(char* ptr) {
        if (ptr != NULL) {
            free(ptr);
        }
    }
}

