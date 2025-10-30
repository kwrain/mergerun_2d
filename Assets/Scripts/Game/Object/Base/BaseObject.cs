using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///
/// </summary>
public partial class BaseObject : MonoBehaviour
{
  [SerializeField] public CircleCollider2D circleCollider;

  protected bool Initialized { get; private set; }
  public bool IsTouchable { get; protected set; }

  protected virtual void Awake()
  {
    InitInstance();
  }

  protected virtual void Start()
  {
    
  }

  protected virtual void OnEnable()
  {
    if (IsPlaying)
    {
      SetAnimation(AnimationIndex);
    }
  }

  protected virtual void OnDisable()
  {
    // _index = -1; // 오브젝트가 꺼지기전에 재생하던 애니메이션을 다시 재생하려할떄 인덱스가 같이서 애니메이션이 재생되지 않는 것을 방지함.
    StopAnimationTimer();
  }

  public void OnDestroy()
  {
    CleanupResources();
  }

  /// <summary>
  /// GameManager 에서 관리한다.
  /// </summary>
  /// <param name="pauseStatus"></param>
  public virtual void ApplicationPause(bool pauseStatus)
  {

  }

  protected virtual bool InitInstance()
  {
    if (Initialized)
      return false;

    if (animator == null)
    {
      animator = gameObject.GetComponent<Animator>();
    }

    if (animator != null && animator.runtimeAnimatorController != null)
    {
      animationEvents = new Dictionary<string, Action>();
      animationClips = new Dictionary<string, AnimationClip>();
      foreach (var clip in animator.runtimeAnimatorController.animationClips)
      {
        animationClips[clip.name] = clip;
      }
      parameters = animator.parameters;

      animator.speed = 0f; // SetAnimation 호출로 동작하기 위해서 일단 꺼준다.
    }

    if (circleCollider == null)
    {
      circleCollider = GetComponent<CircleCollider2D>();
      if (circleCollider != null)
      {
        var radius = circleCollider.radius;
        var offset = circleCollider.offset;
        Destroy(circleCollider);
        circleCollider = gameObject.AddComponent<CircleCollider2D>();
        circleCollider.radius = radius;
        circleCollider.offset = offset;
      }
      else
      {
        circleCollider = gameObject.AddComponent<CircleCollider2D>();
      }
    }

    return Initialized = true;
  }

  /// <summary>
  /// 애니메이션이 필요한 모델인스턴스의 경우 GameManager 또는 다른 곳에서 아래 함수를 만드시 업데이트 구분에서 호출해줘야한다.
  /// </summary>
  public virtual void OnUpdate()
  {

  }

  private void CleanupResources()
  {
    animationEvents?.Clear();
    animationClips?.Clear();
    StopAnimationTimer();
  }
}
