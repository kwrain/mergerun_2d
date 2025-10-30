using UnityEngine;

public class VerticalLoopingBackground : MonoBehaviour
{
  [Tooltip("시차 속도 (0 = 카메라에 고정, 1 = 월드에 고정)")]
  [Range(0f, 1f)]
  public float parallaxSpeed = 1.0f;

  private Transform cameraTransform;
  private Transform[] backgroundSprites;
  private float spriteHeight;
  private Vector3 lastCameraPosition;

  private void Start()
  {
    cameraTransform = Camera.main.transform;
    lastCameraPosition = cameraTransform.position;

    // 1. 자식 오브젝트(배경 스프라이트 3개)를 배열로 가져옵니다.
    backgroundSprites = new Transform[transform.childCount];
    for (int i = 0; i < transform.childCount; i++)
    {
      backgroundSprites[i] = transform.GetChild(i);
    }

    // 2. 스프라이트 높이를 첫 번째 자식 기준으로 계산합니다.
    // (모든 스프라이트의 크기가 같다고 가정)
    spriteHeight = backgroundSprites[0].GetComponent<SpriteRenderer>().bounds.size.y;

    // 3. Y축 기준으로 스프라이트를 정렬합니다. (낮은 순 -> 높은 순)
    System.Array.Sort(backgroundSprites, (a, b) => a.position.y.CompareTo(b.position.y));

    // 4. 정렬된 순서대로 빈틈없이 재배치합니다. (초기 설정)
    for (int i = 1; i < backgroundSprites.Length; i++)
    {
      backgroundSprites[i].position = new Vector3(
          backgroundSprites[0].position.x,
          backgroundSprites[0].position.y + i * spriteHeight,
          backgroundSprites[0].position.z);
    }
  }

  private void LateUpdate()
  {
    // 카메라가 위로 이동한 거리 (deltaY는 양수)
    float deltaY = cameraTransform.position.y - lastCameraPosition.y;

    // 카메라가 위로 이동했을 때만(deltaY > 0) 처리
    if (deltaY > 0)
    {
      // 1. Parallax 적용:
      // 부모 오브젝트(this.transform)를 카메라 이동의 반대 방향(아래)으로 이동시킵니다.
      transform.position -= new Vector3(0, deltaY * parallaxSpeed, 0);

      // 2. 재배치 (Leapfrogging) 로직
      // 카메라의 하단 시야 경계를 계산합니다.
      float cameraBottomEdge = cameraTransform.position.y - Camera.main.orthographicSize;

      // 3. 가장 아래에 있는 스프라이트를 찾습니다.
      Transform bottomSprite = backgroundSprites[0]; // 임시로 첫 번째를 할당
      foreach (Transform sprite in backgroundSprites)
      {
        if (sprite.position.y < bottomSprite.position.y)
        {
          bottomSprite = sprite;
        }
      }

      // 4. 가장 아래 스프라이트가 화면 밖으로 나갔는지 확인
      // (스프라이트의 상단 경계가 카메라의 하단 경계보다 아래에 있는지)
      float bottomSpriteTopEdge = bottomSprite.position.y + (spriteHeight / 2f);

      if (bottomSpriteTopEdge < cameraBottomEdge)
      {
        // 5. 가장 위에 있는 스프라이트를 찾습니다.
        Transform topSprite = backgroundSprites[0]; // 임시로 첫 번째를 할당
        foreach (Transform sprite in backgroundSprites)
        {
          if (sprite.position.y > topSprite.position.y)
          {
            topSprite = sprite;
          }
        }

        // 6. 가장 아래(화면 밖)에 있던 스프라이트를
        //    가장 위 스프라이트의 바로 위로 순간이동시킵니다.
        bottomSprite.position = new Vector3(
            topSprite.position.x,
            topSprite.position.y + spriteHeight,
            topSprite.position.z);
      }
    }

    // 마지막 카메라 위치를 갱신합니다.
    lastCameraPosition = cameraTransform.position;
  }
}