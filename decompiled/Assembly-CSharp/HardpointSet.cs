// Decompiled with JetBrains decompiler
// Type: HardpointSet
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using JamesFrowen.ScriptableVariables;
using NuclearOption.SavedMission;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

#nullable disable
[Serializable]
public class HardpointSet
{
  [Header("Config")]
  [FormerlySerializedAs("hardpointName")]
  public string name;
  [Tooltip("List of HardpointIndexes. if other hardpoint has mount, then this must have no mount")]
  public List<byte> precludingHardpointSets;
  [Tooltip("What mounts to show as options. Note: other mounts can still be loaded by customizing the mission json")]
  public List<WeaponMount> weaponOptions = new List<WeaponMount>();
  [Header("References")]
  [Tooltip("Runtime mount")]
  [ReadOnly]
  [HideInInspector]
  public WeaponMount weaponMount;
  public List<Hardpoint> hardpoints = new List<Hardpoint>();

  public void SpawnMounts(Aircraft aircraft, WeaponMount weaponMount)
  {
    this.weaponMount = weaponMount;
    foreach (Hardpoint hardpoint in this.hardpoints)
    {
      hardpoint.SpawnMount(aircraft, weaponMount);
      if ((UnityEngine.Object) weaponMount != (UnityEngine.Object) null)
        hardpoint.ShowPylon(true);
    }
  }

  public void RemoveMounts()
  {
    foreach (Hardpoint hardpoint in this.hardpoints)
      hardpoint.ShowPylon(false);
    if ((UnityEngine.Object) this.weaponMount == (UnityEngine.Object) null)
      return;
    this.weaponMount = (WeaponMount) null;
    foreach (Hardpoint hardpoint in this.hardpoints)
      hardpoint.RemoveMount();
  }

  public bool BlockedByOtherHardpoint(Loadout loadout)
  {
    for (int index = 0; index < loadout.weapons.Count; ++index)
    {
      if (!((UnityEngine.Object) loadout.weapons[index] == (UnityEngine.Object) null) && this.precludingHardpointSets.Contains((byte) index))
        return true;
    }
    return false;
  }
}
