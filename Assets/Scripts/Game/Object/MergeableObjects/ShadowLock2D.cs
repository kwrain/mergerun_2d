using UnityEngine;

public class ShadowLock2D : MonoBehaviour
{
  private readonly Vector3 OFFSET = new Vector3(0f, -0.88f, 0f);

  [SerializeField] private Transform target;
  void LateUpdate()
  {
    if (target == null)
      return;

    transform.position = target.position + OFFSET;
    // 이 오브젝트의 월드 회전값을 (0, 0, 0)으로 강제 고정합니다.
    // 부모(A)가 아무리 회전해도 이 코드가 덮어쓰게 됩니다.
    transform.rotation = Quaternion.identity;

    // 만약 Z축 회전만 막고 싶다면 아래 코드를 사용할 수도 있습니다.
    // transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, 0f);
  }
}