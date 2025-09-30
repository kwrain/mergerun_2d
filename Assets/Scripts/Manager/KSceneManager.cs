using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// 화면 정보 정의
/// - 각 화면별로 사용하는 값을 정의해 놓는다.
/// </summary>
public enum ESceneName
{
  /// <summary>
  /// 화면을 바로 실행한 경우
  /// </summary>
  None,

  Splash,
  Lobby,
  Game,

  Max
}

public class KSceneManager : Singleton<KSceneManager>
{
  public static event UnityAction<Scene> scenePreload;
  public static event UnityAction<ESceneName> sceneLoaded;

  public static Dictionary<ESceneName, bool> dtCheckFirstLoad = new Dictionary<ESceneName, bool>();

  public static Scene ActiveScene { get { return SceneManager.GetActiveScene(); } }
  public static BaseScene ActiveSceneBehaviour { get; private set; }

  public static ESceneName ActiveSceneName { get { return ActiveSceneBehaviour.eSceneName; } }

  protected override void SceneLoadedEvent(Scene scene, LoadSceneMode SceneMode)
  {
    base.SceneLoadedEvent(scene, SceneMode);

    ESceneName eSceneName = scene.name.ToEnum<ESceneName>();
    if(dtCheckFirstLoad.ContainsKey(eSceneName))
    {
      dtCheckFirstLoad[eSceneName] = false;
    }
    else
    {
      dtCheckFirstLoad[eSceneName] = true;
    }

    List<GameObject> gameObjects = new List<GameObject>();
    scene.GetRootGameObjects(gameObjects);

    GameObject goScene = gameObjects.Find(go => go.name == "Scene");
    if(goScene != null)
    {
      ActiveSceneBehaviour = goScene.GetComponent<BaseScene>();
      ActiveSceneBehaviour.Loaded(dtCheckFirstLoad[eSceneName]);
    }

    if (sceneLoaded != null)
    {
      sceneLoaded.Invoke(eSceneName);
    }
  }

  public void LoadScene(ESceneName sceneName)
  {
    LoadScene(sceneName.ToString());
  }
  public void LoadSceneAddressables(ESceneName sceneName)
  {
    if (scenePreload != null)
    {
      scenePreload.Invoke(ActiveScene);
    }
    Addressables.LoadSceneAsync(sceneName.ToString());
  }
  public void LoadScene(string sceneName)
  {
    if (scenePreload != null)
    {
      scenePreload.Invoke(ActiveScene);
    }

    SceneManager.LoadScene(sceneName);
  }
  public void LoadScene(int sceneBuildIndex)
  {
    Scene scene = SceneManager.GetSceneByBuildIndex(sceneBuildIndex);
    if(scene != null)
    {
      LoadScene(scene.name);
    }
  }

  public AsyncOperation LoadSceneAsync(ESceneName sceneName)
  {
    return LoadSceneAsync(sceneName.ToString());
  }
  public AsyncOperation LoadSceneAsync(string sceneName)
  {
    if (scenePreload != null)
      scenePreload.Invoke(ActiveScene);

    return SceneManager.LoadSceneAsync(sceneName);
  }
  public AsyncOperation LoadSceneAsync(int sceneBuildIndex)
  {
    Scene scene = SceneManager.GetSceneByBuildIndex(sceneBuildIndex);
    if (scene != null)
    {
      return LoadSceneAsync(scene.name);
    }

    return null;
  }
}
