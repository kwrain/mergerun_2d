using System.Threading.Tasks;
using FAIRSTUDIOS.UI;
using UnityEngine;

public partial class BaseObject
{
  [SerializeField, HideInInspector] private HUDBehaviour hudBehaviour;

  public virtual HUDBehaviour HUDBehaviour
  {
    get => hudBehaviour;
    set => hudBehaviour = value;
  }

  public void UpdateHUD()
  {
    if (UIManager.Instance.CurrUI == null || !UIManager.Instance.CurrUI.Attribute.UseHUD)
    {
      // UIManager.Instance.AddPauseHUD<HUDBuilding>(this);
      return;
    }

    UpdateHUD(null);

    if (hudBehaviour != null)
    {
      hudBehaviour.RaycastTarget = true;
    }

    // Debug.LogError($"Name : {gameObject.name} / HUD : {hudBehaviour == null}");
  }

  protected virtual async Task UpdateHUD(params object[] args) { }
}
