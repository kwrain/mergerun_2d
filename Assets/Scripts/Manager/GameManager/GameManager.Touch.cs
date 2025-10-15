

using System.Collections.Generic;
using UnityEngine;


/**
* GameManager.Event.cs
* 작성자 : lds3794@gmail.com
* 작성일 : 2022년 06월 22일 오후 9시 47분
*/
public partial class GameManager : ITouchEvent
{
  private const int TOUCH_PRIORITY = 1000;

  private Vector2 standardPos;
  private BaseObject selectedObject;

  private GameCamera gameCamera;

  public LayerMask AllLayer { get; private set; }
  public LayerMask BuildingLayer { get; private set; }
  public LayerMask NpcLayer { get; private set; }

  public GameCamera GameCamera
  {
    get
    {
      if (gameCamera == null || gameCamera.gameObject.IsDestroyed())
      {
        gameCamera = Camera.main.gameObject.GetComponentInParent<GameCamera>();
      }

      return gameCamera;
    }
  }
  public BaseObject SelectedObject
  {
    get => selectedObject;
    set
    {
      selectedObject = value;
    }
  }

  public List<BaseObject> GetSortedHitObjects(List<BaseObject> hitObjects, bool onlyLayer = false)
  {
    // kw 24.12.17
    // - 데코레이션 건물은 터치 우선 순위를 최하위로 설정한다.
    // - BuildingGround 보다는 높아야함.

    hitObjects.Sort(LayerSort);
    return hitObjects;

    int LayerSort(BaseObject obj1, BaseObject obj2)
    {
      GetSortData(obj1, out var sortingLayer1, out var orderInLayer1);
      GetSortData(obj2, out var sortingLayer2, out var orderInLayer2);

      // 동일 위치 sortingLayer 우선적으로 확인한다.
      if (sortingLayer1 == sortingLayer2)
      {
        return orderInLayer2.CompareTo(orderInLayer1);
      }
      else
      {
        return sortingLayer2.CompareTo(sortingLayer1);
      }
    }

    void GetSortData(BaseObject obj, out int sortingLayer, out int orderInLayer)
    {
      sortingLayer = obj.SortingLayerValue;
      orderInLayer = obj.RenderOrder;
    }

  }


  public void OnTouchBegan(Vector3 pos, bool isFirstTouchedUI)
  {

  }

  public void OnTouchStationary(Vector3 pos, float time, bool isFirstTouchedUI)
  {

  }

  public void OnTouchMoved(Vector3 lastPos, Vector3 newPos, bool isFirstTouchedUI)
  {
  }

  public void OnTouchEnded(Vector3 pos, bool isFirstTouchedUI, bool isMoved)
  {

  }

  public void OnTouchCanceled(Vector3 pos, bool isFirstTouchedUI, bool isMoved)
  {
  }

  public void OnLongTouched(Vector3 pos, bool isFirstTouchedUI)
  {
  }

  public void OnClicked(Vector3 pos, bool isFirstTouchedUI)
  {
  }

  public void OnPinchUpdated(float offset, float zoomSpeed, bool isFirstTouchedUI)
  {
  }
  public void OnPinchEnded()
  {
  }

  public void OnChangeTouchEventState(bool state)
  {

  }
}