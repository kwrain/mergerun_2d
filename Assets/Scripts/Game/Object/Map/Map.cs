using System.Collections.Generic;
using FAIRSTUDIOS.SODB.Utils;
using UnityEngine;

public class Map : MonoBehaviour
{
  [SerializeField] private CompositeCollider2D compositeCollider;

  [Header("[Settings]")]
  [Range(0.1f, 100f), SerializeField]
  [Tooltip("오브젝트와 겹치는 면적이 이 값(%) 미만으로 떨어지면 '벗어난 것'으로 간주합니다.")]
  private float overlapThresholdPercentage = 30.0f;

  [SerializeField]
  [Tooltip("벗어난 상태가 이 시간(초) 이상 지속되면 해당 오브젝트의 함수를 호출합니다.")]
  private float delayBeforeAction = 0.3f;

  [Space, Header("[Objects]"), SerializeField]
  private GenericDictionary<int, MergeableBase> trackedObjects = new();
  private GenericDictionary<int, MergeableBase> droppingObjects = new();
  private List<MergeableBase> objectsToDrop = new();
  private List<MergeableBase> objectsToTrack = new();
  private List<int> objectsToRemove = new();

  // 트리거 콜백에서 들어온 딕셔너리 변경 요청을 모아 두는 큐
  private List<MergeableBase> queuedAddTracked = new();
  private List<MergeableBase> queuedRemoveTracked = new();
  private List<MergeableBase> queuedRemoveDropping = new();

  // Update 실행 제어 플래그
  private bool isUpdateEnabled = true;

  // 컴포넌트를 미리 가져옵니다.
  protected virtual void Awake()
  {
    compositeCollider = GetComponent<CompositeCollider2D>();
    if (compositeCollider == null)
    {
      Debug.LogError("CompositeCollider2D 컴포넌트가 이 오브젝트에 없습니다!", gameObject);
    }
  }

  // 게임 시작 시, 이미 내부에 있는 오브젝트들을 스캔합니다.
  protected virtual void Start()
  {
    ScanMergeableObjects();
  }

  private void Update()
  {
    // Update가 비활성화되어 있으면 실행하지 않음
    if (!isUpdateEnabled)
      return;

    // 0. 임시 리스트 초기화
    objectsToDrop.Clear();
    objectsToTrack.Clear();
    objectsToRemove.Clear();

    // 0-1. 트리거 콜백에서 요청된 딕셔너리 변경을 여기서 한 번에 처리
    if (queuedRemoveTracked.Count > 0)
    {
      foreach (var obj in queuedRemoveTracked)
      {
        if (obj == null) continue;
        trackedObjects.Remove(obj.GetHashCode());
      }
      queuedRemoveTracked.Clear();
    }

    if (queuedRemoveDropping.Count > 0)
    {
      foreach (var obj in queuedRemoveDropping)
      {
        if (obj == null) continue;
        droppingObjects.Remove(obj.GetHashCode());
      }
      queuedRemoveDropping.Clear();
    }

    if (queuedAddTracked.Count > 0)
    {
      foreach (var obj in queuedAddTracked)
      {
        if (obj == null || !obj.gameObject.activeSelf) continue;
        var hash = obj.GetHashCode();
        trackedObjects[hash] = obj;
      }
      queuedAddTracked.Clear();
    }

    // 1. '추적 중인' 오브젝트(trackedObjects) 검사
    //    -> 안전 반경 조건을 만족하지 못하면 '떨어지는' 목록으로 이동 준비
    foreach (var kvp in trackedObjects)
    {
      var obj = kvp.Value;
      if (obj == null || !obj.gameObject.activeSelf)
      {
        objectsToRemove.Add(kvp.Key);
        continue;
      }

      var circle = obj.circleCollider;
      bool isSafe = IsInsideSafeCircle(circle);

      if (!isSafe)
      {
        // 겹침이 부족함 -> '떨어지는' 목록으로 이동
        objectsToDrop.Add(obj);
      }
      // (else: 안전 반경을 만족하면 'trackedObjects'에 그대로 둡니다)
    }

    // 1-1. 'trackedObjects'에서 제거할 오브젝트 처리
    foreach (var hash in objectsToRemove)
    {
      trackedObjects.Remove(hash);
    }
    objectsToRemove.Clear();

    // 2. '떨어지는 중인' 오브젝트(droppingObjects) 검사
    //    -> 안전 반경 조건을 다시 만족하면 '추적' 목록으로 복귀 준비
    foreach (var kvp in droppingObjects)
    {
      var obj = kvp.Value;
      if (obj == null || !obj.gameObject.activeSelf)
      {
        objectsToRemove.Add(kvp.Key);
        continue;
      }

      var circle = obj.circleCollider;
      bool isSafe = IsInsideSafeCircle(circle);

      if (isSafe)
      {
        // 겹침이 충분해짐 -> '추적' 목록으로 복귀
        objectsToTrack.Add(obj);
      }
      // (else: 여전히 안전 반경을 만족하지 못하면 'droppingObjects'에 그대로 둡니다)
    }

    // 2-1. 'droppingObjects'에서 제거할 오브젝트 처리
    foreach (var hash in objectsToRemove)
    {
      droppingObjects.Remove(hash);
    }


    // 3. 상태가 변경된 오브젝트들을 실제 딕셔너리에서 이동 처리

    // 3-1. '추적' -> '떨어짐'으로 이동
    foreach (var obj in objectsToDrop)
    {
      var hash = obj.GetHashCode();
      if (trackedObjects.Remove(hash)) // '추적'에서 제거하고
      {
        // 타이머를 시작하며 '떨어짐'에 추가
        obj.StartDropTimer(obj.IsPlayer ? delayBeforeAction : (delayBeforeAction * 0.5f));
        droppingObjects[hash] = obj;
      }
    }

    // 3-2. '떨어짐' -> '추적'으로 이동
    foreach (var obj in objectsToTrack)
    {
      var hash = obj.GetHashCode();
      if (droppingObjects.Remove(hash)) // '떨어짐'에서 제거하고
      {
        // 타이머를 멈추고 '추적'에 추가
        obj.StopDropTimer();
        trackedObjects[hash] = obj;
      }
    }
  }

  /// <summary>
  /// Update 함수의 실행 여부를 설정합니다.
  /// </summary>
  /// <param name="enabled">true면 Update 실행, false면 실행하지 않음</param>
  public void SetUpdateEnabled(bool enabled)
  {
    isUpdateEnabled = enabled;
  }

  [ContextMenu("ScanMergeableObjects")]
  public void ScanMergeableObjects()
  {
    trackedObjects.Clear();
    droppingObjects.Clear(); // [추가] 떨어지는 목록도 초기화

    List<Collider2D> results = new();
    ContactFilter2D filter = new();
    filter.SetLayerMask(LayerMask.GetMask("MergeableObject", "MergeablePlayer"));
    filter.useTriggers = true;

    int count = Physics2D.OverlapBox(compositeCollider.bounds.center, compositeCollider.bounds.size, 0f, filter, results);
    if (count > 0)
    {
      foreach (var collider in results)
      {
        if (collider.TryGetComponent<MergeableBase>(out var obj))
        {
          var hash = obj.GetHashCode();
          if (!trackedObjects.ContainsKey(hash))
          {
            // 스캔 시점에는 겹침 비율을 모르므로 일단 'trackedObjects'에 넣습니다.
            // 다음 Update 프레임에서 겹침 비율을 계산하고 
            // 필요시 'droppingObjects'로 이동시킬 것입니다.
            trackedObjects.Add(hash, obj);
          }
        }
      }
    }
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.TryGetComponent<MergeableBase>(out var obj))
    {
      var hash = obj.GetHashCode();

      // [수정] '떨어지는' 중이었다면, 상태를 되돌리도록 큐에 추가합니다.
      if (droppingObjects.ContainsKey(hash))
      {
        obj.StopDropTimer();
        queuedRemoveDropping.Add(obj);
      }

      // '추적' 목록에 추가하도록 큐에 등록합니다.
      queuedAddTracked.Add(obj);
    }
  }

  private void OnTriggerExit2D(Collider2D other)
  {
    if (other.TryGetComponent<MergeableBase>(out var obj))
    {
      var hash = obj.GetHashCode();

      // [수정] 두 목록 모두에서 제거하도록 큐에 추가합니다.
      queuedRemoveTracked.Add(obj);
      queuedRemoveDropping.Add(obj);
    }
  }

  /// <summary>
  /// CircleCollider의 중심에서 overlapThresholdPercentage 만큼 작은 "안전 반경"을 잡고,
  /// 그 둘레를 이루는 점들이 모두 맵 안에 있는지 검사합니다.
  /// 하나라도 맵 밖이면 "겹침 부족(나감)"으로 간주합니다.
  /// </summary>
  /// <param name="circle">검사할 원형 콜라이더</param>
  /// <param name="samples">둘레 샘플 개수</param>
  private bool IsInsideSafeCircle(CircleCollider2D circle, int samples = 16)
  {
    if (circle == null || compositeCollider == null) return false;
    if (samples <= 0) samples = 1;

    // 월드 기준 중심/반지름
    Bounds bounds = circle.bounds;
    Vector2 center = bounds.center;
    float radius = Mathf.Max(bounds.extents.x, bounds.extents.y);

    // overlapThresholdPercentage(0~100)를 "겹침이 이 값(%) 미만이면 벗어난 것"으로 사용하므로
    // 안전 반경 비율은 (1 - threshold) 로 해석합니다.
    // 예) threshold = 30 -> 반지름의 70% 지점까지는 반드시 맵 안에 있어야 한다.
    float rate = 1f - Mathf.Clamp01(overlapThresholdPercentage / 100f);
    float safeRadius = radius * rate;

    for (int i = 0; i < samples; i++)
    {
      float angle = (Mathf.PI * 2f) * i / samples;
      Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
      Vector2 p = center + dir * safeRadius;

      // 안전 반경 둘레 중 하나라도 맵 밖이면 "안전하지 않다"
      if (!compositeCollider.OverlapPoint(p))
      {
        return false;
      }
    }

    // 안전 반경 둘레 전체가 맵 안
    return true;
  }
}