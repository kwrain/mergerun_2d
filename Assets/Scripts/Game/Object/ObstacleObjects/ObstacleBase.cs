using System.Collections;
using UnityEngine;

public class ObstacleBase : MonoBehaviour
{
  public enum ObstacleTypes
  {
    None = -1,

    Spike = 0,
    Wall,

    Count
  }

  [SerializeField] private ObstacleTypes type = ObstacleTypes.None;
  [SerializeField] private BoxCollider2D boxCollider;

  [Header("[Settings]")]
  [SerializeField, Range(0.1f, 100f)]
  [Tooltip("충돌 감지 비율")]
  private float overlapPercentage = 50.0f;

  public ObstacleTypes Type => type;
  
  protected virtual void Awake()
  {
    // 시작할 때 자신의 BoxCollider2D 컴포넌트를 가져옵니다.
    if (boxCollider == null)
    {
      boxCollider = GetComponent<BoxCollider2D>();
      if (boxCollider == null)
      {
        Debug.LogError("BoxCollider2D 컴포넌트가 이 오브젝트에 없습니다!", gameObject);
      }
    }
  }

  // 다른 콜라이더와 겹쳐있는 동안 매 프레임 호출됩니다.
  // 두 오브젝트 중 하나 이상에 Rigidbody2D가 있고, 하나 이상 isTrigger가 체크되어야 합니다.
  private void OnTriggerStay2D(Collider2D other)
  {
    Debug.Log("dmdkdkdkdkdk");
    // 겹침 비율을 계산합니다.
    float overlapPercentage = CalculateOverlapPercentage(other);
    // 계산된 비율이 우리가 설정한 임계값을 넘었는지 확인합니다.
    if (overlapPercentage >= this.overlapPercentage)
    {
      var obj = other.gameObject.GetComponent<MergeablePlayer>();
      if (obj != null)
      {
        obj.OnCollisionObstacles(this);
      }
    }
  }

  /// 두 콜라이더의 경계 상자(Bounds)를 기준으로 겹친 영역의 비율을 계산합니다.
  /// </summary>
  /// <returns>BoxCollider2D 영역 대비 겹친 영역의 비율(%)</returns>
  private float CalculateOverlapPercentage(Collider2D target)
  {
    if (boxCollider == null)
    {
      return 0f;
    }

    // 각 콜라이더의 월드 좌표 기준 경계 상자(Bounds)를 가져옵니다.
    Bounds boxBounds = boxCollider.bounds;
    Bounds circleBounds = target.bounds;

    // 두 경계 상자가 겹치는 영역을 계산합니다.
    float overlapMinX = Mathf.Max(boxBounds.min.x, circleBounds.min.x);
    float overlapMinY = Mathf.Max(boxBounds.min.y, circleBounds.min.y);
    float overlapMaxX = Mathf.Min(boxBounds.max.x, circleBounds.max.x);
    float overlapMaxY = Mathf.Min(boxBounds.max.y, circleBounds.max.y);

    // 겹치는 영역의 너비와 높이를 계산합니다.
    float overlapWidth = overlapMaxX - overlapMinX;
    float overlapHeight = overlapMaxY - overlapMinY;

    // 겹치는 영역이 실제로 존재하는지 확인합니다. (너비나 높이가 0 이하이면 겹치지 않음)
    if (overlapWidth <= 0 || overlapHeight <= 0)
    {
      return 0f;
    }

    // 겹친 영역의 면적과 자신의(박스) 전체 면적을 계산합니다.
    float overlapArea = overlapWidth * overlapHeight;
    float boxArea = boxBounds.size.x * boxBounds.size.y;

    // 0으로 나누는 것을 방지합니다.
    if (boxArea == 0)
    {
      return 0f;
    }

    // (겹친 면적 / 박스 전체 면적) * 100 을 하여 백분율을 반환합니다.
    return overlapArea / boxArea * 100f;
  }
}
