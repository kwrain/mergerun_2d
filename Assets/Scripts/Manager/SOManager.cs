using FAIRSTUDIOS.SODB.Core;
using UnityEngine;

public class SOManager : Singleton<SOManager>
{
  #region Sriptalbe Objects
  [SerializeField] private SoundDataCollection soundDataCollection;
  [SerializeField] private LocalizeTextDataCollection localizeTextAssetCollection;
  [SerializeField] private LocalizeSpriteAtlasDataCollection localizeSpriteAtlasCollection;

  public SoundDataCollection SoundDataCollection => soundDataCollection;
  public LocalizeTextDataCollection LocalizeTextAssetCollection => localizeTextAssetCollection;
  public LocalizeSpriteAtlasDataCollection LocalizeSpriteAtlasCollection => localizeSpriteAtlasCollection;
  #endregion Sriptalbe Objects

  #region Models
  [SerializeField, Space] private GameModel gameModel;
  [SerializeField] private DeviceInfoModel deviceInfoModel;
  [SerializeField] private PlayerPrefsModel playerPrefsModel;

  static SOManager()
  {
    // 부모 클래스(Singleton<T>)의 static protected 필드인 prefabPath를 설정합니다.
    // 여기에 SOManager 프리팹의 실제 Addressable/Resources 경로를 입력하세요.
    prefabPath = "BundleLocal/Prefabs/Manager/SOManager.prefab";
  }

  public GameModel GameModel
  {
    get
    {
      if (!gameModel.isInit)
        gameModel.Init();
      return gameModel;
    }
  }
  public PlayerPrefsModel PlayerPrefsModel
  {
    get
    {
      if (!playerPrefsModel.isInit)
        playerPrefsModel.Init();
      return playerPrefsModel;
    }
  }
  public DeviceInfoModel DeviceInfoModel
  {
    get
    {
      if (!deviceInfoModel.isInit)
        deviceInfoModel.Init();
      return deviceInfoModel;
    }
  }
  #endregion Models

  #region Game Data

  [SerializeField] private GameDataTable gameDataTable;

  public GameDataTable GameDataTable => gameDataTable;
  #endregion

  protected override void Awake()
  {
    base.Awake();

    // TODO : 라이브 시에 제거 예정.
    style.normal.textColor = Color.red;
    style.fontSize = 30;
  }

  private void Update()
  {
// #if UNITY_EDITOR
//     // 에디터 전용
//     // 디바이스 시뮬레이터에서는 키보드 입력 감지가 안되므로
//     // Control Panle - Utility - RefreshScreen으로 대체
//     if (Input.GetKeyDown(KeyCode.A))
//     {
//       deviceInfoModel.OnScreenInfoChanged();
//     }
// #endif
//     deviceInfoModel.OnScreenInfoChanged();
  }

  protected override void OnDestroy()
  {
    // TODO - base.OnDestory()보다 먼저 호출되어야 하는 경우
    // 이곳에서 글로벌 모델이 OnDsiableModel 호출이 필요한 경우 사용해주세요.

    Binding.Clear(); // lds - 23.6.22, 등록된 모든 SODB vm 이벤트 제거.
    base.OnDestroy();
  }

  // TODO : 라이브 시에 제거 예정.
  GUIStyle style = new();
  private void OnGUI()
  {
#if SHOW_QA_VERSION
    GUI.Label(new Rect(Screen.width * 0f, Screen.height * 1f - 40f, Screen.width * 0.208f, Screen.height * 0.24f), $"{versionInfo.QAVersion} {Application.platform}", style);
#endif
  }
}
