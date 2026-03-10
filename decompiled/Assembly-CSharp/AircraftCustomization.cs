// Decompiled with JetBrains decompiler
// Type: AircraftCustomization
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.SavedMission;

#nullable disable
public class AircraftCustomization
{
  public Loadout loadout;
  public float fuelLevel;
  public int livery;

  public AircraftCustomization(Loadout loadout, float fuelLevel, int livery)
  {
    this.loadout = loadout;
    this.fuelLevel = fuelLevel;
    this.livery = livery;
  }

  public void Update(Loadout loadout, float fuelLevel, int livery)
  {
    this.loadout = loadout;
    this.fuelLevel = fuelLevel;
    this.livery = livery;
  }
}
