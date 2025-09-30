using FAIRSTUDIOS.UI;
using UnityEditor;

[CanEditMultipleObjects, CustomEditor(typeof(KToggleSwitch)), InitializeOnLoad]
public class KToggleSwitchEditor : Editor
{
  SerializedProperty onValueChanged;

  private void OnEnable()
  {
    onValueChanged = serializedObject.FindProperty("onValueChanged");
  }

  public override void OnInspectorGUI()
  {
    base.OnInspectorGUI();

    serializedObject.Update();

    KToggleSwitch toggle = target as KToggleSwitch;

    EditorGUILayout.Space();
    EditorGUILayout.LabelField("[Additional Area]", EditorStyles.boldLabel);
    toggle.bAdditionalArea = EditorGUILayout.Toggle("Additional Area", toggle.bAdditionalArea);
    toggle.SetAdditionalTouchArea(toggle.bAdditionalArea);

    EditorGUILayout.Space();
    EditorGUILayout.PropertyField(onValueChanged);

    serializedObject.ApplyModifiedProperties();
  }
}
