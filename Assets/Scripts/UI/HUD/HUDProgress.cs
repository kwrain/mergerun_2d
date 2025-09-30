using System.Collections;
using System.Collections.Generic;
using FAIRSTUDIOS.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[HUD(Type = typeof(HUDProgress), PrefabPath = "HUDProgress")]
public class HUDProgress : HUDBehaviour
{
  public KProgressBar progressBar;
  public Text txtDescription;

  public KButton btnCancel;

  public void SetData(string des, float duration, bool showCancelButton)
  {
    txtDescription.text = des;
    btnCancel.SetActive(showCancelButton);
    progressBar.AutoProgress(0, 1, duration);
    progressBar.AddEndEvent(() => { Hide(); });
  }

  public void AddCancelEvent(UnityAction unityAction)
  {
    btnCancel.onClick.AddListener(unityAction);
  }

  public void AddStartEvent(UnityAction unityAction)
  {
    progressBar.AddStartEvent(() => { Hide(); });
  }
  public void AddUpdateEvent(UnityAction<float> unityAction)
  {
    progressBar.AddUpdateEvent(unityAction);
  }
  public void AddEndEvent(UnityAction unityAction)
  {
    progressBar.AddEndEvent(unityAction);
  }
}