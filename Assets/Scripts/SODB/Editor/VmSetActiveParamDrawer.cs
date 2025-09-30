using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/**
* VmSetActiveParamDrawer.cs
* 작성자 : doseon@fairstudios.kr
* 작성일 : 2022년 08월 09일 오후 3시 00분
*/

[CustomPropertyDrawer(typeof(VmSetActive.Param), true)]
public class VmSetActiveParamDrawer : PropertyDrawer
{
  private Dictionary<string, SerializedPropertyInfo> contextMap = new();
  public class SerializedPropertyInfo
  {
    public SerializedProperty conditionalLogic;
    public SerializedProperty comparison;
    public SerializedProperty expected;
    public SerializedProperty isExpectedString;
    public int index;

    public SerializedPropertyInfo(SerializedProperty property)
    {
      conditionalLogic = property.FindPropertyRelative("conditionalLogic");
      comparison = property.FindPropertyRelative("comparison");
      expected = property.FindPropertyRelative("expected");
      isExpectedString = property.FindPropertyRelative("isExpectedString");
      var splitPropertyPath = property.propertyPath.Split(".");
      // pInfos : 0
      // Array : 1
      // data[n] : 2
      // param : 3
      var data = splitPropertyPath[2];
      var dataToInt = data.Replace("data[", string.Empty).Replace("]", string.Empty);
      index = int.Parse(dataToInt);

    }
  }
  private float height;
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    string name = label.text;
    height = 0f;
    EditorGUI.BeginProperty(position, label, property);
    if(contextMap.ContainsKey(property.propertyPath) == false)
      contextMap.Add(property.propertyPath, new(property));

    var pInfo = contextMap[property.propertyPath];

    var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
    property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, name, true);
    if(property.isExpanded == false)
    {
      EditorGUI.EndProperty();
      return;
    }
    var propertyRect = new Rect(position.x + EditorGUIUtility.singleLineHeight, position.y, position.width - EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight);
    if(pInfo.index > 0)
    {
      IncreaseHeight(ref propertyRect);
      EditorGUI.PropertyField(propertyRect, pInfo.conditionalLogic);
    }

    IncreaseHeight(ref propertyRect, 8f);
    IncreaseHeight(ref propertyRect);
    EditorGUI.PropertyField(propertyRect, pInfo.comparison);

    IncreaseHeight(ref propertyRect, 8f);
    IncreaseHeight(ref propertyRect);
    if(GUI.Button(propertyRect, "Select Expected Value"))
    {
      var menu = new GenericMenu();
      menu.AddItem(new GUIContent("empty"), string.IsNullOrEmpty(pInfo.expected.stringValue) == true,
      () =>
      {
        pInfo.expected.stringValue = string.Empty;
        property.serializedObject.ApplyModifiedProperties();
      });
      string @true = "true";
      menu.AddItem(new GUIContent(@true), pInfo.expected.stringValue == @true,
      () =>
      {
        pInfo.expected.stringValue = @true;
        property.serializedObject.ApplyModifiedProperties();
      });
      string @false = "false";
      menu.AddItem(new GUIContent(@false), pInfo.expected.stringValue == @false,
      () =>
      {
        pInfo.expected.stringValue = @false;
        property.serializedObject.ApplyModifiedProperties();
      });
      menu.AddDisabledItem(new GUIContent("custom"),
      string.IsNullOrEmpty(pInfo.expected.stringValue) == false
      && pInfo.expected.stringValue != @true
      && pInfo.expected.stringValue != @false);
      menu.ShowAsContext();
    }

    IncreaseHeight(ref propertyRect, 4f);
    IncreaseHeight(ref propertyRect);
    EditorGUI.PropertyField(propertyRect, pInfo.expected);

    IncreaseHeight(ref propertyRect);
    EditorGUI.PropertyField(propertyRect, pInfo.isExpectedString);

    EditorGUI.EndProperty();
  }

  private void IncreaseHeight(ref Rect rect, float height = -1f, float addedY = 0f)
  {
    var prevHeight = rect.height;
    if (addedY > 0)
      prevHeight += addedY;
    if (height < 0)
      height = EditorGUIUtility.singleLineHeight;
    rect = new Rect(rect.x, rect.y + prevHeight, rect.width, height);
    this.height += height + (addedY > 0 ? addedY : 0);
  }

  public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
  {
    if(property.isExpanded == true)
      return base.GetPropertyHeight(property, label) + height;
    else
      return base.GetPropertyHeight(property, label);
  }
}