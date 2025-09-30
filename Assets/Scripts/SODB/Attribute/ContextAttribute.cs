using System;
using UnityEngine;

/**
* ContextAttribute.cs
* 작성자 : doseon@fairstudios.kr
* 작성일 : 2022년 07월 17일 오전 12시 48분
*/

/// <summary>
/// Vm에서 PropertyClassData의 프로퍼티를 노출하기 위해서 ContextAttribute를 사용할 것.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ContextAttribute : Attribute
{
  
}