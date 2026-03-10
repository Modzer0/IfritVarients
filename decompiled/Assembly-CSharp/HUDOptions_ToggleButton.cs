// Decompiled with JetBrains decompiler
// Type: HUDOptions_ToggleButton
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#nullable disable
public class HUDOptions_ToggleButton : 
  MonoBehaviour,
  IPointerEnterHandler,
  IEventSystemHandler,
  IPointerClickHandler,
  IPointerExitHandler
{
  [SerializeField]
  private HUDOptions_Item item;
  [SerializeField]
  private Image image;
  [SerializeField]
  private Text label;
  public bool isActive = true;
  public bool status;
  public bool onFocus;
  [SerializeField]
  private Color colorIsOn = Color.green;
  [SerializeField]
  private Color colorIsOff = Color.grey;
  public List<UnitDefinition> listDefinitions = new List<UnitDefinition>();
  public List<int> prioritySettings = new List<int>();
  public List<bool> vehiclesSettings = new List<bool>();
  public List<bool> buildingsSettings = new List<bool>();

  public void Toggle()
  {
    if (this.status)
      this.Set(false);
    else
      this.Set(true);
  }

  public void Set(bool arg)
  {
    this.status = arg;
    this.SetColor();
    if (!this.status)
      return;
    SceneSingleton<HUDOptions>.i.ApplyToggleSettings(this);
  }

  public void OnPointerEnter(PointerEventData eventData) => this.onFocus = true;

  public void SetColor()
  {
    if ((Object) this.image != (Object) null)
      this.image.color = this.status ? this.colorIsOn : this.colorIsOff;
    if (!((Object) this.label != (Object) null))
      return;
    this.label.color = this.status ? this.colorIsOn : this.colorIsOff;
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    if (!this.onFocus)
      return;
    if (eventData.button == PointerEventData.InputButton.Left)
    {
      if (this.prioritySettings.Count > 0)
      {
        SceneSingleton<HUDOptions>.i.ToggleButtons(this);
      }
      else
      {
        if (this.listDefinitions.Count > 0)
          this.Toggle();
        else if ((Object) this.item != (Object) null)
          this.item.SetPriority(this.item.ButtonIndex(this));
        SceneSingleton<HUDOptions>.i.SaveValues();
      }
    }
    if (eventData.button != PointerEventData.InputButton.Right || this.listDefinitions.Count <= 0)
      return;
    SceneSingleton<HUDOptions>.i.ToggleButtons(this);
  }

  public void OnPointerExit(PointerEventData eventData) => this.onFocus = false;

  public bool CheckDefinition(UnitDefinition definition)
  {
    bool flag = false;
    if (this.listDefinitions.Count > 0)
    {
      for (int index = 0; index < this.listDefinitions.Count; ++index)
      {
        if ((Object) definition == (Object) this.listDefinitions[index])
          flag = true;
      }
    }
    return flag;
  }

  public void SaveValues()
  {
    if (!this.status || this.prioritySettings.Count <= 0 || this.vehiclesSettings.Count <= 0 || this.buildingsSettings.Count <= 0)
      return;
    for (int index = 0; index < SceneSingleton<HUDOptions>.i.listOptionItems.Count; ++index)
      this.prioritySettings[index] = Mathf.RoundToInt((float) (((double) SceneSingleton<HUDOptions>.i.listOptionItems[index].transparency - 0.75) * 8.0));
    for (int index = 0; index < SceneSingleton<HUDOptions>.i.listVehicleTypes.Count; ++index)
      this.vehiclesSettings[index] = SceneSingleton<HUDOptions>.i.listVehicleTypes[index].status;
    for (int index = 0; index < SceneSingleton<HUDOptions>.i.listBuildingTypes.Count; ++index)
      this.buildingsSettings[index] = SceneSingleton<HUDOptions>.i.listBuildingTypes[index].status;
  }
}
