using UnityEngine;
using Unity.Services.LevelPlay;
using System;

namespace FAIRSTUDIOS.Manager
{
  /// <summary>
  /// 광고 관리 매니저
  /// created kas.
  /// </summary>
  public class AdManager : Singleton<AdManager>//, IUnityAdsListener
  {
#if UNITY_ANDROID
    string appKey = "2433a4d65";
    string bannerAdUnitId = "jyn2eahywlsut710";
    string interstitialAdUnitId = "g4982qh27z7shhmg";
#elif UNITY_IOS
    string appKey = "2433a13dd";
    string bannerAdUnitId = "zp66aptxldfm434s";
    string interstitialAdUnitId = "od7n3duhhc9ow8xi";
#else
    string appKey = "unexpected_platform";
    string bannerAdUnitId = "unexpected_platform";
    string interstitialAdUnitId = "unexpected_platform";
#endif

    // 🔹 외부에서 구독 가능한 이벤트 추가
    public event Action onInternetLostEvent;
    public event Action onInternetRestoredEvent;

    private bool initialized = false;

    private LevelPlayBannerAd bannerAd;

    private LevelPlayInterstitialAd interstitialAd;
    private Action onInterstitialAdCompleted;
    private Action<int> onInterstitialAdFailed; // 에러 코드를 함께 넘기는 실패 콜백

    [Header("인터넷 체크 간격 (초)")]
    [SerializeField] private float internetCheckInterval = 3f;
    private float timer;
    private bool isConnected;

    public bool IsShowBanner { get; private set; }
    public float BannerHeight => 50 + Mathf.RoundToInt(50 * Screen.dpi / 160);

    public bool WaitingForInternet { get; private set; }
    private bool IsInternetAvailable => Application.internetReachability != NetworkReachability.NotReachable;

    protected override void Start()
    {
      base.Start();

      Debug.Log("unity-script: IronSource.Agent.validateIntegration");

      // SDK init
      Debug.Log("unity-script: LevelPlay SDK initialization");

      LevelPlay.Init(appKey);
      LevelPlay.OnInitSuccess += SdkInitializationCompletedEvent;
      LevelPlay.OnInitFailed += SdkInitializationFailedEvent;
    }

    private void Update()
    {
      timer += Time.deltaTime;
      if (timer < internetCheckInterval)
        return;
      timer = 0f;

      bool nowConnected = IsInternetAvailable;

      if (nowConnected && !isConnected)
      {
        Debug.Log("🌐 Internet connection restored");
        OnInternetRestored();
      }
      else if (!nowConnected && isConnected)
      {
        Debug.Log("❌ Internet connection lost");
        OnInternetLost();
      }

      isConnected = nowConnected;
    }

    private void SdkInitializeComplete()
    {
      if (initialized)
        return;

      initialized = true;

      bannerAd = new LevelPlayBannerAd(bannerAdUnitId);

      // Register to Banner events
      bannerAd.OnAdLoaded += BannerOnAdLoadedEvent;
      bannerAd.OnAdLoadFailed += BannerOnAdLoadFailedEvent;
      bannerAd.OnAdDisplayed += BannerOnAdDisplayedEvent;
      bannerAd.OnAdClicked += BannerOnAdClickedEvent;
      bannerAd.OnAdCollapsed += BannerOnAdCollapsedEvent;
      bannerAd.OnAdLeftApplication += BannerOnAdLeftApplicationEvent;
      bannerAd.OnAdExpanded += BannerOnAdExpandedEvent;

      // 배너 / 전면 광고 선로드
#if !UNITY_EDITOR
      try
      {
        bannerAd.LoadAd();       // 배너 선로드
      }
      catch (Exception e)
      {
        Debug.LogError($"[AdManager] 초기 배너 LoadAd 호출 중 예외 발생: {e}");
      }
#endif

      // Create Interstitial object
      interstitialAd = new LevelPlayInterstitialAd(interstitialAdUnitId);

      // Register to Interstitial events
      interstitialAd.OnAdLoaded += InterstitialOnAdLoadedEvent;
      interstitialAd.OnAdLoadFailed += InterstitialOnAdLoadFailedEvent;
      interstitialAd.OnAdDisplayed += InterstitialOnAdDisplayedEvent;
      interstitialAd.OnAdClicked += InterstitialOnAdClickedEvent;
      interstitialAd.OnAdClosed += InterstitialOnAdClosedEvent;
      interstitialAd.OnAdInfoChanged += InterstitialOnAdInfoChangedEvent;

      // 초기화 완료 후 전면 광고는 미리 로드해 둔다.
#if !UNITY_EDITOR
      try
      {
        interstitialAd.LoadAd();
      }
      catch (Exception e)
      {
        Debug.LogError($"[AdManager] 초기 전면광고 LoadAd 호출 중 예외 발생: {e}");
      }
#endif
    }

    private void OnApplicationPause(bool isPaused)
    {
      Debug.Log("unity-script: OnApplicationPause = " + isPaused);
    }

    #region Init callback handlers

    void SdkInitializationCompletedEvent(LevelPlayConfiguration config)
    {
      Debug.Log("unity-script: I got SdkInitializationCompletedEvent with config: " + config);

      SdkInitializeComplete();
    }

    void SdkInitializationFailedEvent(LevelPlayInitError error)
    {
      Debug.Log("unity-script: I got SdkInitializationFailedEvent with error: " + error);
    }

    #endregion

    #region AdInfo Interstitial

    void InterstitialOnAdLoadedEvent(LevelPlayAdInfo adInfo)
    {
      Debug.Log("unity-script: I got InterstitialOnAdLoadedEvent With AdInfo " + adInfo);
      WaitingForInternet = false;
    }

    void InterstitialOnAdLoadFailedEvent(LevelPlayAdError error)
    {
      Debug.Log("unity-script: I got InterstitialOnAdLoadFailedEvent With Error " + error);

      int code = error.ErrorCode;

      // 에러 코드에 따른 분기
      switch (code)
      {
        case 520: // 네트워크 단절
          Debug.LogWarning("[AdManager][Interstitial] Network connection lost. Please check your internet connection.");
          WaitingForInternet = true;
          break;
        case 508: // 타임아웃
          Debug.LogWarning("[AdManager][Interstitial] Ad request timed out.");
          break;
        case 507: // No Fill
          Debug.LogWarning("[AdManager][Interstitial] No fill from ad network. (No Fill)");
          break;
        default:
          Debug.LogWarning($"[AdManager][Interstitial] 알 수 없는 에러 코드: {code}, message: {error.ErrorMessage}");
          break;
      }

      // 실패 콜백 호출 (에러 코드 전달)
      onInterstitialAdFailed?.Invoke(code);
      onInterstitialAdFailed = null;
      onInterstitialAdCompleted = null;
    }

    void InterstitialOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
    {
      Debug.Log("unity-script: I got InterstitialOnAdDisplayedEvent With AdInfo " + adInfo);
    }

    void InterstitialOnAdClickedEvent(LevelPlayAdInfo adInfo)
    {
      Debug.Log("unity-script: I got InterstitialOnAdClickedEvent With AdInfo " + adInfo);
    }

    void InterstitialOnAdClosedEvent(LevelPlayAdInfo adInfo)
    {
      Debug.Log("unity-script: I got InterstitialOnAdClosedEvent With AdInfo " + adInfo);

      onInterstitialAdCompleted?.Invoke();
      onInterstitialAdCompleted = null;
      onInterstitialAdFailed = null;
      interstitialAd.LoadAd();
    }

    void InterstitialOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
    {
      Debug.Log("unity-script: I got InterstitialOnAdInfoChangedEvent With AdInfo " + adInfo);
    }

    #endregion

    #region Banner AdInfo

    void BannerOnAdLoadedEvent(LevelPlayAdInfo adInfo)
    {
      Debug.Log("unity-script: I got BannerOnAdLoadedEvent With AdInfo " + adInfo);
    }

    void BannerOnAdLoadFailedEvent(LevelPlayAdError error)
    {
      Debug.Log("unity-script: I got BannerOnAdLoadFailedEvent With Error " + error);
      int code = error.ErrorCode;

      // 에러 코드에 따른 처리: 스위치문으로 세분화
      switch (code)
      {
        case 520: // 네트워크 단절
          Debug.LogWarning("❌ [AdManager][Banner] Internet connection lost. Please check your network.");
          break;
        case 508: // 타임아웃
          Debug.LogWarning("⚠️ [AdManager][Banner] Ad request timed out. Please check your internet speed.");
          break;
        case 507: // No Fill
          Debug.LogWarning("🕓 [AdManager][Banner] No ads available from the ad network. (Network No Fill)");
          break;
        default:
          Debug.LogWarning($"[AdManager][Banner] 알 수 없는 에러 코드: {code}, message: {error.ErrorMessage}");
          break;
      }

      // 로드 실패 이후에도, 외부에서 ShowBannerAd 를 다시 호출하면
      // bannerAd.LoadAd() 를 통해 재시도할 수 있도록 특별히 막지 않는다.
    }

    void BannerOnAdClickedEvent(LevelPlayAdInfo adInfo)
    {
      Debug.Log("unity-script: I got BannerOnAdClickedEvent With AdInfo " + adInfo);
    }

    void BannerOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
    {
      Debug.Log("unity-script: I got BannerOnAdDisplayedEvent With AdInfo " + adInfo);
    }

    void BannerOnAdCollapsedEvent(LevelPlayAdInfo adInfo)
    {
      Debug.Log("unity-script: I got BannerOnAdCollapsedEvent With AdInfo " + adInfo);
    }

    void BannerOnAdLeftApplicationEvent(LevelPlayAdInfo adInfo)
    {
      Debug.Log("unity-script: I got BannerOnAdLeftApplicationEvent With AdInfo " + adInfo);
    }

    void BannerOnAdExpandedEvent(LevelPlayAdInfo adInfo)
    {
      Debug.Log("unity-script: I got BannerOnAdExpandedEvent With AdInfo " + adInfo);
    }

    #endregion

    // ✅ 외부에서 이벤트 등록 / 해제할 수 있는 함수 제공
    public void AddOnInternetLostListener(Action callback)
    {
      onInternetLostEvent += callback;
    }

    public void RemoveOnInternetLostListener(Action callback)
    {
      onInternetLostEvent -= callback;
    }

    public void AddOnInternetRestoredListener(Action callback)
    {
      onInternetRestoredEvent += callback;
    }

    public void RemoveOnInternetRestoredListener(Action callback)
    {
      onInternetRestoredEvent -= callback;
    }

    /// <summary>
    /// 디스플레이 전면 광고 호출
    /// </summary>
    public void ShowInterstitial(Action onComplete = null, Action<int> onFailed = null)
    {
      Debug.Log("unity-script: ShowInterstitialButtonClicked");

      // SDK 또는 전면 광고 객체가 아직 준비되지 않은 경우 크래시 방지
      if (!initialized || interstitialAd == null)
      {
        Debug.LogWarning("[AdManager] ShowInterstitial 호출 시 SDK 미초기화 또는 interstitialAd == null");
        onFailed?.Invoke(-1); // 내부적인 에러 코드(-1) 전달
        return;
      }

      // 콜백 보관 (성공/실패) — 실제 ShowAd 가 호출되는 시점 기준으로 유효
      onInterstitialAdCompleted = onComplete;
      onInterstitialAdFailed = onFailed;

      if (!IsInternetAvailable)
      {
        Debug.Log("🚫 인터넷 연결 끊김. 광고 표시 대기.");
        WaitingForInternet = true;
        return;
      }

      if (interstitialAd.IsAdReady())
      {
        Debug.Log("ShowInterstitial / IsAdReady() = true");
        interstitialAd.ShowAd();
      }
      else
      {
        Debug.Log("ShowInterstitial / IsAdReady() = false");
        interstitialAd.LoadAd();
      }
    }
    public void ShowBannerAd()
    {
      Debug.Log("unity-script: loadBannerButtonClicked");
#if !UNITY_EDITOR
      bannerAd.LoadAd();
#endif
      IsShowBanner = true;
    }

    public void HideBannerAd()
    {
      Debug.Log("unity-script: HideButtonClicked");
#if !UNITY_EDITOR
      bannerAd.HideAd();
      IsShowBanner = false;
#endif
    }

    // -------------------------------
    // 인터넷 상태 이벤트 처리
    // -------------------------------
    private void OnInternetLost()
    {
      // 배너 닫기
      HideBannerAd();
      onInternetLostEvent?.Invoke();
    }

    private void OnInternetRestored()
    {
      // 전면 광고 다시 로드
      if (WaitingForInternet && (onInterstitialAdCompleted != null || onInterstitialAdFailed != null))
      {
        Debug.Log("🔄 인터넷 복구로 인한 전면 광고 재시도");
        WaitingForInternet = false;
        ShowInterstitial(onInterstitialAdCompleted, onInterstitialAdFailed);
      }
      else
      {
        interstitialAd?.LoadAd();
      }

      // 배너 다시 표시 (원래 배너가 노출 중이었던 경우에만)
      if (IsShowBanner)
      {
        ShowBannerAd();
      }

      onInternetRestoredEvent?.Invoke();
    }
  }
}