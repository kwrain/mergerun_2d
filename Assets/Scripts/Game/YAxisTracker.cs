using UnityEngine;

public class YAxisTracker : MonoBehaviour
{
  // 실제 플레이어 오브젝트를 연결할 변수
  public Transform playerTarget;

  // LateUpdate는 플레이어의 이동 로직(Update)이 모두 끝난 후에 호출됩니다.
  // 카메라 이동처럼 다른 오브젝트를 따라가는 로직에 사용하면 떨림(Jitter) 현상을 방지할 수 있습니다.
  void LateUpdate()
  {
    // 플레이어 타겟이 없으면 실행하지 않음
    if (playerTarget == null)
    {
      return;
    }

    // 이 오브젝트의 위치를 (자신의 X, 플레이어의 Y, 자신의 Z)로 설정합니다.
    transform.position = new Vector3(
        transform.position.x,      // X축은 현재 위치(고정값)를 그대로 사용
        playerTarget.position.y,   // Y축만 플레이어의 위치를 가져와 갱신
        transform.position.z       // Z축도 그대로 유지
    );
  }
}