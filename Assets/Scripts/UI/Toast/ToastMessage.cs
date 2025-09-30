using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using FAIRSTUDIOS.Tools;

public class ToastMessage : MonoBehaviour
{
  [SerializeField] private Text message;
  [SerializeField] private DOTweenSequencer sequencer;

  private void Start()
  {
    var rectTransform = transform as RectTransform;
    rectTransform.anchoredPosition = Vector2.zero;
  }
  
  public void Show(string msg)
  {
    message.text = Localize.GetValue(msg);
    sequencer.Play("Show", alwaysRestart: true);
    
    //if (eSound != ESoundEffect.NONE)
    //{
    //  SoundManager.Play(eSound);
    //}
  }
}
