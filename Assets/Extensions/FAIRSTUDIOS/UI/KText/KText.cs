using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KText : Text
{
  Graphic graphic;

  [SerializeField]
  private float m_spacing = 0f;

  protected override void Awake()
  {
    graphic = this as Graphic;
  }

#if UNITY_EDITOR
  protected override void OnValidate()
  {
    spacing = m_spacing;
    base.OnValidate();
  }
#endif

  public override float preferredWidth => base.preferredWidth + spacing;

  public float spacing
  {
    get { return m_spacing; }
    set
    {
      if (m_spacing == value) return;
      m_spacing = value;
      if (graphic == null)
      {
        graphic = this as Graphic;
      }

      graphic.SetVerticesDirty();
    }
  }

  protected override void OnPopulateMesh(VertexHelper toFill)
  {
    base.OnPopulateMesh(toFill);
    if (!this.IsActive())
      return;

    List<UIVertex> list = new List<UIVertex>();
    toFill.GetUIVertexStream(list);

    ModifyVertices(list);

    toFill.Clear();
    toFill.AddUIVertexTriangleStream(list);

  }

  public void ModifyVertices(List<UIVertex> verts)
  {
    if (!IsActive())
      return;

    string[] lines = text.Split('\n');
    Vector3 pos;
    float letterOffset = spacing * (float)fontSize / 100f;
    float alignmentFactor = 0;
    int glyphIdx = 0;

    switch (alignment)
    {
      case TextAnchor.LowerLeft:
      case TextAnchor.MiddleLeft:
      case TextAnchor.UpperLeft:
        alignmentFactor = 0f;
        break;

      case TextAnchor.LowerCenter:
      case TextAnchor.MiddleCenter:
      case TextAnchor.UpperCenter:
        alignmentFactor = 0.5f;
        break;

      case TextAnchor.LowerRight:
      case TextAnchor.MiddleRight:
      case TextAnchor.UpperRight:
        alignmentFactor = 1f;
        break;
    }

    for (int lineIdx = 0; lineIdx < lines.Length; lineIdx++)
    {
      string line = lines[lineIdx];
      float lineOffset = (line.Length - 1) * letterOffset * alignmentFactor;

      for (int charIdx = 0; charIdx < line.Length; charIdx++)
      {
        int idx1 = glyphIdx * 6 + 0;
        int idx2 = glyphIdx * 6 + 1;
        int idx3 = glyphIdx * 6 + 2;
        int idx4 = glyphIdx * 6 + 3;
        int idx5 = glyphIdx * 6 + 4;
        int idx6 = glyphIdx * 6 + 5;

        // Check for truncated text (doesn't generate verts for all characters)
        if (idx6 > verts.Count - 1)
          return;

        UIVertex vert1 = verts[idx1];
        UIVertex vert2 = verts[idx2];
        UIVertex vert3 = verts[idx3];
        UIVertex vert4 = verts[idx4];
        UIVertex vert5 = verts[idx5];
        UIVertex vert6 = verts[idx6];

        pos = Vector3.right * (letterOffset * charIdx - lineOffset);

        CharacterInfo ci = new CharacterInfo();
        int sum = 0;
        for(int i = 0; i <= charIdx; i++)
        {
          font.GetCharacterInfo(line[i], out ci, fontSize);
          sum += ci.advance;
        }
        
        // 이전 까지의 사이즈와 자신의 사이즈를 더해야한다.
        //Debug.LogFormat("Index : {0}, Pos X : {1}", charIdx, sum + pos.x);
        if (sum + pos.x > rectTransform.sizeDelta.x)
        {
          vert1.color = Color.clear;
          vert2.color = Color.clear;
          vert3.color = Color.clear;
          vert4.color = Color.clear;
          vert5.color = Color.clear;
          vert6.color = Color.clear;
        }
        else
        {
          vert1.position += pos;
          vert2.position += pos;
          vert3.position += pos;
          vert4.position += pos;
          vert5.position += pos;
          vert6.position += pos;
        }

        verts[idx1] = vert1;
        verts[idx2] = vert2;
        verts[idx3] = vert3;
        verts[idx4] = vert4;
        verts[idx5] = vert5;
        verts[idx6] = vert6;

        glyphIdx++;
      }

      // Offset for carriage return character that still generates verts
      glyphIdx++;
    }
  }
}
