// Decompiled with JetBrains decompiler
// Type: TargetListSelector
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class TargetListSelector : SceneSingleton<TargetListSelector>
{
  public MFDScreen screen;
  [SerializeField]
  private List<TargetListSelector_UnitItem> listItems = new List<TargetListSelector_UnitItem>();
  [SerializeField]
  private GameObject unitPrefab;
  [SerializeField]
  private Transform container;
  public TargetListSelector_ToggleButton toggleFollowHUD;
  public TargetListSelector_ToggleButton toggleLaser;
  public List<TargetListSelector_ToggleButton> toggleFactionItems;
  public List<TargetListSelector_ToggleButton> toggleUnitTypesItems;
  public List<TargetListSelector_ToggleButton> toggleVehicleTypesItems;
  private float lastRefresh;
  private float refreshDelay = 1f;
  private bool needUpdateIcon;

  private void Start()
  {
    this.toggleFollowHUD.Set(false);
    this.toggleLaser.Set(false);
    SceneSingleton<DynamicMap>.i.onUnitSelected += new Action<Unit>(this.AddItem);
    SceneSingleton<DynamicMap>.i.onUnitDeselected += new Action<Unit>(this.RemoveItem);
    SceneSingleton<DynamicMap>.i.onAllDeselected += new Action(this.RemoveAll);
    SceneSingleton<HUDOptions>.i.OnApplyOptions += new Action(this.SetFilters);
    this.toggleFollowHUD.OnToggle += new Action(this.OnToggleFollowHUD);
    this.needUpdateIcon = true;
  }

  private void OnDestroy()
  {
    if ((UnityEngine.Object) SceneSingleton<DynamicMap>.i != (UnityEngine.Object) null)
    {
      SceneSingleton<DynamicMap>.i.onUnitSelected -= new Action<Unit>(this.AddItem);
      SceneSingleton<DynamicMap>.i.onUnitDeselected -= new Action<Unit>(this.RemoveItem);
      SceneSingleton<DynamicMap>.i.onAllDeselected -= new Action(this.RemoveAll);
    }
    if (!((UnityEngine.Object) SceneSingleton<HUDOptions>.i != (UnityEngine.Object) null))
      return;
    SceneSingleton<HUDOptions>.i.OnApplyOptions -= new Action(this.SetFilters);
  }

  private void Update()
  {
    if (!this.needUpdateIcon || (double) Time.timeSinceLevelLoad <= (double) this.lastRefresh + (double) this.refreshDelay)
      return;
    if ((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft == (UnityEngine.Object) null && this.toggleFollowHUD.status)
      this.toggleFollowHUD.Set(false);
    this.lastRefresh = Time.timeSinceLevelLoad;
    this.CheckAllExclusions();
    this.needUpdateIcon = false;
  }

  public void AddItem(Unit u)
  {
    if (!((UnityEngine.Object) this.listItems.Find((Predicate<TargetListSelector_UnitItem>) (item => (UnityEngine.Object) item.unit == (UnityEngine.Object) u)) == (UnityEngine.Object) null))
      return;
    TargetListSelector_UnitItem component = UnityEngine.Object.Instantiate<GameObject>(this.unitPrefab, this.container).GetComponent<TargetListSelector_UnitItem>();
    component.SetUnit(u);
    component.selector = this;
    u.onDisableUnit += new Action<Unit>(this.RemoveItem);
    this.listItems.Add(component);
  }

  public void RemoveItem(Unit u)
  {
    if (!((UnityEngine.Object) this.listItems.Find((Predicate<TargetListSelector_UnitItem>) (item => (UnityEngine.Object) item.unit == (UnityEngine.Object) u)) != (UnityEngine.Object) null))
      return;
    TargetListSelector_UnitItem selectorUnitItem = this.listItems.Find((Predicate<TargetListSelector_UnitItem>) (item => (UnityEngine.Object) item.unit == (UnityEngine.Object) u));
    this.listItems.Remove(selectorUnitItem);
    u.onDisableUnit -= new Action<Unit>(this.RemoveItem);
    UnityEngine.Object.Destroy((UnityEngine.Object) selectorUnitItem.gameObject);
  }

  public void RemoveAll()
  {
    this.listItems.Clear();
    if (this.container.childCount <= 0)
      return;
    foreach (Component component in this.container)
      UnityEngine.Object.Destroy((UnityEngine.Object) component.gameObject);
  }

  public void CheckAllExclusions()
  {
    List<Unit> unitList = new List<Unit>();
    foreach (MapIcon selectedIcon in SceneSingleton<DynamicMap>.i.selectedIcons)
    {
      if (selectedIcon is UnitMapIcon unitMapIcon && this.CheckExclusions(unitMapIcon.unit))
        unitList.Add(unitMapIcon.unit);
    }
    foreach (Unit unit in unitList)
      this.ForceDeselect(unit);
    unitList.Clear();
    foreach (MapIcon mapIcon in SceneSingleton<DynamicMap>.i.mapIcons)
    {
      if (mapIcon is UnitMapIcon unitMapIcon)
        unitMapIcon.UnitMapIcon_UpdateColor();
    }
  }

  public void ForceDeselect(Unit unit)
  {
    if ((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft != (UnityEngine.Object) null)
      SceneSingleton<CombatHUD>.i.DeSelectUnit(unit);
    else
      SceneSingleton<DynamicMap>.i.DeselectIcon(unit);
  }

  public bool CheckExclusions(Unit u)
  {
    for (int index = 0; index < this.toggleFactionItems.Count; ++index)
    {
      if (this.toggleFactionItems[index].CheckFactions(u))
        return true;
    }
    for (int index = 0; index < this.toggleUnitTypesItems.Count; ++index)
    {
      if (this.toggleUnitTypesItems[index].CheckUnitTypes(u))
        return true;
    }
    for (int index = 0; index < this.toggleVehicleTypesItems.Count; ++index)
    {
      if (this.toggleVehicleTypesItems[index].CheckDefinitions(u))
        return true;
    }
    return this.toggleLaser.status && (UnityEngine.Object) SceneSingleton<DynamicMap>.i.HQ != (UnityEngine.Object) null && !SceneSingleton<DynamicMap>.i.HQ.IsTargetLased(u);
  }

  public void SetOnlyItem(TargetListSelector_ToggleButton item)
  {
    if (this.toggleFactionItems.Contains(item))
    {
      foreach (TargetListSelector_ToggleButton toggleFactionItem in this.toggleFactionItems)
        toggleFactionItem.Set(false);
    }
    else if (this.toggleUnitTypesItems.Contains(item))
    {
      foreach (TargetListSelector_ToggleButton toggleUnitTypesItem in this.toggleUnitTypesItems)
        toggleUnitTypesItem.Set(false);
    }
    else if (this.toggleVehicleTypesItems.Contains(item))
    {
      foreach (TargetListSelector_ToggleButton vehicleTypesItem in this.toggleVehicleTypesItems)
        vehicleTypesItem.Set(false);
      foreach (TargetListSelector_ToggleButton toggleUnitTypesItem in this.toggleUnitTypesItems)
        toggleUnitTypesItem.Set(item.listDefinitions[0].GetType() == toggleUnitTypesItem.listUnitTypes[0].GetType());
    }
    item.Set(true);
  }

  public void DeselectAll()
  {
    if ((UnityEngine.Object) SceneSingleton<CombatHUD>.i.aircraft != (UnityEngine.Object) null)
      SceneSingleton<CombatHUD>.i.DeselectAll();
    else
      SceneSingleton<DynamicMap>.i.UnselectAll();
  }

  public void ResetFilters()
  {
    this.toggleLaser.Set(false);
    foreach (TargetListSelector_ToggleButton toggleFactionItem in this.toggleFactionItems)
      toggleFactionItem.Set(true);
    foreach (TargetListSelector_ToggleButton toggleUnitTypesItem in this.toggleUnitTypesItems)
      toggleUnitTypesItem.Set(true);
    foreach (TargetListSelector_ToggleButton vehicleTypesItem in this.toggleVehicleTypesItems)
      vehicleTypesItem.Set(true);
  }

  public void SetFilters()
  {
    if (!this.toggleFollowHUD.status)
      return;
    HUDOptions.HUDMode currentMode = SceneSingleton<HUDOptions>.i.currentMode;
    List<int> prioritySettings = SceneSingleton<HUDOptions>.i.listModes[(int) currentMode].prioritySettings;
    List<bool> vehiclesSettings = SceneSingleton<HUDOptions>.i.listModes[(int) currentMode].vehiclesSettings;
    for (int index = 0; index < prioritySettings.Count; ++index)
    {
      bool flag = prioritySettings[index] > 0;
      if (index < 2)
        this.toggleFactionItems[index].Set(flag);
      else
        this.toggleUnitTypesItems[index - 2].Set(flag);
    }
    for (int index = 0; index < vehiclesSettings.Count; ++index)
    {
      bool flag = vehiclesSettings[index];
      this.toggleVehicleTypesItems[index].Set(flag);
    }
  }

  public void OnToggleFollowHUD()
  {
    if (this.toggleFollowHUD.status)
      this.SetFilters();
    else
      this.ResetFilters();
  }

  public void NeedUpdateIcons() => this.needUpdateIcon = true;
}
