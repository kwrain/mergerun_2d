using UnityEngine;

public class MergeablePlayer : MergeableBase, ITouchEvent
{
  [Header("X Axis Settings")]
  public float baseXSpeed = 5f;       // 좌우 이동 기본 속도
  public float sensitivityX = 1f;  // 터치 X 민감도

  [Header("Y Axis Settings")]
  public float baseYSpeed = 2f;       // 기본 Y 속도 (양수=위, 음수=아래)
  public float accelerationY = 3f;    // Y 초기 가속도 (속도가 0→baseYSpeed까지 가속되는 비율)
  public float sensitivityY = 0.01f;  // 터치 Y 민감도 (드래그 반영 비율)

  [Header("Extra Speed Settings")]
  public float extraSpeedDecayRate = 2f;  // 초당 감소 속도 (값이 클수록 빨리 0으로 감속)
  public float maxExtraSpeed = 5f;         // 추가 속도의 최대치

  private float currentXSpeed = 0f;     // 현재 X 속도
  private float currentYSpeed = 0f;     // 현재 Y 속도 (기본 + 추가)
  private float extraYSpeed = 0f;       // 터치에 의해 발생한 추가 Y 속도
  private bool appliedExtraYSpeed;       // 터치에 의해 발생한 추가 속도 적용 여부

  private bool isTouching = false;

  // 새로 추가: OnTouchMoved로 들어온 "즉시 이동할 X 거리(월드 단위)" 누적
  private float pendingXMove = 0f;

  // 장애물 종류별 감지 시간 필요함.

  protected override void Awake()
  {
    base.Awake();

    IsPlayer = true;
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
    Vector2 newPos = rb.position + new Vector2(displacementX, displacementY);
    rb.MovePosition(newPos);

    // 5) 적용 뒤 누적값 초기화
    pendingXMove = 0f;

    // (선택) 다른 시스템에서 X 속도가 필요하면 현재 프레임의 평균 X 속도로 갱신
    currentXSpeed = displacementX / Time.fixedDeltaTime;
  }

  #region Collision

  /// <summary>
  protected virtual void OnCollisionEnter2D(Collision2D collision)
  {
    base.OnCollisionEnter2D(collision);
  }

  public void OnCollisionObstacles(ObstacleBase obstacle)
  {
    switch (obstacle.Type)
    {
      case ObstacleBase.ObstacleTypes.Spike:
        var spike = obstacle as ObstacleSpike;
        if (spike != null)
        {
          if (Grade > 0)
          {

          }
          else
          {

          }
        }
        // 지정된 쿨타임 대기 처리 필요.
        break;

      case ObstacleBase.ObstacleTypes.Wall:
        var wall = obstacle as ObstacleWall;
        if (wall != null)
        {
          // 지정된 단계를 초과하지 못하는 경우
          if (Grade < Grade + wall.LimitRelativeGrade)
          {

          }
        }
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
    isTouching = true;
  }

  public void OnTouchCanceled(Vector3 pos, bool isFirstTouchedUI, bool isMoved)
  {
    currentXSpeed = 0;
    isTouching = appliedExtraYSpeed = false;
  }

  public void OnTouchEnded(Vector3 pos, bool isFirstTouchedUI, bool isMoved)
  {
    currentXSpeed = 0;
    isTouching = appliedExtraYSpeed = false;
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
