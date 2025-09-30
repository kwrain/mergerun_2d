using System.Collections;
using FAIRSTUDIOS.SODB.Core;
using FAIRSTUDIOS.UI;
using UnityEngine;

[RequireComponent(typeof(KProgressBar))]
[BindProperty(typeof(PropertyNormalizedFloat))]
public class ViewModelKProgressBarValueSetter : ViewModelBase<KProgressBar>
{
  public bool useAnimation = false;
  public float animSpeed = 1f;
  private bool isPlaying = false;
  private float targetValue = 0f;
  private float currentValue = 0f;
  //[SerializeField] protected KProgressBar progressBar;
  private void Awake()
  {
    targets = targets != null ? targets : GetComponent<KProgressBar>();
  }

  protected override void OnDisable()
  {
    targets.SetProgress(targetValue);
    StopAllCoroutines();
    isPlaying = false;
    base.OnDisable();
  }

  public override void OnPropertyChanged(PropertyBase property)
  {
    if (targets == null) return;
    var newValue = property as PropertyNormalizedFloat;
    if(useAnimation == false)
      targets.SetProgress(newValue.NormalizedRuntimeValue);
    else
    {
      targetValue = newValue.NormalizedRuntimeValue;
      if(isPlaying == true) return;
      currentValue = newValue.NormalizedSourceValue;
      StartCoroutine(DoAnimation());
    }
  }

  IEnumerator DoAnimation()
  {
    isPlaying = true;
    while(currentValue < targetValue)
    {
      currentValue += Time.deltaTime * animSpeed;
      if(currentValue >= targetValue)
        break;
      targets.SetProgress(currentValue);
      yield return null;
    }
    targets.SetProgress(targetValue);
    isPlaying = false;
  }
}