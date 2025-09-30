using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FAIRSTUDIOS.UI;
using FAIRSTUDIOS.Tools;
using UnityEngine.EventSystems;

public static class UIHelper
{
  #region Extension Method
  public static Canvas CreateCanvas(string name, GameObject goParent, Vector2Int resolution = default, bool useRaycaster = true)
  {
    var go = new GameObject(name);
    go.SetParent(goParent);

    var canvas = go.AddComponent<Canvas>();
    var canvasScaler = go.AddComponent<CanvasScaler>();
    if (resolution != default)
    {
      canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      canvasScaler.referenceResolution = resolution;
    }

    if (useRaycaster)
    {
      var graphicRaycaster = go.AddComponent<GraphicRaycaster>();
    }

    return canvas;
  }

  public static GameObject CreateGameObject(string name, GameObject goParent)
  {
    var go = new GameObject(name);

    go.SetParent(goParent);
    go.AddComponent<RectTransform>();
    if (goParent.GetComponent<RectTransform>() != null)
    {
      var rectTransform = go.AddComponent<RectTransform>();
      rectTransform.anchorMin = new Vector2(0, 0);
      rectTransform.anchorMax = new Vector2(1, 1);
      rectTransform.offsetMin = new Vector2(0, 0);
      rectTransform.offsetMax = new Vector2(0, 0);
    }
    return go;
  }

  public static void SetPosition(this GameObject go, Vector3 position, bool bLocal = true)
  {
    if (bLocal)
      go.transform.localPosition = position;
    else
      go.transform.position = position;
  }
  public static void SetNearTargetPosition(this GameObject go, GameObject goTarget, float fScapcing = 5)
  {
    // 기본적으로 타겟 위에 뜬다.
    // 상단 공간이 부족하면 좌측 또는 우측에 띄워준다.
    if (null == goTarget)
      return;

    var rt = go.GetComponent<RectTransform>();
    var rtTarget = goTarget.GetComponent<RectTransform>();

    var absHalfSize = new Vector2(Mathf.Abs(rt.sizeDelta.x), Mathf.Abs(rt.sizeDelta.y)) * 0.5f;
    var absTargetHalfSize = new Vector2(Mathf.Abs(rtTarget.sizeDelta.x), Mathf.Abs(rtTarget.sizeDelta.y)) * 0.5f;

    var canvas = UIManager.Instance.GetCanvas();
    var rtCanvas = canvas.transform as RectTransform;

    var newPos = Vector3.zero;
    var pos = UIManager.Instance.UICamera.WorldToScreenPoint(rtTarget.position);
    pos = new Vector3(pos.x * (rtCanvas.sizeDelta.x / Screen.width), pos.y * (rtCanvas.sizeDelta.y / Screen.height));

    var fPosY = pos.y + absTargetHalfSize.y + fScapcing + absHalfSize.y;
    if (fPosY + absHalfSize.y <= rtCanvas.sizeDelta.y)
    {
      newPos = new Vector3(pos.x, fPosY, 0);
    }
    else
    {
      var fPosX = pos.x + absTargetHalfSize.x + fScapcing + absHalfSize.x;
      if (fPosX + absHalfSize.x >= rtCanvas.sizeDelta.x)
      {
        newPos = new Vector3(pos.x - absHalfSize.x - absTargetHalfSize.x - fScapcing, pos.y, 0);
      }
      else
      {
        newPos = new Vector3(fPosX, pos.y, 0);
      }
    }

    float x, y;
    if (rt.pivot.x == 0.5f)
    {
      x = 0;
    }
    else if (rt.pivot.x < 0.5f)
    {
      x = -1;
    }
    else
    {
      x = 1;
    }

    if (rt.pivot.y == 0.5f)
    {
      y = 0;
    }
    else if (rt.pivot.y < 0.5f)
    {
      y = -1;
    }
    else
    {
      y = 1;
    }

    newPos = new Vector3(newPos.x + (absHalfSize.x * x) - (rtCanvas.sizeDelta.x * rt.anchorMax.x), newPos.y + (absHalfSize.y * y) - (rtCanvas.sizeDelta.y * rt.anchorMax.y));
    rt.anchoredPosition = newPos;
  }

  public static void SetParentUICanvas(this FAIRSTUDIOS.UI.UIBehaviour uiBehaviour, UICanvasTypes canvasType = UICanvasTypes.Middle, Vector3? localPosition = null)
  {
    uiBehaviour.gameObject.SetParent(UIManager.Instance.GetCanvasObject(canvasType), localPosition);
  }
  public static void SetParentUICanvas(this GameObject go, UICanvasTypes canvasType = UICanvasTypes.Middle, Vector3? localPosition = null)
  {
    go.SetParent(UIManager.Instance.GetCanvasObject(canvasType), localPosition);
  }

  public static void SetOutlineColor(this Text text, Color color)
  {
    var circleOutline = text.GetComponent<CircleOutline>();
    if (null == circleOutline)
    {
      circleOutline = text.gameObject.AddComponent<CircleOutline>();
    }

    circleOutline.effectColor = color;
  }

  /// <summary>
  /// 텍스트 길이 만큼 UI 사이즈를 조정한다.
  /// </summary>
  /// <param name="text"></param>
  /// <returns></returns>
  public static void UpdateSizeFromTextSize(this Text text)
  {
    text.rectTransform.sizeDelta = new Vector2(text.preferredWidth, text.preferredHeight);
  }

  private static IEnumerator _AutoCountText(Text text, float current, float target, float duration)
  {
    float offset = 0;
    if (current < target)
    {
      offset = (target - current) / duration;
    }
    else
    {
      offset = (current - target) / duration;
    }
    if (current < target)
    {
      while ((int)current < (int)target)
      {
        current += offset * Time.deltaTime;
        text.text = ((int)current).ToString();
        yield return null;
      }
    }
    else if (current > target)
    {
      while ((int)current > (int)target)
      {
        current -= offset * Time.deltaTime;
        text.text = ((int)current).ToString();
        yield return null;
      }
    }

    current = target;
    text.text = ((int)current).ToString();
  }
  public static void AutoCountText(this Text text, float current, float target, float duration = 0.2f)
  {
    text.StopCoroutine("_AutoCountText");
    text.StartCoroutine(_AutoCountText(text, current, target, duration));
  }
  private static IEnumerator _AutoCountTextComma(Text text, float current, float target, float duration)
  {
    float offset = 0;
    if (current < target)
    {
      offset = (target - current) / duration;
    }
    else
    {
      offset = (current - target) / duration;
    }
    if (current < target)
    {
      while ((int)current < (int)target)
      {
        current += offset * Time.deltaTime;
        text.text = ((int)current).ToString();
        yield return null;
      }
    }
    else if (current > target)
    {
      while ((int)current > (int)target)
      {
        current -= offset * Time.deltaTime;
        text.text = ((int)current).ToString();
        yield return null;
      }
    }

    current = target;
    text.text = ((int)current).ToString();
  }
  public static void AutoCountTextComma(this Text text, float current, float target, float duration = 0.2f)
  {
    text.StopCoroutine("_AutoCountText");
    text.StartCoroutine(_AutoCountText(text, current, target, duration));
  }

  /// <summary>
  /// 텍스트의 길이(width)에 ui의 가로길이를 맞춘다.
  /// </summary>
  /// <param name="text"></param>
  /// <param name="text"></param>
  /// <returns></returns>
  public static void UpdateSizeFromTextWidth(this Text text)
  {
    var textGen = new TextGenerator();
    var generationSettings = text.GetGenerationSettings(text.rectTransform.rect.size);
    var width = textGen.GetPreferredWidth(text.text, generationSettings);

    var sizeDelta = text.rectTransform.sizeDelta;
    sizeDelta.x = width;
    text.rectTransform.sizeDelta = sizeDelta;
  }

  /// <summary>
  /// 텍스트의 높이(height)에 ui의 세로길이를 맞춘다.
  /// </summary>
  /// <param name="text"></param>
  /// <param name="text"></param>
  /// <returns></returns>
  public static void UpdateSizeFromTextHeight(this Text text)
  {
    var textGen = new TextGenerator();
    var generationSettings = text.GetGenerationSettings(text.rectTransform.rect.size);
    var height = textGen.GetPreferredHeight(text.text, generationSettings);

    var sizeDelta = text.rectTransform.sizeDelta;
    sizeDelta.y = height;
    text.rectTransform.sizeDelta = sizeDelta;
  }

  private static IEnumerator _AutoProgress(Image image, float current, float target, float duration, float delay, EaseType easeType)
  {
    if (delay > 0)
    {
      yield return new WaitForSeconds(delay);
    }

    float time = 0;
    while (time <= duration)
    {
      time += Time.deltaTime;
      image.fillAmount = EaseManager.LerpEaseType(current, target, time / duration, easeType);

      yield return null;
    }
  }
  public static void AutoProgress(this Image image, float current, float target, float duration, float delay = 0, EaseType easeType = EaseType.easeInQuint)
  {
    image.StopCoroutine("_AutoProgress");
    image.StartCoroutine(_AutoProgress(image, current, target, duration, delay, easeType));
  }

  public static void SetGrayScale(this GameObject gameObject, float value)
  {
    var graphic = gameObject.GetComponent<Graphic>();
    if (graphic == null)
      return;

    graphic.SetGrayScale(value);
  }
  public static void SetGrayScale(this Graphic graphic, float value)
  {
    var matGrayScale = ResourceManager.Instance.Load<Material>("Materials/SpriteGrayscale");
    if (matGrayScale.name != graphic.material.name)
    {
      var mt = new Material(matGrayScale);
      graphic.material = mt;
    }

    graphic.material.SetFloat("_GrayscaleAmount", value);
  }

  public static void SetGrayScaleChild(this GameObject gameObject, float value)
  {
    var graphic = gameObject.GetComponent<Graphic>();
    if (graphic == null)
      return;

    graphic.SetGrayScaleChild(value);
  }
  public static void SetGrayScaleChild(this Graphic graphic, float value)
  {
    var graphics = graphic.GetComponentsInChildren<Graphic>(true);
    for (var i = 0; i < graphics.Length; i++)
    {
      SetGrayScale(graphics[i], value);
    }
  }

  public static void SetGrayScaleTint(this GameObject gameObject, Color value)
  {
    var graphic = gameObject.GetComponent<Graphic>();
    if (graphic == null)
      return;

    graphic.SetGrayScaleTint(value);
  }
  public static void SetGrayScaleTint(this Graphic graphic, Color value)
  {
    var matGrayScale = ResourceManager.Instance.Load<Material>("Materials/SpriteGrayscale");
    if (matGrayScale.name != graphic.material.name)
    {
      var mt = new Material(matGrayScale);
      graphic.material = mt;
    }

    graphic.material.SetColor("_Color", value);
  }
  public static void SetGrayScaleTintChild(this GameObject gameObject, Color value)
  {
    var graphic = gameObject.GetComponent<Graphic>();
    if (graphic == null)
      return;

    graphic.SetGrayScaleTintChild(value);
  }
  public static void SetGrayScaleTintChild(this Graphic graphic, Color value)
  {
    var graphics = graphic.GetComponentsInChildren<Graphic>(true);
    for (var i = 0; i < graphics.Length; i++)
    {
      SetGrayScaleTint(graphics[i], value);
    }
  }

  #endregion

  /// <summary>
  /// 스크롤뷰에서 아이템 드래그 시 체크하는 함수
  /// </summary>
  /// <param name="rangeX"></param>
  /// <param name="rangeY"></param>
  /// <param name="touchPos"></param>
  /// <param name="trTarget"></param>
  /// <returns></returns>
  public static bool CheckPickItem(int rangeX, int rangeY, Vector3 touchPos, Transform trTarget)
  {
    var revisionPos = UIManager.Instance.UICamera.WorldToScreenPoint(trTarget.position);

    float width = UIManager.Width;
    float height = UIManager.Height;

    var isPickItem = false;
    if (touchPos.y < revisionPos.y - (rangeY * (Screen.height / height)) || touchPos.y > revisionPos.y + (rangeY * (Screen.height / height)))
    {
      if (touchPos.y < revisionPos.x - (rangeX * (Screen.width / width)) || touchPos.y > revisionPos.x + (rangeX * (Screen.width / width)))
      {
        isPickItem = true;
      }
    }

    return isPickItem;
  }

  /// <summary>
  /// 월드포지션에 해당하는 ui포지션을 얻어온다
  /// </summary>
  public static Vector3 WorldPositionToUIPosition(Vector3 position)
  {
    var mainCamera = Camera.main;
    var uiCamera = UIManager.Instance.UICamera;

    var screenPosition = mainCamera.WorldToScreenPoint(position);
    var MapCameraPos = mainCamera.transform.position;
    var w = (uiCamera.aspect * 2f * uiCamera.orthographicSize) / (mainCamera.aspect * 2f * mainCamera.orthographicSize);

    var newPos = uiCamera.ScreenToWorldPoint(screenPosition) - ((mainCamera.transform.position - MapCameraPos) * w);
    newPos.z = UIManager.Instance.GetCanvas().planeDistance;

    return newPos;
  }

  /// <summary>
  /// 터치 기준 UI 포지션
  /// </summary>
  public static Vector3 GetTouchMapPosToUIPos()
  {
    var newPos = UIManager.Instance.UICamera.ScreenToWorldPoint(TouchManager.LastTouchPosition);
    return newPos;
  }

  /// <summary>
  /// 터치 기준 UI 포지션 스크린 좌표
  /// </summary>
  public static Vector3 GetTouchMapPosToUIPosScreen()
  {
    var newPos = Camera.main.ScreenToWorldPoint(TouchManager.LastTouchPosition);
    newPos = UIManager.Instance.UICamera.WorldToScreenPoint(newPos);

    return newPos;
  }
}

