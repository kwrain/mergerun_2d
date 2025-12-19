using FAIRSTUDIOS.Tools;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Threading.Tasks;

public class SplashScene : BaseScene
{
  [SerializeField] private Reporter logReporter;

  [SerializeField] private Image imageLogo;
  [SerializeField] private KTweenAlpha tweenAlpha;

  protected override async void Start()
  {
    base.Start();

#if LOG && !UNITY_EDITOR
    logReporter.SetActive(true);
#else
    Destroy(logReporter.gameObject);
#endif

    imageLogo.color = new Color(1f, 1f, 1f, 0f);

    // tweenAlpha 연출과 Unity Services 초기화를 병렬로 시작
    await WaitForSplashAndServices();

    // 두 작업이 모두 완료되면 다음 씬으로 전환
    KSceneManager.Instance.LoadScene(ESceneName.Game);
  }

  override protected void OnDisable()
  {
    base.OnDisable();

    imageLogo.color = new Color(1f, 1f, 1f, 0f);
  }

  /// <summary>
  /// tweenAlpha 연출과 Unity Services 초기화가 모두 완료될 때까지 대기
  /// </summary>
  private async Task WaitForSplashAndServices()
  {
    // tweenAlpha 완료를 기다리는 Task 생성
    Task tweenTask = WaitForTweenAlpha();

    // Unity Services 초기화 시작 (GameManager가 생성되어 있는지 확인)
    Task servicesTask = Task.CompletedTask;
    if (GameManager.IsCreated)
    {
      servicesTask = GameManager.Instance.InitializeUnityServices();
    }
    else
    {
      Debug.LogWarning("[SplashScene] GameManager가 아직 생성되지 않았습니다.");
    }

    // 두 작업이 모두 완료될 때까지 대기
    await Task.WhenAll(tweenTask, servicesTask);
    Debug.Log("[SplashScene] 스플래시 연출과 Unity Services 초기화 완료");

    // 데이터 수집 동의 요청 (iOS ATT 또는 안드로이드 동의 팝업)
    if (GameManager.IsCreated)
    {
      await GameManager.Instance.RequestDataCollectionConsent();
      Debug.Log("[SplashScene] 데이터 수집 동의 처리 완료");
    }
  }

  /// <summary>
  /// tweenAlpha 연출이 완료될 때까지 대기하는 Task
  /// </summary>
  private Task WaitForTweenAlpha()
  {
    var tcs = new TaskCompletionSource<bool>();
    
    // 기존 이벤트 제거 후 새로운 완료 콜백 등록
    tweenAlpha.AddFinishedEvent(new UnityAction(() =>
    {
      tcs.SetResult(true);
    }));
    
    // tweenAlpha 연출 시작
    tweenAlpha.enabled = true;
    
    return tcs.Task;
  }
}
