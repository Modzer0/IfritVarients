// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.SavedAircraft
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace NuclearOption.SavedMission;

[Serializable]
public class SavedAircraft : SavedUnit
{
  public bool playerControlled;
  public int playerControlledPriority;
  [Obsolete("replaced by savedLoadout", true)]
  public LoadoutOld loadout;
  public SavedLoadout savedLoadout;
  public int livery;
  public LiveryKey.KeyType liveryType;
  public string liveryName;
  public float fuel = 1f;
  public float skill = 1f;
  public float bravery = 0.5f;
  public float startingSpeed;

  public LiveryKey liveryKey
  {
    get => new LiveryKey(this.liveryType, this.livery, this.liveryName);
    set => value.Save(out this.liveryType, out this.livery, out this.liveryName);
  }
}
