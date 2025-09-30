#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class GlobalDataAccessor : ScriptableObject
{
  private const string SettingFileDirectory = "Assets/Editor Default Resources/BuildAssets";
  private const string SettingFilePath = "Assets/Editor Default Resources/BuildAssets/GlobalDataAccessor.asset";

  private static GlobalDataAccessor instance;

  public static GlobalDataAccessor Instance
  {
    get
    {
      if (instance != null) return instance;

      instance = Resources.Load<GlobalDataAccessor>(path: "BuildAssets/GlobalDataAccessor");

      if (instance == null)
      {
        if (AssetDatabase.IsValidFolder(path: "Assets/Editor Default Resources") == false)
        {
          AssetDatabase.CreateFolder(parentFolder: "Assets", newFolderName: "Editor Default Resources");
        }
        if (AssetDatabase.IsValidFolder(path: SettingFileDirectory) == false)
        {
          AssetDatabase.CreateFolder(parentFolder: "Assets/Editor Default Resources", newFolderName: "BuildAsset");
        }

        instance = AssetDatabase.LoadAssetAtPath<GlobalDataAccessor>(SettingFilePath);

        if (instance == null)
        {
          instance = CreateInstance<GlobalDataAccessor>();
          AssetDatabase.CreateAsset(instance, SettingFilePath);
        }
      }
      return instance;
    }
  }

  [SerializeField] private SoundDataCollection soundDataCollection;
  [SerializeField] private LocalizeTextDataCollection localizeTextAssetCollection;
  [SerializeField] private LocalizeSpriteAtlasDataCollection localizeSpriteAtlasCollection;

  public SoundDataCollection SoundDataCollection => soundDataCollection;
  public LocalizeTextDataCollection LocalizeTextAssetCollection => localizeTextAssetCollection;
  public LocalizeSpriteAtlasDataCollection LocalizeSpriteAtlasCollection => localizeSpriteAtlasCollection;
}
#endif
