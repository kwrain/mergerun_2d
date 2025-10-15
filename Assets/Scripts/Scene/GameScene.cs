using Unity.Cinemachine;
using UnityEngine;

public class GameScene : BaseScene
{
  [SerializeField] private YAxisTracker tracker;
  [SerializeField] private CinemachineCamera cinemachine;

  [Header("Game"), SerializeField] private MergeablePlayer player;

  protected override void Start()
  {
    base.Start();

    GameManager.Instance.Player = player;
  }


}
