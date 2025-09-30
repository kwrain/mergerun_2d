using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.U2D;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.AsyncOperations;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class ResourceManager : Singleton<ResourceManager>
{
  /// <summary>
  /// 로컬 번들 루트
  /// </summary>
  public const string LOCAL_BUNDLE_ROOT = "BundleLocal/";
  /// <summary>
  /// 뉴 클라이언트 번들 루트
  /// </summary>
  public const string NEW_BUNDLE_ROOT = "BundleNew/";
  // AssetBundle
  //  - 번들 관련 내용을 추후 재설계한다.
  //  - 번들 관련 모든 소스코드를 제거한다.

  public const string LOCALIZE_SPRITE_ATLAS_EXTRA_KEY = "####";

  // Atlas
  //  - 아틀라스는 Unity 에서 제공하는 SpriteAtlas 를 사용한다.
  //  - SpriteAtlas 를 사용함으로 기존 코드는 제거한다.
  //  - 텍스쳐 품질을 구분하여 아틀라스를 로드한다.
  //  - 게임 플레이 중 텍스쳐 품질을 변경 시 기존 리소스를 스왑한다.
  //  - 아틀라스 목록의 관리 여부

  private SpriteAtlas chunkSpriteAtlas = null;

  // 한번 로드 한 오브젝트 캐싱
  // 단발성인 경우 해지
  // 우선순위로 메모리 해제
  // 적정 메모리 이상 벗어나면 앞에서부터 메모리를 해제해야한다.
  private Dictionary<string, UnityEngine.Object> m_LoadedObjects = null;// = new();
  private Dictionary<string, Sprite[]> m_LoadedSpriteAtlas = null;// = new();
  private Dictionary<string, SpriteAtlas> m_LoadedLocalizeAtlas = null;// new();

  private Dictionary<string, UnityEngine.Object> LoadedObject => m_LoadedObjects ??= new();
  private Dictionary<string, Sprite[]> LoadedSpriteAtlas => m_LoadedSpriteAtlas ??= new();
  private Dictionary<string, SpriteAtlas> LoadedLocalizeAtlas => m_LoadedLocalizeAtlas ??= new();

  public Dictionary<string, IResourceLocation> LocalLocatorMap { get; set; }
  public Dictionary<string, IResourceLocation> RemoteLocatorMap { get; set; }

  public SpriteAtlas GetLocalizeAtlas(string atlasName, ELanguageCode languageCode)
  {
    // 언어가 변경되지 않은경우.
    // cf. 이 함수를 호출하는 곳에서 먼저 언어변경 체크
    if (languageCode == Localize.ELanguageCode)
    {
      if (LoadedLocalizeAtlas.ContainsKey(atlasName) == true)
        return LoadedLocalizeAtlas[atlasName];
    }

    var splits = atlasName.Split('_');
    var targetAtlasName = $"{splits[0]}_{Localize.LanguageCode}"; // 아틀라스_국가코드
    var atlasKey = $"{splits[0]}_{LOCALIZE_SPRITE_ATLAS_EXTRA_KEY}";

    // 현재 국가코드에 대한 아틀라스가 캐싱 되었다면 해당 아틀라스를 반환
    if (LoadedLocalizeAtlas.ContainsKey(targetAtlasName) == true)
      return LoadedLocalizeAtlas[targetAtlasName];

    // 캐싱해둔 로컬라이즈 아틀라스에 대한 번들 어드레스를 가져옴
    // string address = SOAssetManager.Instance.LocalizeSpriteAtlasCollection.GetLocalizeSpriteAtlasAddress(targetAtlasName);
    string address = null;
    if (string.IsNullOrEmpty(address) == true) return null; // 해당 번들 어드레스가 없는 경우
     // 해당 번들 어드레스가 어드레서블에도 없는 경우
    if (RemoteLocatorMap.ContainsKey(address) == false && LocalLocatorMap.ContainsKey(address) == false) return null;

    // // todo - 이 함수는 AtlasImage의 OnEnable() 이벤트 함수에서 호출되고 있어 동시에 여럿이 OnEnable()이 호출되는 경우 아래 동작을 동시에 하는지 체크 필요
    // 가장 먼저 호출한 곳에서만 LoadAssetAsync 진행하는 것 확인 완료
    var sa = LoadAssetAtAddressables<SpriteAtlas>(address);
    LoadedLocalizeAtlas[targetAtlasName] = sa;  // 캐싱

    var atlas = new Sprite[sa.spriteCount];
    sa.GetSprites(atlas);
    if (atlas is not null)
    {
      LoadedSpriteAtlas[atlasKey] = atlas; // 변경된 언어 코드에 대한 스프라이트로 변경 해서 갖고있게 함.
    }

    return LoadedLocalizeAtlas[targetAtlasName]; // 반환
  }

  public override void OnDestroyObject()
  {
    Clear();

    m_LoadedObjects = null;

    base.OnDestroyObject();
  }

  protected override void OnDestroy()
  {
    Clear();
    base.OnDestroy();
  }

  public void Clear()
  {
    LoadedObject.Clear();
    if (loadAssetOperationHandles is { Count: > 0 })
    {
      foreach (var item in loadAssetOperationHandles)
      {
        if (item.IsValid() == false)
        {
          continue;
        }
        Addressables.Release(item);
      }
      loadAssetOperationHandles.Clear();
    }
  }

  public bool FileExist<T>(string assetPath) where T : UnityEngine.Object { return null != Load<T>(assetPath); }

  #region Addressables

  /// <summary>
  /// lds - 23.6.22 <br/>
  /// 어드레서블을 통해 동기 혹은 비동기로 에셋을 로드할 때 핸들이 생성된다.<br/>
  /// 만약에 아직 완료되지 않은 핸들이 있는데 게임이 종료가 되거나 ResourceManager 객체가 파괴되는 경우 즉시 핸들을 릴리즈 해줘야한다. <br/>
  /// 따라서 loadAssetOperationHandles에 로드를 시작할 때 추가하고 완료되면 제거 하며 <br/>
  /// 필요한 상황에 릴리즈 할 수 있도록 한다. (ex OnDestroy 내에서)
  /// </summary>
  private List<AsyncOperationHandle> loadAssetOperationHandles;

  private T LoadAssetAtAddressables<T>(string assetPath)
  {
#if UNITY_EDITOR
    var sw = new System.Diagnostics.Stopwatch();
    sw.Start();
#endif
    T obj = default;
    var op = Addressables.LoadAssetAsync<T>(assetPath);
    loadAssetOperationHandles ??= new(); // 객체가 없으면 생성
    loadAssetOperationHandles.Add(op);
    //Debug.Log($"핸들 시작 : {loadAssetOperationHandles.Count} (handle Count)");
    op.WaitForCompletion();
    if (op.Status == AsyncOperationStatus.Succeeded)
    {
      obj = op.Result;
    }
    else if (op.Status == AsyncOperationStatus.Failed)
    {
      // 실패시에 핸들을 반드시 해제 해줘야함.
      loadAssetOperationHandles.Remove(op);
      Addressables.Release(op);
    }
    //Debug.Log($"핸들 종료 : {loadAssetOperationHandles.Count} (handle Count)");
#if UNITY_EDITOR
    sw.Stop();
    //Debug.Log($"어드레서블 에셋 로드 : {assetPath}는 {sw.ElapsedMilliseconds * 0.001f}초 걸림");
#endif
    return obj;
  }

  private T LoadAssetAtAddressables<T>(IResourceLocation resourceLocation)
  {
#if UNITY_EDITOR
    var sw = new System.Diagnostics.Stopwatch();
    sw.Start();
#endif
    T obj = default;
    var op = Addressables.LoadAssetAsync<T>(resourceLocation);
    loadAssetOperationHandles ??= new(); // 객체가 없으면 생성
    loadAssetOperationHandles.Add(op);
    //Debug.Log($"핸들 시작 : {loadAssetOperationHandles.Count} (handle Count)");
    op.WaitForCompletion();
    if (op.Status == AsyncOperationStatus.Succeeded)
    {
      obj = op.Result;
    }
    else if (op.Status == AsyncOperationStatus.Failed)
    {
      // 실패시에 핸들을 반드시 해제 해줘야함.
      loadAssetOperationHandles.Remove(op);
      Addressables.Release(op);
    }
    //Debug.Log($"핸들 종료 : {loadAssetOperationHandles.Count} (handle Count)");
#if UNITY_EDITOR
    sw.Stop();
    //Debug.Log($"어드레서블 에셋 로드 : {resourceLocation.PrimaryKey}는 {sw.ElapsedMilliseconds * 0.001f}초 걸림");
#endif
    return obj;
  }

  public string GetNewBundlePath(string assetPathWithExt) => $"{NEW_BUNDLE_ROOT}{assetPathWithExt}";
  public string GetLocalBundlePath(string assetPathWithExt) => $"{LOCAL_BUNDLE_ROOT}{assetPathWithExt}";
  private void PrintNotFoundAddressableAtRemoteBundle(string newAssetPath)
  {
#if RTS_ADDRESSABLES_PRINT_WARNING
    Debug.LogWarning($"[Addressable] key not found at bundle : {newAssetPath}");
#endif
  }
  private void PrintNotFoundAddressableAtLocalBundle(string assetPath)
  {
#if RTS_ADDRESSABLES_PRINT_WARNING
    Debug.LogWarning($"[Addressable] key not found at bundle : {assetPath}");
#endif
  }

  #endregion Addressables

  public string GetResourceExtension<T>(string extension = null) where T : UnityEngine.Object
  {
    string ext = string.Empty;
    if (string.IsNullOrEmpty(extension) == true)
    {
      if (typeof(T) == typeof(GameObject))
        ext = ".prefab";
      else if (typeof(T) == typeof(Font))
        ext = ".otf"; // ttf
      else if (typeof(T) == typeof(Material))
        ext = ".mat";
      else if (typeof(T) == typeof(Texture2D) || typeof(T) == typeof(Sprite) || typeof(T) == typeof(Sprite[]))
        ext = ".png";
      else if (typeof(T) == typeof(RuntimeAnimatorController))
        ext = ".controller";
      //else if (typeof(T) == typeof(AudioClip))
      //  ext = ".mp3";
      else if (typeof(T) == typeof(SpriteAtlas))
        ext = ".spriteatlas";
      else
        ext = null;
    }
    else
      ext = extension;
    return ext;
  }

  /// <summary>
  /// lds - 23.12.19, 어드레서블이 초기화 되기전에 필요한 로컬(빌트인) 리소스에 대해 미리 리소스를 세팅한다. <br/>
  /// 단, 정말로 필요한 경우가 아니면 대부분의 경우 일반적으로 <seealso cref="Load"/> 메소드를 사용하여 로드할 것. <br/>
  /// 예를들어 <seealso cref="PopupAlert"/> <seealso cref="PopupConfirm"/> <seealso cref="PopupError"/> 들은 <br/>
  /// 게임이 시작되어 끝날 때까지 시스템 또는 일반적으로 사용되는 팝업이므로 어드레서블을 사요하지 않고 직접 로드함
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="address"></param>
  /// <param name="targetObject"></param>
  /// <param name="extension"></param>
  public void SetBuiltinLoadedObject<T>(string address, T targetObject) where T : UnityEngine.Object
  {
    if(LoadedObject.ContainsKey(address) == true)
    {
      return;
    }
    LoadedObject[address] = targetObject;
  }

  public T Load<T>(string assetPath, Action<T> callback = null, string extension = null) where T : UnityEngine.Object
  {
    T obj = null;

    var key = assetPath;
    if(typeof(T) == typeof(Sprite))
    {
      key += "(Sprite)";
    }

    if (LoadedObject.ContainsKey(key))
    {
      obj = (T)LoadedObject[key];
    }
    else
    {
      var ext = GetResourceExtension<T>(extension);

#if UNITY_EDITOR
      if (obj is null)
        obj = AssetDatabase.LoadAssetAtPath<T>(assetPath);
#endif

      var locatorMap = RemoteLocatorMap;
      if (obj is null)
      {
        if (locatorMap is not null)
        {
          // lds - 직접 Full 번들 어드레스가 경로 이름으로 들어왔을 때 (확장자가 있는 경우 확장자를 포함해서)
          if (locatorMap.ContainsKey(assetPath) == true)
          {
            obj = LoadAsset(locatorMap[assetPath]);
          }
          else
          {
            // 어드레스의 루트를 제외한 경우
            // 신규 리소스 폴더 먼저 체크
            // 만약 없으면 구 리소스 폴더 체크
            var assetPathWithExt = $"{assetPath}{ext}";
            var newAssetPath = GetNewBundlePath(assetPathWithExt);
            if (locatorMap.ContainsKey(newAssetPath) == true)
            {
              obj = LoadAsset(locatorMap[newAssetPath]);
            }
            else
            {
              PrintNotFoundAddressableAtRemoteBundle(newAssetPath);
            }
          }
        }
      }

      locatorMap = LocalLocatorMap;
      if (obj is null)
      {
        // 로컬 리소스에서 가져오기
        if(locatorMap is not null)
        {
          // lds - 직접 Full 번들 어드레스가 경로 이름으로 들어왔을 때 (확장자가 있는 경우 확장자를 포함해서)
          if (locatorMap.ContainsKey(assetPath) == true)
          {
            obj = LoadAsset(locatorMap[assetPath]);
          }
          else
          {
            var assetPathWithExt = $"{assetPath}{ext}";
            var localAssetPath = GetLocalBundlePath(assetPathWithExt);
            if (locatorMap.ContainsKey(localAssetPath) == true)
            {
              obj = LoadAsset(locatorMap[localAssetPath]);
            }
            else
            {
              PrintNotFoundAddressableAtLocalBundle(localAssetPath);
            }
          }
        }
      }

      if (obj is not null)
      {
        LoadedObject.Add(key, obj);
      }
    }

    if (obj is null)
    {
#if UNITY_EDITOR
      // Debug.LogFormat("[BUNDLE] Asset not found at path : {0}", assetPath);
#endif
    }
    else if (callback is not null)
    {
      callback.Invoke(obj);
    }

    return obj;

    T LoadAsset(IResourceLocation resourceLocation)
    {
      T obj = default;
      if (typeof(T) == typeof(Sprite))
      {
        var texture = LoadAssetAtAddressables<Texture2D>(resourceLocation);
        obj = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), 0.5f * Vector2.one) as T;
        obj.name = texture.name;
      }
      else
      {
        obj = LoadAssetAtAddressables<T>(resourceLocation);
      }
      return obj;
    }
  }

  /// <summary>
  /// 아틀라스 로드
  /// </summary>
  /// <param name="atlasPath"></param>
  /// <param name="caching"> 아틀라스 캐싱 여부 </param>
  /// <returns></returns>
  private Sprite[] LoadAtlas(string atlasPath, bool caching = true, bool isLocalize = false)
  {
    //atlasPath = string.Format(ATLAS_PATH, atlasPath);
    //atlasPath = string.Format(ATLAS_PATH, atlasPath);

    var index = atlasPath.LastIndexOf('/');
    var atlasName = index >= 0 ? atlasPath.Substring(index + 1) : atlasPath;
    if(isLocalize)
    {
      // lds - 24.7.13, 로컬라이즈 아틀라스에 접근 하려고할 땐, 추가 키값을 지정한 atlasName을 사용한다.
      var splits = atlasName.Split('_');
      atlasName = $"{splits[0]}_{LOCALIZE_SPRITE_ATLAS_EXTRA_KEY}";
    }
    if (LoadedSpriteAtlas.ContainsKey(atlasName))
    {
      return LoadedSpriteAtlas[atlasName];
    }
    else
    {
      var ext = ".png";

      UnityEngine.Object[] objects = null;
#if UNITY_EDITOR
      if (null == objects || objects.Length == 0)
        objects = AssetDatabase.LoadAllAssetsAtPath(atlasPath);
#endif

      // 임시 코드
      if (null == objects || objects.Length == 0)
      {
        var assetPathWithExt = $"{atlasPath}{ext}";
        var newAssetPath = GetNewBundlePath(assetPathWithExt);
        var locatorMap = RemoteLocatorMap;
        if (locatorMap is not null)
        {
          if (locatorMap.ContainsKey(newAssetPath) == true)
          {
            objects = LoadAssetAtAddressables<Sprite[]>(newAssetPath);
          }
          else
          {
            PrintNotFoundAddressableAtRemoteBundle(newAssetPath);
          }
        }

        locatorMap = LocalLocatorMap;
        if(objects is null)
        {
          if (locatorMap is not null)
          {
            var localAssetPath = GetLocalBundlePath(assetPathWithExt);
            if (locatorMap.ContainsKey(localAssetPath) == true)
            {
              objects = LoadAssetAtAddressables<Sprite[]>(locatorMap[localAssetPath]);
            }
            else
            {
              PrintNotFoundAddressableAtLocalBundle(localAssetPath);
            }
          }
        }
      }

      Sprite[] atlas = null;
      if (objects != null && objects.Length > 0)
      {
        atlas = objects.OfType<Sprite>().ToArray();
      }

      if (atlas == null || atlas.Length == 0)
      {
        var spriteAtlas = Load<SpriteAtlas>(atlasPath);

        if (spriteAtlas == null)
        {
          Debug.LogFormat("Atlas not found at path: {0}", atlasPath);
          return null;
        }

        atlas = new Sprite[spriteAtlas.spriteCount];
        spriteAtlas.GetSprites(atlas);
      }

      if (caching)
      {
        if(atlas is not null)
        {
          LoadedSpriteAtlas.Add(atlasName, atlas);
        }
      }

      return atlas;
    }
  }
  public Sprite LoadSpriteInAtlas(string atlasPath, string spriteName, bool isLocalize = false)
  {
    Sprite sprite = null;
    var atlas = LoadAtlas(atlasPath, isLocalize);
    spriteName = string.Format($"{spriteName}(Clone)");
    for (var i = 0; i < atlas.Length; i++)
    {
      if (spriteName == atlas[i].name)
      {
        sprite = atlas[i];
        break;
      }
    }

    if (null == sprite)
    {
      Debug.LogFormat("Atlas has no sprite. AtlasPath : {0}, Sprite Name : {1}", atlasPath, spriteName.Replace("(Clone)", string.Empty));
    }

    return sprite;
  }

  public void AddAtlas(SpriteAtlas spriteAtlas)
  {
    if (spriteAtlas == null || LoadedSpriteAtlas.ContainsKey(spriteAtlas.name))
      return;
    var sprites = new Sprite[spriteAtlas.spriteCount];
    spriteAtlas.GetSprites(sprites);

    LoadedSpriteAtlas.TryAdd(spriteAtlas.name, sprites);
    // lds - 24.8.23 확인사항
    // 현재 씬 구조는 Splash > DownloadContent > Login > Island 임
    // 테스트를 위해 디파인 심볼을 통해 Splash와 DownloadContent 사이에 CheatScene을 추가하는 경우가 있음
    // 이 떄 Splash > CheatScene > DownloadContent 순으로 로드 하는경우
    // ArgumentException: An item with the same key has already been added. Key: LocalCommon
    // 위와 같은 에러 로그가 확인됨.
    // 라이브에서는 CheatScene을 사용할일이 없기 때문에 이슈가되는 사항은 아니지만, 추후 에러 트래킹이 필요할 수 있어 히스토리를 위해 남김
    // BaseScene을 상속받지 않는 Scene을 거쳐가는 경우에도 동일한 에러가 발생하기 때문에 BaseScene 상속과는 관련없음.

    // 디버깅 결과는 다음과 같습니다.
    // 1. AddAtlas에 처음 접근된 경우 spriteAtlas.GetSprites(sprites); 까지 진행
    // 2. LoadedSpriteAtlas.Add(spriteAtlas.name, sprites); 가 처리되지 않고 다시 AddAtlas가 호출됨
    // 3. 이 때 spriteAtlas.GetSprites(sprites); 까지 다시 진행되며
    // 4. LoadedSpriteAtlas.Add(spriteAtlas.name, sprites); 가 처리되고 빠져나오며
    // 5. 2번에 의해 LoadedSpriteAtlas.Add(spriteAtlas.name, sprites);가 한번 더 처리됨
    // 6. 이 때 동일한 key값으로 추가를 하려하기 때문에 에러가 발생함.

    // lds - 24.9.4 추가 확인사항
    // CheatScene을 거쳐가지 않아도 동일한 에러가 발생하는 경우가 있음을 확인함
    // 이는 유니티를 키고 처음 플레이하는 경우에 발생, 디바이스에서는 발생하지 않을 수 있음
    // 조치 - TryAdd를 추가하여 중복 추가시 에러가 발생하지 않도록 수정
  }

  /*lds (임시)
    Update - 에디터 및 디바이스에서 종료 함수 호출
    OnEnable - atlasRequested 콜백 추가
    OnDisable - atlasRequested 콜백 제거
    RequestAtlas - 콜벡함수. 지정한 사양에 맞는 아틀라스 스프라이트 로드
    OnApplicationQuit - 어플리케이션 종료 시 지정하는 사양 변경 ( 임시 테스트 용 )
   */
  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.Escape))
    {
      // save any game data here
#if UNITY_EDITOR
      // Application.Quit() does not work in the editor so
      // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
      //UnityEditor.EditorApplication.ExitPlaymode();
#else
      //Application.Quit();
#endif
    }
  }

  private void OnEnable()
  {
    Application.lowMemory += OnLowMemory;
    SpriteAtlasManager.atlasRequested += RequestAtlas;
  }

  private void OnDisable()
  {
    Application.lowMemory -= OnLowMemory;
    SpriteAtlasManager.atlasRequested -= RequestAtlas;
  }
  private void OnLowMemory()
  {
    Resources.UnloadUnusedAssets();
  }

  void RequestAtlas(string tag, System.Action<SpriteAtlas> action)
  {
    //SpriteAtlas sa = null;


    //Debug.Log(SystemInfo.operatingSystem);
    //Debug.Log(SystemInfo.graphicsDeviceVersion);
    //Debug.Log(SystemInfo.graphicsDeviceType);
    //Debug.Log(SystemInfo.maxTextureSize);
    //if (SystemInfo.graphicsDeviceVersion.Contains("ES 3.1") || SystemInfo.graphicsDeviceVersion.Contains("ES 3.2"))
    chunkSpriteAtlas = null;
    // string location = "ChunkSpriteAtlasMaster4096.spriteatlas";
    string location = string.Empty;
#if UNITY_EDITOR
    location = GetSpriteAtlasPath("ChunkSpriteAtlasMaster.spriteatlas");
#else
    // 8192 지원 사양인 경우
    if (SystemInfo.maxTextureSize >= 8192)
    {
      location = GetSpriteAtlasPath("ChunkSpriteAtlasMaster.spriteatlas");
    }
    // 8192 미지원 사양인 경우
    else
    {
      location = GetSpriteAtlasPath("ChunkSpriteAtlasMaster4096.spriteatlas");
    }
#endif
    /** lds-24.09.27, 저사양 텍스처 테스트용 백업 코드
    var atlasorder = PlayerPrefs.GetInt("MyAtlas!@#");
    // 8192 지원 사양인 경우
    if (SystemInfo.maxTextureSize >= 8192)
    {
      if (PlayerPrefs.HasKey("MyAtlas!@#") == true)
      {
        // 최고 사양을 설정한 상태라면
        if (atlasorder == 0)
          location = GetSpriteAtlasPath("ChunkSpriteAtlasMaster.spriteatlas");
        // 최저 사양을 설정한 상태라면
        else if (atlasorder == 1)
          location = GetSpriteAtlasPath("ChunkSpriteAtlasVariant.spriteatlas");
      }
      // 설정한 사양이 없다면 기본 마스터 스프라이트 아틀라스 사용
      else
        location = GetSpriteAtlasPath("ChunkSpriteAtlasMaster.spriteatlas");
    }
    // 8192 미지원 사양인 경우
    else
    {
      if (PlayerPrefs.HasKey("MyAtlas!@#") == true)
      {
        // 최고 사양을 설정한 상태라면
        if (atlasorder == 0)
          location = GetSpriteAtlasPath("ChunkSpriteAtlasMaster4096.spriteatlas");
        // 최저 사양을 설정한 상태라면
        else if (atlasorder == 1)
          location = GetSpriteAtlasPath("ChunkSpriteAtlasVariant.spriteatlas");
      }
      // 설정한 사양이 없다면 기본 마스터 4096 스프라이트 아틀라스 사용
      else
        location = GetSpriteAtlasPath("ChunkSpriteAtlasMaster4096.spriteatlas");
    }
    */

    // RemoteLocatorMap null인 경우가 에디터 내에서 발생함.
    if (string.IsNullOrEmpty(location))
    {
      Debug.LogWarning($"RemoteLocatorMap 또는 LocalLocatorMap이 null 이거나 어드레서블에 {location}이 존재하지 않습니다.");
      return;
    }

    chunkSpriteAtlas = LoadAssetAtAddressables<SpriteAtlas>(location);
    Debug.Log($"order : {chunkSpriteAtlas.name}");
    if (chunkSpriteAtlas == null)
    {
      Debug.LogWarning($"리소스 {location}을 로드하는데 실패했습니다.");
      return;
    }
    action(chunkSpriteAtlas);

    string GetSpriteAtlasPath(string atlasName)
    {
      var path = $"SpriteAtlas/{atlasName}";
      var newAssetPath = $"{NEW_BUNDLE_ROOT}{path}";
      var locatorMap = RemoteLocatorMap;
      if(locatorMap is not null)
      {
        if (locatorMap.ContainsKey(newAssetPath) == true)
          return newAssetPath;
      }

      locatorMap = LocalLocatorMap;
      if (locatorMap is not null)
      {
        var localAssetPath = $"{LOCAL_BUNDLE_ROOT}{path}";
        if (locatorMap.ContainsKey(localAssetPath) == true)
          return localAssetPath;
      }
      return string.Empty;
    }
  }

  // 임시코드.
  protected override void OnApplicationQuit()
  {
    var myatlas = PlayerPrefs.HasKey("MyAtlas!@#") ? PlayerPrefs.GetInt("MyAtlas!@#") : 0;
    myatlas++;
    if (myatlas >= 2) myatlas = 0;
    PlayerPrefs.SetInt("MyAtlas!@#", myatlas);
    base.OnApplicationQuit();
  }

  private float refPixelsPerUnit = 0.5f;  // pixelsPerUnit canvas scaler refference , 50 => 0.5f, 100 => 1f
  private GameObject goCreatedOriginAtRuntime;
  private GameObject GoCreatedOriginAtRuntime
  {
    get
    {
      if (goCreatedOriginAtRuntime == null)
      {
        goCreatedOriginAtRuntime = new GameObject(nameof(GoCreatedOriginAtRuntime));
        goCreatedOriginAtRuntime.SetParent(gameObject);
      }
      return goCreatedOriginAtRuntime;
    }
  }
}