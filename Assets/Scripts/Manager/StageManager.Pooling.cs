using System.Collections.Generic;
using UnityEngine;
using static MapElement;
using static ObstacleBase;

public partial class StageManager
{
  #region Map
  
  private Dictionary<MapElementTypes, Queue<MapElement>> mapElementPool = new();

  public MapElement PeekMapElementInPool(MapElementTypes elementType)
  {
    MapElement element = null;
    if (mapElementPool.ContainsKey(elementType))
    {
      if (mapElementPool[elementType].Count == 0)
      {
        element = Create();
      }
      else
      {
        element = mapElementPool[elementType].Dequeue();
      }
    }
    else
    {
      element = Create();
    }

    if (element == null)
    {
      return null;
    }

    element.SetActive(true);
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
    var type = element.ElementType;
    if (mapElementPool.ContainsKey(type))
    {
      if (mapElementPool[type].Contains(element))
        return;
    }
    else
    {
      mapElementPool.Add(type, new Queue<MapElement>());
    }

    element.SetActive(false);

    if (mapElementPool[type].Count >= POOLING_MAX_SIZE)
    {
      GameManager.Instance.ScheduleForDestruction(element.gameObject);
    }
    else
    {
      mapElementPool[type].Enqueue(element);
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
    var type = obstacle.Type;
    if (obstaclePool.ContainsKey(type))
    {
      if(obstaclePool[type].Contains(obstacle))
        return;
    }
    else
    {
      obstaclePool.Add(type, new Queue<ObstacleBase>());
    }

    obstacle.SetActive(false);

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
      return null;

    obj.SetActive(true);
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
    if (mergeablePool.Contains(obj))
      return;

    obj.SetActive(false);
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
}
