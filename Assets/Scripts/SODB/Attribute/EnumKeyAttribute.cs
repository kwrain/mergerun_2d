using System;
using UnityEngine;

/**
* EnumKeyAttributes.cs
* 작성자 : doseon@fairstudios.kr
* 작성일 : 2022년 07월 16일 오전 5시 30분
*/

/// <summary>
/// Vm에서 GenericDictionary<string, >을 사용할 경우 EnumKeyAttribute를 사용할 것.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class EnumKeyAttribute : Attribute
{
  public Type EnumKeyType { get; private set; }
  public EnumKeyAttribute(Type enumKeyType)
  {
    EnumKeyType = enumKeyType;
  }
}