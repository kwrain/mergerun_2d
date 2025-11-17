using UnityEngine;

public class InfiniteLoopingBackground : MonoBehaviour
{
    [Tooltip("시차 속도 (0 = 카메라에 고정, 1 = 월드에 고정)")]
    [Range(0f, 1f)]
    public float parallaxSpeed = 1.0f;

    [Tooltip("그리드 크기 (홀수 권장: 3, 5 등)")]
    public int gridSize = 3;

    private Transform cameraTransform;
    private Transform[,] backgroundGrid;
    private Vector2 spriteSize;
    private Vector3 lastCameraPosition;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;

        // 첫 번째 자식 스프라이트를 기준으로 크기 계산
        if (transform.childCount == 0)
        {
            Debug.LogError("배경 스프라이트가 자식으로 없습니다!");
            return;
        }

        SpriteRenderer spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("자식 오브젝트에 SpriteRenderer가 없습니다!");
            return;
        }

        spriteSize = spriteRenderer.bounds.size;

        // 그리드 생성
        CreateBackgroundGrid();
    }

    private void CreateBackgroundGrid()
    {
        backgroundGrid = new Transform[gridSize, gridSize];

        // 기존 자식 오브젝트가 있다면 첫 번째 것을 템플릿으로 사용
        Transform template = transform.GetChild(0);

        int centerIndex = gridSize / 2;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Transform tile;

                // 첫 번째 타일은 기존 것 사용, 나머지는 복제
                if (x == centerIndex && y == centerIndex)
                {
                    tile = template;
                }
                else
                {
                    tile = Instantiate(template, transform);
                }

                // 그리드 위치에 배치
                float posX = (x - centerIndex) * spriteSize.x;
                float posY = (y - centerIndex) * spriteSize.y;
                tile.localPosition = new Vector3(posX, posY, 0);

                backgroundGrid[x, y] = tile;
            }
        }

        // 중앙 타일을 카메라 위치에 맞춤
        Vector3 cameraPos = cameraTransform.position;
        transform.position = new Vector3(
            cameraPos.x - backgroundGrid[centerIndex, centerIndex].localPosition.x,
            cameraPos.y - backgroundGrid[centerIndex, centerIndex].localPosition.y,
            transform.position.z
        );
    }

    private void LateUpdate()
    {
        if (backgroundGrid == null)
            return;

        // 카메라 이동 델타 계산
        Vector3 deltaPosition = cameraTransform.position - lastCameraPosition;

        // Parallax 효과 적용
        if (deltaPosition.magnitude > 0.001f)
        {
            transform.position -= new Vector3(
                deltaPosition.x * parallaxSpeed,
                deltaPosition.y * parallaxSpeed,
                0
            );
        }

        // 타일 재배치 확인
        CheckAndRepositionTiles();

        // 마지막 카메라 위치 갱신
        lastCameraPosition = cameraTransform.position;
    }

    private void CheckAndRepositionTiles()
    {
        Vector3 cameraPos = cameraTransform.position;
        Camera mainCamera = Camera.main;

        // 카메라 뷰포트 크기 계산
        float cameraHeight = mainCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        // 각 타일의 월드 위치 확인
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                Transform tile = backgroundGrid[x, y];
                Vector3 tileWorldPos = tile.position;

                // X축 체크 및 재배치
                float distanceX = tileWorldPos.x - cameraPos.x;
                if (distanceX < -(spriteSize.x * gridSize / 2f))
                {
                    // 왼쪽으로 벗어남 -> 오른쪽으로 이동
                    tile.position = new Vector3(
                        tile.position.x + spriteSize.x * gridSize,
                        tile.position.y,
                        tile.position.z
                    );
                }
                else if (distanceX > (spriteSize.x * gridSize / 2f))
                {
                    // 오른쪽으로 벗어남 -> 왼쪽으로 이동
                    tile.position = new Vector3(
                        tile.position.x - spriteSize.x * gridSize,
                        tile.position.y,
                        tile.position.z
                    );
                }

                // Y축 체크 및 재배치
                float distanceY = tileWorldPos.y - cameraPos.y;
                if (distanceY < -(spriteSize.y * gridSize / 2f))
                {
                    // 아래로 벗어남 -> 위로 이동
                    tile.position = new Vector3(
                        tile.position.x,
                        tile.position.y + spriteSize.y * gridSize,
                        tile.position.z
                    );
                }
                else if (distanceY > (spriteSize.y * gridSize / 2f))
                {
                    // 위로 벗어남 -> 아래로 이동
                    tile.position = new Vector3(
                        tile.position.x,
                        tile.position.y - spriteSize.y * gridSize,
                        tile.position.z
                    );
                }
            }
        }
    }

    // 디버그용 - 씬 뷰에서 그리드 표시
    private void OnDrawGizmos()
    {
        if (backgroundGrid == null)
            return;

        Gizmos.color = Color.yellow;
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                if (backgroundGrid[x, y] != null)
                {
                    Vector3 pos = backgroundGrid[x, y].position;
                    Gizmos.DrawWireCube(pos, new Vector3(spriteSize.x, spriteSize.y, 0.1f));
                }
            }
        }

        // 카메라 위치 표시
        if (cameraTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(cameraTransform.position, 1f);
        }
    }
}

