using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FAIRSTUDIOS.UI;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.U2D;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace FAIRSTUDIOS.Tools
{
  /// <summary>
  /// AtlasImage 용 관리자 Editor.
  /// Image의 속성을 상속합니다.
  /// Atlas의 선택은 팝업 스프라이트의 선택은 전용 선택기를 구현하고 있습니다.
  /// 스프라이트 미리보기 기능을 통해 스프라이트의 경계를 관리자에서 변경할 수 있습니다.
  /// </summary>
  [CustomEditor(typeof(AtlasImage), true)]
  [CanEditMultipleObjects]
  public class AtlasImageEditor : ImageEditor
  {
    /// <summary>
    /// 스프라이트 프리뷰
    /// </ summary>
    private SpritePreview preview = new SpritePreview();

    /// <summary>
    /// SerializedProperty] 아틀라스 (m_Atlas)
    /// </summary>
    private SerializedProperty spAtlas;

    /// <summary> SerializedProperty 스프라이트 이름 (m_SpriteName) </ summary>
    private SerializedProperty spSpriteName;

    /// <summary> lds - SerializedProperty 로컬라이즈 가능 이미지 (m_SpriteName) </ summary>
    private SerializedProperty isLocalizeImage;

    /// <summary> SerializedProperty 스프라이트 유형 (m_Type) </ summary>
    private SerializedProperty spType;

    /// <summary> SerializedProperty] 화면 비율을 유지하거나 (m_PreserveAspect) </ summary>
    private SerializedProperty spPreserveAspect;

    private SerializedProperty spriteProperty;

    private SerializedProperty nativeSizeRatio;

    /// <summary> 스프라이트 유형에 따른 애니메이션 부울. </ summary>
    private AnimBool animShowType;

    private SpriteAtlas spriteAtlas;
    private Sprite sprite;

    private GUIContent m_CorrectButtonContent;
    /// <summary>
    /// 관리자 사용 콜백.
    /// </summary>
    protected override void OnEnable()
    {
      if (!target)
        return;

      base.OnEnable();

      spAtlas = serializedObject.FindProperty("m_SpriteAtlas");
      spSpriteName = serializedObject.FindProperty("m_SpriteName");
      isLocalizeImage = serializedObject.FindProperty("m_IsLocalizeImage");
      spType = serializedObject.FindProperty("m_Type");
      spPreserveAspect = serializedObject.FindProperty("m_PreserveAspect");
      spriteProperty = serializedObject.FindProperty("m_Sprite");
      nativeSizeRatio = serializedObject.FindProperty("m_NativeSizeRatio");

      animShowType = new AnimBool(spAtlas.objectReferenceValue && !string.IsNullOrEmpty(spSpriteName.stringValue));
      animShowType.valueChanged.AddListener(new UnityAction(base.Repaint));

      preview.onApplyBorder = () =>
      {
        PackAtlas(spAtlas.objectReferenceValue as SpriteAtlas);
        (target as AtlasImage).sprite = (spAtlas.objectReferenceValue as SpriteAtlas).GetSprite(spSpriteName.stringValue);
      };

      spriteAtlas = null;
      m_CorrectButtonContent = EditorGUIUtility.TrTextContent("Set Half Size", "Sets the size to match the content.");
    }

    protected override void OnDisable()
    {
      base.OnDisable();
      preview.onApplyBorder = null;
    }

    /// <summary>
    /// インスペクタGUIコールバック.
    /// Inspectorウィンドウを表示するときにコールされます.
    /// </summary>
    public override void OnInspectorGUI()
    {
      serializedObject.Update();
      //			using (new EditorGUI.DisabledGroupScope(true))
      //			{
      //				EditorGUILayout.PropertyField(m_Script);
      //			}

      //アトラスとスプライトを表示.
      //			EditorGUILayout.PropertyField(spAtlas);

      var image = target as AtlasImage;

      DrawAtlasPopupLayout(new GUIContent("Sprite Atlas"), new GUIContent("-"), spAtlas);

      EditorGUI.indentLevel++;
      //DrawSpritePopupLayout(new GUIContent("Sprite Name"), new GUIContent("-"), spAtlas.objectReferenceValue as SpriteAtlas, spSpriteName);
       DrawSpritePopup(spAtlas.objectReferenceValue as SpriteAtlas, spSpriteName);
      EditorGUI.indentLevel--;

      EditorGUI.BeginChangeCheck();
      EditorGUILayout.PropertyField(isLocalizeImage, new GUIContent("isLocalizeImage"));
      EditorGUILayout.PropertyField(spriteProperty, new GUIContent("SourceImage"));
      if(EditorGUI.EndChangeCheck())
      {
        if (spriteProperty.objectReferenceValue != null)
          spSpriteName.stringValue = spriteProperty.objectReferenceValue.name;
      }
      /* lds add */
      if (spriteProperty.objectReferenceValue != null)
      {
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(EditorGUIUtility.labelWidth);
        if (GUILayout.Button("Ping", EditorStyles.miniButton) == true)
        {
          var texture = (spriteProperty.objectReferenceValue as Sprite).texture;
          EditorGUIUtility.PingObject(texture);
        }
        EditorGUILayout.EndHorizontal();
      }
      /* lds add */
      //			serializedObject.ApplyModifiedProperties();

      //base.OnInspectorGUI();

      ////Imageインスペクタの再現. ▼ ここから ▼.
      AppearanceControlsGUI();
      RaycastControlsGUI();
      MaskableControlsGUI();

      animShowType.target = spAtlas.objectReferenceValue && !string.IsNullOrEmpty(spSpriteName.stringValue) || spriteProperty.objectReferenceValue != null;
      if (EditorGUILayout.BeginFadeGroup(animShowType.faded))
        this.TypeGUI();
      EditorGUILayout.EndFadeGroup();

      if (EditorGUILayout.BeginFadeGroup(m_ShowNativeSize.faded))
      {
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(spPreserveAspect);
        EditorGUILayout.PropertyField(nativeSizeRatio);
        EditorGUI.indentLevel--;
      }
      EditorGUILayout.EndFadeGroup();
      base.NativeSizeButtonGUI();
      //Imageインスペクタの再現. ▲ ここまで ▲.

      serializedObject.ApplyModifiedProperties();

      //プレビューを更新.
      if (image.spriteAtlas)
      {
        preview.sprite = GetOriginalSprite(image.spriteAtlas, image.spriteName);
      }
      else if (image.sprite)
      {
        preview.sprite = image.sprite;
      }
      else
      {
        preview.sprite = null;
      }

      //spType.intValue = (int)(image.hasBorder ? Image.Type.Sliced : Image.Type.Simple);
      var imageType = (Image.Type)spType.intValue;

      SetShowNativeSize(imageType == Image.Type.Simple || imageType == Image.Type.Filled, false);

      preview.color = image ? image.canvasRenderer.GetColor() : Color.white;

      serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// オブジェクトプレビューのタイトルを返します.
    /// </summary>
    public override GUIContent GetPreviewTitle()
    {
      return preview.GetPreviewTitle();
    }

    /// <summary>
    /// インタラクティブなカスタムプレビューを表示します.
    /// </summary>
    public override void OnPreviewGUI(Rect rect, GUIStyle background)
    {
      preview.OnPreviewGUI(rect);
    }

    /// <summary>
    /// オブジェクトプレビューの上部にオブジェクト情報を示します。
    /// </summary>
    public override string GetInfoString()
    {
      return preview.GetInfoString();
    }

    /// <summary>
    /// プレビューのヘッダーを表示します.
    /// </summary>
    public override void OnPreviewSettings()
    {
      preview.OnPreviewSettings();
    }


    /// <summary>
    /// アトラスポップアップを描画します.
    /// </summary>
    /// <param name="label">ラベル.</param>
    /// <param name="atlas">アトラス.</param>
    /// <param name="spriteName">スプライト名.</param>
    /// <param name="onSelect">変更された時のコールバック.</param>
    public void DrawAtlasPopupLayout(GUIContent label, GUIContent nullLabel, SerializedProperty atlas, UnityAction<SpriteAtlas> onChange = null, params GUILayoutOption[] option)
    {
      DrawAtlasPopup(GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.popup, option), label, nullLabel, atlas, onChange);
    }

    /// <summary>
    /// アトラスポップアップを描画します.
    /// </summary>
    /// <param name="label">ラベル.</param>
    /// <param name="atlas">アトラス.</param>
    /// <param name="spriteName">スプライト名.</param>
    /// <param name="onSelect">変更された時のコールバック.</param>
    public void DrawAtlasPopupLayout(GUIContent label, GUIContent nullLabel, SpriteAtlas atlas, UnityAction<SpriteAtlas> onChange = null, params GUILayoutOption[] option)
    {
      DrawAtlasPopup(GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.popup, option), label, nullLabel, atlas, onChange);
    }


    /// <summary>
    /// アトラスポップアップを描画します.
    /// </summary>
    /// <param name="rect">描画範囲の矩形.</param>
    /// <param name="label">ラベル.</param>
    /// <param name="atlas">アトラス.</param>
    /// <param name="onSelect">変更された時のコールバック.</param>
    public void DrawAtlasPopup(Rect rect, GUIContent label, GUIContent nullLabel, SerializedProperty atlas, UnityAction<SpriteAtlas> onSelect = null)
    {
      DrawAtlasPopup(rect, label, nullLabel, atlas.objectReferenceValue as SpriteAtlas, obj =>
      {
        atlas.objectReferenceValue = obj;
        if (onSelect != null)
          onSelect(obj);
        atlas.serializedObject.ApplyModifiedProperties();

        if (spriteAtlas != obj)
        {
          spriteAtlas = obj;
          PackAtlas(obj);
        }
      });
    }

    /// <summary>
    /// アトラスポップアップを描画します.
    /// </summary>
    /// <param name="rect">描画範囲の矩形.</param>
    /// <param name="label">ラベル.</param>
    /// <param name="atlas">アトラス.</param>
    /// <param name="onSelect">変更された時のコールバック.</param>
    public void DrawAtlasPopup(Rect rect, GUIContent label, GUIContent nullLabel, SpriteAtlas atlas, UnityAction<SpriteAtlas> onSelect = null)
    {
      rect = EditorGUI.PrefixLabel(rect, label);

      var atlasName = string.Empty;
      if (atlas != null)
      {
        atlasName = atlas.name;
        //if (atlasName.Substring(atlasName.Length - 3, 1) == "_")
        //{
        //  atlasName = atlasName.Substring(0, atlasName.Length - 3);
        //}
      }

      if (GUI.Button(rect, atlas ? new GUIContent(atlasName) : nullLabel, EditorStyles.popup))
      {
        var gm = new GenericMenu();
        gm.AddItem(nullLabel, !atlas, () => onSelect(null));

        foreach (var path in AssetDatabase.FindAssets("t:" + typeof(SpriteAtlas).Name).Select(x => AssetDatabase.GUIDToAssetPath(x)))
        {
          var displayName = Path.GetFileNameWithoutExtension(path);

          // 파일명에 언어코드가 들어간 경우
          //if (displayName.Substring(displayName.Length - 3, 1) == "_")
          //  continue;

          gm.AddItem(
            new GUIContent(displayName),
            atlas && (atlas.name == displayName),
            x => onSelect(x == null ? null : AssetDatabase.LoadAssetAtPath((string)x, typeof(SpriteAtlas)) as SpriteAtlas),
            path
          );
        }

        gm.DropDown(rect);
      }
    }

    public void DrawLanguageCodePopupLayout(GUIContent label,SerializedProperty language, UnityAction<ELanguageCode, string> onChange = null, params GUILayoutOption[] option)
    {
      DrawLanguageCodePopup(GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.popup, option), label, language, onChange);
    }

    public void DrawLanguageCodePopup(Rect rect, GUIContent label, SerializedProperty language, UnityAction<ELanguageCode, string> onSelect = null)
    {
      DrawLanguageCodePopup(rect, label, language.intValue, (obj, path) =>
      {
        language.intValue = (int)obj;
        onSelect?.Invoke(obj, path);

        var atlas = AssetDatabase.LoadAssetAtPath(path, typeof(SpriteAtlas)) as SpriteAtlas;
        spAtlas.objectReferenceValue = atlas;
        spAtlas.serializedObject.ApplyModifiedProperties();
        language.serializedObject.ApplyModifiedProperties();

        var image = target as AtlasImage;
        image.SetMaterialDirty();
      });
    }

    public void DrawLanguageCodePopup(Rect rect, GUIContent label, int language, UnityAction<ELanguageCode, string> onSelect = null)
    {
      rect = EditorGUI.PrefixLabel(rect, label);
      if(GUI.Button(rect, ((ELanguageCode)language).ToString(), EditorStyles.popup))
      {
        if (spriteAtlas == null && spAtlas.objectReferenceValue != null)
        {
          spriteAtlas = (SpriteAtlas)spAtlas.objectReferenceValue;
        }

        if (spriteAtlas == null)
          return;

        var defaultAtlas = spriteAtlas.name;      // 기본 아틀라스 이름
        if (defaultAtlas.Substring(defaultAtlas.Length - 3, 1) == "_")
        {
          defaultAtlas = defaultAtlas.Substring(0, defaultAtlas.Length - 3);
        }

        var support = new Dictionary<string, string>();
        var atlaslist = AssetDatabase.FindAssets("t:" + typeof(SpriteAtlas).Name)
          .Select(x => AssetDatabase.GUIDToAssetPath(x));
        foreach (var path in atlaslist)
        {
          var displayName = Path.GetFileNameWithoutExtension(path);
          if(!displayName.Contains(defaultAtlas))
            continue;

          if (displayName == defaultAtlas)
          {
            support[ELanguageCode.KO.ToString()] = path;
          }
          else
          {
            support[displayName.Substring(displayName.Length - 2, 2)] = path;
          }
        }

        var gm = new GenericMenu();
        gm.AddItem( new GUIContent(ELanguageCode.KO.ToString()), language == (int)ELanguageCode.KO, () => onSelect(ELanguageCode.KO, support[ELanguageCode.KO.ToString()]));

        foreach(ELanguageCode code in System.Enum.GetValues(typeof(ELanguageCode)))
        {
          if(code == ELanguageCode.KO)
            continue;

          if(!support.ContainsKey(code.ToString()))
            continue;

          gm.AddItem(new GUIContent(code.ToString()),
            language == (int)code,
            x => onSelect(code, support[code.ToString()]),
            code
          );
        }

        gm.DropDown(rect);
      }
    }


    public void DrawSpritePopupLayout(GUIContent label, GUIContent nullLabel, SpriteAtlas atlas, SerializedProperty spriteName)
    {
      if (atlas == null)
        return;

      DrawSpritePopup(GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.popup), label, nullLabel, atlas, spriteName, name =>
      {
        if (spriteName == null)
          return;

        spriteName.stringValue = name;
        spriteName.serializedObject.ApplyModifiedProperties();
      });
    }

    public void DrawSpritePopup(Rect rect, GUIContent label, GUIContent nullLabel, SpriteAtlas atlas, SerializedProperty spriteName, UnityAction<string> onSelect = null)
    {
      var name = spriteName.stringValue;
      rect = EditorGUI.PrefixLabel(rect, label);
      if (GUI.Button(rect, string.IsNullOrEmpty(name) ? nullLabel : new GUIContent(name), EditorStyles.popup))
      {
        if (spriteAtlas != atlas)
        {
          spriteAtlas = atlas;
          PackAtlas(atlas);
        }

        var gm = new GenericMenu();
        gm.AddItem(nullLabel, string.IsNullOrEmpty(name), () => onSelect(null));

        var sprites = new Sprite[atlas.spriteCount];
        atlas.GetSprites(sprites);

        sprites = sprites.OrderBy(s => s.name).ToArray();

        foreach (var sprite in sprites)
        {
          var displayName = sprite.name.Replace("(Clone)", string.Empty);
          gm.AddItem(new GUIContent(displayName), atlas && sprite.name == name,
            x => onSelect(x == null ? null : sprite.name.Replace("(Clone)", string.Empty)), sprite);
        }

        gm.DropDown(rect);
      }
    }


    /// <summary>
    /// スプライトポップアップを描画します.
    /// </summary>
    /// <param name="atlas">アトラス.</param>
    /// <param name="spriteName">スプライト名.</param>
    public void DrawSpritePopup(SpriteAtlas atlas, SerializedProperty spriteName)
    {
      DrawSpritePopup(new GUIContent(spriteName.displayName, spriteName.tooltip), atlas, spriteName);
    }

    /// <summary>
    /// スプライトポップアップを描画します.
    /// </summary>
    /// <param name="label">ラベル.</param>
    /// <param name="atlas">アトラス.</param>
    /// <param name="spriteName">スプライト名.</param>
    public void DrawSpritePopup(GUIContent label, SpriteAtlas atlas, SerializedProperty spriteName)
    {
      DrawSpritePopup(
        label,
        atlas,
        string.IsNullOrEmpty(spriteName.stringValue) ? "-" : spriteName.stringValue,
        name =>
        {
          if (spriteName == null)
            return;

          spriteName.stringValue = name;
          spriteName.serializedObject.ApplyModifiedProperties();
        }
      );
    }

    static bool openSelectorWindow = false;

    /// <summary>
    /// スプライトポップアップを描画します.
    /// </summary>
    /// <param name="atlas">アトラス.</param>
    /// <param name="spriteName">スプライト名.</param>
    /// <param name="onChange">変更された時のコールバック.</param>
    public void DrawSpritePopup(GUIContent label, SpriteAtlas atlas, string spriteName, UnityAction<string> onChange)
    {
      var controlID = GUIUtility.GetControlID(FocusType.Passive);
      if (openSelectorWindow)
      {
        var atlasLabel = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(atlas));
        var findAsset = AssetDatabase.FindAssets("l:" + atlasLabel);
        if(findAsset.Length > 0)
          EditorGUIUtility.ShowObjectPicker<Sprite>(atlas.GetSprite(spriteName), false, "l:" + atlasLabel, controlID);
        openSelectorWindow = false;
      }

      // Popup-styled button to select sprite in atlas.
      using (new EditorGUI.DisabledGroupScope(!atlas))
      using (new EditorGUILayout.HorizontalScope())
      {
        EditorGUILayout.PrefixLabel(label);
        if (GUILayout.Button(string.IsNullOrEmpty(spriteName) ? "-" : spriteName, "minipopup") && atlas)
        {
          if (spriteAtlas != atlas)
          {
            spriteAtlas = atlas;
            PackAtlas(atlas);
          }
          openSelectorWindow = true;
        }
      }

      // lds - 유니티 에디터 2021.2 부터는
      // controlID == EditorGUIUtility.GetObjectPickerControlID() 를 선 비교하면 에러남
      var commandName = Event.current.commandName;
      //選択オブジェクト更新イベント
      if (commandName == "ObjectSelectorUpdated" && controlID == EditorGUIUtility.GetObjectPickerControlID())
      {
        var picked = EditorGUIUtility.GetObjectPickerObject();
        onChange(picked ? picked.name.Replace("(Clone)", "") : "");
      }
      //クローズイベント
      else if (commandName == "ObjectSelectorClosed" && controlID == EditorGUIUtility.GetObjectPickerControlID())
      {
        // On close selector window, reomove the atlas label from sprites.
        //SetAtlasLabelToSprites(atlas, false);
      }
    }

    /// <summary>
    /// Sets the atlas label to sprites.
    /// </summary>
    /// <returns>The atlas label to sprites.</returns>
    /// <param name="atlas">Atlas.</param>
    /// <param name="add">If set to <c>true</c> add.</param>
    string SetAtlasLabelToSprites(SpriteAtlas atlas, bool add)
    {
      string[] assetLabels = { AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(atlas)) };

      var spPackedSprites = new SerializedObject(atlas).FindProperty("m_PackedSprites");
      var sprites = Enumerable.Range(0, spPackedSprites.arraySize)
        .Select(index => spPackedSprites.GetArrayElementAtIndex(index).objectReferenceValue)
        .OfType<Sprite>()
        .ToArray();

      foreach (var s in sprites)
      {
        var newLabels = add
          ? AssetDatabase.GetLabels(s).Union(assetLabels).ToArray()
          : AssetDatabase.GetLabels(s).Except(assetLabels).ToArray();

        if (add == true)
          AssetDatabase.SetLabels(s, new[] { assetLabels[0] });
        else
          AssetDatabase.SetLabels(s, null);
      }
      return assetLabels[0];
    }

    /// <summary>
    /// Packs the atlas.
    /// </summary>
    /// <param name="atlas">Atlas.</param>
    void PackAtlas(SpriteAtlas atlas)
    {
      var type = System.Type.GetType("UnityEditor.U2D.SpriteAtlasUtility, UnityEditor");
      var methodInfo = type.GetMethod("PackAtlases", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
      if (methodInfo != null)
      {
        methodInfo.Invoke(null, new object[] { new[] { atlas }, EditorUserBuildSettings.activeBuildTarget, true });
      }
    }

    Sprite GetOriginalSprite(SpriteAtlas atlas, string name)
    {
      if (!atlas || string.IsNullOrEmpty(name))
      {
        return null;
      }

      var spPackedSprites = new SerializedObject(atlas).FindProperty("m_PackedSprites");
      return Enumerable.Range(0, spPackedSprites.arraySize)
        .Select(index => spPackedSprites.GetArrayElementAtIndex(index).objectReferenceValue)
        .OfType<Sprite>()
        .FirstOrDefault(s => s.name == name);
    }

    //%%%% v Context menu for editor v %%%%
    [MenuItem("CONTEXT/Image/Convert To AtlasImage", true)]
    bool _ConvertToAtlasImage(MenuCommand command)
    {
      return CanConvertTo<AtlasImage>(command.context);
    }

    [MenuItem("CONTEXT/Image/Convert To AtlasImage", false)]
    void ConvertToAtlasImage(MenuCommand command)
    {
      ConvertTo<AtlasImage>(command.context);
    }

    [MenuItem("CONTEXT/Image/Convert To Image", true)]
    bool _ConvertToImage(MenuCommand command)
    {
      return CanConvertTo<Image>(command.context);
    }

    [MenuItem("CONTEXT/Image/Convert To Image", false)]
    void ConvertToImage(MenuCommand command)
    {
      ConvertTo<Image>(command.context);
    }

    /// <summary>
    /// Verify whether it can be converted to the specified component.
    /// </summary>
    protected bool CanConvertTo<T>(Object context)
      where T : MonoBehaviour
    {
      return context && context.GetType() != typeof(T);
    }

    /// <summary>
    /// Convert to the specified component.
    /// </summary>
    protected void ConvertTo<T>(Object context) where T : MonoBehaviour
    {
      var target = context as MonoBehaviour;
      var so = new SerializedObject(target);
      so.Update();

      var oldEnable = target.enabled;
      target.enabled = false;

      // Find MonoScript of the specified component.
      foreach (var script in Resources.FindObjectsOfTypeAll<MonoScript>())
      {
        if (script.GetClass() != typeof(T))
          continue;

        // Set 'm_Script' to convert.
        so.FindProperty("m_Script").objectReferenceValue = script;
        so.ApplyModifiedProperties();
        break;
      }

      (so.targetObject as MonoBehaviour).enabled = oldEnable;
    }
  }
}