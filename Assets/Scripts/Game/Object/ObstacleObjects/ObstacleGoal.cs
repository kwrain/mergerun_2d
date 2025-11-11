using DG.Tweening;
using UnityEngine;

public class ObstacleGoal : ObstacleBase
{
  [SerializeField] private int limitRelativeLevel = 0;

  [Header("Settings"), SerializeField] private Animator animator;

  public int LimitRelativeLevel => limitRelativeLevel;

  public Animator Animator => animator;
}
