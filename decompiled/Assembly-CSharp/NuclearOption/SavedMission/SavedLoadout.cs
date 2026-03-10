// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.SavedLoadout
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace NuclearOption.SavedMission;

[Serializable]
public class SavedLoadout
{
  public List<SavedLoadout.SelectedMount> Selected = new List<SavedLoadout.SelectedMount>();

  public SavedLoadout() => this.Selected = new List<SavedLoadout.SelectedMount>();

  public Loadout CreateLoadout(GameObject prefab)
  {
    return this.CreateLoadout(prefab.GetComponent<Aircraft>().weaponManager);
  }

  public Loadout CreateLoadout(WeaponManager weaponManager)
  {
    Loadout loadout = new Loadout();
    for (int index = 0; index < this.Selected.Count && index < weaponManager.hardpointSets.Length; ++index)
    {
      HardpointSet hardpointSet = weaponManager.hardpointSets[index];
      WeaponMount weaponMount = this.Selected[index].GetWeaponMount(hardpointSet);
      loadout.weapons.Add(weaponMount);
    }
    return loadout;
  }

  [Serializable]
  public struct SelectedMount : IEquatable<SavedLoadout.SelectedMount>
  {
    public string Key;

    public bool Equals(SavedLoadout.SelectedMount other) => this.Key == other.Key;

    public readonly WeaponMount GetWeaponMount(HardpointSet hardpointSet)
    {
      if (string.IsNullOrEmpty(this.Key))
        return (WeaponMount) null;
      List<WeaponMount> weaponOptions = hardpointSet.weaponOptions;
      if (weaponOptions == null || weaponOptions.Count <= 0)
        return (WeaponMount) null;
      for (int index = 1; index < weaponOptions.Count; ++index)
      {
        WeaponMount weaponMount = weaponOptions[index];
        if (weaponMount.jsonKey == this.Key)
          return weaponMount;
      }
      Debug.LogWarning((object) $"Could not find {this.Key} in HardpointSet:{hardpointSet.name}.");
      WeaponMount weaponMount1;
      if (Encyclopedia.WeaponLookup.TryGetValue(this.Key, out weaponMount1))
        return weaponMount1;
      Debug.LogError((object) $"Could not find {this.Key} in Encyclopedia or HardpointSet:{hardpointSet.name}");
      return (WeaponMount) null;
    }
  }
}
