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
    public uint stageId;       // 스테이지 번호
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
    public uint limitRelativeGrade;
  }

  [Serializable]
  public class MergeableData : StageObjectData
  {
    public uint grade;
  }

  [Header("[Prefabs]")] public GameObject mapElementGround;
  public GameObject mapElementBridge;
  [Space]
  public GameObject obstacleWall;
  public GameObject obstacleSpike;
  [Space]
  public GameObject mergeableObject;

  [Space]
  public GenericDictionary<uint, StageData> stageData;
  public GenericDictionary<uint, StageData> infinityStagedata;

  public StageData GetStageData(uint stageID, bool infinity)
  {
    var datas = infinity ? infinityStagedata : stageData;
    if (datas.ContainsKey(stageID))
      return datas[stageID];
    else
      return null;
  }
}