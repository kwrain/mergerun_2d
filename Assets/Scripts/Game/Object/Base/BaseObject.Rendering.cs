using System.Collections.Generic;
using UnityEngine;

public partial class BaseObject
{
  private const int RENDER_ORDER_Y_MULTIPLIER = 1000;
  private const int RENDER_ORDER_X_MULTIPLIER = 100;

  private MaterialPropertyBlock _propertyBlock;

  // Unity
  [SerializeField, HideInInspector] protected List<SpriteRenderer> sprites = new();
  protected Dictionary<int, int> settingOrders = new Dictionary<int, int>();

  protected MeshRenderer meshRenderer;

  public Color Color { get; protected set; } = Color.white;
  public float Alpha => Color.a;

  public virtual Renderer MainRenderer
  {
    get
    {
      if (sprites != null && sprites.Count > 0)
      {
        return sprites[0];
      }

      if (meshRenderer != null)
      {
        return meshRenderer;
      }

      return null;
    }
  }

  public virtual int RenderOrder
  {
    get
    {
      if (sprites != null && sprites.Count > 0)
      {
        return sprites[0].sortingOrder;
      }

      if (meshRenderer != null)
      {
        return meshRenderer.sortingOrder;
      }

      return 0;
    }
    set
    {
      foreach (var sprite in sprites)
      {
        sprite.sortingOrder = value + settingOrders[sprite.GetInstanceID()];
      }

      if (meshRenderer != null)
      {
        meshRenderer.sortingOrder = value;
      }
    }
  }

  public virtual int SortingLayerValue
  {
    get
    {
      if (sprites != null && sprites.Count > 0)
      {
        return SortingLayer.GetLayerValueFromID(sprites[0].sortingLayerID);
      }

      if (meshRenderer != null)
      {
        return SortingLayer.GetLayerValueFromID(meshRenderer.sortingLayerID);
      }

      return -1;
    }
    set
    {
      foreach (var sprite in sprites)
      {
        sprite.sortingLayerID = value;
      }

      if (meshRenderer != null)
      {
        meshRenderer.sortingLayerID = value;
      }
    }
  }

  public virtual string SortingLayerName
  {
    get
    {
      if (sprites != null && sprites.Count > 0)
      {
        return sprites[0].sortingLayerName;
      }

      if (meshRenderer != null)
      {
        return meshRenderer.sortingLayerName;
      }

      return null;
    }
    set
    {
      foreach (var sprite in sprites)
      {
        sprite.sortingLayerName = value;
      }

      if (meshRenderer != null)
      {
        meshRenderer.sortingLayerName = value;
      }
    }
  }

  public void SetColor(Color color)
  {
    Color = color;

    foreach (var sprite in sprites)
    {
      sprite.GetPropertyBlock(_propertyBlock);
      // lds - sprite.color 직접 변경으로 batches 감소
      // sprite.color = color;
      // kw - 애니메이션 키프레임에서 컬러값 조작 시 컬러 변경이 안되므로, 아래 코드 살림 (2021.2.22)
      _propertyBlock.SetColor("_Color", color);
      sprite.SetPropertyBlock(_propertyBlock);
    }

    if (meshRenderer != null)
    {
      meshRenderer.GetPropertyBlock(_propertyBlock);
      _propertyBlock.SetColor("_Color", color);
      meshRenderer.SetPropertyBlock(_propertyBlock);
    }
  }

  public virtual void SetAlpha(float alpha)
  {
    var color = Color;
    color.a = alpha;
    SetColor(color);
  }


  [ContextMenu("RefreshRenderOrder")]
  public virtual void RefreshRenderOrder()
  {
    var order = GetRenderOrder(transform.position.x, transform.position.y);
    RenderOrder = order;
    RefreshRenderOrderComplete();
  }

  public int GetRenderOrder(int x, int y) => y * RENDER_ORDER_Y_MULTIPLIER * -1 - x * RENDER_ORDER_X_MULTIPLIER;
  public int GetRenderOrder(float x, float y) => (int)(y * RENDER_ORDER_Y_MULTIPLIER) * -1 - (int)x * RENDER_ORDER_X_MULTIPLIER;
  public int GetRenderOrder(Vector3 worldPosition) => (int)(worldPosition.y * RENDER_ORDER_Y_MULTIPLIER) * -1 - (int)worldPosition.x * RENDER_ORDER_X_MULTIPLIER;


  protected virtual void RefreshRenderOrderComplete()
  {
  }

}
