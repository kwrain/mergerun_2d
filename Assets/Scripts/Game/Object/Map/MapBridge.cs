using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapBridge : MapElement
{
}

#if UNITY_EDITOR
[CustomEditor(typeof(MapBridge))]
public class MapBridgeEditor : Editor
{
  private MapBridge mapBridge;

  private void OnEnable()
  {
    mapBridge = (MapBridge)target;
  }

  public override void OnInspectorGUI()
  {
    // 기본 인스펙터 표시
    DrawDefaultInspector();

    var spriteRenderer = mapBridge.spriteRenderer;
    if (mapBridge != null && spriteRenderer != null)
    {
      // SpriteRenderer의 DrawMode가 Tiled일 때만 size 동기화
      if (spriteRenderer.drawMode != SpriteDrawMode.Simple && mapBridge != null)
      {
        Vector2 mainSize = spriteRenderer.size;
        var collider = mapBridge.boxCollider;
        // 메인 사이즈와 다르면 자동으로 반영
        if (collider.size != mainSize)
        {
          Undo.RecordObject(collider, "Sync Box Collider Size");
          collider.offset = new Vector2(0, mainSize.y * 0.5f);
          collider.size = mainSize;

          // 즉시 에디터에 반영
          EditorUtility.SetDirty(collider);
          SceneView.RepaintAll();
        }
      }
    }
  }
}
#endif