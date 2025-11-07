using UnityEngine;
using UnityEngine.U2D;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MapGround : MapElement
{
  public SpriteRenderer spriteRendererTexture;
  [SerializeField] private SpriteAtlas atlas;

  public override void SetData(StageDataTable.MapData mapData)
  {
    base.SetData(mapData);

    spriteRenderer.sprite = atlas.GetSprite("MapRoad");

    var size = mapData.size - Vector2.one * 0.15f;
    size.x *= 0.9f;
    spriteRendererTexture.size = size;
    spriteRendererTexture.transform.localPosition = new Vector3(0f, (mapData.size.y - size.y) * 0.5f, 0f);
  }
}


#if UNITY_EDITOR
[CustomEditor(typeof(MapGround))]
public class MapGroundEditor : Editor
{
  private MapGround mapGround;

  private void OnEnable()
  {
    mapGround = (MapGround)target;
  }

  public override void OnInspectorGUI()
  {
    // 기본 인스펙터 표시
    DrawDefaultInspector();

    var spriteRenderer = mapGround.spriteRenderer;
    if (mapGround != null && spriteRenderer != null)
    {
      // SpriteRenderer의 DrawMode가 Tiled일 때만 size 동기화
      if (spriteRenderer.drawMode != SpriteDrawMode.Simple && mapGround != null)
      {
        var srTexture = mapGround.spriteRendererTexture;
        if (srTexture != null)
        {
          Vector2 mainSize = spriteRenderer.size;

          // 메인 사이즈와 다르면 자동으로 반영
          if (srTexture.size != mainSize)
          {
            Undo.RecordObject(srTexture, "Sync SpriteRenderer Texture Size");
            mapGround.boxCollider.offset = new Vector2(0, mainSize.y * 0.5f);
            mapGround.boxCollider.size = mainSize;
            var size = mainSize - Vector2.one * 0.5f;
            size.x *= 0.91f;
            srTexture.size = size;
            srTexture.transform.localPosition = new Vector3(0f, (mainSize.y - srTexture.size.y) * 0.5f, 0f);

            // 즉시 에디터에 반영
            EditorUtility.SetDirty(srTexture);
            SceneView.RepaintAll();
          }
        }
      }
    }
  }
}
#endif