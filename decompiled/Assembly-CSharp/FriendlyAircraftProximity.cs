// Decompiled with JetBrains decompiler
// Type: FriendlyAircraftProximity
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class FriendlyAircraftProximity
{
  private float moraleBoost;
  private float lastMoraleCheck;

  public float CheckMorale(Aircraft tracker)
  {
    if ((double) Time.timeSinceLevelLoad - (double) this.lastMoraleCheck < 5.0)
      return this.moraleBoost;
    this.lastMoraleCheck = Time.timeSinceLevelLoad;
    this.moraleBoost = 0.0f;
    foreach (Unit unitsInRange in BattlefieldGrid.GetUnitsInRangeEnumerable(tracker.transform.position.ToGlobalPosition(), 5000f))
    {
      if ((Object) unitsInRange.NetworkHQ == (Object) tracker.NetworkHQ && (Object) unitsInRange != (Object) tracker && (double) unitsInRange.radarAlt > 10.0 && unitsInRange is Aircraft aircraft)
        this.moraleBoost += 1000f / Mathf.Max(Vector3.Distance(aircraft.transform.position, tracker.transform.position), 1000f);
    }
    return this.moraleBoost;
  }
}
