using FAIRSTUDIOS.SODB.Core;
using UnityEngine;

public class PropertyNormalizedFloat : PropertyBase<float>
{
  [SerializeField] private float ratio = 1f;

  [System.NonSerialized] private float sourceValue;
  public float SourceValue { get=> sourceValue; set=> sourceValue = value; }

  public float NormalizedSourceValue => sourceValue * ratio;
  public float NormalizedDefaultValue => defaultValue * ratio;
  public float NormalizedRuntimeValue => runtimeValue * ratio;
  //public override float RuntimeValue { get => base.runtimeValue * ratio ;  }
}