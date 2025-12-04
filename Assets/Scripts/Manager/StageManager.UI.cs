using FAIRSTUDIOS.UI;
using TMPro;
using UnityEngine;

public partial class StageManager
{
  [Header("[UI]"), SerializeField] private KProgressBar expProgressBar;

  [SerializeField] private KToggle toogleVibe;
  [SerializeField] private KToggle toogleStageMode;

  [SerializeField] private TextMeshProUGUI text;

  [SerializeField] private Animator stageCompleteAnimator;
  [SerializeField] private float stageCompleteWaitTime = 1f;

  private void SetUI()
  {
    toogleVibe.Set(SOManager.Instance.PlayerPrefsModel.VibrationEnabled, false);
    toogleStageMode.Set(GameModel.Global.InfinityMode, false);
  }

  public void SetText(string value)
  {
    text.text = value;
  }

  public void OnToggleVibration(bool value)
  {
    // Vibration.Vibrate(100);
    SOManager.Instance.PlayerPrefsModel.VibrationEnabled = value;
  }

  public void OnToggleModeChange(bool value)
  {
    GameModel.Global.InfinityMode = value;
    StartStage(value);
  }
}
