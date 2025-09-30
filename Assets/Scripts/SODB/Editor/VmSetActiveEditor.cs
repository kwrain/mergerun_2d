using System.Reflection;
using FAIRSTUDIOS.SODB.Property;
using UnityEditor;
using UnityEngine;

/**
* VmSetActiveEditor.cs
* 작성자 : doseon@fairsutdios.kr (LEE DO SEON)
* 작성일 : 2022년 11월 30일 오후 4시 44분
*/

[CustomEditor(typeof(VmSetActive), true)]
[CanEditMultipleObjects]
public class VmSetActiveEditor : VmEditor
{
  private PropertyInfoBase<VmSetActive.Param>[] m_pInfos;
  protected override void DrawCustom()
  {
    base.DrawCustom();
    var vmSetActive = serializedObject.targetObject as VmSetActive;
    if(m_pInfos == null)
    {
      var type = vmSetActive.GetType();
      var pInfosFiledInfo = type.GetField("pInfos", BindingFlags.NonPublic | BindingFlags.Instance);
      var value = pInfosFiledInfo.GetValue(vmSetActive);
      m_pInfos = value as PropertyInfoBase<VmSetActive.Param>[];
    }
    var result = string.Empty;
    if(m_pInfos != null)
    {
      for (int i = 0; i < m_pInfos.Length; i++)
      {
        var pInfo = m_pInfos[i];
        var property = pInfo.Property;
        if(property == null) continue;
        var param = pInfo.Param;
        if(param == null) continue;

#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
        string indexer = pInfo.ContextType switch
        {
          PropertyInfoContextType.Mono => string.Empty,
          PropertyInfoContextType.Index => $"[{pInfo.Index}]",
          PropertyInfoContextType.StringKey => $"[\"{pInfo.StringKey}\"]",
          PropertyInfoContextType.PropertyName => pInfo.NameContextType switch
          {
            PropertyNameContextType.Mono => string.Empty,
            PropertyNameContextType.Index => $"[{pInfo.Index}]",
            PropertyNameContextType.StringKey => $"[\"{pInfo.StringKey}\"]",
          },
        };
#pragma warning restore CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.

#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
        var conditionalLogic = param.ConditionalLogic switch
        {
          VmSetActive.ConditionalLogicType.AND => "&&",
          VmSetActive.ConditionalLogicType.OR => "||",
        };
#pragma warning restore CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.

#pragma warning disable CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.
        var comparison = param.Comparison switch
        {
          VmSetActive.ComparisonType.EqualTo => "==",
          VmSetActive.ComparisonType.NotEqual => "!=",
          VmSetActive.ComparisonType.GreaterThan => ">",
          VmSetActive.ComparisonType.LessThan => "<",
          VmSetActive.ComparisonType.GreaterThanOrEqualTo => ">=",
          VmSetActive.ComparisonType.LessThenOrEqualTo => "<=",
        };
#pragma warning restore CS8524 // The switch expression does not handle some values of its input type (it is not exhaustive) involving an unnamed enum value.

        var expected = param.Expected;
        var type = property.GetType();
        if(param.IsExpectedString == true
        || type == typeof(PropertyString)
        || type == typeof(PropertyListString)
        || type == typeof(PropertyMapStringString))
        {
          expected = $"\"{expected}\"";
        }

        if(i == 0)
        {
          result += $"{property.name}{indexer} {comparison} {expected} ";
        }
        else
        {
          result += $"{conditionalLogic} {property.name}{indexer} {comparison} {expected} ";
        }
      }

    }

    EditorGUILayout.LabelField("예상 로직 : ");
    EditorGUILayout.TextArea(result);
  }
}