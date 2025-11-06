using DG.Tweening;
using UnityEngine;

public class ObstacleGoal : ObstacleBase
{
  [SerializeField] private int limitRelativeLevel = 0;

  public int LimitRelativeLevel => limitRelativeLevel;
}
