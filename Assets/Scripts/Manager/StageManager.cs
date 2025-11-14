using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static StageDataTable;
using static MapElement;
using static ObstacleBase;
using FAIRSTUDIOS.Manager;

#if UNITY_EDITOR
using UnityEditor;
#endif 

public partial class StageManager : Singleton<StageManager>
{
  private const int POOLING_MAX_SIZE = 30;

  [SerializeField] private MergeablePlayer player;

  [Header("[Settings]")]
  [SerializeField] private Transform startPosition;

  [Header("[Parents]")]
  [SerializeField] private Transform mapParent;
  [SerializeField] private Transform mergeableParent;
  [SerializeField] private Transform obstacleParent;

  [Header("[Data]")]
  [SerializeField] private StageDataTable stageDataTable;

#if UNITY_EDITOR
  [HideInInspector, SerializeField] private int stageIdForEditor;
  [HideInInspector, SerializeField] private bool infinityForEditor;
  [HideInInspector, SerializeField] private bool isTest; 
#endif

  private StageData prevStageData;
  private StageData stageData;
  private StageData nextStageData;

  private Dictionary<int, List<MapElement>> stageMapElements = new();
  private Dictionary<int, List<MergeableObject>> stageMergeables = new();
  private Dictionary<int, List<ObstacleBase>> stageObstalces = new();

  private bool ReadyInterstitialAd { get; set; }

  public StageDataTable StageDataTable => stageDataTable;

  public int StageID => stageData.stageId;
  public bool Infinity => stageData.infinity;

  public MergeablePlayer Player => player;

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

  private void OnGUI()
  {
    // OnGUI는 매 프레임 여러 번 호출될 수 있으므로, 
    // GUI 스타일을 한 번만 설정하는 것이 좋습니다.
    GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
    buttonStyle.fontSize = 20; // 폰트 크기 조절

    // 화면 우측 상단에 "Restart Stage" 버튼을 그립니다.
    // Rect(x, y, width, height)
    if (GUI.Button(new Rect(Screen.width - 210, 10, 200, 60), "Reset UserData", buttonStyle))
    {
      ClearUserData();
      StartStage();
    }
  }

  [ContextMenu("ClearUserData")]
  private void ClearUserData()
  {
    SOManager.Instance.PlayerPrefsModel.UserBestLevel = 1;
    SOManager.Instance.PlayerPrefsModel.UserSavedLevel = 1;
    SOManager.Instance.PlayerPrefsModel.UserSavedExp = 0;
    SOManager.Instance.PlayerPrefsModel.UserSavedStage = 0;
  }

  private IEnumerator Timer(float time, Action onComplete)
  {
    yield return new WaitForSeconds(time);
    onComplete?.Invoke();
  }

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

  private float CalculateStageEndY(List<MapElement> mapElements)
  {
    if (mapElements.Count == 0)
      return 0f;

    float value = 0;
    foreach (var e in mapElements)
    {
      if (e == null) continue;
      var sprite = e.GetComponent<SpriteRenderer>();
      float sizeY = sprite ? sprite.bounds.size.y : e.transform.localScale.y;
      value = e.transform.position.y + sizeY;
    }

    return value; // 다음 스테이지 시작 기준점
  }

  private void GenerateStageObjects(StageData data, float offsetY = 0f, bool active = true)
  {
    var stageId = data.stageId;
    if (!stageMapElements.ContainsKey(stageId))
    {
      stageMapElements.Add(stageId, new());
    }
    if (!stageObstalces.ContainsKey(stageId))
    {
      stageObstalces.Add(stageId, new());
    }
    if (!stageMergeables.ContainsKey(stageId))
    {
      stageMergeables.Add(stageId, new());
    }

    foreach (var mapData in data.mapData)
    {
      var element = PeekMapElementInPool(mapData.type);
      if (element != null)
      {
        var pos = mapData.position;
        pos.y += offsetY;
        element.SetData(mapData);
        element.transform.position = pos;
        element.SetActive(active);

        stageMapElements[stageId].Add(element);
      }
    }

    foreach (var obstacleData in data.obstacleData)
    {
      var obstacle = PeekObstacleInPool(obstacleData.type);
      if (obstacle != null)
      {
        var pos = obstacleData.position;
        pos.y += offsetY;
        obstacle.SetData(obstacleData);
        obstacle.transform.position = pos;
        obstacle.SetActive(active);

        stageObstalces[stageId].Add(obstacle);
      }
    }

    foreach (var mergeableData in data.mergeableData)
    {
      var mergeable = PeekMergeableInPool();
      if (mergeable != null)
      {
        var pos = mergeableData.position;
        pos.y += offsetY;
        mergeable.SetData(mergeableData);
        mergeable.transform.position = pos;
        mergeable.SetActive(active);

        stageMergeables[stageId].Add(mergeable);
      }
    }
  }

  /// <summary>
  /// 디바이스에 저장된 정보를 사용한다.
  /// </summary>
  public void StartStage(bool infinity = false, bool restart = false)
  {
    StopAllCoroutines();
    stageCompleteAnimator.SetActive(false);
    SOManager.Instance.GameModel.StageComplete = false;

    if (infinity)
    {
      LoadInfinityStage(restart);
    }
    else
    {
      LoadStage(restart);
    }

    PlayerSetting();
  }

  private void ClearStageObjects()
  {
    foreach (var kv in stageMapElements)
    {
      for (int i = kv.Value.Count - 1; i >= 0; i--)
      {
        var value = kv.Value[i];
        PushMapElementInPool(value);
      }
    }

    foreach (var kv in stageObstalces)
    {
      for (int i = kv.Value.Count - 1; i >= 0; i--)
      {
        var value = kv.Value[i];
        PushObstacleInPool(value);
      }
    }

    foreach (var kv in stageMergeables)
    {
      for (int i = kv.Value.Count - 1; i >= 0; i--)
      {
        var value = kv.Value[i];
        PushMergeableInPool(value);
      }
    }

    stageMapElements.Clear();
    stageObstalces.Clear();
    stageMergeables.Clear();
  }

  void InterstitialAdCompleted()
  {
    StartStage();
  }

  #region Normal

  public void LoadStage(bool restart = false)
  {
    ClearStageObjects();

    if (!restart)
    {
      var stage = SOManager.Instance.PlayerPrefsModel.UserSavedStage;
      var data = stageDataTable.GetStageData(stage, false);
      if (data == null)
        return;

      stageData = data;
      SetText((data.stageId + 1).ToString());
    }

    GenerateStageObjects(stageData, 0, true);
  }

  [ContextMenu("CompleteStage")]
  public void CompleteStage()
  {
    player.Movable = false;

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
          var stageData = StageDataTable.GetStageData(level, Infinity);
          if (stageData != null)
          {
            SOManager.Instance.PlayerPrefsModel.UserSavedLevel = ++level;
          }

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

      // expProgressBar.AutoProgress()
      expProgressBar.SetProgress(currExp / expData.exp);
      expProgressBar.SetText($"{currExp}/{expData.exp}");
    }

    SOManager.Instance.GameModel.StageComplete = true;
    SOManager.Instance.PlayerPrefsModel.UserSavedStage = StageID + 1;

    stageCompleteAnimator.SetActive(true);
    stageCompleteAnimator.SetTrigger("Show");

    StartCoroutine(Timer(stageCompleteWaitTime, () =>
    {
      // 전면 광고 노출
#if UNITY_EDITOR
      InterstitialAdCompleted();
#else
      // AdManager.Instance.ShowInterstitial(InterstitialAdCompleted);
      InterstitialAdCompleted();
#endif

    }));

    // 레벨 완료 연출 시간 보장(3 ~ 5초) -> 광고 노출 이후 스테이지 재시작 처리를 한다.
  } 
  #endregion

  #region Infinity

  private void LoadInfinityStage(bool restart = false)
  {
    ClearStageObjects();

    if (restart)
    {
      if (ReadyInterstitialAd)
      {
        // AdManager.Instance.ShowInterstitial(InterstitialAdCompleted);

#if !UNITY_EDITOR
#endif
        ReadyInterstitialAd = false;
      }
    }
    else
    {
      var infinityStages = stageDataTable.infinityStagedata.Values.ToList();
      if (infinityStages.Count == 0)
      {
        Debug.LogError("Infinity stage data is empty!");
        return;
      }

      stageData = infinityStages[UnityEngine.Random.Range(0, infinityStages.Count)];

      var level = SOManager.Instance.PlayerPrefsModel.UserBestLevel;
      var text = SOManager.Instance.GameDataTable.PowerOfTwoString(level);
      SetText(text);

    }

    GenerateStageObjects(stageData, 0f, true);

    // 다음 스테이지 미리 로드
    PreloadNextInfinityStage();

    StartCoroutine(Timer(30,
    () =>
     {
       ReadyInterstitialAd = true;
     }));
  }

  public void UnloadPrevInfiniytyStage()
  {
    if (prevStageData == null)
      return;

    // 이전 스테이지 정보가 있으면 풀에 넣어준다.
    var stageId = prevStageData.stageId;
    var mapElements = stageMapElements[stageId];
    for (int i = mapElements.Count - 1; i >= 0; i--)
    {
      var value = mapElements[i];
      PushMapElementInPool(value);
    }

    var obstacles = stageObstalces[stageId];
    for (int i = obstacles.Count - 1; i >= 0; i--)
    {
      var value = obstacles[i];
      PushObstacleInPool(value);
    }

    var mergeables = stageMergeables[stageId];
    for (int i = mergeables.Count - 1; i >= 0; i--)
    {
      var value = mergeables[i];
      PushMergeableInPool(value);
    }
  }

  private void PreloadNextInfinityStage()
  {
    var infinityStages = stageDataTable.infinityStagedata.Values.ToList();
    infinityStages.RemoveAll(s => s.stageId == stageData.stageId);
    if (nextStageData != null)
    {
      infinityStages.RemoveAll(s => s.stageId == nextStageData.stageId);
    }
    if (prevStageData != null)
    {
      infinityStages.RemoveAll(s => s.stageId == prevStageData.stageId);
    }

    if (infinityStages.Count == 0)
      return;

    // 오프셋 계산 (이전 스테이지의 마지막 Y)
    var offsetY = 0f;
    var mepElements = stageMapElements[stageData.stageId];
    if (mepElements != null)
    {
      offsetY = CalculateStageEndY(mepElements);
    }

    nextStageData = infinityStages[UnityEngine.Random.Range(0, infinityStages.Count)];
    // 다음 스테이지 오브젝트 생성 (비활성 상태)
    GenerateStageObjects(nextStageData, offsetY, true);
  }

  public void CompleteInfinityStage()
  {
    UnloadPrevInfiniytyStage();

    prevStageData = stageData;
    stageData = nextStageData;
    nextStageData = null;

    PreloadNextInfinityStage();
  }

  #endregion

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
  private SerializedProperty stageIdForEditor;
  private SerializedProperty infinityForEditor;

  protected void OnEnable()
  {
    if (!target)
      return;

    stageIdForEditor = serializedObject.FindProperty("stageIdForEditor");
    infinityForEditor = serializedObject.FindProperty("infinityForEditor");
  }

  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();

    serializedObject.Update();
    StageManager stage = (StageManager)target;

    EditorGUILayout.PropertyField(stageIdForEditor);
    EditorGUILayout.PropertyField(infinityForEditor);

    EditorGUILayout.Space();

    // --- ScriptableObject 섹션 ---
    if (stage.StageDataTable != null)
    {
      if (GUILayout.Button("Save to ScriptableObject"))
      {
        bool confirm = EditorUtility.DisplayDialog(
          "저장 확인",
          "현재 Stage 데이터를 ScriptableObject에 저장하시겠습니까?",
          "저장",
          "취소"
        );

        if (confirm)
        {
          SaveStageToSO(stage);
        }
        else
        {
          Debug.Log("저장 작업이 취소되었습니다.");
        }
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

    var stageId = stageIdForEditor.intValue;
    var infinity = infinityForEditor.boolValue;


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

    var data = stage.StageDataTable.GetStageData(stageIdForEditor.intValue, infinityForEditor.boolValue);
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
