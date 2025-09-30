using UnityEngine;
using UnityEngine.U2D;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAtlasSetter : MonoBehaviour
{
  [SerializeField] private SpriteRenderer spriteRenderer;
  [SerializeField] private SpriteAtlas spriteAtlas;
  [SerializeField] private string spriteName;

  private void Awake()
  {
    if(spriteRenderer == null)
    {
      spriteRenderer = GetComponent<SpriteRenderer>();
    }

    SetSprite();
  }

  [ContextMenu("SetSprite")]
  private void SetSprite()
  {
    if (spriteAtlas == null || string.IsNullOrEmpty(spriteName))
      return;

    var sprite = spriteAtlas.GetSprite(spriteName);
    if(sprite != null)
    {
      spriteRenderer.sprite = sprite;
    }
  }
}
