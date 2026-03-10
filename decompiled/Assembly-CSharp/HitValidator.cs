// Decompiled with JetBrains decompiler
// Type: HitValidator
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
public static class HitValidator
{
  private static Dictionary<PersistentID, HitValidator.FiringLog> firingLogs;

  public static void LogFiring(PersistentID shooter, Vector3 position, Vector3 velocity)
  {
    HitValidator.FiringLog firingLog;
    if (!HitValidator.firingLogs.TryGetValue(shooter, out firingLog))
    {
      firingLog = new HitValidator.FiringLog(0.0f);
      HitValidator.firingLogs.Add(shooter, firingLog);
    }
    firingLog.LogFiring(position, velocity);
  }

  public static bool HitValidated(Unit claimer, Vector3 hitPosition, Vector3 hitVelocity)
  {
    if ((Object) claimer == (Object) null)
    {
      Debug.LogWarning((object) "Unable to validate claimed hit - unit does not exist");
      return false;
    }
    HitValidator.FiringLog firingLog;
    if (!HitValidator.firingLogs.TryGetValue(claimer.persistentID, out firingLog))
    {
      Debug.LogWarning((object) $"Unable to validate hit claimed by {claimer.unitName} - firing log does not exist");
      return false;
    }
    if (firingLog.HitValidated(hitPosition, hitVelocity))
      return true;
    Debug.LogError((object) ("Unable to validate hit claimed by " + claimer.unitName));
    return false;
  }

  public static void Initialize()
  {
    if (HitValidator.firingLogs == null)
      HitValidator.firingLogs = new Dictionary<PersistentID, HitValidator.FiringLog>();
    else
      HitValidator.firingLogs.Clear();
  }

  private class FiringLog
  {
    private readonly List<HitValidator.FiringLog.Snapshot> snapshots = new List<HitValidator.FiringLog.Snapshot>();
    private float lastUpdate;

    public FiringLog(float lastUpdate) => this.lastUpdate = lastUpdate;

    public void LogFiring(Vector3 firePosition, Vector3 fireVelocity)
    {
      if (this.snapshots.Count > 0)
      {
        List<HitValidator.FiringLog.Snapshot> snapshots = this.snapshots;
        if ((double) snapshots[snapshots.Count - 1].GetAge() < 0.10000000149011612)
          return;
      }
      this.snapshots.Add(new HitValidator.FiringLog.Snapshot(firePosition, fireVelocity));
      if ((double) this.snapshots[0].GetAge() <= 5.0)
        return;
      this.snapshots.RemoveAt(0);
    }

    public bool HitValidated(Vector3 hitPosition, Vector3 hitVelocity)
    {
      for (int index = this.snapshots.Count - 1; index >= 0; --index)
      {
        HitValidator.FiringLog.Snapshot snapshot = this.snapshots[index];
        if ((double) snapshot.GetAge() > 5.0)
        {
          this.snapshots.RemoveAt(index);
        }
        else
        {
          snapshot = this.snapshots[index];
          if (snapshot.HitPlausible(hitPosition, hitVelocity))
            return true;
        }
      }
      return false;
    }

    private struct Snapshot(Vector3 position, Vector3 velocity)
    {
      private readonly Ray ray = new Ray(position, velocity);
      public readonly float timestamp = Time.timeSinceLevelLoad;

      public bool HitPlausible(Vector3 hitPosition, Vector3 hitVelocity)
      {
        float num = Vector3.Distance(hitPosition, this.ray.origin);
        return (double) Vector3.Angle(hitPosition - this.ray.origin, this.ray.direction) * (double) Mathf.Clamp01(num * 0.01f) < 10.0 && (double) num < 3000.0;
      }

      public float GetAge() => Time.timeSinceLevelLoad - this.timestamp;
    }
  }
}
