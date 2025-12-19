using DG.Tweening;
using TMPro;
using UnityEngine;

public class ObstacleGoal : ObstacleBase
{
  [SerializeField] private int limitRelativeLevel = 0;

  [Header("Settings"), SerializeField] private SpriteRenderer spriteRenderer;
  [SerializeField] private TextMeshPro textMeshPro;

  private Sequence currentAnimationSequence;
  private Vector3 textInitialScale;
  private Vector2 textInitialAnchoredPosition;
  private Vector3 textInitialLocalPosition;

  public int LimitRelativeLevel => limitRelativeLevel;
  public int LimitLevel { get; private set; }
  public bool IsChecked { get; set; }

  public static Color GetObstacleGoalColor(int level)
  {
    // 11보다 큰 경우 1부터 다시 시작 (12 -> 1, 13 -> 2, ...)
    int normalizedLevel = ((level - 1) % 11) + 1;

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
    
    // Text 초기값 저장
    if (textMeshPro != null)
    {
      textInitialScale = textMeshPro.transform.localScale;
      textInitialLocalPosition = textMeshPro.transform.localPosition;
      
      var rectTransform = textMeshPro.GetComponent<RectTransform>();
      if (rectTransform != null)
      {
        textInitialAnchoredPosition = rectTransform.anchoredPosition;
      }
    }
  }

  private void OnEnable()
  {
    // Idle 애니메이션 시작
    PlayIdleAnimation();
  }

  private void OnDisable()
  {
    IsChecked = false;
    
    // 진행 중인 애니메이션 정리
    if (currentAnimationSequence != null && currentAnimationSequence.IsActive())
    {
      currentAnimationSequence.Kill();
      currentAnimationSequence = null;
    }
  }


  public override void SetData(StageDataTable.ObstacleData obstacleData)
  {
    base.SetData(obstacleData);

    limitRelativeLevel = obstacleData.limitRelativeLevel;

    UpdateLevel();
  }

  public void SetAnimation(string name)
  {
    // 진행 중인 애니메이션 정리
    if (currentAnimationSequence != null && currentAnimationSequence.IsActive())
    {
      currentAnimationSequence.Kill();
      currentAnimationSequence = null;
    }

    switch (name)
    {
      case "idle":
        PlayIdleAnimation();
        break;
      case "pass":
        PlayPassAnimation();
        break;
      case "fail":
        PlayFailAnimation();
        break;
    }
  }

  private void PlayIdleAnimation()
  {
    // Idle: SpriteRenderer alpha 0.8, Text scale (1,1,1) - 루프
    if (spriteRenderer != null)
    {
      Color color = spriteRenderer.color;
      color.a = 0.8f;
      spriteRenderer.color = color;
    }

    if (textMeshPro != null)
    {
      textMeshPro.transform.localScale = textInitialScale;
    }
  }

  private void PlayPassAnimation()
  {
    // Pass: SpriteRenderer alpha 깜빡임 (0.8 -> 0.4 -> 0.8 반복), Text scale 1 -> 1.1 -> 0
    // 총 1초, alpha는 0.8->0.4->0.8->0.4->0.8->0.4->0.8 (7단계)
    currentAnimationSequence = DOTween.Sequence();

    // SpriteRenderer alpha 깜빡임
    if (spriteRenderer != null)
    {
      currentAnimationSequence.Append(spriteRenderer.DOFade(0.4f, 0.1667f));
      currentAnimationSequence.Append(spriteRenderer.DOFade(0.8f, 0.1667f));
      currentAnimationSequence.Append(spriteRenderer.DOFade(0.4f, 0.1667f));
      currentAnimationSequence.Append(spriteRenderer.DOFade(0.8f, 0.1667f));
      currentAnimationSequence.Append(spriteRenderer.DOFade(0.4f, 0.1667f));
      currentAnimationSequence.Append(spriteRenderer.DOFade(0.8f, 0.1667f));
    }

    // Text scale 애니메이션
    if (textMeshPro != null)
    {
      currentAnimationSequence.Insert(0f, textMeshPro.transform.DOScale(textInitialScale * 1.1f, 0.25f));
      currentAnimationSequence.Insert(0.4167f, textMeshPro.transform.DOScale(Vector3.zero, 0.1667f));
    }
  }

  private void PlayFailAnimation()
  {
    // Fail: SpriteRenderer alpha 0.8->1, color 빨간색->흰색, Text 위치 흔들림
    // 총 0.25초
    currentAnimationSequence = DOTween.Sequence();

    if (spriteRenderer != null)
    {
      // Alpha 애니메이션: 0.8 -> 1 (0.083초에 도달)
      currentAnimationSequence.Append(spriteRenderer.DOFade(1f, 0.083f));
      
      // Color 애니메이션: 빨간색 (0.937, 0.278, 0.109) -> 흰색 (1, 1, 1)
      Color startColor = new Color(0.9372549f, 0.2784314f, 0.10980392f, 1f);
      Color endColor = Color.white;
      currentAnimationSequence.Join(spriteRenderer.DOColor(endColor, 0.083f).From(startColor));
    }

    // Text 위치 흔들림 (0 -> 0.1 -> 0 -> 0.1 -> 0)
    if (textMeshPro != null)
    {
      var rectTransform = textMeshPro.GetComponent<RectTransform>();
      if (rectTransform != null)
      {
        // UI TextMeshPro (RectTransform 사용)
        Vector2 originalPos = textInitialAnchoredPosition;
        currentAnimationSequence.Insert(0f, rectTransform.DOAnchorPosX(originalPos.x + 0.1f, 0.033f));
        currentAnimationSequence.Insert(0.033f, rectTransform.DOAnchorPosX(originalPos.x, 0.033f));
        currentAnimationSequence.Insert(0.0667f, rectTransform.DOAnchorPosX(originalPos.x + 0.1f, 0.033f));
        currentAnimationSequence.Insert(0.1f, rectTransform.DOAnchorPosX(originalPos.x, 0.133f));
      }
      else
      {
        // World Space TextMeshPro (Transform 사용)
        Vector3 originalPos = textInitialLocalPosition;
        currentAnimationSequence.Insert(0f, textMeshPro.transform.DOLocalMoveX(originalPos.x + 0.1f, 0.033f));
        currentAnimationSequence.Insert(0.033f, textMeshPro.transform.DOLocalMoveX(originalPos.x, 0.033f));
        currentAnimationSequence.Insert(0.0667f, textMeshPro.transform.DOLocalMoveX(originalPos.x + 0.1f, 0.033f));
        currentAnimationSequence.Insert(0.1f, textMeshPro.transform.DOLocalMoveX(originalPos.x, 0.133f));
      }
    }
  }

  public void UpdateLevel()
  {
    if (!Application.isPlaying)
      return;

    if (StageManager.Instance.Infinity)
    {
      LimitLevel = StageManager.Instance.Player.Level + limitRelativeLevel;
      textMeshPro.text = SOManager.Instance.GameDataTable.PowerOfTwoString(LimitLevel);
      
      // 색상 업데이트 시 alpha 값 유지
      if (spriteRenderer != null)
      {
        Color newColor = GetObstacleGoalColor(LimitLevel);
        newColor.a = spriteRenderer.color.a; // 기존 alpha 유지
        spriteRenderer.color = newColor;
      }
    }
    else
    {
      textMeshPro.text = "GOAL!";
    }
  }
}
