#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.DeviceSimulation;
using UnityEngine.UIElements;

public class DeviceSimulatorUtility : DeviceSimulatorPlugin
{
  public override string title => "Utility";
  // private Label m_TouchCountLabel;
  // private Label m_LastTouchEvent;
  // private Button m_ResetCountButton;

  private Label m_CurrentOrientation;
  private Label m_SafeArea;
  private Label m_Resolution;
  private Button m_RefreshScreenInfoButton;
  private Button m_ApplySafeAreaButton;

  [SerializeField]
  //private int m_TouchCount = 0;

  public override void OnCreate()
  {
  }

  public override VisualElement OnCreateUI()
  {
    VisualElement root = new VisualElement();

    //m_LastTouchEvent = new Label("Last touch event: None");
    //m_TouchCountLabel = new Label();

    m_CurrentOrientation = new Label("current orientation");
    m_SafeArea = new Label("safe area");
    m_Resolution = new Label("resolution");
    m_RefreshScreenInfoButton = new Button { text = "Refresh Screen Info" };
    m_RefreshScreenInfoButton.clicked += () =>
    {
      m_CurrentOrientation.text = $"current orientation : \n{UnityEngine.Device.Screen.orientation}";
      m_SafeArea.text = $"safe area : \n{UnityEngine.Device.Screen.safeArea}";
      m_Resolution.text = $"resolution : \n{UnityEngine.Device.Screen.currentResolution}";
    };

    m_ApplySafeAreaButton = new Button { text = "Apply SafeArea" };
    m_ApplySafeAreaButton.clicked += () =>
    {
      if (Application.isPlaying == false) return;

      SOManager.Instance.DeviceInfoModel.OnScreenInfoChanged();
    };
    root.Add(m_CurrentOrientation);
    root.Add(m_SafeArea);
    root.Add(m_Resolution);
    root.Add(m_RefreshScreenInfoButton);
    root.Add(m_ApplySafeAreaButton);

    return root;
  }
}

#endif