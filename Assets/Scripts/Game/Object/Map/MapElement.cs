using System.Collections.Generic;
using UnityEngine;
using static StageDataTable;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapElement : MonoBehaviour
{
  public enum MapElementTypes
  {
    None = -1,

    Ground = 0,
    GroundDiagonal,

    Bridge = 10,

    Count
  }


  [SerializeField] private MapElementTypes elementType;

  public PolygonCollider2D polygonCollider;
  public SpriteRenderer spriteRenderer;

  protected MapData Data { get; private set; }

  public MapElementTypes ElementType => elementType;

  private void Awake()
  {

  }

  /// <summary>
  /// SpriteRenderer의 size가 변경되었을 때 실행되는 함수
  /// </summary>
  /// <param name="newSize">새로운 SpriteRenderer의 size 값</param>
  public void HandleSpriteSizeChanged(Vector2 newSize)
  {
    // Debug.Log($"✅ SpriteRenderer의 Size가 변경되었습니다! 새 Size: {newSize}");

    UpdateCollider();
  }

  public virtual void SetData(MapData mapData)
  {
    Data = mapData;

    transform.position = mapData.position;
    transform.localScale = mapData.scale;

    spriteRenderer.size = mapData.size;
    UpdateCollider();
  }

  [ContextMenu("UpdateCollider")]
  protected virtual void UpdateCollider()
  {
    if (spriteRenderer.sprite == null) return;

    var sprite = spriteRenderer.sprite;
    Vector2 spriteSize = sprite.bounds.size;
    Vector2 scale = new Vector2(
        spriteRenderer.size.x / spriteSize.x,
        spriteRenderer.size.y / spriteSize.y
    );

    polygonCollider.pathCount = sprite.GetPhysicsShapeCount();

    List<Vector2> path = new List<Vector2>();
    for (int i = 0; i < polygonCollider.pathCount; i++)
    {
      sprite.GetPhysicsShape(i, path);

      for (int p = 0; p < path.Count; p++)
        path[p] = new Vector2(path[p].x * scale.x, path[p].y * scale.y);

      polygonCollider.SetPath(i, path);
    }
  }
}

#if UNITY_EDITOR
/// <summary>
/// SizeChangeMonitor 컴포넌트에 대한 Custom Editor
/// SpriteRenderer의 Size 변경을 감지하고 HandleSpriteSizeChanged 함수를 호출합니다.
/// </summary>
[CustomEditor(typeof(MapElement), true)]
public class MapElementEditor : Editor
{
  // 타겟 MonoBehaviour 인스턴스
  private MapElement monitorTarget;

  // SpriteRenderer 컴포넌트 인스턴스
  private SpriteRenderer cachedRenderer;

  // 이전 size 값을 저장하여 변경 감지
  private Vector2 previousSize;

  private void OnEnable()
  {
    // 타겟 MonoBehaviour 인스턴스 가져오기
    monitorTarget = (MapElement)target;

    // SpriteRenderer 컴포넌트 인스턴스 캐싱
    cachedRenderer = monitorTarget.spriteRenderer;
    if (cachedRenderer == null)
    {
      cachedRenderer = monitorTarget.GetComponent<SpriteRenderer>();
    }

    // 초기 size 값 저장
    if (cachedRenderer != null)
    {
      previousSize = cachedRenderer.size;
    }

    // EditorApplication.update를 통해 주기적으로 size 변경 체크
    EditorApplication.update += CheckSizeChange;
  }

  private void OnDisable()
  {
    // 메모리 누수 방지를 위해 update 콜백 제거
    EditorApplication.update -= CheckSizeChange;
  }

  /// <summary>
  /// SpriteRenderer의 size 변경을 주기적으로 체크하는 함수
  /// </summary>
  private void CheckSizeChange()
  {
    if (cachedRenderer == null || monitorTarget == null)
      return;

    Vector2 currentSize = cachedRenderer.size;
    
    // size가 변경되었는지 확인
    if (previousSize != currentSize)
    {
      previousSize = currentSize;
      monitorTarget.HandleSpriteSizeChanged(currentSize);
      
      // 변경 사항을 Unity에 알림
      EditorUtility.SetDirty(monitorTarget);
    }
  }

  public override void OnInspectorGUI()
  {
    // 1. 기본 인스펙터 UI 그리기
    DrawDefaultInspector();

    // 2. SpriteRenderer 컴포넌트 확인
    if (cachedRenderer == null)
    {
      EditorGUILayout.HelpBox("SpriteRenderer 컴포넌트가 필요합니다.", MessageType.Warning);
      return;
    }

    // 3. OnInspectorGUI에서도 size 변경 체크 (추가 안전장치)
    Vector2 currentSize = cachedRenderer.size;
    if (previousSize != currentSize)
    {
      previousSize = currentSize;
      monitorTarget.HandleSpriteSizeChanged(currentSize);
    }
  }
}

#endif