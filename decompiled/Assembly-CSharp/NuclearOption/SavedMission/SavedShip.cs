// Decompiled with JetBrains decompiler
// Type: NuclearOption.SavedMission.SavedShip
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace NuclearOption.SavedMission;

[Serializable]
public class SavedShip : SavedUnit
{
  public bool holdPosition;
  public float skill = 0.7f;
  public List<VehicleWaypoint> waypoints = new List<VehicleWaypoint>();
}
