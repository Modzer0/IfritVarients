// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.VehicleWaypoint
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace NuclearOption.SavedMission;

[Serializable]
public class VehicleWaypoint : ICloneable, IEquatable<VehicleWaypoint>
{
  public GlobalPosition position;
  public string objective;

  public object Clone()
  {
    return (object) new VehicleWaypoint()
    {
      position = this.position,
      objective = this.objective
    };
  }

  public bool Equals(VehicleWaypoint other)
  {
    return this.position == other.position && this.objective == other.objective;
  }
}
