using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public enum ENationCode
{
  KR
}

public enum ELanguageCode
{
  None = -1,  // lds - 22.5.18, None값 추가
  EN = 10, // 영어
  FR = 14, // 프랑스어
  DE = 15, // 독일어
  JA = 22, // 일본어
  KO = 23, // 한국어
  PT = 28, // 포르투갈어
  RU = 30, // 러시아어
  ES = 34, // 스페인어
  SC = 40, // 중국어 번체
  TC = 41, // 중국어 간체

  //Afrikaans = 0,
  //Arabic = 1,
  //Basque = 2,
  //Belarusian = 3,
  //Bulgarian = 4,
  //Catalan = 5,
  //Chinese = 6,
  //Czech = 7,
  //Danish = 8,
  //Dutch = 9,
  //English = 10,
  //Estonian = 11,
  //Faroese = 12,
  //Finnish = 13,
  //French = 14,
  //German = 15,
  //Greek = 16,
  //Hebrew = 17,
  //Hugarian = 18,
  //Hungarian = 18,
  //Icelandic = 19,
  //Indonesian = 20,
  //Italian = 21,
  //Japanese = 22,
  //Korean = 23,
  //Latvian = 24,
  //Lithuanian = 25,
  //Norwegian = 26,
  //Polish = 27,
  //Portuguese = 28,
  //Romanian = 29,
  //Russian = 30,
  //SerboCroatian = 31,
  //Slovak = 32,
  //Slovenian = 33,
  //Spanish = 34,
  //Swedish = 35,
  //Thai = 36,
  //Turkish = 37,
  //Ukrainian = 38,
  //Vietnamese = 39,
  //ChineseSimplified = 40,
  //ChineseTraditional = 41,
  //Unknown = 42
}

public enum ETimeTable
{
  SECONDS,
  MINUTES,
  MILLISECONDS,
  HOURS,
  DAYS,
}
