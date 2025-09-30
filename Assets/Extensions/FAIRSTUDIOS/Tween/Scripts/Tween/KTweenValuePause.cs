namespace FAIRSTUDIOS.Tools
{
  /// <summary>
  /// KTweenValue 에서 Pause 를 사용하기위해 OnDisable 에서 bForceStart 변수를 셋팅하지 않기 위해 만든 클래스
  /// </summary>
  public class KTweenValuePause : KTweenValue
  {
    public override float value
    {
      get { return base.value; }
      set
      { 
        base.value = value;
        Factor = value;
      }
    }
  }
}
