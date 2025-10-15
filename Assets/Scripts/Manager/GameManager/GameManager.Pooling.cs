
using System.Collections.Generic;
using UnityEngine;

public partial class GameManager
{
  private const int POOLING_MAX_SIZE = 30;

  /// <summary>
  /// 풀링 시 제거 대상 처리를 위한 큐
  /// </summary>
  private Queue<GameObject> destructionQueue = new();

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
}
