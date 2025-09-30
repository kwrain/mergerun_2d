using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[CreateAssetMenu(fileName = "LevelData", menuName = "Data/Game/LevelData")]
public class LevelDataTable : ScriptableObject
{
  // 마지막 데이터 기준으로 반복처리.
  [Serializable]
  public class Data
  {
    const char BASE_ALPHABET = 'A';

    public uint level = 1;
    public ulong exp = 2;
    public float speed = 1.0f;

    /// <summary>
    /// 2의 n제곱을 지정된 규칙의 문자열로 변환합니다.
    /// </summary>
    /// <returns>규칙에 따라 변환된 문자열</returns>
    public string PowerOfTwoString
    {
      get
      {
        // n이 13 이하일 경우: 2^n 값을 그대로 반환
        if (level <= 13)
        {
          // Math.Pow는 double을 반환하므로 long으로 형변환
          long result = (long)Math.Pow(2, level);
          return result.ToString();
        }
        // n이 14 이상일 경우: 숫자 + 알파벳 조합
        else
        {
          // 숫자 부분 계산: 2^(((n-14) % 10) + 4)
          uint numberExponent = ((level - 14) % 10) + 4;
          ulong numberPart = (ulong)Math.Pow(2, numberExponent);

          // 알파벳 부분 계산: 'A' + ((n-14) / 10)
          uint alphabetIndex = (level - 14) / 10;
          char alphabetPart = (char)(BASE_ALPHABET + alphabetIndex);

          return $"{numberPart}{alphabetPart}";
        }
      }
    }
  }

  [SerializeField] private SpriteAtlas spriteAtlas;
  [SerializeField] private List<Data> values;

  public SpriteAtlas SpriteAtlas => spriteAtlas;

  public Data GetData(int grade)
  {
    var index = grade - 1;
    if (index < values.Count)
    {
      return values[index];
    }
    else
    {
      return values[^1];
    }
  }
}
