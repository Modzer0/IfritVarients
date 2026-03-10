// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.FactionSupply
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace NuclearOption.SavedMission;

[Serializable]
public class FactionSupply : ICloneable, IEquatable<FactionSupply>
{
  public string unitType;
  public int count;

  public FactionSupply()
  {
  }

  public FactionSupply(string unitType, int count)
  {
    this.unitType = unitType;
    this.count = count;
  }

  public bool Equals(FactionSupply other)
  {
    return this.unitType == other.unitType && this.count == other.count;
  }

  public object Clone() => (object) new FactionSupply(this.unitType, this.count);
}
