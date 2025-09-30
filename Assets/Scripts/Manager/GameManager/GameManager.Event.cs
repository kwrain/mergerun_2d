/**
* GameManager.Event.cs
* 작성자 : lds3794@gmail.com
* 작성일 : 2022년 06월 22일 오후 9시 47분
*/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public partial class GameManager
{
  private CancellationTokenSource eventCancellationTokenSource;

  private List<EventHandlerBase> eventHandlers;

  #region 이벤트 핸들러 테스크

  private void RunEventTask()
  {
    if(eventCancellationTokenSource != null) return;

    CreateEventHandlers();
    eventCancellationTokenSource = new();
    UpdateEventTask();
  }

  /// <summary>
  /// 씬 변경 또는 어플리케이션 종료 시 기존 테스크 해제 및 삭제
  /// </summary>
  private void StopEventTask()
  {
    eventCancellationTokenSource?.Cancel();
    if (eventHandlers != null)
    {
      for (int i = 0; i < eventHandlers.Count; i++)
        eventHandlers[i].ReleaseEventHandler();
    }
    eventHandlers = null;
    eventCancellationTokenSource = null;
  }

  /// <summary>
  /// 이벤트 헨들러 리셋 <br/>
  /// 섬 진입시 리셋이 필요한 부분을 리셋함.
  /// </summary>
  public void ResetEventHandlers()
  {
    if (eventHandlers == null) return;
    for (int i = 0; i < eventHandlers.Count; i++)
      eventHandlers[i].ResetEventHandler();
  }

  private async void UpdateEventTask()
  {
    while(true)
    {
      if (eventCancellationTokenSource == null || eventCancellationTokenSource.IsCancellationRequested == true)
        break;
      if(eventHandlers != null)
      {
        for (int i = 0; i < eventHandlers.Count; i++)
          eventHandlers[i].Update();
      }
      await Task.Yield();
    }
  }

  public bool HasEventHandler(EventHandlerBase eventHandler)
  {
    if (eventHandlers == null) return false;
    return eventHandlers.Contains(eventHandler);
  }

  public void AddEventHandler(EventHandlerBase eventHandler)
  {
    if(HasEventHandler(eventHandler) == true) return;
    eventHandlers.Add(eventHandler);
  }

  public void RemoveEventHandler(EventHandlerBase eventHandler)
  {
    if(HasEventHandler(eventHandler) == false) return;
    eventHandlers.Remove(eventHandler);
  }
  #endregion

  #region 이벤트 핸들러

  /// <summary>
  /// 이벤트 핸들러 생성
  /// </summary>
  private void CreateEventHandlers()
  {
    eventHandlers = new();

    // 슬래시 키 입력시 메시지 출력하는 이벤트 헨들러 ( 테스트 용)
    // EventHandlerBase printSlashEventHandler = new("슬래쉬 키 입력");
    // printSlashEventHandler.AddEventTrigger(() => Input.GetKeyDown(KeyCode.Slash));
    // printSlashEventHandler.AddEventCallback(() => Debug.Log("슬래쉬 눌림"));
    // eventHandlers.Add(printSlashEventHandler);
  }

  public abstract class EventHandlerBase
  {
    protected string eventHandlerName;
    protected Func<Task<bool>> eventTrigger; // 이벤트 트리거
    protected System.Action resetCallback; // 리셋 콜백
    protected System.Action releaseCallback; // 해제 콜백

    public string Name => eventHandlerName;

    public EventHandlerBase(string eventHandlerName = null)
    {
      this.eventHandlerName = eventHandlerName;
    }
    public abstract void Update();
    public void AddEventTrigger(Func<Task<bool>> eventTrigger) => this.eventTrigger += eventTrigger;
    public void AddResetCallback(System.Action resetCallback) => this.resetCallback += resetCallback;
    public void AddReleaseCallback(System.Action releaseCallback) => this.resetCallback += releaseCallback;
    public void RemoveEventTrigger(Func<Task<bool>> eventTrigger) => this.eventTrigger -= eventTrigger;
    public void RemoveResetCallback(System.Action resetCallback) => this.resetCallback -= resetCallback;
    public void RemoveReleaseCallback(System.Action releaseCallback) => this.resetCallback -= releaseCallback;
    public void ClearEventTrigger() => this.eventTrigger = null;
    public void ClearResetCallback() => this.resetCallback = null;
    public void ClearReleaseCallback() => this.releaseCallback = null;
    public void ResetEventHandler() => ResetEventTriggerImpl();
    public void ReleaseEventHandler() => ReleaseEventTriggerImpl();
    protected virtual void ResetEventTriggerImpl()
    {
      resetCallback?.Invoke();
    }
    protected virtual void ReleaseEventTriggerImpl()
    {
      releaseCallback?.Invoke();
    }

    public abstract Task ForceInvokeEventCallback();
  }

  public class EventHandler : EventHandlerBase
  {
    protected System.Func<Task> eventCallback; // 이벤트 콜백

    public EventHandler(string eventHandlerName = null) : base(eventHandlerName) { }

    public override async void Update()
    {
      if(eventTrigger == null) return;
      if(await eventTrigger() == false) return;
      // 이벤트 트리거 발동 시 이벤트 콜백 실행
#if UNITY_EDITOR && DEBUG_EVENT_HANDLER
      Debug.Log($"{this.eventHandlerName} : 이벤트 콜백 실행!");
#endif
      await eventCallback?.Invoke();
    }

    public void AddEventCallback(System.Func<Task> eventCallback) => this.eventCallback += eventCallback;
    public void RemoveEventCallback(System.Func<Task> eventCallback) => this.eventCallback -= eventCallback;
    public void ClearEventCallback() => this.eventCallback = null;

    public override async Task ForceInvokeEventCallback()
    {
      await eventCallback?.Invoke();
    }
  }

  public class DelayEventHandler : EventHandler
  {
    protected float t = 0f;
    protected float delay = 0f;
#if UNITY_EDITOR && DEBUG_EVENT_HANDLER
    protected float debugElapsedTime;
#endif
    public enum ETimeType
    {
      None = -1,
      Scaled = 0,
      Unscaled,
      Fixed,
    }

    /// <summary>
    /// 일정한 시간 마다 갱신
    /// </summary>
    /// <param name="eventHandlerName"></param>
    /// <param name="delay"></param>
    /// <param name="timeType"></param>
    /// <returns></returns>
    public DelayEventHandler(string eventHandlerName, float delay, ETimeType timeType) : base(eventHandlerName)
    {
      this.delay = delay;
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
      eventTrigger = async () =>
      {
        float deltaTime  = GetDeltaTime(timeType);
#if UNITY_EDITOR && DEBUG_EVENT_HANDLER
        PrintRemainTime(deltaTime);
#endif
        bool isDelayed = t >= this.delay;
        t = isDelayed == true ? 0 : t;
        t += deltaTime;
        return isDelayed;
      };
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }

    /// <summary>
    /// 특정 시간 마다 갱신 후, 딜레이값 재설정
    /// </summary>
    /// <param name="eventHandlerName"></param>
    /// <param name="delayCallback"></param>
    /// <param name="timeType"></param>
    /// <returns></returns>
    public DelayEventHandler(string eventHandlerName, Func<float> delayCallback, ETimeType timeType) : base(eventHandlerName)
    {
      if (delayCallback == null) { Debug.LogWarning($"{nameof(delayCallback)}을 반드시 설정해주세요."); }
      this.delay = delayCallback();
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
      eventTrigger = async () =>
      {
        float deltaTime = GetDeltaTime(timeType);
#if UNITY_EDITOR && DEBUG_EVENT_HANDLER
        PrintRemainTime(deltaTime);
#endif
        bool isDelayed = t >= this.delay;
        t = isDelayed == true ? 0 : t;
        t += deltaTime;
        if(isDelayed == true) this.delay = delayCallback(); // 딜레이 값 변경
        return isDelayed;
      };
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }

#if UNITY_EDITOR && DEBUG_EVENT_HANDLER
    private void PrintRemainTime(float deltaTime)
    {
      if (this.delay >= 1f)
      {
        if(debugElapsedTime >= 1f)
        {
          int intPart = (int)debugElapsedTime;
          debugElapsedTime -= intPart;
          Debug.Log($"{this.eventHandlerName} : 트리거가 발동하기 까지 남은 시간 ... {this.delay - t:F1}초 / {this.delay:F2}초");
        }
        debugElapsedTime += deltaTime;
      }
      else if(this.delay < 1f)
        Debug.Log($"{this.eventHandlerName} : 트리거가 발동하기 까지 남은 시간 ... {this.delay - t:F2}초 / {this.delay:F2}초");
    }
#endif

    public float GetDeltaTime(ETimeType timeType) => timeType switch
    {
      ETimeType.Scaled => Time.deltaTime,
      ETimeType.Unscaled => Time.unscaledDeltaTime,
      ETimeType.Fixed => Time.fixedDeltaTime,
      _ => Time.deltaTime,
    };

    protected override void ResetEventTriggerImpl()
    {
      base.ResetEventTriggerImpl();
      ResetTimer();
    }

    public void ResetTimer()
    {
      t = 0f;
#if UNITY_EDITOR && DEBUG_EVENT_HANDLER
      debugElapsedTime = t;
#endif
    }
  }

  /// <summary>
  /// UnscaledDeltaTime
  /// </summary>
  public class ServerTimeSyncEventHandler : EventHandlerBase
  {
    public delegate Task EventCallbackDelegate(int seconds);
    protected EventCallbackDelegate eventCallback;
    private System.Func<Task> refreshCallback;
    protected float elapsedTime = 0f;
    protected float delay = 0f;
    protected int dSeconds = 0;
    protected bool isAwaiting = false;

    public ServerTimeSyncEventHandler(string eventHandlerName, float delay = 1f, float refreshTime = 9999f, System.Func<Task> refreshCallback = null) : base(eventHandlerName)
    {
      this.delay = delay;
      this.refreshCallback = refreshCallback;
      eventTrigger = async () =>
      {
        if(Time.unscaledDeltaTime > refreshTime)
        {
          ResetTimer();
          await this.refreshCallback?.Invoke();
          return false;
        }
        if(elapsedTime >= this.delay)
        {
          dSeconds = (int)elapsedTime;
          elapsedTime -= dSeconds;
          elapsedTime += Time.unscaledDeltaTime;
          return true;
        }
        elapsedTime += Time.unscaledDeltaTime;
        return false;
      };
    }

    public override async void Update()
    {
      if(eventTrigger == null) return;
      // 테스크 내에서 대기 상태가 있는 경우 대기한 시간만큼 더한 후 return;
      if(isAwaiting == true)
      {
        elapsedTime += Time.unscaledDeltaTime;
        return;
      }
      if(await eventTrigger() == false) return;
      isAwaiting = true;
      await eventCallback?.Invoke(dSeconds);
      isAwaiting = false;
    }
    public void AddEventCallback(EventCallbackDelegate eventCallback) => this.eventCallback += eventCallback;
    public void RemoveEventCallback(EventCallbackDelegate eventCallback) => this.eventCallback -= eventCallback;
    public void ClearEventCallback() => this.eventCallback = null;

    public async Task InvokeRefreshCallback()
    {
      ResetTimer();
      await this.refreshCallback?.Invoke();
    }

    public override async Task ForceInvokeEventCallback()
    {
      await eventCallback?.Invoke(dSeconds);
    }

    public void ResetTimer()
    {
      elapsedTime = 0f;
      dSeconds = 0;
    }
  }
  #endregion
}