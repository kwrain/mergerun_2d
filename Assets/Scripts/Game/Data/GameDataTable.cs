using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

[CreateAssetMenu(fileName = "GameDataTable", menuName = "Game/GameDataTable")]
public class GameDataTable : ScriptableObject
{
  [Serializable]
  public class ExpData
  {
    public int level = 1;
    public int nextLevel = 2;

    public int exp = 1;
  }

  // 마지막 데이터 기준으로 반복처리.
  [Serializable]
  public class LevelData
  {
    const char BASE_ALPHABET = 'A';

    public int level = 1;
    public float speed = 2f;
    public float accel = 3f;
    public float scale = 1.0f;
    public float animationSpeed = 1.0f;

    public long PowerOfTwo
    {
      get
      {
        return (long)Math.Pow(2, level);
      }
    }

    /// <summary>
    /// 2의 n제곱을 지정된 규칙의 문자열로 변환합니다.
    /// </summary>
    /// <returns>규칙에 따라 변환된 문자열</returns>
    public string PowerOfTwoString
    {
      get
      {
        // level 13 이하일 때는 2^level 그대로 반환
        if (level <= 13)
        {
          ulong result = (ulong)Math.Pow(2, level);
          return result.ToString();
        }
        // 14 이상일 때는 숫자 + 알파벳 조합
        else
        {
          // 숫자 부분 계산: 2^(((n-14) % 10) + 4)
          int numberExponent = ((level - 14) % 10) + 4;
          ulong numberPart = (ulong)Math.Pow(2, numberExponent);

          // 알파벳 부분 계산: (n-14)/10 → A, B, … Z, AA, AB … ZZ, AAA …
          int alphabetIndex = (level - 14) / 10;
          string alphabetPart = GetAlphabetCode(alphabetIndex);

          return $"{numberPart}{alphabetPart}";
        }
      }
    }

    /// <summary>
    /// 0부터 시작하는 index를 A~Z, AA~ZZ, AAA~ 형태의 26진 문자열로 변환합니다.
    /// </summary>
    private string GetAlphabetCode(int index)
    {
      string result = string.Empty;
      while (index >= 0)
      {
        result = (char)(BASE_ALPHABET + (index % 26)) + result;
        index = (index / 26) - 1;
      }
      return result;
    }
  }

  [SerializeField] private SpriteAtlas spriteAtlas;
  [SerializeField] private List<ExpData> expDatas;
  [SerializeField] private List<LevelData> levelDatas;

  public SpriteAtlas SpriteAtlas => spriteAtlas;

  public ExpData GetExpData(int level)
  {
    var data = expDatas.Find(data => data.level == level);
    data ??= expDatas[^1];

    return data;
  }

  public LevelData GetLevelData(int level)
  {
    var data = levelDatas.Find(data => data.level == level);
    data ??= levelDatas[^1];

    return data;
  }

  public string PowerOfTwoString(int level)
  {
    // level 13 이하일 때는 2^level 그대로 반환
    if (level <= 13)
    {
      ulong result = (ulong)Math.Pow(2, level);
      return result.ToString();
    }
    // 14 이상일 때는 숫자 + 알파벳 조합
    else
    {
      // 숫자 부분 계산: 2^(((n-14) % 10) + 4)
      int numberExponent = ((level - 14) % 10) + 4;
      ulong numberPart = (ulong)Math.Pow(2, numberExponent);

      // 알파벳 부분 계산: (n-14)/10 → A, B, … Z, AA, AB … ZZ, AAA …
      int alphabetIndex = (level - 14) / 10;
      string alphabetPart = GetAlphabetCode(alphabetIndex);

      return $"{numberPart}{alphabetPart}";
    }
  }

  /// <summary>
  /// 0부터 시작하는 index를 A~Z, AA~ZZ, AAA~ 형태의 26진 문자열로 변환합니다.
  /// </summary>
  private string GetAlphabetCode(int index)
  {
    string result = string.Empty;
    while (index >= 0)
    {
      result = (char)('A' + (index % 26)) + result;
      index = (index / 26) - 1;
    }
    return result;
  }
}
