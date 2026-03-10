// Decompiled with JetBrains decompiler
// Type: PersistentUnit
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using NuclearOption.Networking;
using UnityEngine;

#nullable disable
public class PersistentUnit
{
  public Unit unit;
  public PersistentID id;
  private FactionHQ _hq;
  public string unitName;
  public Player player;
  public UnitDefinition definition;

  public PersistentUnit(Unit unit, PersistentID id)
  {
    this.unit = unit;
    this.id = id;
    this._hq = unit.NetworkHQ;
    this.unitName = unit.unitName;
    this.definition = unit.definition;
    if (!(unit is GroundVehicle groundVehicle) || !((Object) groundVehicle.Networkowner != (Object) null))
      return;
    this.player = groundVehicle.Networkowner;
  }

  public void SetHQ(FactionHQ value) => this._hq = value;

  public FactionHQ GetHQ()
  {
    if ((Object) this.unit != (Object) null)
      this._hq = this.unit.NetworkHQ;
    return this._hq;
  }

  public bool HasHQ(out FactionHQ hq)
  {
    hq = this.GetHQ();
    return (Object) hq != (Object) null;
  }

  public Faction GetFaction()
  {
    FactionHQ hq = this.GetHQ();
    return !((Object) hq != (Object) null) ? (Faction) null : hq.faction;
  }
}
