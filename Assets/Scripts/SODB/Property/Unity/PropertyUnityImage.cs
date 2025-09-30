using FAIRSTUDIOS.SODB.Core;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

public class PropertyUnityImage : PropertyBase<PropertyUnityImage.Element>
{
  [System.Serializable]
  public class Element
  {
    public PropertyUnityImage Property { get; set; }

    [SerializeField] private bool raycastTarget = true;
    [SerializeField] private bool maskable = false;

    [SerializeField] private Image.Type imageType = Image.Type.Simple;


    [SerializeField, ShowIf("IsSimple"), AllowNesting()]
    private bool useSpriteMesh = false;

    [SerializeField, ShowIf(EConditionOperator.Or, "IsSliced", "IsTiled"), AllowNesting()]
    private float pixelsPerUnitMultiplier = 1f;

    [SerializeField, ShowIf("IsFilled"), AllowNesting()]
    private Image.FillMethod fillMethod = Image.FillMethod.Horizontal;

    [SerializeField, ShowIf("IsFilled"), AllowNesting(), Range(0f,1f)]
    private float fillAmount = 0;
    [SerializeField, ShowIf(EConditionOperator.Or, "IsSimple", "IsFilled"), AllowNesting()]
    private bool preserveAspect = false;

    [SerializeField, ShowIf(EConditionOperator.Or, "IsHorizontal"), AllowNesting()]
    private Image.OriginHorizontal originHorizontal = 0;
    [SerializeField, ShowIf(EConditionOperator.Or, "IsVertical"), AllowNesting()]
    private Image.OriginVertical originVertical = 0;

    [SerializeField, ShowIf(EConditionOperator.Or, "IsRadial90", "IsRadial180", "IsRadial360"), AllowNesting()]
    private bool fillClockwise = true;

    [SerializeField, ShowIf(EConditionOperator.Or, "IsRadial90"), AllowNesting()]
    private Image.Origin90 origin90 = 0;
    [SerializeField, ShowIf(EConditionOperator.Or, "IsRadial180"), AllowNesting()]
    private Image.Origin180 origin180 = 0;
    [SerializeField, ShowIf(EConditionOperator.Or, "IsRadial360"), AllowNesting()]
    private Image.Origin360 origin360 = 0;

    public bool RaycastTarget { get => raycastTarget; set { raycastTarget = value; Notify(); } }
    public bool Maskable { get => maskable; set { maskable = value; Notify(); } }

    public Image.Type ImageType { get => imageType; set { imageType = value; Notify(); } }
    public bool UseSpriteMesh { get => useSpriteMesh; set { useSpriteMesh = value; Notify(); } }

    public float PixelsPerUnitMultiplier { get => pixelsPerUnitMultiplier; set { pixelsPerUnitMultiplier = value; Notify(); } }

    public Image.FillMethod FillMethod { get => fillMethod; set { fillMethod = value; Notify(); } }
    public float FillAmount { get => fillAmount; set { fillAmount = value; Notify(); } }
    public bool PreserveAspect { get => preserveAspect; set { preserveAspect = value; Notify(); } }

    public Image.OriginHorizontal OriginHorizontal { get => originHorizontal; set { originHorizontal = value; Notify(); } }
    public Image.OriginVertical OriginVertical { get => originVertical; set { originVertical = value; Notify(); } }

    public bool FillClockwise { get => fillClockwise; set { fillClockwise = value; Notify(); } }
    public Image.Origin90 Origin90 { get => origin90; set { origin90 = value; Notify(); } }
    public Image.Origin180 Origin180 { get => origin180; set { origin180 = value; Notify(); } }
    public Image.Origin360 Origin360 { get => origin360; set { origin360 = value; Notify(); } }


    public bool IsSimple() => imageType == Image.Type.Simple;
    public bool IsSliced() => imageType == Image.Type.Sliced;
    public bool IsTiled() => imageType == Image.Type.Tiled;
    public bool IsFilled() => imageType == Image.Type.Filled;

    public bool IsHorizontal() => imageType == Image.Type.Filled && FillMethod == Image.FillMethod.Horizontal;
    public bool IsVertical() => imageType == Image.Type.Filled && FillMethod == Image.FillMethod.Vertical;
    public bool IsRadial90() => imageType == Image.Type.Filled && FillMethod == Image.FillMethod.Radial90;
    public bool IsRadial180() => imageType == Image.Type.Filled && FillMethod == Image.FillMethod.Radial180;
    public bool IsRadial360() => imageType == Image.Type.Filled && FillMethod == Image.FillMethod.Radial360;

    private void Notify() => Property.NotifyPropertyChanged();
  }

  public override void InitValue()
  {
    base.InitValue();
    RuntimeValue.Property = this;
  }

}