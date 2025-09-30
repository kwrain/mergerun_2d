using System.Collections;
using System.Collections.Generic;
using FAIRSTUDIOS.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(KToggle), true)]
[CanEditMultipleObjects]
public class KToggleEditor : SelectableEditor
{

  SerializedProperty m_IsInteractable;

  SerializedProperty m_uniqueID;
  SerializedProperty m_IsOn;
  SerializedProperty m_CheckBeforeChange;
  SerializedProperty m_CheckBeforeChangeWaitTime;
  SerializedProperty m_Transition;
  SerializedProperty m_fadeDuration;

  SerializedProperty m_colorOn;
  SerializedProperty m_colorOff;
  SerializedProperty m_Graphics;

  SerializedProperty m_shadowColorOn;
  SerializedProperty m_shadowColorOff;
  SerializedProperty m_Shadows;

  SerializedProperty m_ShowBackground;
  SerializedProperty m_BackgroundGraphic;
  SerializedProperty m_AddedBackgroundGraphic;
  
  SerializedProperty m_Graphic;
  SerializedProperty m_AddedGraphics;
  
  SerializedProperty m_UnInteractableGraphic;
  SerializedProperty m_AddedUnInteractableGraphics;
  
  SerializedProperty m_Group;

  SerializedProperty m_OnBeforeToggleChanged;
  SerializedProperty m_OnValueChanged;
  SerializedProperty m_OnToggleChanged;
  SerializedProperty m_OnClickDisabled;


  bool isCheckChangeInteractable = false;

  protected override void OnEnable()
  {
    base.OnEnable();
    m_IsInteractable = serializedObject.FindProperty("m_Interactable");

    m_uniqueID = serializedObject.FindProperty("m_UniqueID");

    m_IsOn = serializedObject.FindProperty("m_IsOn");
    m_CheckBeforeChange = serializedObject.FindProperty("m_CheckBeforeChange");
    m_CheckBeforeChangeWaitTime = serializedObject.FindProperty("m_CheckBeforeChangeWaitTime");

    m_Transition = serializedObject.FindProperty("toggleTransition");
    m_fadeDuration = serializedObject.FindProperty("m_fadeDuration");

    m_colorOn = serializedObject.FindProperty("m_ColorOn");
    m_colorOff = serializedObject.FindProperty("m_ColorOff");
    m_Graphics = serializedObject.FindProperty("m_Graphics");

    m_shadowColorOn = serializedObject.FindProperty("m_ShadowColorOn");
    m_shadowColorOff = serializedObject.FindProperty("m_ShadowColorOff");
    m_Shadows = serializedObject.FindProperty("m_Shadows");
    
    m_ShowBackground = serializedObject.FindProperty("m_ShowBackground");
    m_BackgroundGraphic = serializedObject.FindProperty("backgroundGraphic");
    m_AddedBackgroundGraphic = serializedObject.FindProperty("addedBacgkroundGraphics");
    
    m_Graphic = serializedObject.FindProperty("graphic");
    m_AddedGraphics = serializedObject.FindProperty("addedGraphics");
    
    m_UnInteractableGraphic = serializedObject.FindProperty("unInteractableGraphic");
    m_AddedUnInteractableGraphics = serializedObject.FindProperty("addedUnInteractionableGraphics");
    
    m_Group = serializedObject.FindProperty("m_Group");

    m_OnBeforeToggleChanged = serializedObject.FindProperty("onBeforeToggleChange");
    m_OnValueChanged = serializedObject.FindProperty("onValueChanged");
    m_OnToggleChanged = serializedObject.FindProperty("onToggleChanged");
    m_OnClickDisabled = serializedObject.FindProperty("onClickDisabled");

    KToggle toggle = target as KToggle;
    isCheckChangeInteractable = toggle.interactable;
  }

  public override void OnInspectorGUI()
  {
    serializedObject.Update();

    EditorGUILayout.PropertyField(m_uniqueID);

    serializedObject.ApplyModifiedProperties();

    EditorGUILayout.Space();

    base.OnInspectorGUI();
    EditorGUILayout.Space();

    serializedObject.Update();
    KToggle toggle = serializedObject.targetObject as KToggle;

    if(isCheckChangeInteractable != toggle.interactable)
    {
      toggle.interactable = isCheckChangeInteractable = toggle.interactable;
    }
    
    EditorGUI.BeginChangeCheck();
    EditorGUILayout.PropertyField(m_IsOn);  
    EditorGUILayout.PropertyField(m_CheckBeforeChange);  
    if (EditorGUI.EndChangeCheck())
    {
      if(!Application.isPlaying)
      {
        EditorSceneManager.MarkSceneDirty(toggle.gameObject.scene);
      }

      KToggleGroup group = m_Group.objectReferenceValue as KToggleGroup;

      toggle.isOn = m_IsOn.boolValue;

      if (group != null && toggle.IsActive())
      {
        if (toggle.isOn || (!group.AnyTogglesOn() && !group.AllowSwitchOff))
        {
          toggle.isOn = true;
          group.NotifyToggleOn(toggle);
        }
      }
    }

    EditorGUILayout.PropertyField(m_Transition);
    EditorGUILayout.PropertyField(m_fadeDuration);
    EditorGUILayout.Space();

    EditorGUILayout.PropertyField(m_colorOn);
    EditorGUILayout.PropertyField(m_colorOff);
    EditorGUILayout.PropertyField(m_Graphics);

    EditorGUILayout.Space();

    EditorGUILayout.PropertyField(m_shadowColorOn);
    EditorGUILayout.PropertyField(m_shadowColorOff);
    EditorGUILayout.PropertyField(m_Shadows);

    EditorGUILayout.Space();

    EditorGUILayout.PropertyField(m_ShowBackground, new GUIContent("Hide Off"));
    if (!toggle.showBackground)
    {
      EditorGUILayout.PropertyField(m_BackgroundGraphic, new GUIContent("Off Graphic"));
      EditorGUILayout.PropertyField(m_AddedBackgroundGraphic, new GUIContent("Added Off Graphic"));
    }
    
    EditorGUILayout.PropertyField(m_Graphic, new GUIContent("On Graphic"));
    EditorGUILayout.PropertyField(m_AddedGraphics, new GUIContent("Added On Graphic"));


    EditorGUILayout.PropertyField(m_UnInteractableGraphic, new GUIContent("Uninteractable Graphic"));
    EditorGUILayout.PropertyField(m_AddedUnInteractableGraphics, new GUIContent("Added Uninteractable Graphic"));

    EditorGUI.BeginChangeCheck();
    EditorGUILayout.PropertyField(m_Group);
    if (EditorGUI.EndChangeCheck())
    {
      EditorSceneManager.MarkSceneDirty(toggle.gameObject.scene);
      KToggleGroup group = m_Group.objectReferenceValue as KToggleGroup;
      for (int i = 0; i < targets.Length; i++)
      {
        var ktoggle = targets[i] as KToggle;
        if (group == null)
        {
          ktoggle.Group.UnregisterToggle(ktoggle);
        }
        ktoggle.Group = group;
      }
      //toggle.Group = group;
    }

    EditorGUILayout.Space();

    // Draw the event notification options
    if (toggle.checkBeforeChange)
    {
      EditorGUILayout.PropertyField(m_CheckBeforeChangeWaitTime);
      EditorGUILayout.PropertyField(m_OnBeforeToggleChanged);
    }
    EditorGUILayout.PropertyField(m_OnValueChanged);
    EditorGUILayout.PropertyField(m_OnToggleChanged);
    EditorGUILayout.PropertyField(m_OnClickDisabled);

    serializedObject.ApplyModifiedProperties();
  }  
}
