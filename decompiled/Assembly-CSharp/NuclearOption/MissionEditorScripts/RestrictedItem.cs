// Decompiled with JetBrains decompiler
// Type: NuclearOption.MissionEditorScripts.RestrictedItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.UI;

#nullable disable
namespace NuclearOption.MissionEditorScripts;

public class RestrictedItem : MonoBehaviour
{
  [SerializeField]
  private Text itemName;
  private RestrictionsTab restrictionsTab;
  private WeaponMount weaponMount;
  private UnitDefinition unitDefinition;

  public void SetItem(WeaponMount weaponMount, RestrictionsTab restrictionsTab)
  {
    this.weaponMount = weaponMount;
    this.restrictionsTab = restrictionsTab;
    this.itemName.text = weaponMount.mountName;
  }

  public void SetItem(UnitDefinition unitDefinition, RestrictionsTab restrictionsTab)
  {
    this.unitDefinition = unitDefinition;
    this.restrictionsTab = restrictionsTab;
    this.itemName.text = unitDefinition.unitName;
  }

  public void RemoveItem()
  {
    if ((Object) this.weaponMount != (Object) null)
      this.restrictionsTab.UnrestrictWeapon(this.weaponMount, this);
    if (!((Object) this.unitDefinition != (Object) null))
      return;
    this.restrictionsTab.UnrestrictAircraft(this.unitDefinition, this);
  }
}
