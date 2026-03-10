// Decompiled with JetBrains decompiler
// Type: TrackingInfo
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
public class TrackingInfo
{
  private const float LAST_SPOTTED_EXTRA_TIME = 4f;
  public GlobalPosition lastKnownPosition;
  public float lastSpottedTime;
  public PersistentID id;
  public sbyte attackers;
  public sbyte missileAttacks;
  private Unit unit;

  public event Action OnSpotted;

  public TrackingInfo(Unit unit)
  {
    this.unit = !((UnityEngine.Object) unit == (UnityEngine.Object) null) ? unit : throw new ArgumentNullException(nameof (unit), "Null unit passed to TrackingInfo");
    this.lastKnownPosition = unit.GlobalPosition();
    this.lastSpottedTime = Time.timeSinceLevelLoad;
    this.id = unit.persistentID;
    this.attackers = (sbyte) 0;
    this.missileAttacks = (sbyte) 0;
  }

  public TrackingInfo(PersistentID id, GlobalPosition lastKnownPosition, float lastSpottedTime)
  {
    this.id = id;
    this.lastKnownPosition = lastKnownPosition;
    this.lastSpottedTime = lastSpottedTime;
  }

  [Obsolete("Use TryGetUnit instead")]
  public Unit GetUnit()
  {
    this.TryGetUnit(out Unit _);
    return this.unit;
  }

  public float GetStrategicPriority()
  {
    return (UnityEngine.Object) this.unit == (UnityEngine.Object) null || this.unit.disabled ? 0.0f : this.unit.definition.typeIdentity.strategic / (float) (1 + (int) this.missileAttacks + (int) this.attackers);
  }

  public bool TryGetUnit(out Unit unit)
  {
    if ((UnityEngine.Object) this.unit != (UnityEngine.Object) null || UnitRegistry.TryGetUnit(new PersistentID?(this.id), out this.unit))
    {
      unit = this.unit;
      return true;
    }
    unit = (Unit) null;
    return false;
  }

  public void UpdateInfo(GlobalPosition position)
  {
    this.lastKnownPosition = position;
    this.lastSpottedTime = Time.timeSinceLevelLoad;
    Action onSpotted = this.OnSpotted;
    if (onSpotted == null)
      return;
    onSpotted();
  }

  public bool Observed() => (double) Time.timeSinceLevelLoad - (double) this.lastSpottedTime < 4.0;

  public GlobalPosition GetPosition()
  {
    Unit unit;
    if ((double) Time.timeSinceLevelLoad - (double) this.lastSpottedTime < 4.0 && this.TryGetUnit(out unit))
      this.lastKnownPosition = unit.GlobalPosition();
    return this.lastKnownPosition;
  }
}
