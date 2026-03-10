// Decompiled with JetBrains decompiler
// Type: NuclearOption.Jobs.JobSharedFields
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: ED189D4A-56F4-4523-9409-1B9BC04B44A6
// Assembly location: D:\SteamLibrary\steamapps\common\Nuclear Option\NuclearOption_Data\Managed\Assembly-CSharp.dll

using System.Threading;
using UnityEngine;

#nullable disable
namespace NuclearOption.Jobs;

public struct JobSharedFields
{
  public BurstDatum datum;
  public float fixedDeltaTime;
  public float timeSinceLevelLoad;
  public int tickOffset;
  public long controlMath;
  public long controlAccess;
  public long aeroAccess;
  public long aeroMath;
  public long vehicleAccess;
  public long vehicle1;
  public long vehicle2;
  public long waterAccess;
  public long waterMath;
  public int DebugMarkersCount;
  public PtrArray<DebugVisJobMarker> DebugMarkersArray;

  public void LogAndReset(JobPerf jobPerf)
  {
    this.controlMath = 0L;
    this.controlAccess = 0L;
    this.aeroAccess = 0L;
    this.aeroMath = 0L;
    this.vehicleAccess = 0L;
    this.vehicle1 = 0L;
    this.vehicle2 = 0L;
    this.waterAccess = 0L;
    this.waterMath = 0L;
  }

  public bool NextDebugIndex(out Ptr<DebugVisJobMarker> marker)
  {
    int index = Interlocked.Increment(ref this.DebugMarkersCount) - 1;
    if (index < this.DebugMarkersArray.Length)
    {
      marker = this.DebugMarkersArray.GetPtr(index);
      return true;
    }
    marker = new Ptr<DebugVisJobMarker>();
    return false;
  }

  public void DrawDebugMarkers()
  {
    if (this.DebugMarkersArray.Length == 0)
    {
      this.DebugMarkersArray = new PtrArray<DebugVisJobMarker>(64 /*0x40*/);
    }
    else
    {
      int num = Mathf.Min(this.DebugMarkersCount, this.DebugMarkersArray.Length);
      this.DebugMarkersCount = 0;
      for (int index = 0; index < num; ++index)
      {
        DebugVisJobMarker debugVisJobMarker = this.DebugMarkersArray[index];
        switch (debugVisJobMarker.Type)
        {
          case DebugVisJobMarkerType.DebugPoint:
            GameObject gameObject1 = Object.Instantiate<GameObject>(GameAssets.i.debugPoint, Datum.origin);
            gameObject1.transform.position = debugVisJobMarker.Position;
            gameObject1.transform.localScale = Vector3.one * 3f;
            Object.Destroy((Object) gameObject1, 0.2f);
            break;
          case DebugVisJobMarkerType.DebugArrowGreen:
            GameObject gameObject2 = Object.Instantiate<GameObject>(GameAssets.i.debugArrowGreen, Datum.origin);
            gameObject2.transform.position = debugVisJobMarker.Position;
            gameObject2.transform.rotation = debugVisJobMarker.Rotation;
            gameObject2.transform.localScale = debugVisJobMarker.Scale;
            Object.Destroy((Object) gameObject2, 0.2f);
            break;
        }
      }
    }
  }
}
