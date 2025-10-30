using System;
using System.Linq;
using System.Text;
// using FAIRSTUDIOS.Manager;
using UnityEngine;
using UnityEngine.Profiling;

public class BaseScene : MonoBehaviour
{
  [SerializeField, Header("Scene Name")]
  public ESceneName eSceneName = ESceneName.None;

  #region FPS 관련 변수
  private float deltaTime = 0.0f;
  private StringBuilder builder = new StringBuilder();
  private GUIStyle styleFrameGUI;
  #endregion

  public bool IsFirstLoad { get; private set; }

  protected virtual void Awake()
  {
    // lds - 24.11.20, 해당 조건을 모두 만족하는 경우 로그가 발생하지 않게함
    // 1. 디바이스에서 실행되고 (!UNITY_EDITOR)
    // 2. LOG 디파인을 사용하지 않으며 (!LOG)
    // 3. 라이브 서버여야 하고 (LIVE_SERVER)
    // 4. 풀빌드가 아닌 경우 (!__FULL_BUILD__)
    // => 실제 스토어에 배포될 빌드의 경우, 로그 출력을 막도록 처리 (LOG 디파인이 없어야함)
    // 반면 에디터이거나, LOG 디파인이 있거나, 라이브 서버가 아니거나, 풀빌드인 경우에는 로그 활성화
#if !UNITY_EDITOR && !LOG && LIVE_SERVER && !__FULL_BUILD__
    Debug.unityLogger.logEnabled = true;
    Debug.unityLogger.filterLogType = LogType.Error; // lds - 25.2.14, Error 및 Exception 로그 메시지만 발생
#else
    Debug.unityLogger.logEnabled = true;
#endif
    Debug.LogFormat("=== Awake.Begin - {0} ===", eSceneName);

    if (Application.isPlaying)
    {
      SOManager.CreateInstance("BundleLocal/Prefabs/Manager/SOManager.prefab");
      GameManager.CreateInstance();
      KSceneManager.CreateInstance();
      SoundManager.CreateInstance();
      TouchManager.CreateInstance();
    }
  }
  protected virtual void Start()
  {
    Debug.LogFormat(string.Format("=== Start - {0} ===", eSceneName));
  }

  protected virtual void OnEnable()
  {
    //WebManager.AddListenerWebResponse(this);
  }
  protected virtual void OnDisable()
  {
    //WebManager.RemoveListenerWebResponse(this);
  }

  protected virtual void Update()
  {
#if !UNITY_EDITOR && UNITY_ANDROID
    if(Input.deviceOrientation == DeviceOrientation.LandscapeLeft
    && UnityEngine.Device.Screen.orientation != ScreenOrientation.LandscapeLeft)
    {
      UnityEngine.Device.Screen.orientation = ScreenOrientation.LandscapeLeft;
    }
    else if(Input.deviceOrientation == DeviceOrientation.LandscapeRight
    && UnityEngine.Device.Screen.orientation != ScreenOrientation.LandscapeRight)
    {
      UnityEngine.Device.Screen.orientation = ScreenOrientation.LandscapeRight;
    }
#endif
  }

  public virtual void LoadScene(ESceneName eSceneName) { KSceneManager.Instance.LoadScene(eSceneName); }
  public virtual void LoadPrevScene() { }
  public virtual void LoadNextScene() { }

  public virtual void Loaded(bool isFirstLoad)
  {
    IsFirstLoad = isFirstLoad;
  }

  public virtual void OnGUI()
  {
#if DEV || EDIT
    FrameGUI();
    CheckMemory();
#endif
  }

  /// <summary>
  /// 현재 사용중인 텍스쳐 메모리 확인을 위한 함수
  /// </summary>
  void CheckMemory()
  {
    if (GUI.Button(new Rect(0, Screen.height * 0.25f, Screen.width * 0.15f, Screen.height * 0.05f), "Memory Check"))
    {
      var sortedAll = Resources.FindObjectsOfTypeAll(typeof(Texture2D)).OrderBy(go => Profiler.GetRuntimeMemorySizeLong(go)).ToList();

      StringBuilder sb = new StringBuilder("");
      int memTexture = 0;
      for (int i = sortedAll.Count - 1; i >= 0; i--)
      {
        if (!sortedAll[i].name.StartsWith("d_"))
        {
          memTexture += (int)Profiler.GetRuntimeMemorySizeLong(sortedAll[i]);
          sb.Append(typeof(Texture2D).ToString());

          sb.Append("Size#");
          sb.Append(sortedAll.Count - i);
          sb.Append(":");
          sb.Append(sortedAll[i].name);
          sb.Append("/InstanceID:");
          sb.Append(sortedAll[i].GetInstanceID());
          sb.Append("/Mem:");
          sb.Append(Profiler.GetRuntimeMemorySizeLong(sortedAll[i]).ToString());
          sb.Append("B/Total:");
          sb.Append(memTexture / 1024);
          sb.Append("KB");
          sb.Append("\n");

        }
      }

      Debug.Log("Texture2DInspect:" + sb.ToString());
    }
  }

  /// <summary>
  /// 프레임을 확인하기 위한 함수
  /// </summary>
  private void FrameGUI()
  {
    deltaTime += (Time.deltaTime - deltaTime) * 0.1f;

    int w = Screen.width;
    int h = Screen.height;

    //Rect rect = new Rect(0, 0, w, h * 2 / 100);
    Rect rect = new Rect(0, 0, w * 0.6f, h * 2 / 100);
    if (styleFrameGUI == null)
    {
      styleFrameGUI = new GUIStyle
      {
        alignment = TextAnchor.UpperRight,
        fontSize = h * 2 / 60
      };
      styleFrameGUI.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
    }

    float msec = Mathf.Round((deltaTime * 1000.0f) * 100) * 0.01f;
    float fps = Mathf.Round(1.0f / deltaTime);

    //float msec = deltaTime * 1000.0f;
    //float fps = 1.0f / deltaTime;

    //StringBuilder builder = new StringBuilder();
    builder.Append(msec);
    builder.Append(" ms (");
    builder.Append(fps);
    builder.Append(" fps)");

    //string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
    //GUI.Label(rect, text, style);
    GUI.Label(rect, builder.ToString(), styleFrameGUI);

    builder.Remove(0, builder.Length);
  }

  public virtual void OnApplicationQuit()
  {
    Debug.LogFormat("=== OnApplicationQuit - {0} ===", eSceneName);

    //DataManager.Instance.OnDestroyObject();
    // lds 23.3.28, 아래 매니저들의 호출 순서 이슈 때문에 임시 주석함.
    // ResourceManager.Instance.OnDestroyObject();
    // SoundManager.Instance.OnDestroyObject();
    // UIManager.Instance.OnDestroyObject();
    // GameManager.Instance.OnDestroyObject();
    // lds 23.3.28

    //TouchManager.Instance.OnDestroyObject();

    //WebManager.Instance.OnDestroyObject();
  }
}
