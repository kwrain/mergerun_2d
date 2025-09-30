using UnityEngine;

namespace FAIRSTUDIOS.Tools
{
  [AddComponentMenu("K.Tools/Tween/Tween Shake")]
  public class KTweenShake : KTweener
  {
    Vector3 originPos;
    Quaternion originRot;

    RectTransform rectTransform;

    public float from;
    public float to;

    public bool shakePosition = true;
    public bool shakeRotation = false;

    private void Awake()
    {
      rectTransform = GetComponent<RectTransform>();
    }

    protected override void OnEnable()
    {
      base.OnEnable();

      if (shakePosition)
      {
        if (null == rectTransform)
        {
          originPos = transform.position;
        }
        else
        {
          originPos = rectTransform.anchoredPosition;
        }
      }

      if (shakeRotation)
      {
        originRot = transform.rotation;
      }
    }

    protected override void OnDisable()
    {
      base.OnDisable();

      if(shakePosition)
      {
        if (null == rectTransform)
        {
          transform.position = originPos;
        }
        else
        {
          rectTransform.anchoredPosition = originPos;
        }
      }

      if (shakeRotation)
      {
        transform.rotation = originRot;
      }
    }

    protected override void OnUpdate(float _factor, bool _isFinished)
    {
      if(shakePosition)
      {
        if (null == rectTransform)
        {
          transform.position = originPos + Random.insideUnitSphere * Mathf.Lerp(from, to, _factor);
        }
        else
        {
          rectTransform.anchoredPosition = originPos + Random.insideUnitSphere * Mathf.Lerp(from, to, _factor);
        }
      }

      if(shakeRotation)
      {
        transform.rotation = new Quaternion(originRot.x + Random.Range(from, to), originRot.y + Random.Range(from, to), 
          originRot.z + Random.Range(from, to), originRot.w + Random.Range(from, to) * Mathf.Lerp(from, to, _factor));
      }
    }
  }
}