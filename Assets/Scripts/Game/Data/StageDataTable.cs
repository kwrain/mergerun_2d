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

  public bool VaildStageData(int stageID, bool infinity)
  {
    var datas = infinity ? infinityStagedata : stageData;
    return datas.ContainsKey(stageID);
  }

  public StageData GetStageData(int stageID, bool infinity, params int[] excludeIds)
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
        
        // 제외할 ID가 있으면 필터링
        if (excludeIds != null && excludeIds.Length > 0)
        {
          keys.RemoveAll(id => System.Array.IndexOf(excludeIds, id) >= 0);
        }
        
        // 필터링 후 남은 키가 있는지 확인
        if (keys.Count > 0)
        {
          var randomKey = keys[UnityEngine.Random.Range(0, keys.Count)];
          return datas[randomKey];
        }
        else
        {
          // 모든 키가 제외되었을 경우 원본 키에서 랜덤 추출
          var allKeys = new List<int>(datas.Keys);
          if (allKeys.Count > 0)
          {
            var randomKey = allKeys[UnityEngine.Random.Range(0, allKeys.Count)];
            return datas[randomKey];
          }
        }
      }
      return null;
    }
  }
}