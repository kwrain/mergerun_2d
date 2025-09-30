using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FAIRSTUDIOS.Tools
{
  [AddComponentMenu("K.Tools/Tween/Tween Alpha")]
  public class KTweenAlpha : KTweenValue
  {
    public bool includeChilds = false;
    public List<GameObject> ignoreChilds = new List<GameObject>();
    
    // 텍스트 컴포넌트가 포함된 경우
    // 마크업 포맷의 컬러값을 수정 할지 체크해야한다.

    [SerializeField, HideInInspector]
    private bool useCanvasGroup;

    public bool UseCanvasGroup
    {
      get
      {
        // lds - UseCanvasGroup 프로퍼티 set이 KTweenAlphaEditor에서만 발생하는 문제가 있음.
        // 보통은 트윈 플레이 전에 스크립트에서 UseCanvasGroup = true를 해주기 때문에 발생하지 않을 수 있지만
        // 그렇지 않은 경우가 있을 수 도 있기 때문에 대응
        if (useCanvasGroup == true)
        {
          if (null == m_CanvasGroup)
            m_CanvasGroup = GetComponent<CanvasGroup>();

          if (m_CanvasGroup == null)
            m_CanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        return useCanvasGroup; 
      }
      set
      {
        useCanvasGroup = value;
        if (useCanvasGroup == true)
        {
          if(null == m_CanvasGroup)
            m_CanvasGroup = GetComponent<CanvasGroup>();

          if (m_CanvasGroup == null)
            m_CanvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
      }
    }

    private CanvasGroup m_CanvasGroup;

    private Text mText;
    //private Light mLight;
    private Image mImage;
    //private RawImage mRawImage;
    private ModifiedShadow m_ModifiedShadow;

    //private SpriteRenderer mSpriteRender;
    //private Renderer mRend;
    //private Material mMat;

    float mAlpha = 0f;

    public float alpha
    {
      get => mAlpha;
      set
      {
        SetAlpha(transform, value);
        mAlpha = value;
      }
    }

    protected override void ValueUpdate(float value, bool isFinished)
    {
      alpha = value;
    }

    protected virtual void SetAlpha(Transform _transform, float _alpha)
    {
      if (UseCanvasGroup == true)
      {
        m_CanvasGroup.alpha = _alpha;
        return;
      }

      Color c = Color.white;
      mText = _transform.GetComponent<Text>();
      if (null != mText)
      {
        c = mText.color;
        c.a = _alpha;
        mText.color = c;
        
        // 텍스트에 포함된 마크업 포의 컬러값 수정 여부 체크
        mText.text = CommonHelper.AlphaTagChangeInText(mText.text, _alpha);
      }

      mImage = _transform.GetComponent<Image>();
      if (null != mImage)
      {
        c = mImage.color;
        c.a = _alpha;
        mImage.color = c;
      }

      //mRawImage = _transform.GetComponent<RawImage>();
      //if (null != mRawImage)
      //{
      //  c = mRawImage.color;
      //  c.a = _alpha;
      //  mRawImage.color = c;
      //}

      m_ModifiedShadow = _transform.GetComponent<ModifiedShadow>();
      if(null != m_ModifiedShadow)
      {
        c = m_ModifiedShadow.effectColor;
        c.a = _alpha;
        m_ModifiedShadow.effectColor = c;
      }

      //mSpriteRender = _transform.GetComponent<SpriteRenderer>();
      //if (mSpriteRender != null)
      //{
      //  c = mSpriteRender.color;
      //  c.a = _alpha;
      //  mSpriteRender.color = c;
      //}
      //else
      //{
      //  mRend = _transform.GetComponent<Renderer>();
      //  if (null != mRend)
      //  {
      //    mMat = mRend.material;
      //    if (null != mMat)
      //    {
      //      c = mMat.color;
      //      c.a = _alpha;
      //      mMat.color = c;
      //    }
      //  }
      //}

      //mLight = _transform.GetComponent<Light>();
      //if (null != mLight)
      //{
      //  c = mLight.color;
      //  c.a = _alpha;
      //  mLight.color = c;
      //}

      if (includeChilds)
      {
        for (int i = 0; i < _transform.childCount; ++i)
        {
          Transform child = _transform.GetChild(i);
          if (!ignoreChilds.Contains(child.gameObject))
            SetAlpha(child, _alpha);
        }
      }
    }

    public void AddIgnoreChild(GameObject go)
    {
      if (!ignoreChilds.Contains(go))
        ignoreChilds.Add(go);
    }

    public void RemoveIgnoreChild(GameObject go)
    {
      if (ignoreChilds.Contains(go))
        ignoreChilds.Remove(go);
    }

    public void Begin(float from, float to, float duration = 1f, float delay = 0f, EaseType easeType = EaseType.linear, LoopStyle loopStyle = LoopStyle.Once, UnityAction onFinished = null)
    {
      this.from = from;
      this.to = to;
      this.duration = duration;
      this.delay = delay;
      this.easeType = easeType;
      this.loopStyle = loopStyle;

      if (onFinished != null)
        this.onFinished.AddListener(onFinished);

      if (duration <= 0)
      {
        Sample(1, true);
        enabled = false;
      }
      else
      {
        ForceStart = true;
        enabled = true;
      }
    }

    public static KTweenAlpha Begin(GameObject go, float from, float to, float duration = 1f, float delay = 0f, EaseType easeType = EaseType.linear, LoopStyle loopStyle = LoopStyle.Once, UnityAction onFinished = null)
    {
      KTweenAlpha comp = InitializeTween<KTweenAlpha>(go);

      comp.Begin(from, to, duration, delay, easeType, loopStyle, onFinished);

      return comp;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="go"></param>
    /// <param name="includeChilds">자식 적용 여부</param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="duration"></param>
    /// <param name="delay"></param>
    /// <param name="easeType"></param>
    /// <param name="loopStyle"></param>
    /// <param name="onFinished"></param>
    /// <returns></returns>
    public static KTweenAlpha Begin(GameObject go, bool includeChilds ,float from, float to, float duration = 1f, float delay = 0f, EaseType easeType = EaseType.linear, LoopStyle loopStyle = LoopStyle.Once, UnityAction onFinished = null)
    {
      KTweenAlpha comp = InitializeTween<KTweenAlpha>(go);
      comp.includeChilds = includeChilds;

      comp.Begin(from, to, duration, delay, easeType, loopStyle, onFinished);

      return comp;
    }
  }
}