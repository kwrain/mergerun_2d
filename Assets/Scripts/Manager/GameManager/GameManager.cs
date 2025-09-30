using System;
using System.Collections.Generic;
using System.Linq;
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
public partial class GameManager : Singleton<GameManager>, ITouchEvent
{
  private const int TOUCH_PRIORITY = 1000;

  /// <summary>
  /// 풀링 시 제거 대상 처리를 위한 큐
  /// </summary>
  private Queue<GameObject> destructionQueue = new();

  /// <summary>
  /// 업데이트 구분이 필요한 모델들을 등록해서 사용.
  /// 주로, 애니메이션이 필요하나 건물이나 엔피씨처럼 관리되지 않는 경우에 해당됨.
  /// </summary>
  private List<BaseObject> onUpdateModels = new();

  private Vector2 standardPos;
  private BaseObject selectedObject;

  private GameCamera gameCamera;

  /// <summary>
  /// ApplicationPause
  /// </summary>
  private DateTime pauseStartTime;

  private bool IsApplicationPause { get; set; }

  public float CpuScore { get; private set; } = 1.0f;
  public float GpuScore { get; private set; } = 1.0f;
  public float RamScore { get; private set; } = 1.0f;

  public float SuspendTime { get; private set; }

  public LayerMask AllLayer { get; private set; }
  public LayerMask BuildingLayer { get; private set; }
  public LayerMask NpcLayer { get; private set; }

  public GameCamera GameCamera
  {
    get
    {
      if (gameCamera == null || gameCamera.gameObject.IsDestroyed())
      {
        gameCamera = Camera.main.gameObject.GetComponentInParent<GameCamera>();
      }

      return gameCamera;
    }
  }
  public BaseObject SelectedObject
  {
    get => selectedObject;
    set
    {
      selectedObject = value;
    }
  }

  protected override void Awake()
  {
    base.Awake();

    // init layer mask
    AllLayer = LayerMask.GetMask("Default", "Default_2", "Building", "Npc");
    BuildingLayer = LayerMask.GetMask("Building");
    NpcLayer = LayerMask.GetMask("Npc");  

    AutoSetting();

    void AutoSetting()
    {
      (
        int processorFrequency,
        int processorCount,
        int graphicsMemorySize,
        int graphicsShaderLevel,
        int maxTextureSize,
        int systemMemorySize
        ) referenceSpec = (2150, 4, 3417, 45, 16384, 3417);

      SetNeverSleepMode(); // lds - 25.2.3, 앱 시작 시에는 일단 절전 모드를 비활성화 함.

      //프레임 고정
      QualitySettings.vSyncCount = 0;

      Input.multiTouchEnabled = true;

      EventSystem.current.pixelDragThreshold = (int)(0.5f * Screen.dpi / 2.54f);

#if UNITY_EDITOR
      CpuScore *= UnityEngine.Device.SystemInfo.processorFrequency / referenceSpec.processorFrequency;
      CpuScore *= UnityEngine.Device.SystemInfo.processorCount / referenceSpec.processorCount;

      GpuScore *= UnityEngine.Device.SystemInfo.graphicsMemorySize / referenceSpec.graphicsMemorySize;
      GpuScore *= UnityEngine.Device.SystemInfo.graphicsShaderLevel / referenceSpec.graphicsShaderLevel;
      GpuScore *= UnityEngine.Device.SystemInfo.maxTextureSize / referenceSpec.maxTextureSize;

      RamScore *= UnityEngine.Device.SystemInfo.systemMemorySize / referenceSpec.systemMemorySize;
#else
      CpuScore *= SystemInfo.processorFrequency / referenceSpec.processorFrequency;
      CpuScore *= SystemInfo.processorCount / referenceSpec.processorCount;

      GpuScore *= SystemInfo.graphicsMemorySize / referenceSpec.graphicsMemorySize;
      GpuScore *= SystemInfo.graphicsShaderLevel / referenceSpec.graphicsShaderLevel;
      GpuScore *= SystemInfo.maxTextureSize / referenceSpec.maxTextureSize;

      RamScore *= SystemInfo.systemMemorySize / referenceSpec.systemMemorySize;
#endif

      Debug.Log($"Device Score : CPU : {CpuScore} / GPU : {GpuScore} / RAM : {RamScore}");
    }
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

  public void ScheduleForDestruction(GameObject obj)
  {
    destructionQueue.Enqueue(obj);

    // 일정 수준 이상 쌓이면 즉시 처리
    if (destructionQueue.Count > 5)
    {
      ProcessDestructionQueue();
    }
  }

  /// <summary>
  /// 호출 시점 체크 필요.
  /// </summary>
  public void ProcessDestructionQueue()
  {
    while (destructionQueue.Count > 0)
    {
      var obj = destructionQueue.Dequeue();
      if (obj != null)
      {
        Destroy(obj);
      }
    }
  }

  #region Touch Event

  public List<BaseObject> GetSortedHitObjects(List<BaseObject> hitObjects, bool onlyLayer = false)
  {
    // kw 24.12.17
    // - 데코레이션 건물은 터치 우선 순위를 최하위로 설정한다.
    // - BuildingGround 보다는 높아야함.

    hitObjects.Sort(LayerSort);
    return hitObjects;

    int LayerSort(BaseObject obj1, BaseObject obj2)
    {
      GetSortData(obj1, out var sortingLayer1, out var orderInLayer1);
      GetSortData(obj2, out var sortingLayer2, out var orderInLayer2);

      // 동일 위치 sortingLayer 우선적으로 확인한다.
      if (sortingLayer1 == sortingLayer2)
      {
        return orderInLayer2.CompareTo(orderInLayer1);
      }
      else
      {
        return sortingLayer2.CompareTo(sortingLayer1);
      }
    }

    void GetSortData(BaseObject obj, out int sortingLayer, out int orderInLayer)
    {
      sortingLayer = obj.SortingLayerValue;
      orderInLayer = obj.RenderOrder;
    }

  }


  public void OnTouchBegan(Vector3 pos, bool isFirstTouchedUI)
  {
   
  }

  public void OnTouchStationary(Vector3 pos, float time, bool isFirstTouchedUI)
  {

  }

  public void OnTouchMoved(Vector3 lastPos, Vector3 newPos, bool isFirstTouchedUI)
  {
  }

  public void OnTouchEnded(Vector3 pos, bool isFirstTouchedUI, bool isMoved)
  {
   
  }

  public void OnTouchCanceled(Vector3 pos, bool isFirstTouchedUI, bool isMoved)
  {
  }

  public void OnLongTouched(Vector3 pos, bool isFirstTouchedUI)
  {
  }

  public void OnClicked(Vector3 pos, bool isFirstTouchedUI)
  {
  }

  public void OnPinchUpdated(float offset, float zoomSpeed, bool isFirstTouchedUI)
  {
  }
  public void OnPinchEnded()
  {
  }

  public void OnChangeTouchEventState(bool state)
  {

  }

  #endregion

  public void SetSystemSleepMode()
  {
    Screen.sleepTimeout = SleepTimeout.SystemSetting;
  }

  public void SetNeverSleepMode()
  {
    Screen.sleepTimeout = SleepTimeout.NeverSleep;
  }
}