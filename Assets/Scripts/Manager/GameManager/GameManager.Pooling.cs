
using System.Collections.Generic;

public partial class GameManager
{
  private const int POOLING_MAX_SIZE = 30;

  Queue<MergeableObject> mergeableObjectPool;

  private MergeableObject CreateMergeableObject(int grade = 0)
  {
    // ResourceManager.Instance.Load

    return null;
  }

  public MergeableObject PeekBuildingInPool()
  {
    MergeableObject obj = null;
    // 풀에서 건물 가져오기
    if (mergeableObjectPool.Count == 0)
    {
      // create
      // 생성 시 생성 위치 지정 필수.
    }
    else
    {
      obj = mergeableObjectPool.Dequeue();
    }

    if (obj == null)
    {
      // error 
      return null;
    }

    obj.SetActive(true);

    return obj;
  }

  public void PushBuildingInPool(MergeableObject obj)
  {
    obj.SetActive(false);

    // nedd check OnUpdate object;
    RemoveUpdateModel(obj);

    // UI 처리 (안전한 널 체크)
    // UIManager.Instance?.HideHUD(building);

    if (mergeableObjectPool.Count >= POOLING_MAX_SIZE)
    {
      ScheduleForDestruction(obj.gameObject);
      return;
    }

    mergeableObjectPool.Enqueue(obj);
  }
}
