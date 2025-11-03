using Unity.Cinemachine;
using UnityEngine;

public class GameScene : BaseScene
{
  [SerializeField] private CinemachineCamera cinemachine;

  [Header("Game"), SerializeField] private MergeablePlayer player;
}
