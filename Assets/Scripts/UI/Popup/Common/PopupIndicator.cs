using System;
using System.Collections;
using FAIRSTUDIOS.SODB.Utils;
using FAIRSTUDIOS.UI;
using UnityEngine;
using UnityEngine.UI;

[PopupUI(Type = typeof(PopupIndicator), PrefabPath = "Common/PopupIndicator", CanvasType = UICanvasTypes.Focus, DimType = EUIDimType.BLACK, DontCloseOnInput = true, IsBlockMoveClose = true, DontCloseOnLoad = true)]
public class PopupIndicator : PopupUIBehaviour
{
  public Text textProgress;
  public Text txtDot;

  public float waitTime = 0.2f;

  private Action onDoneCallback;
  private readonly int IndicatorTriggerHash = Animator.StringToHash("IndicatorTrigger");

  private EIndicatorType currentType = EIndicatorType.Common;
  private bool loopIndicator;
  public GenericDictionary<EIndicatorType, Indicator> indicatorMap;

  public enum EIndicatorType
  {
    /// <summary>기본 인디케이터 타입, 애니메이터 없음</summary>
    Common = 0,
    /// <summary>제작 및 붙해 인디케이터 타입</summary>
    Production,
    /// <summary>스케치 북 인디케이터 타입</summary>
    SketchBook,
    /// <summary>선착장 비행선 항해중 인디케이터 타입, 애니메이터 없음</summary>
    AirshipDepart,
    /// <summary>일괄수확 인디케이터 타입, 애니메이터 없음</summary>
    HarvestBatch,
  }

  [System.Serializable]
  public class Indicator
  {
    public GameObject root;
    public Animator animator;
  }

  public void SetIndicator(EIndicatorType type, int textProgress, Action onCompleted = null, bool loopIndicator = false)
    => SetIndicator(type, textProgress.ToString(), onCompleted, loopIndicator);

  public void SetIndicator(EIndicatorType type, string textProgress, Action onCompleted = null, bool loopIndicator = false)
  {
    foreach(var item in indicatorMap)
      item.Value.root.SetActive(item.Key == type);

    currentType = type;
    var indicator = indicatorMap[currentType];
    if(indicator.animator != null)
      indicator.animator.SetTrigger(IndicatorTriggerHash);
    this.textProgress.text = Localize.GetValue(textProgress);
    this.onDoneCallback = onCompleted;
    this.loopIndicator = loopIndicator;
    StartCoroutine(LoopDot());
  }

  public override void Hide(Action complete = null, bool check = true)
  {
    base.Hide(complete, check);
    onDoneCallback?.Invoke();
    onDoneCallback = null;
    StopAllCoroutines();
  }

  IEnumerator LoopDot()
  {
    var indicator = indicatorMap[currentType];
    yield return new WaitForSeconds(waitTime);

    while (true)
    {
      if (txtDot.text.Length < 3)
      {
        txtDot.text += ".";
        yield return new WaitForSeconds(waitTime);
      }
      else
      {
        txtDot.text = string.Empty;
        yield return new WaitForSeconds(waitTime);
      }
      if(this.loopIndicator == false)
      {
        if(indicator.animator != null)
        {
          var stateInfo = indicator.animator.GetCurrentAnimatorStateInfo(0);
          if (stateInfo.normalizedTime >= 1f)
          {
            UIManager.Instance.HideLoading();
          }
        }
      }
    }
  }
}
