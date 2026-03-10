// Decompiled with JetBrains decompiler
// Type: ThreatTracker
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class ThreatTracker
{
  private FactionHQ hq;
  private TypeIdentity typeIdentity;
  private int checkIndex;
  private float lastCheck;
  private float checkInterval;
  private List<ThreatTracker.UnitThreatInfo> threatList;
  private Dictionary<PersistentID, ThreatTracker.UnitThreatInfo> threatLookup;

  public ThreatTracker(FactionHQ hq, float checkInterval, TypeIdentity typeIdentity)
  {
    this.hq = hq;
    this.threatList = new List<ThreatTracker.UnitThreatInfo>();
    this.threatLookup = new Dictionary<PersistentID, ThreatTracker.UnitThreatInfo>();
    this.typeIdentity = typeIdentity;
    this.checkInterval = checkInterval;
    hq.onDiscoverUnit += new Action<PersistentID>(this.ThreatTracker_OnDiscoverUnit);
    hq.onForgetUnit += new Action<PersistentID>(this.ThreatTracker_OnForgetUnit);
  }

  private void ThreatTracker_OnDiscoverUnit(PersistentID id)
  {
    ThreatTracker.UnitThreatInfo unitThreatInfo = new ThreatTracker.UnitThreatInfo(this.hq.trackingDatabase[id], this.typeIdentity);
    this.threatList.Add(unitThreatInfo);
    this.threatLookup.Add(id, unitThreatInfo);
  }

  private void ThreatTracker_OnForgetUnit(PersistentID id)
  {
    ThreatTracker.UnitThreatInfo unitThreatInfo;
    if (!this.threatLookup.Remove(id, ref unitThreatInfo))
      return;
    this.threatList.Remove(unitThreatInfo);
  }

  public void CheckThreats()
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastCheck < (double) this.checkInterval || this.threatList.Count == 0)
      return;
    if (this.checkIndex >= this.threatList.Count)
      this.checkIndex = 0;
    ThreatTracker.UnitThreatInfo threat1 = this.threatList[this.checkIndex];
    threat1.ResetCombinedThreat();
    TrackingInfo trackingInfo = threat1.trackingInfo;
    foreach (ThreatTracker.UnitThreatInfo threat2 in this.threatList)
      threat1.AddCombinedThreat(trackingInfo, threat2.GetIndividualThreat());
    ++this.checkIndex;
  }

  public float GetThreat(PersistentID id)
  {
    ThreatTracker.UnitThreatInfo unitThreatInfo;
    if (this.threatLookup.TryGetValue(id, out unitThreatInfo))
      return unitThreatInfo.GetCombinedThreat();
    Debug.LogError((object) $"unit {id} was not found in {this.hq}'s threat lookup");
    return 0.0f;
  }

  private class UnitThreatInfo
  {
    public Unit unit;
    public TrackingInfo trackingInfo;
    private float individualThreat;
    private float combinedThreat;

    public UnitThreatInfo(TrackingInfo trackingInfo, TypeIdentity typeIdentity)
    {
      this.trackingInfo = trackingInfo;
      this.unit = trackingInfo.GetUnit();
      this.individualThreat = typeIdentity.ThreatPosedBy(this.unit.definition.roleIdentity);
    }

    public void ResetCombinedThreat() => this.combinedThreat = 0.0f;

    public void AddCombinedThreat(TrackingInfo otherUnit, float otherThreat)
    {
      if ((double) otherThreat < 0.40000000596046448 || otherUnit == this.trackingInfo)
        return;
      float num = Mathf.Max(1f, 1f / 1000f * FastMath.Distance(otherUnit.lastKnownPosition, this.trackingInfo.lastKnownPosition));
      this.combinedThreat += otherThreat / num;
    }

    public float GetIndividualThreat() => this.individualThreat;

    public float GetCombinedThreat() => this.individualThreat + this.combinedThreat;
  }
}
