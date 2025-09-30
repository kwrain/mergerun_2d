using UnityEngine;

namespace FAIRSTUDIOS.Tools
{
  [RequireComponent(typeof(CanvasGroup))]
  public class KTweenCanvasGroupAlpha : KTweenValue
  {
    CanvasGroup canvasGroup;

    public float CanvasGroupAlpha
    {
      get { return canvasGroup.alpha; }
      set { canvasGroup.alpha = value; }
    }

    // Use this for initialization
    void Start()
    {
      canvasGroup = GetComponent<CanvasGroup>();
    }

    protected override void ValueUpdate(float value, bool isFinished)
    {
      CanvasGroupAlpha = value;
    }
  }
}