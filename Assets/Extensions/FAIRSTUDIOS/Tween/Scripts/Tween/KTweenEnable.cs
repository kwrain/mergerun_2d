using UnityEngine;

/// <summary>
/// tween 을 이용하여 자식의 게임 오브젝트 활성화 시켜준다.
/// 활성화 비활성화 토클하기 위해 만든 스크립트
/// </summary>
namespace FAIRSTUDIOS.Tools
{
	public class KTweenEnable : KTweener
  {
    bool bValue = true;

    [SerializeField]
    bool bChildEnable = true;   //component disable 될때 차일드 Gameobject enble 여부

    protected override void OnDisable()
    {
      base.OnEnable();

      gameObject.SetActiveInChildren(bChildEnable);
    }

    protected override void OnUpdate (float factor, bool isFinished)
    {
      ValueUpdate(factor > 0.5f, isFinished);
    }

    virtual protected void ValueUpdate(bool val, bool isFinished)
    {
      if( bValue == val )
        return;
      bValue = val;

      gameObject.SetActiveInChildren(bValue);
    }
  }
}
