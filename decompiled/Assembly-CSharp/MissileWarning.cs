// Decompiled with JetBrains decompiler
// Type: MissileWarning
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class MissileWarning : MonoBehaviour
{
  private List<Missile> unknownMissiles = new List<Missile>();
  public List<Missile> knownMissiles = new List<Missile>();
  [SerializeField]
  private float detectionRange = 5000f;
  private float lastDetectionCheck;
  private Aircraft aircraft;

  public event Action<MissileWarning.OnMissileWarning> onMissileWarning;

  public event Action<MissileWarning.OffMissileWarning> offMissileWarning;

  public void LockedByMissile(Aircraft aircraft, Missile missile)
  {
    this.enabled = true;
    this.aircraft = aircraft;
    this.unknownMissiles.Add(missile);
  }

  public bool IsWarning() => this.knownMissiles.Count > 0;

  public bool TryGetNearestIncoming(out Missile nearestIncoming)
  {
    nearestIncoming = (Missile) null;
    if (this.knownMissiles.Count == 0)
      return false;
    float num1 = float.MaxValue;
    foreach (Missile knownMissile in this.knownMissiles)
    {
      float num2 = FastMath.SquareDistance(knownMissile.GlobalPosition(), this.aircraft.GlobalPosition());
      if ((double) num2 < (double) num1)
      {
        nearestIncoming = knownMissile;
        num1 = num2;
      }
    }
    return (UnityEngine.Object) nearestIncoming != (UnityEngine.Object) null;
  }

  private void Update()
  {
    if (this.unknownMissiles.Count == 0 && this.knownMissiles.Count == 0 || this.aircraft.disabled)
      this.enabled = false;
    if ((double) Time.timeSinceLevelLoad - (double) this.lastDetectionCheck < 0.20000000298023224)
      return;
    this.lastDetectionCheck = Time.timeSinceLevelLoad;
    for (int index = this.unknownMissiles.Count - 1; index >= 0; --index)
    {
      Missile unit;
      if (UnitRegistry.TryGetUnit<Missile>(new PersistentID?(this.unknownMissiles[index].persistentID), out unit))
      {
        if (unit.disabled || unit.targetID != this.aircraft.persistentID)
          this.unknownMissiles.RemoveAt(index);
        else if (FastMath.InRange(unit.transform.position, this.transform.position, this.detectionRange) && unit.LineOfSight(this.aircraft.transform.position, 1000f))
        {
          this.knownMissiles.Add(unit);
          this.aircraft.NetworkHQ.CmdUpdateTrackingInfo(unit.persistentID);
          Action<MissileWarning.OnMissileWarning> onMissileWarning = this.onMissileWarning;
          if (onMissileWarning != null)
            onMissileWarning(new MissileWarning.OnMissileWarning()
            {
              missile = unit
            });
          this.unknownMissiles.RemoveAt(index);
        }
        else if (unit.seekerMode < Missile.SeekerMode.passive && (this.aircraft.NetworkHQ.GetTrackingData(unit.persistentID) != null || !((UnityEngine.Object) unit.NetworkHQ != (UnityEngine.Object) this.aircraft.NetworkHQ)))
        {
          this.knownMissiles.Add(unit);
          Action<MissileWarning.OnMissileWarning> onMissileWarning = this.onMissileWarning;
          if (onMissileWarning != null)
            onMissileWarning(new MissileWarning.OnMissileWarning()
            {
              missile = unit
            });
          this.unknownMissiles.RemoveAt(index);
        }
      }
    }
    for (int index = this.knownMissiles.Count - 1; index >= 0; --index)
    {
      Missile knownMissile = this.knownMissiles[index];
      if ((UnityEngine.Object) knownMissile == (UnityEngine.Object) null || knownMissile.disabled || knownMissile.targetID != this.aircraft.persistentID)
      {
        this.knownMissiles.RemoveAt(index);
        if ((UnityEngine.Object) knownMissile != (UnityEngine.Object) null)
        {
          Action<MissileWarning.OffMissileWarning> offMissileWarning = this.offMissileWarning;
          if (offMissileWarning != null)
            offMissileWarning(new MissileWarning.OffMissileWarning()
            {
              missile = knownMissile
            });
        }
      }
    }
  }

  public struct OnMissileWarning
  {
    public Missile missile;
  }

  public struct OffMissileWarning
  {
    public Missile missile;
  }
}
