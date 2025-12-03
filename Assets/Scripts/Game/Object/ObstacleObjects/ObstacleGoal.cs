using DG.Tweening;
using UnityEngine;

public class ObstacleGoal : ObstacleBase
{
  [SerializeField] private int limitRelativeLevel = 0;

  [Header("Settings"), SerializeField] private Animator animator;

  public Animator Animator => animator;

  public int LimitRelativeLevel => limitRelativeLevel;
  public bool IsChecked { get; set; }

  protected override void Awake()
  {
    base.Awake();

    IsChecked = false;
  }

  private void OnDisable()
  {
    IsChecked = false;
  }

  public override void SetData(StageDataTable.ObstacleData obstacleData)
  {
    base.SetData(obstacleData);

    limitRelativeLevel = obstacleData.limitRelativeLevel;
  }
}
