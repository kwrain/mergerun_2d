using FAIRSTUDIOS.UI;
using System;
using System.Collections;
using FAIRSTUDIOS.Tools;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Coffee.UISoftMask;

namespace FAIRSTUDIOS.UI
{
  [ExecuteInEditMode][RequireComponent(typeof(AtlasImage))]
  public class KProgressBar : MonoBehaviour
  {
    [Serializable]
    public class ProgressUpdateEvent : UnityEvent<float> { }

    public enum EProgressType
    {
      Size,
      Fill
    }

    private RectTransform _rectTransform;
    private Coroutine coAutoProgress;
    private Vector2 originalSize;
    private Vector2 sizeDelta;
    private float minSize;

    [SerializeField]
    private float amount;
    
    [SerializeField]
    private EProgressType progressType = EProgressType.Fill;

    [SerializeField]
    private AtlasImage imgBG;

    [SerializeField]
    public AtlasImage imgProgressMask;
    [SerializeField]
    public AtlasImage imgProgress;
    [SerializeField]
    public RectTransform rtProgress;

    [SerializeField]
    private AtlasImage imgAmountMask;
    [SerializeField]
    private AtlasImage imgAmount;
    [SerializeField]
    private RectTransform rtAmount;

    [SerializeField]
    private Text txtProgress;

    [SerializeField]
    private bool showPercent = false;

    [SerializeField]
    private bool amountView = false;

    [SerializeField]
    private bool decimalPointDisplay = false;

    // use only auto progress
    [SerializeField]
    private UnityEvent onStart;
    [SerializeField]
    private ProgressUpdateEvent onUpdate;
    [SerializeField]
    private UnityEvent onEnd;

    [SerializeField]
    private Vector2 _padding;

    public EProgressType ProgressType
    {
      get { return progressType; }
      set
      {
        float amount = Amount;

        progressType = value;
        switch (progressType)
        {
          case EProgressType.Size:
            imgProgress.fillAmount = imgAmount.fillAmount = 1;
            imgProgress.type = imgAmount.type = Image.Type.Sliced;
            break;

          default:
          case EProgressType.Fill:
            rtProgress.sizeDelta = originalSize;
            // lds - 기존
            //imgProgress.type = imgAmount.type = Image.Type.Filled;
            // lds - 현재 수정된 방식
            imgProgress.type = Image.Type.Filled;
            imgAmount.type = Image.Type.Sliced;
            imgProgress.fillMethod = Image.FillMethod.Horizontal;
            break;
        }

        SetProgress(Amount);
      }
    }

    public Vector2 padding
    {
      get { return _padding; }
      set
      {
        _padding = value;

        Vector2 vector = new Vector2(_padding.x, rtProgress.anchoredPosition.y);
        // lds - Fill 방식에 맞도록 수정.
        rtProgress.anchoredPosition /*= rtAmount.anchoredPosition*/ = vector;

        vector = originalSize;
        CheckOriginalSize();

        // 변경된 경우 UI 재설정
        if (vector != originalSize)
        {
          UpdateRectTransforms();
        }
        SetProgress(Amount);
      }
    }

    public RectTransform rectTransform
    {
      get
      {
        if (null == _rectTransform)
          _rectTransform = GetComponent<RectTransform>();

        return _rectTransform;
      }
    }

    public bool ShowPercent { get => showPercent; set => showPercent = value; }

    public bool AmountView
    {
      get { return amountView; }
      set
      {
        if (value)
        {
        }
        rtAmount.SetActive(amountView = value);
      }
    }

    public bool DecimalPointDisplay { get => decimalPointDisplay; set => decimalPointDisplay = value; }

    public float Amount { get => amount;  set => amount = value; }

    private void Awake()
    {
      CheckOriginalSize();

      InitComponent();
      
      SetProgress(amount);
    }

    private void OnRectTransformDimensionsChange()
    {
      // 루트 사이즈가 조절되는 경우?
      // 게임 오브젝트 생성 시에도 호출된다.
      Vector2 prevSize = originalSize;
      CheckOriginalSize();


      // 변경된 경우 UI 재설정
      if(prevSize != originalSize)
      {
        UpdateRectTransforms();
      }
    }

    [ContextMenu("InitComponent")]
    public void InitComponent()
    {
      if (imgProgressMask == null || rtProgress == null || imgProgress == null)
      {
        if (imgProgressMask == null)
        {
          imgProgressMask = gameObject.FindChild<AtlasImage>("ProgressMask");
          if (imgProgressMask == null)
          {
            GameObject go = new GameObject("ProgressMask");
            go.SetParent(gameObject);

            imgProgressMask = go.AddComponent<AtlasImage>();

            SoftMask mask = go.AddComponent<SoftMask>();
            mask.showMaskGraphic = false;
            mask.downSamplingRate = SoftMask.DownSamplingRate.x1;
          }
        }

        imgProgress = imgProgressMask.FindChild<AtlasImage>("Progress");
        if (imgProgress == null)
        {
          GameObject go = new GameObject("Progress", typeof(AtlasImage));
          go.SetParent(imgProgressMask.rectTransform);

          imgProgress = go.GetComponent<AtlasImage>();
        }

        if (rtProgress == null)
        {
          rtProgress = imgProgress.rectTransform;
        }
      }

      if (imgAmountMask == null || rtAmount == null || imgAmount == null)
      {
        if (imgAmountMask == null)
        {
          imgAmountMask = gameObject.FindChild<AtlasImage>("AmountMask");
          if (imgAmountMask == null)
          {
            GameObject go = new GameObject("AmountMask");
            go.SetParent(gameObject);

            imgAmountMask = go.AddComponent<AtlasImage>();

            SoftMask mask = go.AddComponent<SoftMask>();
            mask.showMaskGraphic = false;
            mask.downSamplingRate = SoftMask.DownSamplingRate.x1;
          }
        }

        imgAmount = imgAmountMask.FindChild<AtlasImage>("Amount");
        if (imgAmount == null)
        {
          GameObject go = new GameObject("Amount", typeof(AtlasImage));
          go.SetParent(imgAmountMask.rectTransform);

          imgAmount = go.GetComponent<AtlasImage>();
        }

        if (rtAmount == null)
        {
          rtAmount = imgAmount.rectTransform;
        }

        AmountView = amountView;
      }

      if (txtProgress == null)
      {
        txtProgress = gameObject.FindChild<Text>("Text");
        if (txtProgress == null)
        {
          GameObject go = new GameObject("Text", typeof(Text));
          go.SetParent(gameObject);

          txtProgress = go.GetComponent<Text>();
        }
      }

      Vector2 vector = new Vector2(0, rtProgress.anchorMin.y);
      rtProgress.anchorMin = rtProgress.anchorMax = rtProgress.pivot = vector;
      // lds - Fill 방식에 맞도록 수정.
      //rtAmount.anchorMin = rtAmount.anchorMax = rtAmount.pivot = vector;

      vector = new Vector2(0, imgProgress.rectTransform.anchorMin.y);
      imgProgress.rectTransform.anchorMin = imgProgress.rectTransform.anchorMax = imgProgress.rectTransform.pivot = vector;
      imgProgress.rectTransform.anchoredPosition = Vector2.zero;

      // lds - Fill 방식에 맞도록 수정.
      //imgAmount.rectTransform.anchorMin = imgAmount.rectTransform.anchorMax = imgAmount.rectTransform.pivot = vector;
      //imgAmount.rectTransform.anchoredPosition = Vector2.zero;

      ProgressType = progressType;

#if UNITY_EDITOR
      CheckOriginalSize();
      UpdateRectTransforms();
#endif
    }

    public void CheckOriginalSize()
    {
      originalSize.x = (rectTransform.sizeDelta.x - (padding.x + padding.y));

      if(rtProgress != null)
      {
        sizeDelta.y = originalSize.y = rtProgress.sizeDelta.y;
        if(imgProgress.type == Image.Type.Sliced)
        {
          Sprite sprite = imgProgress.sprite;
          if (sprite != null)
          {
            minSize = (int)(sprite.border.x + sprite.border.z);
          }
        }
        else
        {
          minSize = 0;
        }
      }
    }

    public void UpdateRectTransforms()
    {
      rtProgress.sizeDelta /*= rtAmount.sizeDelta*/ = originalSize;
      imgProgress.rectTransform.sizeDelta /*= imgAmount.rectTransform.sizeDelta*/ = originalSize;
    }

    private IEnumerator _AutoProgress(float current, float amount, float duration, float delay, EaseType easeType)
    {
      if (delay > 0)
      {
        yield return new WaitForSeconds(delay);
      }

      if(onStart != null)
      {
        onStart.Invoke();
      }

      float time = 0;
      float value = 0;
      while (time <= duration)
      {
        time += Time.deltaTime;
        value = EaseManager.LerpEaseType(current, amount, time / duration, easeType);
        SetProgress(value);
        
        if(onUpdate != null)
        {
          onUpdate.Invoke(value);
        }

        yield return null;
      }

      if(onEnd != null)
      {
        onEnd.Invoke();
      }

      onStart.RemoveAllListeners();
      onUpdate.RemoveAllListeners();
      onEnd.RemoveAllListeners();
    }

    //public void AutoProgress(float current, float amount, float duration, float delay = 0, EaseType easeType = EaseType.easeInQuint)
    public void AutoProgress(float current, float amount, float duration, float delay = 0, EaseType easeType = EaseType.linear)
    {
      current = Mathf.Clamp(current, 0, 1);
      amount = Mathf.Clamp(amount, 0, 1);

      if (AmountView)
      {
        switch (progressType)
        {
          case EProgressType.Size:
            sizeDelta.x = originalSize.x * amount;
            //rtAmount.sizeDelta = sizeDelta;
            break;

          default:
          case EProgressType.Fill:
            imgAmount.fillAmount = amount;
            imgAmountMask.fillAmount = amount;
            break;
        }
      }

      StopAllCoroutines();
      coAutoProgress = StartCoroutine(_AutoProgress(current, amount, duration, delay, easeType));
    }

    public void SetProgress(float amount)
    {
      if (imgProgress == null || imgAmount == null)
        return;

      switch (progressType)
      {
        case EProgressType.Size:
          sizeDelta.x = originalSize.x * amount;
          if (amount > 0 && sizeDelta.x < minSize)
          {
            rtProgress.anchoredPosition = new Vector2(-minSize + sizeDelta.x, 0);
            sizeDelta.x = minSize;
          }
          else
          {
            var pos = Vector2.zero;
            pos.x += padding.x;
            rtProgress.anchoredPosition = pos;
          }
          rtProgress.sizeDelta = sizeDelta;
          if (AmountView)
          {
            // lds - Fill 방식에 맞도록 수정.
            //rtAmount.sizeDelta = sizeDelta;
          }
          break;

        default:
        case EProgressType.Fill:
          imgProgress.fillAmount = amount;
          imgAmountMask.fillAmount = amount;
          if (AmountView)
          {
            imgProgress.fillAmount = amount;
          }
          break;
      }

      Amount = amount;

      if (ShowPercent)
      {
        amount *= 100f;
        if (!DecimalPointDisplay)
        {
          amount = (int)amount;
        }

        txtProgress.text = string.Format("{0}%", amount);
      }
    }

    public void SetText(string text)
    {
      txtProgress.text = text;
    }

    public void AddStartEvent(UnityAction unityAction)
    {
      onStart.AddListener(unityAction);
    }
    public void AddUpdateEvent(UnityAction<float> unityAction)
    {
      onUpdate.AddListener(unityAction);
    }
    public void AddEndEvent(UnityAction unityAction)
    {
      onEnd.AddListener(unityAction);
    }
  }
}