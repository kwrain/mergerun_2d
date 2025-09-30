using System.Collections.Generic;
using FAIRSTUDIOS.SODB.Utils;
using UnityEngine;

/**
* BuiltinLocalizeAsset.cs
* 작성자 : doseon@fairsutdios.kr (LEE DO SEON)
* 작성일 : 2023년 11월 21일 오후 2시 44분
*/

// TODO : 커스텀 에디터 및 커스텀 에디터 추가
// 빌트인 로컬라이즈 관리 > 모든 로컬라이즈 에셋 인스펙터를 로컬라이즈 한번에 보여주는 윈도우 열기
// 기능 1. 로컬라이즈 추가 => 키와 언어별로 벨류를 입력후 추가 버튼을 누르면 일괄적으로 추가
// 기능 2. 로컬라이즈 삭제 => 키를 입력한 후 삭제 버튼을 누르면 일괄적으로 추가
// 기능 3. 로컬라이즈 일괄 수정 => 키와 언어별로 벨류를 입력후 수정 버튼을 누르면 일괄적으로 수정
// (빈값 또는 원치 않는 수정 처리도 필요, ex) 특정 언어의 벨류는 변경될 필요가 없을 경우 해당 벨류 란에 값이 있더라도 수정되지 않도록 )
// 기능 4. 로컬라이즈 뷰어 => 키와 언어별 벨류가 잘 들어가있는지 확인하는 용도, 편집은 못함.
[CreateAssetMenu(fileName = "LocalizeTextData", menuName = "Data/Localize/LocalizeTextData")]
public class LocalizeTextData : ScriptableObject
{
  [SerializeField] private GenericDictionary<string, string> values;

  public bool ContainsKey(string key) => values.ContainsKey(key);

  public string GetValue(string key)
  {
    if (values.TryGetValue(key, out var value) == false)
      return key;

#if !LOCALIZATION_CHECK
    return values[key];
#else
    return Localize.LOCALIZATION_CHECK_STRING + values[key];
#endif
  }
}