using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace FAIRSTUDIOS.UI
{
  [AddComponentMenu("K.UI/Toggle Group")]
  [DisallowMultipleComponent]
  public class KToggleGroup : UnityEngine.EventSystems.UIBehaviour
  {
    [SerializeField]
    private bool m_AllowSwitchOff = false;
    public bool AllowSwitchOff { get { return m_AllowSwitchOff; } set { m_AllowSwitchOff = value; } }

    [SerializeField] private List<KToggle> m_Toggles = new List<KToggle>();

    [SerializeField] private bool DontUnregistToggles = false;
    public KToggle this[int index]
    {
      get
      {
        if (m_Toggles.Count == 0 || m_Toggles[index] == null) return null;
        return m_Toggles[index];
      }
    }

    [Serializable]
    public class ToggleGroupEvent : UnityEvent<bool>
    { }

    public ToggleGroupEvent onToggleGroupChanged = new ToggleGroupEvent();
    public ToggleGroupEvent onToggleGroupToggleChanged = new ToggleGroupEvent();

    public KToggle SelectedToggle { get; private set; }

    public KToggle ActiveToggle
    {
      get
      {
        foreach (var toggle in m_Toggles)
        {
          if (toggle.isOn)
            return toggle;
        }

        return null;
      }
    }

    protected KToggleGroup()
    { }

    private bool ValidateToggleIsInGroup(KToggle toggle)
    {
      if (toggle == null || !m_Toggles.Contains(toggle))
      {
        throw new ArgumentException(string.Format("Toggle {0} is not part of ToggleGroup {1}", new object[] { toggle, this }));
      }

      return true;
    }

    public void NotifyToggleOn(KToggle toggle, bool bTransition = true)
    {
      if (!ValidateToggleIsInGroup(toggle))
        return;

      // disable all toggles in the group
      SelectedToggle = toggle;
      for (var i = 0; i < m_Toggles.Count; i++)
      {
        var lastTransition = m_Toggles[i].toggleTransition;
        m_Toggles[i].toggleTransition = bTransition ? m_Toggles[i].toggleTransition : KToggle.ToggleTransition.None;
        if (m_Toggles[i] == toggle)
        {
          m_Toggles[i].toggleTransition = lastTransition;
          continue;
        }

        m_Toggles[i].Set(false, true, true, true);
        m_Toggles[i].toggleTransition = lastTransition;
      }
      onToggleGroupChanged.Invoke(AnyTogglesOn());
    }

    public void UnregisterToggle(KToggle toggle)
    {
      if(Application.isPlaying == true)
      {
        if (DontUnregistToggles == true) return;
      }

      if (m_Toggles == null)
        return;

      if (m_Toggles.Contains(toggle))
      {
        m_Toggles.Remove(toggle);
        toggle.onValueChanged.RemoveListener(NotifyToggleChanged);
      }
    }

    private void NotifyToggleChanged(bool isOn)
    {
      onToggleGroupToggleChanged.Invoke(isOn);
    }

    public void RegisterToggle(KToggle toggle)
    {
      if (!m_Toggles.Contains(toggle))
      {
        m_Toggles.Add(toggle);
        toggle.onValueChanged.AddListener(NotifyToggleChanged);
      }
    }

    public bool AnyTogglesOn()
    {
      return m_Toggles.Find(x => x.isOn) != null;
    }

    public IEnumerable<KToggle> ActiveToggles()
    {
      return m_Toggles.Where(x => x.isOn);
    }

    public void SetAllTogglesOff()
    {
      bool oldAllowSwitchOff = m_AllowSwitchOff;
      m_AllowSwitchOff = true;

      for (var i = 0; i < m_Toggles.Count; i++)
        m_Toggles[i].isOn = false;

      m_AllowSwitchOff = oldAllowSwitchOff;
    }

    public void HasTheGroupToggle(bool value)
    {
      Debug.Log("Testing, the group has toggled [" + value + "]");
    }

    public void HasAToggleFlipped(bool value)
    {
      Debug.Log("Testing, a toggle has toggled [" + value + "]");
    }
  }
}