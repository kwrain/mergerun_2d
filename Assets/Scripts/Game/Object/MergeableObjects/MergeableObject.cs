using Unity.VisualScripting;
using UnityEngine;

public class MergeableObject : MergeableBase
{
  protected Vector2 lastVelocity; // 충돌 직전 속도를 비교하기 위해 사용

  protected virtual void FixedUpdate()
  {
    lastVelocity = rb.linearVelocity;
  }

  protected override void Merge(MergeableObject other)
  {
    // 충돌 직전 속도(lastVelocity)의 크기를 비교
    if (lastVelocity.magnitude >= other.lastVelocity.magnitude)
    {
      base.Merge(this);
    }
    else
    {
      base.Merge(other);
    }
  }
}
