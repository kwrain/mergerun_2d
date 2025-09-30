using FAIRSTUDIOS.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static FAIRSTUDIOS.UI.KProgressBar;

[CanEditMultipleObjects, CustomEditor(typeof(KProgressBar)), InitializeOnLoad]
public class KProgressBarEditor : Editor
{
  bool foldOutPadding = true;

  SerializedProperty onStart;
  SerializedProperty onUpdate;
  SerializedProperty onEnd;

  private void OnEnable()
  {
    onStart = serializedObject.FindProperty("onStart");
    onUpdate = serializedObject.FindProperty("onUpdate");
    onEnd = serializedObject.FindProperty("onEnd");
  }

  public override void OnInspectorGUI()
  {
    //base.OnInspectorGUI();

    serializedObject.Update();

    KProgressBar progress = target as KProgressBar;

    progress.CheckOriginalSize();

    EditorGUILayout.Space();
    EditorGUI.BeginChangeCheck();
    EProgressType progressType = (EProgressType)EditorGUILayout.EnumPopup("Type", progress.ProgressType);
    if(EditorGUI.EndChangeCheck())
    {
      progress.ProgressType = progressType;
    }

    EditorGUILayout.Space();
    EditorGUI.BeginChangeCheck();
    float amount = EditorGUILayout.Slider("Amount", progress.Amount, 0, 1);
    if(EditorGUI.EndChangeCheck())
    {
      progress.SetProgress(amount);
      EditorUtility.SetDirty(progress);
    }

    EditorGUILayout.Space();
    EditorGUI.BeginChangeCheck();
    bool showPercent = EditorGUILayout.Toggle("Show Percent", progress.ShowPercent);
    if (EditorGUI.EndChangeCheck())
    {
      if (!showPercent)
      {
        progress.SetText(string.Empty);
      }

      progress.ShowPercent = showPercent;
    }

    progress.AmountView = EditorGUILayout.Toggle("Amount View", progress.AmountView);
    progress.DecimalPointDisplay = EditorGUILayout.Toggle("Decimal Point Display", progress.DecimalPointDisplay);

    EditorGUILayout.Space();
    if (foldOutPadding = EditorGUILayout.Foldout(foldOutPadding, "Padding"))
    {
      EditorGUI.indentLevel++;

      Vector2 padding = new Vector2(EditorGUILayout.FloatField("Left", progress.padding.x), EditorGUILayout.FloatField("Right", progress.padding.y));
      EditorGUI.indentLevel--;

      if (padding != progress.padding)
      {
        progress.padding = padding;
      }
    }

    EditorGUILayout.Space();
    EditorGUILayout.PropertyField(onStart);
    EditorGUILayout.PropertyField(onUpdate);
    EditorGUILayout.PropertyField(onEnd);

    serializedObject.ApplyModifiedProperties();
  }
}
