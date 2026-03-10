// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.InventoryItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class InventoryItem : MonoBehaviour
{
  [SerializeField]
  private Text unitName;
  private IReadOnlyList<MissionFaction> selectedFactions;
  private int supplyIndex;
  private UnitDefinition definition;
  private FactionSettingsTab factionSettingsTab;
  private int count;

  public void SetInventoryItem(
    int supplyIndex,
    FactionSupply factionSupply,
    IReadOnlyList<MissionFaction> selectedFactions,
    FactionSettingsTab factionSettingsTab)
  {
    this.selectedFactions = selectedFactions;
    this.factionSettingsTab = factionSettingsTab;
    this.supplyIndex = supplyIndex;
    this.count = factionSupply.count;
    this.definition = Encyclopedia.Lookup[factionSupply.unitType];
    this.RefreshDisplay();
  }

  public void RefreshDisplay() => this.unitName.text = $"{this.definition.unitName}[{this.count}]";

  public void AddOne()
  {
    ++this.count;
    foreach (MissionFaction selectedFaction in (IEnumerable<MissionFaction>) this.selectedFactions)
      selectedFaction.supplies[this.supplyIndex].count = this.count;
    this.RefreshDisplay();
  }

  public void RemoveOne()
  {
    --this.count;
    foreach (MissionFaction selectedFaction in (IEnumerable<MissionFaction>) this.selectedFactions)
      selectedFaction.supplies[this.supplyIndex].count = this.count;
    this.RefreshDisplay();
    if (this.count > 0)
      return;
    bool flag = false;
    foreach (MissionFaction selectedFaction in (IEnumerable<MissionFaction>) this.selectedFactions)
    {
      foreach (FactionSupply supply in selectedFaction.supplies)
      {
        if (supply.unitType == this.definition.unitPrefab.name)
        {
          selectedFaction.supplies.Remove(supply);
          flag = true;
          break;
        }
      }
    }
    if (!flag)
      return;
    Object.Destroy((Object) this.gameObject);
    this.factionSettingsTab.RemoveSupplyEntry(this);
  }
}
