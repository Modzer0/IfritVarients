// Decompiled with JetBrains decompiler
// Type: StandardLoadout
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;
using System;
using UnityEngine;

#nullable disable
[Serializable]
public class StandardLoadout
{
  public bool disabled;
  public string Name;
  public Loadout loadout;
  [Range(0.0f, 1f)]
  public float FuelRatio;

  public bool AllowedByHQ(WeaponManager weaponManager, FactionHQ hq)
  {
    return this.loadout.AllowedByHQ(weaponManager, hq);
  }
}
