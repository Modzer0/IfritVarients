// Decompiled with JetBrains decompiler
// Type: WeaponMask
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
[Serializable]
public struct WeaponMask(int mask)
{
  public readonly int Mask = mask;

  public bool Get(int index) => (this.Mask & 1 << index) != 0;

  public static WeaponMask Set(WeaponMask oldMask, int index, bool value)
  {
    int mask = oldMask.Mask;
    return new WeaponMask(!value ? mask & ~(1 << index) : mask | 1 << index);
  }

  public static bool HasChangedToTrue(WeaponMask oldMask, WeaponMask newMask, int index)
  {
    return (oldMask.Get(index) ? 1 : 0) == 0 & newMask.Get(index);
  }
}
