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
        Debug.Log("🌐 인터넷 복구됨");
        OnInternetRestored();
      }
      else if (!nowConnected && isConnected)
      {
        Debug.Log("❌ 인터넷 끊김 감지");
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

      // Create Interstitial object
      interstitialAd = new LevelPlayInterstitialAd(interstitialAdUnitId);

      // Register to Interstitial events
      interstitialAd.OnAdLoaded += InterstitialOnAdLoadedEvent;
      interstitialAd.OnAdLoadFailed += InterstitialOnAdLoadFailedEvent;
      interstitialAd.OnAdDisplayed += InterstitialOnAdDisplayedEvent;
      interstitialAd.OnAdClicked += InterstitialOnAdClickedEvent;
      interstitialAd.OnAdClosed += InterstitialOnAdClosedEvent;
      interstitialAd.OnAdInfoChanged += InterstitialOnAdInfoChangedEvent;
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

      if (WaitingForInternet && onInterstitialAdCompleted != null)
      {
        WaitingForInternet = false;
        ShowInterstitial(onInterstitialAdCompleted);
      }
    }

    void InterstitialOnAdLoadFailedEvent(LevelPlayAdError error)
    {
      Debug.Log("unity-script: I got InterstitialOnAdLoadFailedEvent With Error " + error);
      if (error.ErrorCode == 520)
      {
        WaitingForInternet = true;
      }
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
      if (code == 520)
      {
        Debug.LogWarning("❌ 인터넷 연결이 끊겨 있습니다. 네트워크를 확인하세요.");
      }
      else if (code == 508)
      {
        Debug.LogWarning("⚠️ 광고 요청이 타임아웃 되었습니다. 인터넷 속도를 확인하세요.");
      }
      else if (code == 507)
      {
        Debug.LogWarning("🕓 광고 네트워크에서 광고가 없습니다. (Network No Fill)");
      }
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
    public void ShowInterstitial(Action onComplete = null, Action onFailed = null)
    {
      Debug.Log("unity-script: ShowInterstitialButtonClicked");
      if (!IsInternetAvailable)
      {
        Debug.Log("🚫 인터넷 연결 끊김. 광고 표시 대기.");
        WaitingForInternet = true;
        onInterstitialAdCompleted = onComplete;
        onFailed?.Invoke();
        return;
      }

      if (interstitialAd.IsAdReady())
      {
        Debug.Log("✅ 전면 광고 표시");
        interstitialAd.ShowAd();
        onComplete?.Invoke();
      }
      else
      {
        Debug.Log("📭 광고 준비 중. 로드 후 재시도 예정");
        onInterstitialAdCompleted = onComplete;
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
      interstitialAd?.LoadAd();

      // 배너 다시 표시
      ShowBannerAd();

      // 전면 광고 콜백 대기 중이면 재시도
      if (WaitingForInternet && onInterstitialAdCompleted != null)
      {
        Debug.Log("🔄 인터넷 복구로 인한 전면 광고 재시도");
        WaitingForInternet = false;
        ShowInterstitial(onInterstitialAdCompleted);
      }

      onInternetRestoredEvent?.Invoke();
    }
  }
}