using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using static StageDataTable;

public class MergeableBase : BaseObject
{
  protected Coroutine coDropTimer;
  protected Sequence fadeOutSequence;

  [SerializeField] protected Rigidbody2D rb;
  [SerializeField] protected TextMeshPro text;
  [SerializeField] protected SpriteRenderer spriteRenderer;
  [SerializeField] protected SpriteRenderer spriteRendererShadow;

  [Header("[Move]"), SerializeField] protected float moveDuration = 0.1f;
  [Header("[Scale]"), SerializeField] protected float scaleUpDuration = 0.025f; // 커지는 시간
  [SerializeField] protected float scaleDownDuration = 0.025f; // 작아지는 시간
  [SerializeField, Tooltip("커지는 비율")] protected float scaleUpFactor = 1.5f; // 커지는 비율 (원래 크기의 1.2배)
  [SerializeField, Tooltip("작아지는 비율")] protected float scaleDownFactor = 0.7f;

  [Header("[Data]"), SerializeField] protected GameDataTable.LevelData levelData;

  [field: SerializeField] public virtual int Level { get; protected set; } = 1;

  public bool IsPlayer { get; protected set; } = false;
  public bool IsMerging { get; protected set; } = false;
  public bool IsDropCounting => coDropTimer != null;

  protected override void Awake()
  {
    base.Awake();

    if (rb == null)
    {
      rb = gameObject.GetComponent<Rigidbody2D>();
    }
  }

  protected override void Start()
  {
    base.Start();

    // 단계에 맞는 데이터 가지고 오기
    UpdateLevelData();
  }

  protected override void OnDisable()
  {
    base.OnDisable();

    StopAllCoroutines();
  }

  protected virtual void OnCollisionEnter2D(Collision2D collision)
  {
    if (IsMerging)
      return;

    var otherObject = collision.gameObject.GetComponent<MergeableObject>();
    if (otherObject != null)
    {
      if (otherObject.IsMerging || (Level != otherObject.Level))
      {
        // 합성이 불가능한 충돌 → 튕김 사운드
        SoundManager.Instance.PlayFX(SoundFxTypes.BOUNCE);
        return;
      }

      // 병합 처리
      Merge(otherObject);
    }
  }

  protected virtual void Initialize()
  {
    IsMerging = false;

    rb.simulated = true;
    circleCollider.enabled = true;

    if (fadeOutSequence != null)
    {
      fadeOutSequence.Kill();
      fadeOutSequence = null;
    }

    StopAllCoroutines();
    text.DOKill();
    spriteRenderer.DOKill();
    spriteRendererShadow.DOKill();
    spriteRendererShadow.color = new Color(1f, 1f, 1f, 0.3f);
    spriteRenderer.color = text.color = Color.white;
  }

  [ContextMenu("UpdateLevelData")]
  protected virtual void UpdateLevelData()
  {
    var table = SOManager.Instance.GameDataTable;
    spriteRenderer.sprite = table.SpriteAtlas.GetSprite($"m{(Level - 1) % 11 + 1}");

    levelData = table.GetLevelData(Level);
    // Debug.Log($"levelData : {levelData} / Level : {Level}");
    text.text = levelData.PowerOfTwoString;
  }

  public virtual void SetData(MergeableData mergeableData)
  {
    transform.position = mergeableData.position;
    transform.localScale = mergeableData.scale;
    transform.localRotation = Quaternion.identity;

    Initialize();
  }

  public virtual void SetLevel(int level)
  {
    Level = level;
    UpdateLevelData();
  }

  // 병합 및 레벨업 처리 함수
  protected virtual void Merge(MergeableObject other)
  {
    IsMerging = true;
    Level++;
    // 2. 'other' 오브젝트를 0.3초간 'this'의 위치로 이동

    other.circleCollider.enabled = false;
    StartCoroutine(MoveTowardsTarget(other, transform));

    IEnumerator MoveTowardsTarget(MergeableObject from, Transform to)
    {
      float elapsed = 0f;
      Vector3 start = from.transform.position;
      while (elapsed < moveDuration)
      {
        if (!to || !from)
          yield break;

        elapsed += Time.deltaTime;
        float t = elapsed / moveDuration;
        // 현재 목표 위치를 실시간으로 참조
        from.transform.position = Vector3.Lerp(start, to.position, t);
        yield return null;
      }

      from.transform.position = to.position;
      MoveComplete();
    }

    void MoveComplete()
    {
      IsMerging = false;

      if (!gameObject.activeSelf)
        return;

      // 이동 완료 후, other 오브젝트를 풀링하기 전에 스케일 애니메이션 시작
      // 3. 이동이 완료되면 'this' 오브젝트의 스케일을 변경하는 시퀀스 시작
      // 먼저 커지는 애니메이션

      SetLevel(Level);
      StageManager.Instance.PushMergeableInPool(other);
      TweenScale(Vector3.one * levelData.scale * scaleUpFactor, scaleUpDuration, Ease.InQuad, ScaleComplete);
    }

    void ScaleComplete()
    {
      if (!gameObject.activeSelf)
        return;

      TweenScale(Vector3.one * levelData.scale, scaleDownDuration, Ease.OutQuad);

      // 합성 완료 사운드
      SoundManager.Instance.PlayFX(SoundFxTypes.MERGE);
    }
  }

  public void TweenScale(Vector3 to, float duration, Ease ease, TweenCallback onComplete = null)
  {
    var tween = transform.DOScale(to, duration).SetEase(Ease.OutQuad);
    tween.OnComplete(onComplete);
  }

  public Sequence TweenAlpha(float to, float duration, Ease ease, TweenCallback onComplete = null)
  {
    text.DOKill();
    spriteRenderer.DOKill();
    spriteRendererShadow.DOKill();

    if (fadeOutSequence != null)
    {
      fadeOutSequence.Kill();
    }

    fadeOutSequence = DOTween.Sequence();
    fadeOutSequence.Join(text.DOFade(to, duration).SetEase(ease));
    fadeOutSequence.Join(spriteRenderer.DOFade(to, duration).SetEase(ease));
    fadeOutSequence.Join(spriteRendererShadow.DOFade(to, duration).SetEase(ease));

    // 람다식(Lambda)을 사용하면 더 간결하게 표현 가능합니다.
    fadeOutSequence.OnComplete(() => { onComplete?.Invoke(); });

    return fadeOutSequence;
  }

  public virtual void StartDropTimer(float time)
  {
    if (coDropTimer != null)
      return;

    // 연출 시작
    spriteRenderer.material.SetFloat("_TimeSpeed", 3);
    spriteRenderer.material.SetFloat("_SineGlowFade", 1);
    spriteRenderer.material.SetColor("_SineGlowColor", Color.red);
    coDropTimer = StartCoroutine(Timer(time, Drop));
  }

  public virtual void StopDropTimer()
  {
    if (coDropTimer == null)
      return;

    spriteRenderer.material.SetFloat("_SineGlowFade", 0);
    StopCoroutine(coDropTimer);
    coDropTimer = null;
  }

  public virtual void Drop()
  {
    coDropTimer = null;
    spriteRenderer.material.SetFloat("_SineGlowFade", 0);
  }

  private IEnumerator Timer(float time, Action onComplete)
  {
    yield return new WaitForSeconds(time);

    onComplete?.Invoke();
  }
}
