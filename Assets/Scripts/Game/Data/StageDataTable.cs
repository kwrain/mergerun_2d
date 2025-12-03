using System;
using System.Collections.Generic;
using FAIRSTUDIOS.SODB.Utils;
using UnityEngine;

[CreateAssetMenu(fileName = "StageDataTable", menuName = "Game/StageDataTable")]
public class StageDataTable : ScriptableObject
{
  [Serializable]
  public class StageData
  {
    public int stageId;       // 스테이지 번호
    public bool infinity;       // 무한모드 여부

    public List<MapData> mapData = new();
    public List<ObstacleData> obstacleData = new();
    public List<MergeableData> mergeableData = new();
  }

  [Serializable]
  public class StageObjectData
  {
    public Vector3 position;
    public Vector3 scale;

    public Vector2 size;
    public Vector2 offset;
  }

  // --- 지형 데이터 ---
  [Serializable]
  public class MapData : StageObjectData
  {
    public MapElement.MapElementTypes type;
  }

  // --- 장애물 데이터 ---
  [Serializable]
  public class ObstacleData : StageObjectData
  {
    public ObstacleBase.ObstacleTypes type;

    // 각 장애물별 데이터 요소
    public float cooltime;
    public int limitRelativeLevel;
  }

  [Serializable]
  public class MergeableData : StageObjectData
  {
    public int relativeLevel;
  }

  [Header("[Prefabs]")] public MapGround mapElementGround;
  public MapGround mapElementGroundDiagonal;
  public MapBridge mapElementBridge;
  [Space]
  public ObstacleSpike obstacleSpike;
  public ObstacleGoal obstacleGoal;
  [Space]
  public MergeableObject mergeableObject;

  [Space]
  public GenericDictionary<int, StageData> stageData;
  public GenericDictionary<int, StageData> infinityStagedata;

  public StageData GetStageData(int stageID, bool infinity)
  {
    var datas = infinity ? infinityStagedata : stageData;
    if (datas.ContainsKey(stageID))
      return datas[stageID];
    else
    {
      // infinity가 false일 때 stageID가 존재하지 않으면 랜덤으로 추출
      if (!infinity && datas.Count > 0)
      {
        var keys = new List<int>(datas.Keys);
        var randomKey = keys[UnityEngine.Random.Range(0, keys.Count)];
        return datas[randomKey];
      }
      return null;
    }
  }
}