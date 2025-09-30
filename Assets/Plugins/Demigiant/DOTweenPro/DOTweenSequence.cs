using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace DG.Tweening
{
  public class DOTweenSequence : MonoBehaviour
  {
    [Serializable]
    public class DOTweenAnimationData
    {
      public enum AddType
      {
        Append,
        Join,
        Insert,
        Prepend
      }

      public DOTweenSequenceAnimation animation;

      public AddType addType = AddType.Insert;
      public float stay;
      public UnityEvent appendCallback;

      public float insertTime;
      public UnityEvent insertCallback;

      public float Duration => animation.delay + animation.duration * (animation.loops == 0 ? 1 : animation.loops) + stay;

      public DOTweenAnimationData()
      {
      }

      public DOTweenAnimationData(DOTweenSequenceAnimation animation)
      {
        this.animation = animation;
      }
    }

    private Sequence sequence;

    [SerializeField] private bool autoGenerate = true;
    [SerializeField] private bool autoPlay = true;
    [SerializeField] private bool autoKill = true;
    
    [SerializeField] private float delay;
    [SerializeField] private float stay;
    [SerializeField] private int loops;
    [SerializeField] private LoopType loopType;
    private int loopCount;

    [SerializeField] private string id;
    [SerializeField] private float duration;

    public UnityEvent onStart;
    public UnityEvent onPlay;
    public UnityEvent onUpdate;
    public UnityEvent onStepComplete;
    public UnityEvent onComplete;
    public UnityEvent onRewind;
    public UnityEvent onAppendCallback;
    
    private Action onStartAction;
    private Action onPlayAction;
    private Action onUpdateAction;
    private Action onStepCompleteAction;
    private Action onCompleteAction;
    private Action onRewindAction;
    private Action onAppendCallbackAction;
    
    [SerializeField] private bool hasOnStart;
    [SerializeField] private bool hasOnPlay;
    [SerializeField] private bool hasOnUpdate;
    [SerializeField] private bool hasOnStepComplete;
    [SerializeField] private bool hasOnComplete;
    [SerializeField] private bool hasOnRewind;
    [SerializeField] private bool hasOnAppendCallback;
    
    [SerializeField] private bool foldoutAnimations;

    public CancellationTokenSource timerCancelTokenSource;

    [SerializeField] private List<DOTweenAnimationData> tweenAnimationDatas;
    public List<DOTweenAnimationData> TweenAnimationDatas => tweenAnimationDatas;

    public string ID => id;
    public bool IsInitialized => sequence != null;
    public bool IsPlaying { get; private set; }

    public float Delay => delay;
    public float Duration => duration;

    public bool Complete { get; set; }

    private void OnEnable()
    {
      if (autoPlay)
      {
        Play();
      }
    }

    private void OnDisable()
    {
      StopTimer();
    }

    public void InitializeSequence()
    {
      if (sequence != null)
        return;
      
      sequence = DOTween.Sequence();
      sequence.OnStart(() => { onStart?.Invoke(); });
      sequence.OnPlay(() => { onPlay?.Invoke(); });
      sequence.OnUpdate(() => { onUpdate?.Invoke(); });
      sequence.OnStepComplete(() => { onStepComplete?.Invoke(); });
      sequence.OnComplete(() => { onComplete?.Invoke(); });
      sequence.OnRewind(() => { onRewind?.Invoke(); });
      if (delay > 0) sequence.SetDelay(delay);
      sequence.SetLoops(loops, loopType);
      sequence.SetAutoKill(autoKill);
      sequence.Pause();
      
      onStart.AddListener(() => { onStartAction?.Invoke(); });
      onPlay.AddListener(() => { onPlayAction?.Invoke(); });
      onUpdate.AddListener(() => { onUpdateAction?.Invoke(); });
      onComplete.AddListener(() => { onCompleteAction?.Invoke(); });
      onStepComplete.AddListener(() => { onStepCompleteAction?.Invoke(); });
      onRewind.AddListener(() => { onRewindAction?.Invoke(); });
      onAppendCallback.AddListener(() => { onAppendCallbackAction?.Invoke(); });

      var duration = 0f;
      var tweenStartPosition = 0f;
      for (var i = 0; i < tweenAnimationDatas.Count; i++)
      {
        var data = tweenAnimationDatas[i];
        if (data.animation == null) continue;
        var tween = data.animation.CreateTweenInstance();
        data.animation.startValueApplyAction?.Invoke();
        
        switch (data.addType)
        {
          case DOTweenAnimationData.AddType.Append:
            sequence.Append(tween);
            sequence.AppendInterval(data.stay);
            sequence.AppendCallback(() => { data.appendCallback?.Invoke(); });
            break;

          case DOTweenAnimationData.AddType.Prepend:
            sequence.Prepend(tween);
            // sequence.PrependInterval()
            // sequence.PrependCallback()

            tweenStartPosition = 0f;
            break;

          case DOTweenAnimationData.AddType.Join:
            sequence.Join(tween);
            break;

          case DOTweenAnimationData.AddType.Insert:
            sequence.Insert(data.insertTime, tween);
            sequence.InsertCallback(data.insertTime, () => { data.insertCallback?.Invoke(); });

            tweenStartPosition = data.insertTime;
            break;
        }

        if (duration < tweenStartPosition + data.Duration)
          duration = tweenStartPosition + data.Duration;

        tweenStartPosition += data.animation.delay;
        if (i + 1 < tweenAnimationDatas.Count)
        {
          var nextData = tweenAnimationDatas[i + 1];
          if (nextData.addType != DOTweenAnimationData.AddType.Join)
          {
            tweenStartPosition += data.animation.duration + data.stay;
          }
        }
      }

      this.duration = duration;

      if (stay > 0)
      {
        sequence.AppendInterval(stay);
        sequence.AppendCallback(() => { onAppendCallback?.Invoke(); });
      }
    }

    public void PlayInEditor(bool checkLoop = true)
    {
      var duration = 0f;
      var tweenStartPosition = 0f;
      for (var i = 0; i < tweenAnimationDatas.Count; i++)
      {
        var data = tweenAnimationDatas[i];
        var t = data.animation.CreateEditorPreview();
        t.SetUpdate(UpdateType.Manual);
        t.SetAutoKill(false);
        t.Pause();

        switch (data.addType)
        {
          case DOTweenAnimationData.AddType.Append:
            break;

          case DOTweenAnimationData.AddType.Prepend:
            tweenStartPosition = 0f;
          break;

          case DOTweenAnimationData.AddType.Join:
            break;

          case DOTweenAnimationData.AddType.Insert:
            tweenStartPosition = data.insertTime;
            break;
        }

        if (duration < tweenStartPosition + data.Duration)
          duration = tweenStartPosition + data.Duration;

        tweenStartPosition += data.animation.delay;
        t.SetDelay(tweenStartPosition + delay);
        // Debug.Log($"{data.animation.gameObject.name} : {tweenStartPosition}");

        if (i + 1 < tweenAnimationDatas.Count)
        {
          var nextData = tweenAnimationDatas[i + 1];
          if (nextData.addType != DOTweenAnimationData.AddType.Join)
          {
            tweenStartPosition += data.animation.duration + data.stay;
          }
        }
        t.Play();
      }
      
      Timer(duration, () =>
      {
        if (loopCount-- > 0)
        {
          PlayInEditor(false);
        }
      });

      if (!checkLoop) return;
      if (loops is 0 or 1) return;
      loopCount = loops == -1 ? int.MaxValue : loops;
    }

    private async Task Timer(float time, Action callback)
    {
      try
      {
        timerCancelTokenSource = new CancellationTokenSource();
        await Task.Delay((int)(time * 1000), timerCancelTokenSource.Token);

        callback?.Invoke();
      }
      catch (Exception e)
      {
      }
    }

    public void StopTimer()
    {
      timerCancelTokenSource?.Cancel();
      timerCancelTokenSource = null;
    }

    public void Play()
    {
      Stop();
      
      if (sequence == null)
      {
        InitializeSequence();
      }

      IsPlaying = true;
      sequence.Restart();
    }

    public void Stop()
    {
      if (!IsPlaying)
        return;

      IsPlaying = false;
      sequence.Pause();
      sequence.Rewind();
    }

    public void CheckDuration()
    {
      if(tweenAnimationDatas == null) return;
      var duration = 0f;
      var tweenStartPosition = 0f;
      for (var i = 0; i < tweenAnimationDatas.Count; i++)
      {
        var data = tweenAnimationDatas[i];
        if(data.animation == null) continue;
        
        switch (data.addType)
        {
          case DOTweenAnimationData.AddType.Append:
            break;

          case DOTweenAnimationData.AddType.Prepend:
            tweenStartPosition = 0f;
            break;

          case DOTweenAnimationData.AddType.Join:
            break;

          case DOTweenAnimationData.AddType.Insert:
            tweenStartPosition = data.insertTime;
            break;
        }

        if (duration < tweenStartPosition + data.Duration)
          duration = tweenStartPosition + data.Duration;

        tweenStartPosition += data.animation.delay;
        if (i + 1 < tweenAnimationDatas.Count)
        {
          var nextData = tweenAnimationDatas[i + 1];
          if (nextData.addType != DOTweenAnimationData.AddType.Join)
          {
            tweenStartPosition += data.animation.duration + data.stay;
          }  
        }
      }

      var l = loops;
      if(l <= -1) l = -1; // loops가 음수인 경우 무한 루프.
      if(l == 0) l = 1; // loops가 0인경우 한번만 동작.
      this.duration = duration * (l == -1 ? int.MaxValue : l);
    }
    
    public void OnStart(Action action)
    {
      onStartAction = action;
    }

    public void OnPlay(Action action)
    {
      onPlayAction = action;
    }

    public void OnUpdate(Action action)
    {
      onUpdateAction = action;
    }

    public void OnComplete(Action action)
    {
      onCompleteAction = action;
    }

    public void OnStepComplete(Action action)
    {
      onStepCompleteAction = action;
    }

    public void OnRewind(Action action)
    {
      onRewindAction = action;
    }

    public void OnAppendCallback(Action action)
    {
      onAppendCallbackAction = action;
    }

#if UNITY_EDITOR
    public void SetTweenAnimationDatas(List<DOTweenAnimationData> tweenAnimationDatas) => this.tweenAnimationDatas = tweenAnimationDatas;
#endif
  }
}