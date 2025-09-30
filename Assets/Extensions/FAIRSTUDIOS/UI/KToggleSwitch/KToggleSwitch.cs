using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Toggle;

namespace FAIRSTUDIOS.UI
{
  [AddComponentMenu("K.UI/Toggle Switch")]
  public class KToggleSwitch : MonoBehaviour
  {
    private Graphic[] graphics;
    private Image imgAdditionalArea;

    private float t = 0.0f;
    private bool switching = false;

    public RectTransform toggle;

    [Header("[BG]")]
    public Color colorOnBG;
    public Color colorOffBG;
    public Image imgBG;

    [Header("[Handle]")]
    public Color colorOnHandle;
    public Color colorOffHandle;
    public Image imgHandle;

    [Header("[Position]")]
    public Vector3 onPos;
    public Vector3 offPos;

    [Header("[Icon]")]
    public CanvasGroup canvasOnIcon;
    public CanvasGroup canvasOffIcon;

    [Header("[Speed]")]
    public float speed;

    [HideInInspector]
    public bool bAdditionalArea = false; // 터치 영역 확대 여부

    [HideInInspector]
    public ToggleEvent onValueChanged;

    public bool isOn = true;

    private void Awake()
    {
      Image[] images = imgBG.GetComponentsInChildren<Image>(true);
      foreach (Image image in images)
      {
        if (image.name == "ImgAdditionalArea")
        {
          imgAdditionalArea = image;
          break;
        }
      }
      graphics = GetComponentsInChildren<Graphic>();
    }
    private void Start()
    {
      SetAdditionalTouchArea(bAdditionalArea);

      if (isOn)
      {
        imgBG.color = colorOnBG;
        imgHandle.color = colorOnHandle;
        imgHandle.rectTransform.anchoredPosition = onPos;
        canvasOnIcon.gameObject.SetActive(true);
        canvasOffIcon.gameObject.SetActive(false);
      }
      else
      {
        imgBG.color = colorOffBG;
        imgHandle.color = colorOffHandle;
        imgHandle.rectTransform.anchoredPosition = offPos;
        canvasOnIcon.gameObject.SetActive(false);
        canvasOffIcon.gameObject.SetActive(true);
      }
    }

    void Update()
    {
      if (switching)
      {
        Toggle();
      }
    }

    void StopSwitching()
    {
      if (t > 1.0f)
      {
        switching = false;

        t = 0.0f;

        isOn = !isOn;
        onValueChanged.Invoke(isOn);
      }
    }

    public void Switching()
    {
      switching = true;
    }

    private void Toggle()
    {
      if (!canvasOnIcon.gameObject.activeSelf || !canvasOffIcon.gameObject.activeSelf)
      {
        canvasOnIcon.gameObject.SetActive(true);
        canvasOffIcon.gameObject.SetActive(true);
      }

      if (isOn)
      {
        imgBG.color = SmoothColor(colorOnBG, colorOffBG);
        imgHandle.color = SmoothColor(colorOnHandle, colorOffHandle);
        imgHandle.rectTransform.anchoredPosition = SmoothMove(onPos.x, offPos.x);

        Transparency(canvasOnIcon, 1f, 0f);
        Transparency(canvasOffIcon, 0f, 1f);
      }
      else
      {
        imgBG.color = SmoothColor(colorOffBG, colorOnBG);
        imgHandle.color = SmoothColor(colorOffHandle, colorOnHandle);
        imgHandle.rectTransform.anchoredPosition = SmoothMove(offPos.x, onPos.x);

        Transparency(canvasOnIcon, 0f, 1f);
        Transparency(canvasOffIcon, 1f, 0f);
      }
    }
    public void SetStatus(bool toggleStatus)
    {
      isOn = toggleStatus;
      if (isOn)
      {
        imgBG.color = colorOnBG;
        imgHandle.color = colorOnHandle;
        imgHandle.rectTransform.anchoredPosition = onPos;
        canvasOnIcon.gameObject.SetActive(true);
        canvasOffIcon.gameObject.SetActive(false);
      }
      else
      {
        imgBG.color = colorOffBG;
        imgHandle.color = colorOffHandle;
        imgHandle.rectTransform.anchoredPosition = offPos;
        canvasOnIcon.gameObject.SetActive(false);
        canvasOffIcon.gameObject.SetActive(true);
      }
    }

    public void SetAdditionalTouchArea(bool bAdditionalArea)
    {
      if (bAdditionalArea)
      {
        if (graphics == null || graphics.Length == 0)
        {
          graphics = GetComponentsInChildren<Graphic>();
        }

        if (imgAdditionalArea == null)
        {
          Image[] images = imgBG.GetComponentsInChildren<Image>(true);
          foreach(Image image in images)
          {
            if(image.name == "ImgAdditionalArea")
            {
              imgAdditionalArea = image;
              break;
            }
          }

          if (imgAdditionalArea == null)
          {
            GameObject go = new GameObject("ImgAdditionalArea");
            go.SetParent(imgBG.gameObject);

            imgAdditionalArea = go.AddComponent<Image>();
            imgAdditionalArea.color = Color.clear;
            imgAdditionalArea.rectTransform.sizeDelta = graphics[0].rectTransform.sizeDelta * 1.5f;
            imgAdditionalArea.enabled = true;
          }
        }

        if (imgAdditionalArea != null && graphics.Length > 0)
        {
          Image image = graphics[0].GetComponent<Image>();
          if (null != image)
          {
            imgAdditionalArea.sprite = image.sprite;
          }
        }
        else
        {
          imgAdditionalArea.enabled = false;
        }
      }
      else if (imgAdditionalArea != null)
      {
        imgAdditionalArea.enabled = false;
      }
    }

    Vector3 SmoothMove(float startPosX, float endPosX)
    {
      Vector3 position = new Vector3(Mathf.Lerp(startPosX, endPosX, t += speed * Time.deltaTime), 0f, 0f);
      StopSwitching();
      return position;
    }

    Color SmoothColor(Color startCol, Color endCol)
    {
      Color resultCol;
      resultCol = Color.Lerp(startCol, endCol, t += speed * Time.deltaTime);
      return resultCol;
    }

    CanvasGroup Transparency(CanvasGroup alphaVal, float startAlpha, float endAlpha)
    {
      alphaVal.alpha = Mathf.Lerp(startAlpha, endAlpha, t += speed * Time.deltaTime);
      return alphaVal;
    }
  }
}