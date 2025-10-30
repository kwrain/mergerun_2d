using UnityEngine;

/// <summary>
/// 무한모드에서만 동작.
/// </summary>
public class ObstacleGoal : ObstacleBase
{
  [SerializeField] private int limitRelativeLevel = 0;

  public int LimitRelativeLevel => limitRelativeLevel;
}
