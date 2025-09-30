using UnityEngine;

public class ObstacleSpike : ObstacleBase
{
  [Tooltip("쿨타임")]
  [SerializeField] private float cooltime = 2.0f; // 2초 쿨다운
}
