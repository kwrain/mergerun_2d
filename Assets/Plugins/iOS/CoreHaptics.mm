#import <CoreHaptics/CoreHaptics.h>
#import <AudioToolbox/AudioToolbox.h>
#import "UnityInterface.h"

// CoreHaptics 엔진 인스턴스 (싱글톤)
static CHHapticEngine* g_hapticEngine = nil;
static bool g_engineStarted = false;

extern "C" {
    
    // CoreHaptics 엔진 초기화
    void InitializeCoreHaptics() {
        if (@available(iOS 13, *)) {
            if (g_hapticEngine == nil) {
                NSError* error = nil;
                g_hapticEngine = [[CHHapticEngine alloc] initAndReturnError:&error];
                
                if (error != nil) {
                    NSLog(@"[CoreHaptics] 엔진 초기화 실패: %@", error.localizedDescription);
                    g_hapticEngine = nil;
                    return;
                }
                
                // 엔진이 중지될 때 재시작하도록 설정
                g_hapticEngine.stoppedHandler = ^(CHHapticEngineStoppedReason reason) {
                    NSLog(@"[CoreHaptics] 엔진 중지됨: %ld", (long)reason);
                    g_engineStarted = false;
                };
                
                g_hapticEngine.resetHandler = ^{
                    NSLog(@"[CoreHaptics] 엔진 리셋됨");
                    g_engineStarted = false;
                    if (g_hapticEngine != nil) {
                        NSError* error = nil;
                        [g_hapticEngine startAndReturnError:&error];
                        if (error == nil) {
                            g_engineStarted = true;
                        }
                    }
                };
            }
        }
    }
    
    // CoreHaptics 엔진 시작
    bool StartCoreHapticsEngine() {
        if (@available(iOS 13, *)) {
            if (g_hapticEngine == nil) {
                InitializeCoreHaptics();
            }
            
            if (g_hapticEngine != nil && !g_engineStarted) {
                NSError* error = nil;
                [g_hapticEngine startAndReturnError:&error];
                
                if (error != nil) {
                    NSLog(@"[CoreHaptics] 엔진 시작 실패: %@", error.localizedDescription);
                    return false;
                }
                
                g_engineStarted = true;
                return true;
            }
            return g_engineStarted;
        }
        return false;
    }
    
    // 기본 진동 (duration 밀리초)
    void VibrateWithDuration(float duration) {
        if (@available(iOS 13, *)) {
            if (!StartCoreHapticsEngine()) {
                // CoreHaptics 실패 시 기본 진동 사용
                AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
                return;
            }
            
            NSError* error = nil;
            
            // 진동 이벤트 생성
            CHHapticEvent* event = [[CHHapticEvent alloc]
                                    initWithEventType:CHHapticEventTypeHapticTransient
                                    parameters:@[]
                                    relativeTime:0
                                    duration:duration / 1000.0f]; // 초 단위로 변환
            
            CHHapticPattern* pattern = [[CHHapticPattern alloc]
                                       initWithEvents:@[event]
                                       parameters:@[]
                                       error:&error];
            
            if (error != nil) {
                NSLog(@"[CoreHaptics] 패턴 생성 실패: %@", error.localizedDescription);
                AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
                return;
            }
            
            id<CHHapticPatternPlayer> player = [g_hapticEngine createPlayerWithPattern:pattern error:&error];
            
            if (error != nil) {
                NSLog(@"[CoreHaptics] 플레이어 생성 실패: %@", error.localizedDescription);
                AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
                return;
            }
            
            [player startAtTime:0 error:&error];
            
            if (error != nil) {
                NSLog(@"[CoreHaptics] 진동 재생 실패: %@", error.localizedDescription);
                AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
            }
        } else {
            // iOS 13 미만은 기본 진동 사용
            AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
        }
    }
    
    // 강도와 지속시간을 지정한 진동
    void VibrateWithIntensityAndDuration(float intensity, float duration) {
        if (@available(iOS 13, *)) {
            if (!StartCoreHapticsEngine()) {
                AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
                return;
            }
            
            NSError* error = nil;
            
            // 강도 파라미터 생성 (0.0 ~ 1.0)
            CHHapticEventParameter* intensityParam = [[CHHapticEventParameter alloc]
                                                     initWithParameterID:CHHapticEventParameterIDHapticIntensity
                                                     value:intensity];
            
            // 진동 이벤트 생성
            CHHapticEvent* event = [[CHHapticEvent alloc]
                                    initWithEventType:CHHapticEventTypeHapticContinuous
                                    parameters:@[intensityParam]
                                    relativeTime:0
                                    duration:duration / 1000.0f];
            
            CHHapticPattern* pattern = [[CHHapticPattern alloc]
                                       initWithEvents:@[event]
                                       parameters:@[]
                                       error:&error];
            
            if (error != nil) {
                NSLog(@"[CoreHaptics] 패턴 생성 실패: %@", error.localizedDescription);
                AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
                return;
            }
            
            id<CHHapticPatternPlayer> player = [g_hapticEngine createPlayerWithPattern:pattern error:&error];
            
            if (error != nil) {
                NSLog(@"[CoreHaptics] 플레이어 생성 실패: %@", error.localizedDescription);
                AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
                return;
            }
            
            [player startAtTime:0 error:&error];
            
            if (error != nil) {
                NSLog(@"[CoreHaptics] 진동 재생 실패: %@", error.localizedDescription);
                AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
            }
        } else {
            AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
        }
    }
    
    // 패턴 진동 (밀리초 단위 배열)
    void VibrateWithPattern(long* pattern, int patternLength) {
        if (@available(iOS 13, *)) {
            if (!StartCoreHapticsEngine()) {
                AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
                return;
            }
            
            NSError* error = nil;
            NSMutableArray<CHHapticEvent*>* events = [[NSMutableArray alloc] init];
            
            double currentTime = 0;
            bool isVibration = true; // 첫 번째는 진동으로 시작
            
            for (int i = 0; i < patternLength; i++) {
                float duration = pattern[i] / 1000.0f; // 초 단위로 변환
                
                if (duration > 0) {
                    if (isVibration) {
                        // 진동 이벤트 생성
                        CHHapticEvent* event = [[CHHapticEvent alloc]
                                                initWithEventType:CHHapticEventTypeHapticTransient
                                                parameters:@[]
                                                relativeTime:currentTime
                                                duration:duration];
                        [events addObject:event];
                    }
                    // 대기 시간은 이벤트를 추가하지 않음 (relativeTime만 증가)
                    
                    currentTime += duration;
                }
                
                isVibration = !isVibration; // 진동/대기 토글
            }
            
            if (events.count == 0) {
                AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
                return;
            }
            
            CHHapticPattern* hapticPattern = [[CHHapticPattern alloc]
                                             initWithEvents:events
                                             parameters:@[]
                                             error:&error];
            
            if (error != nil) {
                NSLog(@"[CoreHaptics] 패턴 생성 실패: %@", error.localizedDescription);
                AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
                return;
            }
            
            id<CHHapticPatternPlayer> player = [g_hapticEngine createPlayerWithPattern:hapticPattern error:&error];
            
            if (error != nil) {
                NSLog(@"[CoreHaptics] 플레이어 생성 실패: %@", error.localizedDescription);
                AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
                return;
            }
            
            [player startAtTime:0 error:&error];
            
            if (error != nil) {
                NSLog(@"[CoreHaptics] 진동 재생 실패: %@", error.localizedDescription);
                AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
            }
        } else {
            AudioServicesPlaySystemSound(kSystemSoundID_Vibrate);
        }
    }
    
    // CoreHaptics 엔진 중지
    void StopCoreHapticsEngine() {
        if (@available(iOS 13, *)) {
            if (g_hapticEngine != nil && g_engineStarted) {
                [g_hapticEngine stopWithCompletionHandler:^(NSError* error) {
                    if (error != nil) {
                        NSLog(@"[CoreHaptics] 엔진 중지 실패: %@", error.localizedDescription);
                    }
                }];
                g_engineStarted = false;
            }
        }
    }
}

