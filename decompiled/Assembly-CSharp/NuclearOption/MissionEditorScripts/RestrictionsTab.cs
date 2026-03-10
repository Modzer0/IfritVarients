// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.RestrictionsTab
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class RestrictionsTab : MonoBehaviour, IMissionTab
{
  [SerializeField]
  private Dropdown factionDropdown;
  [SerializeField]
  private Dropdown weaponDropdown;
  [SerializeField]
  private Dropdown aircraftDropdown;
  [SerializeField]
  private GameObject restrictedItemPrefab;
  private MissionFaction selectedFaction;
  [SerializeField]
  private Transform restrictedWeaponsTransform;
  [SerializeField]
  private Transform restrictedAircraftTransform;
  private List<RestrictedItem> displayedRestrictions = new List<RestrictedItem>();
  private Dictionary<string, UnitDefinition> aircraftNameLookup = new Dictionary<string, UnitDefinition>();

  public void SetMission(Mission mission)
  {
    this.factionDropdown.options.Clear();
    foreach (MissionFaction faction in MissionManager.CurrentMission.factions)
      this.factionDropdown.options.Add(new Dropdown.OptionData(faction.factionName));
    this.aircraftNameLookup.Clear();
    foreach (AircraftDefinition aircraftDefinition in Encyclopedia.i.aircraft)
      this.aircraftNameLookup.Add(aircraftDefinition.unitName, (UnitDefinition) aircraftDefinition);
    this.factionDropdown.SetValueWithoutNotify(0);
    this.SelectFaction();
  }

  public void SelectFaction()
  {
    string text = this.factionDropdown.options[this.factionDropdown.value].text;
    MissionFaction faction;
    if (MissionManager.CurrentMission.TryGetFaction(text, out faction))
      this.selectedFaction = faction;
    else
      Debug.LogWarning((object) ("Failed to find faction with name: " + text));
    this.RefreshDisplay();
  }

  private void RefreshDisplay()
  {
    foreach (RestrictedItem displayedRestriction in this.displayedRestrictions)
    {
      if ((Object) displayedRestriction != (Object) null)
        Object.Destroy((Object) displayedRestriction.gameObject);
    }
    this.displayedRestrictions.Clear();
    this.weaponDropdown.ClearOptions();
    this.aircraftDropdown.ClearOptions();
    foreach (WeaponMount weaponMount in Encyclopedia.i.weaponMounts)
    {
      if (this.selectedFaction.restrictions.weapons.Contains(weaponMount.name))
      {
        RestrictedItem component = Object.Instantiate<GameObject>(this.restrictedItemPrefab, this.restrictedWeaponsTransform).GetComponent<RestrictedItem>();
        component.SetItem(weaponMount, this);
        this.displayedRestrictions.Add(component);
      }
      else if (!weaponMount.disabled)
        this.weaponDropdown.options.Add(new Dropdown.OptionData(weaponMount.name));
    }
    foreach (AircraftDefinition aircraftDefinition in Encyclopedia.i.aircraft)
    {
      if (this.selectedFaction.restrictions.aircraft.Contains(aircraftDefinition.unitPrefab.name))
      {
        RestrictedItem component = Object.Instantiate<GameObject>(this.restrictedItemPrefab, this.restrictedAircraftTransform).GetComponent<RestrictedItem>();
        component.SetItem((UnitDefinition) aircraftDefinition, this);
        this.displayedRestrictions.Add(component);
      }
      else if (!aircraftDefinition.disabled)
        this.aircraftDropdown.options.Add(new Dropdown.OptionData(aircraftDefinition.unitName));
    }
    this.weaponDropdown.SetValueWithoutNotify(0);
    this.weaponDropdown.RefreshShownValue();
    this.aircraftDropdown.SetValueWithoutNotify(0);
    this.aircraftDropdown.RefreshShownValue();
  }

  public void RestrictWeapon()
  {
    if (this.weaponDropdown.options.Count > 0)
      this.selectedFaction.restrictions.weapons.Add(this.weaponDropdown.options[this.weaponDropdown.value].text);
    this.RefreshDisplay();
  }

  public void RestrictAircraft()
  {
    if (this.aircraftDropdown.options.Count > 0)
      this.selectedFaction.restrictions.aircraft.Add(this.aircraftNameLookup[this.aircraftDropdown.options[this.aircraftDropdown.value].text].unitPrefab.name);
    this.RefreshDisplay();
  }

  public void UnrestrictWeapon(WeaponMount weaponMount, RestrictedItem restrictedItem)
  {
    Object.Destroy((Object) restrictedItem);
    this.selectedFaction.restrictions.weapons.Remove(weaponMount.name);
    this.RefreshDisplay();
  }

  public void UnrestrictAircraft(UnitDefinition unitDefinition, RestrictedItem restrictedItem)
  {
    Object.Destroy((Object) restrictedItem);
    this.selectedFaction.restrictions.aircraft.Remove(unitDefinition.unitPrefab.name);
    this.RefreshDisplay();
  }
}
