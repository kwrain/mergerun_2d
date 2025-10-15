using System.Collections.Generic;
using UnityEngine;
using static StageDataTable;
using static MapElement;
using static ObstacleBase;
using System;
using System.Threading.Tasks;



#if UNITY_EDITOR
using UnityEditor;
#endif 

public class StageManager : Singleton<StageManager>
{
  private const int POOLING_MAX_SIZE = 30;

  [Header("[Settings]")]
  [SerializeField] private Transform startPosition;

  [Header("[Parents]")]
  [SerializeField] private Transform mapParent;
  [SerializeField] private Transform mergeableParent;
  [SerializeField] private Transform obstacleParent;

  [Header("[Data]")]
  [SerializeField] private StageDataTable stageDataTable;
  [SerializeField] private StageData stageData;

  private Dictionary<int, MapElement> stageMapElements;
  private Dictionary<int, MergeableObject> stageMergeable;
  private Dictionary<int, ObstacleBase> stageObstalce;

  public StageDataTable StageDataTable => stageDataTable;
  public StageData StageData => stageData;

  public uint StageID => stageData.stageId;
  public bool Infinity => stageData.infinity;

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

  public override Task Initialize()
  {
    ClearStageObjects();

    return base.Initialize();
  }

  private void ClearStageObjects()
  {
    // 현재 스테이지에 생성된 오브젝트를 재배치한다. (일단 그냥 배치)
    foreach (var kv in stageMapElements)
    {
      PushMapElementInPool(kv.Value);
    }

    foreach (var kv in stageObstalce)
    {
      PushObstacleInPool(kv.Value);
    }

    foreach (var kv in stageMergeable)
    {
      PushMergeableInPool(kv.Value);
    }
    stageMapElements.Clear();
    stageObstalce.Clear();
    stageMergeable.Clear();
  }

  #region Map

  private Queue<MapElement> mapElementPool;

  public MapElement PeekMapElementInPool(MapElementTypes elementType)
  {
    MapElement element;
    if (mapElementPool.Count == 0)
    {
      element = Create();
    }
    else
    {
      element = mapElementPool.Dequeue();
    }

    element.SetActive(true);
    return element;

    MapElement Create()
    {
      if (stageDataTable.mergeableObject == null)
        return null;

      var go = Instantiate(stageDataTable.mergeableObject);
      if (go == null)
        return null;

      return go.GetComponent<MapElement>();
    }
  }

  public void PushMapElementInPool(MapElement element)
  {
    element.SetActive(false);

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

  private Dictionary<Type, Queue<ObstacleBase>> obstaclePool = new();

  public T PeekObstacleInPool<T>() where T : ObstacleBase
  {
    var type = typeof(T);

    T obstacle;
    if (obstaclePool.ContainsKey(type))
    {
      if (obstaclePool[type].Count == 0)
      {
        obstacle = Create<T>();
      }
      else
      {
        obstacle = (T)obstaclePool[type].Dequeue();
      }
    }
    else
    {
      obstacle = Create<T>();
    }

    if (obstacle == null)
    {
      return null;
    }

    obstacle.SetActive(true);
    return obstacle;

    T Create<T>() where T : ObstacleBase
    {
      if (stageDataTable.mergeableObject == null)
        return null;

      var go = Instantiate(stageDataTable.mergeableObject);
      if (go == null)
        return null;

      return go.GetComponent<T>();
    }
  }

  public void PushObstacleInPool(ObstacleBase obstacle)
  {
    obstacle.SetActive(false);

    // // nedd check OnUpdate object;
    // GameManager.Instance.RemoveUpdateModel(obj);
    var type = obstacle.GetType();
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

  private Queue<MergeableObject> mergeablePool;

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
    return obj;

    MergeableObject Create()
    {
      if (stageDataTable.mergeableObject == null)
        return null;

      var go = Instantiate(stageDataTable.mergeableObject);
      if (go == null)
        return null;

      return go.GetComponent<MergeableObject>();
    }
  }

  public void PushMergeableInPool(MergeableObject obj)
  {
    obj.SetActive(false);

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

  public void LoadStage(uint stage, bool infinity)
  {
    ClearStageObjects();

    // 풀에 있는 오브젝트를 꺼내서 사용한다.
    var data = stageDataTable.GetStageData(stage, infinity);
    if (data == null)
      return;

    GameObject go = null;
    foreach (var mapData in stageData.mapData)
    {
      switch (mapData.type)
      {
        case MapElementTypes.Ground:
          go = Instantiate(stageDataTable.mapElementGround, mapParent);
          break;

        case MapElementTypes.Bridge:
          go = Instantiate(stageDataTable.mapElementBridge, mapParent);
          break;
      }

      go.transform.position = mapData.position;
      go.transform.localScale = mapData.scale;
    }

    foreach (var obstacleData in stageData.obstacleData)
    {
      switch (obstacleData.type)
      {
        case ObstacleTypes.Spike:
          go = Instantiate(stageDataTable.obstacleSpike, obstacleParent);
          break;

        case ObstacleTypes.Wall:
          go = Instantiate(stageDataTable.obstacleWall, obstacleParent);
          break;
      }

      go.transform.position = obstacleData.position;
      go.transform.localScale = obstacleData.scale;
    }

    foreach (var mergeableData in stageData.mergeableData)
    {
      go = Instantiate(stageDataTable.mergeableObject, mergeableParent);
      go.transform.position = mergeableData.position;
      go.transform.localScale = mergeableData.scale;
    }
  }

  public void UnloadStage()
  {

  }
}

#region EDITOR

#if UNITY_EDITOR
[CustomEditor(typeof(StageManager))]
public class StageManagerEditor : Editor
{
  private uint stageId;
  private bool infinity;       // 무한모드 여부

  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();

    serializedObject.Update();
    StageManager stage = (StageManager)target;

    EditorGUILayout.Space();
    EditorGUILayout.LabelField("[Editor Only]", EditorStyles.boldLabel);
    var value = EditorGUILayout.IntField("Stage ID", (int)stageId);
    if (value < 0) value = 0; // 음수 방지
    stageId = (uint)value;

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
    if (stage.StageDataTable.stageData.ContainsKey(stageId))
    {
      stage.StageDataTable.stageData[stageId] = stageData;
    }
    else
    {
      stage.StageDataTable.stageData.Add(stageId, stageData);
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

      var mapElements = stage.GetComponentsInChildren<MapElement>();
      foreach (var mapElement in mapElements)
      {
        var data = new MapData
        {
          type = mapElement.ElementType,
          position = mapElement.transform.position,
          scale = mapElement.transform.localScale,
          size = mapElement.GetComponent<BoxCollider2D>().size,
          offset = mapElement.GetComponent<BoxCollider2D>().offset
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
          offset = mergeable.GetComponent<CircleCollider2D>().offset
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

      GameObject go = null;
      foreach (var mapData in stageData.mapData)
      {
        switch (mapData.type)
        {
          case MapElementTypes.Ground:
            go = Instantiate(stage.StageDataTable.mapElementGround, mapParent);
            break;

          case MapElementTypes.Bridge:
            go = Instantiate(stage.StageDataTable.mapElementBridge, mapParent);
            break;
        }

        go.transform.position = mapData.position;
        go.transform.localScale = mapData.scale;
      }

      foreach (var obstacleData in stageData.obstacleData)
      {
        switch (obstacleData.type)
        {
          case ObstacleTypes.Spike:
            go = Instantiate(stage.StageDataTable.obstacleSpike, obstacleParent);
            break;

          case ObstacleTypes.Wall:
            go = Instantiate(stage.StageDataTable.obstacleWall, obstacleParent);
            break;
        }

        go.transform.position = obstacleData.position;
        go.transform.localScale = obstacleData.scale;
      }

      foreach (var mergeableData in stageData.mergeableData)
      {
        go = Instantiate(stage.StageDataTable.mergeableObject, mergeableParent);
        go.transform.position = mergeableData.position;
        go.transform.localScale = mergeableData.scale;
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
