using System;
using UnityEngine;

/**
* SupportedPropertyAttribute.cs
* 작성자 : doseon@fairstudios.kr
* 작성일 : 2022년 07월 16일 오전 11시 55분
*/

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class SupportedPropertyAttribute : Attribute
{
  public Type[] SupportedPropertyTypes { get; private set; }
  public SupportedPropertyAttribute(params Type[] supportedPropertyTypes)
  {
    SupportedPropertyTypes = supportedPropertyTypes;
  }
}