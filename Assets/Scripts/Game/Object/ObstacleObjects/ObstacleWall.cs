using UnityEngine;

/// <summary>
/// 무한모드에서만 동작.
/// </summary>
public class ObstacleWall : ObstacleBase
{
  [Tooltip("쿨타임")]
  [SerializeField] private uint limitRelativeGrade = 0;

  public uint LimitRelativeGrade => limitRelativeGrade;
}
