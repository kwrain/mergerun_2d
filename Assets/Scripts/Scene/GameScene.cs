using Unity.Cinemachine;
using UnityEngine;

public class GameScene : BaseScene
{
  [SerializeField] private CinemachineCamera cinemachine;

  [Header("Game"), SerializeField] private MergeablePlayer player;

  protected override void Awake()
  {
    base.Awake();

    StageManager.CreateInstance("BundleLocal/Prefabs/Manager/StageManager.prefab");
  }
}
