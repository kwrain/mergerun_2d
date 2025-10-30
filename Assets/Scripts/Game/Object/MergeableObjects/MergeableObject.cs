using Unity.VisualScripting;
using UnityEngine;

public class MergeableObject : MergeableBase
{
  protected Vector2 lastVelocity; // 충돌 직전 속도를 비교하기 위해 사용

  public bool IsMergeable { get; set; }

  protected virtual void FixedUpdate()
  {
    lastVelocity = rb.linearVelocity;
  }

  protected override void Merge(MergeableObject other)
  {
    // 충돌 직전 속도(lastVelocity)의 크기를 비교
    // Debug.Log($"{GetHashCode()}: {lastVelocity.magnitude} / {other.GetHashCode()} : {other.lastVelocity.magnitude}");
    if (lastVelocity.Abs().magnitude >= other.lastVelocity.Abs().magnitude)
    {
      // base.Merge(this);
      other.Merge(this);
    }
    else
    {
      base.Merge(other);
    }
  }

  protected override void Drop()
  {
    base.Drop();

    StageManager.Instance.PushMergeableInPool(this);
  }
}
