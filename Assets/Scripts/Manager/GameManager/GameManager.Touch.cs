

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