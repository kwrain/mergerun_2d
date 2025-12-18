using UnityEngine;
using Unity.Services.Analytics;
using Unity.Services.Core;
using System.Collections.Generic;

/**
* GameManager.Analytics.cs
* Unity Analytics 표준 이벤트 관리 기능
* 게임 내 Unity Analytics 표준 이벤트를 전송합니다.
*/
public partial class GameManager
{
  #region Analytics

  /// <summary>
  /// 스테이지 시작 이벤트를 전송합니다.
  /// </summary>
  /// <param name="gameMode">게임 모드 ("stage" 또는 "endless")</param>
  /// <param name="stageId">스테이지 번호 (무한 모드면 0)</param>
  /// <param name="retryCount">해당 스테이지 재시도 횟수</param>
  public void AnalyticsStageStart(string gameMode, int stageId, int retryCount = 0)
  {
    // 에디터에서는 데이터 수집하지 않음
    if (Application.isEditor)
    {
      Debug.Log($"[Analytics] Stage Start (Editor - Skipped): Mode={gameMode}, StageID={stageId}, Retry={retryCount}");
      return;
    }

    try
    {
      var customEvent = new CustomEvent("stage_start");
      customEvent["game_mode"] = gameMode;
      customEvent["stage_id"] = stageId;
      customEvent["retry_count"] = retryCount;

      AnalyticsService.Instance.RecordEvent(customEvent);
      Debug.Log($"[Analytics] Stage Start: Mode={gameMode}, StageID={stageId}, Retry={retryCount}");
    }
    catch (ServicesInitializationException e)
    {
      Debug.LogWarning($"[Analytics] Analytics 서비스가 초기화되지 않았습니다: {e.Message}");
    }
    catch (System.Exception e)
    {
      Debug.LogError($"[Analytics] 이벤트 전송 실패: {e.Message}");
    }
  }

  /// <summary>
  /// 스테이지 완료 이벤트를 전송합니다.
  /// </summary>
  /// <param name="gameMode">게임 모드 ("Stage")</param>
  /// <param name="stageId">스테이지 번호</param>
  /// <param name="finalBallValue">클리어 시 공의 숫자</param>
  /// <param name="playTimeSec">플레이 소요 시간 (초)</param>
  public void AnalyticsStageComplete(string gameMode, int stageId, int finalBallValue, float playTimeSec)
  {
    // 에디터에서는 데이터 수집하지 않음
    if (Application.isEditor)
    {
      Debug.Log($"[Analytics] Stage Complete (Editor - Skipped): Mode={gameMode}, StageID={stageId}, FinalValue={finalBallValue}, Time={playTimeSec:F2}s");
      return;
    }

    try
    {
      var customEvent = new CustomEvent("stage_complete");
      customEvent["game_mode"] = gameMode;
      customEvent["stage_id"] = stageId;
      customEvent["final_ball_value"] = finalBallValue;
      customEvent["play_time_sec"] = playTimeSec;

      AnalyticsService.Instance.RecordEvent(customEvent);
      Debug.Log($"[Analytics] Stage Complete: Mode={gameMode}, StageID={stageId}, FinalValue={finalBallValue}, Time={playTimeSec:F2}s");
    }
    catch (ServicesInitializationException e)
    {
      Debug.LogWarning($"[Analytics] Analytics 서비스가 초기화되지 않았습니다: {e.Message}");
    }
    catch (System.Exception e)
    {
      Debug.LogError($"[Analytics] 이벤트 전송 실패: {e.Message}");
    }
  }

  /// <summary>
  /// 레벨 실패 이벤트를 전송합니다.
  /// </summary>
  /// <param name="gameMode">게임 모드 ("stage" 또는 "endless")</param>
  /// <param name="stageId">스테이지 모드면 스테이지 ID, 무한 모드면 0</param>
  /// <param name="progressRatio">스테이지 모드 진행률 (0.0 ~ 1.0)</param>
  /// <param name="failReason">실패 이유 ("fall" 또는 "value_zero")</param>
  /// <param name="deathPosX">죽은 위치 X 좌표</param>
  /// <param name="currentBallValue">죽을 당시 공의 숫자</param>
  /// <param name="maxNumberReached">무한 모드 도달한 최대 숫자</param>
  /// <param name="stageClear">무한 모드 총 스테이지 돌파 수</param>
  /// <param name="playTimeSec">플레이 타임 (초)</param>
  /// <param name="isNewRecord">무한 모드 유저의 기존 최고 기록 갱신 여부</param>
  public void AnalyticsStageFail(
    string gameMode,
    int stageId,
    float? progressRatio = null,
    string failReason = null,
    float? deathPosX = null,
    int? currentBallValue = null,
    int? maxNumberReached = null,
    int? stageClear = null,
    float? playTimeSec = null,
    bool? isNewRecord = null)
  {
    var customEvent = new CustomEvent("stage_fail");
    customEvent["game_mode"] = gameMode;
    customEvent["stage_id"] = stageId;
    
    if (progressRatio.HasValue)
      customEvent["progress_ratio"] = progressRatio.Value;
    if (!string.IsNullOrEmpty(failReason))
      customEvent["fail_reason"] = failReason;
    if (deathPosX.HasValue)
      customEvent["death_pos_x"] = deathPosX.Value;
    if (currentBallValue.HasValue)
      customEvent["current_ball_value"] = currentBallValue.Value;
    if (maxNumberReached.HasValue)
      customEvent["max_number_reached"] = maxNumberReached.Value;
    if (stageClear.HasValue)
      customEvent["stage_clear"] = stageClear.Value;
    if (playTimeSec.HasValue)
      customEvent["play_time_sec"] = playTimeSec.Value;
    if (isNewRecord.HasValue)
      customEvent["is_new_record"] = isNewRecord.Value;

    // 에디터에서는 데이터 수집하지 않음
    if (Application.isEditor)
    {
      Debug.Log($"[Analytics] Stage Fail (Editor - Skipped): Mode={gameMode}, StageID={stageId}, Reason={failReason ?? "N/A"}, Value={currentBallValue ?? -1}");
      return;
    }

    try
    {
      AnalyticsService.Instance.RecordEvent(customEvent);
      Debug.Log($"[Analytics] Stage Fail: Mode={gameMode}, StageID={stageId}, Reason={failReason ?? "N/A"}, Value={currentBallValue ?? -1}");
    }
    catch (ServicesInitializationException e)
    {
      Debug.LogWarning($"[Analytics] Analytics 서비스가 초기화되지 않았습니다: {e.Message}");
    }
    catch (System.Exception e)
    {
      Debug.LogError($"[Analytics] 이벤트 전송 실패: {e.Message}");
    }
  }

  #region Ad Monetization Events

  /// <summary>
  /// 광고 시작 이벤트를 전송합니다.
  /// </summary>
  /// <param name="adType">광고 타입 (예: "banner", "interstitial", "rewarded")</param>
  /// <param name="placementId">광고 배치 ID (선택사항)</param>
  /// <param name="network">광고 네트워크 (선택사항)</param>
  public void AnalyticsAdStart(string adType, string placementId = null, string network = null)
  {
    // 에디터에서는 데이터 수집하지 않음
    if (Application.isEditor)
    {
      Debug.Log($"[Analytics] Ad Start (Editor - Skipped): Type={adType}, Placement: {placementId ?? "N/A"}, Network: {network ?? "N/A"}");
      return;
    }

    try
    {
      var customEvent = new CustomEvent("ad_start");
      customEvent["ad_type"] = adType;
      if (!string.IsNullOrEmpty(placementId))
      {
        customEvent["placement_id"] = placementId;
      }
      if (!string.IsNullOrEmpty(network))
      {
        customEvent["network"] = network;
      }

      AnalyticsService.Instance.RecordEvent(customEvent);
      Debug.Log($"[Analytics] Ad Start: Type={adType}, Placement: {placementId ?? "N/A"}, Network: {network ?? "N/A"}");
    }
    catch (ServicesInitializationException e)
    {
      Debug.LogWarning($"[Analytics] Analytics 서비스가 초기화되지 않았습니다: {e.Message}");
    }
    catch (System.Exception e)
    {
      Debug.LogError($"[Analytics] 이벤트 전송 실패: {e.Message}");
    }
  }

  /// <summary>
  /// 광고 완료 이벤트를 전송합니다.
  /// </summary>
  /// <param name="adType">광고 타입 (예: "banner", "interstitial", "rewarded")</param>
  /// <param name="placementId">광고 배치 ID (선택사항)</param>
  /// <param name="network">광고 네트워크 (선택사항)</param>
  /// <param name="duration">광고 시청 시간 (초, 선택사항)</param>
  public void AnalyticsAdComplete(string adType, string placementId = null, string network = null, float? duration = null)
  {
    // 에디터에서는 데이터 수집하지 않음
    if (Application.isEditor)
    {
      Debug.Log($"[Analytics] Ad Complete (Editor - Skipped): Type={adType}, Placement: {placementId ?? "N/A"}, Network: {network ?? "N/A"}");
      return;
    }

    try
    {
      var customEvent = new CustomEvent("ad_complete");
      customEvent["ad_type"] = adType;
      if (!string.IsNullOrEmpty(placementId))
      {
        customEvent["placement_id"] = placementId;
      }
      if (!string.IsNullOrEmpty(network))
      {
        customEvent["network"] = network;
      }
      if (duration.HasValue)
      {
        customEvent["duration"] = duration.Value;
      }

      AnalyticsService.Instance.RecordEvent(customEvent);
      Debug.Log($"[Analytics] Ad Complete: Type={adType}, Placement: {placementId ?? "N/A"}, Network: {network ?? "N/A"}");
    }
    catch (ServicesInitializationException e)
    {
      Debug.LogWarning($"[Analytics] Analytics 서비스가 초기화되지 않았습니다: {e.Message}");
    }
    catch (System.Exception e)
    {
      Debug.LogError($"[Analytics] 이벤트 전송 실패: {e.Message}");
    }
  }

  /// <summary>
  /// 광고 클릭 이벤트를 전송합니다.
  /// </summary>
  /// <param name="adType">광고 타입 (예: "banner", "interstitial", "rewarded")</param>
  /// <param name="placementId">광고 배치 ID (선택사항)</param>
  /// <param name="network">광고 네트워크 (선택사항)</param>
  public void AnalyticsAdClick(string adType, string placementId = null, string network = null)
  {
    // 에디터에서는 데이터 수집하지 않음
    if (Application.isEditor)
    {
      Debug.Log($"[Analytics] Ad Click (Editor - Skipped): Type={adType}, Placement: {placementId ?? "N/A"}, Network: {network ?? "N/A"}");
      return;
    }

    try
    {
      var customEvent = new CustomEvent("ad_click");
      customEvent["ad_type"] = adType;
      if (!string.IsNullOrEmpty(placementId))
      {
        customEvent["placement_id"] = placementId;
      }
      if (!string.IsNullOrEmpty(network))
      {
        customEvent["network"] = network;
      }

      AnalyticsService.Instance.RecordEvent(customEvent);
      Debug.Log($"[Analytics] Ad Click: Type={adType}, Placement: {placementId ?? "N/A"}, Network: {network ?? "N/A"}");
    }
    catch (ServicesInitializationException e)
    {
      Debug.LogWarning($"[Analytics] Analytics 서비스가 초기화되지 않았습니다: {e.Message}");
    }
    catch (System.Exception e)
    {
      Debug.LogError($"[Analytics] 이벤트 전송 실패: {e.Message}");
    }
  }

  /// <summary>
  /// 광고 노출(임프레션) 이벤트를 전송합니다.
  /// </summary>
  /// <param name="adType">광고 타입 ("interstitial")</param>
  /// <param name="placement">배치 ("next_stage", "enter_endless", "retry_endless")</param>
  /// <param name="gameMode">게임 모드 ("stage" 또는 "endless")</param>
  /// <param name="stageId">선형 모드면 방금 깬 스테이지 ID, 무한 모드면 0</param>
  /// <param name="network">광고 네트워크 (선택사항)</param>
  public void AnalyticsAdImpression(string adType, string placement, string gameMode, int stageId, string network = null)
  {
    // 에디터에서는 데이터 수집하지 않음
    if (Application.isEditor)
    {
      Debug.Log($"[Analytics] Ad Impression (Editor - Skipped): Type={adType}, Placement={placement}, Mode={gameMode}, StageID={stageId}");
      return;
    }

    try
    {
      var customEvent = new CustomEvent("ad_impression");
      customEvent["ad_type"] = adType;
      customEvent["placement"] = placement;
      customEvent["game_mode"] = gameMode;
      customEvent["stage_id"] = stageId;
      if (!string.IsNullOrEmpty(network))
      {
        customEvent["network"] = network;
      }

      AnalyticsService.Instance.RecordEvent(customEvent);
      Debug.Log($"[Analytics] Ad Impression: Type={adType}, Placement={placement}, Mode={gameMode}, StageID={stageId}");
    }
    catch (ServicesInitializationException e)
    {
      Debug.LogWarning($"[Analytics] Analytics 서비스가 초기화되지 않았습니다: {e.Message}");
    }
    catch (System.Exception e)
    {
      Debug.LogError($"[Analytics] 이벤트 전송 실패: {e.Message}");
    }
  }

  /// <summary>
  /// 광고 실패 이벤트를 전송합니다.
  /// </summary>
  /// <param name="adType">광고 타입 (예: "banner", "interstitial", "rewarded")</param>
  /// <param name="placementId">광고 배치 ID (선택사항)</param>
  /// <param name="network">광고 네트워크 (선택사항)</param>
  /// <param name="errorCode">에러 코드 (선택사항)</param>
  /// <param name="errorMessage">에러 메시지 (선택사항)</param>
  public void AnalyticsAdFailed(string adType, string placementId = null, string network = null, int? errorCode = null, string errorMessage = null)
  {
    // 에디터에서는 데이터 수집하지 않음
    if (Application.isEditor)
    {
      Debug.Log($"[Analytics] Ad Fail (Editor - Skipped): Type={adType}, Placement: {placementId ?? "N/A"}, Network: {network ?? "N/A"}, Error: {errorCode ?? -1}");
      return;
    }

    try
    {
      var customEvent = new CustomEvent("ad_fail");
      customEvent["ad_type"] = adType;
      if (!string.IsNullOrEmpty(placementId))
      {
        customEvent["placement_id"] = placementId;
      }
      if (!string.IsNullOrEmpty(network))
      {
        customEvent["network"] = network;
      }
      if (errorCode.HasValue)
      {
        customEvent["error_code"] = errorCode.Value;
      }
      if (!string.IsNullOrEmpty(errorMessage))
      {
        customEvent["error_message"] = errorMessage;
      }

      AnalyticsService.Instance.RecordEvent(customEvent);
      Debug.Log($"[Analytics] Ad Fail: Type={adType}, Placement: {placementId ?? "N/A"}, Network: {network ?? "N/A"}, Error: {errorCode ?? -1}");
    }
    catch (ServicesInitializationException e)
    {
      Debug.LogWarning($"[Analytics] Analytics 서비스가 초기화되지 않았습니다: {e.Message}");
    }
    catch (System.Exception e)
    {
      Debug.LogError($"[Analytics] 이벤트 전송 실패: {e.Message}");
    }
  }

  #endregion

  /// <summary>
  /// 커스텀 이벤트를 전송합니다.
  /// </summary>
  /// <param name="eventName">이벤트 이름</param>
  /// <param name="parameters">이벤트 파라미터 (선택사항)</param>
  public void AnalyticsCustomEvent(
    string eventName,
    Dictionary<string, object> parameters = null)
  {
    // 에디터에서는 데이터 수집하지 않음
    if (Application.isEditor)
    {
      Debug.Log($"[Analytics] Custom Event (Editor - Skipped): {eventName}");
      return;
    }

    try
    {
      var customEvent = new CustomEvent(eventName);
      if (parameters != null)
      {
        foreach (var kvp in parameters)
        {
          customEvent[kvp.Key] = kvp.Value;
        }
      }

      AnalyticsService.Instance.RecordEvent(customEvent);
      Debug.Log($"[Analytics] Custom Event: {eventName}");
    }
    catch (ServicesInitializationException e)
    {
      Debug.LogWarning($"[Analytics] Analytics 서비스가 초기화되지 않았습니다: {e.Message}");
    }
    catch (System.Exception e)
    {
      Debug.LogError($"[Analytics] 이벤트 전송 실패: {e.Message}");
    }
  }

  #endregion
}
