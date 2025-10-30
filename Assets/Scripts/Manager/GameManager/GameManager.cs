using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

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