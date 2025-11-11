using UnityEngine;

public class ArmLegSpeedControl : StateMachineBehaviour
{
  [SerializeField] public float Speed { get; set; } = 1f;

  override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    if (stateInfo.IsName("idle"))
    {
      animator.speed = Speed;
    }
  }

  override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
  {
    animator.speed = 1.0f; // 원래대로 복구
  }
}
