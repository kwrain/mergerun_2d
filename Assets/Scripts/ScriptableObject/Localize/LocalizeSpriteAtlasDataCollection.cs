/**
* LocalizeSpriteAtlasCollection.cs
* 작성자 : lds3794@gmail.com
* 작성일 : 2022년 05월 18일 오후 3시 09분
*/
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FAIRSTUDIOS.SODB.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.U2D;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Compilation;
#endif

[PreferBinarySerialization]
[CreateAssetMenu(fileName = "LocalizeSpriteAtlasCollection", menuName = "Data/Localize/LocalizeSpriteAtlasCollection")]
public class LocalizeSpriteAtlasDataCollection : ScriptableObject
{
  [SerializeField] private GenericDictionary<string, string> localizeSAMap;
  
  public string GetLocalizeSpriteAtlasAddress(string atlasName)
  {
    if (localizeSAMap.ContainsKey(atlasName) == false) return null;
    return localizeSAMap[atlasName];
  }

#if UNITY_EDITOR
  public string path;
  public string addressablesLabel;

  [ContextMenu("CollectLocalizeSpriteAtlas")]
  public void RequestCollectLocalizeSpriteAtlas(bool RequestScriptCompilation)
  {
    // 단일 호출 시
    if(RequestScriptCompilation == true)
    {
      CompilationPipeline.compilationFinished -= OnCollectLocalizeSpriteAtlasAfterCompilationFinished;
      CompilationPipeline.compilationFinished += OnCollectLocalizeSpriteAtlasAfterCompilationFinished;
      CompilationPipeline.RequestScriptCompilation();
    }
    // All 빌드 시
    else
    {
      OnCollectLocalizeSpriteAtlasAfterCompilationFinished(null);
    }
  }

  private void OnCollectLocalizeSpriteAtlasAfterCompilationFinished(object value)
  {
    Debug.Log($"[Localize Atlas Build] Start");
    var op = Addressables.LoadResourceLocationsAsync(addressablesLabel, typeof(SpriteAtlas));
    var locators = op.WaitForCompletion();
    var map = new Dictionary<string, string>();
    foreach (var locator in locators)
      map[locator.InternalId] = locator.PrimaryKey;
    localizeSAMap = new();
    var guids = AssetDatabase.FindAssets("t:" + typeof(SpriteAtlas).Name);
    var paths = guids.Select(x => AssetDatabase.GUIDToAssetPath(x));
    foreach (var path in paths)
    {
      if (map.ContainsKey(path) == false) continue;
      var splits = path.Split(Path.DirectorySeparatorChar);
      var fullName = splits[^1];
      var atlasName = fullName.Replace(".spriteatlas", "");
      if (atlasName.Contains("_") == false) continue;
      localizeSAMap[atlasName] = map[path];
    }
    EditorUtility.SetDirty(this);
    AssetDatabase.SaveAssetIfDirty(this);
    Debug.Log($"[Localize Atlas Build] Finished");

    CompilationPipeline.compilationFinished -= OnCollectLocalizeSpriteAtlasAfterCompilationFinished;
  }
#endif
}