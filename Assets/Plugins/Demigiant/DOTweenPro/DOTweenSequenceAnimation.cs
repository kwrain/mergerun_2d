using UnityEngine;

/**
* DOTweenSequenceAnimation.cs
* 작성자 : dev@fairstudios.kr
* 작성일 : 2023년 01월 10일 오후 3시 31분
*/

namespace DG.Tweening
{
  [AddComponentMenu("DOTween/DOTween Sequence Animation")]
  public class DOTweenSequenceAnimation : DOTweenAnimationExtended
  {
    protected override bool Independent => false;
  }
}