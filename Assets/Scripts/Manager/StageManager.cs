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
  [SerializeField] private Map map;
  [SerializeField] private Transform startPosition;
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
  private Coroutine interstitialAdTimerCoroutine;

  // Analytics 관련 변수
  private float stageStartTime; // 스테이지 시작 시간
  private int infinityStageClearCount = 0; // 무한 모드 스테이지 클리어 수
  private int currentStageIdForAnalytics = 0; // 현재 스테이지 ID (Analytics용)

  // Analytics 접근용 프로퍼티
  public float StageStartTime => stageStartTime;
  public int InfinityStageClearCount => infinityStageClearCount;

  public StageDataTable StageDataTable => stageDataTable;

  public StageData StageData => stageData;
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
    #if DEV
    // OnGUI는 매 프레임 여러 번 호출될 수 있으므로, 
    // GUI 스타일을 한 번만 설정하는 것이 좋습니다.
    GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
    buttonStyle.fontSize = 20; // 폰트 크기 조절

    // 화면 우측 상단에 "Restart Stage" 버튼을 그립니다.
    // Rect(x, y, width, height)
    if (GUI.Button(new Rect(Screen.width - 210, 10, 200, 60), "Reset UserData", buttonStyle))
    {
      ClearUserData();
      StartStage(Infinity);
    }
    #endif
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

  private void PlayerSetting(bool infinity)
  {
    StopAllCoroutines();
    
    if (player != null)
    {
      var level = infinity ? 1 : SOManager.Instance.PlayerPrefsModel.UserSavedLevel;
      var levelData = SOManager.Instance.GameDataTable.GetLevelData(level);

      if (levelData != null)
      {
        MergeableData data = new()
        {
          scale = Vector3.one * levelData.scale
        };

        player.SetData(data);
        player.SetLevel(level);
      }
    }
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
        var pos =  mapData.position;
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

    // Map의 Update 비활성화 (스테이지 로딩 중에는 Update가 실행되지 않도록)
    if (map != null)
    {
      map.SetUpdateEnabled(false);
    }

    PlayerSetting(infinity);

    if (infinity)
    {
      LoadInfinityStage(restart);
    }
    else
    {
      LoadStage(restart);
    }

    // 스테이지 시작 시간 기록
    stageStartTime = Time.time;

    // Analytics: stage_start 이벤트 전송
    if (GameManager.Instance != null && stageData != null)
    {
      currentStageIdForAnalytics = infinity ? 0 : stageData.stageId;
      string gameMode = infinity ? "endless" : "stage";
      
      // 재시도 횟수 가져오기
      int retryCount = 0;
      if (restart)
      {
        // 재시도 시 횟수 증가
        retryCount = SOManager.Instance.PlayerPrefsModel.UserSavedStageRetryCount + 1;
        SOManager.Instance.PlayerPrefsModel.UserSavedStageRetryCount = retryCount;
      }
      else
      {
        // 새 스테이지 시작 시 재시도 횟수 초기화
        SOManager.Instance.PlayerPrefsModel.UserSavedStageRetryCount = 0;
        retryCount = 0;
      }

      GameManager.Instance.AnalyticsStageStart(gameMode, currentStageIdForAnalytics, retryCount: retryCount);
    }

    // Map의 MergeableObject 스캔
    if (map != null)
    {
      map.ScanMergeableObjects();
      // 스캔 완료 후 Update 다시 활성화
      map.SetUpdateEnabled(true);
    }
  }

  private void ClearStageObjects()
  {
    foreach (var kv in stageMapElements)
    {
      for (int i = kv.Value.Count - 1; i >= 0; i--)
      {
        var value = kv.Value[i];
        if (value != null)
        {
          PushMapElementInPool(value);
        }
      }
    }

    foreach (var kv in stageObstalces)
    {
      for (int i = kv.Value.Count - 1; i >= 0; i--)
      {
        var value = kv.Value[i];
        if (value != null)
        {
          PushObstacleInPool(value);
        }
      }
    }

    foreach (var kv in stageMergeables)
    {
      for (int i = kv.Value.Count - 1; i >= 0; i--)
      {
        var value = kv.Value[i];
        if (value != null)
        {
          PushMergeableInPool(value);
        }
      }
    }

    stageMapElements.Clear();
    stageObstalces.Clear();
    stageMergeables.Clear();
  }

  private void InterstitialAdCompleted()
  {
    StartStage(Infinity);
  }

  private void InterstitialAdFailed(int code)
  {
    StartStage(Infinity);
  }

  /// <summary>
  /// 진행률 계산 (선형 모드용)
  /// </summary>
  /// <param name="currentPosY">현재 위치 Y 좌표</param>
  /// <returns>진행률 (0.0 ~ 1.0)</returns>
  public float CalculateProgressRatio(float currentPosY)
  {
    if (Infinity || stageData == null)
      return 0f;

    // 시작 위치 찾기
    float startY = startPosition != null ? startPosition.position.y : 0f;

    // 골 위치 찾기
    float goalY = startY;
    if (stageObstalces.ContainsKey(stageData.stageId))
    {
      var obstacles = stageObstalces[stageData.stageId];
      foreach (var obstacle in obstacles)
      {
        if (obstacle != null && obstacle.Type == ObstacleTypes.Goal)
        {
          goalY = obstacle.transform.position.y;
          break;
        }
      }
    }

    // 진행률 계산
    if (Mathf.Approximately(startY, goalY))
      return 0f;

    float totalDistance = Mathf.Abs(goalY - startY);
    float currentDistance = Mathf.Abs(currentPosY - startY);
    float ratio = Mathf.Clamp01(currentDistance / totalDistance);
    
    return ratio;
  }

  #region Normal

  public void LoadStage(bool restart = false)
  {
    ClearStageObjects();

    var stage = SOManager.Instance.PlayerPrefsModel.UserSavedStage;

    if (!restart)
    {
      StageData data = null;
      if (stageData != null)
      {
        data = stageDataTable.GetStageData(stage, false, stageData.stageId);
      }
      else
      {
        data = stageDataTable.GetStageData(stage, false);
      }
      if (data == null)
        return;

      stageData = data;
      SetText((stage + 1).ToString());
    }

    GenerateStageObjects(stageData, 0, true);
  }

  [ContextMenu("CompleteStage")]
  public void CompleteStage()
  {
    // 플레이 타임 계산
    float playTimeSec = Time.time - stageStartTime;

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
      expProgressBar.SetProgress((float)currExp / expData.exp);
      expProgressBar.SetText($"{currExp}/{expData.exp}");
    }

    SOManager.Instance.GameModel.StageComplete = true;

    if (stageDataTable.VaildStageData(StageID + 1, false))
    {
      SOManager.Instance.PlayerPrefsModel.UserSavedStage++;
    }
    else
    {
      SOManager.Instance.PlayerPrefsModel.UserSavedStage = StageID + 1;
    }

    // Analytics: stage_complete 이벤트 전송
    if (GameManager.Instance != null && stageData != null)
    {
      int finalBallValue = player.Level;
      GameManager.Instance.AnalyticsStageComplete("Stage", stageData.stageId, finalBallValue, playTimeSec);
      
      // 재시도 횟수 초기화 (클리어했으므로)
      SOManager.Instance.PlayerPrefsModel.UserSavedStageRetryCount = 0;
    }

    stageCompleteAnimator.SetActive(true);
    stageCompleteAnimator.SetTrigger("Show");

    StartCoroutine(Timer(stageCompleteWaitTime, () =>
    {
      player.Movable = false;

      // Analytics: ad_impression 이벤트 전송 (선형 모드 스테이지 완료 후)
      if (GameManager.Instance != null && stageData != null)
      {
        GameManager.Instance.AnalyticsAdImpression("interstitial", "next_stage", "stage", stageData.stageId);
      }

      // 전면 광고 노출
#if UNITY_EDITOR
      InterstitialAdCompleted();
#else
      AdManager.Instance.ShowInterstitial(InterstitialAdCompleted, InterstitialAdFailed);
#endif

    }));

    // 레벨 완료 연출 시간 보장(3 ~ 5초) -> 광고 노출 이후 스테이지 재시작 처리를 한다.
  } 
  #endregion

  #region Infinity

  private void LoadInfinityStage(bool restart = false)
  {
    ClearStageObjects();

    // 기존 코루틴 중지
    if (interstitialAdTimerCoroutine != null)
    {
      StopCoroutine(interstitialAdTimerCoroutine);
      interstitialAdTimerCoroutine = null;
    }

    if (restart)
    {
      if (ReadyInterstitialAd)
      {
        // Analytics: ad_impression 이벤트 전송 (무한 모드 재시도)
        if (GameManager.Instance != null)
        {
          GameManager.Instance.AnalyticsAdImpression("interstitial", "retry_endless", "endless", 0);
        }

#if !UNITY_EDITOR
        AdManager.Instance.ShowInterstitial(InterstitialAdCompleted, InterstitialAdFailed);
#endif
        ReadyInterstitialAd = false;
      }
      
      // restart일 때 stageData가 null이면 새로 로드
      if (stageData == null)
      {
        var infinityStages = stageDataTable.infinityStagedata.Values.ToList();
        if (infinityStages.Count == 0)
        {
          Debug.LogError("Infinity stage data is empty!");
          return;
        }
        stageData = infinityStages[UnityEngine.Random.Range(0, infinityStages.Count)];
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

    if (stageData == null)
    {
      Debug.LogError("StageData is null!");
      return;
    }

    GenerateStageObjects(stageData, 0f, true);

    interstitialAdTimerCoroutine = StartCoroutine(Timer(30,
    () =>
     {
       ReadyInterstitialAd = true;
       interstitialAdTimerCoroutine = null;
     }));
  }

  public void UnloadPrevInfiniytyStage()
  {
    if (prevStageData == null)
      return;

    // 이전 스테이지 정보가 있으면 풀에 넣어준다.
    var stageId = prevStageData.stageId;
    if (stageMapElements.ContainsKey(stageId))
    {
      var mapElements = stageMapElements[stageId];
      for (int i = mapElements.Count - 1; i >= 0; i--)
      {
        var value = mapElements[i];
        if (value != null)
        {
          PushMapElementInPool(value);
        }
      }
      stageMapElements.Remove(stageId);
    }

    if (stageObstalces.ContainsKey(stageId))
    {
      var obstacles = stageObstalces[stageId];
      for (int i = obstacles.Count - 1; i >= 0; i--)
      {
        var value = obstacles[i];
        if (value != null)
        {
          PushObstacleInPool(value);
        }
      }
      stageObstalces.Remove(stageId);
    }

    if (stageMergeables.ContainsKey(stageId))
    {
      var mergeables = stageMergeables[stageId];
      for (int i = mergeables.Count - 1; i >= 0; i--)
      {
        var value = mergeables[i];
        if (value != null)
        {
          PushMergeableInPool(value);
        }
      }
      stageMergeables.Remove(stageId);
    }
  }

  private void PreloadNextInfinityStage()
  {
    if (stageData == null)
    {
      Debug.LogWarning("PreloadNextInfinityStage: stageData is null!");
      return;
    }

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

    var offsetY = 0f;
    // stageMapElements에서 현재 스테이지의 MapElement 중 가장 큰 offset 값을 찾기
    if (stageMapElements.ContainsKey(stageData.stageId))
    {
      var mapElements = stageMapElements[stageData.stageId];
      if (mapElements != null && mapElements.Count > 0)
      {
        foreach (var element in mapElements)
        {
          if (element != null)
          {
            var spriteRenderer = element.spriteRenderer;
            if (spriteRenderer != null)
            {
              var currentOffset = spriteRenderer.bounds.size.y + element.transform.position.y;
              if (currentOffset > offsetY)
              {
                offsetY = currentOffset;
              }
            }
          }
        }
      }
    }

    nextStageData = infinityStages[UnityEngine.Random.Range(0, infinityStages.Count)];
    // 다음 스테이지 오브젝트 생성 (비활성 상태)
    GenerateStageObjects(nextStageData, offsetY, true);
  }

  public void CompleteInfinityStage()
  {
    if (map != null)
    {
      map.SetUpdateEnabled(false);
    }

    // 무한 모드 스테이지 클리어 수 증가
    infinityStageClearCount++;

    UnloadPrevInfiniytyStage();
    PreloadNextInfinityStage();
    prevStageData = stageData;
    stageData = nextStageData;

    // 새로운 스테이지 시작 시간 기록
    stageStartTime = Time.time;

    if (map != null)
    {
      map.SetUpdateEnabled(true);
    }
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
          size = ground.spriteRenderer.size,
          offset = ground.GetComponent<PolygonCollider2D>().offset
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
          size = bridge.spriteRenderer.size,
          offset = bridge.GetComponent<PolygonCollider2D>().offset
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

          case MapElementTypes.GroundDiagonal:
            element = Instantiate(stage.StageDataTable.mapElementGroundDiagonal, mapParent);
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
