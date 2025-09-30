using System.Reflection;
using FAIRSTUDIOS.SODB.Property;
using UnityEditor;
using UnityEngine;

/**
* VmTextTextSetterEditor.cs
* 작성자 : doseon@fairsutdios.kr (LEE DO SEON)
* 작성일 : 2023년 02월 27일 오후 4시 42분
*/

[CustomEditor(typeof(VmTextTextSetter), true)]
[CanEditMultipleObjects]
public class VmTextTextSetterEditor : VmEditor
{
  private SerializedProperty localizeID;
  private string currentLocalizeID = string.Empty;
  private string localizeGetValue = string.Empty;
  protected override void OnEnable()
  {
    base.OnEnable();
    localizeID = serializedObject.FindProperty("localizeID");
    currentLocalizeID = localizeID.stringValue;
    localizeGetValue = Localize.GetValue(currentLocalizeID);
  }
  protected override void DrawCustom()
  {
    base.DrawCustom();
    if(currentLocalizeID != localizeID.stringValue)
    {
      currentLocalizeID = localizeID.stringValue;
      localizeGetValue = Localize.GetValue(currentLocalizeID);
    }
    if(string.IsNullOrEmpty(localizeGetValue) == false)
    {
      EditorGUILayout.LabelField($"Localize Value :");
      EditorGUILayout.TextArea($"{localizeGetValue}");
    }
  }
}