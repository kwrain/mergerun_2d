using System.Collections.Generic;
using System.Linq;
using FAIRSTUDIOS.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class KRaycast
{
  #region Raycast2D

  private static RaycastHit2D Raycast2D(Ray ray, int? layerMask = null, float? distance = null)
  {
    RaycastHit2D[] res = null;
    if (layerMask.HasValue)
      res = Physics2D.RaycastAll(ray.origin, ray.direction, distance.HasValue ? distance.Value : Camera.main.farClipPlane, layerMask.Value).OrderBy(d => d.distance).ToArray();
    else
      res = Physics2D.RaycastAll(ray.origin, ray.direction, distance.HasValue ? distance.Value : Camera.main.farClipPlane).OrderBy(d => d.distance).ToArray();

    if (res == null || res.Length == 0)
      return default;

    return res[0];
  }
  private static RaycastHit2D[] Raycast2DAll(Ray ray, int? layerMask = null, float? distance = null)
  {
    RaycastHit2D[] res = null;
    if (layerMask.HasValue)
      res = Physics2D.RaycastAll(ray.origin, ray.direction, distance.HasValue ? distance.Value : Camera.main.farClipPlane, layerMask.Value).OrderBy(d => d.distance).ToArray();
    else
      res = Physics2D.RaycastAll(ray.origin, ray.direction, distance.HasValue ? distance.Value : Camera.main.farClipPlane).OrderBy(d => d.distance).ToArray();

    if (res == null || res.Length == 0)
      return default;

    return res;
  }
  private static RaycastHit2D Raycast2DTransparency(Ray ray, int? layerMask = null, float? distance = null)
  {
    RaycastHit2D[] res = null;
    if (layerMask.HasValue)
      res = Physics2D.RaycastAll(ray.origin, ray.direction, distance.HasValue ? distance.Value : Camera.main.farClipPlane, layerMask.Value).OrderBy(d => d.distance).ToArray();
    else
      res = Physics2D.RaycastAll(ray.origin, ray.direction, distance.HasValue ? distance.Value : Camera.main.farClipPlane).OrderBy(d => d.distance).ToArray();

    RaycastHit2D raycastHit2D;
    if (res == null || res.Length == 0)
    {
      raycastHit2D = new RaycastHit2D();
      return raycastHit2D;
    }

    //현재 위치 저장
    var screenPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

    //리스트 순회
    for (var i = 0; i < res.Length; i++)
    {
      raycastHit2D = res[i];

      var rend = raycastHit2D.transform.GetComponent<SpriteRenderer>();
      SpriteRenderer[] rends = null;
      if (rend == null)
      {
        rends = raycastHit2D.transform.GetComponentsInChildren<SpriteRenderer>();
        if (rends == null)
          continue;

        if (rends.Length <= 0)
          return raycastHit2D;

        rend = rends[0];
      }

      if (rend.sprite == null)
        continue;

      var tx = rend.sprite.texture;
      if (!tx.isReadable || (tx.isReadable && tx.format != TextureFormat.RGBA32))
      {
        return raycastHit2D;
      }

      //실제 이미지에서 터치한 영역을 찾는다
      var TPosition = screenPos;
      TPosition = rend.transform.InverseTransformPoint(TPosition);
      //HDebug.Log(string.Format("<color=red>{0}</color> {1} // {2}", rend.name, screenPos, TPosition), rend.gameObject);

      // 비율로 변환된 이미지 사이즈를 찾는다.
      //float _width = rend.sprite.bounds.size.x * rend.sprite.pixelsPerUnit;
      //float _height = rend.sprite.bounds.size.y * rend.sprite.pixelsPerUnit;
      //float ratioSizeX = rend.sprite.rect.width / rend.sprite.pixelsPerUnit;
      //float ratioSizeY = rend.sprite.rect.height / rend.sprite.pixelsPerUnit;

      ////HDebug.Log( string.Format("<color=red>{0}</color>",rend.name),rend.gameObject);
      ////HDebug.Log(string.Format("{0} {1}", _width, _height));

      //// 객체의 크기에 따른 이미지 사이즈를 찾는다.
      //float txX = _width / (rend.sprite.bounds.size.x * rend.transform.localScale.x);
      //float txY = _height / (rend.sprite.bounds.size.y * rend.transform.localScale.y);

      //float txX = (rend.sprite.pixelsPerUnit / rend.transform.localScale.x * ratioSizeX);
      //float txY = (rend.sprite.pixelsPerUnit / rend.transform.localScale.y * ratioSizeY);


      var txX = rend.sprite.pixelsPerUnit / rend.transform.localScale.x;
      var txY = rend.sprite.pixelsPerUnit / rend.transform.localScale.y;

      //float txX = (rend.sprite.pixelsPerUnit / rend.transform.localScale.x) * ratioSizeX;
      //float txY = (rend.sprite.pixelsPerUnit / rend.transform.localScale.y) * ratioSizeY;

      //HDebug.Log(string.Format("{0} {1}", txX, txY));


      //float fPivotValueX1 = rend.sprite.pivot.x;
      //float fPivotValueY1 = rend.sprite.pivot.y;
      //float fPivotValueX = rend.sprite.pivot.normalized.x;
      //float fPivotValueY = rend.sprite.pivot.normalized.y;

      ////X의 피벗 값이 0보다 작은 값이 나오는 경우가 있다.
      //if (fPivotValueX < 0)
      //  fPivotValueX = 0; //0으로 해준다.

      ////Y의 피벗 값이 0보다 작은 값이 나오는 경우가 있다.
      //if (fPivotValueY < 0)
      //  fPivotValueY = 0; //0으로 해준다.


      //float pivotX = fPivotValueX / _width;
      //float pivotY = fPivotValueY / _height;

      //float pivotX = rend.sprite.pivot.x / _width;
      //float pivotY = rend.sprite.pivot.y / _height;

      //HDebug.Log(string.Format("{0} {1}", pivotX, pivotY));

      //피벗과 픽셀 퍼센트에 따른 실제 이미지내 터치영역을 찾는다.
      var pX = Mathf.RoundToInt((TPosition.x * txX) + rend.sprite.pivot.x);
      var pY = Mathf.RoundToInt((TPosition.y * txY) + rend.sprite.pivot.y);

      // 아틀라스 대응
      pX = (int)rend.sprite.rect.x + pX;
      pY = (int)rend.sprite.rect.y + pY;

      //DebugHelper.Log(rend.size.x);
      //DebugHelper.Log(rend.size.y);

      //실제 이미지가 커진 픽셀 사이즈를 구한다
      var fSizeX = (rend.size.x * rend.sprite.pixelsPerUnit) * 0.5f;
      var fSizeY = (rend.size.y * rend.sprite.pixelsPerUnit) * 0.5f;

      //rend.size.x; //이미지 가로 사이즈 비율
      //rend.size.y; //이미지 세로 사이즈 비율
      //rend.sprite.rect.width;  //이미지 가로 사이즈
      //rend.sprite.rect.height; //이미지 세로 사이즈

      //Rect rectTexturer = rend.sprite.rect.xMax - rend.sprite.rect.xMin

      //이미지 영역 안에 있는지 체크 한다.(아틀라스 이미지 체크)
      if (rend.sprite.rect.center.x - fSizeX <= pX && pX <= rend.sprite.rect.center.x + fSizeX
        && rend.sprite.rect.center.y - fSizeY <= pY && pY <= rend.sprite.rect.center.y + fSizeY)
      {
        //이미지 영역 안에 있을 경우
        var _color = rend.sprite.texture.GetPixel(pX, pY);

        //HDebug.Log(string.Format("{0}  {1}  {2} == {3},{4}", h.collider.name, _color,tx.name,pX,pY));
        if (_color.a > 0.01f)
        {
          //HDebug.Log(string.Format("hit {0}", raycastHit2D.collider.name), raycastHit2D.collider.gameObject);

          return raycastHit2D;
        }
        //else // 아틀라스 문제로 주석 처리함
        //{
        //  _color = tx.GetPixel(Mathf.RoundToInt(rend.sprite.textureRect.x) + pX, Mathf.RoundToInt(rend.sprite.textureRect.y) + pY);

        //  if (_color.a > 0)
        //  {
        //    return raycastHit2D;
        //  }
        //}
      }
    }

    raycastHit2D = new RaycastHit2D();
    return raycastHit2D; // null
  }

  public static T GetHit2DComponent<T>(Vector2 pos, int? layerMask = null, float? distance = null) where T : UnityEngine.Component
  {
    var go = GetHit2DGameObject(pos, layerMask, distance);
    if (null == go)
      return default(T);

    return go.GetComponent<T>();
  }
  public static List<T> GetHit2DComponents<T>(Vector2 pos, int? layerMask = null, float? distance = null)
  {
    var result = new List<T>();
    var gos = GetHit2DGameObjects(pos, layerMask, distance);
    if (gos== null || gos.Count == 0)
      return null;

    gos = gos.OrderBy(go => go.transform.position.y).ToList();
    for (var i = 0; i < gos.Count; i++)
    {
      var comp = gos[i].GetComponent<T>();
      if(comp == null)
        continue;
      
      result.Add(comp);
    }

    return result;
  }
  public static T GetHit2DComponentToParent<T>(Vector2 pos, int? layerMask = null, float? distance = null) where T : UnityEngine.Component
  {
    var go = GetHit2DGameObject(pos, layerMask, distance);
    if (null == go)
      return default(T);

    return go.GetComponentInParent<T>();
  }

  public static GameObject GetHit2DGameObject(Vector2 pos, int? layerMask = null, float? distance = null)
  {
    Camera camera = null;
    if (null == camera)
    {
      camera = Camera.main;

      if (null == camera)
        return null;
    }

    var ray = camera.ScreenPointToRay(pos);
    var hitInfo2D = Raycast2D(ray, layerMask, distance);
    if (!hitInfo2D)
    {
      return null;
    }
    else
    {
      return hitInfo2D.collider.gameObject;
    }
  }
  public static List<GameObject> GetHit2DGameObjects(Vector2 pos, int? layerMask = null, float? distance = null)
  {
    Camera camera = null;
    if (null == camera)
    {
      camera = Camera.main;

      if (null == camera)
        return null;
    }


    var result = new List<GameObject>();
    var ray = camera.ScreenPointToRay(pos);
    var hitInfo2D = Raycast2DAll(ray, layerMask, distance);
    if (hitInfo2D == null || hitInfo2D.Length == 0)
    {
      return null;
    }
    else
    {
      for (var i = 0; i < hitInfo2D.Length; i++)
      {
        result.Add(hitInfo2D[i].collider.gameObject);
      }

      return result;
    }
  }
  
  public static bool IsTouched(this GameObject go, bool checkChild = false)
  {
    var eventData = new PointerEventData(EventSystem.current);
    eventData.position = Input.mousePosition;

    var results = new List<RaycastResult>();
    EventSystem.current.RaycastAll(eventData, results); 
    var graphics = go.GetComponentsInChildren<Graphic>(true);
    if (checkChild)
    {
      var childs = graphics.Select(graphic => graphic.gameObject).ToList();
      for (var i = 0; i < results.Count; i++)
      {
        if (childs.Contains(results[i].gameObject))
          return true;
      }
    }
    else
    {
      for (var i = 0; i < results.Count; i++)
      {
        if (results[i].gameObject == go)
          return true;
      }
    }
    
    return false;
  }

  #endregion

  #region Graphic Raycast
  
  public static RaycastResult GetRaycastResult()
  {
    var eventData = new PointerEventData(EventSystem.current);
    eventData.position = Input.mousePosition;

    var result = new List<RaycastResult>();
    EventSystem.current.RaycastAll(eventData, result);

    if (result == null || result.Count == 0)
      return default;
    
    return result[0];
  }
  
  public static List<RaycastResult> GetRaycastResults()
  {
    var eventData = new PointerEventData(EventSystem.current);
    eventData.position = Input.mousePosition;

    var result = new List<RaycastResult>();
    EventSystem.current.RaycastAll(eventData, result);

    return result;
  }

  public static T GetGraphicRayCastHitComponentInParent<T>(Vector2 pos) where T : Component
  {
    if (EventSystem.current.currentSelectedGameObject != null)
    {
      var component = EventSystem.current.currentSelectedGameObject.GetComponentInParent<T>();
      if (component == null)
        return null;
      
      if (component.GetType() == typeof(T))
        return component;
    }
    
    var pointerEventData = new PointerEventData(EventSystem.current) { position = pos };
    var results = new List<RaycastResult>();
    EventSystem.current.RaycastAll(pointerEventData, results);

    foreach (var result in results)
    {
      var component = result.gameObject.GetComponentInParent<T>();
      if(component == null)
        continue;
      
      if (component.GetType() == typeof(T))
        return component;
    }

    return null;
  }
  public static List<T> GetGraphicRayCastHitComponentsInParent<T>(Vector2 pos) where T : Component
  {
    List<T> list = null;
    if (EventSystem.current.currentSelectedGameObject != null)
    {
      list ??= new List<T>();
      var component = EventSystem.current.currentSelectedGameObject.GetComponentInParent<T>();
      if (null != component && !list.Contains(component))
        list.Add(component);
    }
    
    var pointerEventData = new PointerEventData(EventSystem.current) { position = pos };
    var results = new List<RaycastResult>();
    EventSystem.current.RaycastAll(pointerEventData, results);

    foreach (var result in results)
    {
      list ??= new List<T>();

      var component = result.gameObject.GetComponentInParent<T>();
      if (null != component && !list.Contains(component))
        list.Add(component);
    }

    return list;
  }
  public static List<T> GetGraphicRayCastHitComponents<T>(Vector2 pos) where T : UnityEngine.Component
  {
    var pointerEventData = new PointerEventData(null);
    pointerEventData.position = pos;

    GraphicRaycaster graphicRaycaster = null;
    var result = new List<RaycastResult>();
    for (var i = 0; i < (int)UICanvasTypes.Max; i++)
    {
      graphicRaycaster = UIManager.Instance.GetCanvasGraphicRaycaster((UICanvasTypes)i);
      graphicRaycaster.Raycast(pointerEventData, result);
    }

    List<T> list = null;
    for (var i = 0; i < result.Count; i++)
    {
      if (null == list)
        list = new List<T>();

      var component = result[i].gameObject.GetComponent<T>();
      if (null != component)
        list.Add(component);
    }

    return list;
  }
  public static T GetGraphicRayCastHitComponent<T>(Vector2 pos, string goName) where T : UnityEngine.Component
  {
    var ltT = GetGraphicRayCastHitComponents<T>(pos);

    T component = null;
    for (var i = 0; i < ltT.Count; i++)
    {
      if (ltT[i].gameObject.name == goName)
      {
        component = ltT[i];
        break;
      }
    }

    return component;
  }

  public static List<GameObject> GetGraphicRayCastHitGameObjects(Vector2 pos)
  {
    var pointerEventData = new PointerEventData(null);
    pointerEventData.position = pos;

    GraphicRaycaster graphicRaycaster = null;
    var result = new List<RaycastResult>();
    for (var i = 0; i < (int)UICanvasTypes.Max; i++)
    {
      graphicRaycaster = UIManager.Instance.GetCanvasGraphicRaycaster((UICanvasTypes)i);
      graphicRaycaster.Raycast(pointerEventData, result);
    }

    List<GameObject> list = null;
    for (var i = 0; i < result.Count; i++)
    {
      if (null == list)
        list = new List<GameObject>();

      list.Add(result[i].gameObject);
    }

    return list;
  }
  public static GameObject GetGraphicRayCastHitGameObject(Vector2 pos, string name)
  {
    var ltGameObject = GetGraphicRayCastHitGameObjects(pos);
    if (null == ltGameObject || ltGameObject.Count == 0)
      return null;

    GameObject go = null;
    for (var i = 0; i < ltGameObject.Count; i++)
    {
      if (ltGameObject[i].name == name)
      {
        go = ltGameObject[i];
        break;
      }
    }

    return go;
  }

  #endregion
}
