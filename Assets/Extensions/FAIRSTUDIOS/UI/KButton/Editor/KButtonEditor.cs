using System;
using System.Globalization;
using System.Linq;
using FAIRSTUDIOS.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UI;
using UnityEngine;
using static FAIRSTUDIOS.UI.KButton;

[CanEditMultipleObjects, CustomEditor(typeof(KButton)), InitializeOnLoad]
public class KButtonEditor : SelectableEditor
{

  SerializedProperty m_IsInteractable;
  SerializedProperty m_uniqueID;

  SerializedProperty isAdditionalArea;
  SerializedProperty m_AdditionalArea;

  SerializedProperty positionType;

  SerializedProperty m_ColorOn;
  SerializedProperty m_ColorOff;
  SerializedProperty m_Graphics;

  SerializedProperty m_ShadowColorOn;
  SerializedProperty m_ShadowColorOff;
  SerializedProperty m_Shadows;

  SerializedProperty blockEventTime;
  SerializedProperty blockAutoInteractable;

  SerializedProperty pressedWaitTime;
  SerializedProperty repeatedSpeed;
  SerializedProperty maxAcceleration;
  SerializedProperty blockPointerUpWhenPressed;

  SerializedProperty onClick;
  SerializedProperty onDown;
  SerializedProperty onUp;
  SerializedProperty onBlocked;
  SerializedProperty onPress;
  SerializedProperty onPressed;
  SerializedProperty onClickDisabled;

  SerializedProperty m_Graphic;
  SerializedProperty m_AddedGraphics;
  SerializedProperty m_UnInteractableGraphic;
  SerializedProperty m_AddedUnInteractableGraphics;

  SerializedProperty m_CustomClickSound;
  SerializedProperty m_CustomDisabledSound;
  SerializedProperty m_CustomDownSound;
  SerializedProperty m_CustomUpSound;
  SerializedProperty m_ClickSound;
  SerializedProperty m_DisabledSound;
  SerializedProperty m_DownSound;
  SerializedProperty m_UpSound;

  string searchClickSound = string.Empty;
  bool isSearchClickSound = false;

  string searchDisableSound = string.Empty;
  bool isSearchDisableSound = false;

  string searchDownSound = string.Empty;
  bool isSearchDownSound = false;

  string searchUpSound = string.Empty;
  bool isSearchUpSound = false;

  bool isCheckChangeInteractable = false;

  protected override void OnEnable()
  {
    base.OnEnable();
    m_IsInteractable = serializedObject.FindProperty("m_Interactable");

    m_uniqueID = serializedObject.FindProperty("m_UniqueID");

    isAdditionalArea = serializedObject.FindProperty("isAdditionalArea");
    m_AdditionalArea = serializedObject.FindProperty("m_AdditionalArea");

    positionType = serializedObject.FindProperty("positionType");

    m_ColorOn = serializedObject.FindProperty("m_ColorOn");
    m_ColorOff = serializedObject.FindProperty("m_ColorOff");
    m_Graphics = serializedObject.FindProperty("m_Graphics");

    m_ShadowColorOn = serializedObject.FindProperty("m_ShadowColorOn");
    m_ShadowColorOff = serializedObject.FindProperty("m_ShadowColorOff");
    m_Shadows = serializedObject.FindProperty("m_Shadows");

    blockEventTime = serializedObject.FindProperty("blockEventTime");
    blockAutoInteractable = serializedObject.FindProperty("blockAutoInteractable");

    pressedWaitTime = serializedObject.FindProperty("pressedWaitTime");
    repeatedSpeed = serializedObject.FindProperty("repeatedSpeed");
    maxAcceleration = serializedObject.FindProperty("maxAcceleration");
    blockPointerUpWhenPressed = serializedObject.FindProperty("blockPointerUpWhenPressed");

    onClickDisabled = serializedObject.FindProperty("m_OnClickDisabled");
    onClick = serializedObject.FindProperty("m_OnClick");
    onDown = serializedObject.FindProperty("m_OnDown");
    onUp = serializedObject.FindProperty("m_OnUp");
    onBlocked = serializedObject.FindProperty("m_OnBlocked");
    onPress = serializedObject.FindProperty("m_OnPress");
    onPressed = serializedObject.FindProperty("m_OnPressed");

    m_Graphic = serializedObject.FindProperty("graphic");
    m_AddedGraphics = serializedObject.FindProperty("addedGraphics");
    m_UnInteractableGraphic = serializedObject.FindProperty("unInteractableGraphic");
    m_AddedUnInteractableGraphics = serializedObject.FindProperty("addedUnInteractionableGraphics");

    // sound
    m_CustomClickSound = serializedObject.FindProperty("customClickSound");
    m_CustomDisabledSound = serializedObject.FindProperty("customDisabledSound");
    m_CustomDownSound = serializedObject.FindProperty("customDownSound");
    m_CustomUpSound = serializedObject.FindProperty("customUpSound");

    m_ClickSound = serializedObject.FindProperty("clickSound");
    m_DisabledSound = serializedObject.FindProperty("disabledSound");
    m_DownSound = serializedObject.FindProperty("downSound");
    m_UpSound = serializedObject.FindProperty("upSound");

    KButton button = target as KButton;
    isCheckChangeInteractable = button.interactable;
  }

  private T SearchEnumLabel<T>(string label, T state, ref string searchEnumText, ref bool isSearchEnumLabelInSearch) where T : struct, IConvertible
  {
    if (!typeof(T).IsEnum)
    {
      EditorGUILayout.LabelField("T must be an enumerated type");
      return state;
    }

    var states = Enum.GetValues(typeof(T)).Cast<object>().Select(o => o.ToString()).ToArray();
    if (string.IsNullOrEmpty(searchEnumText) && states.Length > 0)
      searchEnumText = state.ToString(CultureInfo.InvariantCulture);

    var text = EditorGUILayout.TextField(label, searchEnumText);
    if (text != searchEnumText || isSearchEnumLabelInSearch)
    {
      searchEnumText = text;
      var mach = states.Select((v, i) => new { value = v, index = i }).Where(a => a.value.ToLower().StartsWith(text.ToLower())).ToList();
      var targetState = state;
      if (mach.Any())
      {
        // many of results
        targetState = (T)Enum.GetValues(typeof(T)).GetValue(mach[0].index);
        //EditorGUILayout.LabelField("Select closested: " + targetState);
        Repaint();
        var selected = GUILayout.SelectionGrid(-1, mach.Select(v => v.value).ToArray(), 1);
        if (selected != -1)
        {
          targetState = (T)Enum.GetValues(typeof(T)).GetValue(mach[selected].index);
          searchEnumText = targetState.ToString(CultureInfo.InvariantCulture);
          isSearchEnumLabelInSearch = false;
          GUI.FocusControl("FocusAway");
          Repaint();
        }
      }

      state = targetState;
      isSearchEnumLabelInSearch = !string.Equals(searchEnumText, targetState.ToString(CultureInfo.InvariantCulture), StringComparison.CurrentCultureIgnoreCase);
    }

    return state;
  }
  public override void OnInspectorGUI()
  {
    serializedObject.Update();

    EditorGUILayout.PropertyField(m_uniqueID);

    serializedObject.ApplyModifiedProperties();

    EditorGUILayout.Space();

    base.OnInspectorGUI();

    serializedObject.Update();

    KButton button = target as KButton;

    if(isCheckChangeInteractable != button.interactable)
    {
      button.interactable = isCheckChangeInteractable = button.interactable;
    }

    EditorGUILayout.Space();
    EditorGUI.BeginChangeCheck();
    EditorGUILayout.PropertyField(isAdditionalArea);
    if (EditorGUI.EndChangeCheck())
    {
      button.SetAdditionalTouchArea(isAdditionalArea.boolValue);
    }
    if(button.IsAdditionalArea)
    {
      EditorGUILayout.PropertyField(m_AdditionalArea);
    }

    EditorGUILayout.Space();

    EditorGUI.BeginChangeCheck();
    EditorGUILayout.PropertyField(positionType);
    if (EditorGUI.EndChangeCheck())
    {
      button.CheckPositionType();
    }

    EditorGUILayout.PropertyField(m_ColorOn);
    EditorGUILayout.PropertyField(m_ColorOff);
    EditorGUILayout.PropertyField(m_Graphics);

    EditorGUILayout.Space();

    EditorGUILayout.PropertyField(m_ShadowColorOn);
    EditorGUILayout.PropertyField(m_ShadowColorOff);
    EditorGUILayout.PropertyField(m_Shadows);

    EditorGUILayout.Space();

    EditorGUILayout.PropertyField(m_Graphic);
    EditorGUILayout.PropertyField(m_AddedGraphics);

    EditorGUILayout.PropertyField(m_UnInteractableGraphic);
    EditorGUILayout.PropertyField(m_AddedUnInteractableGraphics);

    EditorGUILayout.Space();

    EditorGUILayout.PropertyField(blockEventTime);
    if (blockEventTime.floatValue > 0)
    {
      EditorGUILayout.PropertyField(blockAutoInteractable);
    }
    EditorGUILayout.Space();

    EditorGUILayout.PropertyField(pressedWaitTime);
    if (pressedWaitTime.floatValue > 0)
    {
      EditorGUILayout.PropertyField(repeatedSpeed);
      EditorGUILayout.PropertyField(maxAcceleration);
      blockPointerUpWhenPressed.boolValue = EditorGUILayout.ToggleLeft("blockPointerUpWhenPressed", blockPointerUpWhenPressed.boolValue);
      //EditorGUILayout.PropertyField(blockPointerUpWhenPressed);
    }

    EditorGUILayout.Space();

    KButton[] buttons = Array.ConvertAll(targets, _t => _t as KButton);

    EditorGUILayout.Space();
    
    EditorGUILayout.PropertyField(m_CustomClickSound, new GUIContent("Custom Click Sound"));
    if (m_CustomClickSound.boolValue)
    {
      EditorGUILayout.PropertyField(m_ClickSound);
    }

    EditorGUILayout.PropertyField(m_CustomDisabledSound, new GUIContent("Custom Disabled Sound"));
    if (m_CustomDisabledSound.boolValue)
    {
      EditorGUILayout.PropertyField(m_DisabledSound);
    }

    EditorGUILayout.PropertyField(m_CustomDownSound, new GUIContent("Custom Down Sound"));
    if (m_CustomDownSound.boolValue)
    {
      EditorGUILayout.PropertyField(m_DownSound);
    }

    EditorGUILayout.PropertyField(m_CustomUpSound, new GUIContent("Custom Up Sound"));
    if (m_CustomUpSound.boolValue)
    {
      EditorGUILayout.PropertyField(m_UpSound);
    }
    // button.ClickSound = SearchEnumLabel("Click Sound", button.ClickSound, ref searchClickSound, ref isSearchClickSound);
    // button.DownSound = SearchEnumLabel("Down Sound", button.DownSound, ref searchDownSound, ref isSearchDownSound);
    // button.UpSound = SearchEnumLabel("Up Sound", button.UpSound, ref searchUpSound, ref isSearchUpSound);
    // button.DisabledSound = SearchEnumLabel("Disable Sound", button.DisabledSound, ref searchDisableSound, ref isSearchDisableSound);

    // foreach (KButton btn in buttons)
    // {
    //   btn.ClickSound = button.ClickSound;
    //   btn.DownSound = button.DownSound;
    //   btn.UpSound = button.UpSound;
    //   btn.DisabledSound = button.DisabledSound;
    // }
    EditorGUILayout.Space();

    EditorGUILayout.PropertyField(onClickDisabled); 
    EditorGUILayout.PropertyField(onClick);
    EditorGUILayout.PropertyField(onDown);
    EditorGUILayout.PropertyField(onUp);
    if (blockEventTime.floatValue > 0)
    {
      EditorGUILayout.PropertyField(onBlocked);
    }
    if (pressedWaitTime.floatValue > 0)
    {
      EditorGUILayout.PropertyField(onPress);
      EditorGUILayout.PropertyField(onPressed);
    }
    serializedObject.ApplyModifiedProperties();
  }
}
