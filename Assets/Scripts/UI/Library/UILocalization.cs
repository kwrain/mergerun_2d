using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Localize;

public class UILocalization : MonoBehaviour
{
  private readonly Regex s_Regex = new Regex(@"<color=(.+?)>", RegexOptions.Singleline);

  private bool dirtyFlag = false;

  [SerializeField]
  private Text textComponent;
  [SerializeField]
  private TextMeshPro tmpPro;
  [SerializeField]
  private TextMeshProUGUI tmpProUGUI;

  [Header("[ CODE ]")]
  public string localizationID;

  [Header("[ COLOR TAG ]")]
  public Color[] colors;

  [Header("[ OPTION ]"), Tooltip("마지막 결과 값에 지정된 포맷형식을 적용함(valueArgs 보다 나중에 적용)")]
  public string textFormat = null;
  [Tooltip("로컬라이즈 결과 값에 포맷을 적용함(textFormat 보다 우선 적용)")]
  public string[] valueArgs = null;

  public ELetterType eLetterType = ELetterType.Normal;

  public string Text
  {
    get
    {
      if (textComponent != null) return textComponent.text;
      if (tmpPro != null) return tmpPro.text;
      
      return tmpProUGUI != null ? tmpProUGUI.text : null;
    }
    set
    {
      if (textComponent != null) textComponent.text = value;
      if (tmpPro != null) tmpPro.text = value;
      if (tmpProUGUI != null) tmpProUGUI.text = value;
    }
  }

  private bool bInitCompleted = false;
  // Use this for initialization

  private void SetDirty()
  {
    dirtyFlag = true;
  }

  // Update is called once per frame
  private void Init()
  {
    textComponent = GetComponent<Text>();
    tmpPro = GetComponent<TextMeshPro>();
    tmpProUGUI = GetComponent<TextMeshProUGUI>();
    bInitCompleted = true;
  }

  void UpdateUI()
  {
    if (!bInitCompleted)
      Init();

    if (textComponent == null && tmpPro == null && tmpProUGUI == null)
    {
      bInitCompleted = false;
      Debug.LogError("Text Component를 찾지 못 했습니다.", gameObject);
      return;
    }

    if (string.IsNullOrEmpty(localizationID))
      return;

    var value = valueArgs is {Length: > 0} ? GetValueFormat(localizationID, valueArgs) : GetValue(localizationID);
    #if UNITY_EDITOR && LOCALIZATION_CHECK
    if(Application.isPlaying == false) value = value.Replace(LOCALIZATION_CHECK_STRING, string.Empty);
    #endif
    Text = string.IsNullOrEmpty(textFormat) ? value : string.Format(textFormat, value);

    switch(eLetterType)
    {
      case ELetterType.Lower:
        Text = Text.ToLower();
        break;

      case ELetterType.Upper:
        Text = Text.ToUpper();
        break;
    }

    UpdateColor();

    //문제 가능성인 부분은 메시지 안에 문자열이나 변수들을 포함하는 메시지들은 어떻게 처리할것인가에대한 고민.
    //해당 변수들을 리스트로 받아와서 뿌려주는 방법으로 해결가능할까?
  }

  void UpdateColor()
  {
    var strColor = string.Empty;
    var matchCollection = s_Regex.Matches(Text);
    if (colors is {Length: > 0} && matchCollection.Count == colors.Length)
    {
      for (var i = 0; i < colors.Length; i++)
      {
        strColor = colors[i].ToHex();
        Text = Regex.Replace(Text, s_Regex.ToString(), $"<color=#{strColor}>");
      }
    }
    else
    {
      var ltColor = new List<Color>();
      foreach (Match match in matchCollection)
      {
        #region CHECK_COLOR
        strColor = match.Groups[1].Value;
        strColor = strColor.Replace("'", string.Empty);
        strColor = strColor.ToUpper();
        switch (strColor)
        {
          case "BLACK":
            ltColor.Add(Color.black);
            break;

          case "BLUE":
            ltColor.Add(Color.blue);
            break;

          case "CLEAR":
            ltColor.Add(Color.clear);
            break;

          case "CYAN":
            ltColor.Add(Color.cyan);
            break;

          case "GRAY":
            ltColor.Add(Color.gray);
            break;

          case "GREEN":
            ltColor.Add(Color.green);
            break;

          case "GREY":
            ltColor.Add(Color.grey);
            break;

          case "MAGENTA":
            ltColor.Add(Color.magenta);
            break;

          case "RED":
            ltColor.Add(Color.red);
            break;

          case "WHITE":
            ltColor.Add(Color.white);
            break;

          case "YELLOW":
            ltColor.Add(Color.yellow);
            break;

          default:
            ltColor.Add(strColor.ToColorFromHex());
            break;
        }
        #endregion
      }
      colors = ltColor.ToArray();
    }
  }

  [ContextMenu("ApplyLocalizing")]
  public void Start()
  {
    SetDirty();
  }

  private void OnEnable()
  {
    UpdateUI();
    if(Application.isPlaying == true)
      OnChangedLanguageCode += UpdateUI;
  }
  private void OnDisable()
  {
    SetDirty();
    OnChangedLanguageCode -= UpdateUI;
  }

  public void Update()
  {
    if (!dirtyFlag) return;
    UpdateUI();
    dirtyFlag = false;
  }

#if UNITY_EDITOR
  void OnValidate()
  {
    if (!Application.isPlaying)
      UpdateUI();
    else
      SetDirty();
  }
#endif
}