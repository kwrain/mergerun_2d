using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FAIRSTUDIOS.SODB.Core;
using FAIRSTUDIOS.SODB.Property;
using FAIRSTUDIOS.SODB.Utils;
using UnityEditor;
using UnityEngine;

/**
* PropertyInfoDrawer.cs
* 작성자 : doseon@fairsutdios.kr (LEE DO SEON)
* 작성일 : 2022년 07월 17일 오전 12시 50분
*/

[CustomPropertyDrawer(typeof(PropertyInfoBase), true)]
public class PropertyInfoDrawer : PropertyDrawer
{
  SerializedProperty activeProperty;
  private float height;
  void Expand() { activeProperty.isExpanded = true; }
  void Unexpand() { activeProperty.isExpanded = false; }
  private Dictionary<string, (bool fold, float height)> foldMap = new();
  private Dictionary<string, SerializedPropertyInfo> contextMap = new();
  private GUIStyle labelStyle;

  private List<Type> supportedPropertyTypes;
  public ModelBase[][] models;

  public class SerializedPropertyInfo
  {
    public GenericMenu modelMenu;
    public ModelBase model;
    public SerializedProperty property;
    public SerializedProperty contextType;
    public SerializedProperty index;
    public SerializedProperty stringKey;
    public SerializedProperty nameContextType;
    public SerializedProperty propertyName;
    public SerializedProperty propertyIndex;
    public SerializedProperty propertyStringKey;
    public SerializedProperty param;

    public SerializedPropertyInfo(SerializedProperty property)
    {
      this.property = property.FindPropertyRelative("property");
      this.contextType = property.FindPropertyRelative("contextType");
      this.index = property.FindPropertyRelative("index");
      this.stringKey = property.FindPropertyRelative("stringKey");
      this.nameContextType = property.FindPropertyRelative("nameContextType");
      this.propertyName = property.FindPropertyRelative("propertyName");
      this.propertyIndex = property.FindPropertyRelative("propertyIndex");
      this.propertyStringKey = property.FindPropertyRelative("propertyStringKey");
      this.param = property.FindPropertyRelative("param");
      if(this.property != null && this.property.objectReferenceValue != null)
      {
        var obj = new SerializedObject(this.property.objectReferenceValue);
        model = obj.FindProperty("model").objectReferenceValue as ModelBase;
      }
    }

  }

  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    if (labelStyle == null)
    {
      labelStyle = new();
      labelStyle.alignment = TextAnchor.MiddleLeft;
      labelStyle.fontStyle = FontStyle.Bold;
      labelStyle.normal.textColor = Color.white;
    }
    if (supportedPropertyTypes == null)
    {
      supportedPropertyTypes = new();
      var attributes = property.serializedObject.targetObject.GetType().GetCustomAttributes(true);
      foreach (var attribute in attributes)
      {
        if (attribute.GetType() != typeof(SupportedPropertyAttribute)) continue;
        var bindPropertyAttribute = attribute as SupportedPropertyAttribute;
        if(bindPropertyAttribute.SupportedPropertyTypes != null)
        {
          foreach (var type in bindPropertyAttribute.SupportedPropertyTypes)
          {
            if (supportedPropertyTypes.Contains(type) == false)
              supportedPropertyTypes.Add(type);
          }
        }
      }
    }
    if(models == null)
    {
      var sodbFolderSettings = SODBEditorSettingAsset.Instance.SodbFolderSettings;
      var guids = new string[sodbFolderSettings.Length][];
      models = new ModelBase[guids.Length][];
      for (int i = 0; i < guids.Length; i++)
      {
        guids[i] = AssetDatabase.FindAssets("t:ModelBase", new[] { sodbFolderSettings[i].FullPath });
        models[i] = new ModelBase[guids[i].Length];
        for (int j = 0; j < guids[i].Length; j++)
          models[i][j] = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[i][j]), typeof(ModelBase)) as ModelBase;
      }
    }

    string name = label.text;
    height = 0f;

    EditorGUI.BeginProperty(position, label, property);
    if(foldMap.ContainsKey(property.propertyPath) == false)
      foldMap.Add(property.propertyPath, (property.isExpanded, height));
    if(contextMap.ContainsKey(property.propertyPath) == false)
      contextMap.Add(property.propertyPath, new(property));

    var pInfo = contextMap[property.propertyPath];
    var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
    property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, name, true);
    if (property.isExpanded == false)
    {
      foldMap[property.propertyPath] = (property.isExpanded, height);
      EditorGUI.EndProperty();
      return;
    }
    IncreaseHeight(ref rect, 2f);
    EditorGUI.DrawRect(rect, Color.black);

    IncreaseHeight(ref rect);
    var newRect = new Rect(rect.x, rect.y, rect.width / 6f, rect.height);
    EditorGUI.BeginDisabledGroup(pInfo.model == null);
    if (GUI.Button(newRect, "Model") == true)
    {
      var propertyEditor = Type.GetType("UnityEditor.PropertyEditor, UnityEditor");
      var methods = propertyEditor.GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
      var openPropertyEditor = propertyEditor.GetMethod("OpenPropertyEditor"
      , BindingFlags.NonPublic | BindingFlags.Static
      , null
      , CallingConventions.Any
      , new Type[] { typeof(IList<UnityEngine.Object>) }
      , null);

      var targets = new List<UnityEngine.Object>();
      targets.Add(pInfo.model);
      openPropertyEditor.Invoke(null, new object[] { targets });
    }
    EditorGUI.EndDisabledGroup();

    newRect = new Rect(newRect.x + newRect.width, newRect.y, rect.width / 6f, newRect.height);
    if(GUI.Button(newRect, "Select") == true)
    {
      var menu = new GenericMenu();
      menu.AddItem(new GUIContent("None"), ReferenceEquals(null, pInfo.model)
      , () =>
      {
        pInfo.property.objectReferenceValue = null;
        pInfo.model = null;
        property.serializedObject.ApplyModifiedProperties();
      });
      var sodbFolderSettings = SODBEditorSettingAsset.Instance.SodbFolderSettings;
      for (int i = 0; i < models.Length; i++)
      {
        if(models[i] == null) continue;
        var fullPath = sodbFolderSettings[i].FullPath;
        var popupPath = sodbFolderSettings[i].PopupCategory;
        foreach(var model in models[i])
        {
          var isInValidAssetPath = false;   // 모델 에셋 경로 유효성 체크
          var isInvalidPopupPath = false;   // 모델 팝업 경로 유효성 체크
          var path = AssetDatabase.GetAssetPath(model);

          if(model.popupPath == string.Empty) continue;
          if(model.assetPath != path) isInValidAssetPath = true;
          model.assetPath = path;

          var newPopupPath = model.assetPath.Replace(fullPath, popupPath);
          if(model.popupPath != newPopupPath) isInValidAssetPath = true;
          model.popupPath = newPopupPath;

          if(isInValidAssetPath == true || isInvalidPopupPath == true)
          {
            EditorUtility.SetDirty(model);
            AssetDatabase.SaveAssetIfDirty(model);
          }

          menu.AddItem(new GUIContent(model.popupPath), ReferenceEquals(model, pInfo.model)
          , () =>
          {
            pInfo.property.objectReferenceValue = null;
            pInfo.model = model;
            property.serializedObject.ApplyModifiedProperties();
          });
        }
      }
      menu.ShowAsContext();
    }

    if(pInfo.model != null)
    {
      newRect = new Rect(newRect.x + newRect.width + 10f, newRect.y, rect.width * 4f / 6f - 10f, newRect.height);
      EditorGUI.BeginDisabledGroup(true);
      EditorGUI.ObjectField(newRect, "", pInfo.model, typeof(ModelBase), null);
      EditorGUI.EndDisabledGroup();
    }

    IncreaseHeight(ref rect, 2f, 5f);
    EditorGUI.DrawRect(rect, Color.black);

    if(pInfo.property == null || pInfo.property.objectReferenceValue == null)
    {

    }
    IncreaseHeight(ref rect);
    newRect = new Rect(rect.x, rect.y, rect.width / 6f, rect.height);
    EditorGUI.BeginDisabledGroup(pInfo.property == null || pInfo.property.objectReferenceValue == null);
    if (GUI.Button(newRect, "Property") == true)
    {
      var propertyEditor = Type.GetType("UnityEditor.PropertyEditor, UnityEditor");
      var methods = propertyEditor.GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
      var openPropertyEditor = propertyEditor.GetMethod("OpenPropertyEditor"
      , BindingFlags.NonPublic | BindingFlags.Static
      , null
      , CallingConventions.Any
      , new Type[] { typeof(IList<UnityEngine.Object>) }
      , null);

      var targets = new List<UnityEngine.Object>();
      targets.Add(pInfo.property.objectReferenceValue);
      openPropertyEditor.Invoke(null, new object[] { targets });
    }
    EditorGUI.EndDisabledGroup();

    newRect = new Rect(newRect.x + newRect.width, newRect.y, rect.width / 6f, newRect.height);
    EditorGUI.BeginDisabledGroup(pInfo.model == null);
    if(GUI.Button(newRect, "Select") == true)
    {
      var menu = new GenericMenu();
      var propertyList = pInfo.model.Properties;
      menu.AddItem(new GUIContent("None"), ReferenceEquals(null, pInfo.property.objectReferenceValue)
      , () =>
      {
        pInfo.property.objectReferenceValue = null;
        // var obj = new SerializedObject(pInfo.property.objectReferenceValue);
        // pInfo.model = obj.FindProperty("model").objectReferenceValue as ModelBase;
        property.serializedObject.ApplyModifiedProperties();
      });
      foreach (var p in propertyList)
      {
        if(supportedPropertyTypes.Count > 0 
          && supportedPropertyTypes.Contains(p.GetType()) == false) 
          continue;        

        menu.AddItem(new GUIContent(p.name), ReferenceEquals(p, pInfo.property.objectReferenceValue)
        , () =>
        {
          pInfo.property.objectReferenceValue = p;

          var baseType = p.GetType();
          if (baseType == typeof(PropertyContextClass))
          {
            pInfo.contextType.intValue = (int)PropertyInfoContextType.PropertyName;
          }
          else
          {
            while (baseType != null)
            {
              if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(PropertyListBase<>))
              {
                pInfo.contextType.intValue = (int)PropertyInfoContextType.Index;
                break;
              }

              if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(PropertyMapBase<,>))
              {
                var args = baseType.GetGenericArguments();
                if (args[0] == typeof(string))
                {
                  pInfo.contextType.intValue = (int)PropertyInfoContextType.StringKey;
                  break;
                }
                else if (args[0] == typeof(int))
                {
                  pInfo.contextType.intValue = (int)PropertyInfoContextType.Index;
                  break;
                }
              }
              baseType = baseType.BaseType;
            }
          }
          if (baseType == null)
            pInfo.contextType.intValue = (int)PropertyInfoContextType.Mono;
          property.serializedObject.ApplyModifiedProperties();
        });
      }

      menu.ShowAsContext();
    }
    EditorGUI.EndDisabledGroup();
    newRect = new Rect(newRect.x + newRect.width + 10f, newRect.y, rect.width * 4f / 6f - 10f, newRect.height);

    EditorGUI.BeginChangeCheck();
    var prev = pInfo.property.objectReferenceValue;
    EditorGUI.PropertyField(newRect, pInfo.property, new GUIContent(), false);
    if(EditorGUI.EndChangeCheck() == true)
    {
      if(supportedPropertyTypes.Count > 0)
      {
        if(supportedPropertyTypes.Contains(pInfo.property.objectReferenceValue.GetType()) == false)
        {
          pInfo.property.objectReferenceValue = prev;
        }
      }
      var p = pInfo.property.objectReferenceValue;
      var obj = new SerializedObject(p);
      pInfo.model = obj.FindProperty("model").objectReferenceValue as ModelBase;
      var baseType = p.GetType();
      if (baseType == typeof(PropertyContextClass))
      {
        pInfo.contextType.intValue = (int)PropertyInfoContextType.PropertyName;
      }
      else
      {
        while (baseType != null)
        {
          if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(PropertyListBase<>))
          {
            pInfo.contextType.intValue = (int)PropertyInfoContextType.Index;
            break;
          }

          if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(PropertyMapBase<,>))
          {
            var args = baseType.GetGenericArguments();
            if (args[0] == typeof(string))
            {
              pInfo.contextType.intValue = (int)PropertyInfoContextType.StringKey;
              break;
            }
            else if(args[0] == typeof(int))
            {
              pInfo.contextType.intValue = (int)PropertyInfoContextType.Index;
              break;
            }
          }
          baseType = baseType.BaseType;
        }
      }
      if (baseType == null)
        pInfo.contextType.intValue = (int)PropertyInfoContextType.Mono;
      property.serializedObject.ApplyModifiedProperties();
    }

    if(pInfo.property != null && pInfo.property.objectReferenceValue != null)
    {
      switch ((PropertyInfoContextType)pInfo.contextType.intValue)
      {
        case PropertyInfoContextType.Mono: break;
        case PropertyInfoContextType.Index:
          {
            IncreaseHeight(ref rect, 2f, 2f);
            IncreaseHeight(ref rect);
            EditorGUI.PropertyField(rect, pInfo.index);
          }
          break;
        case PropertyInfoContextType.StringKey:
          {
            IncreaseHeight(ref rect, 2f, 2f);
            IncreaseHeight(ref rect);
            newRect = new Rect(rect.x, rect.y, rect.width / 6f, rect.height);
            EditorGUI.LabelField(newRect, "StringKey");
            newRect = new Rect(newRect.x + newRect.width, newRect.y, rect.width / 6f, newRect.height);
            if(GUI.Button(newRect, "Select"))
            {
              var menu = new GenericMenu();
              var obj = new SerializedObject(pInfo.property.objectReferenceValue);
              var keys = obj.FindProperty("defaultValue").FindPropertyRelative("list");
              menu.AddItem(new GUIContent("None"), string.IsNullOrEmpty(pInfo.stringKey.stringValue) == true
              , () =>
              {
                pInfo.stringKey.stringValue = string.Empty;
                property.serializedObject.ApplyModifiedProperties();
              });
              if(keys != null)
              {
                for (int i = 0; i < keys.arraySize; i++)
                {
                  var key = keys.GetArrayElementAtIndex(i).FindPropertyRelative("Key");
                  menu.AddItem(new GUIContent(key.stringValue), key.stringValue == pInfo.stringKey.stringValue
                  , () =>
                  {
                    pInfo.stringKey.stringValue = key.stringValue;
                    property.serializedObject.ApplyModifiedProperties();
                  });
                }
                menu.ShowAsContext();
              }
            }
            newRect = new Rect(newRect.x + newRect.width + 10f, newRect.y, rect.width * 4f / 6f - 10f, newRect.height);
            EditorGUI.PropertyField(newRect, pInfo.stringKey, new GUIContent());
          }
          break;
        case PropertyInfoContextType.PropertyName:
          {
            IncreaseHeight(ref rect, 2f, 2f);
            IncreaseHeight(ref rect);
            newRect = new Rect(rect.x, rect.y, rect.width / 6f, rect.height);
            EditorGUI.LabelField(newRect, "Context");
            newRect = new Rect(newRect.x + newRect.width, newRect.y, rect.width / 6f, newRect.height);
            if(GUI.Button(newRect, "Select"))
            {
              var menu = new GenericMenu();

              menu.AddItem(new GUIContent("None"), string.IsNullOrEmpty(pInfo.propertyName.stringValue) == true
              , () =>
              {
                pInfo.propertyName.stringValue = string.Empty;
                property.serializedObject.ApplyModifiedProperties();
              });
              var pClass = pInfo.property.objectReferenceValue as PropertyContextClass;
              var type = Type.GetType($"{pClass.ContextClassName}, {pClass.AssemblyName}");
              var properties = type.GetProperties();
              foreach(var p in properties)
              {
                var propertyType = p.PropertyType;
                if(supportedPropertyTypes.Count > 0 
                  && supportedPropertyTypes.Contains(propertyType) == false) 
                  continue;
                var attributes = p.GetCustomAttributes();
                foreach(var attr in attributes)
                {
                  if(attr.GetType() == typeof(ContextAttribute))
                  {
                    menu.AddItem(new GUIContent(p.Name), p.Name == pInfo.propertyName.stringValue
                    , () =>
                    {
                      pInfo.propertyName.stringValue = p.Name;
                      if(propertyType.IsGenericType == true)
                      {
                        var def = propertyType.GetGenericTypeDefinition();
                        if(def == typeof(ContextList<>))
                        {
                          pInfo.nameContextType.intValue = (int)PropertyNameContextType.Index;
                        }
                        else if(def == typeof(ContextDictionary<>))
                        {
                          pInfo.nameContextType.intValue = (int)PropertyNameContextType.StringKey;
                        }
                      }
                      else
                      {
                        pInfo.nameContextType.intValue = (int)PropertyNameContextType.Mono;
                      }
                      property.serializedObject.ApplyModifiedProperties();
                    });
                  }
                }
              }

              menu.ShowAsContext();
            }
            newRect = new Rect(newRect.x + newRect.width + 10f, newRect.y, rect.width * 4f / 6f - 10f, newRect.height);
            EditorGUI.PropertyField(newRect, pInfo.propertyName, new GUIContent());
            switch ((PropertyNameContextType)pInfo.nameContextType.intValue)
            {
              case PropertyNameContextType.Mono: break;
              case PropertyNameContextType.Index:
                {
                  IncreaseHeight(ref rect, 2f, 2f);
                  IncreaseHeight(ref rect);
                  EditorGUI.PropertyField(rect, pInfo.propertyIndex);
                }
                break;
              case PropertyNameContextType.StringKey:
                {
                  IncreaseHeight(ref rect, 2f, 2f);
                  IncreaseHeight(ref rect);
                  newRect = new Rect(rect.x, rect.y, rect.width / 6f, rect.height);
                  EditorGUI.LabelField(newRect, "StringKey");
                  newRect = new Rect(newRect.x + newRect.width, newRect.y, rect.width / 6f, newRect.height);
                  if(GUI.Button(newRect, "Select"))
                  {
                    var pClass = pInfo.property.objectReferenceValue as PropertyContextClass;
                    var type = Type.GetType($"{pClass.ContextClassName}, {pClass.AssemblyName}");
                    var p = type.GetProperty(pInfo.propertyName.stringValue);
                    var attrs = p.GetCustomAttributes();
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent("None"), string.IsNullOrEmpty(pInfo.propertyStringKey.stringValue) == true
                    , () =>
                    {
                      pInfo.propertyStringKey.stringValue = string.Empty;
                      property.serializedObject.ApplyModifiedProperties();
                    });

                    foreach(var attr in attrs)
                    {
                      if(attr.GetType() == typeof(EnumKeyAttribute))
                      {
                        var enumKey = attr as EnumKeyAttribute;
                        var enumNames = Enum.GetNames(enumKey.EnumKeyType).ToList();
                        foreach(var enumName in enumNames)
                        {
                          menu.AddItem(new GUIContent(enumName), pInfo.propertyStringKey.stringValue == enumName,
                          () =>
                          {
                            pInfo.propertyStringKey.stringValue = enumName;
                            property.serializedObject.ApplyModifiedProperties();
                          });
                        }
                      }
                    }

                    menu.ShowAsContext();
                  }
                  newRect = new Rect(newRect.x + newRect.width + 10f, newRect.y, rect.width * 4f / 6f - 10f, newRect.height);
                  EditorGUI.PropertyField(newRect, pInfo.propertyStringKey, new GUIContent());
                }
                break;
            }
          }
          break;
      }
    }

    if (pInfo.param != null)
    {
      IncreaseHeight(ref rect, 2f, 2f);
      EditorGUI.PropertyField(rect, pInfo.param, true);
      IncreaseHeight(ref rect, EditorGUI.GetPropertyHeight(pInfo.param));
    }

    IncreaseHeight(ref rect, 2f, 2f);
    EditorGUI.DrawRect(rect, Color.black);

    foldMap[property.propertyPath] = (property.isExpanded, height);
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
    if (foldMap.ContainsKey(property.propertyPath) == false)
      return base.GetPropertyHeight(property, label);
    var fold = foldMap[property.propertyPath];
    if (fold.fold == true)
      return base.GetPropertyHeight(property, label) + fold.height;
    else
      return base.GetPropertyHeight(property, label);
  }
}