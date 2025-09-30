using UnityEngine;
using UnityEngine.UI;

namespace FAIRSTUDIOS.UI
{
  [AddComponentMenu("K.UI/Effects/Gradient")]
  public class Gradient : BaseMeshEffect
  {
    public Color m_color1 = Color.white;
    public Color m_color2 = Color.white;
    [Range(-180f, 180f)]
    public float m_angle = 0f;
    public bool m_ignoreRatio = true;

    private VertexHelper preVh;

    public void SetColors(Color color1, Color color2)
    {
      m_color1 = color1;
      m_color2 = color2;

      graphic.SetVerticesDirty();
      graphic.Rebuild(CanvasUpdate.PreRender);
    }

    public override void ModifyMesh(VertexHelper vh)
    {
      if (enabled)
      {
        var rect = graphic.rectTransform.rect;
        var dir = GradientUtils.RotationDir(m_angle);

        if (!m_ignoreRatio)
          dir = GradientUtils.CompensateAspectRatio(rect, dir);

        var localPositionMatrix = GradientUtils.LocalPositionMatrix(rect, dir);

        var vertex = default(UIVertex);
        for (var i = 0; i < vh.currentVertCount; i++)
        {
          vh.PopulateUIVertex(ref vertex, i);
          var localPosition = localPositionMatrix * vertex.position;
          vertex.color *= Color.Lerp(m_color2, m_color1, localPosition.y);
          vh.SetUIVertex(vertex, i);
        }

        preVh = vh;
      }
    }
  }
}


