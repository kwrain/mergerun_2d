using System.Collections;
using TMPro;
using UnityEngine;

public class MergeableBase : BaseObject
{
  private Coroutine timer;

  [SerializeField] protected Rigidbody2D rb;
  [SerializeField] protected TextMeshPro text;

  [Header("Settings"), SerializeField] private float bounceForce = 5f;

  [Header("Data"), SerializeField] protected LevelDataTable.Data levelData;

  private SpriteRenderer spriteRenderer => sprites[0];

  [field: SerializeField] public int Grade { get; protected set; } = 1;
  [field: SerializeField] public bool IsDropCounting { get; private set; }

  public bool IsPlayer { get; protected set; } = false;

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
    var otherObject = collision.gameObject.GetComponent<MergeableObject>();
    if (otherObject != null)
    {
      if (Grade == otherObject.Grade)
      {
        // 병합 처리
        Merge(otherObject);
      }
      else    // 2. 다른 레벨인 경우 (튕겨내기 조건)
      {
     
      }
    }
  }

  protected virtual void UpdateLevelData()
  {
    var table = SOManager.Instance.LevelDataTable;
    levelData = table.GetData(Grade);

    // 이미지 교체
    spriteRenderer.sprite = table.SpriteAtlas.GetSprite($"m{Grade}");
    // 텍스트 교체
    text.text = levelData.PowerOfTwoString;
  }

  // 병합 및 레벨업 처리 함수
  protected virtual void Merge(MergeableObject other)
  {
    // 병합처리 시 확인사항
    // 1.플레이어와 병합
    // 2.오브젝트간 병합 - 현재 이동속도가 더 빠른쪽에 메인이되서 머지하도록 함.

    // 연출

    // 연출 이후 호출
    // 흡수당한 오브젝트는 오브젝트 풀링에 추가
    Grade++;
    UpdateLevelData();
    other.SetActive(false);
    // GameManager.Instance.PushBuildingInPool(other);
  }

  public void StartDropTimer(float time)
  {
    timer = StartCoroutine(Timer());

    IEnumerator Timer()
    {
      // 연출 시작
      IsDropCounting = true;

      yield return new WaitForSeconds(time);

      IsDropCounting = false;
      // 연출 종료
    }
  }

  public void StopDropTimer()
  {
    if (timer != null)
    {
      StopCoroutine(timer);
      timer = null;
      IsDropCounting = false;
    }
  }
}
