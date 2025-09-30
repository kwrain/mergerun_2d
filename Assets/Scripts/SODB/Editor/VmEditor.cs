using System;
using System.Collections.Generic;
using DG.DemiEditor;
using UnityEditor;
using UnityEngine;

/**
* VmEditor.cs
* 작성자 : doseon@fairstudios.kr
* 작성일 : 2022년 07월 17일 오전 5시 26분
*/

[CustomEditor(typeof(VmBase), true)]
[CanEditMultipleObjects]
public class VmEditor : Editor
{
  protected SerializedProperty m_Script;
  protected SerializedProperty isViewThis;
  protected SerializedProperty view;
  protected SerializedProperty pInfos;

  #region Events
  protected SerializedProperty useOnStart;
  protected SerializedProperty useOnActivate;
  protected SerializedProperty useOnViewChanged;

  protected SerializedProperty foldoutEvents;
  protected SerializedProperty onStart;
  protected SerializedProperty onActivate;
  protected SerializedProperty onViewChanged;
  #endregion
  
  protected SerializedProperty lastPropertyForInspector;
  protected List<SerializedProperty> others = new();
  private List<Type> supportedPropertyTypes;
  protected virtual void OnEnable()
  {
    m_Script = serializedObject.FindProperty("m_Script");
    isViewThis = serializedObject.FindProperty("isViewThis");
    view = serializedObject.FindProperty("view");
    pInfos = serializedObject.FindProperty("pInfos");
    #region Events
    foldoutEvents = serializedObject.FindProperty("foldoutEvents");
    useOnStart = serializedObject.FindProperty("useOnStart");
    useOnActivate = serializedObject.FindProperty("useOnActivate");
    useOnViewChanged = serializedObject.FindProperty("useOnViewChanged");

    onStart = serializedObject.FindProperty("onStart");
    onActivate = serializedObject.FindProperty("onActivate");
    onViewChanged = serializedObject.FindProperty("onViewChanged");    
    #endregion
    lastPropertyForInspector = serializedObject.FindProperty("lastPropertyForInspector");

    supportedPropertyTypes = new();
    var attributes = serializedObject.targetObject.GetType().GetCustomAttributes(true);
    foreach (var attribute in attributes)
    {
      if (attribute.GetType() != typeof(SupportedPropertyAttribute)) continue;
      var bindPropertyAttribute = attribute as SupportedPropertyAttribute;
      if (bindPropertyAttribute.SupportedPropertyTypes != null)
      {
        foreach (var type in bindPropertyAttribute.SupportedPropertyTypes)
        {
          if (supportedPropertyTypes.Contains(type) == false)
            supportedPropertyTypes.Add(type);
        }
      }
    }
    var iter = lastPropertyForInspector.Copy();
    while(iter.NextVisible(false))
      others.Add(iter.Copy());
  }

  public override void OnInspectorGUI()
  {
    serializedObject.Update();

    DrawSupportedPropertyTypes();

    EditorGUI.BeginDisabledGroup(true);
    EditorGUILayout.PropertyField(m_Script);
    EditorGUI.EndDisabledGroup();


    DrawView();
    DrawViewModel();
    EditorGUILayout.PropertyField(pInfos);
    DrawCustom();
    DrawEvents();
    serializedObject.ApplyModifiedProperties();
  }

  private void DrawSupportedPropertyTypes()
  {
    if (GUILayout.Button("지원 프로퍼티 리스트") == true)
    {
      var menu = new GenericMenu();
      if (supportedPropertyTypes.Count > 0)
      {
        foreach (var type in supportedPropertyTypes)
        {
          menu.AddDisabledItem(new GUIContent(type.Name));
        }
      }
      else
      {
        menu.AddDisabledItem(new GUIContent("All but PropertyClass's List yet"));
      }
      menu.ShowAsContext();
    }
  }

  private void DrawViewModel()
  {
    EditorGUILayout.BeginVertical(new GUIStyle(GUI.skin.box));
    EditorGUILayout.LabelField("[VIEW MODEL]");
    foreach (var other in others)
    {
      EditorGUILayout.PropertyField(other);
    }
    EditorGUILayout.EndVertical();

  }

  private void DrawView()
  {
    EditorGUILayout.BeginHorizontal(new GUIStyle(GUI.skin.box));

    if (view.objectReferenceValue == null)
      isViewThis.boolValue = false;
    else
    {
      var vmGO = (serializedObject.targetObject as VmBase).gameObject;
      var viewGO = view.objectReferenceValue as GameObject;

      if (viewGO == null)
        viewGO = (view.objectReferenceValue as Component).gameObject;
      isViewThis.boolValue = ReferenceEquals(vmGO, viewGO);
    }
    EditorGUILayout.LabelField("[VIEW]", GUILayout.Width(100));
    EditorGUI.BeginChangeCheck();
    EditorGUILayout.PropertyField(view, new GUIContent());
    if (EditorGUI.EndChangeCheck() == true)
    {
      if (view.objectReferenceValue == null)
        isViewThis.boolValue = false;
      else
      {
        var vmGO = (serializedObject.targetObject as VmBase).gameObject;
        var viewGO = view.objectReferenceValue as GameObject;

        if (viewGO == null)
          viewGO = (view.objectReferenceValue as Component).gameObject;
        isViewThis.boolValue = ReferenceEquals(vmGO, viewGO);
      }
      serializedObject.ApplyModifiedProperties();
    }

    if (GUILayout.Button(isViewThis.boolValue ? "Set View Other" : "Set View This", GUILayout.Width(150)))
    {
      isViewThis.boolValue = !isViewThis.boolValue;
      var objs = serializedObject.targetObjects;
      foreach (var obj in objs)
      {
        var vm = obj as VmBase;
        if (isViewThis.boolValue == true)
        {
          vm.ReleaseView();
          vm.SetView();
        }
        else
          vm.ReleaseView();
      }
      serializedObject.Update();
    }

    EditorGUILayout.EndHorizontal();
  }

  protected virtual void DrawCustom()
  {

  }

  public GUIStyle buttonEventOnStyle;
  public GUIStyle buttonEventOffStyle;

  private void DrawEvents()
  {
    buttonEventOnStyle ??= new(GUI.skin.button);
    buttonEventOffStyle ??= new(GUI.skin.button);
    buttonEventOffStyle.normal.background = MakeBackgroundTexture(10, 10, Color.black);
    buttonEventOffStyle.onNormal.background = MakeBackgroundTexture(10, 10, Color.black);
    foldoutEvents.boolValue = EditorGUILayout.Foldout(foldoutEvents.boolValue, "foldout", true);
    if(foldoutEvents.boolValue == true)
    {
      using(new EditorGUILayout.HorizontalScope(GUI.skin.box))
      {
        bool isOn = useOnStart.boolValue;
        if(GUILayout.Button(new GUIContent("OnStart", useOnStart.tooltip), isOn == true ? buttonEventOnStyle : buttonEventOffStyle))
        {
          useOnStart.boolValue = !useOnStart.boolValue;
        }
        //EditorGUILayout.PropertyField(useOnStart);
        isOn = useOnActivate.boolValue;
        if (GUILayout.Button(new GUIContent("OnActivate", useOnActivate.tooltip), isOn == true ? buttonEventOnStyle : buttonEventOffStyle))
        {
          useOnActivate.boolValue = !useOnActivate.boolValue;
        }
        //EditorGUILayout.PropertyField(useOnActivate);
        isOn = useOnViewChanged.boolValue;
        if (GUILayout.Button(new GUIContent("OnViewChanged", useOnViewChanged.tooltip), isOn == true ? buttonEventOnStyle : buttonEventOffStyle))
        {
          useOnViewChanged.boolValue = !useOnViewChanged.boolValue;
        }
        //EditorGUILayout.PropertyField(useOnViewChanged);
      }
      if(useOnStart.boolValue == true)
        EditorGUILayout.PropertyField(onStart);
      if(useOnActivate.boolValue == true)
        EditorGUILayout.PropertyField(onActivate);
      if(useOnViewChanged.boolValue == true)
        EditorGUILayout.PropertyField(onViewChanged);
    }
  }
  private Texture2D MakeBackgroundTexture(int width, int height, Color color)
  {
    Color[] pixels = new Color[width * height];

    for (int i = 0; i < pixels.Length; i++)
    {
      pixels[i] = color;
    }

    Texture2D backgroundTexture = new Texture2D(width, height);

    backgroundTexture.SetPixels(pixels);
    backgroundTexture.Apply();

    return backgroundTexture;
  }
}