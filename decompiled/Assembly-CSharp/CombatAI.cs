// Decompiled with JetBrains decompiler
// Type: CombatAI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public static class CombatAI
{
  private static List<CombatAI.TargetCandidate> targetCandidates = new List<CombatAI.TargetCandidate>();
  private static Dictionary<WeaponInfo, float> exclusionRadiusLookup = new Dictionary<WeaponInfo, float>();
  private static List<Unit> unitsInBlastRadius;
  private static List<CombatAI.TargetAttack> targetsAttacked;

  public static float GetExclusionRadius(WeaponInfo weaponInfo)
  {
    float exclusionRadius1;
    if (CombatAI.exclusionRadiusLookup.TryGetValue(weaponInfo, out exclusionRadius1))
      return exclusionRadius1;
    float exclusionRadius2 = Mathf.Pow(weaponInfo.weaponPrefab.GetComponent<Missile>().GetYield(), 0.3333f) * 13f;
    CombatAI.exclusionRadiusLookup.Add(weaponInfo, exclusionRadius2);
    return exclusionRadius2;
  }

  public static float GetSafeStandoffDist(GlobalPosition fromPosition, FactionHQ hq)
  {
    float a = 0.0f;
    foreach (KeyValuePair<PersistentID, TrackingInfo> keyValuePair in hq.trackingDatabase)
    {
      Unit unit;
      if (keyValuePair.Value.TryGetUnit(out unit) && (double) unit.radarAlt < 10.0 && !(unit is Aircraft))
      {
        float range = unit.GetMaxRange() * 1.2f;
        if ((double) range != 0.0 && FastMath.InRange(keyValuePair.Value.lastKnownPosition, fromPosition, range))
          a = Mathf.Max(a, range - FastMath.Distance(keyValuePair.Value.lastKnownPosition, fromPosition));
      }
    }
    return a;
  }

  public static float InterceptViability(
    Unit target,
    Unit analyzer,
    WeaponStation weaponStation,
    float maxRange,
    float targetDist,
    float targetMaxSpeed)
  {
    Vector3 vector3 = FastMath.NormalizedDirection(analyzer.GlobalPosition(), target.GlobalPosition());
    Vector3 rhs = target.rb.velocity + Mathf.Max(target.speed, targetMaxSpeed) * vector3;
    float num = Vector3.Dot(-vector3, rhs) / weaponStation.WeaponInfo.GetMaxSpeed();
    return Mathf.Min((float) ((double) maxRange * (1.0 + (double) num) / (double) targetDist - 1.0), 1f);
  }

  public static OpportunityThreat AnalyzeTarget(
    WeaponStation weaponStation,
    Unit analyzer,
    TrackingInfo trackingInfo,
    float armorTierOffset = 0.0f,
    float targetDistance = 1000f,
    bool mobile = false)
  {
    Unit unit;
    if (!trackingInfo.TryGetUnit(out unit))
      return new OpportunityThreat(0.0f, 0.0f);
    TargetRequirements targetRequirements = weaponStation.WeaponInfo.targetRequirements;
    OpportunityThreat opportunityThreat = weaponStation.CalcOpportunityThreat(unit.definition, analyzer);
    float num1 = opportunityThreat.opportunity;
    float threat1 = opportunityThreat.threat;
    if ((double) num1 == 0.0 || (double) trackingInfo.missileAttacks > (double) weaponStation.WeaponInfo.CalcAttacksNeeded(unit))
      return new OpportunityThreat(0.0f, threat1);
    float threat2 = threat1 / (float) (1 + 2 * Mathf.Max((int) trackingInfo.missileAttacks + (int) trackingInfo.attackers, 0));
    float num2 = targetRequirements.minAltitude * targetDistance / targetRequirements.maxRange;
    if (weaponStation.WeaponInfo.nuclear)
    {
      if (trackingInfo.attackers > (sbyte) 0)
        return new OpportunityThreat(0.0f, threat2);
      num1 *= unit.definition.typeIdentity.strategic;
    }
    if ((double) unit.radarAlt < (double) num2 || (double) unit.radarAlt > (double) targetRequirements.maxAltitude)
      return new OpportunityThreat(0.0f, threat2);
    if ((double) unit.speed > (double) targetRequirements.maxSpeed)
      return new OpportunityThreat(0.0f, threat2);
    if ((double) targetRequirements.minIR > 0.0 && !unit.HasIRSignature())
      return new OpportunityThreat(0.0f, threat2);
    if ((double) targetRequirements.minRadar > 0.0 && !unit.HasRadarEmission())
      return new OpportunityThreat(0.0f, threat2);
    if ((double) weaponStation.WeaponInfo.armorTierEffectiveness + (double) armorTierOffset < (double) unit.definition.armorTier)
      return new OpportunityThreat(0.0f, threat2);
    if ((double) unit.speed > 100.0 && (double) unit.radarAlt > 1.0)
    {
      float targetMaxSpeed = 0.0f;
      if (unit is Missile missile)
      {
        targetMaxSpeed = missile.GetWeaponInfo().GetMaxSpeed() * 0.67f;
        if (missile.targetID == analyzer.persistentID)
          threat2 *= 3f;
      }
      if ((double) targetMaxSpeed > 0.0)
        num1 *= CombatAI.InterceptViability(unit, analyzer, weaponStation, targetRequirements.maxRange, targetDistance, targetMaxSpeed);
    }
    else if (!mobile && (double) targetDistance > (double) targetRequirements.maxRange)
      num1 = 0.0f;
    if ((double) unit.definition.value < (double) targetRequirements.minValue)
      num1 *= 0.01f;
    return new OpportunityThreat(num1 * (targetRequirements.lineOfSight || (double) targetRequirements.maxRange <= 10000.0 ? 1f : 100f) * (float) (1.0 + (double) Mathf.Sqrt(unit.definition.value) * 0.0099999997764825821), threat2);
  }

  public static int LookForMissileTargets(
    Aircraft aircraft,
    Unit currentTarget,
    WeaponStation weaponStation)
  {
    int num1 = 0;
    int range = 1000;
    float minAlignment = weaponStation.WeaponInfo.targetRequirements.minAlignment;
    float maxRange = weaponStation.WeaponInfo.targetRequirements.maxRange;
    if (CombatAI.targetsAttacked == null)
      CombatAI.targetsAttacked = new List<CombatAI.TargetAttack>();
    CombatAI.targetsAttacked.Clear();
    GlobalPosition a = aircraft.GlobalPosition();
    int num2 = weaponStation.WeaponInfo.laserGuided ? aircraft.GetLaserDesignator().GetMaxTargets() : 16 /*0x10*/;
    GlobalPosition knownPosition;
    if (!aircraft.NetworkHQ.TryGetKnownPosition(currentTarget, out knownPosition))
      return 0;
    foreach (Unit unitsInRange in BattlefieldGrid.GetUnitsInRangeEnumerable(knownPosition, (float) range))
    {
      TrackingInfo trackingData = aircraft.NetworkHQ.GetTrackingData(unitsInRange.persistentID);
      if (!((UnityEngine.Object) unitsInRange.NetworkHQ == (UnityEngine.Object) null) && !((UnityEngine.Object) unitsInRange.NetworkHQ == (UnityEngine.Object) aircraft.NetworkHQ) && trackingData != null && (double) Vector3.Angle(unitsInRange.transform.position - aircraft.transform.position, aircraft.transform.forward) < (double) minAlignment && (double) CombatAI.AnalyzeTarget(weaponStation, (Unit) aircraft, trackingData).opportunity > 0.0 && FastMath.InRange(trackingData.lastKnownPosition, unitsInRange.GlobalPosition(), 20f) && FastMath.InRange(a, trackingData.lastKnownPosition, maxRange) && (!weaponStation.WeaponInfo.targetRequirements.lineOfSight || unitsInRange.LineOfSight(aircraft.transform.position, 1000f)))
      {
        int attacks = Mathf.Max(Mathf.FloorToInt(weaponStation.WeaponInfo.CalcAttacksNeeded(unitsInRange)), 1) - (int) trackingData.missileAttacks;
        if (attacks > 0)
        {
          CombatAI.targetsAttacked.Add(new CombatAI.TargetAttack(unitsInRange, attacks));
          ++num1;
          if (num1 >= num2)
            break;
        }
      }
    }
    CombatAI.DistributeTargets(aircraft, aircraft.weaponManager, CombatAI.targetsAttacked);
    return num1;
  }

  public static CombatAI.TargetSearchResults ChooseHQTarget(
    Unit searcher,
    float bravery,
    List<WeaponStation> stationList)
  {
    float num1 = 0.0f;
    float num2 = 0.0f;
    float num3 = 0.0f;
    bool outOfAmmo = stationList.Count == 0;
    Unit target = (Unit) null;
    WeaponStation chosenWeaponStation = (WeaponStation) null;
    if (CombatAI.targetCandidates == null)
      CombatAI.targetCandidates = new List<CombatAI.TargetCandidate>();
    else
      CombatAI.targetCandidates.Clear();
    foreach (KeyValuePair<PersistentID, TrackingInfo> keyValuePair in searcher.NetworkHQ.trackingDatabase)
    {
      TrackingInfo trackingInfo = keyValuePair.Value;
      Unit unit;
      if (trackingInfo.TryGetUnit(out unit) && (UnityEngine.Object) unit.NetworkHQ != (UnityEngine.Object) null && (UnityEngine.Object) unit.NetworkHQ != (UnityEngine.Object) searcher.NetworkHQ)
        CombatAI.targetCandidates.Add(new CombatAI.TargetCandidate(trackingInfo, FastMath.Distance(trackingInfo.GetPosition(), searcher.GlobalPosition())));
    }
    foreach (WeaponStation station in stationList)
    {
      if (station.Ammo <= 0 || station.WeaponInfo.energy && (double) searcher.GetPowerSupply().GetCharge() < 0.60000002384185791)
        outOfAmmo = true;
      else if (!station.Cargo)
      {
        TargetRequirements targetRequirements = station.WeaponInfo.targetRequirements;
        foreach (CombatAI.TargetCandidate targetCandidate in CombatAI.targetCandidates)
        {
          OpportunityThreat opportunityThreat = CombatAI.AnalyzeTarget(station, searcher, targetCandidate.trackingInfo, targetDistance: targetCandidate.range, mobile: true);
          num1 = Mathf.Max(num1, opportunityThreat.opportunity);
          float num4 = opportunityThreat.opportunity * (1f + opportunityThreat.threat) / targetCandidate.range;
          if ((double) targetCandidate.range > (double) station.WeaponInfo.targetRequirements.maxRange * 1.2000000476837158)
            num4 *= 0.5f;
          if ((double) num4 > (double) num3 && searcher.NetworkHQ.IsTargetPositionAccurate(targetCandidate.unit, 1000f))
          {
            target = targetCandidate.unit;
            num3 = num4;
            chosenWeaponStation = station;
            num2 = targetCandidate.range;
          }
        }
      }
    }
    if ((UnityEngine.Object) target != (UnityEngine.Object) null && (double) num1 * (double) bravery * 2.0 < 0.34999999403953552 && (double) searcher.NetworkHQ.GetAircraftThreat(target.persistentID) > (double) num1 * (double) bravery * 2.0 && (double) num2 > (double) chosenWeaponStation.WeaponInfo.targetRequirements.maxRange * 2.0)
    {
      target = (Unit) null;
      num1 = 0.0f;
    }
    return new CombatAI.TargetSearchResults(target, chosenWeaponStation, num1, outOfAmmo);
  }

  public static List<Unit> FilterTargetsWithinCone(
    List<Unit> unitList,
    Transform firingCone,
    float maxDegrees)
  {
    for (int index = unitList.Count - 1; index >= 0; --index)
    {
      if ((double) Vector3.Angle(unitList[index].transform.position - firingCone.position, firingCone.forward) > (double) maxDegrees)
        unitList.RemoveAt(index);
    }
    return unitList;
  }

  private static void DistributeTargets(
    Aircraft aircraft,
    WeaponManager weaponManager,
    List<CombatAI.TargetAttack> targetsAttacks)
  {
    targetsAttacks.Sort((Comparison<CombatAI.TargetAttack>) ((a, b) => aircraft.definition.ThreatPosedBy(b.unit.definition.roleIdentity).CompareTo(aircraft.definition.ThreatPosedBy(a.unit.definition.roleIdentity))));
    while (targetsAttacks.Count > 0)
    {
      for (int index = targetsAttacks.Count - 1; index >= 0; --index)
      {
        weaponManager.AddTargetList(targetsAttacks[index].unit);
        --targetsAttacks[index].attacks;
        if (targetsAttacks[index].attacks <= 0)
          targetsAttacks.RemoveAt(index);
      }
    }
  }

  public static int LookForBombingTargets(
    Aircraft aircraft,
    Unit currentTarget,
    WeaponStation weaponStation)
  {
    int num = 0;
    float range = aircraft.radarAlt * 0.5f;
    List<CombatAI.TargetAttack> targetsAttacks = new List<CombatAI.TargetAttack>();
    GlobalPosition knownPosition;
    if (!aircraft.NetworkHQ.TryGetKnownPosition(currentTarget, out knownPosition))
      return 0;
    foreach (Unit unitsInRange in BattlefieldGrid.GetUnitsInRangeEnumerable(knownPosition, range))
    {
      TrackingInfo trackingData = aircraft.NetworkHQ.GetTrackingData(unitsInRange.persistentID);
      if (!((UnityEngine.Object) unitsInRange.NetworkHQ == (UnityEngine.Object) null) && !((UnityEngine.Object) unitsInRange.NetworkHQ == (UnityEngine.Object) aircraft.NetworkHQ) && trackingData != null && !FastMath.OutOfRange(unitsInRange.transform.position, currentTarget.transform.position, range))
      {
        float targetDistance = FastMath.Distance(trackingData.lastKnownPosition, unitsInRange.GlobalPosition());
        if ((double) CombatAI.AnalyzeTarget(weaponStation, (Unit) aircraft, trackingData, targetDistance: targetDistance, mobile: true).opportunity > 0.0 && (double) targetDistance < 50.0)
        {
          int attacks = Mathf.Max(Mathf.FloorToInt(weaponStation.WeaponInfo.CalcAttacksNeeded(unitsInRange)), 1) - (int) trackingData.missileAttacks;
          if (attacks > 0)
          {
            targetsAttacks.Add(new CombatAI.TargetAttack(unitsInRange, attacks));
            ++num;
          }
        }
      }
    }
    CombatAI.DistributeTargets(aircraft, aircraft.weaponManager, targetsAttacks);
    return num;
  }

  private struct TargetCandidate
  {
    public readonly Unit unit;
    public readonly TrackingInfo trackingInfo;
    public readonly float range;

    public TargetCandidate(TrackingInfo trackingInfo, float range)
    {
      this.unit = trackingInfo.GetUnit();
      this.trackingInfo = trackingInfo;
      this.range = range;
    }
  }

  private class TargetAttack
  {
    public readonly Unit unit;
    public int attacks;

    public TargetAttack(Unit unit, int attacks)
    {
      this.unit = unit;
      this.attacks = attacks;
    }
  }

  public struct TargetSearchResults(
    Unit target,
    WeaponStation chosenWeaponStation,
    float opportunity,
    bool outOfAmmo)
  {
    public readonly Unit target = target;
    public readonly WeaponStation chosenWeaponStation = chosenWeaponStation;
    public readonly float opportunity = opportunity;
    public readonly bool outOfAmmo = outOfAmmo;
  }
}
