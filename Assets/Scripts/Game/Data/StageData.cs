using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Game/Stage Data")]
public class StageData : ScriptableObject
{
  // public string stageName;      // 스테이지 이름 (예: "얼음 동굴")
  public int stageNumber;       // 스테이지 번호
  public GameObject stagePrefab; // 이 스테이지에서 사용할 프리팹 (가장 중요!
  // public float stageLength;     // 스테이지의 Y축 길이
  // public AudioClip backgroundMusic; // 스테이지 전용 배경음악 (선택)

  // public List<MergeableObject> mergeableObjects; 
  // public List<ObstacleBase> obstacles; 


}