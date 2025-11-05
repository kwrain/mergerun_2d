using System.Collections.Generic;
using UnityEngine;
using static StageDataTable;
using static MapElement;
using static ObstacleBase;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif 

public partial class StageManager : Singleton<StageManager>
{
  private const int POOLING_MAX_SIZE = 30;

  [SerializeField] private MergeablePlayer player;

  [Header("[Settings]")]
  [SerializeField] private bool isTest;
  [SerializeField] private Transform startPosition;

  [Header("[Parents]")]
  [SerializeField] private Transform mapParent;
  [SerializeField] private Transform mergeableParent;
  [SerializeField] private Transform obstacleParent;

  [Header("[Data]")]
  [SerializeField] private StageDataTable stageDataTable;
  [SerializeField] private StageData stageData;

  private List<MapElement> stageMapElements = new();
  private List<MergeableObject> stageMergeable = new();
  private List<ObstacleBase> stageObstalce = new();

  public StageDataTable StageDataTable => stageDataTable;
  public StageData StageData => stageData;

  public int StageID => stageData.stageId;
  public bool Infinity => stageData.infinity;

  static StageManager()
  {
    // 부모 클래스(Singleton<T>)의 static protected 필드인 prefabPath를 설정합니다.
    // 여기에 SOManager 프리팹의 실제 Addressable/Resources 경로를 입력하세요.
    prefabPath = "BundleLocal/Prefabs/Manager/StageManager.prefab";
  }

  protected override void Awake()
  {
    base.Awake();

    var mapElements = mapParent.GetComponentsInChildren<MapElement>();
    foreach (var mapElement in mapElements)
    {
      PushMapElementInPool(mapElement);
    }

    var obstacles = obstacleParent.GetComponentsInChildren<ObstacleBase>();
    foreach (var obstacle in obstacles)
    {
      PushObstacleInPool(obstacle);
    }

    var mergeables = mergeableParent.GetComponentsInChildren<MergeableObject>();
    foreach (var mergeable in mergeables)
    {
      PushMergeableInPool(mergeable);
    }
  }

  protected override void Start()
  {
    base.Start();

    SetUI();
    StartStage();
  }

  private void OnEnable()
  {
#if !UNITY_EDITOR
    AdManager.Instance.AddOnInternetLostListener(OnInternetLost);
    AdManager.Instance.AddOnInternetRestoredListener(OnInternetRestored);
#endif
  }

  private void OnDisable()
  {
#if !UNITY_EDITOR
    AdManager.Instance.RemoveOnInternetLostListener(OnInternetLost);
    AdManager.Instance.RemoveOnInternetRestoredListener(OnInternetRestored);
#endif
  }

  private void Update()
  {

  }

#if UNITY_EDITOR
  /// <summary>
  /// 에디터의 "Game" 뷰에서만 테스트용 버튼을 노출합니다.
  /// </summary>
  private void OnGUI()
  {
    // OnGUI는 매 프레임 여러 번 호출될 수 있으므로, 
    // GUI 스타일을 한 번만 설정하는 것이 좋습니다.
    GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
    buttonStyle.fontSize = 20; // 폰트 크기 조절

    // 화면 우측 상단에 "Restart Stage" 버튼을 그립니다.
    // Rect(x, y, width, height)
    if (GUI.Button(new Rect(Screen.width - 210, 10, 200, 60), "Restart Stage", buttonStyle))
    {
      // 버튼을 클릭하면 RestartStage 함수를 호출합니다.
      RestartStage();
    }

    if (GUI.Button(new Rect(Screen.width - 210, 70, 200, 60), "Normal Stage", buttonStyle))
    {
      StartStage(false);
    }

    if (GUI.Button(new Rect(Screen.width - 210, 130, 200, 60), "Limit Stage", buttonStyle))
    {
      StartStage(true);
    }
  }
#endif

  public override Task Initialize()
  {
    ClearStageObjects();

    return base.Initialize();
  }

  private void ClearStageObjects()
  {
    for (int i = stageMapElements.Count - 1; i >= 0; i--)
    {
      var value = stageMapElements[i];
      PushMapElementInPool(value);
    }

    for (int i = stageObstalce.Count - 1; i >= 0; i--)
    {
      var value = stageObstalce[i];
      PushObstacleInPool(value);
    }

    for (int i = stageMergeable.Count - 1; i >= 0; i--)
    {
      var value = stageMergeable[i];
      PushMergeableInPool(value);
    }
    stageMapElements.Clear();
    stageObstalce.Clear();
    stageMergeable.Clear();
  }

  #region Map

  private Queue<MapElement> mapElementPool = new();

  public MapElement PeekMapElementInPool(MapElementTypes elementType)
  {
    MapElement element = null;
    if (mapElementPool.Count == 0)
    {
      element = Create();
    }
    else
    {
      element = mapElementPool.Dequeue();
    }

    element.SetActive(true);
    stageMapElements.Add(element);
    return element;

    MapElement Create()
    {
      switch (elementType)
      {
        case MapElementTypes.Ground:
          element = Instantiate(stageDataTable.mapElementGround, mapParent);
          break;

        case MapElementTypes.Bridge:
          element = Instantiate(stageDataTable.mapElementBridge, mapParent);
          break;
      }

      return element;
    }
  }

  public void PushMapElementInPool(MapElement element)
  {
    element.SetActive(false);
    stageMapElements.Remove(element);

    // // nedd check OnUpdate object;
    // GameManager.Instance.RemoveUpdateModel(obj);
    if (mapElementPool.Count >= POOLING_MAX_SIZE)
    {
      GameManager.Instance.ScheduleForDestruction(element.gameObject);
    }
    else
    {
      mapElementPool.Enqueue(element);
    }
  }

  #endregion Map

  #region Obstacle

  private Dictionary<ObstacleTypes, Queue<ObstacleBase>> obstaclePool = new();

  public ObstacleBase PeekObstacleInPool(ObstacleTypes type)
  {
    ObstacleBase obstacle = null;
    if (obstaclePool.ContainsKey(type))
    {
      if (obstaclePool[type].Count == 0)
      {
        obstacle = Create();
      }
      else
      {
        obstacle = obstaclePool[type].Dequeue();
      }
    }
    else
    {
      obstacle = Create();
    }

    if (obstacle == null)
    {
      return null;
    }

    obstacle.SetActive(true);
    stageObstalce.Add(obstacle);
    return obstacle;

    ObstacleBase Create()
    {
      switch (type)
      {
        case ObstacleTypes.Spike:
          obstacle = Instantiate(StageDataTable.obstacleSpike, obstacleParent);
          break;

        case ObstacleTypes.Goal:
          obstacle = Instantiate(StageDataTable.obstacleGoal, obstacleParent);
          break;
      }

      return obstacle;
    }
  }

  public void PushObstacleInPool(ObstacleBase obstacle)
  {
    obstacle.SetActive(false);
    stageObstalce.Remove(obstacle);

    // // nedd check OnUpdate object;
    // GameManager.Instance.RemoveUpdateModel(obj);
    var type = obstacle.Type;
    if (!obstaclePool.ContainsKey(type))
    {
      obstaclePool.Add(type, new Queue<ObstacleBase>());
    }

    if (obstaclePool[type].Count >= POOLING_MAX_SIZE)
    {
      GameManager.Instance.ScheduleForDestruction(obstacle.gameObject);
    }
    else
    {
      obstaclePool[type].Enqueue(obstacle);
    }
  }

  #endregion Obstacle

  #region MergeableObject

  private Queue<MergeableObject> mergeablePool = new();

  public MergeableObject PeekMergeableInPool()
  {
    MergeableObject obj;

    // 풀에서 건물 가져오기
    if (mergeablePool.Count == 0)
    {
      // create
      // 생성 시 생성 위치 지정 필수.
      obj = Create();
    }
    else
    {
      obj = mergeablePool.Dequeue();
    }

    if (obj == null)
    {
      return null;
    }


    obj.SetActive(true);
    stageMergeable.Add(obj);
    return obj;

    MergeableObject Create()
    {
      if (stageDataTable.mergeableObject == null)
        return null;

      var go = Instantiate(stageDataTable.mergeableObject, mergeableParent);
      if (go == null)
        return null;

      return go.GetComponent<MergeableObject>();
    }
  }

  public void PushMergeableInPool(MergeableObject obj)
  {
    obj.SetActive(false);
    stageMergeable.Remove(obj);

    // nedd check OnUpdate object;
    GameManager.Instance.RemoveUpdateModel(obj);

    // UI 처리 (안전한 널 체크)
    // UIManager.Instance?.HideHUD(building);

    if (mergeablePool.Count >= POOLING_MAX_SIZE)
    {
      GameManager.Instance.ScheduleForDestruction(obj.gameObject);
    }
    else
    {
      mergeablePool.Enqueue(obj);
    }
  }

  #endregion MergeableObject

  private void PlayerSetting()
  {
    if (player != null)
    {
      var savedLevel = SOManager.Instance.PlayerPrefsModel.UserSavedLevel;
      var levelData = SOManager.Instance.GameDataTable.GetLevelData(savedLevel);
      if (levelData != null)
      {
        MergeableData data = new()
        {
          scale = Vector3.one * levelData.scale
        };

        player.SetData(data);
        player.SetLevel(savedLevel);
      }
    }
  }

  public void LoadStage(int stage, bool infinity)
  {
    Debug.Log($"Load Staget : {stage} / {infinity}");
    ClearStageObjects();

    // 풀에 있는 오브젝트를 꺼내서 사용한다.
    var data = stageDataTable.GetStageData(stage, infinity);
    if (data == null)
      return;

    stageData = data;
    GameModel.Global.InfinityMode = data.infinity;
    if (data.infinity)
    {
      var level = SOManager.Instance.PlayerPrefsModel.UserBestLevel;
      var text = SOManager.Instance.GameDataTable.PowerOfTwoString(level);
      SetText(text);
    }
    else
    {
      SetText((data.stageId + 1).ToString());
    }
    foreach (var mapData in data.mapData)
      {
        var element = PeekMapElementInPool(mapData.type);
        if (element != null)
        {
          element.SetData(mapData);
        }
      }

    foreach (var obstacleData in data.obstacleData)
    {
      var obstacle = PeekObstacleInPool(obstacleData.type);
      if (obstacle != null)
      {
        obstacle.SetData(obstacleData);
      }
    }

    foreach (var mergeableData in data.mergeableData)
    {
      var mergeable = PeekMergeableInPool();
      if (mergeable != null)
      {
        mergeable.SetData(mergeableData);
        mergeable.SetLevel(SOManager.Instance.PlayerPrefsModel.UserSavedLevel + mergeableData.relativeLevel);
      }
    }
  }

  /// <summary>
  /// 디바이스에 저장된 정보를 사용한다.
  /// </summary>
  public void StartStage(bool infinity = false)
  {
    Debug.Log("StartStage");
    var stage = SOManager.Instance.PlayerPrefsModel.UserSavedStage;
    PlayerSetting();
    LoadStage(stage, infinity);
    Debug.Log("StartStage");
  }

  public void RestartStage()
  {
    Debug.Log("RestartStage");
    PlayerSetting();
    LoadStage(stageData.stageId, stageData.infinity);
  }

  public void CompleteStage()
  {
    // 경험치 체크
    var level = SOManager.Instance.PlayerPrefsModel.UserSavedLevel;
    var expData = SOManager.Instance.GameDataTable.GetExpData(level);
    if (expData != null)
    {
      var exp = (int)Math.Pow(2, player.Level); // 쌓을수잇는 경험치
      var currExp = SOManager.Instance.PlayerPrefsModel.UserSavedExp;
      if (currExp + exp >= expData.exp)
      {
        // 최대 레벨이 아닌 경우
        if (level < SOManager.Instance.PlayerPrefsModel.MaxSavedLevel)
        {
          SOManager.Instance.PlayerPrefsModel.UserSavedLevel = ++level;
          currExp = currExp + exp - expData.exp;
          expData = SOManager.Instance.GameDataTable.GetExpData(level);
        }
        else // 최대 레벨인 경우
        {
          currExp = expData.exp;
        }
      }
      else
      {
        currExp += exp;
      }

      SOManager.Instance.PlayerPrefsModel.UserSavedExp = currExp;

      expProgressBar.SetProgress(currExp / expData.exp);
      expProgressBar.SetText($"{currExp}/{expData.exp}");
    }

    SOManager.Instance.GameModel.StageComplete = true;
    SOManager.Instance.PlayerPrefsModel.UserSavedStage = StageID + 1;

    StartCoroutine(Timer(3f, () =>
    {
      // 전면 광고 노출
#if UNITY_EDITOR
      InterstitialAdCompleted();
#else
      AdManager.Instance.ShowInterstitial(InterstitialAdCompleted);
#endif
    }));

    // 레벨 완료 연출 시간 보장(3 ~ 5초) -> 광고 노출 이후 스테이지 재시작 처리를 한다.

    IEnumerator Timer(float time, Action onComplete)
    {
      yield return new WaitForSeconds(time);
      onComplete?.Invoke();
    }

    void InterstitialAdCompleted()
    {
      SOManager.Instance.GameModel.StageComplete = false;
      StartStage();
    }
  }

#if !UNITY_EDITOR
  private void OnInternetLost()
  {
    SOManager.Instance.GameModel.DisconnectInternet = true;
  }

  private void OnInternetRestored()
  {
    SOManager.Instance.GameModel.DisconnectInternet = false;
  }
#endif
}

#region EDITOR

#if UNITY_EDITOR
[CustomEditor(typeof(StageManager))]
public class StageManagerEditor : Editor
{
  private int stageId;
  private bool infinity;       // 무한모드 여부

  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();

    serializedObject.Update();
    StageManager stage = (StageManager)target;

    EditorGUILayout.Space();
    EditorGUILayout.LabelField("[Editor Only]", EditorStyles.boldLabel);
    stageId = EditorGUILayout.IntField("Stage ID", stageId);
    infinity = EditorGUILayout.Toggle("Infinity", infinity);

    // --- ScriptableObject 섹션 ---
    if (stage.StageDataTable != null)
    {
      if (GUILayout.Button("Save to ScriptableObject"))
      {
        SaveStageToSO(stage);
      }
      if (GUILayout.Button("Load from ScriptableObject"))
      {
        LoadStageFromSO(stage);
      }
      if (GUILayout.Button("Clear Steage Objects"))
      {
        ClearStageObjects(stage);
      }
    }

    // 변경된 사항을 적용합니다. Undo/Redo를 위해 필수입니다.
    serializedObject.ApplyModifiedProperties();
  }

  // --- SO 저장/불러오기 로직 ---
  private void SaveStageToSO(StageManager stage)
  {
    if (stage.StageDataTable == null)
    {
      Debug.LogError("StageDataTable가 할당되지 않았습니다!");
      return;
    }

    var stageData = CreateDataFromScene(stage);
    var table = infinity ? stage.StageDataTable.infinityStagedata : stage.StageDataTable.stageData;
    if (table.ContainsKey(stageId))
    {
      table[stageId] = stageData;
    }
    else
    {
      table.Add(stageId, stageData);
    }

    EditorUtility.SetDirty(stage.StageDataTable);
    AssetDatabase.SaveAssets();

    StageData CreateDataFromScene(StageManager stage)
    {
      var stageData = new StageData
      {
        stageId = stageId,
        infinity = infinity
      };

      var mapGrounds = stage.GetComponentsInChildren<MapGround>();
      mapGrounds = mapGrounds.OrderBy(g => g.transform.position.y).ToArray();
      for (int i = 0; i < mapGrounds.Length; i++)
      {
        var ground = mapGrounds[i];
        var data = new MapData
        {
          type = ground.ElementType,
          position = ground.transform.position,
          scale = ground.transform.localScale,
          // size = ground.GetComponent<BoxCollider2D>().size
          size = ground.spriteRenderer.size,
          offset = ground.GetComponent<BoxCollider2D>().offset
        };

        if (i == 0)
        {
          data.isFrist = true;
        }
        else if (i == mapGrounds.Length - 1)
        {
          data.isLast = true;
        }

        stageData.mapData.Add(data);
      }
      var mapBridges = stage.GetComponentsInChildren<MapBridge>();
      foreach (var bridge in mapBridges)
      {
        var data = new MapData
        {
          type = bridge.ElementType,
          position = bridge.transform.position,
          scale = bridge.transform.localScale,
          // size = bridge.GetComponent<BoxCollider2D>().size,
          size = bridge.spriteRenderer.size,
          offset = bridge.GetComponent<BoxCollider2D>().offset
        };

        stageData.mapData.Add(data);
      }

      var obstacles = stage.GetComponentsInChildren<ObstacleBase>();
      foreach (var obstacle in obstacles)
      {
        var data = new ObstacleData
        {
          position = obstacle.transform.position,
          scale = obstacle.transform.localScale,
          size = obstacle.GetComponent<BoxCollider2D>().size,
          offset = obstacle.GetComponent<BoxCollider2D>().offset,
          type = obstacle.Type
        };

        switch (obstacle.Type)
        {
          case ObstacleTypes.Goal:
            var goal = obstacle as ObstacleGoal;
            data.limitRelativeLevel = goal.LimitRelativeLevel;
            break;
        }

        stageData.obstacleData.Add(data);
      }

      var mergeables = stage.GetComponentsInChildren<MergeableObject>();
      foreach (var mergeable in mergeables)
      {
        var data = new MergeableData
        {
          position = mergeable.transform.position,
          scale = mergeable.transform.localScale,
          // size = mergeable.GetComponent<CircleCollider2D>().size,
          offset = mergeable.GetComponent<CircleCollider2D>().offset,
          relativeLevel = mergeable.RelativeLevel
        };

        stageData.mergeableData.Add(data);
      }

      return stageData;
    }
  }

  private void LoadStageFromSO(StageManager stage)
  {
    if (stage.StageDataTable == null)
    {
      Debug.LogError("StageDataTable가 할당되지 않았습니다!");
      return;
    }

    var data = stage.StageDataTable.GetStageData(stageId, infinity);
    if (data != null)
    {
      GenerateSceneFromData(stage, data);
      Debug.Log("ScriptableObject로부터 스테이지를 로드했습니다!");
    }
    else
    {
      Debug.LogError("Stage ID를 확인해주세요!");
    }

    void GenerateSceneFromData(StageManager stage, StageData stageData)
    {
      // 현재 있는 오브젝트 전부 제거 후 재생성해야함.
      var mapParent = stage.transform.Find("Map");
      var obstacleParent = stage.transform.Find("Obstacles");
      var mergeableParent = stage.transform.Find("MergeableObjects");

      ClearChildren(mapParent);
      ClearChildren(obstacleParent);
      ClearChildren(mergeableParent);

      MapElement element = null;
      foreach (var mapData in stageData.mapData)
      {
        switch (mapData.type)
        {
          case MapElementTypes.Ground:
            element = Instantiate(stage.StageDataTable.mapElementGround, mapParent);
            break;

          case MapElementTypes.Bridge:
            element = Instantiate(stage.StageDataTable.mapElementBridge, mapParent);
            break;
        }

        if (element != null)
        {
          element.SetData(mapData);
        }
      }

      ObstacleBase obstacle = null;
      foreach (var obstacleData in stageData.obstacleData)
      {
        switch (obstacleData.type)
        {
          case ObstacleTypes.Spike:
            obstacle = Instantiate(stage.StageDataTable.obstacleSpike, obstacleParent);
            break;

          case ObstacleTypes.Goal:
            obstacle = Instantiate(stage.StageDataTable.obstacleGoal, obstacleParent);
            break;
        }

        if (obstacle != null)
        {
          obstacle.SetData(obstacleData);
        }
      }

      foreach (var mergeableData in stageData.mergeableData)
      {
        var mergeable = Instantiate(stage.StageDataTable.mergeableObject, mergeableParent);
        if (mergeable != null)
        {
#if UNITY_EDITOR
          mergeable.gameObject.name = mergeable.gameObject.GetHashCode().ToString();
#endif

          mergeable.SetData(mergeableData);
        }
      }
    }
  }

  private void ClearStageObjects(StageManager stage)
  {
    ClearChildren(stage.transform.Find("Map"));
    ClearChildren(stage.transform.Find("Obstacles"));
    ClearChildren(stage.transform.Find("MergeableObjects"));
  }

  private void ClearChildren(Transform parent)
  {
    for (int i = parent.childCount - 1; i >= 0; i--)
    {
      DestroyImmediate(parent.GetChild(i).gameObject);
    }
  }
}
#endif

#endregion EDITOR
