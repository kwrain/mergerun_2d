using UnityEngine;

public class SplashScene : BaseScene
{
  [SerializeField] private UISplash uiSplash;
  [SerializeField] private Reporter logReporter;

  protected override void Start()
  {
    base.Start();
    uiSplash.SetActive(true);

#if LOG && !UNITY_EDITOR
    logReporter.SetActive(true);
#else
    Destroy(logReporter.gameObject);
#endif
  }
}
