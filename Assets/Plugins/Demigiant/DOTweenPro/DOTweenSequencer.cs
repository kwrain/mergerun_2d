using System;
using System.Collections.Generic;
using DG.Tweening.Custom.Sequencer;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;

namespace DG.Tweening.Custom.Sequencer
{
  [Serializable]
  public class DOTweenSeqs
  {
    private string id;

    private List<DOTweenSequence> sequences = new();
    private Events events = new();

    private DOTweenSequence completeSequence;
    private bool assignComplete;
    private float completeFullDuration;

    public DOTweenSequence this[int i]
    {
      get => sequences[i];
      set => sequences[i] = value;
    }

    public int Count => sequences.Count;

    public Events Events => events;

    public DOTweenSeqs(string id)
    {
      this.id = id;
    }

    public void Add(DOTweenSequence sequence)
    {
      if (sequences.Contains(sequence))
        return;

      if (assignComplete)
      {

      }
      else
      {
        // 우선순위
        if (sequence.Complete) // 지정된 컴플리트 시퀀스가 있으면 아래 경우를 체크하지 않음.
        {
          completeSequence = sequence;
          assignComplete = true;
        }
        else //  시퀀스가 무한루프거나 시퀀스가 가진 트윈이 무한 루프인 경우에는 체크하지 말아야함.
        {
          if (sequence.Duration == 0)
          {
            sequence.CheckDuration();
          }

          if (completeFullDuration < sequence.Delay + sequence.Duration)
          {
            completeSequence = sequence;
            completeFullDuration = sequence.Delay + sequence.Duration;
          }
        }
      }

      sequences.Add(sequence);
    }

    public void Remove(DOTweenSequence sequence)
    {
      sequences.Remove(sequence);
      if (sequences.Count != 0) return;
      assignComplete = false;
      completeSequence = null;
      completeFullDuration = 0;
    }

    public void Play()
    {
      // lds - 23.1.19, DOTweenSeqs의 이벤트, DOTweenSequence의 이벤트는 하단의 sequence.Play() 내에서 확인.
      // lds - 23.3.18, onPlay 호출 우선 순위 변경
      events.onPlay?.Invoke();

      foreach (var sequence in sequences)
      {
        sequence.Play();
      }

      if(completeSequence == null)
      {
        if (completeFullDuration > 0)
        {
          Debug.LogError($"completeSequence가 지정되지 않았습니다. {nameof(Add)} 메서드에 문제가 없는지 확인 해주세요.");
        }
      }
      else
      {
        completeSequence.OnComplete(() => { events.onComplete?.Invoke(); });
      }
    }

    public void Stop()
    {
      foreach (var sequence in sequences)
      {
        sequence.Stop();
      }
    }
  }

  // [Serializable]
  [Serializable]
  public class Events
  {
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

    public Events()
    {
      onStart ??= new UnityEvent();
      onStart.AddListener(() => { onStartAction?.Invoke(); });
      onPlay ??= new UnityEvent();
      onPlay.AddListener(() => { onPlayAction?.Invoke(); });
      onUpdate ??= new UnityEvent();
      onUpdate.AddListener(() => { onUpdateAction?.Invoke(); });
      onComplete ??= new UnityEvent();
      onComplete.AddListener(() => { onCompleteAction?.Invoke(); });
      onStepComplete ??= new UnityEvent();
      onStepComplete.AddListener(() => { onStepCompleteAction?.Invoke(); });
      onRewind ??= new UnityEvent();
      onRewind.AddListener(() => { onRewindAction?.Invoke(); });
      onAppendCallback ??= new UnityEvent();
      onAppendCallback.AddListener(() => { onAppendCallbackAction?.Invoke(); });
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
  }
}

namespace DG.Tweening
{
  [DisallowMultipleComponent]
  public class DOTweenSequencer : MonoBehaviour
  {
    [SerializeField] private List<DOTweenSequence> sequencesForEditor = new();
    private Dictionary<string, DOTweenSeqs> sequences = new();
    private Dictionary<string, DOTweenSeqs> playingSequences = new();

    public List<DOTweenSequence> SequencesForEditor => sequencesForEditor;
    public Dictionary<string, DOTweenSeqs> Sequences => sequences;

    public DOTweenSeqs this[string id] => sequences.ContainsKey(id) ? sequences[id] : null;

    public bool IsPlaying => playingSequences.Count > 0;

    private void Awake()
    {
      foreach (var sequence in sequencesForEditor)
      {
        AddSequence(sequence);
      }
    }

    private void OnDisable()
    {
      // 추후 인스펙터에 옵션으로 추가 예정
      StopAll();
    }

    private void Update()
    {
      foreach (var kv in playingSequences)
      {
        kv.Value.Events.onUpdate?.Invoke();
      }
    }
    private void AddSequence(DOTweenSequence sequence, bool invokeStart = true)
    {
      if (!sequences.ContainsKey(sequence.ID))
      {
        sequences[sequence.ID] = new DOTweenSeqs(sequence.ID);
        sequences[sequence.ID].Events.onComplete.AddListener(() =>
        {
          playingSequences.Remove(sequence.ID);
        });
      }

      sequences[sequence.ID].Add(sequence);
    }

    public bool IsPlayingForID(string id) => playingSequences.ContainsKey(id);

    public DOTweenSeqs Play(string id, bool alwaysRestart = false, bool autoPlay = true)
    {
      if (!sequences.ContainsKey(id) || (alwaysRestart == false && playingSequences.ContainsKey(id)))
        return null;

      playingSequences[id] = sequences[id];
      // lds - 23.3.18, 추가한 이유 : 이전에 방식으로는 playingSequences의 onPlay가 호출되지 않았음.
      // autoPlay를 false로 두고 Events.OnPlay()로 콜벡을 추가 후 해당 시퀀스를 직접 Play 해줘야 Events의 onPlay 이벤트가 먼저 호출된다.
      if(autoPlay == true)
      {
        playingSequences[id].Play();
      }
      return playingSequences[id];
    }

    public void Stop(string id, bool invokeComplete = false)
    {
      if (!IsPlaying || !playingSequences.ContainsKey(id))
        return;

      playingSequences[id].Stop();
      if (invokeComplete)
      {
        playingSequences[id].Events.onComplete?.Invoke();
      }
    }

    public void StopAll(bool invokeComplete = false)
    {
      if (!IsPlaying)
        return;

      foreach (var kv in playingSequences)
      {
        Stop(kv.Key, invokeComplete);
      }
      playingSequences.Clear();
    }

    #if UNITY_EDITOR
    public void FindAllSequences()
    {
      var sequences = GetComponentsInChildren<DOTweenSequence>(true);
      sequencesForEditor.Clear();
      foreach(var v in sequences)
      {
        if(string.IsNullOrEmpty(v.ID))
          continue;
        sequencesForEditor.Add(v);
      }
    }
    #endif
  }
}