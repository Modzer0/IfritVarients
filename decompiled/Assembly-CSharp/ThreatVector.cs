// Decompiled with JetBrains decompiler
// Type: ThreatVector
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class ThreatVector
{
  private FactionHQ hq;
  private float checkInterval;
  private int index;
  private int totalThreats;
  private Unit owner;
  public Vector3 threatVector;
  private Vector3 constructThreatVector;

  public ThreatVector(Unit owner)
  {
    this.checkInterval = 5f;
    this.owner = owner;
    this.hq = owner.NetworkHQ;
  }

  private void IterateThreats(Unit currentTarget)
  {
    ++this.index;
    if (this.index >= UnitRegistry.allUnits.Count)
    {
      this.index = 0;
      this.threatVector = this.constructThreatVector;
      this.threatVector.y = 0.0f;
      this.constructThreatVector = Vector3.zero;
      Airbase nearestAirbase;
      if (!this.hq.TryGetNearestAirbase(this.owner.transform.position, out nearestAirbase) || (double) Vector3.Dot(-this.threatVector, this.owner.transform.position - nearestAirbase.center.position) < 0.0)
      {
        this.threatVector = Vector3.zero;
        this.totalThreats = 0;
      }
      else
        this.totalThreats = 0;
    }
    else
    {
      Unit allUnit = UnitRegistry.allUnits[this.index];
      if ((Object) allUnit == (Object) null || allUnit.disabled || (Object) allUnit == (Object) currentTarget || allUnit is Missile || !this.hq.IsTargetPositionAccurate(allUnit, 1000f))
        return;
      float num = this.owner.definition.ThreatPosedBy(allUnit.definition.roleIdentity);
      Vector3 vector3 = allUnit.GlobalPosition() - this.owner.GlobalPosition();
      float magnitude = vector3.magnitude;
      if ((double) magnitude < 3000.0)
        return;
      ++this.totalThreats;
      this.constructThreatVector += vector3.normalized * num * 2000f / magnitude;
    }
  }

  public Vector3 CheckThreats(Unit currentTarget)
  {
    this.IterateThreats(currentTarget);
    return this.threatVector;
  }
}
