using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D;
#if UNITY_IOS
#endif
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace FAIRSTUDIOS.Tools
{
  public class KEditor : EditorWindow
  {
    private static GUIStyle richLabel;
    internal static GUIStyle RichLabel
    {
      get
      {
        if (richLabel == null)
        {
          richLabel = new GUIStyle(EditorStyles.label)
          {
            wordWrap = true,
            richText = true
          };
        }

        return richLabel;
      }
    }

    private static GUIStyle panelWithBackground;
    internal static GUIStyle PanelWithBackground
    {
      get
      {
        if (panelWithBackground == null)
        {
          panelWithBackground = new GUIStyle(GUI.skin.box)
          {
            padding = new RectOffset()
          };
        }

        return panelWithBackground;
      }
    }

    private static GUIStyle compactButton;
    internal static GUIStyle CompactButton
    {
      get
      {
        if (compactButton == null)
        {
          compactButton = new GUIStyle(GUI.skin.button)
          {
            margin = RichLabel.margin,
            overflow = RichLabel.overflow,
            padding = new RectOffset(5, 5, 1, 4),
            richText = true
          };
          //compactButton.margin = new RectOffset(2, 2, 3, 2);
        }

        return compactButton;
      }
    }

    private Vector2 scrollPostion;

    BuildTarget Target
    {
      get
      {
#if UNITY_EDITOR
        return EditorUserBuildSettings.activeBuildTarget;
#elif UNITY_ANDROID
        return BuildTarget.Android;
#elif UNITY_IOS
        return BuildTarget.iOS;
#endif
      }
    }

    [MenuItem("Tools/K")]
    static void ShowWindow()
    {
      GetWindow(typeof(KEditor));
    }

    private void OnGUI()
    {
      scrollPostion = EditorGUILayout.BeginScrollView(scrollPostion);

      EditorGUILayout.Space();
      ApplicationInfo();

      EditorGUILayout.Space();
      DefineSymbol();

      EditorGUILayout.Space();
      FontChanger();

      EditorGUILayout.Space();
      Atlas();

      EditorGUILayout.Space();
      CreateAnimationInAnimator();

      EditorGUILayout.EndScrollView();
    }

    #region Application Info
    private void ApplicationInfo()
    {
      GUILayout.BeginVertical(PanelWithBackground);

      EditorGUILayout.LabelField("Version Info");

      PlayerSettings.productName = EditorGUILayout.TextField("Name", PlayerSettings.productName);

      PlayerSettings.bundleVersion = EditorGUILayout.TextField("Version", PlayerSettings.bundleVersion);

      // 버전 네이밍 룰 적용

#if UNITY_ANDROID
      PlayerSettings.Android.bundleVersionCode = EditorGUILayout.IntField("Bundle Version", PlayerSettings.Android.bundleVersionCode);
#elif UNITY_IOS
       PlayerSettings.iOS.buildNumber = EditorGUILayout.TextField("Build", PlayerSettings.iOS.buildNumber);
#endif

      GUILayout.EndVertical();
    }
    #endregion

    #region Define Symbols
    private bool USE_DEV_SYMBOL
    {
      get { return EditorPrefs.GetBool("DEV_SYMBOL"); }
      set { EditorPrefs.SetBool("DEV_SYMBOL", value); }
    }
    private bool USE_NEST_SYMBOL
    {
      get { return EditorPrefs.GetBool("NEST_SYMBOL"); }
      set { EditorPrefs.SetBool("NEST_SYMBOL", value); }
    }
    private bool USE_LOCAL_SERVER_SYMBOL
    {
      get { return EditorPrefs.GetBool("LOCAL_SERVER_SYMBOL"); }
      set { EditorPrefs.SetBool("LOCAL_SERVER_SYMBOL", value); }
    }
    private bool USE_EC2_SERVER_SYMBOL
    {
      get { return EditorPrefs.GetBool("EC2_SERVER_SYMBOL"); }
      set { EditorPrefs.SetBool("EC2_SERVER_SYMBOL", value); }
    }
    private bool USE_LOG_SYMBOL
    {
      get { return EditorPrefs.GetBool("LOG_SYMBOL"); }
      set { EditorPrefs.SetBool("LOG_SYMBOL", value); }
    }

    private void DefineSymbol()
    {
      GUILayout.BeginVertical(PanelWithBackground);

      EditorGUILayout.LabelField("Scripting Define Symbols");

      string getString = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(Target));

      EditorGUILayout.TextField(getString);

      EditorGUILayout.BeginHorizontal();

      USE_DEV_SYMBOL = GUILayout.Toggle(USE_DEV_SYMBOL, "Dev");
      if (USE_DEV_SYMBOL)
      {
        //심볼이 없을 때만 추가하도록 한다.
        if (!GetSymbol(getString, "DEV"))
          SetSymbol("DEV", BuildPipeline.GetBuildTargetGroup(Target));
      }
      else
      {
        //심볼이 있을 때만 제거하도록 한다.
        if (GetSymbol(getString, "DEV"))
          RemoveSymbol("DEV", BuildPipeline.GetBuildTargetGroup(Target));
      }

      //USE_NEST_SYMBOL = GUILayout.Toggle(USE_DEV_SYMBOL, "__USE_NEST__");
      //if (USE_NEST_SYMBOL)
      //{
      //  //심볼이 없을 때만 추가하도록 한다.
      //  if (!GetSymbol(getString, "__USE_NEST__"))
      //    SetSymbol("__USE_NEST__", BuildPipeline.GetBuildTargetGroup(Target));
      //}
      //else
      //{
      //  //심볼이 있을 때만 제거하도록 한다.
      //  if (GetSymbol(getString, "__USE_NEST__"))
      //    RemoveSymbol("__USE_NEST__", BuildPipeline.GetBuildTargetGroup(Target));
      //}

      //USE_LOCAL_SERVER_SYMBOL = GUILayout.Toggle(USE_DEV_SYMBOL, "LOCAL_TEST_SERVER");
      //if (USE_LOCAL_SERVER_SYMBOL)
      //{
      //  //심볼이 없을 때만 추가하도록 한다.
      //  if (!GetSymbol(getString, "LOCAL_TEST_SERVER"))
      //    SetSymbol("LOCAL_TEST_SERVER", BuildPipeline.GetBuildTargetGroup(Target));

      //  USE_EC2_SERVER_SYMBOL = false;
      //}
      //else
      //{
      //  //심볼이 있을 때만 제거하도록 한다.
      //  if (GetSymbol(getString, "LOCAL_TEST_SERVER"))
      //    RemoveSymbol("LOCAL_TEST_SERVER", BuildPipeline.GetBuildTargetGroup(Target));
      //}

      //USE_EC2_SERVER_SYMBOL = GUILayout.Toggle(USE_DEV_SYMBOL, "EC2_SERVER");
      //if (USE_EC2_SERVER_SYMBOL)
      //{
      //  //심볼이 없을 때만 추가하도록 한다.
      //  if (!GetSymbol(getString, "EC2_SERVER"))
      //    SetSymbol("EC2_SERVER", BuildPipeline.GetBuildTargetGroup(Target));

      //  USE_LOCAL_SERVER_SYMBOL = false;
      //}
      //else
      //{
      //  //심볼이 있을 때만 제거하도록 한다.
      //  if (GetSymbol(getString, "EC2_SERVER"))
      //    RemoveSymbol("EC2_SERVER", BuildPipeline.GetBuildTargetGroup(Target));
      //}

      USE_LOG_SYMBOL = GUILayout.Toggle(USE_LOG_SYMBOL, "Log");
      if (USE_LOG_SYMBOL)
      {
        //심볼이 없을 때만 추가하도록 한다.
        if (!GetSymbol(getString, "LOG"))
          SetSymbol("LOG", BuildPipeline.GetBuildTargetGroup(Target));
      }
      else
      {
        //심볼이 있을 때만 제거하도록 한다.
        if (GetSymbol(getString, "LOG"))
          RemoveSymbol("LOG", BuildPipeline.GetBuildTargetGroup(Target));
      }

      EditorGUILayout.EndHorizontal();

      EditorGUILayout.Space();
      if (GUILayout.Button("Clear Symbol", CompactButton))
      {
        USE_DEV_SYMBOL = false;
        USE_NEST_SYMBOL = false;
        USE_LOCAL_SERVER_SYMBOL = false;
        USE_EC2_SERVER_SYMBOL = false;
        USE_LOG_SYMBOL = false;

        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(Target), "");
      }

      GUILayout.EndVertical();
    }

    private bool GetSymbol(string symbols, string symbol)
    {
      bool result = false;

      if (symbols == symbol)
      {
        result = true;
      }
      else if (symbols.StartsWith(symbol + ';'))
      {
        result = true;
      }
      else if (symbols.EndsWith(';' + symbol))
      {
        result = true;
      }
      else if (symbols.Contains(';' + symbol + ';'))
      {
        result = true;
      }

      return result;
    }
    private void SetSymbol(string symbol, BuildTargetGroup buildTargetGroup)
    {
      string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
      if (symbols.Length == 0)
      {
        symbols = symbol;
      }
      else
      {
        if (symbols.EndsWith(";"))
        {
          symbols += symbol;
        }
        else
        {
          symbols += ';' + symbol;
        }
      }

      PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, symbols);
    }
    private void RemoveSymbol(string symbol, BuildTargetGroup buildTargetGroup)
    {
      string symbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

      if (symbols == symbol)
      {
        symbols = string.Empty;
      }
      else if (symbols.StartsWith(symbol + ';'))
      {
        symbols = symbols.Remove(0, symbol.Length + 1);
      }
      else if (symbols.EndsWith(';' + symbol))
      {
        symbols = symbols.Remove(symbols.LastIndexOf(';' + symbol, StringComparison.Ordinal), symbol.Length + 1);
      }
      else if (symbols.Contains(';' + symbol + ';'))
      {
        symbols = symbols.Replace(';' + symbol + ';', ";");
      }

      PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, symbols);
    }
    #endregion

    #region Font Changer
    private Font font;
    private GameObject fontChangeTarget;

    public void FontChanger()
    {
      GUILayout.BeginVertical(PanelWithBackground);

      EditorGUILayout.LabelField("Font Changer");

      // 씬에 존재하는 오브젝트와 모든 프리팹의 폰트 변경
      // 특정 프리팹의 폰트 변경

      // 변경할 폰트 선택
      font = (Font)EditorGUILayout.ObjectField("Font", font, typeof(Font), false);
      fontChangeTarget = (GameObject)EditorGUILayout.ObjectField("Prefab", fontChangeTarget, typeof(GameObject), false);

      if (font != null)
      {
        if (GUILayout.Button("Change Scene Object and All Prefab Font", CompactButton))
        {
          ChangeSceneObjectFont();
          ChagneAllPrefabFont();
          AssetDatabase.Refresh();
        }

        if (fontChangeTarget != null)
        {
          if (GUILayout.Button("Change Prefab Font", CompactButton))
          {
            ChangePrefabFont();
            AssetDatabase.Refresh();
          }
        }
      }

      GUILayout.EndVertical();
    }

    private void ChangeSceneObjectFont()
    {
      int matches = 0;
      for (int i = 0; i < SceneManager.sceneCount; i++)
      {
        Scene scene = SceneManager.GetSceneAt(i);
        List<GameObject> gos = new List<GameObject>(scene.GetRootGameObjects());
        foreach (GameObject go in gos)
        {
          matches += ReplaceFonts(font, go.GetComponentsInChildren<Text>(true));
          EditorUtility.DisplayProgressBar("폰트 변경중", string.Format("{0} 씬 체크 변경중...", scene.name), matches / (float)gos.Count);
        }
      }

      EditorUtility.ClearProgressBar();
    }

    private void ChagneAllPrefabFont()
    {
      int matches = 0;
      string[] prefabsGUID = AssetDatabase.FindAssets("t:Prefab");
      string path;
      GameObject prefab;
      Text[] texts;
      foreach (string GUID in prefabsGUID)
      {
        path = AssetDatabase.GUIDToAssetPath(GUID);
        prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        texts = prefab.GetComponentsInChildren<Text>(true);
        if (texts.Length > 0)
        {
          EditorUtility.DisplayProgressBar("폰트 변경중", string.Format("{0} 프리팹 체크 변경중...", prefab.name), matches / (float)prefabsGUID.Length);
          matches += ReplaceFonts(font, texts);

          PrefabUtility.SaveAsPrefabAsset(prefab, path);
        }
      }

      EditorUtility.ClearProgressBar();
    }

    private void ChangePrefabFont()
    {
      int matches = 0;
      string[] prefabsGUID = AssetDatabase.FindAssets("t:Prefab");
      string path;
      GameObject prefab;
      Text[] texts;
      foreach (string GUID in prefabsGUID)
      {
        path = AssetDatabase.GUIDToAssetPath(GUID);
        prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        texts = prefab.GetComponentsInChildren<Text>(true);
        if (texts.Length > 0)
        {
          EditorUtility.DisplayProgressBar("폰트 변경중", string.Format("{0} 프리팹 체크 변경중...", prefab.name), matches / (float)prefabsGUID.Length);
          matches += ReplaceFonts(font, texts);

          PrefabUtility.SaveAsPrefabAsset(prefab, path);
        }
      }

      EditorUtility.ClearProgressBar();
    }

    private int ReplaceFonts(Font _nowFont, IEnumerable<Text> texts)
    {
      int matches = 0;
      IEnumerable<Text> textsFiltered = texts;
      foreach (Text text in textsFiltered)
      {
        text.font = _nowFont;
        matches++;
      }

      return matches;
    }
    #endregion

    #region Atlas
    private SpriteAtlas atlas;

    public void Atlas()
    {
      GUILayout.BeginVertical(PanelWithBackground);

      EditorGUILayout.LabelField("Atlas");

      atlas = (SpriteAtlas)EditorGUILayout.ObjectField("Sprite Atlas", atlas, typeof(SpriteAtlas), false);

      if(atlas != null)
      {
        if (GUILayout.Button("Packing", CompactButton))
        {
          SpriteAtlas[] atlaslist = new SpriteAtlas[1] { atlas };
          SpriteAtlasUtility.PackAtlases(atlaslist, EditorUserBuildSettings.activeBuildTarget);
        }
      }

      if (GUILayout.Button("Packing All", CompactButton))
      {
        SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget);
      }

      GUILayout.EndVertical();
    }
    #endregion

    #region Animation
    private AnimatorController animatorController;
    private string clipName = string.Empty;
    private AnimationClip addClip = null;
    private bool[] removeClips;

    public void CreateAnimationInAnimator()
    {
      GUILayout.BeginVertical(PanelWithBackground);

      animatorController = (AnimatorController)EditorGUILayout.ObjectField("Animator Controller", animatorController, typeof(AnimatorController), false);
      EditorGUILayout.Space();

      if(animatorController != null)
      {
        clipName = EditorGUILayout.TextField("Create Clip Name", clipName);

        var overlap = false;
        foreach (var clip in animatorController.animationClips)
        {
          if (clip.name == clipName)
          {
            overlap = true;
            break;
          }
        }

        if (overlap || string.IsNullOrEmpty(clipName))
        {

        }
        else
        {
          if (GUILayout.Button("Create Animation", CompactButton))
          {
            var animationClip = new AnimationClip {name = clipName};
            AssetDatabase.AddObjectToAsset(animationClip, animatorController);
            animatorController.AddMotion(animationClip);

            AssetDatabase.SaveAssets();
          }
          EditorGUILayout.Space();

          // addClip = (AnimationClip)EditorGUILayout.ObjectField("Add Animation", addClip, typeof(AnimationClip), false);
          // if (addClip != null)
          // {
          //   if (GUILayout.Button("Add Animation", CompactButton))
          //   {
          //     AnimationUtility.GetEditorCurve()
          //     AnimationUtility.cur
          //     var animationClip = new AnimationClip {name = clipName};
          //     AssetDatabase.AddObjectToAsset(animationClip, animatorController);
          //     animatorController.AddMotion(animationClip);
          //
          //     AssetDatabase.SaveAssets();
          //   }
          // }
        }

        EditorGUILayout.Space();
        if (removeClips == null || removeClips.Length != animatorController.animationClips.Length)
        {
          removeClips = new bool[animatorController.animationClips.Length];
        }

        var showRemove = false;
        for (var i = 0; i < animatorController.animationClips.Length; i++)
        {
          var clip = animatorController.animationClips[i];
          removeClips[i] = EditorGUILayout.Toggle(clip.name, removeClips[i]);
          if (removeClips[i] && !showRemove)
          {
            showRemove = true;
          }
        }

        if (showRemove && GUILayout.Button("Remove Animation", CompactButton))
        {
          for (var i = 0; i < removeClips.Length; i++)
          {
            var remove = removeClips[i];
            if (remove)
            {
              AssetDatabase.RemoveObjectFromAsset(animatorController.animationClips[i]);
            }
          }

          AssetDatabase.SaveAssets();
        }
      }

      GUILayout.EndVertical();
    }
    #endregion
  }
}
