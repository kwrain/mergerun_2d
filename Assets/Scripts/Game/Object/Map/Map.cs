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
  private float delayBeforeAction = 1.5f;

  [Space, Header("[Elements]"), SerializeField]
  private List<MergeableBase> grounds = new();
  [SerializeField]
  private List<MergeableBase> bridges = new();

  [Space, Header("[Objects]"), SerializeField]
  private GenericDictionary<int, MergeableBase> trackedObjects = new();

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
    var objects = new List<MergeableBase>(trackedObjects.Values);
    foreach (var obj in objects)
    {
      if (obj == null || !obj.gameObject.activeSelf)
      {
        trackedObjects.Remove(obj.GetHashCode());
        continue;
      }

      var circle = obj.circleCollider;
      var currentOverlap = CalculateOverlapPercentage(circle);
      if (currentOverlap < overlapThresholdPercentage)
      {
        obj.StartDropTimer(delayBeforeAction);
      }
      else
      {
        obj.StopDropTimer();
      }
    }
  }

  [ContextMenu("ScanMergeableObjects")]
  private void ScanMergeableObjects()
  {
    trackedObjects.Clear();

    List<Collider2D> results = new();
    ContactFilter2D filter = new();
    filter.SetLayerMask(LayerMask.GetMask("MergeableObject"));
    filter.useTriggers = true;
    // 자신의 콜라이더 영역과 겹치는 모든 콜라이더를 찾아 results 리스트에 담습니다.
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
            trackedObjects.Add(hash, obj);
          }
        }
      }
    }
  }

  // (이하 코드는 이전과 동일합니다)
  private void OnTriggerEnter2D(Collider2D other)
  {
    if (other.TryGetComponent<MergeableBase>(out var obj))
    {
      var hash = obj.GetHashCode();
      if (!trackedObjects.ContainsKey(hash))
      {
        trackedObjects.Add(hash, obj);
      }
    }
  }

  private void OnTriggerExit2D(Collider2D other)
  {
    if (other.TryGetComponent<MergeableBase>(out var obj))
    {
      var hash = obj.GetHashCode();
      trackedObjects.Remove(hash);
    }
  }

  private float CalculateOverlapPercentage(CircleCollider2D circle)
  {
    Bounds circleBounds = circle.bounds;
    Bounds compositeBounds = compositeCollider.bounds;

    float overlapMinX = Mathf.Max(circleBounds.min.x, compositeBounds.min.x);
    float overlapMinY = Mathf.Max(circleBounds.min.y, compositeBounds.min.y);
    float overlapMaxX = Mathf.Min(circleBounds.max.x, compositeBounds.max.x);
    float overlapMaxY = Mathf.Min(circleBounds.max.y, compositeBounds.max.y);

    float overlapWidth = overlapMaxX - overlapMinX;
    float overlapHeight = overlapMaxY - overlapMinY;

    if (overlapWidth <= 0 || overlapHeight <= 0) return 0f;

    float overlapArea = overlapWidth * overlapHeight;
    float circleArea = circleBounds.size.x * circleBounds.size.y;

    if (circleArea == 0) return 0f;

    return (overlapArea / circleArea) * 100f;
  }
}