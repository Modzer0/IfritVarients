// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.StoredUnitCount
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace NuclearOption.SavedMission;

[Serializable]
public class StoredUnitCount : ICloneable, IEquatable<StoredUnitCount>
{
  public string UnitType;
  public int Count;

  public StoredUnitCount()
  {
    this.UnitType = string.Empty;
    this.Count = 1;
  }

  public StoredUnitCount(string type, int number)
  {
    this.UnitType = type;
    this.Count = number;
  }

  public object Clone() => (object) new StoredUnitCount(this.UnitType, this.Count);

  public bool Equals(StoredUnitCount other)
  {
    return this.UnitType == other.UnitType && this.Count == other.Count;
  }
}
