using UnityEngine;

public class SplashScene : BaseScene
{
  [SerializeField] private Reporter logReporter;

  protected override void Start()
  {
    base.Start();

#if LOG && !UNITY_EDITOR
    logReporter.SetActive(true);
#else
    Destroy(logReporter.gameObject);
#endif
  }
}
