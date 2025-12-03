using DG.Tweening;
using TMPro;
using UnityEngine;

public class ObstacleGoal : ObstacleBase
{
  [SerializeField] private int limitRelativeLevel = 0;

  [Header("Settings"), SerializeField] private Animator animator;
  [SerializeField] private SpriteRenderer spriteRenderer;
  [SerializeField] private TextMeshPro textMeshPro;

  public Animator Animator => animator;

  public int LimitRelativeLevel => limitRelativeLevel;
  public bool IsChecked { get; set; }

  public static Color GetObstacleGoalColor(int level)
  {
    // 12보다 큰 경우 1부터 다시 시작 (12 -> 1, 13 -> 2, ...)
    int normalizedLevel = ((level - 1) % 12) + 1;

    string hex = normalizedLevel switch
    {
      1 => "EF471C",// 2 : #EF471C
      2 => "FF6B32",// 4 : #FF6B32
      3 => "A04EC0",// 8 : #A04EC0
      4 => "FF9102",// 16 : #FFB902
      5 => "FC8C1C",// 32 : #FC8C1C
      6 => "EF471C",// 64 : #EF471C
      7 => "CEDA0D",// 128 : #CEDA0D
      8 => "FF8661",// 256 : #FF8661
      9 => "FF9102",// 512 : #F5BD26
      10 => "A2D03C",// 1024 : #A2D03C
      11 => "49A837",// 2048 : #49A837
      _ => null,
    };

    return hex.ToColorFromHex();
  }

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

    if (StageManager.Instance.Infinity)
    {
      UpdateLevel();
    }
  }

  public void UpdateLevel()
  {
    var level = StageManager.Instance.Player.Level + limitRelativeLevel;
    textMeshPro.text = SOManager.Instance.GameDataTable.PowerOfTwoString(level);
    spriteRenderer.color = GetObstacleGoalColor(level);
  }
}
