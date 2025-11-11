using System;
using UnityEngine;
using UnityEngine.EventSystems;


/**
* GameManager.Event.cs
* 작성자 : lds3794@gmail.com
* 작성일 : 2022년 06월 22일 오후 9시 47분
*/
public partial class GameManager
{
  /// <summary>
  /// ApplicationPause
  /// </summary>
  private DateTime pauseStartTime;

  private bool IsApplicationPause { get; set; }

  public float CpuScore { get; private set; } = 1.0f;
  public float GpuScore { get; private set; } = 1.0f;
  public float RamScore { get; private set; } = 1.0f;

  public float SuspendTime { get; private set; }

  private void AutoSetting()
  {
    (
      int processorFrequency,
      int processorCount,
      int graphicsMemorySize,
      int graphicsShaderLevel,
      int maxTextureSize,
      int systemMemorySize
      ) = (2150, 4, 3417, 45, 16384, 3417);

    SetNeverSleepMode(); // lds - 25.2.3, 앱 시작 시에는 일단 절전 모드를 비활성화 함.

    //프레임 고정
    QualitySettings.vSyncCount = 0;

    Input.multiTouchEnabled = true;

    EventSystem.current.pixelDragThreshold = (int)(0.5f * Screen.dpi / 2.54f);

    void SetDeviceScore()
    {
#if UNITY_EDITOR
      CpuScore *= UnityEngine.Device.SystemInfo.processorFrequency / processorFrequency;
      CpuScore *= UnityEngine.Device.SystemInfo.processorCount / processorCount;

      GpuScore *= UnityEngine.Device.SystemInfo.graphicsMemorySize / graphicsMemorySize;
      GpuScore *= UnityEngine.Device.SystemInfo.graphicsShaderLevel / graphicsShaderLevel;
      GpuScore *= UnityEngine.Device.SystemInfo.maxTextureSize / maxTextureSize;

      RamScore *= UnityEngine.Device.SystemInfo.systemMemorySize / systemMemorySize;
#else
      CpuScore *= SystemInfo.processorFrequency / processorFrequency;
      CpuScore *= SystemInfo.processorCount / processorCount;

      GpuScore *= SystemInfo.graphicsMemorySize / graphicsMemorySize;
      GpuScore *= SystemInfo.graphicsShaderLevel / graphicsShaderLevel;
      GpuScore *= SystemInfo.maxTextureSize / maxTextureSize;

      RamScore *= SystemInfo.systemMemorySize / systemMemorySize;
#endif

      Debug.Log($"Device Score : CPU : {CpuScore} / GPU : {GpuScore} / RAM : {RamScore}");
    }
  }



  public void SetSystemSleepMode()
  {
    Screen.sleepTimeout = SleepTimeout.SystemSetting;
  }

  public void SetNeverSleepMode()
  {
    Screen.sleepTimeout = SleepTimeout.NeverSleep;
  }
}