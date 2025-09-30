using System;
using System.Collections.Generic;
using FAIRSTUDIOS.SODB.Core;
using FAIRSTUDIOS.SODB.Property;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 디바이스 관련된 정보나, 옵션 정보를 멤버로 갖는다.
/// </summary>
[CreateAssetMenu(fileName = "GameModel", menuName = "Model/GameModel")]
public class GameModel : ModelBase
{
  // Default
  [SerializeField] protected PropertyString deviceToken;

  [SerializeField] protected PropertyString gameVersion;
  [SerializeField] protected PropertyString currentVersion;

  [SerializeField] protected PropertyString marketType;

  // Refresh
  [SerializeField] protected PropertyBoolean refresh;

  // TODO : rename to onUnloadIslandScene
  [NonSerialized] private Dictionary<object, Dictionary<string, Action>> onLogOutSucceeded;

  public static GameModel Global
  {
    get
    {
      if (SOManager.IsCreated == false) return null;
      if (!SOManager.Instance.GameModel.isInit)
        SOManager.Instance.GameModel.Init();
      return SOManager.Instance.GameModel;
    }
  }

  public string DeviceToken
  {
    get => deviceToken.RuntimeValue;
    set => deviceToken.RuntimeValue = value;
  }

  public string CountryCode => "ko_KR";

  public override void OnApplicationPauseModel(bool pauseStatus)
  {
    base.OnApplicationPauseModel(pauseStatus);

    if (pauseStatus)
    {

    }
    else
    {
      if (ElapsedTimeFromPause > 7200)
      {
      }
    }
  }

  public void Reset()
  {
  }

  public void ResetLoginInfo()
  {
  }

  /// <summary>
  /// SOManager.Instance.AddOnApplicationQuitCallback Wrapper
  /// </summary>
  /// <param name="target"></param>
  /// <param name="context"></param>
  /// <param name="callback"></param>
  public void AddOnApplicationQuitPrefsCallback(object target, string context, Action<PlayerPrefsModel> callback)
  {
    // SOManager.Instance.AddOnApplicationQuitCallback(target, context, () => callback(playerPrefsModel));
  }

  /// <summary>
  /// 로그아웃 성공 시 호출되는 콜벡을 추가하는 함수 <br/>
  /// 예시 1 (익명 함수) : GameModel.Global.AddOnLogOutCallback(this, "Clear List", () => myValue = 0;); <br/>
  /// 예시 2 (멤버 함수) : GameModel.Global.AddOnLogOutCallback(this, "Clear List", OnLogout); <br/>
  /// 콜벡 삭제는 옵션으로 <seealso cref="RemoveOnLogOutCallback">를 사용하면됨.</seealso> <br/>
  /// </summary>
  /// <param name="target">콜벡을 가지고 있는 대상</param>
  /// <param name="context">콜백에 대한 컨텍스트</param>
  /// <param name="func">콜벡</param>
  // TODO : rename to AddOnUnloadIslandScene
  public void AddOnLogOutCallback(object target, string context, System.Action func)
  {
    onLogOutSucceeded ??= new();
    if(onLogOutSucceeded.ContainsKey(target) == false)
    {
      onLogOutSucceeded.Add(target, new());
    }
    if(onLogOutSucceeded[target].ContainsKey(context) == false)
    {
      onLogOutSucceeded[target].Add(context, func);
    }
  }

  /// <summary>
  /// 로그아웃 성공 시 호출되는 콜벡을 제거하는 함수<br/>
  /// 이전에 추가된 콜벡을 제거하는 것으로, 특별한 경우가 아니면 삭제할일 없음.<br/>
  /// </summary>
  /// <param name="target"></param>
  /// <param name="context"></param>
  // TODO : rename to RemoveOnUnloadIslandScene
  public void RemoveOnLogOutCallback(object target, string context)
  {
    if(onLogOutSucceeded == null)
      return;
    if(onLogOutSucceeded.ContainsKey(target) == false)
      return;
    if(onLogOutSucceeded[target].ContainsKey(context) == false)
      return;
    onLogOutSucceeded[target].Remove(context);
  }

  public void OnSceneChanged(Scene currScene)
  {
    // // 현재 씬이 Island였다가 다른 씬으로 넘어갈 때.
    // if(currScene.name == "Island")
    // {
    //   if (onLogOutSucceeded != null)
    //   {
    //     foreach (var kv in onLogOutSucceeded)
    //     {
    //       foreach (var kv2 in kv.Value)
    //       {
    //         kv2.Value?.Invoke();
    //       }
    //     }
    //     onLogOutSucceeded.Clear();
    //   }

    //   Reset();
    // }
  }
}