using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

/**
* GraphicRaycasterRegistration.cs
* 작성자 : doseon@fairsutdios.kr (LEE DO SEON)
* 작성일 : 2023년 03월 16일 오전 9시 31분
*/

[RequireComponent(typeof(GraphicRaycaster))]
public class GraphicRaycasterRegistration : MonoBehaviour
{
  private GraphicRaycaster raycaster;
  private CancellationTokenSource cancellationTokenSource;
  private void Awake()
  {
    TryGetComponent<GraphicRaycaster>(out raycaster);
  }

  private async void OnEnable()
  {
    bool registable = false; // 등록가능한지 여부
    // UIManager가 생성되지않은 경우
    if(UIManager.IsCreated == false)
    {
      cancellationTokenSource = new();
      while(true)
      {
        // 어플리케이션이 도중에 종료 되거나
        // UIManager 인스턴스가 생성되기 전에 오브젝트 또는 컴포넌트가 비활성화 되어 캔슬 되면
        // while문을 빠져나온다.
        if(Application.isPlaying == false 
        || cancellationTokenSource.IsCancellationRequested == true)
        {
          break;
        }
        // UIManager가 생성이된 경우
        if(UIManager.IsCreated == true)
        {
          // 등록 가능한 상태로 변경 후 while문 빠져나옴
          registable = true;
          break;
        }
        await Task.Yield();
      }
    }
    // UIManager가 생성되어 있는 경우
    else
    {
      registable = true;
    }
    if(registable == false) return;
    // 등록 가능한 상태라면 먼저 현재 UIManager의 !IsBlockEvent로 동기화
    SetEnableRaycaster(!UIManager.Instance.IsBlockEvent);
    // onChangeBlockEventState에 추가.
    UIManager.Instance.onChangeBlockEventState += SetEnableRaycaster;
  }

  private void OnDisable()
  {
    // OnEnable내 Task 정지
    cancellationTokenSource?.Cancel();
    cancellationTokenSource = null;
    // UIManager가 생성되었고, 객체가 존재하는 경우에
    if(UIManager.IsCreated == true
    && UIManager.Instance != null)
    {
      // onChangeBlockEventState에서 제거.
      UIManager.Instance.onChangeBlockEventState -= SetEnableRaycaster;
    }
  }

  private void OnApplicationQuit()
  {
    // OnEnable내 Task 정지
    cancellationTokenSource?.Cancel();
    cancellationTokenSource = null;
    // UIManager가 생성되었고, 객체가 존재하는 경우에
    if (UIManager.IsCreated == true
    && UIManager.Instance != null)
    {
      // onChangeBlockEventState에서 제거.
      UIManager.Instance.onChangeBlockEventState -= SetEnableRaycaster;
    }
  }

  private void SetEnableRaycaster(bool enabled)
  {
    if(raycaster == null) return;
    raycaster.enabled = enabled;
  }
}