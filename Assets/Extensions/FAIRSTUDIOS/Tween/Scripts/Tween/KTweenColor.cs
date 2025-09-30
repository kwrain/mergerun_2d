using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

namespace FAIRSTUDIOS.Tools
{
  [AddComponentMenu("K.Tools/Tween/Tween Color")]
  public class KTweenColor : KTweener
  {
    public Color from = Color.white;
    public Color to = Color.white;
    public bool includeChilds = false;
    public List<GameObject> ignoreChilds = new List<GameObject>();

    private Text mText;
    //private Light mLight;
    private Image mImage;
    //private RawImage mRawImage;
    private ModifiedShadow m_ModifiedShadow;

    //private SpriteRenderer mSpriteRender;
    //private Renderer mRend;
    //private Material mMat;

    Color mColor = Color.white;

    // HSV
    Shader m_hsvShader = null;
    Material m_hsvMaterial = null;

    public Color colorValue
    {
      get
      {
        return mColor;
      }
      set
      {
        SetColor(value);
        mColor = value;
      }
    }

    private void Awake()
    {
      if (null == m_hsvShader)
        m_hsvShader = Shader.Find("HongInt/HSV");

      if (null == m_hsvMaterial)
        m_hsvMaterial = new Material(Resources.Load<Material>("Materials/HSV"));
    }

    protected override void OnUpdate(float factor, bool isFinished)
    {
      colorValue = Color.Lerp(from, to, factor);
    }

    public void Begin(Color from, Color to, float duration = 1f, float delay = 0f)
    {
      this.from = from;
      this.to = to;
      this.duration = duration;
      this.delay = delay;

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

    public static KTweenColor Begin(GameObject go, Color from, Color to, float duration = 1f, float delay = 0f)
    {
      KTweenColor comp = KTweener.InitializeTween<KTweenColor>(go);
      comp.Begin(from, to, duration, delay);

      return comp;
    }

    void SetColor(Transform _transform, Color _color)
    {
      mText = _transform.GetComponent<Text>();
      if (null != mText)
      {
        mText.color = _color;
      }

      mImage = _transform.GetComponent<Image>();
      if (null != mImage)
      {
        mImage.color = _color;
      }

      //mRawImage = _transform.GetComponent<RawImage>();
      //if (null != mRawImage)
      //{
      //  mRawImage.color = _color;
      //}

      m_ModifiedShadow = _transform.GetComponent<ModifiedShadow>();
      if (null != m_ModifiedShadow)
      {
        m_ModifiedShadow.effectColor = _color;
      }

      //mSpriteRender = _transform.GetComponent<SpriteRenderer>();
      //if (mSpriteRender != null)
      //{
      //  mSpriteRender.color = _color;
      //}
      //else
      //{
      //  mRend = _transform.GetComponent<Renderer>();
      //  if (null != mRend)
      //  {
      //    if (null == mMat)
      //      mMat = mRend.material;

      //    if (null != mMat)
      //    {
      //      mMat.color = _color;
      //    }
      //  }
      //}

      //mLight = _transform.GetComponent<Light>();
      //if (null != mLight)
      //{
      //  mLight.color = _color;
      //}

      if (includeChilds)
      {
        for (int i = 0; i < _transform.childCount; ++i)
        {
          Transform child = _transform.GetChild(i);
          if (!ignoreChilds.Contains(child.gameObject))
            SetColor(child, _color);
        }
      }
    }
    public void SetColor(Color _color)
    {
      SetColor(transform, _color);
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
    public void ClearIgnoreChild()
    {
      ignoreChilds.Clear();
    }


    public void Begin(Color from, Color to, float duration = 1f, float delay = 0f, EaseType easeType = EaseType.linear, LoopStyle loopStyle = LoopStyle.Once, UnityAction onFinished = null)
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

    public static KTweenColor Begin(GameObject go, Color from, Color to, float duration = 1f, float delay = 0f, EaseType easeType = EaseType.linear, LoopStyle loopStyle = LoopStyle.Once, UnityAction onFinished = null)
    {
      KTweenColor comp = InitializeTween<KTweenColor>(go);

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
    public static KTweenColor Begin(GameObject go, bool includeChilds, Color from, Color to, float duration = 1f, float delay = 0f, EaseType easeType = EaseType.linear, LoopStyle loopStyle = LoopStyle.Once, UnityAction onFinished = null)
    {
      KTweenColor comp = InitializeTween<KTweenColor>(go);
      comp.includeChilds = includeChilds;

      comp.Begin(from, to, duration, delay, easeType, loopStyle, onFinished);

      return comp;
    }
  }
}
