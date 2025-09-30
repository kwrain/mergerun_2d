using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace FAIRSTUDIOS.Tools
{
  [AddComponentMenu("K.Tools/Tween/Tween HSV")]
  public class KTweenHSV : KTweener
  {
    const string HUE = "_Hue";
    const string SATUTAION = "_Saturation";
    const string BRIGHTNESS = "_Brightness";
    const string ALPHA = "_AlphaShift";

    Dictionary<int, Material> dtRenderMaterial = new Dictionary<int, Material>();

    Material matHSV;
    Material matCloneHSV = null;

    Image image;
    RawImage rawImage;
    SpriteRenderer spriteRenderer;

    float hue;
    float saturation;
    float brightness;
    float alpha;

    [Range(0, 360)]
    public float fromHue = 0.0f;
    [Range(0, 360)]
    public float toHue = 0.0f;

    [Range(0, 2), Space(10)]
    public float fromSaturation = 1.0f;
    [Range(0, 2)]
    public float toSaturation = 1.0f;

    [Range(0, 2), Space(10)]
    public float fromBrightness = 1.0f;
    [Range(0, 2)]
    public float toBrightness = 1.0f;

    [Range(0, 1), Space(10)]
    public float fromAlpha = 1.0f;
    [Range(0, 1)]
    public float toAlpha = 1.0f;

    [Space(10)]
    public bool includeChilds = false;
    public List<GameObject> ignoreChilds = new List<GameObject>();

    public float Hue
    {
      get { return hue; }
      set
      {
        hue = value;
        SetHSV(hue, saturation, brightness, alpha);
      }
    }
    public float Saturation
    {
      get { return saturation; }
      set
      {
        saturation = value;
        SetHSV(hue, saturation, brightness, alpha);
      }
    }
    public float Brightness
    {
      get { return brightness; }
      set
      {
        brightness = value;
        SetHSV(hue, saturation, brightness, alpha);
      }
    }
    public float Alpha
    {
      get { return alpha; }
      set
      {
        alpha = value;
        SetHSV(hue, saturation, brightness, alpha);
      }
    }

    private void Awake()
    {
      matHSV = Resources.Load<Material>("Materials/HSV");
    }

    protected override void OnUpdate(float factor, bool isFinished)
    {
      hue = Mathf.Lerp(fromHue, toHue, factor);
      saturation = Mathf.Lerp(fromSaturation, toSaturation, factor);
      brightness = Mathf.Lerp(fromBrightness, toBrightness, factor);
      alpha = Mathf.Lerp(fromAlpha, toAlpha, factor);

      SetHSV(transform, hue, saturation, brightness, alpha);
    }

    void SetHSV(Transform _transform, float _Hue, float _Saturation, float _Brigtness, float _Alpha)
    {
      if (null == matHSV)
        return;

      spriteRenderer = _transform.GetComponent<SpriteRenderer>();
      if (null != spriteRenderer && spriteRenderer.IsActiveSelf())
      {
        if (null == matCloneHSV)
          matCloneHSV = new Material(matHSV);

        if (!dtRenderMaterial.ContainsKey(spriteRenderer.GetHashCode()))
        {
          dtRenderMaterial.Add(spriteRenderer.GetHashCode(), spriteRenderer.sharedMaterial);
          spriteRenderer.sharedMaterial = null;
        }

        if (null == spriteRenderer.sharedMaterial || spriteRenderer.sharedMaterial != matCloneHSV)
        {
          spriteRenderer.sharedMaterial = matCloneHSV;
        }

        matCloneHSV.SetFloat(HUE, _Hue);
        matCloneHSV.SetFloat(SATUTAION, _Saturation);
        matCloneHSV.SetFloat(BRIGHTNESS, _Brigtness);
        matCloneHSV.SetFloat(ALPHA, _Alpha);
      }

      image = _transform.GetComponent<Image>();
      if (null != image && image.IsActiveSelf())
      {
        if (null == matCloneHSV)
          matCloneHSV = new Material(matHSV);

        image.material = matCloneHSV;

        matCloneHSV.SetFloat(HUE, _Hue);
        matCloneHSV.SetFloat(SATUTAION, _Saturation);
        matCloneHSV.SetFloat(BRIGHTNESS, _Brigtness);
        matCloneHSV.SetFloat(ALPHA, _Alpha);
      }

      rawImage = _transform.GetComponent<RawImage>();
      if (null != rawImage && rawImage.IsActiveSelf())
      {
        if (null == matCloneHSV)
          matCloneHSV = new Material(matHSV);

        rawImage.material = matCloneHSV;

        matCloneHSV.SetFloat(HUE, _Hue);
        matCloneHSV.SetFloat(SATUTAION, _Saturation);
        matCloneHSV.SetFloat(BRIGHTNESS, _Brigtness);
        matCloneHSV.SetFloat(ALPHA, _Alpha);
      }

      if (includeChilds)
      {
        for (int i = 0; i < _transform.childCount; ++i)
        {
          Transform child = _transform.GetChild(i);
          if (!ignoreChilds.Contains(child.gameObject))
            SetHSV(child, _Hue, _Saturation, _Brigtness, _Alpha);
        }
      }
    }
    public void SetHSV(float _Hue, float _Saturation, float _Brigtness, float _Alpha)
    {
      SetHSV(transform, _Hue, _Saturation, _Brigtness, _Alpha);
    }

    void ResetMaterial(Transform _transform)
    {
      spriteRenderer = _transform.GetComponent<SpriteRenderer>();
      if (null != spriteRenderer && spriteRenderer.IsActiveSelf())
      {
        if (dtRenderMaterial.ContainsKey(spriteRenderer.GetHashCode()))
        {
          spriteRenderer.sharedMaterial = dtRenderMaterial[spriteRenderer.GetHashCode()];
        }
      }

      image = _transform.GetComponent<Image>();
      if (null != image && image.IsActiveSelf())
      {
        image.material = null;
      }

      rawImage = _transform.GetComponent<RawImage>();
      if (null != rawImage && rawImage.IsActiveSelf())
      {
        rawImage.material = null;
      }

      if (includeChilds)
      {
        for (int i = 0; i < _transform.childCount; ++i)
        {
          Transform child = _transform.GetChild(i);
          if (!ignoreChilds.Contains(child.gameObject))
            ResetMaterial(child);
        }
      }
    }
    public void ResetMaterial()
    {
      ResetMaterial(transform);
      dtRenderMaterial.Clear();
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

    public void Begin(Vector4 from, Vector4 to, float duration = 1f, float delay = 0f)
    {
      fromHue = from.x;
      toHue = to.x;

      fromSaturation = from.y;
      toSaturation = to.y;

      fromBrightness = from.z;
      toBrightness = to.z;

      fromAlpha = from.w;
      toAlpha = to.w;

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

    public static KTweenHSV Begin(GameObject go, Vector4 from, Vector4 to, float duration = 1f, float delay = 0f)
    {
      KTweenHSV comp = InitializeTween<KTweenHSV>(go);
      comp.Begin(from, to, duration, delay);

      return comp;
    }
  }
}
