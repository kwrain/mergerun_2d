using UnityEngine;
using UnityEngine.U2D;

public class MapGround : MapElement
{
  [SerializeField] private SpriteAtlas atlas;
  [SerializeField] private SpriteRenderer spriteRendererMask;
  [SerializeField] private SpriteRenderer spriteRendererTexture;

  public override void SetData(StageDataTable.MapData mapData)
  {
    base.SetData(mapData);

    switch (ElementType)
    {
      case MapElementTypes.Ground:
        spriteRenderer.sprite = atlas.GetSprite("MapRoad");
        break;

      case MapElementTypes.GroundDiagonal:
        spriteRenderer.sprite = atlas.GetSprite("MapRoadDiagonal");
        break;
    }

    SetTextrue();
  }

  private void SetTextrue()
  {
    if (spriteRenderer == null || spriteRendererTexture == null)
      return;

    var size = spriteRenderer.size - Vector2.one * 0.15f;
    size.x *= 0.9f;
    spriteRendererTexture.size = size;

    var pos = new Vector3(0f, (spriteRenderer.size.y - size.y) * 0.5f, 0f);
    spriteRendererTexture.transform.localPosition = pos;

    if (spriteRendererMask != null)
    {
      spriteRendererMask.size = size;
      spriteRendererMask.transform.localPosition = pos;
    }
  }

  protected override void UpdateCollider()
  {
    base.UpdateCollider();

    SetTextrue();
  }
}
