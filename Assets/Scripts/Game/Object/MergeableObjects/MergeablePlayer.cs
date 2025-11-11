using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class MergeablePlayer : MergeableBase, ITouchEvent
{
  [Header("[X Axis Settings]")]
  [SerializeField] private float baseXSpeed = 5f;       // 좌우 이동 기본 속도
  [SerializeField] private float sensitivityX = 1f;  // 터치 X 민감도
  [SerializeField] private float limitX = 5f;            // <<< [추가] X축 최소 좌표

  [Header("[Y Axis Settings]")]
  [SerializeField] private float baseYSpeed = 2f;       // 기본 Y 속도 (양수=위, 음수=아래)
  [SerializeField] private float accelerationY = 3f;    // Y 초기 가속도 (속도가 0→baseYSpeed까지 가속되는 비율)
  [SerializeField] private float sensitivityY = 0.01f;  // 터치 Y 민감도 (드래그 반영 비율)

  [Header("[Extra Speed Settings]")]
  private bool appliedExtraYSpeed;       // 터치에 의해 발생한 추가 속도 적용 여부
  [SerializeField] private float extraSpeedDecayRate = 2f;  // 초당 감소 속도 (값이 클수록 빨리 0으로 감속)
  [SerializeField] private float maxExtraSpeed = 5f;         // 추가 속도의 최대치

  [Header("[Currernt Speed]")]
  [SerializeField] private float currentXSpeed = 0f;     // 현재 X 속도
  [SerializeField] private float currentYSpeed = 0f;     // 현재 Y 속도 (기본 + 추가)
  [SerializeField] private float extraYSpeed = 0f;       // 터치에 의해 발생한 추가 Y 속도

  [Header("[Game Settings]"), SerializeField] private float obstacleDelay = 0.5f;
  [SerializeField] private SpriteRenderer[] armLegs;
  private ArmLegSpeedControl armLegSpeedControl;

  // 새로 추가: OnTouchMoved로 들어온 "즉시 이동할 X 거리(월드 단위)" 누적
  private float pendingXMove = 0f;

  // 장애물 종류별 감지 시간 필요함.
  private bool ignoreSpike;

  [field: SerializeField] public bool Movable { get; set; }

  public override int Level
  {
    get => base.Level;
    protected set
    {
      base.Level = value;

      if (!StageManager.Instance.Infinity)
      {
        GameModel.Global.ObstacleGoalColor = GetObstacleGoalColor(value);
      }
    }
  }

  protected override void Awake()
  {
    base.Awake();

    IsPlayer = true;
    if (animator != null)
    {
      armLegSpeedControl = animator.GetBehaviour<ArmLegSpeedControl>();
    }
  }

  protected override void Start()
  {
    base.Start();

    currentYSpeed = 0f; // 시작 시 0에서 출발 → 가속도로 baseYSpeed까지 올라감
  }

  protected override void OnEnable()
  {
    base.OnEnable();

    TouchManager.AddListenerTouchEvent(this);
  }

  protected override void OnDisable()
  {
    base.OnDisable();

    TouchManager.RemoveListenerTouchEvent(this);
  }

  private void FixedUpdate()
  {
    MoveObject();
  }
  
  private void MoveObject()
  {
    if (!Movable)
      return;

    // 1) Y 속도 보정 (가속)
    currentYSpeed = Mathf.MoveTowards(currentYSpeed, baseYSpeed, accelerationY * Time.fixedDeltaTime);

    // 2) extraYSpeed는 추가 속도가 존재하는 경우에만 감소 처리.
    if (extraYSpeed != 0)
    {
      if (Mathf.Abs(extraYSpeed) > 0.01f)
      {
        extraYSpeed = Mathf.MoveTowards(extraYSpeed, 0f, extraSpeedDecayRate * Time.fixedDeltaTime);
      }
      else
      {
        extraYSpeed = 0f;
      }
    }

    float totalYSpeed = currentYSpeed + extraYSpeed;
    float displacementY = totalYSpeed * Time.fixedDeltaTime;

    // 3) X는 OnTouchMoved에서 누적한 '거리'를 그대로 적용 (월드 단위)
    float displacementX = pendingXMove;

    // 4) 물리 이동을 한 번에 수행 (충돌 계산 유지)
    Vector2 currentPos = rb.position; // <<< [수정] 이동 전 현재 위치 저장
    Vector2 newPos = currentPos + new Vector2(displacementX, displacementY);

    // 5) <<< [추가] X축 경계 처리
    // newPos.x 값을 minX와 maxX 사이로 제한합니다.
    newPos.x = Mathf.Clamp(newPos.x, limitX * -1f, limitX);

    rb.MovePosition(newPos);

    // 6) <<< [수정] 적용 뒤 누적값 초기화 (순서 변경)
    pendingXMove = 0f;

    // 7) <<< [수정] 다른 시스템에서 X 속도가 필요하면 '실제' 이동한 거리로 갱신
    // (경계에 막혔을 경우 displacementX와 실제 이동 거리가 다를 수 있음)
    float actualDisplacementX = newPos.x - currentPos.x;
    currentXSpeed = actualDisplacementX / Time.fixedDeltaTime;
  }

  private void StartGame()
  {
    Movable = false;
    transform.DOKill();
    transform.localScale = Vector3.zero;
    TweenScale(Vector3.one * levelData.scale * scaleUpFactor, 0.5f, Ease.OutQuad, () =>
    {
      if (!gameObject.activeSelf)
        return;

      TweenScale(Vector3.one * levelData.scale, scaleDownDuration, Ease.OutQuad, () =>
      {
        Movable = true;
      });
    });
  }

  public Color GetObstacleGoalColor(int level)
  {
    string hex = level switch
    {
      1 => "EF471C",// 2 : #EF471C
      2 => "FF6B32",// 4 : #FF6B32
      3 => "A04EC0",// 8 : #A04EC0
      4 => "FF9102",// 16 : #FFB902
      5 => "FC8C1C",// 32 : #FC8C1C
      6 => "EF471C",// 64 : #EF471C
      7 => "CEDA0D",// 128 : #CEDA0D
      8 => "FF8661",// 256 : #FF8661
      9 => "FF9102",// 512 : #F5BD26
      10 => "A2D03C",// 1024 : #A2D03C
      11 => "49A837",// 2048 : #49A837
      _ => null,
    };

    return hex.ToColorFromHex();
  }

  public Color GetArmLegColor(int level)
  {
    // 팔다리 Mergeable Object 별 Color HexCode
    string hex = level switch
    {
      1 => "C01B09",// 2 : #C01B09
      2 => "AA3D1E",// 4 : #AA3D1E
      3 => "7A289C",// 8 : #7A289C
      4 => "E5651E",// 16 : #E5651E
      5 => "C03D07",// 32 : #C03D07
      6 => "C01B09",// 64 : #C01B09
      7 => "729301",// 128 : #729301
      8 => "C9370C",// 256 : #C9370C
      9 => "D3851F",// 512 : #D3851F
      10 => "447925",// 1024 : #447925
      11 => "577133",// 2048 : #577133
      _ => null,
    };

    return hex.ToColorFromHex();
  }


  protected override void Initialize()
  {
    base.Initialize();

    spriteRenderer.material.SetFloat("_SineGlowFade", 0);
  }

  public override void SetData(StageDataTable.MergeableData mergeableData)
  {
    base.SetData(mergeableData);
    baseYSpeed = levelData.speed;
    accelerationY = levelData.accel;
    IsMerging = ignoreSpike = false;
    gameObject.SetActive(true);
    StartGame();
  }

  protected override void UpdateLevelData()
  {
    base.UpdateLevelData();

    baseYSpeed = levelData.speed;
    accelerationY = levelData.accel;

    armLegSpeedControl.Speed= levelData.animationSpeed;
    
    var color = GetArmLegColor(Level);
    foreach (var sr in armLegs)
    {
      sr.color = color;
    }
  }

  protected override void Merge(MergeableObject other)
  {
    base.Merge(other);

    // 최대 합성 단계를 기록해야한다.
    if (StageManager.Instance.Infinity)
    {
      SOManager.Instance.PlayerPrefsModel.UserBestLevel = Level;
      var text = SOManager.Instance.GameDataTable.PowerOfTwoString(Level);
      StageManager.Instance.SetText(text);
    }
  }

  public override void StartDropTimer(float time)
  {
    base.StartDropTimer(time);

    if (ignoreSpike)
    {
      // 컬러와 속도를 바꿔줌.
      spriteRenderer.material.SetFloat("_TimeSpeed", 3);
      spriteRenderer.material.SetColor("_SineGlowColor", Color.red);
    }
  }

  public override void StopDropTimer()
  {
    if (ignoreSpike)
    {
      // ignoreSpike 가 아직 유효한 경우
      spriteRenderer.material.SetFloat("_TimeSpeed", 6);
      spriteRenderer.material.SetColor("_SineGlowColor", Color.white);

      StopCoroutine(coDropTimer);
      coDropTimer = null;
    }
    else
    {
      base.StopDropTimer();
    }
  }

  protected override void Drop()
  {
    if (ignoreSpike)
    {
      spriteRenderer.material.SetFloat("_TimeSpeed", 6);
      spriteRenderer.material.SetColor("_SineGlowColor", Color.white);

      coDropTimer = null;
    }
    else
    {
      base.Drop();
    }

    StageManager.Instance.StartStage(restart:true);
  }


  #region Collision

  IEnumerator Timer(float time, Action onComplete)
  {
    yield return new WaitForSeconds(time);
    onComplete?.Invoke();
  }

  private void Spike()
  {
    if (ignoreSpike)
      return;

    if (Level > 1)
    {
      // 한 단계 감소하고, MergeableObject 를 생성한다.
      SetLevel(--Level);

      if (!IsDropCounting) // 맵 외곽 연출이 더 우선순위가 높음.
      {
        spriteRenderer.material.SetFloat("_TimeSpeed", 6);
        spriteRenderer.material.SetFloat("_SineGlowFade", 1);
        spriteRenderer.material.SetColor("_SineGlowColor", Color.white);
      }

      StartCoroutine(Timer(obstacleDelay, ()=>
      {
        if (!IsDropCounting)
        {
          spriteRenderer.material.SetFloat("_SineGlowFade", 0);
        }
      }));

      // 단계 변경 시 크기 감소
      TweenScale(Vector3.one * levelData.scale * scaleDownFactor, scaleDownDuration, Ease.InQuad, () =>
      {
        if (!gameObject.activeSelf)
          return;

        var mergeable = StageManager.Instance.PeekMergeableInPool();
        if (mergeable != null)
        {
          // 무작위 방향으로 튕겨내며, 일정 시간뒤에 사라져야함.
          // 생성된 오브젝트는 상호작용을 하지 않는다.
          mergeable.SetLevel(Level);
          mergeable.circleCollider.enabled = false;
          mergeable.transform.position = transform.position;
          mergeable.transform.localScale = Vector3.zero;
          mergeable.TweenScale(Vector3.one * levelData.scale, 0.1f, Ease.InQuad, () =>
          {
            if (!mergeable.gameObject.activeSelf)
              return;

            var seq = mergeable.TweenAlpha(0, 1f, Ease.OutQuad, () => { StageManager.Instance.PushMergeableInPool(mergeable); });
            seq.Join(mergeable.transform.DOMove(transform.position - new Vector3(3, 3, 0), 1).SetEase(Ease.InOutQuad));
          });
        }

        TweenScale(Vector3.one * levelData.scale, scaleUpDuration, Ease.OutQuad);
      });

      // 지정된 쿨타임동안 가시 영향을 받지 않도록함.
      ignoreSpike = true;
      StartCoroutine(Timer(obstacleDelay, ()=> { ignoreSpike = false; }));
    }
    else
    {
      // 게임오버 -> 재시작
      StageManager.Instance.StartStage(restart: true);
    }
  }

  private void Goal(ObstacleGoal goal)
  {
    if (goal.IsChecked)
      return;

    goal.IsChecked = true;
    if (goal != null)
    {
      if (StageManager.Instance.Infinity)
      {
        // 지정된 단계를 초과하지 못하는 경우
        if (Level < SOManager.Instance.PlayerPrefsModel.UserSavedLevel + goal.LimitRelativeLevel)
        {
          // 벽에 부딪힌 경우 더 이상 진행이 불가능할때,
          // 이동을 멈추고, 죽는 연출 이후, 광고를 노출한다.
          // 리워드광고 완료 이후 스테이지 재시작 처리를 한다.
          Movable = false;
          goal.Animator.SetTrigger("fail");
          StartCoroutine(Timer(1f, () =>
          {
            StageManager.Instance.StartStage(restart: true);
          }));

        }
        else
        {
          goal.Animator.SetTrigger("pass");
          StageManager.Instance.PushObstacleInPool(goal);
        }
      }
      else
      {
        goal.Animator.SetTrigger("pass");
        StageManager.Instance.CompleteStage();
      }

    }
  }

  public void OnCollisionObstacles(ObstacleBase obstacle)
  {
    switch (obstacle.Type)
    {
      case ObstacleBase.ObstacleTypes.Spike:
        Spike();
        break;

      case ObstacleBase.ObstacleTypes.Goal:
        Goal(obstacle as ObstacleGoal);
        break;

      default:
#if LOG
        Debug.Log("This type is not supported.");
#endif
        break;
    }
  }

  #endregion

  #region Touch

  public void OnChangeTouchEventState(bool state)
  {
  }

  public void OnClicked(Vector3 pos, bool isFirstTouchedUI)
  {
  }

  public void OnLongTouched(Vector3 pos, bool isFirstTouchedUI)
  {
  }

  public void OnPinchEnded()
  {
  }

  public void OnPinchUpdated(float offset, float zoomSpeed, bool isFirstTouchedUI)
  {
  }

  public void OnTouchBegan(Vector3 pos, bool isFirstTouchedUI)
  {
  }

  public void OnTouchCanceled(Vector3 pos, bool isFirstTouchedUI, bool isMoved)
  {
    currentXSpeed = 0;
    appliedExtraYSpeed = false;
  }

  public void OnTouchEnded(Vector3 pos, bool isFirstTouchedUI, bool isMoved)
  {
    currentXSpeed = 0;
    appliedExtraYSpeed = false;
  }

  public void OnTouchMoved(Vector3 lastPos, Vector3 newPos, bool isFirstTouchedUI)
  {
    var delta = newPos - lastPos;

    // lastPos/newPos가 스크린 좌표라면 월드 좌표로 변환
    Vector3 lastWorld = Camera.main.ScreenToWorldPoint(lastPos);
    Vector3 newWorld = Camera.main.ScreenToWorldPoint(newPos);
    var worldDelta = newWorld - lastWorld;
    float moveXWorld = worldDelta.x * sensitivityX;
    pendingXMove += moveXWorld;

    // Y 터치에 의한 속도 보정 (기존 방식 유지)
    if (extraYSpeed == 0f && !appliedExtraYSpeed)
    {
      float speedChange = delta.y * sensitivityY;
      extraYSpeed += speedChange;
      extraYSpeed = Mathf.Clamp(extraYSpeed, -maxExtraSpeed, maxExtraSpeed);
      appliedExtraYSpeed = true;
    }
  }

  public void OnTouchStationary(Vector3 pos, float time, bool isFirstTouchedUI)
  {
  }

  #endregion
}
