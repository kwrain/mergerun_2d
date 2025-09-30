using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Base of tween
/// </summary>
namespace FAIRSTUDIOS.Tools
{
  public enum uTweenerTimeType
  {
    Unscaled,
    Scaled,
    Fixed
  }

  public abstract class KTweener : MonoBehaviour
  {
    bool stayEnable = false;
    bool bForceStart = false;

    float mAmountPerDelta = 1000f;
    float mAmountPerDeltaStay = 1000f;
    float mStay = 0f;
    float mDuration = 0f;
    float mStartTime = -1f;
    float mFactor = 0f;

    float mAmountPerDeltaFactor;

    // Loop and PingPong Option
    int nLoopCount;
    int nLoopDelta = 0;
    int nLoopCountLimit;

    [SerializeField]
    protected UnityEvent onFinished = null;

    protected bool enableTween = true;

    public AnimationCurve animationCurve = new AnimationCurve(new Keyframe(0f, 0f, 0f, 1f), new Keyframe(1f, 1f, 1f, 0f));
    public EaseType easeType = EaseType.none;
    public LoopStyle loopStyle = LoopStyle.Once;
    public float delay = 0f;
    public float duration = 1f;
    public float stay = 0f;
    public uTweenerTimeType timeType = uTweenerTimeType.Unscaled;

    /// <summary>
    /// Gets the amount per delta.
    /// </summary>
    /// <value>The amount per delta.</value>
    float AmountPerDelta
    {
      get
      {
        if (mDuration != duration)
        {
          mDuration = duration;
          mAmountPerDelta = duration > 0 ? 1f / duration : 1000f;
        }
        return mAmountPerDelta;
      }
    }
    float AmountPerDeltaStay
    {
      get
      {
        if (mStay != stay)
        {
          mStay = stay;
          mAmountPerDeltaStay = stay > 0 ? 1f / stay : 1000f;
        }
        return mAmountPerDeltaStay;
      }
    }

    float CurrentDelta
    {
      get
      {
        switch (timeType)
        {
          case uTweenerTimeType.Unscaled:
            return Time.unscaledDeltaTime;
          case uTweenerTimeType.Fixed:
            return Time.fixedDeltaTime;
          default:
            return Time.deltaTime;
        }
      }
    }
    float CurrentTime
    {
      get
      {
        switch (timeType)
        {
          case uTweenerTimeType.Unscaled:
            return Time.unscaledTime;
          case uTweenerTimeType.Fixed:
            return Time.fixedTime;
          default:
            return Time.time;
        }
      }
    }

    /// <summary>
    /// Gets or sets the factor.
    /// </summary>
    /// <value>The factor.</value>
    public float Factor
    {
      get { return mFactor; }
      set { mFactor = Mathf.Clamp01(value); }
    }

    /// <summary>
    /// 강제 시작
    /// - 현재 진행중인 Tween을 무시하고, 새로운 Tween을 시작할 때 사용.
    /// </summary>
    public bool ForceStart
    {
      get { return bForceStart; }
      set { bForceStart = value; }
    }

    public int LimitLoopCount
    {
      get { return nLoopCountLimit; }
      set
      {
        nLoopDelta = 0;
        nLoopCount = 0;
        nLoopCountLimit = value;
      }
    }

    public float StartTime
    {
      get { return mStartTime; }
    }

    // Use this for initialization
    void Start()
    {
      //Update();
     // LateUpdate();
    }

    // Update is called once per frame
    private void FixedUpdate()
    //{
      
    //}
    //void Update()
    {
      if (!enableTween)
        return;

      float delta = CurrentDelta;
      float time = CurrentTime;

      if (delta >= 1f)
        return;

      if (mStartTime < 0) mStartTime = time + delay;
      if (time < mStartTime) return;

      if (stayEnable)
      {
        mStay += AmountPerDeltaStay * delta;
        if (mStay > 1f)
        {
          mStay = 0;
          stayEnable = false;

          // lds - 22.7.5
          // if (loopStyle == LoopStyle.Once) 구문과
          // if (onFinished != null) 구문 위치 변경
          // loopStyle이 LoopStyle.Once이고 stay가 0보다 큰 경우
          // onFinished에서 다시 트윈을 플레이 시 즉시 정지되는 이슈가 있기 때문에 변경.
          if (loopStyle == LoopStyle.Once)
          {
            Sample(1, true);
            enabled = false;  //finished.set script enable
          }
          
          if (onFinished != null)
          {
            onFinished.Invoke();
          }
          
          if(loopStyle == LoopStyle.Once)
            return;
        }
        else
          return;
      }

      if (bForceStart)
      {
        mAmountPerDelta = 1000f;
        mAmountPerDeltaStay = 1000f;

        mDuration = 0f;
        mStartTime = time + delay;
        mFactor = 0f;
        mStay = 0;

        bForceStart = false;
      }
      else
      {
        mAmountPerDeltaFactor = AmountPerDelta * delta;
        mFactor += mAmountPerDeltaFactor;
      }

      if (loopStyle == LoopStyle.Loop)
      {
        if (mFactor > 1f)
        {
          mFactor -= Mathf.Floor(mFactor);

          if (delay > 0)
          {
            mStartTime = time + delay;
            Sample(0, false);
          }

          if (stay > 0)
          {
            stayEnable = true;
          }
          else
          {
            if (onFinished != null)
            {
              onFinished.Invoke();
            }
          }

          return;
        }
      }
      else if (loopStyle == LoopStyle.PingPong)
      {
        if (mFactor > 1f)
        {
          nLoopDelta++;
          mFactor = 1f - (mFactor - Mathf.Floor(mFactor));
          mAmountPerDelta = -mAmountPerDelta;
        }
        else if (mFactor < 0f)
        {
          nLoopDelta--;
          mFactor = -mFactor;
          mFactor -= Mathf.Floor(mFactor);
          mAmountPerDelta = -mAmountPerDelta;

          if (delay > 0)
          {
            mStartTime = time + delay;
            Sample(0, false);
          }

          if (nLoopDelta == 0)
            nLoopCount++;

          if (nLoopCountLimit > 0 && nLoopCountLimit <= nLoopCount)
          {
            nLoopCount = 0;
            enabled = false;
            if (null != onFinished)
            {
              onFinished.Invoke();
            }
          }
          else if (stay > 0)
          {
            stayEnable = true;
          }
          else if (null != onFinished)
          {
            onFinished.Invoke();
          }

          return;
        }
      }

      if ((loopStyle == LoopStyle.Once) && (duration == 0f || mFactor > 1f || mFactor < 0f))
      {
        if (stay > 0)
        {
          stayEnable = true;
          return;
        }

        Sample(1, true);
        enabled = false;  //finished.set script enable
        if (onFinished != null)
        {
          //Debug.Log("uTweener, [" + name + "]onFinished", gameObject);
          onFinished.Invoke();
        }

        return;
      }

      Sample(mFactor, false);
    }

    protected virtual void OnEnable() { }
    protected virtual void OnDisable() { bForceStart = true; }

    /// <summary>
    /// Raises the update event.
    /// </summary>
    /// <param name="_factor">_factor.</param>
    /// <param name="_isFinished">If set to <c>true</c> _is finished.</param>
    protected virtual void OnUpdate(float _factor, bool _isFinished) { }

    protected static T InitializeTween<T>(GameObject go) where T : KTweener
    {
      T comp = go.GetComponent<T>();
      if (comp == null)
      {
        comp = go.AddComponent<T>();
      }

      comp.Reset();
      comp.enabled = false;

      return comp;
    }

    /// <summary>
    /// Begin the specified _go and _duration.
    /// </summary>
    /// <param name="go">_go.</param>
    /// <param name="_duration">_duration.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public static T Begin<T>(GameObject go, float _duration) where T : KTweener
    {
      T comp = go.GetComponent<T>();
      if (comp == null)
      {
        comp = go.AddComponent<T>();
      }
      comp.Reset();
      comp.duration = _duration;
      comp.enabled = true;
      return comp;
    }

    [ContextMenu("Set 'From' to current value")]
    public virtual void SetStartToCurrentValue() { }

    [ContextMenu("Set 'To' to current value")]
    public virtual void SetEndToCurrentValue() { }

    [ContextMenu("Assume value of 'From'")]
    public virtual void SetCurrentValueToStart() { }

    [ContextMenu("Assume value of 'To'")]
    public virtual void SetCurrentValueToEnd() { }

    public void AddFinishedEvent(UnityAction callback)
    {
      if (null == onFinished)
        onFinished = new UnityEvent();

      onFinished.AddListener(callback);
    }

    public void ClearFinishedEvent()
    {
      if (onFinished != null)
        onFinished = null;
    }

    public void Play(PlayDirection dir)
    {
      mAmountPerDelta = (dir == PlayDirection.Reverse) ? -Mathf.Abs(AmountPerDelta) : Mathf.Abs(AmountPerDelta);
      enabled = true;
      //LateUpdate();
    }

    [ContextMenu("RePlay")]
    public void RePlay()
    {
      mAmountPerDelta = 1000f;
      mDuration = 0f;
      mStartTime = -1f;
      mFactor = 0f;
      enabled = true;

      //Debug.Log("uTweener, == RePlay, [" + name + "] 1 mFactor : " + mFactor + ", amountPerDelta : " + amountPerDelta);
    }
    /// <summary>
    /// Reset this instance.
    /// </summary>
    public void Reset()
    {
      enabled = true;
      easeType = EaseType.linear;
      loopStyle = LoopStyle.Once;
      delay = 0f;
      duration = 1f;
      //eventRecevier = null;
      onFinished = null;

      mAmountPerDelta = 1000f;
      mDuration = 0f;
      mStartTime = -1f;
      mFactor = 0f;
    }

    /// <summary>
    /// Sample the specified _factor and _isFinished.
    /// </summary>
    /// <param name="_factor">_factor.</param>
    /// <param name="_isFinished">If set to <c>true</c> _is finished.</param>
    public void Sample(float _factor, bool _isFinished)
    {
      float val = Mathf.Clamp01(_factor);
      val = (easeType == EaseType.none) ? animationCurve.Evaluate(val) : EaseManager.LerpEaseType(0, 1, val, easeType);
      OnUpdate(val, _isFinished);
    }
    public void SetTimeType(int nTimeType)
    {
      timeType = (uTweenerTimeType)nTimeType;
    }

    public void Toggle()
    {
      mAmountPerDelta *= -1;
      enabled = true;
    }
  }
}