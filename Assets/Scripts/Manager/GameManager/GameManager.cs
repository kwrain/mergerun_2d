using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Core;

/// <summary>
/// 게임 코어로직 관련 매니저
/// <code>
/// - 각 오브젝트 별 생성 및 풀링
/// - 오브젝트 관련 기능 함수 정의
/// - 오브젝트 별 관리 대상 정리
/// 2021.12.23
/// - GameManager, NpcManager 기능을 통합함.
/// - 기능은 통합하되, 클래스를 partial 로 분리
/// </code>
/// </summary>
public partial class GameManager : Singleton<GameManager>
{
  /// <summary>
  /// 업데이트 구분이 필요한 모델들을 등록해서 사용.
  /// 주로, 애니메이션이 필요하나 건물이나 엔피씨처럼 관리되지 않는 경우에 해당됨.
  /// </summary>
  private List<BaseObject> onUpdateModels = new();


  protected override void Awake()
  {
    base.Awake();

    AutoSetting();

    // 화면 방향을 완전히 세로로 고정한다.
    // OS 자동 회전 설정과 무관하게 Unity에서 강제.
    Screen.orientation = ScreenOrientation.Portrait;

    // 혹시 다른 코드에서 AutoRotation 으로 전환하더라도
    // 가로는 허용되지 않도록 기본 플래그도 세로만 허용.
    Screen.autorotateToPortrait = true;
    Screen.autorotateToPortraitUpsideDown = true;
    Screen.autorotateToLandscapeLeft = false;
    Screen.autorotateToLandscapeRight = false;      
  }

  protected override void ScenePreloadEvent(Scene currScene)
  {
    base.ScenePreloadEvent(currScene);

    Debug.LogWarning($"ScenePreloadEvent currScene : {currScene.name}");

    GameModel.Global?.OnSceneChanged(currScene);

    TouchManager.AddListenerTouchEvent(this, TOUCH_PRIORITY);
  }

  protected override async void SceneLoadedEvent(Scene scene, LoadSceneMode SceneMode)
  {
    base.SceneLoadedEvent(scene, SceneMode);

    Debug.LogWarning($"SceneLoadedEvent scene : {scene.name}");
  }

  /// <summary>
  /// lds - 22.9.1 어플리케이션 종료 시 발생하는 이벤트
  /// </summary>
  protected override void OnApplicationQuit()
  {
    // NotificationBadgeClear();

    base.OnApplicationQuit();
  }

  private void OnApplicationPause(bool pauseStatus)
  {
    // Debug.Log($"KW / ApplicationPause  / GameManager / Status {pauseStatus}");

    IsApplicationPause = pauseStatus;
    if (pauseStatus)
    {
      SuspendTime = 0f;
      pauseStartTime = DateTime.Now;
    }
    else
    {
      var diff = DateTime.Now - pauseStartTime;
      SuspendTime = (float)diff.TotalSeconds;
      // Debug.Log($"KW / Calc SuspendTime : {SuspendTime}");
    }

    // NotificationBadgeClear();

    if (SOManager.IsCreated)
    {
      GameModel.Global.OnApplicationPauseModel(pauseStatus);
    }

    foreach(var model in onUpdateModels)
    {
      model.ApplicationPause(pauseStatus);
    }
  }

  private void Update()
  {
    if (IsApplicationPause)
      return;

    // lds - 24.1.26, 현재 씬이 섬씬인 경우에만 아래 Update 처리 하도록함
    // 밭에 작물을 심고나서 로그아웃 후에도 해당 작물 프로세스가 진행되면서 object_information.php 무한 요청 현상이 확인되어 수정함
    // 이외에 다른 오브젝트들도 해당 현상이 발생할 수 있기 때문에 함께 처리함.
    // 섬씬이 아닌곳에서 섬 오브젝트들을 Update처리해야될일은 없겠지만, 추후 필요한 경우 분기 처리가 필요할것으로 보임.
    if (KSceneManager.IsCreated == true)
    {

    }
    else
    {
      return;
    }

    if(onUpdateModels != null && onUpdateModels.Count > 0)
    {
      for (int i = onUpdateModels.Count - 1; i >= 0; i--)
      {
        var model = onUpdateModels[i];
        if (model == null)
          continue;

        model.OnUpdate();
      }
    }
  }

  public override async Task Initialize()
  {
    await base.Initialize();
    
    // Unity Services 초기화 (Analytics 사용을 위해 필요)
    await InitializeUnityServices();
  }

  /// <summary>
  /// Unity Services 초기화
  /// </summary>
  public async Task InitializeUnityServices()
  {
    try
    {
      await UnityServices.InitializeAsync();
      Debug.Log("[GameManager] Unity Services 초기화 완료");
    }
    catch (Exception e)
    {
      Debug.LogWarning($"[GameManager] Unity Services 초기화 실패: {e.Message}");
    }
  }

  /// <summary>
  /// 데이터 수집 동의 요청 처리 (iOS ATT 또는 안드로이드 동의 팝업)
  /// </summary>
  /// <param name="onComplete">동의 처리 완료 콜백</param>
  public async Task RequestDataCollectionConsent(System.Action onComplete = null)
  {
#if UNITY_IOS && !UNITY_EDITOR
    // iOS: ATT 권한 요청
    await RequestIOSDataCollectionConsent(onComplete);
#elif UNITY_ANDROID && !UNITY_EDITOR
    // 안드로이드: 데이터 수집 동의 팝업
    await RequestAndroidDataCollectionConsent(onComplete);
#else
    // 에디터나 다른 플랫폼: 동의한 것으로 간주하고 데이터 수집 시작
    Debug.Log("[GameManager] 에디터/기타 플랫폼: 데이터 수집 동의 건너뜀");
    StartDataCollection();
    onComplete?.Invoke();
    await Task.CompletedTask;
#endif
  }

#if UNITY_IOS && !UNITY_EDITOR
  /// <summary>
  /// iOS ATT 권한 요청 처리
  /// </summary>
  private async Task RequestIOSDataCollectionConsent(System.Action onComplete)
  {
    var attBridge = FAIRSTUDIOS.Manager.AppTrackingTransparencyBridge.Instance;
    var currentStatus = attBridge.GetStatus();

    Debug.Log($"[GameManager] 현재 ATT 권한 상태: {currentStatus}");

    // 아직 요청하지 않은 경우에만 권한 요청
    if (currentStatus == FAIRSTUDIOS.Manager.AppTrackingTransparencyBridge.TrackingAuthorizationStatus.NotDetermined)
    {
      Debug.Log("[GameManager] ATT 권한 요청 시작");
      
      var tcs = new TaskCompletionSource<bool>();
      attBridge.RequestAuthorization((status) =>
      {
        Debug.Log($"[GameManager] ATT 권한 요청 완료: {status}");
        
        // 권한이 허용된 경우 데이터 수집 시작
        if (status == FAIRSTUDIOS.Manager.AppTrackingTransparencyBridge.TrackingAuthorizationStatus.Authorized)
        {
          StartDataCollection();
        }
        
        tcs.SetResult(true);
      });
      
      await tcs.Task;
    }
    else
    {
      // 이미 권한이 결정된 경우
      Debug.Log($"[GameManager] ATT 권한이 이미 결정되어 있습니다: {currentStatus}");
      
      // 권한이 허용된 경우 데이터 수집 시작
      if (currentStatus == FAIRSTUDIOS.Manager.AppTrackingTransparencyBridge.TrackingAuthorizationStatus.Authorized)
      {
        StartDataCollection();
      }
    }
    
    onComplete?.Invoke();
  }
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
  /// <summary>
  /// 안드로이드 데이터 수집 동의 팝업 처리
  /// </summary>
  private async Task RequestAndroidDataCollectionConsent(System.Action onComplete)
  {
    // 이미 동의 여부가 저장되어 있는지 확인
    bool hasConsent = DevicePrefs.HasKey(EDevicePrefs.ANDROID_ANALYTICS_CONSENT);
    
    if (hasConsent)
    {
      // 이미 동의 여부가 결정된 경우
      bool consentGiven = DevicePrefs.GetBool(EDevicePrefs.ANDROID_ANALYTICS_CONSENT);
      Debug.Log($"[GameManager] 안드로이드 데이터 수집 동의 상태: {consentGiven}");
      
      if (consentGiven)
      {
        StartDataCollection();
      }
      
      onComplete?.Invoke();
      return;
    }

    // 동의 여부가 결정되지 않은 경우 팝업 표시
    Debug.Log("[GameManager] 안드로이드 데이터 수집 동의 팝업 표시");
    
    // 한국어인지 확인하여 메시지 언어 결정
    bool isKorean = Application.systemLanguage == SystemLanguage.Korean;
    
    string title, message, positiveText, negativeText;
    
    if (isKorean)
    {
      // 한국어 메시지
      title = "데이터 수집 동의";
      message = "게임 개선을 위해 데이터를 수집합니다.\n데이터 수집에 동의하시겠습니까?";
      positiveText = "동의";
      negativeText = "거부";
    }
    else
    {
      // 영어 메시지 (한국을 제외한 모든 국가)
      title = "Data Collection Consent";
      message = "We collect data to improve the game.\nDo you agree to data collection?";
      positiveText = "Agree";
      negativeText = "Decline";
    }
    
    var tcs = new TaskCompletionSource<bool>();
    
    FAIRSTUDIOS.Manager.AndroidDialogManager.Instance.ShowDialog(
      title: title,
      message: message,
      positiveButtonText: positiveText,
      negativeButtonText: negativeText,
      onPositiveClick: () =>
      {
        Debug.Log("[GameManager] 사용자가 데이터 수집에 동의했습니다.");
        DevicePrefs.SetBool(EDevicePrefs.ANDROID_ANALYTICS_CONSENT, true);
        StartDataCollection();
        tcs.SetResult(true);
      },
      onNegativeClick: () =>
      {
        Debug.Log("[GameManager] 사용자가 데이터 수집을 거부했습니다.");
        DevicePrefs.SetBool(EDevicePrefs.ANDROID_ANALYTICS_CONSENT, false);
        tcs.SetResult(true);
      }
    );
    
    await tcs.Task;
    onComplete?.Invoke();
  }
#endif

  /// <summary>
  /// 데이터 수집 시작
  /// </summary>
  private void StartDataCollection()
  {
    try
    {
      Unity.Services.Analytics.AnalyticsService.Instance.StartDataCollection();
      Debug.Log("[GameManager] 데이터 수집 시작");
    }
    catch (Exception e)
    {
      Debug.LogWarning($"[GameManager] 데이터 수집 시작 실패: {e.Message}");
    }
  }

  /// <summary>
  /// Update 루틴이 필요한 모델 등록
  /// </summary>
  /// <param name="value"></param>
  public void AddUpdateModel(BaseObject value)
  {
    if (onUpdateModels.Contains(value))
      return;

    onUpdateModels.Add(value);
  }

  /// <summary>
  /// Update 루틴이 필요없어질 경우 모델 등록 해제
  /// </summary>
  /// <param name="value"></param>
  public void RemoveUpdateModel(BaseObject value)
  {
    onUpdateModels.Remove(value);
  }

  public void UpdateHUD()
  {

  }
}