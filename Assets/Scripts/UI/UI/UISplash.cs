using FAIRSTUDIOS.Tools;
using FAIRSTUDIOS.UI;
using UnityEngine;

public class UISplash : MonoBehaviour
{
  public enum ChannelType
  {
    Dev = 0,
    Alpha,
    Local,
  }

  public AtlasImage imgLogo;
  public KTweenAlpha tweenAlpha;
  public UnityEngine.UI.Text txtPercent;

  private void Awake()
  {
    imgLogo.color = new Color(1f, 1f, 1f, 0f);
  }

  private void Start()
  {
    // Addressables.WebRequestOverride = EditWebRequestURL; // 원격에서 다운로드 요청 시 URL에 대한 수정을 진행하는 메서드 지정
    //StartCoroutine(LoadLocalResourceLocationAsync());
    ShowSplash();
  }

  private void ShowSplash()
  {
    tweenAlpha.enabled = true;
    tweenAlpha.from = 0f;
    tweenAlpha.to = 1f;
    tweenAlpha.duration = 2;
    tweenAlpha.stay = 0.2f;
    tweenAlpha.ClearFinishedEvent();
    //tweenAlpha.AddFinishedEvent(HideSplash);
    tweenAlpha.AddFinishedEvent(LoadNextScene);
    tweenAlpha.RePlay();
  }

  private void HideSplash()
  {
    tweenAlpha.enabled = true;
    tweenAlpha.from = 1f;
    tweenAlpha.to = 0f;
    tweenAlpha.duration = 2;
    tweenAlpha.stay = 0.2f;
    tweenAlpha.ClearFinishedEvent();
    tweenAlpha.AddFinishedEvent(LoadNextScene);
    tweenAlpha.RePlay();
  }

  private void LoadNextScene()
  {
    KSceneManager.Instance.LoadScene(ESceneName.Game);
  }
}

