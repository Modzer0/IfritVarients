// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.Loadout
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace NuclearOption.SavedMission;

[Serializable]
public class Loadout
{
  public List<WeaponMount> weapons = new List<WeaponMount>();

  public bool AllowedByHQ(WeaponManager weaponManager, FactionHQ hq)
  {
    int num = 0;
    int warheadAvailableForAi = hq.GetWarheadAvailableForAI();
    for (int index = 0; index < weaponManager.hardpointSets.Length; ++index)
    {
      HardpointSet hardpointSet = weaponManager.hardpointSets[index];
      if (index >= this.weapons.Count)
        return false;
      WeaponMount weapon = this.weapons[index];
      if (!((UnityEngine.Object) weapon == (UnityEngine.Object) null) && !((UnityEngine.Object) weapon.info == (UnityEngine.Object) null))
      {
        if (hq.restrictedWeapons.Contains(weapon.name))
          return false;
        if (weapon.info.nuclear)
        {
          if (!MissionManager.AllowTactical() || weapon.info.strategic && !MissionManager.AllowStrategic())
            return false;
          num += weapon.ammo * hardpointSet.hardpoints.Count;
        }
      }
    }
    return num == 0 || num <= warheadAvailableForAi;
  }
}
