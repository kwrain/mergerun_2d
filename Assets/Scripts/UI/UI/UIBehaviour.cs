using System;
using System.Threading;
using System.Threading.Tasks;
using DG.Tweening;
using FAIRSTUDIOS.SODB.Core;
using UnityEngine;
using UnityEngine.Events;

namespace FAIRSTUDIOS.UI
{
  /// <summary>
  /// UI 기본 클래스
  /// </summary>
  public class UIBehaviour : MonoBehaviour
  {
    private RectTransform _rectTransform;

    private bool bInitAnchor = false;
    private bool bInitAnimation;

    protected Action onShowCompleteAction;
    protected Action onHideCompleteAction;

    protected Sequence showDefaultSequence;
    protected Sequence hideDefaultSequence;

    protected CanvasGroup canvasGroup;

    [SerializeField, HideInInspector] protected bool useShowAnimation;
    [SerializeField, HideInInspector] protected bool useShowDefaultAnimation = true;
    [SerializeField, HideInInspector] protected string useShowAnimationID = "Show";

    [SerializeField, HideInInspector] protected bool useHideAnimation;
    [SerializeField, HideInInspector] protected bool useHideDefaultAnimation = true;
    [SerializeField, HideInInspector] protected string useHideAnimationID = "Hide";

    [SerializeField, HideInInspector] protected bool isPlayedShowSound;
    [SerializeField, HideInInspector] protected bool isPlayedHideSound;

    [SerializeField, HideInInspector] private UnityEvent onShowComplete;
    [SerializeField, HideInInspector] private UnityEvent onHideComplete;

    [SerializeField, HideInInspector] protected bool playShowAndHideSound = true;
    [SerializeField, HideInInspector] protected bool isPlayingAnimation;

    [SerializeField, HideInInspector, Tooltip("Prepare를 취소하는 시간값(ms)"), Range(1000, 30000)]
    protected int preparingCancelTime = 3000;

    protected bool UseShowDefaultAnimation => useShowDefaultAnimation;
    protected bool UseHideDefaultAnimation => useHideDefaultAnimation;
    
    public bool UseShowAnimation => useShowAnimation;
    public bool UseHideAnimation => useHideAnimation;

    public bool PlayShowAndHideSound => playShowAndHideSound;

    public bool IsPlayingAnimation
    {
      get => isPlayingAnimation;
      private set
      {
        isPlayingAnimation = value;
      }
    }

    public int PreparingCancelTime => preparingCancelTime;

    public bool RequireUpdate { get; set; }


    protected virtual SoundFxTypes ShowSound => SoundFxTypes.SHOW_UI;
    protected virtual SoundFxTypes HideSound => SoundFxTypes.HIDE_UI;

    public RectTransform rectTransform
    {
      get
      {
        if (null == _rectTransform)
          _rectTransform = GetComponent<RectTransform>();

        return _rectTransform;
      }
    }

    public UIAttribute Attribute { get; set; }

    public UIBehaviour PrevUI { get; set; }

    /// <summary>
    /// UI 사전 준비 동작 중 캔슬 테스크가 반드시 실행 되는데, 그 사이에 완료되면 캔슬 테스크를 취소하기 위한 토큰 소스
    /// </summary>
    public CancellationTokenSource PrepareCancelTokenSource { get; protected set; }

    public DOTweenSequencer Sequencer { get; private set; }

    /// <summary>
    /// lds - 22.10.28, Start 여부
    /// </summary>
    /// <value></value>
    public bool IsStarted { get; private set; }
    private CancellationTokenSource waitStartCancellationTokenSource;

    /// <summary>
    /// lds - 23.2.16, Prepared 대리자 <br/>
    /// prepared 가 true이면 Prepare성공, false이면 실패
    /// </summary>
    /// <param name="prepared"></param>
    public delegate void OnPrepared(bool prepared);
    /// <summary>
    /// lds - 23.2.15, prepared 마지막에 호출시킬 콜벡 <br/>
    /// </summary>
    public OnPrepared preparedCallback;

    /// <summary>
    /// lds - 23.6.7, 앱 종료 발생 여부.
    /// </summary>
    protected bool isApplicationQuit = false;

    protected virtual void Awake()
    {
      isApplicationQuit = false; // lds - 23.6.7, 추가
      canvasGroup = GetComponent<CanvasGroup>();
      if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

      onShowComplete.AddListener(() => { onShowCompleteAction?.Invoke(); });
      onHideComplete.AddListener(() => { onHideCompleteAction?.Invoke(); });

      // lds - 22.7.16
      // 기존에는 ModelBehaviourDataSet을 사용하는 UI나 Popup의 Awake에서 뷰모델을 활성화 해주었지만
      // 그렇지 않은 UI나 Popup의 경우 하위에 뷰모델이 존재하면 활성화가 되지 않던 상황이 있음
      // 따라서 Sodb의 ModelBehaviourDataSet에서 ViewModel을 활성화 해주던 부분을 제거하고 UIBehaviour에 추가.
      // 신규 Vm도 동작하도록 추가함
      var vms = GetComponentsInChildren<VmBase>(true);
      for (var i = 0; i < vms.Length; i++)
        vms[i]?.gameObject?.SetActive(true);

      var viewModels = GetComponentsInChildren<ViewModelBase>(true);
      for (var i = 0; i < viewModels.Length; i++)
        viewModels[i]?.gameObject?.SetActive(true);
    }

    protected virtual void Start()
    {
      InitAnimation();
      IsStarted = true; // lds - 22.10.28, Is Started.
      waitStartCancellationTokenSource?.Cancel();
      waitStartCancellationTokenSource = null;
    }

    /// <summary>
    /// lds - 22.10.28, Start 대기
    /// </summary>
    /// <returns></returns>
    public async Task WaitStartSync()
    {
      if (IsStarted == true)
      {
        return;
      }
      waitStartCancellationTokenSource = new();
      while (Application.isPlaying == true && IsStarted == false)
      {
        if (waitStartCancellationTokenSource == null || waitStartCancellationTokenSource.IsCancellationRequested == true)
        {
          break;
        }
        await Task.Yield();
      }
    }

    protected virtual void OnEnable()
    {
      if (canvasGroup != null)
      {
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
      }
    }

    protected virtual void OnDisable()
    {
      isPlayedShowSound = false;
      isPlayedHideSound = false;
    }

    // lds - 23.3.29 추가
    protected virtual void OnDestroy()
    {
      // lds 23.3.29, cancellationTokenSource 초기화 추가
      waitStartCancellationTokenSource?.Cancel();
      waitStartCancellationTokenSource = null;
      CancelPrepareTimer();
    }

    // lds - 23.6.7 ㅊ가
    protected virtual void OnApplicationQuit()
    {
      isApplicationQuit = true;
      waitStartCancellationTokenSource?.Cancel();
      waitStartCancellationTokenSource = null;
      CancelPrepareTimer();
    }

    public void InitAnchor()
    {
      if (bInitAnchor)
        return;
      rectTransform.anchorMin = Vector2.zero;
      rectTransform.anchorMax = Vector2.one;
      rectTransform.anchoredPosition = Vector2.zero;
      rectTransform.sizeDelta = Vector2.zero;
      bInitAnchor = true;
    }

    private void InitAnimation()
    {
      if (bInitAnimation)
        return;

      if (Sequencer == null)
        Sequencer = GetComponent<DOTweenSequencer>();

      bInitAnimation = true;
    }

    public virtual void Initialize() { }

    /// <summary>
    /// UI 초기화
    /// <para> - 씬 이동 전에 호출된다.</para>
    /// </summary>

    /// <summary>
    /// <para>- 호출 순서 OnEnable -> Prepare -> Prepared -> SetUI -> Show</para>
    /// </summary>
    public virtual void Prepare(object obj = null)
    {
      gameObject.SetActive(true);

      InitAnimation();

      if (canvasGroup != null)
      {
        canvasGroup.alpha = 0;
      }

      if (hideDefaultSequence?.IsPlaying() == true)
      {
        hideDefaultSequence.Pause();
      }
      else if(Sequencer?.IsPlayingForID(useHideAnimationID) == true)
      {
        Sequencer.Stop(useHideAnimationID);
      }

      onHideCompleteAction = null; // 닫히는 도중 재활용되는 경우를 위해 초기화
      PrepareCancelTokenSource = new(); // lds - 22.9.20, 캔슬 대기 취소 토큰 소스 생성
    }

    public virtual void Prepared()
    {
      CancelPrepareTimer();

      preparedCallback?.Invoke(true); // lds - 23.2.15, 준비가 완료되면 콜벡 호출
      preparedCallback = null;
    }

    public void CancelPrepareTimer()
    {
      PrepareCancelTokenSource?.Cancel(); // lds - 22.9.20, 준비가 완료되면 캔슬 대기 취소.
      PrepareCancelTokenSource = null;
    }

    /// <summary>
    /// UI Show 함수 호출 시 호출된다. <br/>
    /// UIManager상에서 호출할 때는 bUpdateUI파라미터가 false이므로 SetUI가 호출됨.<br/>
    /// <para>- 호출 순서 Show -> OnEnable -> SetUI</para>
    /// </summary>
    public virtual void SetUI(object obj = null)
    {
    }

    /// <summary>
    /// 사용자가 UI를 Show할 때 bUpdateUI를 직접적으로 true를 지정하는 경우에 UpdateUI가 호출됨. <br/>
    /// </summary>
    /// <param name="param"></param>
    public virtual void UpdateUI(object param = null)
    {
    }


    /// <summary>
    /// 뒤로가기 버튼 (PC: ESC, Android : Back) 을 눌렀을 떄 호출
    /// </summary>
    public virtual void OnButtonEscape()
    {
      UIManager.Instance.HideUI(true);
    }
    public virtual void OnButtonClose()
    {
      UIManager.Instance.HideUI();
    }
    public void OnButtonShowUI(string name)
    {
      var type = Type.GetType(name);
      if (type == null)
        return;

      UIManager.Instance.ShowUI(type);
    }

    public static Sequence CreateDefaultShowAnimation(CanvasGroup canvasGroup)
    {
      var sequence = DOTween.Sequence();
      var fade = canvasGroup.DOFade(1, UIManager.TWEEN_UI_SHOW * 0.9f).SetEase(Ease.Linear);
      fade.ChangeStartValue(0f);
      sequence.Append(fade);
      sequence.SetAutoKill(false);
      sequence.Pause();

      return sequence;
    }
    protected virtual void DefaultShowAnimation()
    {
      if (showDefaultSequence == null)
      {
        showDefaultSequence = DOTween.Sequence();
        var fade = canvasGroup.DOFade(1, UIManager.TWEEN_UI_SHOW * 0.9f).SetEase(Ease.Linear);
        fade.ChangeStartValue(0f);
        showDefaultSequence.Append(fade);
        showDefaultSequence.OnComplete(CompleteShow);
        showDefaultSequence.SetAutoKill(false);
        showDefaultSequence.Pause();
      }

      showDefaultSequence.OnComplete(CompleteShow);
      showDefaultSequence.Restart();
    }
    public static Sequence CreateDefaultHideAnimation(CanvasGroup canvasGroup)
    {
      var sequence = DOTween.Sequence();
      var fade = canvasGroup.DOFade(0, UIManager.TWEEN_UI_HIDE * 0.9f).SetEase(Ease.Linear);
      fade.ChangeStartValue(1f);
      sequence.Append(fade);
      sequence.SetAutoKill(false);
      sequence.Pause();

      return sequence;
    }
    protected virtual void DefaultHideAnimation()
    {
      if (hideDefaultSequence == null)
      {
        hideDefaultSequence = DOTween.Sequence();
        var fade = canvasGroup.DOFade(0, UIManager.TWEEN_UI_HIDE * 0.9f).SetEase(Ease.Linear);
        fade.ChangeStartValue(1f);
        hideDefaultSequence.Append(fade);
        hideDefaultSequence.SetAutoKill(false);
        hideDefaultSequence.Pause();
      }

      hideDefaultSequence.OnComplete(CompleteHide);
      hideDefaultSequence.Restart();
    }

    protected virtual void ShowAnimation()
    {
      var seqs = Sequencer.Play(useShowAnimationID);
      seqs?.Events.OnComplete(CompleteShow);
    }

    protected virtual void HideAnimation()
    {
      var seqs = Sequencer.Play(useHideAnimationID);
      seqs?.Events.OnComplete(CompleteHide);
    }

    protected virtual void CompleteShow()
    {
      if (canvasGroup != null)
      {
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
      }

      onShowComplete?.Invoke();
      onShowCompleteAction = null;

      IsPlayingAnimation = false;
    }

    protected virtual void CompleteHide()
    {
      // UI 종료 시 UIPool 로 이동이 안됬으면 이동시켜준다.
      if (null != Attribute)
      {
        UIManager.Instance.PushUIPool(this);
      }

      gameObject.SetActive(IsPlayingAnimation = false);

      onHideComplete?.Invoke();
      onHideCompleteAction = null;
    }

    protected virtual void PlayShowSound()
    {
      if (!PlayShowAndHideSound || isPlayedShowSound)
        return;

      SoundManager.Instance.PlayFX(ShowSound);
      isPlayedShowSound = true;
    }

    protected virtual void PlayHideSound()
    {
      if (!PlayShowAndHideSound || isPlayedHideSound)
        return;

      SoundManager.Instance.PlayFX(HideSound, UIManager.TWEEN_UI_HIDE * 0.5f);
      isPlayedHideSound = true;
    }

    /// <summary>
    ///
    /// <para>- 호출 순서 Show -> SetUI</para>
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="bUpdate"></param>
    public virtual void Show(object obj = null, bool bUpdate = false, Action complete = null)
    {
      InitAnimation();

      if (bUpdate)
      {
        UpdateUI(obj);
      }
      else
      {
        SetUI(obj);
      }

      if (canvasGroup != null)
      {
        canvasGroup.alpha = 1;
      }

      if (complete != null)
      {
        onShowCompleteAction += complete;
      }

      if (UseShowAnimation)
      {
        if (UseShowDefaultAnimation)
        {
          DefaultShowAnimation();
        }
        else
        {
          ShowAnimation();
        }

        IsPlayingAnimation = true;
      }
      else
      {
        CompleteShow();
      }

      PlayShowSound();
    }

    /// <summary>
    /// UI를 닫는다.
    /// </summary>
    /// <param name="callback"></param>
    public virtual void Hide(Action complete = null, bool check = true)
    {
      //Debug.Log("Hide : " + gameObject.name);
      if (check && UIManager.Instance.CurrUI == this)
      {
        UIManager.Instance.HideUI(true);
      }
      else
      {
        if (canvasGroup != null)
        {
          canvasGroup.blocksRaycasts = false;
        }

        if (complete != null)
        {
          onHideCompleteAction += complete;
        }

        if (UseHideAnimation && gameObject.activeSelf)
        {
          if (UseHideDefaultAnimation)
          {
            DefaultHideAnimation();
          }
          else
          {
            HideAnimation();
          }

          IsPlayingAnimation = true;
        }
        else
        {
          CompleteHide();
        }

        PlayHideSound();
      }
    }

    public virtual void OnAfterPushToPool()
    {

    }
  }
}