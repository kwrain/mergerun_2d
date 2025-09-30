using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BaseObject
{
  [Space, Header("[Animation]"), SerializeField] private Animator animator;
  private AnimatorControllerParameter[] parameters;
  private Dictionary<string, AnimationClip> animationClips;
  private Dictionary<string, Action> animationEvents;
  private Coroutine animationTimerCoroutine;

  private float CurrentAnimationTime { get; set; }
  protected string CurrentAnimationState => parameters[AnimationIndex].name;

  protected bool AnimationRepeat { get; private set; }

  public string AnimationName { get; private set; }
  public float AnimationLength { get; private set; }
  public float AnimationSpeed
  {
    get
    {
      if (animator != null)
        return animator.speed;

      return 0;
    }
    set
    {
      if (animator != null)
        animator.speed = value;
    }
  }
  public int AnimationIndex { get; private set; } = -1;

  public bool IsPlaying => animator != null && animator.speed == 1f;

  public virtual bool IsAnimationEnded
  {
    get
    {
      if (animator == null) return false;
      if (animator.speed != 1f)
        return false;

      return CurrentAnimationTime >= AnimationLength;
    }
  }

  private Coroutine AnimationTimer()
  {
    if (string.IsNullOrEmpty(AnimationName) || AnimationLength <= 0)
      return null;

    return StartCoroutine(CurrentAnimationTimer());

    IEnumerator CurrentAnimationTimer()
    {
      while (CurrentAnimationTime <= AnimationLength)
      {
        CurrentAnimationTime += Time.deltaTime;
        yield return null;
      }

      CurrentAnimationTime = AnimationLength;
      if (animationEvents.ContainsKey(AnimationName))
      {
        var aniName = AnimationName;
        animationEvents[aniName]?.Invoke();
        animationEvents[aniName] = null;
      }
    }
  }
  private void StopAnimationTimer()
  {
    if (animationTimerCoroutine != null)
    {
      StopCoroutine(animationTimerCoroutine);
      animationTimerCoroutine = null;
    }
  }
  
  public float GetAnimationLength(string name) => animationClips.ContainsKey(name) ? animationClips[name].length : -1;

  private void SetAnimatorController(RuntimeAnimatorController controller)
  {
    if (animator == null || controller == null)
      return;

    AnimationIndex = -1;
    animator.runtimeAnimatorController = controller;
    animationClips = new Dictionary<string, AnimationClip>();
    foreach (var clip in animator.runtimeAnimatorController.animationClips)
    {
      animationClips[clip.name] = clip;
    }
    parameters = animator.parameters;
    animator.speed = 0f;
  }

  protected void SetAnimatorController(string path)
  {
    SetAnimatorController(ResourceManager.Instance.Load<RuntimeAnimatorController>(path));
  }

  public bool SetAnimation(int index, bool replay = true, Action action = null)
  {
    if (index < 0 || animator == null)
    {
      if (animator != null)
      {
        animator.speed = 0f;
      }

      AnimationName = null;
      AnimationLength = 0;
      Reset();
      return true;
    }

    // 반복 재생 요청이 아니고, 이미 해당 애니메이션이 재생 중인 경우 리턴 처리.
    if (!replay && AnimationIndex == index)
      return true;

    AnimationIndex = index;
    AnimationRepeat = replay;
    CurrentAnimationTime = 0;

    if (index >= parameters.Length)
    {
      Debug.LogError(animator.runtimeAnimatorController.name);
      return false;
    }

    if (animationTimerCoroutine != null)
    {
      StopCoroutine(animationTimerCoroutine);
    }

    var parameter = parameters[index];
    var clip = animationClips[parameter.name];
    AnimationName = parameter.name;
    AnimationLength = clip.length;
    if (action != null)
    {
      animationEvents[parameter.name] = action;
    }
    if (animator.speed != 1f)
      animator.speed = 1f;

    animator.SetTrigger(parameter.name);

    if (gameObject.activeSelf)
    {
      animationTimerCoroutine = AnimationTimer();
    }

    return true;
  }
  public virtual bool SetAnimation(string name, bool replay = true, Action action = null)
  {
    if (animator == null) return false;
    for (var i = 0; i < parameters.Length; i++)
    {
      if (parameters[i].name == name)
      {
        return SetAnimation(i, replay, action);
      }
    }

    return false;
  }

  public void StopAnimation(float normalizedTime = 0.0f)
  {
    if (gameObject == null || animator == null || parameters == null)
      return;

    var index = AnimationIndex <= -1 ? 0 : AnimationIndex;
    if (index < 0 || index >= parameters.Length || !gameObject.activeSelf)
      return;

    if (animationTimerCoroutine != null)
    {
      StopCoroutine(animationTimerCoroutine);
    }

    var parameter = parameters[index];
    var clip = animationClips[parameter.name];
    AnimationName = parameter.name;
    AnimationLength = clip.length;

    if (animator != null)
    {
      animator.Play(parameter.name, 0, normalizedTime);
      animator.speed = 0f;
    }
  }

  /// <summary>
  /// 애니메이션 재시작???
  /// </summary>
  public void Reset()
  {
    if (animator != null && animator.speed == 1f)
    {
      SetAnimation(AnimationIndex);
    }

    CurrentAnimationTime = 0;
  }
}
