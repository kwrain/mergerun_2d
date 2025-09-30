using FAIRSTUDIOS.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace FAIRSTUDIOS.Tools
{
  public class MenuEditor : Editor
  {
    #region Prefs
    [MenuItem("Tools/Delete All PlayerPrefs")]
    public static void DeleteAllPlayerPrefs()
    {
      PlayerPrefs.DeleteAll();
    }

    [MenuItem("Tools/Delete All UserPrefs")]
    public static void DeleteAllUserPrefs()
    {
      UserPrefs.DeleteAll();
    }

    [MenuItem("Tools/Delete All DevicePrefs")]
    public static void DeleteAllDevicePrefs()
    {
      DevicePrefs.DeleteAll();
    }
    
    #endregion

    #region  UI

    [MenuItem("GameObject/UI/Extensions/AtlasImage")]
    public static void CreateAtlasImage(MenuCommand menuCommand)
    {
      GameObject go = CreateCustomGameObject("AtlasImage", menuCommand);

      go.AddComponent<AtlasImage>();
    }

    [MenuItem("GameObject/UI/Extensions/Button")]
    public static void CreateCustomButton(MenuCommand menuCommand)
    {
      GameObject go = CreateCustomGameObject("Button", menuCommand);

      go.AddComponent<AtlasImage>();
      go.AddComponent<KButton>();
    }

    [MenuItem("GameObject/UI/Extensions/Button Add Text")]
    public static void CreateCustomButtonAddText(MenuCommand menuCommand)
    {
      GameObject go = CreateCustomGameObject("Button", menuCommand);

      go.AddComponent<AtlasImage>();
      go.AddComponent<KButton>();

      GameObject goChild = new GameObject("Text");
      goChild.SetParent(go);

      Text text = goChild.AddComponent<Text>();
      text.alignment = TextAnchor.MiddleCenter;
      text.font = Resources.Load<Font>("Fonts/JUA Noto Sans KR");
      text.text = "Button";
    }

    [MenuItem("GameObject/UI/Extensions/Toggle")]
    public static void CreateCustomToggle(MenuCommand menuCommand)
    {
      GameObject go = Resources.Load<GameObject>("Toggle");
      go = Instantiate(go);
      go.name = "Toggle";

      GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
    }

    [MenuItem("GameObject/UI/Extensions/Toggle Group")]
    public static void CreateCustomToggleGroup(MenuCommand menuCommand)
    {
      GameObject go = CreateCustomGameObject("ToggleGroup", menuCommand);

      go.AddComponent<KToggleGroup>();
    }

    [MenuItem("GameObject/UI/Extensions/Toggle Switch")]
    public static void CreateCustomToggleSwitch(MenuCommand menuCommand)
    {
      GameObject go = Resources.Load<GameObject>("ToggleSwitch");
      go = Instantiate(go);
      go.name = "ToggleSwitch";

      GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
    }

    [MenuItem("GameObject/UI/Extensions/ProgressBar")]
    public static void CreateCustomProgressBar(MenuCommand menuCommand)
    {
      GameObject go = CreateCustomGameObject("ProgressBar", menuCommand);

      go.AddComponent<RectTransform>();
      go.AddComponent<KProgressBar>();
    }
    
    #endregion

    #region Tween
    
    public static void CreateTweenAlpha(MenuCommand menuCommand)
    {
      GameObject go = CreateCustomGameObject("TweenAlpha", menuCommand);

      go.AddComponent<KTweenAlpha>();
    }
    
    public static void CreateTweenColor(MenuCommand menuCommand)
    {
      GameObject go = CreateCustomGameObject("TweenColor", menuCommand);

      go.AddComponent<KTweenColor>();
    }
    
    public static void CreateTweenHSV(MenuCommand menuCommand)
    {
      GameObject go = CreateCustomGameObject("TweenHSV", menuCommand);

      go.AddComponent<KTweenHSV>();
    }
    
    public static void CreateTweenPosition(MenuCommand menuCommand)
    {
      GameObject go = CreateCustomGameObject("TweenPosition", menuCommand);

      go.AddComponent<KTweenPosition>();
    }
    
    public static void CreateTweenScale(MenuCommand menuCommand)
    {
      GameObject go = CreateCustomGameObject("TweenScale", menuCommand);
      go.AddComponent<KTweenScale>();
    }
    
    public static void CreateTweenShake(MenuCommand menuCommand)
    {
      GameObject go = CreateCustomGameObject("TweenShake", menuCommand);

      go.AddComponent<KTweenShake>();
    }
    
    #endregion

    static GameObject CreateCustomGameObject(string name, MenuCommand menuCommand)
    {
      GameObject go = new GameObject(name);
      GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
      Undo.RegisterCreatedObjectUndo(go, string.Format("Create {0}", go.name));
      Selection.activeGameObject = go;
      return go;
    }
  }
}