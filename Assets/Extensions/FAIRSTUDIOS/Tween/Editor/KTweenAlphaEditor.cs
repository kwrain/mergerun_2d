using FAIRSTUDIOS.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CanEditMultipleObjects, CustomEditor(typeof(KTweenAlpha)), InitializeOnLoad]
public class KTweenAlphaEditor : Editor
{
  SerializedProperty ignoreChilds;

  private void OnEnable()
  {
    ignoreChilds = serializedObject.FindProperty("ignoreChilds");
  }

  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();

    KTweenAlpha tween = target as KTweenAlpha;
    tween.UseCanvasGroup = EditorGUILayout.Toggle("Use Canvas Group", tween.UseCanvasGroup);
  }
}
