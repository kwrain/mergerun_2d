using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FAIRSTUDIOS.EditorGUIStyle
{
  public class EditorGUIStyle
  {
    #region Label Style
    
    private static GUIStyle titleLabelStyle;
    
    private static GUIStyle textLabelStyle;
    private static GUIStyle textRedLabelStyle;
    private static GUIStyle textBlueLabelStyle;
    private static GUIStyle textGrayLabelStyle;
    
    public static GUIStyle TitleLabelStyle
    {
      get
      {
        if (titleLabelStyle == null)
        {
          titleLabelStyle = new GUIStyle(GUI.skin.button);
          titleLabelStyle.normal.textColor = Color.white;
          titleLabelStyle.alignment = TextAnchor.MiddleLeft;
          titleLabelStyle.fontSize = 15;
          titleLabelStyle.fontStyle = FontStyle.Bold;
          titleLabelStyle.wordWrap = true;
          titleLabelStyle.richText = true;
        }

        return titleLabelStyle;
      }
    }
    
    public static GUIStyle TextLabelStyle
    {
      get
      {
        if (textLabelStyle == null)
        {
          textLabelStyle = new GUIStyle(GUI.skin.label);
          textLabelStyle.normal.textColor = Color.white;
          textLabelStyle.alignment = TextAnchor.MiddleLeft;
          textLabelStyle.fontStyle = FontStyle.Bold;
          textLabelStyle.richText = true;
        }

        return textLabelStyle;
      }
    }
    public static GUIStyle TextRedLabelStyle
    {
      get
      {
        if (textRedLabelStyle == null)
        {
          textRedLabelStyle = new GUIStyle(GUI.skin.label);
          textRedLabelStyle.normal.textColor = new Color(1f, 0.43f, 0.48f);
          textRedLabelStyle.alignment = TextAnchor.MiddleLeft;
          textRedLabelStyle.fontStyle = FontStyle.Bold;
          textRedLabelStyle.richText = true;
        }

        return textRedLabelStyle;
      }
    }
    public static GUIStyle TextBlueLabelStyle
    {
      get
      {
        if (textBlueLabelStyle == null)
        {
          textBlueLabelStyle = new GUIStyle(GUI.skin.label);
          textBlueLabelStyle.normal.textColor = new Color(0.68f, 0.9f, 1f);
          textBlueLabelStyle.alignment = TextAnchor.MiddleLeft;
          textBlueLabelStyle.fontStyle = FontStyle.Bold;
          textBlueLabelStyle.richText = true;
        }

        return textBlueLabelStyle;
      }
    }
    public static GUIStyle TextGrayLabelStyle
    {
      get
      {
        if (textGrayLabelStyle == null)
        {
          textGrayLabelStyle = new GUIStyle(GUI.skin.label);
          textGrayLabelStyle.normal.textColor = Color.gray;
          textGrayLabelStyle.alignment = TextAnchor.MiddleLeft;
          textGrayLabelStyle.fontStyle = FontStyle.Bold;
          textGrayLabelStyle.richText = true;
        }

        return textGrayLabelStyle;
      }
    }
    
    #endregion
    
    private static GUIStyle groupBoxStyle;
    private static GUIStyle innerGroupBoxStyle;
    private static GUIStyle toggleStyle;


    public static GUIStyle GroupBoxStyle
    {
      get
      {
        if (groupBoxStyle == null)
        {
          groupBoxStyle = new GUIStyle(GUI.skin.box);
          groupBoxStyle.normal.textColor = Color.white;
          groupBoxStyle.normal.background = Texture2D.linearGrayTexture;
          groupBoxStyle.alignment = TextAnchor.MiddleCenter;
          groupBoxStyle.fontSize = 10;
          groupBoxStyle.padding = new RectOffset();
        }

        return groupBoxStyle;
      }
    }
    public static GUIStyle InnerGroupBoxStyle
    {
      get
      {
        if (innerGroupBoxStyle == null)
        {
          innerGroupBoxStyle = new GUIStyle(GUI.skin.box);
          innerGroupBoxStyle.normal.textColor = Color.white;
          innerGroupBoxStyle.normal.background = Texture2D.grayTexture;
          innerGroupBoxStyle.alignment = TextAnchor.MiddleCenter;
          innerGroupBoxStyle.fontSize = 10;
          innerGroupBoxStyle.padding = new RectOffset();
        }

        return innerGroupBoxStyle;
      }
    }
    public static GUIStyle ToggleStyle
    {
      get
      {
        if (toggleStyle == null)
        {
          toggleStyle = new GUIStyle(GUI.skin.toggle);
          toggleStyle.normal.textColor = Color.yellow;
          toggleStyle.alignment = TextAnchor.MiddleRight;
          toggleStyle.richText = true;
        }

        return toggleStyle;
      }
    }
  }
}