using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.UI;
using Yoyo.UI;

namespace YoyoEditor
{
	[CustomEditor(typeof(RichText), true), CanEditMultipleObjects]
	public class RichTextEditor : GraphicEditor
	{
		private SerializedProperty m_Content;
		private SerializedProperty m_FontData;
		private SerializedProperty m_SpriteGroups;
		private SerializedProperty m_UsedEffects;
		private SerializedProperty m_UnusedEffects;
		private SerializedProperty m_OnLinkProperty;

		protected override void OnEnable()
		{
			base.OnEnable();
			m_Content = serializedObject.FindProperty("m_Content");
			m_FontData = serializedObject.FindProperty("m_FontData");
			m_UsedEffects = serializedObject.FindProperty("m_UsedEffects");
			m_UnusedEffects = serializedObject.FindProperty("m_UnusedEffects");
			m_SpriteGroups = serializedObject.FindProperty("m_SpriteGroups");
			m_OnLinkProperty = serializedObject.FindProperty("m_OnLink");
		}

		void SetHideFlags(SerializedProperty effects)
		{
			for (var i = 0; i < effects.arraySize; i++) {
				var sp = effects.GetArrayElementAtIndex(i);
				var effect = sp.objectReferenceValue as TextEffect;
				if (effect != null) {
#if YOYO_DEBUG
					effect.hideFlags = HideFlags.None;
#else
					effect.hideFlags = HideFlags.HideInInspector;
#endif
				}
			}
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();
#if YOYO_DEBUG
			var text = serializedObject.FindProperty("m_Text");
			EditorGUI.BeginDisabledGroup(true);
			EditorGUILayout.PropertyField(text, new GUILayoutOption[0]);
			EditorGUI.EndDisabledGroup();
#endif
			EditorGUILayout.PropertyField(m_Content, new GUIContent("Text"), new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(m_FontData, new GUILayoutOption[0]);
			AppearanceControlsGUI();
			RaycastControlsGUI();

			EditorGUILayout.PropertyField(m_SpriteGroups, true, new GUILayoutOption[0]);
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(m_OnLinkProperty, new GUILayoutOption[0]);

			SetHideFlags(m_UsedEffects);
			SetHideFlags(m_UnusedEffects);

			serializedObject.ApplyModifiedProperties();
		}

		[MenuItem("GameObject/UI/Yoyo/RichText", false)]
		static void AddRichText(MenuCommand menuCommand)
		{
			var CreateUIElementRoot = typeof(DefaultControls).GetMethod("CreateUIElementRoot", BindingFlags.Static | BindingFlags.NonPublic);
			var gameObject = CreateUIElementRoot.Invoke(null, new object[] { "RichText", new Vector2(160f, 30f) }) as GameObject;
			var richText = gameObject.AddComponent<RichText>();
			richText.text = "New Text";

			Assembly assembly = Assembly.Load("UnityEditor.UI");
			var type = assembly.GetType("UnityEditor.UI.MenuOptions");
			var PlaceUIElementRoot = type.GetMethod("PlaceUIElementRoot", BindingFlags.Static | BindingFlags.NonPublic);
			PlaceUIElementRoot.Invoke(null, new object[] { gameObject, menuCommand });
		}
	}
}
